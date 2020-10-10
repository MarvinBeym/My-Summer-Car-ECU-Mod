using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DonnerTech_ECU_Mod.Reporter
{
    class Report
    {
        public string name { get; set; }
        public string[] files { get; set; }

        public bool directory { get; set; } = false;
    }
}