using System.IO;

namespace Servant.End2EndTests.Helpers.FileSystem
{
    public abstract class FileSystemItem
    {
        protected FileSystemItem(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract void Create(DirectoryInfo rootDir);
    }
}
