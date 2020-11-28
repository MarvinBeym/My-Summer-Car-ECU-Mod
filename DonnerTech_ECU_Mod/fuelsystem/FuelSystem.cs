using HutongGames.PlayMaker;
using ModApi;
using ModApi.Attachable;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    public class FuelSystem
    {
        public FuelSystemLogic fuel_system_logic { get; set; }
        public ChipProgrammer chip_programmer { get; set; }

        public SimplePart fuel_injector1_part;
        public SimplePart fuel_injector2_part;
        public SimplePart fuel_injector3_part;
        public SimplePart fuel_injector4_part;

        public SimplePart throttle_body1_part;
        public SimplePart throttle_body2_part;
        public SimplePart throttle_body3_part;
        public SimplePart throttle_body4_part;

        public SimplePart fuel_rail_part;
        public SimplePart fuel_pump_cover_part;
        public SimplePart fuel_injection_manifold_part;
        public SimplePart electric_fuel_pump_part;
        public List<SimplePart> allParts = new List<SimplePart>();
        public List<OriginalPart> allOriginalParts = new List<OriginalPart>();

        public FsmFloat distributor_sparkAngle;

        public GameObject fuelStrainer_gameObject;
        public GameObject fuelStrainer_trigger;
        public FsmBool fuelStrainer_installed;
        public FsmBool fuelStrainer_bolted;
        public FsmBool fuelStrainer_detach;

        public GameObject carb_trigger;
        public FsmBool carb_installed;

        public GameObject twinCarb_trigger;
        public FsmBool twinCarb_installed;

        public FsmFloat racingCarb_adjustAverage;
        public FsmFloat racingCarb_tolerance;

        public Drivetrain satsumaDriveTrain;

        private const string orignal_parts_saveFile = "original_parts_saveFile.json";
        private OriginalPartsSave originalPartsSave;

        public List<ChipPart> chip_parts = new List<ChipPart>();

        public bool fuel_injection_manifold_applied = false;

        public Vector3 chip_installLocation = new Vector3(0.008f, 0.001f, -0.058f);
        public Vector3 chip_installRotation = new Vector3(0, 90, -90);
        public DonnerTech_ECU_Mod mod;
        public bool allInstalled
        {
            get
            {
                return allParts.All(c => c.InstalledScrewed() == true);
            }
        }
        public bool anyInstalled
        {
            get
            {
                return allParts.Any(c => c.InstalledScrewed() == true);
            }
        }
        public bool anyOriginalInstalled
        {
            get
            {
                return allOriginalParts.Any(originalPart => originalPart.gameObjectInstalled == true);
            }
        }

        public FuelSystem(DonnerTech_ECU_Mod mod)
        {

            this.mod = mod;

            GameObject satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();

            chip_programmer = new ChipProgrammer(mod, this);

            PlayMakerFSM distributor = GameObject.Find("Distributor").GetComponent<PlayMakerFSM>();

            PlayMakerFSM carb = GameObject.Find("Carburator").GetComponent<PlayMakerFSM>();
            PlayMakerFSM twinCarb = GameObject.Find("Twin Carburators").GetComponent<PlayMakerFSM>();
            PlayMakerFSM raceCarb = GameObject.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
            //PlayMakerFSM fuelStrainer = GameObject.Find("FuelStrainer").GetComponent<PlayMakerFSM>();

            originalPartsSave = Helper.LoadSaveOrReturnNew<OriginalPartsSave>(mod, Helper.CombinePaths(new string[] { ModLoader.GetModConfigFolder(mod), "fuelSystem", orignal_parts_saveFile }));

            allOriginalParts.Add(new OriginalPart("Electrics", "pivot_electrics", GameObject.Find("Electrics"), originalPartsSave.electrics_position, originalPartsSave.electrics_rotation, originalPartsSave.electrics_installed));
            allOriginalParts.Add(new OriginalPart("Distributor", "pivot_distributor", GameObject.Find("Distributor"), originalPartsSave.distributor_position, originalPartsSave.distributor_rotation, originalPartsSave.distributor_installed));
            allOriginalParts.Add(new OriginalPart("Racing Carburators", "pivot_carburator", GameObject.Find("Racing Carburators"), originalPartsSave.racingCarb_position, originalPartsSave.racingCarb_rotation, originalPartsSave.racingCarb_installed));
            allOriginalParts.Add(new OriginalPart("Fuelpump", "pivot_fuel pump", GameObject.Find("Fuelpump"), originalPartsSave.fuelPump_position, originalPartsSave.fuelPump_rotation, originalPartsSave.fuelPump_installed));

            //fuelStrainer_gameObject = GameObject.Find("fuel strainer(Clone)");
            //fuelStrainer_detach = GetDetachFsmBool(fuelStrainer_gameObject);
            //fuelStrainer_trigger = distributor.FsmVariables.FindFsmGameObject("Trigger").Value;
            //fuelStrainer_installed = distributor.FsmVariables.FindFsmBool("Installed");
            //fuelStrainer_bolted = distributor.FsmVariables.FindFsmBool("Bolted");

            distributor_sparkAngle = distributor.FsmVariables.FindFsmFloat("SparkAngle");

            carb_trigger = carb.FsmVariables.FindFsmGameObject("Trigger").Value;
            carb_installed = carb.FsmVariables.FindFsmBool("Installed");

            twinCarb_trigger = twinCarb.FsmVariables.FindFsmGameObject("Trigger").Value;
            twinCarb_installed = twinCarb.FsmVariables.FindFsmBool("Installed");

            racingCarb_adjustAverage = raceCarb.FsmVariables.FindFsmFloat("AdjustAverage");
            racingCarb_tolerance = raceCarb.FsmVariables.FindFsmFloat("Tolerance");


            fuel_injector1_part = mod.fuel_injectors_box.parts[0];
            fuel_injector2_part = mod.fuel_injectors_box.parts[1];
            fuel_injector3_part = mod.fuel_injectors_box.parts[2];
            fuel_injector4_part = mod.fuel_injectors_box.parts[3];

            throttle_body1_part = mod.throttle_bodies_box.parts[0];
            throttle_body2_part = mod.throttle_bodies_box.parts[1];
            throttle_body3_part = mod.throttle_bodies_box.parts[2];
            throttle_body4_part = mod.throttle_bodies_box.parts[3];

            fuel_rail_part = mod.fuel_rail_part;
            fuel_pump_cover_part = mod.fuel_pump_cover_part;
            fuel_injection_manifold_part = mod.fuel_injection_manifold_part;

            electric_fuel_pump_part = mod.electric_fuel_pump_part;

            allParts.Add(fuel_injector1_part);
            allParts.Add(fuel_injector2_part);
            allParts.Add(fuel_injector3_part);
            allParts.Add(fuel_injector4_part);

            allParts.Add(throttle_body1_part);
            allParts.Add(throttle_body2_part);
            allParts.Add(throttle_body3_part);
            allParts.Add(throttle_body4_part);

            allParts.Add(fuel_rail_part);
            allParts.Add(fuel_pump_cover_part);
            allParts.Add(fuel_injection_manifold_part);
            allParts.Add(electric_fuel_pump_part);


            fuel_system_logic = mod.smart_engine_module_part.rigidPart.AddComponent<FuelSystemLogic>();
            fuel_system_logic.Init(this, mod);

            if (anyInstalled)
            {
                foreach (OriginalPart originalPart in allOriginalParts)
                {
                    originalPart.HandleOriginalSave();
                }
            }

            LoadChips();
        }

        public void DisassembleChip()
        {
            fuel_system_logic.fuelMap = null;
        }

        public bool allInstalled_applied = false;
        public void Handle()
        {
            chip_programmer.Handle();

            bool allInstalled_tmp = allInstalled;
            bool anyInstalled_tmp = anyInstalled;
            mod.fuelInjection_allInstalled.Value = allInstalled_tmp;
            mod.fuelInjection_anyInstalled.Value = anyInstalled_tmp;

            if (anyOriginalInstalled && !anyInstalled_tmp)
            {
                foreach (SimplePart part in allParts)
                {
                    part.removePart();
                    part.partTrigger.triggerGameObject.SetActive(false);
                }
                foreach (OriginalPart originalPart in allOriginalParts)
                {
                    originalPart.trigger.SetActive(true);
                }
            }
            
            if(anyInstalled_tmp && !anyOriginalInstalled)
            {
                foreach (SimplePart part in allParts)
                {
                    part.partTrigger.triggerGameObject.SetActive(true);
                }
                foreach (OriginalPart originalPart in allOriginalParts)
                {
                    originalPart.trigger.SetActive(false);
                }
            }

            if(!anyInstalled_tmp && !anyOriginalInstalled)
            {
                foreach (OriginalPart originalPart in allOriginalParts)
                {
                    originalPart.trigger.SetActive(true);
                }
                foreach (SimplePart part in allParts)
                {
                    part.partTrigger.triggerGameObject.SetActive(true);
                }
            }

            if(anyInstalled_tmp)

            if (mod.smart_engine_module_part.InstalledScrewed())
            {
                if (allInstalled_tmp && !allInstalled_applied)
                {
                    mod.wires_injectors_pumps.enabled = true;
                    mod.wires_sparkPlugs1.enabled = true;
                    mod.wires_sparkPlugs2.enabled = true;

                    allInstalled_applied = true;
                    if (chip_parts.Any(c => c.InstalledScrewed() == true))
                    {
                        for (int i = 0; i < chip_parts.Count; i++)
                        {
                            if (!chip_parts[i].InstalledScrewed())
                            {
                                chip_parts[i].partTrigger.triggerGameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < chip_parts.Count; i++)
                        {
                            chip_parts[i].partTrigger.triggerGameObject.SetActive(true);
                        }
                    }


                    if (fuel_system_logic.fuelMap != null)
                    {
                        foreach (OriginalPart originalPart in allOriginalParts)
                        {
                            originalPart.SetFakedInstallStatus(true);
                        }

                    }
                    else
                    {
                        allInstalled_applied = false;
                        foreach (OriginalPart originalPart in allOriginalParts)
                        {
                            originalPart.SetFakedInstallStatus(false);
                        }

                    }
                }
                else if (!allInstalled)
                {
                    allInstalled_applied = false;
                    mod.wires_injectors_pumps.enabled = false;
                    mod.wires_sparkPlugs1.enabled = false;
                    mod.wires_sparkPlugs2.enabled = false;
                }
            }
        }

        public void SaveOriginals()
        {
            try
            {
                OriginalPart fuelPump = allOriginalParts.Find(originalPart => originalPart.partName == "Fuelpump");
                originalPartsSave.fuelPump_installed = fuelPump.gameObjectInstalled;
                originalPartsSave.fuelPump_position = fuelPump.gameObject.transform.position;
                originalPartsSave.fuelPump_rotation = fuelPump.gameObject.transform.rotation;

                OriginalPart racingCarb = allOriginalParts.Find(originalPart => originalPart.partName == "Racing Carburators");
                originalPartsSave.racingCarb_installed = racingCarb.gameObjectInstalled;
                originalPartsSave.racingCarb_position = racingCarb.gameObject.transform.position;
                originalPartsSave.racingCarb_rotation = racingCarb.gameObject.transform.rotation;

                OriginalPart distributor = allOriginalParts.Find(originalPart => originalPart.partName == "Distributor");
                originalPartsSave.distributor_installed = distributor.gameObjectInstalled;
                originalPartsSave.distributor_position = distributor.gameObject.transform.position;
                originalPartsSave.distributor_rotation = distributor.gameObject.transform.rotation;

                OriginalPart electrics = allOriginalParts.Find(originalPart => originalPart.partName == "Electrics");
                originalPartsSave.electrics_installed = electrics.gameObjectInstalled;
                originalPartsSave.electrics_position = electrics.gameObject.transform.position;
                originalPartsSave.electrics_rotation = electrics.gameObject.transform.rotation;

                SaveLoad.SerializeSaveFile<OriginalPartsSave>(mod, originalPartsSave, Helper.CombinePaths(new string[] { ModLoader.GetModConfigFolder(mod), "fuelSystem", orignal_parts_saveFile }));
            }
            catch (Exception ex)
            {
                mod.logger.New("Error while trying to save original parts replaced by fuel injection system", $"path of save file: {Helper.CombinePaths(new string[] { ModLoader.GetModConfigFolder(mod), "fuelSystem", orignal_parts_saveFile })}", ex);
            }

        }

        public void SaveChips()
        {
            chip_parts.ForEach(delegate (ChipPart part)
            {
                SaveChip(part);
            });


        }
        public void LoadChips()
        {
            string fuel_system_savePath = Helper.CreatePathIfNotExists(Helper.CombinePaths(new string[] { ModLoader.GetModConfigFolder(mod), "fuelSystem", "chips" }));

            string[] chip_saveFiles = Directory.GetFiles(fuel_system_savePath, "chip*_saveFile.json", SearchOption.AllDirectories);
            string[] chip_map_saveFiles = Directory.GetFiles(fuel_system_savePath, "chip*.fuelmap", SearchOption.AllDirectories);

            if (chip_saveFiles.Length != chip_map_saveFiles.Length)
            {
                mod.logger.New("Chip part save and map save do not match. Atleast one or more files are missing.", $"save files found: {chip_saveFiles.Length} | chip map files found: {chip_map_saveFiles.Length}");
                return;
            }
            for (int i = 0; i < chip_saveFiles.Length; i++)
            {
                string chip_part_saveFile_fullPath = chip_saveFiles[i];
                string chip_map_saveFile_fullPath = chip_map_saveFiles[i];

                string chip_part_saveFile = Helper.CombinePaths(new string[] { "fuelSystem", "chips", Path.GetFileName(chip_part_saveFile_fullPath) });
                string chip_map_saveFile = Helper.CombinePaths(new string[] { "fuelSystem", "chips", Path.GetFileName(chip_map_saveFile_fullPath) });

                GameObject chip = GameObject.Instantiate(mod.chip);

                Helper.SetObjectNameTagLayer(chip, "Chip" + i);

                ChipSave chipSave = Helper.LoadSaveOrReturnNew<ChipSave>(mod, chip_map_saveFile);

                ChipPart chip_part = new ChipPart(
                    SimplePart.LoadData(mod, "chip" + i, null),
                    chip,
                    mod.smart_engine_module_part.rigidPart,
                    chip_installLocation,
                    new Quaternion { eulerAngles = chip_installRotation }
                );
                chip_part.SetDisassembleFunction(new Action(DisassembleChip));

                chip_part.chipSave = chipSave;

                chip_part.fuelMap_saveFile = chip_map_saveFile;
                chip_part.saveFile = chip_part_saveFile;
                chip_parts.Add(chip_part);
            }
        }

        private void SaveChip(ChipPart part)
        {
            try
            {
                SaveLoad.SerializeSaveFile<ChipSave>(mod, part.chipSave, part.fuelMap_saveFile);
            }
            catch (Exception ex)
            {
                mod.logger.New("Unable to save chips, there was an error while trying to save the chip", $"save file: {part.fuelMap_saveFile}", ex);
            }
        }

        public void Save()
        {
            chip_programmer.Save();
            SaveChips();
            SaveOriginals();
        }
    }
}
