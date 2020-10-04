using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    public class ChipSave
    {
        public bool chipProgrammed { get; set; } = false;
        public float[,] map { get; set; } = null;
    }
}
