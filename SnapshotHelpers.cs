using System.IO;
using System.Text;

namespace EbonySnapsManager
{
    internal class SnapshotHelpers
    {
        public static void RemoveBlankSnapslinkProcess(string saveFile, string snapshotDir)
        {

        }


        public static byte[] GetImgDataFromSnapshotFile(string ssFile)
        {
            var imgData = new byte[] { };

            if (File.Exists(ssFile))
            {
                using (var ssReader = new BinaryReader(File.Open(ssFile, FileMode.Open, FileAccess.Read)))
                {
                    ssReader.BaseStream.Seek(32, SeekOrigin.Begin);
                    var imgSize = ssReader.ReadUInt32();
                    imgData = new byte[imgSize];
                    imgData = ssReader.ReadBytes((int)imgSize);
                }
            }

            return imgData;
        }


        public static string SaveImgDataToFile(string outImgSavetimeFileName, string imgSaveDir, byte[] imgData)
        {
            var detectedExtn = Path.GetExtension(outImgSavetimeFileName);

            using (var imgStream = new MemoryStream())
            {
                using (var imgReader = new BinaryReader(imgStream))
                {
                    imgStream.Write(imgData, 0, imgData.Length);
                    imgStream.Seek(0, SeekOrigin.Begin);

                    switch (imgReader.ReadUInt16())
                    {
                        case 55551:
                            detectedExtn += ".jpg";
                            break;

                        case 20617:
                            detectedExtn += ".png";
                            break;
                    }
                }
            }

            var outImgFile = Path.Combine(imgSaveDir, Path.GetFileNameWithoutExtension(outImgSavetimeFileName) + detectedExtn);

            if (File.Exists(outImgFile))
            {
                File.Delete(outImgFile);
            }

            File.WriteAllBytes(outImgFile, imgData);

            return outImgFile;
        }


        public static void CreateNewSnapshotFile(string ssFile, byte[] imgData)
        {
            using (var ssStream = new MemoryStream())
            {
                using (var ssWriter = new BinaryWriter(ssStream))
                {
                    ssWriter.Write(Encoding.UTF8.GetBytes("ebb\0"));
                    ssWriter.Write((uint)4);
                    ssWriter.Write((uint)0);
                    ssWriter.Write((uint)imgData.Length + 36);
                    ssWriter.Write(4279566338);
                    ssWriter.Write((uint)1);
                    ssWriter.Write(0xA4CEBC89AE0F8ED9);

                    ssWriter.Write((uint)imgData.Length);
                    ssWriter.Write(imgData);

                    ssWriter.Write((uint)1);
                    ssWriter.Write((uint)49);
                    ssWriter.Write(Encoding.UTF8.GetBytes("Black.Save.Snapshot.SaveSnapshotImageBinaryStruct"));
                    ssWriter.Write(0xA4CEBC89AE0F8ED9);
                    ssWriter.Write(ulong.MaxValue);
                    ssWriter.Write((ushort)1);
                    ssWriter.Write((uint)7);
                    ssWriter.Write(Encoding.UTF8.GetBytes("binary_"));
                    ssWriter.Write((uint)27);
                    ssWriter.Write(Encoding.UTF8.GetBytes("Luminous.Core.Memory.Buffer"));
                    ssWriter.Write((uint)0);
                    ssWriter.Write((uint)24);
                    ssWriter.Write((uint)0x00160001);
                    ssWriter.Write((byte)0);


                    ssWriter.Seek(0, SeekOrigin.Begin);
                    File.WriteAllBytes(ssFile, ssStream.ToArray());
                }
            }
        }
    }
}