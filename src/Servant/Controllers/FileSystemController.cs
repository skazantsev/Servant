using Servant.Common.Entities;
using Servant.Exceptions;
using Servant.Extensions;
using Servant.RequestParams;
using Servant.Validation;
using System.Collections.Generic;
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
        [HttpGet]
        public IHttpActionResult Get([FromUri]GetFileRequestParams query)
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
        public IEnumerable<FileSystemInfoModel> List([FromUri]ListDirectoryRequestParams query)
        {
            return new DirectoryInfo(query.Path)
                .GetFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .Select(FileSystemInfoModel.FromFSInfo);
        }
    }
}
