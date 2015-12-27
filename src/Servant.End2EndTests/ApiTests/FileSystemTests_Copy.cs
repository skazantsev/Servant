using System.IO;
using System.Net;
using Servant.End2EndTests.Core;
using Servant.End2EndTests.Helpers;
using Servant.End2EndTests.Helpers.FileSystem;
using Xunit;

namespace Servant.End2EndTests.ApiTests
{
    public partial class FileSystemTests
    {
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
        public async void When_CopyingExistingFile_Should_CopyIt()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt", "Hello Test")));

                var sourcePath = Path.Combine(fs.TempPath, "dir/1.txt");
                var destPath = Path.Combine(fs.TempPath, "dir/2.txt");
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", sourcePath},
                    {"destPath", destPath}
                };

                Assert.False(File.Exists(destPath));

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                Assert.True(File.Exists(sourcePath));
                Assert.True(File.Exists(destPath));
                Assert.Equal(File.ReadAllText(destPath), "Hello Test");
            }
        }

        [Fact]
        public async void When_CopyingNonExistingFile_Should_Return404()
        {
            using (var fs = new FsInitializer())
            {
                var sourcePath = Path.Combine(fs.TempPath, "non_existing.txt");
                var destPath = Path.Combine(fs.TempPath, "dir/2.txt");
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", sourcePath},
                    {"destPath", destPath}
                };

                Assert.False(File.Exists(destPath));

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.NotFound, result.Response.StatusCode);
                Assert.False(File.Exists(sourcePath));
                Assert.False(File.Exists(destPath));
            }
        }

        [Fact]
        public async void When_CopyingFileToAlreadyExistingPathWithoutOverwrite_Should_Return500AndExceptionMessage()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt"),
                        new FileItem("2.txt")));

                var sourcePath = Path.Combine(fs.TempPath, "dir/1.txt");
                var destPath = Path.Combine(fs.TempPath, "dir/2.txt");
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", sourcePath},
                    {"destPath", destPath}
                };

                var result = await _restApiClient.Post("/api/fs", values);
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.InternalServerError, result.Response.StatusCode);
                Assert.True(File.Exists(destPath));
                Assert.Equal("System.IO.IOException", jcontent["ExceptionType"]);
                Assert.NotNull(jcontent["ExceptionMessage"]);
            }
        }

        [Fact]
        public async void When_CopyingFileToAlreadyExistingPathAndOverwriteSetToTrue_Should_CopyIt()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new FileItem("1.txt", "source_content"),
                        new FileItem("2.txt", "dest_content")));

                var sourcePath = Path.Combine(fs.TempPath, "dir/1.txt");
                var destPath = Path.Combine(fs.TempPath, "dir/2.txt");
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", sourcePath},
                    {"destPath", destPath},
                    {"overwrite", "true"}
                };

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                Assert.True(File.Exists(destPath));
                Assert.Equal("source_content", File.ReadAllText(destPath));
            }
        }

        [Fact]
        public async void When_CopyingEmptyDirectory_Should_CopyIt()
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
        public async void When_CopyingDirectoryContainingFilesAndDirs_Should_CopyEntireDirectory()
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

                // verify source
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir")));
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir\\subdir")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir\\subdir\\2.txt")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir\\1.txt")));

                // verify destination
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2\\subdir")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\subdir\\2.txt")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\1.txt")));
            }
        }

        [Fact]
        public async void When_CopyingAlreadyExistingDirectoryWithoutOverwrite_Should_Return500AndNotChangeDestination()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir1",
                        new DirItem("subdir"),
                        new FileItem("1.txt", "source_content")),
                    new DirItem("dir2",
                        new DirItem("subdir"),
                        new FileItem("1.txt", "dest_content")));

                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", Path.Combine(fs.TempPath, "dir1")},
                    {"destPath", Path.Combine(fs.TempPath, "dir2")}
                };

                var result = await _restApiClient.Post("/api/fs", values);
                var jcontent = JParser.ParseContent(result.Content);

                Assert.Equal(HttpStatusCode.InternalServerError, result.Response.StatusCode);
                Assert.Equal("System.IO.IOException", jcontent["ExceptionType"]);
                Assert.NotNull(jcontent["ExceptionMessage"]);
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2\\subdir")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\1.txt")));
                Assert.Equal("dest_content", File.ReadAllText(Path.Combine(fs.TempPath, "dir2\\1.txt")));
            }
        }

        [Fact]
        public async void When_CopyingAlreadyExistingDirectoryWithOverwriteSetToTrue_Should_CopyEntireDirectory()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir1",
                        new DirItem("subdir"),
                        new FileItem("1.txt", "source_content")),
                    new DirItem("dir2",
                        new DirItem("subdir"),
                        new DirItem("subdir2"),
                        new FileItem("1.txt", "dest_content")));

                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", Path.Combine(fs.TempPath, "dir1")},
                    {"destPath", Path.Combine(fs.TempPath, "dir2")},
                    {"overwrite", "true"}
                };

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2\\subdir")));
                Assert.False(Directory.Exists(Path.Combine(fs.TempPath, "dir2\\subdir2")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\1.txt")));
                Assert.Equal("source_content", File.ReadAllText(Path.Combine(fs.TempPath, "dir2\\1.txt")));
            }
        }

        [Fact]
        public async void When_CopyingDirectoryByUNCPath_Should_CopyEntireDirectory()
        {
            using (var fs = new FsInitializer())
            {
                fs.CreateItems(
                    new DirItem("dir",
                        new DirItem("subdir",
                            new FileItem("2.txt")),
                        new FileItem("1.txt")));

                var uncTempPath = fs.TempPath.Replace(@"C:\", @"\\localhost\C$\");
                var sourcePath = Path.Combine(uncTempPath, "dir");
                var destPath = Path.Combine(uncTempPath, "dir2");
                var values = new KeyValueList<string, string>
                {
                    {"action", "COPY"},
                    {"sourcePath", sourcePath},
                    {"destPath", destPath}
                };

                Assert.False(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));

                var result = await _restApiClient.Post("/api/fs", values);

                Assert.Equal(HttpStatusCode.OK, result.Response.StatusCode);

                // verify source
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir")));
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir\\subdir")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir\\subdir\\2.txt")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir\\1.txt")));

                // verify destination
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2")));
                Assert.True(Directory.Exists(Path.Combine(fs.TempPath, "dir2\\subdir")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\subdir\\2.txt")));
                Assert.True(File.Exists(Path.Combine(fs.TempPath, "dir2\\1.txt")));
            }
        }
    }
}
