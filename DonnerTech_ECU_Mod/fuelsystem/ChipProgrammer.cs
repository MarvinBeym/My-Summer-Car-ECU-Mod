using HutongGames.PlayMaker;
using ModApi;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    public class ChipProgrammer
    {

        public DonnerTech_ECU_Mod mod;
        public FuelSystem fuelSystem;
        public Canvas programmer_ui { get; set; }
        private GameObject programmer_ui_gameObject { get; set; }

        private Toggle cb_startAssistEnabled;
        private Text txtField_error;

        public InputField[,] inputFieldMap = new InputField[14, 17];

        public List<string> chip_errors = new List<string>();

        private GameObject chip_programmer_chip;
        private FsmGameObject itemPivot;
        private bool startAssistEnabled = false;

        private InputField input_sparkAngle;
        private bool chipInstalledOnProgrammer = false;
        private ChipPart chipOnProgrammer = null;

        private RaycastHit hit;

        public Keybind programmer_ui_open = new Keybind("programmer_ui_open", "Open/Close", KeyCode.Keypad0);

        public ChipProgrammer(DonnerTech_ECU_Mod mod, FuelSystem fuelSystem)
        {
            this.mod = mod;
            Keybind.Add(mod, programmer_ui_open);

            this.fuelSystem = fuelSystem;
            itemPivot = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("ItemPivot");

            programmer_ui_gameObject = GameObject.Instantiate((mod.assetBundle.LoadAsset("ui_interface.prefab") as GameObject));
            programmer_ui_gameObject.name = "FuelSystem_Programmer_UI_GameObject";
            programmer_ui = programmer_ui_gameObject.GetComponent<Canvas>();
            programmer_ui.name = "FuelSystem_Programmer_UI";

            Transform panel = programmer_ui.transform.FindChild("panel");
            Button btn_writeChip = panel.FindChild("btn_writeChip").gameObject.GetComponent<Button>();
            Button btn_resetMap = panel.FindChild("btn_resetMap").gameObject.GetComponent<Button>();
            Button btn_closeUi = panel.FindChild("btn_closeUI").gameObject.GetComponent<Button>();
            Button btn_ignitionPlus = panel.FindChild("btn_ignitionPlus").gameObject.GetComponent<Button>();
            Button btn_ignitionMinus = panel.FindChild("btn_ignitionMinus").gameObject.GetComponent<Button>();
            
            input_sparkAngle = panel.FindChild("input_sparkAngle").gameObject.GetComponent<InputField>();
            SparkAngleInputForce sparkAngleInputForce = input_sparkAngle.gameObject.AddComponent<SparkAngleInputForce>();
            
            sparkAngleInputForce.inputField = input_sparkAngle;
            cb_startAssistEnabled = panel.FindChild("cb_startAssistEnabled").gameObject.GetComponent<Toggle>();
            txtField_error = panel.FindChild("errors_displaying").FindChild("txtField_error").gameObject.GetComponent<Text>();
            
            SetProgrammerUIStatus(false);

            btn_closeUi.onClick.AddListener(delegate ()
            {
                SetProgrammerUIStatus(false);
            });

            btn_ignitionPlus.onClick.AddListener(BtnIgnitionPlus);
            btn_ignitionMinus.onClick.AddListener(BtnIgnitionMinus);

            cb_startAssistEnabled.onValueChanged.AddListener(delegate (bool newState)
            {
                startAssistEnabled = !startAssistEnabled;
            });
            btn_writeChip.onClick.AddListener(BtnWriteChip);
            btn_resetMap.onClick.AddListener(BtnResetMap);

            inputFieldMap = new InputField[14, 17];

            GameObject table = programmer_ui.transform.FindChild("panel").FindChild("table").gameObject;

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

            chip_programmer_chip = mod.chip_programmer_part.activePart.transform.FindChild("rigid_chip").gameObject;
            chip_programmer_chip.SetActive(false);
        }


        private void BtnWriteChip()
        {
            chip_errors.Clear();
            txtField_error.text = "";
            float[,] fuelMap = new float[14, 17];

            for (int y = 0; y < 14; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    try
                    {
                        if (inputFieldMap[y, x].text == "")
                        {
                            string error = String.Format("Value in column {0}, row {1} is invalid", y, x);
                            chip_errors.Add(error);
                            txtField_error.text += error + "\n";
                        }
                        fuelMap[y, x] = Convert.ToSingle(inputFieldMap[y, x].text);
                    }
                    catch
                    {
                        mod.logger.New("Error while trying to write chip map");
                    }
                }
            }

            if (chip_errors.Count == 0)
            {
                for (int index = 0; index < fuelSystem.chip_parts.Count; index++)
                {
                    ChipPart part = fuelSystem.chip_parts[index];
                    if (part.chipInstalledOnProgrammer)
                    {
                        part.activePart.SetActive(true);
                        chip_programmer_chip.SetActive(false);
                        chipInstalledOnProgrammer = false;
                        chipOnProgrammer = null;
                        part.chipInstalledOnProgrammer = false;
                        part.chipSave.map = fuelMap;
                        part.chipSave.chipProgrammed = true;
                        part.chipSave.startAssistEnabled = startAssistEnabled;
                        try
                        {
                            part.chipSave.sparkAngle = Convert.ToSingle(input_sparkAngle.text);
                        }
                        catch (Exception e)
                        {
                            mod.logger.New("Error while trying to save spark timing after chip programming", "input field value: " + input_sparkAngle.text, e);
                            part.chipSave.sparkAngle = 14.5f;
                        }


                        SetProgrammerUIStatus(false);
                        Vector3 chip_programmer_position = mod.chip_programmer_part.activePart.transform.position;
                        part.activePart.transform.position = new Vector3(chip_programmer_position.x, chip_programmer_position.y + 0.05f, chip_programmer_position.z);
                        break;
                    }
                }
            }
        }
        private void BtnIgnitionPlus()
        {
            float value = 0;
            try
            {
                value = Convert.ToSingle(input_sparkAngle.text);
            }
            catch
            {
                value = 0;
            }
            value += 0.5f;

            input_sparkAngle.text = value.ToString("0.0");
        }
        private void BtnIgnitionMinus()
        {
            float value = 0;
            try
            {
                value = Convert.ToSingle(input_sparkAngle.text);
            }
            catch
            {
                value = 0;
            }
            value -= 0.5f;

            input_sparkAngle.text = value.ToString("0.0");
        }
        private void BtnResetMap()
        {
            for (int y = 0; y < 14; y++)
            {
                for (int x = 0; x < 17; x++)
                {
                    try
                    {
                        inputFieldMap[y, x].text = "10.0";
                    }
                    catch (Exception ex)
                    {
                        mod.logger.New("Error while trying to reset programmer input field", $"y - x index: {y} - {x}");
                    }
                }
            }
        }

        public void Handle()
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
                                for (int index = 0; index < fuelSystem.chip_parts.Count; index++)
                                {
                                    ChipPart part = fuelSystem.chip_parts[index];
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
                                for (int index = 0; index < fuelSystem.chip_parts.Count; index++)
                                {
                                    ChipPart part = fuelSystem.chip_parts[index];
                                    if (part.chipInstalledOnProgrammer)
                                    {
                                        SetProgrammerUIStatus(false);
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
                                            if (chipOnProgrammer.chipSave.map == null)
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
                                            mod.logger.New("Error while trying to write chip map");
                                        }
                                    }
                                }
                                input_sparkAngle.text = chipOnProgrammer.chipSave.sparkAngle.ToString("00.0");
                                SetProgrammerUIStatus(true);
                                cb_startAssistEnabled.isOn = chipOnProgrammer.chipSave.startAssistEnabled;
                            }
                        }
                    }
                }
            }
        }

        private void SetProgrammerUIStatus(bool status)
        {
            FsmVariables.GlobalVariables.FindFsmBool("PlayerInMenu").Value = status;
            FsmVariables.GlobalVariables.FindFsmBool("PlayerStop").Value = status;
            FsmVariables.GlobalVariables.FindFsmBool("PlayerSeated").Value = status;
            programmer_ui.enabled = status;
            if (status)
            {
                txtField_error.text = "";
            }
        }

        public void Save()
        {
            for (int index = 0; index < fuelSystem.chip_parts.Count; index++)
            {
                ChipPart part = fuelSystem.chip_parts[index];
                if (part.chipInstalledOnProgrammer)
                {
                    part.activePart.SetActive(true);
                    chip_programmer_chip.SetActive(false);
                    part.chipInstalledOnProgrammer = false;

                    Vector3 chip_programmer_position = mod.chip_programmer_part.activePart.transform.position;
                    part.activePart.transform.position = new Vector3(chip_programmer_position.x, chip_programmer_position.y + 0.05f, chip_programmer_position.z);
                    break;
                }
            }
        }
    }
}
