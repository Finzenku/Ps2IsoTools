using Ps2IsoTools.UDF;
using Ps2IsoTools.UDF.Files;
using System.Runtime.InteropServices;

namespace IsoToolTest
{
    internal class Program
    {
        private static readonly string viPatchPath = @"P:\DotHack\Fragment\ViPatch\fragmentVi.iso";
        private static readonly string viIsoPath = @"P:\DotHack\Fragment\ViPatch\fragmentVi";
        private static readonly string viPath = @"P:\DotHack\Fragment\ViPatch\";
        private static readonly string outputPath = @"P:\DotHack\Fragment\ViPatch\test2.iso";
        private static readonly string smbFolder = @"P:\PS2SMB\DVD\";
        private static readonly string infectionISOPath = smbFolder + @"Dot Hack Part 1 - Infection (USA) (En,Ja).iso";

private static byte[] foodPatch = new byte[]
{
    0x00, 0x00, 0x00, 0x00, 0x00, 0xD1, 0x00, 0x00, 0x00, 0xD8, 0x00, 0x00, 0x30, 0xD2, 0x01, 0x00,
    0x00, 0xB0, 0x02, 0x00, 0x00, 0x16, 0x01, 0x00, 0x00, 0xC8, 0x03, 0x00, 0x00, 0xB6, 0x01, 0x00,
    0x00, 0x80, 0x05, 0x00, 0x74, 0x7C, 0x01, 0x00, 0x00, 0x00, 0x07, 0x00, 0xA6, 0xC6, 0x01, 0x00,
    0x00, 0xC8, 0x08, 0x00, 0x00, 0xC2, 0x01, 0x00, 0x00, 0x90, 0x0A, 0x00, 0x50, 0x22, 0x01, 0x00,
    0x00, 0xB8, 0x0B, 0x00, 0x9A, 0xBB, 0x01, 0x00, 0x00, 0x78, 0x0D, 0x00, 0x56, 0x3C, 0x01, 0x00,
    0x00, 0xB8, 0x0E, 0x00, 0x0E, 0x65, 0x01, 0x00, 0x00, 0x20, 0x10, 0x00, 0x00, 0xC1, 0x01, 0x00,
    0x00, 0xE8, 0x11, 0x00, 0x54, 0x1B, 0x01, 0x00, 0x00, 0x08, 0x13, 0x00, 0xD8, 0x64, 0x01, 0x00,
    0x00, 0x70, 0x14, 0x00, 0x80, 0x51, 0x01, 0x00, 0x00, 0xC8, 0x15, 0x00, 0xBE, 0x7D, 0x01, 0x00
};

        static void Main(string[] args)
        {

            //// Builder Test: Create ISO from dumped files
            //UdfBuilder builder = new();
            //builder.VolumeIdentifier = "FRAGMENT";

            //string[] files = Directory.GetFiles(viIsoPath, "*.*", SearchOption.AllDirectories);
            //foreach (string file in files)
            //{
            //    using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read))
            //        builder.AddFile(file.Replace(viIsoPath, ""), fs);
            //}

            //builder.Build(viPath + "hacktest.iso");

            //// Reader Test: Read ISO made from Builder and copy file from it
            //using (UdfReader reader = new UdfReader(viPath + "foodtest.iso"))
            //{
            //    FileIdentifier? fi = reader.GetFileByName("hack_00.elf");
            //    if (fi is not null)
            //    {
            //        Console.WriteLine(fi.FileName);
            //        reader.CopyFile(fi, viPath + fi.FileName);
            //    }
            //}

            //var builder = new UdfBuilder();

            //using (FileStream fs = File.Open(viPath + "HACK_00.ELF", FileMode.Open, FileAccess.ReadWrite))
            //    builder.AddFile("HACK_00.ELF", fs);

            //builder.Build(viPath + "hacktest.iso");

            //// Editor Test: Replace File Stream with external FileStream and Rebuild ISO
            //using (UdfEditor editor = new(viPath + "test.iso"))
            //{
            //    var fi = editor.GetFileByName("HACK_00.ELF");
            //    if (fi is not null)
            //    {
            //        Console.WriteLine(fi.FileName);

            //        using (FileStream fs = File.Open(viPath + "HACK_00.ELF", FileMode.Open, FileAccess.ReadWrite))
            //            editor.ReplaceFileStream(fi, fs);

            //        editor.Rebuild(viPath + "foodtest.iso");
            //    }
            //}
            //UdfEditor fragBak = new(viPath + "fragment.iso", viPath + "fragmentBak.iso");
            //fragBak.Dispose();

            using (UdfEditor editor = new(viPath + "foodtest.iso"))
            using (UdfReader reader = new(infectionISOPath))
            {
                var foodE = reader.GetFileByName("food_E.bin");
                var food = editor.GetFileByName("food.bin");

                if (foodE is not null && food is not null)
                {
                    editor.ReplaceFileStream(food, reader.GetFileStream(foodE), true);
                    editor.Rebuild();

                    var gcmnf = editor.GetFileByName("gcmnf.prg");
                    if (gcmnf is not null)
                    {
                        using (BinaryWriter bw = new(editor.GetFileStream(gcmnf)))
                        {
                            bw.BaseStream.Position = 0xFBF80;
                            bw.Write(foodPatch);
                        }
                    }
                    var gcmno = editor.GetFileByName("gcmno.prg");
                    if (gcmno is not null)
                    {
                        using (BinaryWriter bw = new(editor.GetFileStream(gcmno)))
                        {
                            bw.BaseStream.Position = 0xEC620;
                            bw.Write(foodPatch);
                        }
                        using (BinaryReader br = new(editor.GetFileStream(gcmno)))
                        {
                            string s = br.ReadString();
                        }
                    }
                }
            }

            //using (UdfReader foodtest = new(viPath + "foodtest.iso"))
            //{
            //    var foodFiles = foodtest.GetAllEntries();
            //    long totalLen = 0;
            //    for (int i = 0; i < foodFiles.Count; i++)
            //    {
            //        var file = foodtest.ConvertFileIdToFile(foodFiles[i]);
            //        Console.WriteLine($"{foodFiles[i].FileName, -16}{file.FileLength:X8}  {file.FileEntry.LogicalBlocksRecorded*2048:X8}");
            //        totalLen += RoundUp(file.FileLength, 2048);
            //    }
            //    Console.WriteLine(totalLen.ToString("X8"));
            //}
        }
    }
}