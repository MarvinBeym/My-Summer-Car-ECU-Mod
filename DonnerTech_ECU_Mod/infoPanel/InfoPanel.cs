using ModApi.Attachable;
using MSCLoader;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.infoPanel
{
    public class InfoPanel
    {
        private GameObject gameObject;
        private DonnerTech_ECU_Mod mod;
        
        private bool workaroundChildDisableDone = false;
        public SimplePart part { get; set; }
        public InfoPanel_Logic logic { get; set; }

        public InfoPanel(DonnerTech_ECU_Mod mod, SimplePart part)
        {
            this.mod = mod;
            this.part = part;

            TextMesh[] textMeshes = part.activePart.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in textMeshes)
            {
                textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            SpriteRenderer[] spriteRenderers = part.activePart.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                spriteRenderer.enabled = false;
            }
            if (spriteRenderers.Length > 0 && textMeshes.Length > 0)
            {
                workaroundChildDisableDone = true;
            }


            logic = part.rigidPart.AddComponent<InfoPanel_Logic>();
            logic.Init(this, mod);

            UnityEngine.Object.Destroy(gameObject);
        }

        public void Handle()
        {
            if (!part.installed)
            {
                if (part.activePart.transform.localScale.x < 1.5f)
                {
                    part.activePart.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                }
                if (!workaroundChildDisableDone)
                {
                    TextMesh[] textMeshes = part.activePart.GetComponentsInChildren<TextMesh>();
                    foreach (TextMesh textMesh in textMeshes)
                    {
                        textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
                    }


                    SpriteRenderer[] spriteRenderers = part.activePart.GetComponentsInChildren<SpriteRenderer>();
                    foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                    {
                        spriteRenderer.enabled = false;
                    }
                    if (spriteRenderers.Length > 0 && textMeshes.Length > 0)
                    {
                        workaroundChildDisableDone = true;
                    }
                }
            }
        }
    }
}
