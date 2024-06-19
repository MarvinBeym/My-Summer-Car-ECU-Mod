using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DonnerTech_ECU_Mod.Parts;
using HutongGames.PlayMaker;
using MscModApi.Caching;
using MscModApi.Tools;
using UnityEngine;
using UnityEngine.UI;
using ChipProgrammer = DonnerTech_ECU_Mod.part.ChipProgrammer;


namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class ChipProgrammerLogic : MonoBehaviour
	{
		protected const int TABLE_COLUMNS = 17;
		protected const int TABLE_ROWS = 14;

		public FuelSystem fuelSystem;
		private GameObject rigidChip;

		public Canvas uiCanvas;

		private FsmBool playerInMenu;
		private FsmBool playerStop;
		private GameObject optionsMenu;

		private InputField inputSparkAngle;
		private Toggle toggleStartAssistEnabled;
		private Text textError;
		public List<string> errors = new List<string>();

		public InputField[,] inputFieldMap;
		public ChipProgrammer chipProgrammer;

		private bool chipInstalledOnProgrammer => chipOnProgrammer != null;
		private ChipPart chipOnProgrammer = null;

		private Coroutine awaitChipInsertionRoutine;

		private float sparkAngle
		{
			get
			{
				float value;
				try
				{
					value = Convert.ToSingle(inputSparkAngle.text);
				}
				catch
				{
					value = 0;
				}

				return value;
			}
			set => inputSparkAngle.text = value.ToString("0.0");
		}

		private bool startAssistEnabled
		{
			get => toggleStartAssistEnabled.isOn;
			set => toggleStartAssistEnabled.isOn = value;
		}

		public bool showUi
		{
			get => uiCanvas.enabled;
			set
			{
				playerInMenu.Value = value;
				playerStop.Value = value;
				optionsMenu.SetActive(false);

				uiCanvas.enabled = value;
				if (value) textError.text = "";
			}
		}

		private IEnumerator AwaitChipInsertion(ChipPart chipInHand)
		{
			while (chipOnProgrammer != chipInHand && Vector3.Distance(chipProgrammer.transform.position, chipInHand.transform.position) <= 0.075f)
			{
				UserInteraction.GuiInteraction(UserInteraction.Type.Assemble, $"Insert {chipInHand.cleanName}");
				if (UserInteraction.LeftMouseDown)
				{
					InsertChipIntoProgrammer(chipInHand);
				}
				yield return null;
			}

			awaitChipInsertionRoutine = null;
		}

		private void Update()
		{
			ChipPart chipInHand = fuelSystem.chips.FirstOrDefault(chip => chip.isHolding);
			if (chipInHand != null)
			{
				if (Vector3.Distance(chipProgrammer.transform.position, chipInHand.transform.position) > 0.075f)
				{
					return;
				}

				if (awaitChipInsertionRoutine == null)
				{
					awaitChipInsertionRoutine = StartCoroutine(AwaitChipInsertion(chipInHand));
				}

				return;
			}

			if (!chipProgrammer.isLookingAt)
			{
				return;
			}

			string guiText = String.Format(
				"Press [{0}] to {1}\n" +
				"Press [RIGHT MOUSE] to {2}",
				cInput.GetText("Use"), "open programmer", "remove chip"
			);

			UserInteraction.GuiInteraction(guiText);
			if (UserInteraction.RightMouseDown && chipInstalledOnProgrammer)
			{
				showUi = false;
				RemoveChipFromProgrammer();
			}
			else if (UserInteraction.UseButtonDown)
			{
				for (int row = 0; row < inputFieldMap.GetLength(0); row++)
				{
					for (int column = 0; column < inputFieldMap.GetLength(1); column++)
					{
						try
						{
							if (chipOnProgrammer.GetFuelMap() == null)
							{
								inputFieldMap[row, column].text = "";
							}
							else
							{
								inputFieldMap[row, column].text =
									chipOnProgrammer.GetFuelMapValue(row, column).ToString("00.0");
							}
						}
						catch (Exception ex)
						{
							Logger.New("Error while trying to write input field", $"input field position - Y: {row} X: {column}", ex);
						}
					}
				}

				inputSparkAngle.text = chipOnProgrammer.GetSparkAngle().ToString("00.0");
				toggleStartAssistEnabled.isOn = chipOnProgrammer.IsStartAssistEnabled();
				showUi = true;
			}
		}

		public void Init(ChipProgrammer chipProgrammer, GameObject uiGameObject, FuelSystem fuelSystem)
		{
			this.chipProgrammer = chipProgrammer;
			this.fuelSystem = fuelSystem;
			inputFieldMap = new InputField[TABLE_ROWS, TABLE_COLUMNS];

			uiCanvas = uiGameObject.GetComponent<Canvas>();
			uiCanvas.name = "FuelSystem_Programmer_UI";

			playerInMenu = FsmVariables.GlobalVariables.FindFsmBool("PlayerInMenu");
			playerStop = FsmVariables.GlobalVariables.FindFsmBool("PlayerStop");
			optionsMenu = Cache.Find("Systems").FindChild("OptionsMenu");

			var panel = uiGameObject.transform.FindChild("panel");

			var btnCloseUi = panel.FindChild("btn_closeUI").gameObject.GetComponent<Button>();
			btnCloseUi.onClick.AddListener(() => showUi = false);

			var btnIgnitionPlus = panel.FindChild("btn_ignitionPlus").gameObject.GetComponent<Button>();
			btnIgnitionPlus.onClick.AddListener(() => sparkAngle += 0.5f);

			var btnIgnitionMinus = panel.FindChild("btn_ignitionMinus").gameObject.GetComponent<Button>();
			btnIgnitionMinus.onClick.AddListener(() => sparkAngle -= 0.5f);

			toggleStartAssistEnabled = panel.FindChild("cb_startAssistEnabled").gameObject.GetComponent<Toggle>();
			textError = panel
				.FindChild("errors_displaying")
				.FindChild("txtField_error").gameObject
				.GetComponent<Text>();

			toggleStartAssistEnabled.onValueChanged.AddListener(delegate { startAssistEnabled = !startAssistEnabled; });

			var btnWriteChip = panel.FindChild("btn_writeChip").gameObject.GetComponent<Button>();
			btnWriteChip.onClick.AddListener(BtnWriteChip);

			var btnResetMap = panel.FindChild("btn_resetMap").gameObject.GetComponent<Button>();
			btnResetMap.onClick.AddListener(BtnResetMap);

			inputSparkAngle = panel.FindChild("input_sparkAngle").gameObject.GetComponent<InputField>();
			var sparkAngleInputForce = inputSparkAngle.gameObject.AddComponent<SparkAngleInputForce>();
			sparkAngleInputForce.inputField = inputSparkAngle;

			showUi = false;

			var table = uiCanvas.transform.FindChild("panel").FindChild("table").gameObject;

			var inputFields = new InputField[238];

			for (var i = 0; i < inputFields.Length; i++)
				inputFields[i] = table.transform.FindChild("input-" + (i + 1)).GetComponent<InputField>();

			for (var row = 0; row < TABLE_ROWS; row++)
			for (var column = 0; column < TABLE_COLUMNS; column++)
			{
				var index = row * TABLE_COLUMNS + column;
				var floatForce = inputFields[index].gameObject.AddComponent<FloatForce>();
				floatForce.inputField = inputFields[index];
				inputFieldMap[row, column] = inputFields[index];
			}

			rigidChip = chipProgrammer.transform.FindChild("rigid_chip").gameObject;
			rigidChip.SetActive(false);
		}


		private void BtnResetMap()
		{
			for (var row = 0; row < TABLE_ROWS; row++)
			for (var column = 0; column < TABLE_COLUMNS; column++)
				try
				{
					inputFieldMap[row, column].text = "10.0";
				}
				catch (Exception)
				{
					Logger.New("Error while trying to reset programmer input field", $"row - column index: {row} - {column}");
				}
		}

		private void BtnWriteChip()
		{
			errors.Clear();
			textError.text = "";
			var fuelMap = new float[TABLE_ROWS, TABLE_COLUMNS];

			for (var row = 0; row < TABLE_ROWS; row++)
			for (var column = 0; column < TABLE_COLUMNS; column++)
				try
				{
					if (inputFieldMap[row, column].text == "")
					{
						var error = $"Value in row {row}, column {column} is invalid";
						errors.Add(error);
						textError.text += error + "\n";
						continue;
					}

					fuelMap[row, column] = Convert.ToSingle(inputFieldMap[row, column].text);
				}
				catch (Exception ex)
				{
					Logger.New("Error while trying to write chip map",
						$"fuel map position - Y: {row} X: {column}", ex);
				}

			if (errors.Count != 0)
			{
				return;
			}

			chipOnProgrammer.SetFuelMap(fuelMap);
			chipOnProgrammer.SetProgrammed(true);
			chipOnProgrammer.SetStartAssistEnabled(startAssistEnabled);
			try
			{
				chipOnProgrammer.SetSparkAngle(sparkAngle);
			}
			catch (Exception e)
			{
				Logger.New("Error while trying to save spark timing after chip programming",
					"input field value: " + sparkAngle, e);
				chipOnProgrammer.SetSparkAngle(14.5f);
			}

			showUi = false;
			RemoveChipFromProgrammer();
		}

		protected void InsertChipIntoProgrammer(ChipPart chip)
		{
			chip.active = false;
			rigidChip.SetActive(true);
			chipOnProgrammer = chip;
			chip.SetInstalledOnProgrammer(true);
		}

		protected void RemoveChipFromProgrammer()
		{
			if (!chipInstalledOnProgrammer)
			{
				return;
			}

			chipOnProgrammer.active = true;
			rigidChip.SetActive(false);
			chipOnProgrammer.SetInstalledOnProgrammer(false);

			Vector3 chipProgrammerPosition = chipProgrammer.gameObject.transform.position;
			chipOnProgrammer.position = new Vector3(
				chipProgrammerPosition.x,
				chipProgrammerPosition.y + 0.05f,
				chipProgrammerPosition.z
			);
			chipOnProgrammer = null;
		}
	}
}