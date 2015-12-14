using Newtonsoft.Json.Linq;
using Servant.Common.Entities;
using Servant.Exceptions;
using Servant.Extensions;
using Servant.Helpers;
using Servant.RequestParams;
using Servant.Services.FS;
using Servant.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Servant.Controllers
{
    [ApiExceptionFilter]
    [ValidationActionFilter]
    public class FileSystemController : ApiController
    {
        private readonly FileSystemManager _fileSystemManager;

        public FileSystemController()
        {
            _fileSystemManager = new FileSystemManager();
        }

        [HttpGet]
        public IHttpActionResult Get([FromUri]GetFileRequest query)
        {
            const string downloadKey = "download";

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(File.OpenRead(query.Path))
            };
            if (Request.QueryString().ContainsKey(downloadKey))
            {
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Path.GetFileName(query.Path)
                };
            }
            else
            {
                var mediaType = MimeMapping.GetMimeMapping(Path.GetFileName(query.Path) ?? "");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = Path.GetFileName(query.Path)
                };
            }
            return ResponseMessage(response);
        }

        [HttpGet]
        public IEnumerable<object> Drives()
        {
            return DriveInfo.GetDrives().Select(DriveInfoModel.FromDriveInfo);
        }

        [HttpGet]
        public IEnumerable<FileSystemInfoModel> List([FromUri]ListDirectoryRequest query)
        {
            return new DirectoryInfo(query.Path)
                .GetFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .Select(FileSystemInfoModel.FromFSInfo);
        }

        [HttpPost]
        public IHttpActionResult PostAction([FromBody][Required]JObject query)
        {
            var binder = new JModelBinder(query);
            var action = binder.GetValue<string>("action");
            switch (action.ToUpper())
            {
                case "COPY":
                    var model = binder.GetModel<CopyFileRequest>();
                    return CopyAction(model);
                default:
                    return BadRequest($"Unknown action command - '{action}'.");
            }
        }

        private IHttpActionResult CopyAction(CopyFileRequest query)
        {
            Validate(query);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _fileSystemManager.Copy(query.SourcePath, query.DestPath, query.Overwrite);
            return Ok();
        }
    }
}
