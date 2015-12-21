using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Servant.Common.Entities;
using Servant.Exceptions;
using Servant.Extensions;
using Servant.Helpers;
using Servant.RequestParams;
using Servant.Services.FS;
using Servant.Validation;

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

            if (!File.Exists(query.Path))
                return NotFound();
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
        public IEnumerable<DriveInfoModel> Drives()
        {
            return DriveInfo.GetDrives().Select(DriveInfoModel.FromDriveInfo);
        }

        [HttpGet]
        public IHttpActionResult List([FromUri]ListDirectoryRequest query)
        {
            if (!Directory.Exists(query.Path))
                return NotFound();

            var list = new DirectoryInfo(query.Path)
                .GetFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .Select(FileSystemInfoModel.FromFSInfo);
            return Ok(list);
        }

        [HttpPost]
        public IHttpActionResult PostAction([FromBody][Required]JObject command)
        {
            var binder = new JModelBinder(command);
            var action = binder.GetValue<string>("action");
            switch (action.ToUpper())
            {
                case "COPY":
                    var copyReq = binder.GetModel<CopyFSPathRequest>();
                    return InvokeAction(copyReq, Copy);
                case "MOVE":
                    var moveReq = binder.GetModel<MoveFSPathRequest>();
                    return InvokeAction(moveReq, Move);
                case "DELETE":
                    var deleteReq = binder.GetModel<DeleteFsPathRequest>();
                    return InvokeAction(deleteReq, Delete);
                default:
                    return BadRequest($"Unknown action command - '{action}'.");
            }
        }

        private IHttpActionResult Copy(CopyFSPathRequest command)
        {
            if (!File.Exists(command.SourcePath))
                return NotFound();
            _fileSystemManager.Copy(command.SourcePath, command.DestPath, command.Overwrite);
            return Ok();
        }

        private IHttpActionResult Move(MoveFSPathRequest command)
        {
            if (!File.Exists(command.SourcePath))
                return NotFound();
            _fileSystemManager.Move(command.SourcePath, command.DestPath, command.Overwrite);
            return Ok();
        }

        private IHttpActionResult Delete(DeleteFsPathRequest command)
        {
            _fileSystemManager.Delete(command.FileSystemPath);
            return Ok();
        }

        private IHttpActionResult InvokeAction<T>(T command, Func<T, IHttpActionResult> action)
        {
            Validate(command);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return action(command);
        }
    }
}
