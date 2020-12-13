using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.gui
{
    public class GuiDebugInfo
    {
        internal string debugWindowName;
        private string description;
        private string value;
        private bool isLabel = false;
        public GuiDebugInfo(string debugWindowName, string description, string value)
        {
            this.debugWindowName = debugWindowName;
            this.description = description;
            this.value = value;
        }
        public GuiDebugInfo(string debugWindowName, string description)
        {
            this.debugWindowName = debugWindowName;
            this.description = description;
            isLabel = true;
        }

        internal void Handle()
        {
            if (isLabel)
            {
                GUILayout.Label(description);
                return;
            }
            GUILayout.Label($"{description}: {value}");
        }
    }
}
