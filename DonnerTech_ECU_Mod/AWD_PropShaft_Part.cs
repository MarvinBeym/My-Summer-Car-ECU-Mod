using UnityEngine;
using ModApi.Attachable;

namespace DonnerTech_ECU_Mod
{
    public class AWD_PropShaft_Part : Part
    {
        public AWD_PropShaft_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation)
        {

        }

        public override PartSaveInfo defaultPartSaveInfo => new PartSaveInfo()
        {
            installed = false, //Will make part installed

            position = DonnerTech_ECU_Mod.awd_propshaft_part_spawnLocation, //Sets the spawn location -> where i can be found
            rotation = Quaternion.Euler(0, 0, 0), // Rotation at spawn location
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
            // do stuff on dissemble.
            base.disassemble(startup); // if you want dissemble function, you need to call base!
        }
        public void removePart()
        {
            disassemble(false);
        }
    }
}
