using Newtonsoft.Json.Linq;
using Servant.End2EndTests.Core;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace Servant.End2EndTests.ApiTests
{
    public class FileSystemTests : IDisposable
    {
        private readonly RestApiTestClient _restApiClient;

        private string _tempPath;

        public FileSystemTests()
        {
            _restApiClient = new RestApiTestClient(new Uri("http://localhost:8025"));
            InitFileSystem();
        }

        [Fact]
        public async void When_SendingGetWithoutPath_Should_Return400AndValidationMessage()
        {
            var result = await _restApiClient.Get("/api/fs/get");
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(400, (int)result.Response.StatusCode);
            Assert.NotNull(jcontent["ModelState"]["Path"]);
        }

        [Fact]
        public async void When_SendingGetWithPathContainingInvalidCharacters_Should_Return400AndValidationMessage()
        {
            var filepath = $"{_tempPath}/subdir/<1>.txt";
            var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(400, (int)result.Response.StatusCode);
            Assert.NotNull(jcontent["ModelState"]["Path"]);
        }

        [Fact]
        public async void When_SendingGetWithNonRootedPath_Should_Return400AndValidationMessage()
        {
            var filepath = "dir/1.txt";
            var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");
            var jcontent = JObject.Parse(result.Content);

            Assert.Equal(400, (int)result.Response.StatusCode);
            Assert.NotNull(jcontent["ModelState"]["Path"]);
        }

        [Fact]
        public async void When_GettingNonExistentFile_Should_Return404()
        {
            var filepath = Path.Combine(_tempPath, "subdir/non_existent_file.txt");
            var result = await _restApiClient.Get($"/api/fs/get?path={_tempPath}/subdir/{filepath}");

            Assert.Equal(404, (int)result.Response.StatusCode);
        }

        [Fact]
        public async void When_GettingExistingRootedFile_Should_ReturnFileContent()
        {
            var filepath = Path.Combine(_tempPath, "subdir/1.txt");
            var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");

            Assert.Equal(200, (int)result.Response.StatusCode);
            Assert.IsType(typeof(StreamContent), result.Response.Content);
            Assert.Equal("TEST CONTENT", result.Content);
        }

        [Fact]
        public async void When_GettingFileWithoutDownloadParameter_Should_SetMediaTypeByFileExtension()
        {
            var filepath1 = Path.Combine(_tempPath, "subdir/1.txt");
            var filepath2 = Path.Combine(_tempPath, "subdir/1.xml");

            var result1 = await _restApiClient.Get($"/api/fs/get?path={filepath1}");
            var result2 = await _restApiClient.Get($"/api/fs/get?path={filepath2}");

            Assert.Equal("text/plain", result1.Response.Content.Headers.ContentType.MediaType);
            Assert.Equal("text/xml", result2.Response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async void When_GettingFileWithoutDownloadParameter_Should_SetContentDispositionInline()
        {
            var filepath = Path.Combine(_tempPath, "subdir/1.txt");

            var result = await _restApiClient.Get($"/api/fs/get?path={filepath}");

            Assert.Equal("inline", result.Response.Content.Headers.ContentDisposition.DispositionType);
        }

        [Fact]
        public async void When_GettingFileWithDownloadParameter_Should_SetBinaryMediaType()
        {
            var filepath1 = Path.Combine(_tempPath, "subdir/1.txt");
            var filepath2 = Path.Combine(_tempPath, "subdir/1.xml");

            var result1 = await _restApiClient.Get($"/api/fs/get?path={filepath1}&download=1");
            var result2 = await _restApiClient.Get($"/api/fs/get?path={filepath2}&download=1");

            Assert.Equal("application/octet-stream", result1.Response.Content.Headers.ContentType.MediaType);
            Assert.Equal("application/octet-stream", result2.Response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async void When_GettingFileWithDownloadParameter_Should_SetContentDispositionAttachment()
        {
            var filepath = Path.Combine(_tempPath, "subdir/1.txt");

            var result = await _restApiClient.Get($"/api/fs/get?path={filepath}&download=1");

            Assert.Equal("attachment", result.Response.Content.Headers.ContentDisposition.DispositionType);
        }

        public void Dispose()
        {
            for (var i = 0; i < 5; ++i)
            {
                try
                {
                    Directory.Delete(_tempPath, true);
                    break;
                }
                catch(IOException)
                {
                    Thread.Sleep(100);
                }
            }
        } 

        private void InitFileSystem()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var rootDir = Directory.CreateDirectory(_tempPath);

            var subdir = rootDir.CreateSubdirectory("subdir");
            CreateFile(subdir, "1.txt", "TEST CONTENT");
            CreateFile(subdir, "1.xml", "<test></test>");
        }

        private void CreateFile(DirectoryInfo dirPath, string fileName, string content = null)
        {
            var path = Path.Combine(dirPath.FullName, fileName);
            using (var writer = File.CreateText(path))
            {
                writer.Write(content);
            }
        }
    }
}
