using System;
using System.IO;
using System.Text;

namespace Servant.Common.Entities
{
    public class FileSystemInfoModel
    {
        public string Mode { get; set; }

        public string Name { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        public long? Length { get; set; }

        public static FileSystemInfoModel FromFSInfo(FileSystemInfo fsInfo)
        {
            return new FileSystemInfoModel
            {
                Mode = GetMode(fsInfo.Attributes),
                Name = fsInfo.Name,
                LastWriteTimeUtc = fsInfo.LastWriteTimeUtc,
                Length = GetLength(fsInfo as FileInfo)
            };
        }

        private static long? GetLength(FileInfo fileInfo)
        {
            return fileInfo?.Length;
        }

        private static string GetMode(FileAttributes attr)
        {
            var sb = new StringBuilder();
            sb.Append(attr.HasFlag(FileAttributes.Directory) ? "d" : "-");
            sb.Append(attr.HasFlag(FileAttributes.Archive) ? "a" : "-");
            sb.Append(attr.HasFlag(FileAttributes.ReadOnly) ? "r" : "-");
            sb.Append(attr.HasFlag(FileAttributes.Hidden) ? "h" : "-");
            sb.Append(attr.HasFlag(FileAttributes.System) ? "s" : "-");
            sb.Append(attr.HasFlag(FileAttributes.ReparsePoint) ? "l" : "-");
            return sb.ToString();
        }
    }
}
