using EbonySnapsManager.Crypto;
using EbonySnapsManager.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EbonySnapsManager.LargeProcesses
{
    internal class SavedataProcesses
    {
        private static readonly byte[] SnapSubStructId = new byte[] { 0xE1, 0xC1, 0x1F, 0xEE, 0x93, 0xDF, 0xB2, 0xB6 };

        public static void AddSnapInSave(string saveFile, uint newSnapId)
        {
            var decSaveData = Decrypt.BeginDecryption(File.ReadAllBytes(saveFile));
            var locatedStructOffset = SavedataHelpers.LocateOffset(decSaveData);

            var headerData = new byte[12];
            Array.Copy(decSaveData, headerData, headerData.Length);

            var dataTillSnapStruct = new byte[locatedStructOffset - 16];
            Array.ConstrainedCopy(decSaveData, 16, dataTillSnapStruct, 0, dataTillSnapStruct.Length);

            var snapDataDict = new Dictionary<int, byte[]>();
            var updatedSnapCount = uint.MinValue;
            var updatedSnapDataSize = uint.MinValue;
            var newSnapTime = ulong.MinValue;

            var dataTillFooterOffset = new byte[] { };
            var footerData = new byte[] { };
            var encFooterData = new byte[] { };

            using (var saveDataReader = new BinaryReader(new MemoryStream(decSaveData)))
            {
                saveDataReader.BaseStream.Position = 12;
                var footerOffset = (int)saveDataReader.ReadUInt32();

                saveDataReader.BaseStream.Position = locatedStructOffset + 8;
                var snapCount = saveDataReader.ReadUInt32();

                for (int i = 0; i < snapCount; i++)
                {
                    var structId = saveDataReader.ReadBytes(8);

                    if (!structId.SequenceEqual(SnapSubStructId))
                    {
                        throw new Exception();
                    }

                    var snapId = saveDataReader.ReadUInt32();
                    var attributeFieldsCount = saveDataReader.ReadUInt32();
                    var attributeFieldData = saveDataReader.ReadBytes((int)attributeFieldsCount * 4);
                    var snapTime = saveDataReader.ReadUInt64();
                    var remainingData = saveDataReader.ReadBytes(59);

                    newSnapTime = snapTime + 4;

                    var currentSnapRecordData = new List<byte>();

                    currentSnapRecordData.AddRange(structId);
                    currentSnapRecordData.AddRange(BitConverter.GetBytes(snapId));
                    currentSnapRecordData.AddRange(BitConverter.GetBytes(attributeFieldsCount));
                    currentSnapRecordData.AddRange(attributeFieldData);
                    currentSnapRecordData.AddRange(BitConverter.GetBytes(snapTime));
                    currentSnapRecordData.AddRange(remainingData);

                    updatedSnapDataSize += (uint)currentSnapRecordData.Count();

                    snapDataDict.Add(i, currentSnapRecordData.ToArray());
                    updatedSnapCount++;
                }

                var currentPos = (int)saveDataReader.BaseStream.Position;
                dataTillFooterOffset = saveDataReader.ReadBytes(footerOffset - currentPos);

                currentPos = (int)saveDataReader.BaseStream.Position;
                footerData = saveDataReader.ReadBytes((int)((saveDataReader.BaseStream.Length - 53) - currentPos));
                encFooterData = saveDataReader.ReadBytes(53);
            }

            var newSnapRecordData = new List<byte>();

            newSnapRecordData.AddRange(SnapSubStructId);
            newSnapRecordData.AddRange(BitConverter.GetBytes(newSnapId));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)8));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)16929630));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)16929637));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)16929638));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)17001451));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)17001463));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)17017688));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)17061971));
            newSnapRecordData.AddRange(BitConverter.GetBytes((uint)17075085));
            newSnapRecordData.AddRange(BitConverter.GetBytes(newSnapTime));
            newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)5332261958806667265));
            newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)2305843010301418620));
            newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)8214565721402917683));
            newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)1087028557));
            newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)1072693248));
            newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)50331648));
            newSnapRecordData.AddRange(BitConverter.GetBytes(ulong.MinValue));
            newSnapRecordData.AddRange(BitConverter.GetBytes(ushort.MinValue));
            newSnapRecordData.Add(0);

            snapDataDict.Add((int)updatedSnapCount, newSnapRecordData.ToArray());
            updatedSnapCount++;
            updatedSnapDataSize += 115;

            var updatedSaveData = new byte[] { };

            using (var updatedSaveDataStream = new MemoryStream())
            {
                using (var updatedSaveDataWriter = new BinaryWriter(updatedSaveDataStream))
                {
                    updatedSaveDataWriter.Write(headerData);
                    updatedSaveDataWriter.Write((uint)(16 + dataTillSnapStruct.Length + 12 + updatedSnapDataSize + dataTillFooterOffset.Length));
                    updatedSaveDataWriter.Write(dataTillSnapStruct);
                    updatedSaveDataWriter.Write(SavedataHelpers.SnapStructId);
                    updatedSaveDataWriter.Write(updatedSnapCount);

                    foreach (var snapData in snapDataDict.Values)
                    {
                        updatedSaveDataWriter.Write(snapData);
                    }

                    updatedSaveDataWriter.Write(dataTillFooterOffset);
                    updatedSaveDataWriter.Write(footerData);

                    var currentPos = updatedSaveDataWriter.BaseStream.Position;
                    var padValue = 16;

                    if (currentPos % padValue != 0)
                    {
                        var remainder = currentPos % padValue;
                        var increaseByteAmount = padValue - remainder;

                        var newSize = currentPos + increaseByteAmount;
                        var padNulls = newSize - currentPos;

                        updatedSaveDataWriter.BaseStream.PadNull((int)padNulls);
                    }

                    updatedSaveDataWriter.Write(encFooterData);

                    updatedSaveDataStream.Seek(0, SeekOrigin.Begin);
                    updatedSaveData = updatedSaveDataStream.ToArray();
                }
            }

            var outEncData = Encrypt.BeginEncryption(updatedSaveData);
            File.Delete(saveFile);

            File.WriteAllBytes(saveFile, outEncData);
        }


        public static void RemoveBlankSnapsInSave(string saveFile, string snapshotDir)
        {
            var decSaveData = Decrypt.BeginDecryption(File.ReadAllBytes(saveFile));
            var locatedStructOffset = SavedataHelpers.LocateOffset(decSaveData);

            var headerData = new byte[12];
            Array.Copy(decSaveData, headerData, headerData.Length);

            var dataTillSnapStruct = new byte[locatedStructOffset - 16];
            Array.ConstrainedCopy(decSaveData, 16, dataTillSnapStruct, 0, dataTillSnapStruct.Length);

            var snapDataDict = new Dictionary<int, byte[]>();
            var updatedSnapCount = uint.MinValue;
            var updatedSnapDataSize = uint.MinValue;

            var dataTillFooterOffset = new byte[] { };
            var footerData = new byte[] { };
            var encFooterData = new byte[] { };

            using (var saveDataReader = new BinaryReader(new MemoryStream(decSaveData)))
            {
                saveDataReader.BaseStream.Position = 12;
                var footerOffset = (int)saveDataReader.ReadUInt32();

                saveDataReader.BaseStream.Position = locatedStructOffset + 8;
                var snapCount = saveDataReader.ReadUInt32();

                for (int i = 0; i < snapCount; i++)
                {
                    var structId = saveDataReader.ReadBytes(8);

                    if (!structId.SequenceEqual(SnapSubStructId))
                    {
                        throw new Exception();
                    }

                    var snapId = saveDataReader.ReadUInt32();
                    var attributeFieldsCount = saveDataReader.ReadUInt32();
                    var attributeFieldData = saveDataReader.ReadBytes((int)attributeFieldsCount * 4);
                    var snapTime = saveDataReader.ReadUInt64();
                    var remainingData = saveDataReader.ReadBytes(59);

                    if (File.Exists(Path.Combine(snapshotDir, $"{Convert.ToString(snapId).PadLeft(8, '0')}.ss")))
                    {
                        var currentSnapRecordData = new List<byte>();

                        currentSnapRecordData.AddRange(structId);
                        currentSnapRecordData.AddRange(BitConverter.GetBytes(snapId));
                        currentSnapRecordData.AddRange(BitConverter.GetBytes(attributeFieldsCount));
                        currentSnapRecordData.AddRange(attributeFieldData);
                        currentSnapRecordData.AddRange(BitConverter.GetBytes(snapTime));
                        currentSnapRecordData.AddRange(remainingData);

                        updatedSnapDataSize += (uint)currentSnapRecordData.Count();

                        snapDataDict.Add(i, currentSnapRecordData.ToArray());
                        updatedSnapCount++;
                    }
                }

                var currentPos = (int)saveDataReader.BaseStream.Position;
                dataTillFooterOffset = saveDataReader.ReadBytes(footerOffset - currentPos);

                currentPos = (int)saveDataReader.BaseStream.Position;
                footerData = saveDataReader.ReadBytes((int)((saveDataReader.BaseStream.Length - 53) - currentPos));
                encFooterData = saveDataReader.ReadBytes(53);
            }

            var updatedSaveData = new byte[] { };

            using (var updatedSaveDataStream = new MemoryStream())
            {
                using (var updatedSaveDataWriter = new BinaryWriter(updatedSaveDataStream))
                {
                    updatedSaveDataWriter.Write(headerData);
                    updatedSaveDataWriter.Write((uint)(16 + dataTillSnapStruct.Length + 12 + updatedSnapDataSize + dataTillFooterOffset.Length));
                    updatedSaveDataWriter.Write(dataTillSnapStruct);
                    updatedSaveDataWriter.Write(SavedataHelpers.SnapStructId);
                    updatedSaveDataWriter.Write(updatedSnapCount);

                    foreach (var snapData in snapDataDict.Values)
                    {
                        updatedSaveDataWriter.Write(snapData);
                    }

                    updatedSaveDataWriter.Write(dataTillFooterOffset);
                    updatedSaveDataWriter.Write(footerData);

                    var currentPos = updatedSaveDataWriter.BaseStream.Position;
                    var padValue = 16;

                    if (currentPos % padValue != 0)
                    {
                        var remainder = currentPos % padValue;
                        var increaseByteAmount = padValue - remainder;

                        var newSize = currentPos + increaseByteAmount;
                        var padNulls = newSize - currentPos;

                        updatedSaveDataWriter.BaseStream.PadNull((int)padNulls);
                    }

                    updatedSaveDataWriter.Write(encFooterData);

                    updatedSaveDataStream.Seek(0, SeekOrigin.Begin);
                    updatedSaveData = updatedSaveDataStream.ToArray();
                }
            }

            var outEncData = Encrypt.BeginEncryption(updatedSaveData);
            File.Delete(saveFile);

            File.WriteAllBytes(saveFile, outEncData);
        }
    }
}