using UnityEngine;
using ModApi.Attachable;

namespace DonnerTech_ECU_Mod
{
    public class ECU_Airride_FL_Part : Part
    {
        public ECU_Airride_FL_Part(PartSaveInfo inPartSaveInfo, GameObject inPart, GameObject inParent, Trigger inPartTrigger, Vector3 inPartPosition, Quaternion inPartRotation) : base(inPartSaveInfo, inPart, inParent, inPartTrigger, inPartPosition, inPartRotation)
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