using EbonySnapsManager.Crypto;
using EbonySnapsManager.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace EbonySnapsManager.LargeProcesses
{
    internal class SavedataProcesses
    {
        public static void AddSnapInSave(string saveFile, uint newSnapId, int snapsToAdd)
        {
            var decSaveData = Decrypt.BeginDecryption(File.ReadAllBytes(saveFile));
            var locatedStructOffset = SavedataHelpers.LocateOffset(decSaveData);

            SavedataHelpers.InitialDataOperations(decSaveData, locatedStructOffset);

            var snapDataDict = new Dictionary<int, byte[]>();

            using (var saveDataReader = new BinaryReader(new MemoryStream(decSaveData)))
            {
                saveDataReader.BaseStream.Position = 12;
                var footerOffset = (int)saveDataReader.ReadUInt32();

                saveDataReader.BaseStream.Position = locatedStructOffset + 8;
                var snapCount = saveDataReader.ReadUInt32();

                for (int i = 0; i < snapCount; i++)
                {
                    SavedataHelpers.ReadSnapRecordDataInSave(saveDataReader, true);
                    SavedataHelpers.PackSnapDataRecordToDict(snapDataDict);
                }

                SavedataHelpers.FooterOperations(saveDataReader, footerOffset);
            }

            SavedataHelpers.AddNewSnapDataToDict(snapsToAdd, newSnapId, snapDataDict);

            var updatedSaveData = SavedataHelpers.BuildUpdatedFileData(snapDataDict);
            var outEncData = Encrypt.BeginEncryption(updatedSaveData);
            File.Delete(saveFile);

            File.WriteAllBytes(saveFile, outEncData);
        }


        public static void RemoveBlankSnapsInSave(string saveFile, string snapshotDir)
        {
            var decSaveData = Decrypt.BeginDecryption(File.ReadAllBytes(saveFile));
            var locatedStructOffset = SavedataHelpers.LocateOffset(decSaveData);

            SavedataHelpers.InitialDataOperations(decSaveData, locatedStructOffset);

            var snapDataDict = new Dictionary<int, byte[]>();

            using (var saveDataReader = new BinaryReader(new MemoryStream(decSaveData)))
            {
                saveDataReader.BaseStream.Position = 12;
                var footerOffset = (int)saveDataReader.ReadUInt32();

                saveDataReader.BaseStream.Position = locatedStructOffset + 8;
                var snapCount = saveDataReader.ReadUInt32();

                for (int i = 0; i < snapCount; i++)
                {
                    SavedataHelpers.ReadSnapRecordDataInSave(saveDataReader, false);

                    if (File.Exists(Path.Combine(snapshotDir, $"{Convert.ToString(SavedataHelpers.SnapId).PadLeft(8, '0')}.ss")))
                    {
                        SavedataHelpers.PackSnapDataRecordToDict(snapDataDict);
                    }
                }

                SavedataHelpers.FooterOperations(saveDataReader, footerOffset);
            }

            var updatedSaveData = SavedataHelpers.BuildUpdatedFileData(snapDataDict);
            var outEncData = Encrypt.BeginEncryption(updatedSaveData);
            File.Delete(saveFile);

            File.WriteAllBytes(saveFile, outEncData);
        }
    }
}