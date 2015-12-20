using System.IO;

namespace Servant.End2EndTests.Helpers.FileSystem
{
    public class DirItem : FileSystemItem
    {
        public DirItem(string name, params FileSystemItem[] items)
            : base(name)
        {
            Items = items;
        }

        public FileSystemItem[] Items { get; }

        public override void Create(DirectoryInfo rootDir)
        {
            var path = Path.Combine(rootDir.FullName, Name);
            var currentDir = Directory.CreateDirectory(path);
            foreach (var item in Items)
            {
                item.Create(currentDir);
            }
        }
    }
}
