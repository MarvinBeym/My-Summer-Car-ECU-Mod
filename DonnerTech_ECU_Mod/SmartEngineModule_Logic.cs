using HutongGames.PlayMaker;
using MSCLoader;
using System;
using MscModApi;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class SmartEngineModule_Logic : MonoBehaviour
	{
		private DonnerTech_ECU_Mod mod;

		public enum Module
		{
			Als,
			TwoStep,
		}
		
		private Part part;

		public PlayMakerFSM modulesFsm;
		public FsmFloat twoStepRpm = new FsmFloat("2Step rpm");
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

			modulesFsm = Cache.Find(mod.ID).AddComponent<PlayMakerFSM>();
			modulesFsm.FsmName = "Modules";

			modulesFsm.FsmVariables.BoolVariables = new FsmBool[]
			{
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
			if (mod.smartEngineModule.bolted && !twoStepModuleEnabled.Value && CarH.drivetrain.maxRPM != 8500)
			{
				CarH.drivetrain.maxRPM = 8500;
			}

			if (!CarH.hasPower || !CarH.playerInCar || !mod.cableHarness.bolted ||
			    !mod.mountingPlate.bolted)
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

		internal void Init(Part smartEngineModule)
		{
			part = smartEngineModule;

			part.AddEventListener(PartEvent.Time.Post, PartEvent.Type.Uninstall, OnUninstall);
		}

		private void OnUninstall()
		{
			SetAls(false);
			SetTwoStep(false);
		}

		public void ToggleModule(Module module)
		{
			switch (module)
			{
				case Module.Als:
					SetAls(!alsModuleEnabled.Value);
					break;
				case Module.TwoStep:
					SetTwoStep(!twoStepModuleEnabled.Value);
					break;
			}
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
			if (!this.part.bolted)
			{
				state = false;
			}

			return state;
		}
	}
}