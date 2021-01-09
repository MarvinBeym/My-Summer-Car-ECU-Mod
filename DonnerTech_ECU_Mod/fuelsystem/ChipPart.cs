using DonnerTech_ECU_Mod.fuelsystem;
using ModApi.Attachable;
using Parts;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace DonnerTech_ECU_Mod
{
    public class ChipPart : SimplePart
    {
        public string mapSaveFile { get; set; }
        public ChipSave chipSave { get; set; }
        public bool chipInstalledOnProgrammer { get; set; }
        public ChipPart(List<Object> loadedData, GameObject part, GameObject partParent, Vector3 installLocation, Quaternion installRotation) : base(loadedData, part, partParent, installLocation, installRotation)
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
        }

        protected override void disassemble(bool startup = false)
        {
            // do stuff on dissemble.
            base.disassemble(startup);
        }
    }
}
