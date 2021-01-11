using MSCLoader;
using Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    public class Chip
    {
        internal ChipSave chipSave;
        internal string mapSaveFile;
        internal AdvPart part;
        internal bool chipInstalledOnProgrammer;

        public Chip(AdvPart part)
        {
            this.part = part;
        }

        internal void SaveFuelMap(Mod mod)
        {
            try
            {
                SaveLoad.SerializeSaveFile<ChipSave>(mod, chipSave, mapSaveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Unable to save chips, there was an error while trying to save the chip", $"save file: {mapSaveFile}", ex);
            }
        }
    }
}
