using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.gui
{
    public class GuiDebugInfo
    {
        internal string elementName;
        private string description;
        private string value;
        private bool isLabel = false;
        private GUIStyle valueStyle;
        public GuiDebugInfo(string elementName, string description, string value)
        {
            this.elementName = elementName;
            this.description = description;
            this.value = value;
            valueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleRight,
            };
        }
        public GuiDebugInfo(string elementName, string description)
        {
            this.elementName = elementName;
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
            GUILayout.BeginHorizontal();

            GUILayout.Label(description);
            GUILayout.BeginVertical("box");
            GUILayout.Label(value, valueStyle);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
