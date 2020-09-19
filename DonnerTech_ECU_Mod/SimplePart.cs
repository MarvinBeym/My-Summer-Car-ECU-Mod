using ModApi.Attachable;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class SimplePart : Part
    {
        private ScrewablePart screwablePart;
        private Vector3 installLocation;

        public SimplePart(PartSaveInfo partSaveInfo, GameObject part, GameObject partParent, Trigger trigger, Vector3 installLocation, Quaternion installRotation) : base(partSaveInfo, part, partParent, trigger, installLocation, installRotation)
        {
            this.installLocation = installLocation;
        }
        public void SetScrewablePart(ScrewablePart screwablePart)
        {
            this.screwablePart = screwablePart;
        }
        public ScrewablePart GetScrewablePart()
        {
            return this.screwablePart;
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
            if(this.screwablePart != null)
            {
                this.screwablePart.setScrewsOnAssemble();
            }
        }

        protected override void disassemble(bool startup = false)
        {
            // do stuff on dissemble.
            base.disassemble(startup); // if you want dissemble function, you need to call base!
            if (this.screwablePart != null)
            {
                this.screwablePart.resetScrewsOnDisassemble();
            }
        }
        public void removePart()
        {
            disassemble(false);
        }
    }
}
