
using HutongGames.PlayMaker;
using ModApi;
using ModApi.Attachable;
using MSCLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    public class FuelSystem
    {
        public FuelSystemLogic fuel_system_logic { get; set; }
        public Canvas programmer_ui { get; set; }
        private GameObject programmer_ui_gameObject { get; set; }
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
        public List<SimplePart> allParts = new List<SimplePart>();

        public GameObject carb_trigger;
        public FsmBool carb_installed;

        public GameObject twinCarb_trigger;
        public FsmBool twinCarb_installed;

        public GameObject racingCarb_trigger;
        public FsmBool racingCarb_installed;
        public FsmBool racingCarb_bolted;
        public FsmFloat racingCarb_adjust1;
        public FsmFloat racingCarb_adjust2;
        public FsmFloat racingCarb_adjust3;
        public FsmFloat racingCarb_adjust4;
        public FsmFloat racingCarb_adjustAverage;
        public FsmFloat racingCarb_adjustMax;
        public FsmFloat racingCarb_adjustMin;
        public FsmFloat racingCarb_differenceMax;
        public FsmFloat racingCarb_differenceMin;
        public FsmFloat racingCarb_idealSetting;
        public FsmFloat racingCarb_max;
        public FsmFloat racingCarb_min;
        public FsmFloat racingCarb_tolerance;

        public GameObject pump_trigger;
        public FsmBool pump_installed;
        public FsmBool pump_bolted;

        public FsmBool pump_detach;
        public FsmBool racingCarb_detach;

        public Drivetrain satsumaDriveTrain;
        public AxisCarController axisCarController;

        public InputField[,] inputFieldMap = new InputField[14, 17];
        
        public List<string> chip_errors = new List<string>();
        public List<ChipPart> chip_parts = new List<ChipPart>();

        private RaycastHit hit;

        private GameObject chip_programmer_chip;
        private FsmGameObject itemPivot;
        private bool fuel_injection_manifold_applied = false;

        public Vector3 chip_installLocation = new Vector3(0, 0, 0);

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
                return allParts.Any(c => c.installed == true);
            }
        }

        public FuelSystem(DonnerTech_ECU_Mod mod)
        {

            this.mod = mod;
            itemPivot = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("ItemPivot");
            GameObject satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            axisCarController = satsuma.GetComponent<AxisCarController>();
            axisCarController.smoothInput = true;//Have to set to get smooth throttle value
            PlayMakerFSM carb = GameObject.Find("Carburator").GetComponent<PlayMakerFSM>();
            PlayMakerFSM twinCarb = GameObject.Find("Twin Carburators").GetComponent<PlayMakerFSM>();
            PlayMakerFSM race_carb = GameObject.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
            PlayMakerFSM pump = GameObject.Find("Fuelpump").GetComponent<PlayMakerFSM>();

            PlayMakerFSM[] comps = GameObject.Find("fuel pump(Clone)").GetComponents<PlayMakerFSM>();
            foreach(PlayMakerFSM comp in comps)
            {
                if(comp.FsmName == "Removal")
                {
                    pump_detach = comp.FsmVariables.FindFsmBool("Detach");
                }
            }

            comps = GameObject.Find("racing carburators(Clone)").GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM comp in comps)
            {
                if (comp.FsmName == "Removal")
                {
                    racingCarb_detach = comp.FsmVariables.FindFsmBool("Detach");
                }
            }


            programmer_ui_gameObject = GameObject.Instantiate((mod.assetBundle.LoadAsset("ui_interface.prefab") as GameObject));
            programmer_ui_gameObject.name = "FuelSystem_Programmer_UI_GameObject";
            programmer_ui = programmer_ui_gameObject.GetComponent<Canvas>();
            programmer_ui.name = "FuelSystem_Programmer_UI";
            programmer_ui.enabled = false;

            GameObject[] childs = new GameObject[programmer_ui.transform.childCount];


            GameObject btn_test = new GameObject();
            Button btn_writeChip = programmer_ui.transform.FindChild("btn_writeChip").gameObject.GetComponent<Button>();
            Button btn_resetMap = programmer_ui.transform.FindChild("btn_resetMap").gameObject.GetComponent<Button>();
            btn_writeChip.onClick.AddListener(delegate ()
            {
                chip_errors.Clear();

                /*
                float counter = 10f;
                for (int y = 0; y < 14; y++)
                {
                    for (int x = 0; x < 17; x++)
                    {
                        counter = (10 + 1 + y) + (0.05f * x);
                        try
                        {
                            
                            fuelMap[y, x] = Convert.ToSingle(counter);
                            inputFieldMap[y, x].text = counter.ToString("00.0");
                        }
                        catch
                        {
                            //Write to logger
                        }
                    }
                }
                */
                float[,] fuelMap = new float[14, 17];
                
                for (int y = 0; y < 14; y++)
                {
                    for (int x = 0; x < 17; x++)
                    {
                        try
                        {
                            if (inputFieldMap[y, x].text == "")
                            {
                                chip_errors.Add(String.Format("Value in column {0}, row {1} is invalid", y, x));
                                //Add Error message shown to user
                            }
                            fuelMap[y, x] = Convert.ToSingle(inputFieldMap[y, x].text);
                        }
                        catch
                        {
                            //Write to logger
                        }
                    }
                }

                if (chip_errors.Count == 0)
                {
                    for(int index = 0; index < chip_parts.Count; index++)
                    {
                        ChipPart part = chip_parts[index];
                        if (part.chipInstalledOnProgrammer)
                        {
                            part.activePart.SetActive(true);
                            chip_programmer_chip.SetActive(false);
                            chipInstalledOnProgrammer = false;
                            chipOnProgrammer = null;
                            part.chipInstalledOnProgrammer = false;
                            part.chipSave.map = fuelMap;
                            part.chipSave.chipProgrammed = true;

                            Vector3 chip_programmer_position = mod.chip_programmer_part.activePart.transform.position;
                            part.activePart.transform.position = new Vector3(chip_programmer_position.x, chip_programmer_position.y + 0.05f, chip_programmer_position.z);
                            break;
                        }
                    }
                    SaveChips();
                }
                else
                {
                    //Logger?
                    //Display error message stuff
                }
                
            });
            btn_resetMap.onClick.AddListener(delegate ()
            {
                for (int y = 0; y < 14; y++)
                {
                    for (int x = 0; x < 17; x++)
                    {
                        try
                        {
                            inputFieldMap[y, x].text = "10.0";
                        }
                        catch
                        {
                            //Write to logger
                        }
                    }
                }
            });

            inputFieldMap = new InputField[14, 17];

            GameObject table = programmer_ui.transform.FindChild("table").gameObject;

            InputField[] inputFields = new InputField[238];

            for (int i = 0; i < inputFields.Length; i++)
            {
                inputFields[i] = table.transform.FindChild("input-" + (i + 1)).GetComponent<InputField>();
            }

            for (int y = 0; y < 14; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    int index = ((y * 17) + x);
                    FloatForce floatForce = inputFields[index].gameObject.AddComponent<FloatForce>();
                    floatForce.inputField = inputFields[index];
                    inputFieldMap[y, x] = inputFields[index];
                }
            }


            pump_trigger = pump.FsmVariables.FindFsmGameObject("Trigger").Value;
            pump_installed = pump.FsmVariables.FindFsmBool("Installed");
            pump_bolted = pump.FsmVariables.FindFsmBool("Bolted");

            carb_trigger = carb.FsmVariables.FindFsmGameObject("Trigger").Value;
            carb_installed = carb.FsmVariables.FindFsmBool("Installed");

            twinCarb_trigger = twinCarb.FsmVariables.FindFsmGameObject("Trigger").Value;
            twinCarb_installed = twinCarb.FsmVariables.FindFsmBool("Installed");

            racingCarb_trigger = race_carb.FsmVariables.FindFsmGameObject("Trigger").Value;
            racingCarb_installed = race_carb.FsmVariables.FindFsmBool("Installed");
            racingCarb_bolted = race_carb.FsmVariables.FindFsmBool("Bolted");
            racingCarb_adjust1 = race_carb.FsmVariables.FindFsmFloat("Adjust1");
            racingCarb_adjust2 = race_carb.FsmVariables.FindFsmFloat("Adjust2");
            racingCarb_adjust3 = race_carb.FsmVariables.FindFsmFloat("Adjust3");
            racingCarb_adjust4 = race_carb.FsmVariables.FindFsmFloat("Adjust4");
            racingCarb_adjustAverage = race_carb.FsmVariables.FindFsmFloat("AdjustAverage");
            racingCarb_adjustMax = race_carb.FsmVariables.FindFsmFloat("AdjustMax");
            racingCarb_adjustMin = race_carb.FsmVariables.FindFsmFloat("AdjustMin");
            racingCarb_differenceMax = race_carb.FsmVariables.FindFsmFloat("DifferenceMax");
            racingCarb_differenceMin = race_carb.FsmVariables.FindFsmFloat("DifferenceMin");
            racingCarb_idealSetting = race_carb.FsmVariables.FindFsmFloat("IdealSetting");
            racingCarb_max = race_carb.FsmVariables.FindFsmFloat("Max");
            racingCarb_min = race_carb.FsmVariables.FindFsmFloat("Min");
            racingCarb_tolerance = race_carb.FsmVariables.FindFsmFloat("Tolerance");


            fuel_injector1_part = mod.fuel_injector1_part;
            fuel_injector2_part = mod.fuel_injector2_part;
            fuel_injector3_part = mod.fuel_injector3_part;
            fuel_injector4_part = mod.fuel_injector4_part;

            throttle_body1_part = mod.throttle_body1_part;
            throttle_body2_part = mod.throttle_body2_part;
            throttle_body3_part = mod.throttle_body3_part;
            throttle_body4_part = mod.throttle_body4_part;

            fuel_rail_part = mod.fuel_rail_part;
            fuel_pump_cover_part = mod.fuel_pump_cover_part;
            fuel_injection_manifold_part = mod.fuel_injection_manifold_part;

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

            chip_programmer_chip = mod.chip_programmer_part.activePart.transform.FindChild("rigid_chip").gameObject;
            chip_programmer_chip.SetActive(false);

            fuel_system_logic = mod.smart_engine_module_part.rigidPart.AddComponent<FuelSystemLogic>();
            fuel_system_logic.Init(this);

            if (fuel_injection_manifold_part.installed)
            {
                racingCarb_detach.Value = true;
            }
            if (fuel_pump_cover_part.installed)
            {
                pump_detach.Value = true;
            }
            LoadChips();
#if DEBUG
            /*
            Report modSettings_report = new Report();
            modSettings_report.name = "Mod Settings";
            modSettings_report.files = Directory.GetFiles(ModLoader.GetModConfigFolder(mod));
            
            Report modLoaderOutputLog_report = new Report();
            modLoaderOutputLog_report.name = "ModLoader Output";
            modLoaderOutputLog_report.files = new string[] { Helper.CombinePaths(new string[] { Path.GetFullPath("."), "mysummercar_Data", "output_log.txt" }) };

            Report gameSave_report = new Report();
            gameSave_report.name = "MSC Savegame";
            gameSave_report.files = Directory.GetFiles(Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\LocalLow\Amistech\My Summer Car\")));
            Reporter.Reporter.GenerateReport(mod, new Report[] { modSettings_report, modLoaderOutputLog_report, gameSave_report });
            */
#endif
        }

        public void DisassembleChip()
        {
            fuel_system_logic.fuelMap = null;
        }

        public void Handle()
        {
            HandleProgrammer();

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

            if (carb_installed.Value || twinCarb_installed.Value || (racingCarb_installed.Value && !fuel_injection_manifold_part.installed))
            {
                if (fuel_injection_manifold_part.installed)
                {
                    fuel_injection_manifold_part.removePart();
                }
                fuel_injection_manifold_part.partTrigger.triggerGameObject.SetActive(false);
            }
            else
            {
                fuel_injection_manifold_part.partTrigger.triggerGameObject.SetActive(true);
            }

            if (fuel_injection_manifold_part.InstalledScrewed())
            {
                carb_trigger.SetActive(false);
                twinCarb_trigger.SetActive(false);
                racingCarb_trigger.SetActive(false);
            }
            else
            {
                carb_trigger.SetActive(true);
                twinCarb_trigger.SetActive(true);
                racingCarb_trigger.SetActive(true);
            }

            fuel_pump_cover_part.partTrigger.triggerGameObject.SetActive(!pump_installed.Value);

            pump_trigger.SetActive(!fuel_pump_cover_part.InstalledScrewed());

            if (fuel_pump_cover_part.installed)
            {
                pump_bolted.Value = true;
                pump_installed.Value = true;
            }
            

            if (fuel_injector1_part.InstalledScrewed() && fuel_injector2_part.InstalledScrewed() && fuel_injector3_part.InstalledScrewed() && fuel_injector4_part.InstalledScrewed())
            {
                fuel_rail_part.partTrigger.triggerGameObject.SetActive(true);
            }
            else
            {
                if (fuel_rail_part.InstalledScrewed())
                {
                    fuel_rail_part.removePart();
                }
                fuel_rail_part.partTrigger.triggerGameObject.SetActive(false);
            }

            if (anyInstalled)
            {
                if (allInstalled && fuel_system_logic.fuelMap != null)
                {
                    if (!fuel_injection_manifold_applied && !racingCarb_installed.Value)
                    {
                        racingCarb_installed.Value = true;
                        racingCarb_bolted.Value = true;
                        fuel_injection_manifold_applied = true;
                    }
                }
                else
                {
                    if (fuel_injection_manifold_applied || fuel_system_logic.fuelMap == null || !mod.smart_engine_module_part.InstalledScrewed())
                    {
                        racingCarb_installed.Value = false;
                        racingCarb_bolted.Value = false;
                        fuel_injection_manifold_applied = false;
                    }
                }
            }
        }

        private bool chipInstalledOnProgrammer = false;
        private ChipPart chipOnProgrammer = null;
        private void HandleProgrammer()
        {
            if (!chipInstalledOnProgrammer)
            {
                if (itemPivot.Value != null && itemPivot.Value.transform.childCount > 0)
                {
                    GameObject itemInHand = itemPivot.Value.transform.GetChild(0).gameObject;
                    if (itemInHand.name.StartsWith("Chip") && itemInHand.name != mod.chip_programmer_part.activePart.name)
                    {
                        if (Vector3.Distance(mod.chip_programmer_part.activePart.transform.position, itemInHand.transform.position) <= 0.075f)
                        {
                            ModClient.guiInteract("insert chip", GuiInteractSymbolEnum.Assemble);
                            if (mod.leftMouseDown)
                            {
                                for(int index = 0; index < chip_parts.Count; index++)
                                {
                                    ChipPart part = chip_parts[index];
                                    if (part.activePart.name == itemInHand.name)
                                    {
                                        part.activePart.SetActive(false);
                                        chip_programmer_chip.SetActive(true);
                                        chipInstalledOnProgrammer = true;
                                        part.chipInstalledOnProgrammer = true;

                                        chipOnProgrammer = part;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }
            }

            if (chipInstalledOnProgrammer && Camera.main != null && chipOnProgrammer != null)
            {
                
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f, 1 << LayerMask.NameToLayer("Parts")) != false)
                {
                    GameObject gameObjectHit;
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null && hit.collider)
                    {
                        if (gameObjectHit.name == mod.chip_programmer_part.activePart.name)
                        {
                            string guiText = String.Format(
                                "Press [{0}] to {1}\n" +
                                "Press [RIGHT MOUSE] to {2}",
                                cInput.GetText("Use"), "open programmer", "remove chip"
                            );
                            ModClient.guiInteraction = guiText;
                            if (mod.rightMouseDown)
                            {
                                for (int index = 0; index < chip_parts.Count; index++)
                                {
                                    ChipPart part = chip_parts[index];
                                    if (part.chipInstalledOnProgrammer)
                                    {
                                        programmer_ui.enabled = false;
                                        part.activePart.SetActive(true);
                                        chip_programmer_chip.SetActive(false);
                                        chipInstalledOnProgrammer = false;
                                        part.chipInstalledOnProgrammer = false;
                                        
                                        chipOnProgrammer = null;
                                        Vector3 chip_programmer_position = mod.chip_programmer_part.activePart.transform.position;
                                        part.activePart.transform.position = new Vector3(chip_programmer_position.x, chip_programmer_position.y + 0.05f, chip_programmer_position.z);
                                        break;
                                    }
                                }
                            }
                            else if (mod.useButtonDown)
                            {
                                for (int y = 0; y < inputFieldMap.GetLength(0); y++)
                                {
                                    for (int x = 0; x < inputFieldMap.GetLength(1); x++)
                                    {
                                        try
                                        {
                                            if(chipOnProgrammer.chipSave.map == null)
                                            {
                                                inputFieldMap[y, x].text = "";
                                            }
                                            else
                                            {
                                                inputFieldMap[y, x].text = chipOnProgrammer.chipSave.map[y, x].ToString("00.0");
                                            }
                                            
                                        }
                                        catch
                                        {
                                            //Write to logger
                                        }
                                    }
                                }
                                programmer_ui.enabled = true;
                            }
                        }
                    }
                }
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

            string[] chip_saveFiles = Directory.GetFiles(fuel_system_savePath, "chip*_saveFile.txt", SearchOption.AllDirectories);
            string[] chip_map_saveFiles = Directory.GetFiles(fuel_system_savePath, "chip*.fuelmap", SearchOption.AllDirectories);
            
            if(chip_saveFiles.Length != chip_map_saveFiles.Length)
            {
                //Logger
                return;
            }
            for(int i = 0; i < chip_saveFiles.Length; i++)
            {
                string chip_part_saveFile_fullPath = chip_saveFiles[i];
                string chip_map_saveFile_fullPath = chip_map_saveFiles[i];

                string chip_part_saveFile = Helper.CombinePaths(new string[] { "fuelSystem", "chips", Path.GetFileName(chip_part_saveFile_fullPath) });
                string chip_map_saveFile = Helper.CombinePaths(new string[] { "fuelSystem", "chips", Path.GetFileName(chip_map_saveFile_fullPath) });

                GameObject chip = GameObject.Instantiate(mod.chip);

                mod.SetObjectNameTagLayer(chip, "Chip" + i);

                ChipSave chipSave = SaveLoad.DeserializeSaveFile<ChipSave>(mod, chip_map_saveFile);
                if(chipSave == null)
                {
                    chipSave = new ChipSave();
                }

                ChipPart chip_part = new ChipPart(
                    SimplePart.LoadData(mod, chip_part_saveFile, true),
                    chip,
                    mod.smart_engine_module_part.rigidPart,
                    new Trigger("chip" + i, mod.smart_engine_module_part.rigidPart, chip_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    chip_installLocation,
                    new Quaternion { eulerAngles = new Vector3(0, 0, 0) }
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
                //Logger
            }
        }
    }
}
