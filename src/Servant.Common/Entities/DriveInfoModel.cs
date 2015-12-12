using System;
using System.IO;

namespace Servant.Common.Entities
{
    public class DriveInfoModel
    {
        public string Name { get; set; }

        public string DriveType { get; set; }

        public string DriveFormat { get; set; }

        public bool IsReady { get; set; }

        public long AvailableFreeSpace { get; set; }

        public long TotalFreeSpace { get; set; }

        public long TotalSize { get; set; }

        public string RootDirectory { get; set; }

        public string VolumeLabel { get; set; }

        public static DriveInfoModel FromDriveInfo(DriveInfo di)
        {
            return new DriveInfoModel
            {
                Name = SafeGetDriveProperty(() => di.Name),
                DriveType = SafeGetDriveProperty(() => Enum.GetName(typeof(DriveType), di.DriveType)),
                DriveFormat = SafeGetDriveProperty(() => di.DriveFormat),
                IsReady = SafeGetDriveProperty(() => di.IsReady),
                AvailableFreeSpace = SafeGetDriveProperty(() => di.AvailableFreeSpace),
                TotalFreeSpace = SafeGetDriveProperty(() => di.TotalFreeSpace),
                TotalSize = SafeGetDriveProperty(() => di.TotalSize),
                RootDirectory = SafeGetDriveProperty(() => di.RootDirectory.FullName),
                VolumeLabel = SafeGetDriveProperty(() => di.VolumeLabel)
            };
        }

        public static T SafeGetDriveProperty<T>(Func<T> getter)
        {
            try
            {
                return getter();
            }
            catch (IOException)
            {
                return default(T);
            }
            catch (UnauthorizedAccessException)
            {
                return default(T);
            }
        }
    }
}
