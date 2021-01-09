﻿using ModApi;
using MSCLoader;
using Parts;
using System;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class KitLogic : MonoBehaviour
    {
        private DonnerTech_ECU_Mod mod;
        private Kit kit;
        private RaycastHit hit;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Camera.main != null && kit.spawnedCounter < kit.parts.Length && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
            {
                GameObject gameObjectHit;
                gameObjectHit = hit.collider?.gameObject;
                if (gameObjectHit != null && hit.collider)
                {
                    if (gameObjectHit.name == this.gameObject.name)
                    {
                        ModClient.guiInteraction = string.Format("Press [{0}] to {1}", cInput.GetText("Use"), "Unpack " + kit.parts[kit.spawnedCounter].activePart.name.Replace("(Clone)", ""));
                        if (Helper.UseButtonDown)
                        {
                            SimplePart part = kit.parts[kit.spawnedCounter];

                            part.activePart.transform.position = hit.point;

                            part.activePart.SetActive(true);
                            kit.spawnedCounter++;
                        }
                    }
                }
            }
            if (kit.spawnedCounter >= kit.parts.Length)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void Init(DonnerTech_ECU_Mod mod, Kit kit)
        {
            this.mod = mod;
            this.kit = kit;
        }
    }
}