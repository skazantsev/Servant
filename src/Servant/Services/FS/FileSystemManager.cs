using Servant.Exceptions;
using System.IO;

namespace Servant.Services.FS
{
    public class FileSystemManager
    {
        public void Copy(string source, string dest, bool overwrite = false)
        {
            if (File.Exists(source))
                CopyFile(new FileInfo(source), new FileInfo(dest), overwrite);
            else if (Directory.Exists(source))
                CopyEntireDirectory(new DirectoryInfo(source), new DirectoryInfo(dest), overwrite);
            else
                throw new ServantApiException($"Could not find a file or a directory {source}.");
        }

        private static void CopyEntireDirectory(DirectoryInfo source, DirectoryInfo dest, bool overwrite)
        {
            foreach (var dir in source.GetDirectories())
            {
                CopyEntireDirectory(dir, dest.CreateSubdirectory(dir.Name), overwrite);
            }
            foreach (var file in source.GetFiles())
            {
                var destPath = Path.Combine(dest.FullName, file.Name);
                CopyFile(file, new FileInfo(destPath), overwrite);
            }
        }

        private static void CopyFile(FileInfo source, FileInfo dest, bool overwrite)
        {
            source.CopyTo(dest.FullName, overwrite);
        }
    }
}
