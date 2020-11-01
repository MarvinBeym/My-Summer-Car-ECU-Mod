using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    public class OriginalPart
    {
        public string partName;
        public string parentInstalledName;
        public GameObject gameObject;
        private PlayMakerFSM fsm;
        private FsmBool installed;
        private FsmBool bolted;
        private FsmBool detach;
        public GameObject trigger;

        private bool originalInstalled;
        private Vector3 originalPosition;
        private Quaternion originalRotation;


        public OriginalPart(string partName, string parentInstalledName, GameObject fsmGameObject, Vector3 originalPosition, Quaternion originalRotation, bool originalInstalled)
        {
            this.partName = partName;
            fsm = fsmGameObject.GetComponent<PlayMakerFSM>();
            this.parentInstalledName = parentInstalledName;
            this.originalPosition = originalPosition;
            this.originalRotation = originalRotation;
            this.originalInstalled = originalInstalled;

            gameObject = fsm.FsmVariables.FindFsmGameObject("ThisPart").Value;
            trigger = fsm.FsmVariables.FindFsmGameObject("Trigger").Value;
            installed = fsm.FsmVariables.FindFsmBool("Installed");
            bolted = fsm.FsmVariables.FindFsmBool("Bolted");
            detach = GetDetachFsmBool(gameObject);
        }

        public bool gameObjectInstalled
        {
            get
            {
                return (gameObject.transform.parent != null && gameObject.transform.parent.name == parentInstalledName);
            }
        }
        public void SetFakedInstallStatus(bool status)
        {
            installed.Value = status;
            bolted.Value = status;
        }


        private FsmBool GetDetachFsmBool(GameObject gameObject)
        {
            PlayMakerFSM[] playMakerFSMs = gameObject.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM comp in playMakerFSMs)
            {
                if (comp.FsmName == "Removal")
                {
                    return comp.FsmVariables.FindFsmBool("Detach");
                }
            }
            return null;
        }

        internal void HandleOriginalSave()
        {
            detach.Value = true;
            if (!Helper.ApproximatelyVector(originalPosition, Vector3.zero) && !Helper.ApproximatelyQuaternion(originalRotation, Quaternion.identity))
            {
                gameObject.transform.position = originalPosition;
                gameObject.transform.rotation = originalRotation;
            }
        }
    }
}
