using EbonySnapsManager.Helpers;
using System.Collections.Generic;
using System.IO;

namespace EbonySnapsManager
{
    internal class SnapshotProcesses
    {
        public static void SaveSnapshotsInList(Dictionary<string, string> SnapshotFilesInDirDict, string snapshotsSaveDir)
        {
            foreach (var ssFile in SnapshotFilesInDirDict.Values)
            {
                if (File.Exists(ssFile))
                {
                    _ = SnapshotHelpers.SaveImgDataToFile(ssFile, snapshotsSaveDir, SnapshotHelpers.GetImgDataFromSnapshotFile(ssFile));
                }
            }
        }


        public static void RemoveBlankSnapsInlink(string snapshotlinkFile, string snapshotDir)
        {

        }
    }
}