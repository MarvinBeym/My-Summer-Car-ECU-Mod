using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.gui
{
    public class GuiDebug2Window
    {
        private string buttonText;
        private bool windowEnabled = false;
        public GuiDebug2Window(string buttonText)
        {
            this.buttonText = buttonText;
        }

        public void Handle(GuiDebugInfo[] debugInfos)
        {
            if (GUILayout.Button(buttonText))
            {
                windowEnabled = !windowEnabled;
            }

            if (windowEnabled)
            {
                GUILayout.BeginVertical("", "box", GUILayout.ExpandHeight(false));
                foreach(GuiDebugInfo debugInfo in debugInfos)
                {
                    if(debugInfo.debugWindowName == buttonText)
                    {
                        debugInfo.Handle();
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
