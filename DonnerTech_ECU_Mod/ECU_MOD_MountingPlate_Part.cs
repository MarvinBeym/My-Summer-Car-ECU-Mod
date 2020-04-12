﻿using UnityEngine;
using ModApi.Attachable;

namespace DonnerTech_ECU_Mod
{
    public class ECU_MOD_MountingPlate_Part : Part
    {
        public ECU_MOD_MountingPlate_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation)
        {

        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo()
        {
            installed = false, //Will make part installed

            position = new Vector3(-17.4197388f, 0f, -3.02854872f), //Sets the spawn location -> where i can be found
            rotation = Quaternion.Euler(-0.4087759f, 0.07467341f, 0.02485323f), // Rotation at spawn location
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
            base.assemble(startUp); // if you want assemble function, you need to call base!
            if (DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part_screwable != null)
            {
                DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part_screwable.setScrewsOnAssemble();
            }
        }

        protected override void disassemble(bool startup = false)
        {
            base.disassemble(startup); // if you want dissemble function, you need to call base!
            if (DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part_screwable != null)
            {
                DonnerTech_ECU_Mod.ecu_mod_mountingPlate_Part_screwable.resetScrewsOnDisassemble();
            }
            base.disassemble(startup);
        }
        public void removePart()
        {
            disassemble(false);
        }
    }
}
