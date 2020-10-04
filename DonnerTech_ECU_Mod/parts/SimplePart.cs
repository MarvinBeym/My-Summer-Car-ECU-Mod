using ModApi.Attachable;
using MSCLoader;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace DonnerTech_ECU_Mod
{
    public class SimplePart : Part
    {
        /**
         * returnArray[0] = Mod
         * returnArray[1] = save file string
         * returnArray[2] = Loaded save file. or null if not found/not bought
         */
        protected DonnerTech_ECU_Mod mod;
        protected ScrewablePart screwablePart;
        public Vector3 installLocation;
        public PartSaveInfo partSaveInfo;
        public bool bought;
        private Action disassembleFunction;
        //could be because of static at partSaveInfo
        public string saveFile { get; set; }

        public SimplePart(Object[] loadedData, GameObject part, GameObject partParent, Trigger trigger, Vector3 installLocation, Quaternion installRotation) : base((PartSaveInfo)loadedData[2], part, partParent, trigger, installLocation, installRotation)
        {
            mod = (DonnerTech_ECU_Mod) loadedData[0];
            saveFile = (string) loadedData[1];
            partSaveInfo = (PartSaveInfo)loadedData[2];
            
            this.installLocation = installLocation;
            this.bought = (bool)loadedData[3];

            fixRigidPartNaming();
        }

        public static Object[] LoadData(DonnerTech_ECU_Mod mod, string saveFile, bool bought)
        {
            Object[] loadedData = new Object[4];

            PartSaveInfo partSaveInfo = null;
            if (bought)
            {
                try
                {
                    partSaveInfo = SaveLoad.DeserializeSaveFile<PartSaveInfo>(mod, saveFile);
                }
                catch (Exception ex)
                {
                    //Logger log here
                    partSaveInfo = null;
                }
                
            }
            loadedData[0] = mod;
            loadedData[1] = saveFile;
            loadedData[2] = partSaveInfo;
            loadedData[3] = bought;
            return loadedData;
        }

        public void SetDisassembleFunction(Action action)
        {
            this.disassembleFunction = action;
        }

        public bool InstalledScrewed()
        {
            if (screwablePart != null)
            {
                return (this.installed && screwablePart.partFixed);
            }
            return this.installed;
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

        private void fixRigidPartNaming()
        {
            this.rigidPart.name = this.rigidPart.name.Replace("(Clone)(Clone)", "(Clone)");
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
            if(disassembleFunction != null)
            {
                disassembleFunction.Invoke();
            }
            if (screwablePart != null)
            {
                screwablePart.resetScrewsOnDisassemble();
            }
        }
        public void removePart()
        {
            if (installed)
            {
                disassemble(false);
            }
        }
    }
}
