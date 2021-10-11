using HutongGames.PlayMaker;
using ModApi;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MscPartApi;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class ChipProgrammer
	{
		public DonnerTech_ECU_Mod mod;
		public FuelSystem fuelSystem;
		public Canvas programmer_ui;
		private GameObject programmer_ui_gameObject;

		private Toggle cb_startAssistEnabled;
		private Text txtField_error;

		public InputField[,] inputFieldMap = new InputField[14, 17];

		public List<string> chip_errors = new List<string>();

		private GameObject chip_programmer_chip;
		private FsmGameObject itemPivot;
		private bool startAssistEnabled = false;

		private InputField input_sparkAngle;
		private bool chipInstalledOnProgrammer = false;
		private Chip chipOnProgrammer = null;

		private RaycastHit hit;

		public Keybind programmer_ui_open = new Keybind("programmer_ui_open", "Open/Close", KeyCode.Keypad0);

		public ChipProgrammer(DonnerTech_ECU_Mod mod, FuelSystem fuelSystem)
		{
			this.mod = mod;
			Keybind.Add(mod, programmer_ui_open);

			this.fuelSystem = fuelSystem;
			itemPivot = PlayMakerGlobals.Instance.Variables.FindFsmGameObject("ItemPivot");

			programmer_ui_gameObject =
				GameObject.Instantiate((mod.assetBundle.LoadAsset("ui_interface.prefab") as GameObject));
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
			SparkAngleInputForce sparkAngleInputForce =
				input_sparkAngle.gameObject.AddComponent<SparkAngleInputForce>();

			sparkAngleInputForce.inputField = input_sparkAngle;
			cb_startAssistEnabled = panel.FindChild("cb_startAssistEnabled").gameObject.GetComponent<Toggle>();
			txtField_error = panel.FindChild("errors_displaying").FindChild("txtField_error").gameObject
				.GetComponent<Text>();

			SetProgrammerUIStatus(false);

			btn_closeUi.onClick.AddListener(delegate() { SetProgrammerUIStatus(false); });

			btn_ignitionPlus.onClick.AddListener(BtnIgnitionPlus);
			btn_ignitionMinus.onClick.AddListener(BtnIgnitionMinus);

			cb_startAssistEnabled.onValueChanged.AddListener(delegate(bool newState)
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

			chip_programmer_chip = mod.chip_programmer_part.transform.FindChild("rigid_chip").gameObject;
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
							continue;
						}

						fuelMap[y, x] = Convert.ToSingle(inputFieldMap[y, x].text);
					}
					catch (Exception ex)
					{
						Logger.New("Error while trying to write chip map",
							String.Format("fuel map position - Y: {0} X: {1}", y, x), ex);
					}
				}
			}

			if (chip_errors.Count == 0)
			{
				for (int index = 0; index < fuelSystem.chips.Count; index++)
				{
					Chip chip = fuelSystem.chips[index];
					Part part = chip.part;
					if (chip.chipInstalledOnProgrammer)
					{
						part.SetActive(true);
						chip_programmer_chip.SetActive(false);
						chipInstalledOnProgrammer = false;
						chipOnProgrammer = null;
						chip.chipInstalledOnProgrammer = false;
						chip.chipSave.map = fuelMap;
						chip.chipSave.chipProgrammed = true;
						chip.chipSave.startAssistEnabled = startAssistEnabled;
						try
						{
							chip.chipSave.sparkAngle = Convert.ToSingle(input_sparkAngle.text);
						}
						catch (Exception e)
						{
							Logger.New("Error while trying to save spark timing after chip programming",
								"input field value: " + input_sparkAngle.text, e);
							chip.chipSave.sparkAngle = 14.5f;
						}


						SetProgrammerUIStatus(false);
						Vector3 chip_programmer_position = mod.chip_programmer_part.transform.position;
						part.SetPosition(new Vector3(chip_programmer_position.x, chip_programmer_position.y + 0.05f,
							chip_programmer_position.z));
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
						Logger.New("Error while trying to reset programmer input field", $"y - x index: {y} - {x}");
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
					if (itemInHand.name.StartsWith("Chip") &&
					    itemInHand.name != mod.chip_programmer_part.gameObject.name)
					{
						if (Vector3.Distance(mod.chip_programmer_part.transform.position,
							itemInHand.transform.position) <= 0.075f)
						{
							ModClient.guiInteract("insert chip", GuiInteractSymbolEnum.Assemble);
							if (Helper.LeftMouseDown)
							{
								for (int index = 0; index < fuelSystem.chips.Count; index++)
								{
									Chip chip = fuelSystem.chips[index];
									if (chip.part.gameObject.name == itemInHand.name)
									{
										chip.part.SetActive(false);
										chip_programmer_chip.SetActive(true);
										chipInstalledOnProgrammer = true;
										chip.chipInstalledOnProgrammer = true;

										chipOnProgrammer = chip;
										break;
									}
								}
							}
						}
					}
				}
			}

			if (chipInstalledOnProgrammer && chipOnProgrammer != null)
			{
				if (Helper.DetectRaycastHitObject(mod.chip_programmer_part.gameObject))
				{
					string guiText = String.Format(
						"Press [{0}] to {1}\n" +
						"Press [RIGHT MOUSE] to {2}",
						cInput.GetText("Use"), "open programmer", "remove chip"
					);
					ModClient.guiInteraction = guiText;
					if (Helper.RightMouseDown)
					{
						for (int index = 0; index < fuelSystem.chips.Count; index++)
						{
							Chip chip = fuelSystem.chips[index];
							if (chip.chipInstalledOnProgrammer)
							{
								SetProgrammerUIStatus(false);
								chip.part.SetActive(true);
								chip_programmer_chip.SetActive(false);
								chipInstalledOnProgrammer = false;
								chip.chipInstalledOnProgrammer = false;

								chipOnProgrammer = null;
								Vector3 chip_programmer_position =
									mod.chip_programmer_part.gameObject.transform.position;
								chip.part.SetPosition(new Vector3(chip_programmer_position.x,
									chip_programmer_position.y + 0.05f, chip_programmer_position.z));
								break;
							}
						}
					}
					else if (Helper.UseButtonDown)
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
								catch (Exception ex)
								{
									Logger.New("Error while trying to write input field",
										String.Format("input field position - Y: {0} X: {1}", y, x), ex);
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
			for (int index = 0; index < fuelSystem.chips.Count; index++)
			{
				Chip chip = fuelSystem.chips[index];
				if (chip.chipInstalledOnProgrammer)
				{
					chip.part.SetActive(true);
					chip_programmer_chip.SetActive(false);
					chip.chipInstalledOnProgrammer = false;

					Vector3 chip_programmer_position = mod.chip_programmer_part.gameObject.transform.position;
					chip.part.SetPosition(new Vector3(chip_programmer_position.x, chip_programmer_position.y + 0.05f,
						chip_programmer_position.z));
					break;
				}
			}
		}
	}
}