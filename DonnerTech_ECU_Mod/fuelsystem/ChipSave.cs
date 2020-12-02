using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    public class ChipSave
    {

        public float sparkAngle { get; set; } = 20.0f;

        public bool chipProgrammed { get; set; } = false;
        public bool startAssistEnabled { get; set; } = false;
        public float[,] map { get; set; } = null;

        public static string[] LoadSaveFiles(string savePath, string filenaming)
        {
            return Directory.GetFiles(savePath, filenaming, SearchOption.AllDirectories);
        }

    }
}
