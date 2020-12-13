using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.gui
{
    public class GuiDebug2
    {
        private Rect rect;
        private GuiDebug2Window[] debugWindows;
        private string debugLabel;
        private bool windowEnabled = false;
        public GuiDebug2(int left, int top, int width, string debugLabel, GuiDebug2Window[] debugWindows)
        {
            rect = new Rect(left, top, width, Screen.height - top);
            this.debugWindows = debugWindows;
            this.debugLabel = debugLabel;
        }

        internal void Handle(GuiDebugInfo[] guiDebugInfos)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginVertical(debugLabel, "window", GUILayout.ExpandHeight(false));
            foreach (GuiDebug2Window debugWindow in debugWindows)
            {
                debugWindow.Handle(guiDebugInfos);
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
