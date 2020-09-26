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
    public class TcsPart : SimplePart
    {
        public TcsPart(Object[] loadedData, GameObject part, GameObject partParent, Trigger trigger, Vector3 installLocation, Quaternion installRotation) : base(loadedData, part, partParent, trigger, installLocation, installRotation)
        {
        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo()
        {
            installed = false, //Will make part installed

            position = ModsShop.FleetariSpawnLocation.desk,
            rotation = Quaternion.Euler(0, 0, 0),
        };

        public override GameObject rigidPart
        {
            get;
            set;
        }
        public override GameObject activePart
        {
            get;
            set;
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
            if (mod != null && mod.smart_engine_module_logic != null && mod.smart_engine_module_logic.tcsModule_enabled != null && mod.smart_engine_module_logic.tcsModule_enabled.Value)
            {
                mod.smart_engine_module_logic.ToggleTCS();
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
