using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Servant.Common.Entities;
using Servant.End2EndTests.Core;
using Servant.End2EndTests.Helpers;
using Servant.End2EndTests.Helpers.FileSystem;
using Xunit;

namespace Servant.End2EndTests.ApiTests
{
    public class FileSystemTests
    {
        private readonly RestApiTestClient _restApiClient;

        public FileSystemTests()
        {
            _restApiClient = new RestApiTestClient(new Uri("http://localhost:8025"));
        }

        [Fact]
        public async void When_SendingGetWithoutPath_Should_Return400AndValidationMessage()
        {
            var result = await _restApiClient.Get("/api/fs/get");
            var jcontent = JParser.ParseContent(result.Content);

            Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
            Assert.NotNull(jcontent["ModelState"]["Path"]);
        }

        [Fact]
        public async void When_SendingGetWithPathContainingInvalidCharacters_Should_Return400AndValidationMessage()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(new FileItem("1.txt"));
                var filepath = $"{fs.TempPath}/<1>.txt";
                var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
                Assert.NotNull(jcontent["ModelState"]["Path"]);
            }
        }

        [Fact]
        public async void When_SendingGetWithNonRootedPath_Should_Return400AndValidationMessage()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var filepath = "dir/1.txt";
                var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
                Assert.NotNull(jcontent["ModelState"]["Path"]);
            }
        }

        [Fact]
        public async void When_GettingNonExistentFile_Should_Return404()
        {
            using (var fs = new FsInitializer())
            {
                var result = await _restApiClient.Get($"/api/fs/get?path={fs.TempPath}/non_existent_file.txt");
                Assert.Equal(HttpStatusCode.NotFound, result.Response.StatusCode);
            }
        }

        [Fact]
        public async void When_GettingExistingRootedFile_Should_ReturnFileContent()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt", "TEST CONTENT")));
                var filepath = Path.Combine(fs.TempPath, "dir/1.txt");
                var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                Assert.IsType(typeof (StreamContent), result.Response.Content);
                Assert.Equal("TEST CONTENT", result.Content);
            }
        }

        [Fact]
        public async void When_GettingFileWithoutDownloadParameter_Should_SetMediaTypeByFileExtension()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt"),
                        new FileItem("1.xml")));
                var filepath1 = Path.Combine(fs.TempPath, "dir/1.txt");
                var filepath2 = Path.Combine(fs.TempPath, "dir/1.xml");

                var result1 = await _restApiClient.Get($"/api/fs/get?path={filepath1}");
                var result2 = await _restApiClient.Get($"/api/fs/get?path={filepath2}");

                Assert.Equal("text/plain", result1.Response.Content.Headers.ContentType.MediaType);
                Assert.Equal("text/xml", result2.Response.Content.Headers.ContentType.MediaType);
            }
        }

        [Fact]
        public async void When_GettingFileWithoutDownloadParameter_Should_SetContentDispositionInline()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var filepath = Path.Combine(fs.TempPath, "dir/1.txt");
                var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");
                Assert.Equal("inline", result.Response.Content.Headers.ContentDisposition.DispositionType);
            }
        }

        [Fact]
        public async void When_GettingFileWithDownloadParameter_Should_SetBinaryMediaType()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt"),
                        new FileItem("1.xml")));
                var filepath1 = Path.Combine(fs.TempPath, "dir/1.txt");
                var filepath2 = Path.Combine(fs.TempPath, "dir/1.xml");

                var result1 = await _restApiClient.Get($"/api/fs/get?path={filepath1}&download=1");
                var result2 = await _restApiClient.Get($"/api/fs/get?path={filepath2}&download=1");

                Assert.Equal("application/octet-stream", result1.Response.Content.Headers.ContentType.MediaType);
                Assert.Equal("application/octet-stream", result2.Response.Content.Headers.ContentType.MediaType);
            }
        }

        [Fact]
        public async void When_GettingFileWithDownloadParameter_Should_SetContentDispositionAttachment()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var filepath = Path.Combine(fs.TempPath, "dir/1.txt");
                var result = await _restApiClient.Get($"/api/fs/get?path={filepath}&download=1");
                Assert.Equal("attachment", result.Response.Content.Headers.ContentDisposition.DispositionType);
            }
        }

        [Fact]
        public async void When_GettingDrives_Should_ReturnListOfDriveInfo()
        {
            var result = await _restApiClient.Get("/api/fs/drives");
            var drives = JsonConvert.DeserializeObject<List<DriveInfoModel>>(result.Content);

            Assert.NotNull(drives);
            Assert.True(drives.Any(x => x.Name.Equals("C:\\", StringComparison.OrdinalIgnoreCase)));
            Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
        }

        [Fact]
        public async void When_PostingCopyActionWithoutAction_Should_Return400AndValidationMessage()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var values = new KeyValueList<string, string>
                {
                    {"sourcePath", Path.Combine(fs.TempPath, "dir/1.txt")},
                    {"destPath", Path.Combine(fs.TempPath, "dir/2.txt")}
                };
                var result = await _restApiClient.Post("/api/fs", values);
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
                Assert.Equal("A value for action is not provided.", jcontent["Message"]);
            }
        }

        [Fact]
        public async void When_PostingCopyActionWithUnknownAction_Should_Return400AndValidationMessage()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var values = new KeyValueList<string, string>
                {
                    {"action", "unknown"},
                    {"sourcePath", Path.Combine(fs.TempPath, "dir/1.txt")},
                    {"destPath", Path.Combine(fs.TempPath, "dir/2.txt")}
                };
                var result = await _restApiClient.Post("/api/fs", values);
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
                Assert.Equal("Unknown action command - 'unknown'.", jcontent["Message"]);
            }
        }

        [Fact]
        public async void When_PostingCopyActionWithoutSourcePath_Should_Return400AndValidationMessage()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"destPath", Path.Combine(fs.TempPath, "dir/2.txt")}
                };
                var result = await _restApiClient.Post("/api/fs", values);
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
                Assert.NotNull(jcontent["ModelState"]["SourcePath"]);
            }
        }

        [Fact]
        public async void When_PostingCopyActionWithoutDestPath_Should_Return400AndValidationMessage()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", Path.Combine(fs.TempPath, "dir/1.txt")}
                };
                var result = await _restApiClient.Post("/api/fs", values);
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.BadRequest, result.Response.StatusCode);
                Assert.NotNull(jcontent["ModelState"]["DestPath"]);
            }
        }

        [Fact]
        public async void When_PostingCopyWithFilePath_Should_CopyFile()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt")));
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", Path.Combine(fs.TempPath, "dir/1.txt")},
                    {"destPath", Path.Combine(fs.TempPath, "dir/2.txt")}
                };

                Assert.False(File.Exists(Path.Combine(fs.TempPath, "dir/2.txt")));

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir/2.txt")));
            }
        }

        [Fact]
        public async void When_PostingCopyActionWithEmptyDirPath_Should_CopyDirectory()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(new DirItem("dir"));
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", Path.Combine(fs.TempPath, "dir")},
                    {"destPath", Path.Combine(fs.TempPath, "dir2")}
                };

                Assert.False(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));
            }
        }

        [Fact]
        public async void When_PostingCopyActionWithDirPathContainingFile_Should_CopyDirectory()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new DirItem("subdir",
                            new FileItem("2.txt")),
                        new FileItem("1.txt")));
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", Path.Combine(fs.TempPath, "dir")},
                    {"destPath", Path.Combine(fs.TempPath, "dir2")}
                };

                Assert.False(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\subdir\\2.txt")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\1.txt")));
            }
        }
    }
}
