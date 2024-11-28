using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace EbonySnapsManager
{
    internal class SavedataHelpers
    {
        public static readonly byte[] SnapStructId = new byte[] { 0x32, 0x91, 0x76, 0xEE, 0x72, 0xFF, 0xD3, 0xEB };

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

                        if (currentBytes.SequenceEqual(SnapStructId))
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