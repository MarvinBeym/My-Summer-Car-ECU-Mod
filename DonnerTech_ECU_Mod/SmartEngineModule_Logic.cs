using HutongGames.PlayMaker;
using MSCLoader;
using System;
using MscPartApi;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class SmartEngineModule_Logic : MonoBehaviour
	{
		private DonnerTech_ECU_Mod mod;

		public enum Module
		{
			Abs,
			Esp,
			Tcs,
			Als,
			TwoStep,
		}

		private Part part;
		private Part absModulePart;
		private Part espModulePart;
		private Part tcsModulePart;

		public PlayMakerFSM modulesFsm;
		public FsmFloat twoStepRpm = new FsmFloat("2Step rpm");
		public FsmBool absModuleEnabled = new FsmBool("ABS Module Enabled");
		public FsmBool espModuleEnabled = new FsmBool("ESP Module Enabled");
		public FsmBool tcsModuleEnabled = new FsmBool("TCS Module Enabled");
		public FsmBool alsModuleEnabled = new FsmBool("ALS Module Enabled");
		public FsmBool twoStepModuleEnabled = new FsmBool("2STEP Module Enabled");

		void Start()
		{
			System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
			Mod[] modsArr = mods.ToArray();
			foreach (Mod mod in modsArr)
			{
				if (mod.Name == "DonnerTechRacing ECUs")
				{
					this.mod = (DonnerTech_ECU_Mod) mod;
					break;
				}
			}

			CarH.drivetrain.maxRPM = 8500;
			twoStepRpm.Value = 6500;

			modulesFsm = Game.Find(mod.ID).AddComponent<PlayMakerFSM>();
			modulesFsm.FsmName = "Modules";

			modulesFsm.FsmVariables.BoolVariables = new FsmBool[]
			{
				absModuleEnabled,
				espModuleEnabled,
				tcsModuleEnabled,
				alsModuleEnabled,
				twoStepModuleEnabled,
			};
			modulesFsm.FsmVariables.FloatVariables = new FsmFloat[]
			{
				twoStepRpm,
			};
		}


		void Update()
		{
			if (mod.smart_engine_module_part.IsFixed() && !twoStepModuleEnabled.Value && CarH.drivetrain.maxRPM != 8500)
			{
				CarH.drivetrain.maxRPM = 8500;
			}

			if (!CarH.hasPower || !Helper.PlayerInCar() || !mod.cable_harness_part.IsFixed() ||
			    !mod.mounting_plate_part.IsFixed())
			{
				return;
			}

			if (twoStepModuleEnabled.Value)
			{
				HandleTwoStep();
			}

			if (alsModuleEnabled.Value)
			{
				HandleALSModuleLogic();
			}
		}

		private void HandleALSModuleLogic()
		{
			/*
			timeSinceLastBackFire += Time.deltaTime;
			ModCommunication modCommunication = ecu_mod_smartEngineModule_Part.rigidPart.GetComponent<ModCommunication>();
			if (useThrottleButton && CarH.drivetrain.rpm > 4000 && !als_backfire_allowed)
			{
			    als_backfire_allowed = true;
			}
			else if (!useThrottleButton && CarH.drivetrain.rpm > 4000 && als_backfire_allowed)
			{
			    als_backfire_allowed = false;
			    modCommunication.alsEnabled = true;
			    timeSinceLastBackFire = 0;
			}

			if (alsModuleEnabled && timeSinceLastBackFire >= 0.5)
			{
			    modCommunication.alsEnabled = false;
			}
			*/
		}

		public void HandleTwoStep()
		{
			if (twoStepModuleEnabled.Value && CarH.drivetrain.velo < 3.5f)
			{
				CarH.drivetrain.revLimiterTime = 0;
				CarH.drivetrain.maxRPM = twoStepRpm.Value;
			}
			else
			{
				CarH.drivetrain.revLimiterTime = 0.2f;
				SetTwoStep(false);
			}
		}

		internal void Init(Part smart_engine_module_part, Part absModulePart, Part espModulePart, Part tcsModulePart)
		{
			part = smart_engine_module_part;
			part.AddPostUninstallAction(OnUninstall);

			this.absModulePart = absModulePart;
			this.espModulePart = espModulePart;
			this.tcsModulePart = tcsModulePart;
		}

		private void OnUninstall()
		{
			SetAbs(false);
			SetEsp(false);
			SetTcs(false);
			SetAls(false);
			SetTwoStep(false);
		}

		public void ToggleModule(Module module)
		{
			switch (module)
			{
				case Module.Abs:
					SetAbs(!absModuleEnabled.Value);
					break;
				case Module.Esp:
					SetEsp(!espModuleEnabled.Value);
					break;
				case Module.Tcs:
					SetTcs(!tcsModuleEnabled.Value);
					break;
				case Module.Als:
					SetAls(!alsModuleEnabled.Value);
					break;
				case Module.TwoStep:
					SetTwoStep(!twoStepModuleEnabled.Value);
					break;
			}
		}

		public void SetAbs(bool newState)
		{
			newState = checkStatePartInstalled(newState);
			absModuleEnabled.Value = newState;
			CarH.carController.ABS = newState;
		}

		public void SetEsp(bool newState)
		{
			newState = checkStatePartInstalled(newState);
			espModuleEnabled.Value = newState;
			CarH.carController.ESP = newState;
		}

		public void SetTcs(bool newState)
		{
			newState = checkStatePartInstalled(newState);
			tcsModuleEnabled.Value = newState;
			CarH.carController.TCS = newState;
		}

		public void SetAls(bool newState)
		{
			alsModuleEnabled.Value = checkStatePartInstalled(newState);
		}

		public void SetTwoStep(bool newState)
		{
			twoStepModuleEnabled.Value = checkStatePartInstalled(newState);
		}

		private bool checkStatePartInstalled(bool state)
		{
			if (!this.part.IsFixed())
			{
				state = false;
			}

			return state;
		}
	}
}