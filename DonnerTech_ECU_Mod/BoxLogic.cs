using UnityEngine;
using System.Collections;
using DonnerTech_ECU_Mod;
using ModApi;
using MSCLoader;

namespace DonnerTech_ECU_Mod
{
    public class BoxLogic : MonoBehaviour
    {
        private string actionToDisplay;
        private DonnerTech_ECU_Mod mod;
        private RaycastHit hit;
        private int spawnedCounter = 0;
        private SimplePart[] parts;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Camera.main != null && spawnedCounter < parts.Length)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
                {
                    GameObject gameObjectHit;
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null && hit.collider)
                    {
                        if (gameObjectHit.name == this.gameObject.name)
                        {
                            ModClient.guiInteraction = string.Format("Press [{0}] to {1}", cInput.GetText("Use"), actionToDisplay);
                            if (useButtonDown)
                            {
                                SimplePart part = parts[spawnedCounter];

                                part.activePart.transform.position = hit.point;

                                part.activePart.SetActive(true);
                                spawnedCounter++;
                            }
                        }
                    }
                }
            }
            if(spawnedCounter >= parts.Length)
            {
                this.gameObject.SetActive(false);
            }
        }

        public void Init(DonnerTech_ECU_Mod mod, SimplePart[] parts, string actionToDisplay)
        {
            this.mod = mod;
            this.parts = parts;
            this.actionToDisplay = actionToDisplay;
        }

        private bool useButtonDown
        {
            get { return cInput.GetKeyDown("Use"); }
        }
    }
}