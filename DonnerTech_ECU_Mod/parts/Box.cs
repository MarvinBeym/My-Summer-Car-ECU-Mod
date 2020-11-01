using ModApi.Attachable;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.parts
{
    public class Box
    {
        private GameObject[] part_gameObjects;
        public SimplePart[] parts;
        private string[] saveFiles;
        public BoxLogic logic;
        public Box(DonnerTech_ECU_Mod mod, GameObject box_gameObject, GameObject part_gameObject, string saveFilePrefix, int numberOfParts, string partName, SimplePart parent, bool bought_box, Vector3[] installLocations, Vector3[] rotations)
        {
            string partNameLowerCase = partName.ToLower();
            part_gameObjects = new GameObject[numberOfParts];
            saveFiles = new string[numberOfParts];
            parts = new SimplePart[numberOfParts];

            for(int i = 0; i < numberOfParts; i++)
            {
                int iOffset = i + 1;
                part_gameObjects[i] = GameObject.Instantiate(part_gameObject);
                SetObjectNameTagLayer(part_gameObjects[i], partName + " " + iOffset + "(Clone)");
                saveFiles[i] = saveFilePrefix + iOffset + "_saveFile.txt";

                parts[i] = new SimplePart(
                    SimplePart.LoadData(mod, saveFiles[i], bought_box),
                    part_gameObjects[i],
                    parent.rigidPart,
                    new Trigger(partNameLowerCase.Replace(" ", "_") + iOffset, parent.rigidPart, installLocations[i], new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    installLocations[i],
                    new Quaternion { eulerAngles = rotations[i] }
                );
            }
            logic = box_gameObject.AddComponent<BoxLogic>();
            logic.Init(mod, parts, "Unpack " + partNameLowerCase);
            foreach(SimplePart part in parts)
            {
                mod.partsList.Add(part);
            }
        }


        private GameObject SetObjectNameTagLayer(GameObject gameObject, string name)
        {
            gameObject.name = name;
            gameObject.tag = "PART";

            gameObject.layer = LayerMask.NameToLayer("Parts");
            return gameObject;
        }

        public void AddScrewable(SortedList<string, Screws> screwListSave, AssetBundle screwableAssetsBundle, Screw[] screws)
        {
            foreach(SimplePart part in parts)
            {
                part.screwablePart = new ScrewablePart(screwListSave, screwableAssetsBundle, part.rigidPart, screws);
            }
        }
    }
}
