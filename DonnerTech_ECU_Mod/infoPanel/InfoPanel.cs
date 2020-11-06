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
        public Keybind arrowUp = new Keybind("info_panel_arrowUp", "Arrow Up", KeyCode.Keypad8);
        public Keybind arrowDown = new Keybind("info_panel_arrowDown", "Arrow Down", KeyCode.Keypad2);
        public Keybind circle = new Keybind("info_panel_circle", "Circle", KeyCode.KeypadEnter);
        public Keybind cross = new Keybind("info_panel_cross", "Cross", KeyCode.KeypadPeriod);
        public Keybind plus = new Keybind("info_panel_plus", "Plus", KeyCode.KeypadPlus);
        public Keybind minus = new Keybind("info_panel_minus", "Minus", KeyCode.KeypadMinus);
        private const string saveFile = "info_panel_saveFile.txt";
        public Vector3 installLocation = new Vector3(0.25f, -0.088f, -0.01f);
        private bool workaroundChildDisableDone = false;
        public SimplePart part { get; set; }
        public InfoPanel_Logic logic { get; set; }

        public InfoPanel(DonnerTech_ECU_Mod mod, SortedList<String, Screws> screwListSave)
        {
            this.mod = mod;

            gameObject = (mod.assetBundle.LoadAsset("info-panel.prefab") as GameObject);
            Helper.SetObjectNameTagLayer(gameObject, "DonnerTech Info Panel");

            Keybind.AddHeader(mod, "ECU-Panel Keybinds");
            Keybind.Add(mod, arrowUp);
            Keybind.Add(mod, arrowDown);
            Keybind.Add(mod, circle);
            Keybind.Add(mod, cross);
            Keybind.Add(mod, plus);
            Keybind.Add(mod, minus);

            part = new SimplePart(
                SimplePart.LoadData(mod, saveFile, mod.partBuySave.boughtInfoPanel),
                gameObject,
                GameObject.Find("dashboard(Clone)"),
                new Trigger("info_panel_trigger", GameObject.Find("dashboard(Clone)"), installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                installLocation,
                new Quaternion { eulerAngles = new Vector3(0, 180, 180) }
            );
            mod.partsList.Add(part);

            part.screwablePart = new ScrewablePart(screwListSave, mod.screwableAssetsBundle, part.rigidPart,
            new Screw[] {
                new Screw(new Vector3(0f, -0.025f, -0.067f), new Vector3(180, 0, 0), 0.8f, 8),
            });

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
