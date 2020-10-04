using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class Logger
    {
        private string logging_filePath = "";
        private string file_name = "";
        private DonnerTech_ECU_Mod mod;
        public Logger(DonnerTech_ECU_Mod mod, string file_name)
        {
            this.mod = mod;
            this.file_name = file_name;
            logging_filePath = Path.Combine(ModLoader.GetModConfigFolder(mod), file_name);
            if (!File.Exists(logging_filePath)){
                WriteBaseInformationToFile();
            }
        }

        private void WriteBaseInformationToFile()
        {
            string baseInformation = $@"
╔════════════════════════════════════════════════╗
║ Steam:        { BoolToCracked(ModLoader.CheckSteam()) }
║ OS:           { SystemInfo.operatingSystem }
║
║----
║
║ Mod:          { this.mod.Name }
║ Mod version:  { this.mod.Version }
╚════════════════════════════════════════════════╝
";
            using (StreamWriter streamWriter = new StreamWriter(logging_filePath, false))
            {
                streamWriter.Write(baseInformation);
            }
        }

        public void New(string message)
        {

        }

        private string BoolToCracked(bool steamValid)
        {
            if (steamValid)
            {
                return "Steam detected";
            }
            return "NO Steam detected";
        }

        public void WriteToFile(string message)
        {

        }
    }
}
