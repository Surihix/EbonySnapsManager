using EbonySnapsManager.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EbonySnapsManager.LargeProcesses
{
    internal class SnapshotProcesses
    {
        private static readonly byte[] ShotStructId = new byte[] { 0xA1, 0x1C, 0xFC, 0x58, 0x08, 0x02, 0x47, 0x2E };

        public static void AddSnapsInLink(string snapshotlinkFile, ref uint newShotId)
        {
            var decLinkData = Decrypt.BeginDecryption(File.ReadAllBytes(snapshotlinkFile));

            var headerData = new byte[12];
            Array.Copy(decLinkData, headerData, headerData.Length);

            var dataTillLinkStruct = new byte[16];
            Array.ConstrainedCopy(decLinkData, 16, dataTillLinkStruct, 0, dataTillLinkStruct.Length);

            var shotRecordDataDict = new Dictionary<int, byte[]>();
            var updatedShotCount = uint.MinValue;
            var updatedShotRecordDataSize = uint.MinValue;
            var nextShotId = uint.MinValue;

            var footerData = new byte[] { };
            var encFooterData = new byte[] { };

            using (var snapshotlinkReader = new BinaryReader(new MemoryStream(decLinkData)))
            {
                snapshotlinkReader.BaseStream.Position = 12;
                var footerOffset = (int)snapshotlinkReader.ReadUInt32();

                snapshotlinkReader.BaseStream.Position = 32;
                var shotCount = snapshotlinkReader.ReadUInt32();

                for (int i = 0; i < shotCount; i++)
                {
                    var structId = snapshotlinkReader.ReadBytes(8);

                    if (!structId.SequenceEqual(ShotStructId))
                    {
                        throw new Exception();
                    }

                    var shotId = snapshotlinkReader.ReadUInt32();
                    var fieldsCount = snapshotlinkReader.ReadUInt32();
                    var fieldsData = snapshotlinkReader.ReadBytes((int)fieldsCount * 4);

                    var currentShotRecord = new List<byte>();

                    currentShotRecord.AddRange(structId);
                    currentShotRecord.AddRange(BitConverter.GetBytes(shotId));
                    currentShotRecord.AddRange(BitConverter.GetBytes(fieldsCount));
                    currentShotRecord.AddRange(fieldsData);

                    updatedShotRecordDataSize += (uint)currentShotRecord.Count();

                    shotRecordDataDict.Add(i, currentShotRecord.ToArray());
                    updatedShotCount++;
                }

                nextShotId = snapshotlinkReader.ReadUInt32();
                snapshotlinkReader.BaseStream.Position += 8;

                var currentPos = (int)snapshotlinkReader.BaseStream.Position;
                footerData = snapshotlinkReader.ReadBytes((int)((snapshotlinkReader.BaseStream.Length - 53) - currentPos));
                encFooterData = snapshotlinkReader.ReadBytes(53);
            }

            var newShotRecord = new List<byte>();
            newShotId = nextShotId;
            nextShotId++;

            newShotRecord.AddRange(ShotStructId);
            newShotRecord.AddRange(BitConverter.GetBytes(newShotId));
            newShotRecord.AddRange(BitConverter.GetBytes((uint)3));
            newShotRecord.AddRange(BitConverter.GetBytes((uint)1));
            newShotRecord.AddRange(BitConverter.GetBytes((uint)2));
            newShotRecord.AddRange(BitConverter.GetBytes((uint)0));

            shotRecordDataDict.Add((int)updatedShotCount, newShotRecord.ToArray());
            updatedShotCount++;
            updatedShotRecordDataSize += (uint)newShotRecord.Count();

            var updatedSnapshotlinkData = new byte[] { };

            using (var updatedSnapshotlinkStream = new MemoryStream())
            {
                using (var updatedSnapshotlinkWriter = new BinaryWriter(updatedSnapshotlinkStream))
                {
                    updatedSnapshotlinkWriter.Write(headerData);
                    updatedSnapshotlinkWriter.Write((uint)(16 + dataTillLinkStruct.Length + updatedShotRecordDataSize + 16));
                    updatedSnapshotlinkWriter.Write(dataTillLinkStruct);
                    updatedSnapshotlinkWriter.Write(updatedShotCount);

                    foreach (var shotRecordData in shotRecordDataDict.Values)
                    {
                        updatedSnapshotlinkWriter.Write(shotRecordData);
                    }

                    updatedSnapshotlinkWriter.Write(nextShotId);
                    updatedSnapshotlinkWriter.Write(ulong.MaxValue);
                    updatedSnapshotlinkWriter.Write(footerData);

                    var currentPos = updatedSnapshotlinkWriter.BaseStream.Position;
                    var padValue = 16;

                    if (currentPos % padValue != 0)
                    {
                        var remainder = currentPos % padValue;
                        var increaseByteAmount = padValue - remainder;

                        var newSize = currentPos + increaseByteAmount;
                        var padNulls = newSize - currentPos;

                        updatedSnapshotlinkWriter.BaseStream.PadNull((int)padNulls);
                    }

                    updatedSnapshotlinkWriter.Write(encFooterData);

                    updatedSnapshotlinkWriter.Seek(0, SeekOrigin.Begin);
                    updatedSnapshotlinkData = updatedSnapshotlinkStream.ToArray();
                }
            }

            var outSnapshotlinkData = Encrypt.BeginEncryption(updatedSnapshotlinkData);
            File.Delete(snapshotlinkFile);

            File.WriteAllBytes(snapshotlinkFile, outSnapshotlinkData);
        }


        public static void RemoveBlankSnapsInlink(string snapshotlinkFile, string snapshotDir)
        {
            var decLinkData = Decrypt.BeginDecryption(File.ReadAllBytes(snapshotlinkFile));

            var headerData = new byte[12];
            Array.Copy(decLinkData, headerData, headerData.Length);

            var dataTillLinkStruct = new byte[16];
            Array.ConstrainedCopy(decLinkData, 16, dataTillLinkStruct, 0, dataTillLinkStruct.Length);

            var shotRecordDataDict = new Dictionary<int, byte[]>();
            var updatedShotCount = uint.MinValue;
            var updatedShotRecordDataSize = uint.MinValue;
            var nextShotId = uint.MinValue;

            var footerData = new byte[] { };
            var encFooterData = new byte[] { };

            using (var snapshotlinkReader = new BinaryReader(new MemoryStream(decLinkData)))
            {
                snapshotlinkReader.BaseStream.Position = 12;
                var footerOffset = (int)snapshotlinkReader.ReadUInt32();

                snapshotlinkReader.BaseStream.Position = 32;
                var shotCount = snapshotlinkReader.ReadUInt32();

                for (int i = 0; i < shotCount; i++)
                {
                    var structId = snapshotlinkReader.ReadBytes(8);

                    if (!structId.SequenceEqual(ShotStructId))
                    {
                        throw new Exception();
                    }

                    var shotId = snapshotlinkReader.ReadUInt32();
                    var fieldsCount = snapshotlinkReader.ReadUInt32();
                    var fieldsData = snapshotlinkReader.ReadBytes((int)fieldsCount * 4);

                    if (File.Exists(Path.Combine(snapshotDir, $"{Convert.ToString(shotId).PadLeft(8, '0')}.ss")))
                    {
                        var currentShotRecord = new List<byte>();

                        currentShotRecord.AddRange(structId);
                        currentShotRecord.AddRange(BitConverter.GetBytes(shotId));
                        currentShotRecord.AddRange(BitConverter.GetBytes(fieldsCount));
                        currentShotRecord.AddRange(fieldsData);

                        updatedShotRecordDataSize += (uint)currentShotRecord.Count();

                        shotRecordDataDict.Add(i, currentShotRecord.ToArray());
                        updatedShotCount++;
                    }
                }

                nextShotId = snapshotlinkReader.ReadUInt32();
                snapshotlinkReader.BaseStream.Position += 8;

                var currentPos = (int)snapshotlinkReader.BaseStream.Position;
                footerData = snapshotlinkReader.ReadBytes((int)((snapshotlinkReader.BaseStream.Length - 53) - currentPos));
                encFooterData = snapshotlinkReader.ReadBytes(53);
            }

            var updatedSnapshotlinkData = new byte[] { };

            using (var updatedSnapshotlinkStream = new MemoryStream())
            {
                using (var updatedSnapshotlinkWriter = new BinaryWriter(updatedSnapshotlinkStream))
                {
                    updatedSnapshotlinkWriter.Write(headerData);
                    updatedSnapshotlinkWriter.Write((uint)(16 + dataTillLinkStruct.Length + updatedShotRecordDataSize + 16));
                    updatedSnapshotlinkWriter.Write(dataTillLinkStruct);
                    updatedSnapshotlinkWriter.Write(updatedShotCount);

                    foreach (var shotRecordData in shotRecordDataDict.Values)
                    {
                        updatedSnapshotlinkWriter.Write(shotRecordData);
                    }

                    updatedSnapshotlinkWriter.Write(nextShotId);
                    updatedSnapshotlinkWriter.Write(ulong.MaxValue);
                    updatedSnapshotlinkWriter.Write(footerData);

                    var currentPos = updatedSnapshotlinkWriter.BaseStream.Position;
                    var padValue = 16;

                    if (currentPos % padValue != 0)
                    {
                        var remainder = currentPos % padValue;
                        var increaseByteAmount = padValue - remainder;

                        var newSize = currentPos + increaseByteAmount;
                        var padNulls = newSize - currentPos;

                        updatedSnapshotlinkWriter.BaseStream.PadNull((int)padNulls);
                    }

                    updatedSnapshotlinkWriter.Write(encFooterData);

                    updatedSnapshotlinkWriter.Seek(0, SeekOrigin.Begin);
                    updatedSnapshotlinkData = updatedSnapshotlinkStream.ToArray();
                }
            }

            var outSnapshotlinkData = Encrypt.BeginEncryption(updatedSnapshotlinkData);
            File.Delete(snapshotlinkFile);

            File.WriteAllBytes(snapshotlinkFile, outSnapshotlinkData);
        }
    }
}