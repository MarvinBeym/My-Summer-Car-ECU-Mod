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
		private Text[] allUiTextComponents;
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

		private void UiVisible(bool visible)
		{
			playerInMenu.Value = visible;
			playerStop.Value = visible;
			optionsMenu.SetActive(false);

			uiCanvas.enabled = visible;
			if (visible)
			{
				textError.text = "";
				UiTextGlitchFix();
			}
		}

		/// <summary>
		/// Fixes the glitched ui text of the chip programmer interface (unknown origin)
		/// </summary>
		private void UiTextGlitchFix()
		{
			foreach (var textComponent in allUiTextComponents)
			{
				textComponent.gameObject.SetActive(false);
				textComponent.gameObject.SetActive(true);
			}
		}

		private IEnumerator AwaitChipInsertion(ChipPart chipInHand)
		{
			while (chipProgrammer.chipOnProgrammer != chipInHand && Vector3.Distance(chipProgrammer.transform.position, chipInHand.transform.position) <= 0.075f)
			{
				UserInteraction.GuiInteraction(UserInteraction.Type.Assemble, $"Insert {chipInHand.cleanName}");
				if (UserInteraction.LeftMouseDown)
				{
					chipProgrammer.Insert(chipInHand);
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
			if (UserInteraction.RightMouseDown && chipProgrammer.chipOnProgrammer != null)
			{
				UiVisible(false);
				chipProgrammer.Remove();
			}
			else if (UserInteraction.UseButtonDown && chipProgrammer.chipOnProgrammer != null)
			{
				for (int row = 0; row < inputFieldMap.GetLength(0); row++)
				{
					for (int column = 0; column < inputFieldMap.GetLength(1); column++)
					{
						try
						{
							if (chipProgrammer.chipOnProgrammer.fuelMap == null)
							{
								inputFieldMap[row, column].text = "";
							}
							else
							{
								inputFieldMap[row, column].text =
									chipProgrammer.chipOnProgrammer.GetFuelMapValue(row, column).ToString("00.0");
							}
						}
						catch (Exception ex)
						{
							Logger.New("Error while trying to write input field", $"input field position - Y: {row} X: {column}", ex);
						}
					}
				}

				inputSparkAngle.text = chipProgrammer.chipOnProgrammer.sparkAngle.ToString("00.0");
				toggleStartAssistEnabled.isOn = chipProgrammer.chipOnProgrammer.startAssist;
				UiVisible(true);
			}
		}

		public void Init(ChipProgrammer chipProgrammer, GameObject uiGameObject, FuelSystem fuelSystem)
		{
			this.chipProgrammer = chipProgrammer;
			this.fuelSystem = fuelSystem;
			inputFieldMap = new InputField[TABLE_ROWS, TABLE_COLUMNS];

			allUiTextComponents = uiGameObject.GetComponentsInChildren<Text>(true);

			uiCanvas = uiGameObject.GetComponent<Canvas>();
			uiCanvas.name = "FuelSystem_Programmer_UI";

			playerInMenu = FsmVariables.GlobalVariables.FindFsmBool("PlayerInMenu");
			playerStop = FsmVariables.GlobalVariables.FindFsmBool("PlayerStop");
			optionsMenu = Cache.Find("Systems").FindChild("OptionsMenu");

			var panel = uiGameObject.transform.FindChild("panel");

			var btnCloseUi = panel.FindChild("btn_closeUI").gameObject.GetComponent<Button>();
			btnCloseUi.onClick.AddListener(() => UiVisible(false));

			var btnIgnitionPlus = panel.FindChild("btn_ignitionPlus").gameObject.GetComponent<Button>();
			btnIgnitionPlus.onClick.AddListener(() => sparkAngle += 0.5f);

			var btnIgnitionMinus = panel.FindChild("btn_ignitionMinus").gameObject.GetComponent<Button>();
			btnIgnitionMinus.onClick.AddListener(() => sparkAngle -= 0.5f);

			toggleStartAssistEnabled = panel.FindChild("cb_startAssistEnabled").gameObject.GetComponent<Toggle>();
			textError = panel
				.FindChild("errors_displaying")
				.FindChild("txtField_error").gameObject
				.GetComponent<Text>();

			var btnWriteChip = panel.FindChild("btn_writeChip").gameObject.GetComponent<Button>();
			btnWriteChip.onClick.AddListener(BtnWriteChip);

			var btnResetMap = panel.FindChild("btn_resetMap").gameObject.GetComponent<Button>();
			btnResetMap.onClick.AddListener(BtnResetMap);

			inputSparkAngle = panel.FindChild("input_sparkAngle").gameObject.GetComponent<InputField>();
			var sparkAngleInputForce = inputSparkAngle.gameObject.AddComponent<SparkAngleInputForce>();
			sparkAngleInputForce.inputField = inputSparkAngle;

			UiVisible(false);

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

			chipProgrammer.chipOnProgrammer.fuelMap = fuelMap;
			chipProgrammer.chipOnProgrammer.programmed = true;
			chipProgrammer.chipOnProgrammer.startAssist = toggleStartAssistEnabled.isOn;
			try
			{
				chipProgrammer.chipOnProgrammer.sparkAngle = sparkAngle;
			}
			catch (Exception e)
			{
				Logger.New("Error while trying to save spark timing after chip programming",
					"input field value: " + sparkAngle, e);
				chipProgrammer.chipOnProgrammer.sparkAngle = 14.5f;
			}

			chipProgrammer.Write(fuelMap, toggleStartAssistEnabled.isOn, sparkAngle);

			UiVisible(false);
			chipProgrammer.Remove();
		}
	}
}