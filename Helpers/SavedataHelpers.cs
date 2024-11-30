using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EbonySnapsManager.Helpers
{
    internal class SavedataHelpers
    {
        private static readonly byte[] SnapContainerStructId = new byte[] { 0x32, 0x91, 0x76, 0xEE, 0x72, 0xFF, 0xD3, 0xEB };
        private static readonly byte[] SnapStructId = new byte[] { 0xE1, 0xC1, 0x1F, 0xEE, 0x93, 0xDF, 0xB2, 0xB6 };

        public static long LocateOffset(byte[] saveData)
        {
            long locatedOffset = 0;

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

                    if (currentBytes.SequenceEqual(SnapContainerStructId))
                    {
                        locatedOffset = pos + i;
                        break;
                    }
                }
            }

            return locatedOffset;
        }


        private static byte[] HeaderData = new byte[12];
        private static byte[] DataTillSnapStruct { get; set; }
        public static void InitialDataOperations(byte[] decSaveData, long locatedStructOffset)
        {
            Array.Copy(decSaveData, HeaderData, HeaderData.Length);

            DataTillSnapStruct = new byte[locatedStructOffset - 16];
            Array.ConstrainedCopy(decSaveData, 16, DataTillSnapStruct, 0, DataTillSnapStruct.Length);
        }


        private static byte[] StructId { get; set; }
        public static uint SnapId { get; set; }
        private static uint AttributeFieldsCount { get; set; }
        private static byte[] AttributeFieldData { get; set; }
        private static ulong SnapTime { get; set; }
        private static byte[] RemainingData { get; set; }
        private static ulong NewSnapTime { get; set; }
        public static void ReadSnapRecordDataInSave(BinaryReader saveDataReader, bool isAddingSnap)
        {
            StructId = saveDataReader.ReadBytes(8);

            if (!StructId.SequenceEqual(SnapStructId))
            {
                throw new Exception();
            }

            SnapId = saveDataReader.ReadUInt32();
            AttributeFieldsCount = saveDataReader.ReadUInt32();
            AttributeFieldData = saveDataReader.ReadBytes((int)AttributeFieldsCount * 4);
            SnapTime = saveDataReader.ReadUInt64();
            RemainingData = saveDataReader.ReadBytes(59);

            if (isAddingSnap)
            {
                NewSnapTime = SnapTime + 4;
            }
        }


        public static uint UpdatedSnapDataSize { get; set; }
        public static uint UpdatedSnapCount { get; set; }
        private static int DictIndex { get; set; }
        public static void PackSnapDataRecordToDict(Dictionary<int, byte[]> snapDataDict)
        {
            var currentSnapRecordData = new List<byte>();

            currentSnapRecordData.AddRange(SnapStructId);
            currentSnapRecordData.AddRange(BitConverter.GetBytes(SnapId));
            currentSnapRecordData.AddRange(BitConverter.GetBytes(AttributeFieldsCount));
            currentSnapRecordData.AddRange(AttributeFieldData);
            currentSnapRecordData.AddRange(BitConverter.GetBytes(SnapTime));
            currentSnapRecordData.AddRange(RemainingData);

            UpdatedSnapDataSize += (uint)currentSnapRecordData.Count();
            UpdatedSnapCount++;

            snapDataDict.Add(DictIndex, currentSnapRecordData.ToArray());
            DictIndex++;
        }


        private static byte[] DataTillFooterOffset = new byte[] { };
        private static byte[] FooterData = new byte[] { };
        private static byte[] EncFooterData = new byte[] { };
        public static void FooterOperations(BinaryReader saveDataReader, int footerOffset)
        {
            var currentPos = (int)saveDataReader.BaseStream.Position;
            DataTillFooterOffset = saveDataReader.ReadBytes(footerOffset - currentPos);

            currentPos = (int)saveDataReader.BaseStream.Position;
            FooterData = saveDataReader.ReadBytes((int)((saveDataReader.BaseStream.Length - 53) - currentPos));
            EncFooterData = saveDataReader.ReadBytes(53);
        }


        public static void AddNewSnapDataToDict(int snapsToAdd, uint newSnapId, Dictionary<int, byte[]> snapDataDict)
        {
            for (int i = 0; i < snapsToAdd; i++)
            {
                var newSnapRecordData = new List<byte>();

                newSnapRecordData.AddRange(SnapStructId);
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
                newSnapRecordData.AddRange(BitConverter.GetBytes(NewSnapTime));
                newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)5332261958806667265));
                newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)2305843010301418620));
                newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)8214565721402917683));
                newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)1087028557));
                newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)1072693248));
                newSnapRecordData.AddRange(BitConverter.GetBytes((ulong)50331648));
                newSnapRecordData.AddRange(BitConverter.GetBytes(ulong.MinValue));
                newSnapRecordData.AddRange(BitConverter.GetBytes(ushort.MinValue));
                newSnapRecordData.Add(0);

                snapDataDict.Add(DictIndex, newSnapRecordData.ToArray());

                newSnapId++;
                NewSnapTime += 4;

                UpdatedSnapDataSize += 115;
                UpdatedSnapCount++;
                DictIndex++;
            }
        }


        public static byte[] BuildUpdatedFileData(Dictionary<int, byte[]> snapDataDict)
        {
            var updatedSaveData = new byte[] { };

            using (var updatedSaveDataStream = new MemoryStream())
            {
                using (var updatedSaveDataWriter = new BinaryWriter(updatedSaveDataStream))
                {
                    updatedSaveDataWriter.Write(HeaderData);
                    updatedSaveDataWriter.Write((uint)(16 + DataTillSnapStruct.Length + 12 + UpdatedSnapDataSize + DataTillFooterOffset.Length));
                    updatedSaveDataWriter.Write(DataTillSnapStruct);
                    updatedSaveDataWriter.Write(SnapContainerStructId);
                    updatedSaveDataWriter.Write(UpdatedSnapCount);

                    foreach (var snapData in snapDataDict.Values)
                    {
                        updatedSaveDataWriter.Write(snapData);
                    }

                    updatedSaveDataWriter.Write(DataTillFooterOffset);
                    updatedSaveDataWriter.Write(FooterData);

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

                    updatedSaveDataWriter.Write(EncFooterData);

                    updatedSaveDataStream.Seek(0, SeekOrigin.Begin);
                    updatedSaveData = updatedSaveDataStream.ToArray();
                }
            }

            return updatedSaveData;
        }
    }
}