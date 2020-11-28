using HutongGames.PlayMaker;
using ModApi.Attachable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace DonnerTech_ECU_Mod.parts
{
    public class ReplacedPart : SimplePart
    {

        public GameObject[] originalConditions;
        public SimplePart[] partConditions; //SimpleParts installed state to check before setting originalInstalled and such to true
        public GameObject originalTrigger; //The original trigger to prevent part from beeing installed if replacement is installed and vice versa.
        public ReplacedPartLogic replacedPartLogicActive;
        public ReplacedPartLogic replacedPartLogicRigid;

        public FsmBool originalInstalled;
        public FsmBool originalBolted;
        public FsmBool originalDetach;
        public GameObject logicGameObject; //The logic (where install bolted and such is set
        public GameObject gameObject; //The movable 3d object

        public ReplacedPart(List<Object> loadedData, GameObject part, GameObject partParent, Vector3 installLocation, Quaternion installRotation, GameObject logicGameObject) : base(loadedData, part, partParent, installLocation, installRotation)
        {
            this.logicGameObject = logicGameObject;
            PlayMakerFSM playMakerFSM = logicGameObject.GetComponent<PlayMakerFSM>();
            gameObject = playMakerFSM.FsmVariables.FindFsmGameObject("ThisPart").Value;
            originalTrigger = playMakerFSM.FsmVariables.FindFsmGameObject("Trigger").Value;

            originalInstalled = playMakerFSM.FsmVariables.FindFsmBool("Installed");
            originalBolted = playMakerFSM.FsmVariables.FindFsmBool("Bolted");


            PlayMakerFSM[] comps = gameObject.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM comp in comps)
            {
                if (comp.FsmName == "Removal")
                {
                    originalDetach = comp.FsmVariables.FindFsmBool("Detach");
                    break;
                }
            }
            replacedPartLogicActive = this.activePart.AddComponent<ReplacedPartLogic>();
            replacedPartLogicActive.Init(this);

            replacedPartLogicRigid = this.rigidPart.AddComponent<ReplacedPartLogic>();
            replacedPartLogicRigid.Init(this);

            /*
            if (part.installed)
            {
                originalDetach.Value = true;
                if (!Helper.ApproximatelyVector(originalPartsSave.racingCarb_position, Vector3.zero) && !Helper.ApproximatelyQuaternion(originalPartsSave.racingCarb_rotation, Quaternion.identity))
                {
                    racingCarb_gameObject.transform.position = originalPartsSave.racingCarb_position;
                    racingCarb_gameObject.transform.rotation = originalPartsSave.racingCarb_rotation;
                }

            }
            */
        }

        public void SetConditions(SimplePart[] partConditions)
        {
            this.partConditions = partConditions;
        }
        public void SetOriginalConditions(GameObject[] originalConditions)
        {
            this.originalConditions = originalConditions;
        }

        private bool conditionsMet
        {
            get
            {
                if(partConditions == null || partConditions.Length == 0)
                {
                    return true;
                }

                return !partConditions.Any(p => !p.InstalledScrewed()) && InstalledScrewed();
            }
        }
        private bool originalConditionsMet
        {
            get
            {
                if (originalConditions == null || originalConditions.Length == 0)
                {
                    return true;
                }
                return !originalConditions.Any(op => !(op.transform.parent != null));
            }
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
