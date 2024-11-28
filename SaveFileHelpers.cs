using EbonySnapsManager.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace EbonySnapsManager
{
    internal class SaveFileHelpers
    {
        private static readonly byte[] SnapStructId1 = new byte[] { 0x32, 0x91, 0x76, 0xEE, 0x72, 0xFF, 0xD3, 0xEB };
        private static readonly byte[] SnapStructId2 = new byte[] { 0xE1, 0xC1, 0x1F, 0xEE, 0x93, 0xDF, 0xB2, 0xB6 };

        public static void RemoveBlankSnapsSaveProcess(string saveFile, string snapshotDir)
        {
            var decSaveData = Decrypt.BeginDecryption(File.ReadAllBytes(saveFile));
            var locatedStructOffset = LocateOffset(decSaveData);

            var headerData = new byte[12];
            Array.Copy(decSaveData, headerData, headerData.Length);

            var dataTillSnapStruct = new byte[locatedStructOffset - 16];
            Array.ConstrainedCopy(decSaveData, 16, dataTillSnapStruct, 0, dataTillSnapStruct.Length);

            var snapDataDict = new Dictionary<int, byte[]>();
            var dataTillFooterOffset = new byte[] { };
            var footerData = new byte[] { };

            var updatedSnapCount = uint.MinValue;
            var updatedSnapDataSize = uint.MinValue;

            using (var saveDataReader = new BinaryReader(new MemoryStream(decSaveData)))
            {
                saveDataReader.BaseStream.Position = 12;
                var footerOffset = (int)saveDataReader.ReadUInt32();

                saveDataReader.BaseStream.Position = locatedStructOffset + 8;
                var snapCount = saveDataReader.ReadUInt32();

                for (int i = 0; i < snapCount; i++)
                {
                    var structId = saveDataReader.ReadBytes(8);

                    if (!structId.SequenceEqual(SnapStructId2))
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

                footerData = saveDataReader.ReadBytes((int)(saveDataReader.BaseStream.Length - saveDataReader.BaseStream.Position));
            }

            var updatedSaveData = new byte[] { };

            using (var updatedSaveDataStream = new MemoryStream())
            {
                using (var updatedSaveDataWriter  = new BinaryWriter(updatedSaveDataStream))
                {
                    updatedSaveDataWriter.Write(headerData);
                    updatedSaveDataWriter.Write((uint)(16 + dataTillSnapStruct.Length + 12 + updatedSnapDataSize + dataTillFooterOffset.Length));
                    updatedSaveDataWriter.Write(dataTillSnapStruct);
                    updatedSaveDataWriter.Write(SnapStructId1);
                    updatedSaveDataWriter.Write(updatedSnapCount);

                    for (int i = 0; i < updatedSnapCount; i++)
                    {
                        updatedSaveDataWriter.Write(snapDataDict[i]);
                    }

                    updatedSaveDataWriter.Write(dataTillFooterOffset);
                    updatedSaveDataWriter.Write(footerData);

                    updatedSaveDataStream.Seek(0, SeekOrigin.Begin);
                    updatedSaveData = updatedSaveDataStream.ToArray();
                }
            }

            var outEncData = Encrypt.BeginEncryption(updatedSaveData);
            File.Delete(saveFile);

            File.WriteAllBytes(saveFile, outEncData);
        }


        private static string PrevLoadedDataHash { get; set; }
        private static long PrevLocatedOffset { get; set; }
        public static long LocateOffset(byte[] saveData)
        {
            long locatedOffset = 0;
            bool shouldLocate = false;

            using (var sha256Hash = SHA256.Create())
            {
                var dataHash = BitConverter.ToString(sha256Hash.ComputeHash(saveData)).Replace("-", "");

                if (dataHash != PrevLoadedDataHash)
                {
                    PrevLoadedDataHash = dataHash;
                    shouldLocate = true;
                }
                else
                {
                    locatedOffset = PrevLocatedOffset;
                }
            }

            if (shouldLocate)
            {
                long readAmount;
                long readSkipAmount;

                using (var reader = new BinaryReader(new MemoryStream(saveData)))
                {
                    reader.BaseStream.Position = 12;
                    readAmount = reader.ReadUInt32();

                    reader.BaseStream.Position = 36;
                    readSkipAmount = reader.ReadUInt32();

                    reader.BaseStream.Position += readSkipAmount + 104;

                    if (reader.ReadUInt32() == 0)
                    {
                        reader.BaseStream.Position += 37;
                        readSkipAmount = reader.ReadUInt32() + 8;
                    }
                    else
                    {
                        readSkipAmount = 49;
                    }

                    reader.BaseStream.Position += readSkipAmount;

                    var pos = reader.BaseStream.Position;
                    readAmount -= pos;
                    var currentBytes = new byte[8];

                    for (int i = 0; i < readAmount; i++)
                    {
                        reader.BaseStream.Position = pos + i;
                        currentBytes = reader.ReadBytes(8);

                        if (currentBytes.SequenceEqual(SnapStructId1))
                        {
                            locatedOffset = pos + i;
                            break;
                        }
                    }
                }

                PrevLocatedOffset = locatedOffset;
            }

            return locatedOffset;
        }
    }
}