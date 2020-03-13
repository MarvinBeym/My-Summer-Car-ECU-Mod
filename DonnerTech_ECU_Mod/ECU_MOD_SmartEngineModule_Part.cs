using UnityEngine;
using ModApi.Attachable;

namespace DonnerTech_ECU_Mod
{
    public class ECU_MOD_SmartEngineModule_Part : Part
    {
        public ECU_MOD_SmartEngineModule_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation)
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
            // do stuff on assemble.
            base.assemble(startUp); // if you want assemble function, you need to call base!
        }

        protected override void disassemble(bool startup = false)
        {
            if (DonnerTech_ECU_Mod.alsModuleEnabled)
            {
                DonnerTech_ECU_Mod.ToggleALS();
            }
            if (DonnerTech_ECU_Mod.stage2revModuleEnabled)
            {
                DonnerTech_ECU_Mod.ToggleStage2Rev();
            }
            // do stuff on dissemble.
            base.disassemble(startup); // if you want dissemble function, you need to call base!
        }
        public void removePart()
        {
            disassemble(false);
        }
    }
}
