﻿using UnityEngine;
using System.Collections;
using DonnerTech_ECU_Mod;
using ModApi;
using MSCLoader;
using System.Linq;
using DonnerTech_ECU_Mod.parts;
using Parts;
using Tools;

namespace DonnerTech_ECU_Mod
{
    public class BoxLogic : MonoBehaviour
    {
        private Box box;
        private string actionToDisplay;
        private DonnerTech_ECU_Mod mod;
        private SimplePart[] parts;
        private RaycastHit hit;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            RaycastHit hit;
            if (box.spawnedCounter < parts.Length && Camera.main != null && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
            {
                GameObject gameObjectHit;
                gameObjectHit = hit.collider?.gameObject;

                if(gameObjectHit != null && hit.collider && gameObjectHit == this.gameObject)
                {
                    ModClient.guiInteraction = string.Format("Press [{0}] to {1}", cInput.GetText("Use"), actionToDisplay);
                    if (Helper.UseButtonDown)
                    {
                        SimplePart part = parts[box.spawnedCounter];

                        part.activePart.transform.position = hit.point;

                        part.activePart.SetActive(true);
                        box.spawnedCounter++;
                    }
                }
            }
            if(box.spawnedCounter >= parts.Length)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void Init(DonnerTech_ECU_Mod mod, SimplePart[] parts, string actionToDisplay, Box box)
        {
            this.box = box;
            this.mod = mod;
            this.parts = parts;
            this.actionToDisplay = actionToDisplay;
        }

        public void CheckBoxPosReset(bool boughtBox)
        {
            if(boughtBox)
            {
                if(!parts.Any(part => part.installed || part.activePart.activeSelf))
                {
                    this.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
                }
            }
        }
    }
}