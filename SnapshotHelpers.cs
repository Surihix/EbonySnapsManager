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



    }
}