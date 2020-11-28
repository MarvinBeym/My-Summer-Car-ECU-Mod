using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.gui
{
    public class GuiInfo
    {
        public string parentElement = "";
        public string description;
        public string value;
        public GuiInfo(string parentElement, string description, string value)
        {
            this.parentElement = parentElement;
            this.description = description + ": ";
            this.value = value;
        }
    }
}
