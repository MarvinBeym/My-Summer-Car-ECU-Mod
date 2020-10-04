/*
using MSCLoader;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
namespace DonnerTech_ECU_Mod.Reporter
{
    class Reporter
    {
        public static void GenerateReport(Mod mod, Report[] reports)
        {
            string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), String.Format("mod_report_[{0}]_v{1}", mod.Name, mod.Version));
            using (FileStream fsOut = File.Create(outputPath))
            {
                using (var zipStream = new ZipOutputStream(fsOut))
                {
                    zipStream.SetLevel(3);
                    foreach(Report report in reports)
                    {
                        foreach(string file in report.files)
                        {
                            FileInfo fi = new FileInfo(file);

                            ZipEntry entry = new ZipEntry(fileToZip);
                            entry.DateTime = fi.LastWriteTime;
                            entry.Size = fi.Length;
                            zipStream.PutNextEntry(entry);

                            var buffer = new byte[4096];
                            using (FileStream fsInput = File.OpenRead(file))
                            {
                                ICSharpCode.SharpZipLib.Zip..Copy(fsInput, zipStream, buffer);
                            }
                        }
                        
                    }


                }
            }
        }
    }
}
*/