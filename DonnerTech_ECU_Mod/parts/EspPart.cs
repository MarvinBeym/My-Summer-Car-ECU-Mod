using ModApi.Attachable;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace DonnerTech_ECU_Mod
{
    public class EspPart : SimplePart
    {
        public EspPart(Object[] loadedData, GameObject part, GameObject partParent, Trigger trigger, Vector3 installLocation, Quaternion installRotation) : base(loadedData, part, partParent, trigger, installLocation, installRotation)
        {
        }

        protected override void assemble(bool startUp = false)
        {
            // do stuff on assemble.
            base.assemble(startUp);
            if (this.screwablePart != null)
            {
                this.screwablePart.setScrewsOnAssemble();
            }
        }

        protected override void disassemble(bool startup = false)
        {
            if (mod != null && mod.smart_engine_module_logic != null && mod.tcsModule_enabled != null && mod.tcsModule_enabled.Value)
            {
                mod.smart_engine_module_logic.ToggleESP();
            }
            // do stuff on dissemble.
            base.disassemble(startup); // if you want dissemble function, you need to call base!
            if (this.screwablePart != null)
            {
                this.screwablePart.resetScrewsOnDisassemble();
            }
        }
    }
}
