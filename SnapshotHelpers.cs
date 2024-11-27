using System.IO;

namespace EbonySnapsManager
{
    internal class SnapshotHelpers
    {
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
    }
}