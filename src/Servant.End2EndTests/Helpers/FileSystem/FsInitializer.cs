using System;
using System.IO;
using System.Threading;

namespace Servant.End2EndTests.Helpers.FileSystem
{
    public class FsInitializer : IDisposable
    {
        private readonly DirectoryInfo _tempDir;

        public FsInitializer()
        {
            TempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _tempDir = Directory.CreateDirectory(TempPath);
        }

        public string TempPath { get; }

        public string CreateItems(params FileSystemItem[] items)
        {
            foreach (var item in items)
            {
                item.Create(_tempDir);
            }

            return _tempDir.FullName;
        }

        public void Dispose()
        {
            if (_tempDir != null)
                DeleteDir(_tempDir);
        }

        private static void DeleteDir(DirectoryInfo dir)
        {
            if (string.IsNullOrEmpty(dir.FullName))
                return;

            for (var i = 0; i < 5; ++i)
            {
                try
                {
                    Directory.Delete(dir.FullName, true);
                    break;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
