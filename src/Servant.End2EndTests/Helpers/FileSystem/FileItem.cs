using System.IO;

namespace Servant.End2EndTests.Helpers.FileSystem
{
    public class FileItem : FileSystemItem
    {
        public FileItem(string name, string content = null)
            : base(name)
        {
            Content = content;
        }

        public string Content { get; }

        public override void Create(DirectoryInfo rootDir)
        {
            var path = Path.Combine(rootDir.FullName, Name);
            using (var reader = File.CreateText(path))
            {
                reader.Write(Content);
            }
        }
    }
}
