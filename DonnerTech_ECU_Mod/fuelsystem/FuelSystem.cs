﻿using System.Linq;
using HutongGames.PlayMaker;
using MscModApi.Parts;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DonnerTech_ECU_Mod.Parts;
using MscModApi;
using MscModApi.Caching;
using UnityEngine;
using UnityEngine.UI;
using MscModApi.Parts.ReplacePart;
using MscModApi.Tools;
using Tools;
using Helper = MscModApi.Tools.Helper;
using MscModApi.Parts.ReplacePart;
using MscModApi.Shopping;

namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class FuelSystem
	{
		public FuelSystemLogic fuel_system_logic;
		public ChipProgrammer chip_programmer;

		public FsmFloat distributor_sparkAngle;
		public FsmFloat racingCarb_adjustAverage;
		public FsmFloat racingCarb_idealSetting;

		public FsmFloat airFuelMixture;
		internal Part[] fuelInjectorParts;
		internal Part[] throttleBodyParts;
		public List<ChipPart> chips = new List<ChipPart>();


		public DonnerTech_ECU_Mod mod;

		internal ReplacementPart fuelInjectionParts;

		public FuelSystem(DonnerTech_ECU_Mod mod, Part[] fuelInjectorParts, Part[] throttleBodyParts)
		{
			this.mod = mod;

			chip_programmer = new ChipProgrammer(mod, this);

			PlayMakerFSM distributor = Cache.Find("Distributor").GetComponent<PlayMakerFSM>();
			//PlayMakerFSM fuelStrainer = Cache.Find("FuelStrainer").GetComponent<PlayMakerFSM>();

			distributor_sparkAngle = distributor.FsmVariables.FindFsmFloat("SparkAngle");
			PlayMakerFSM raceCarb = Cache.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
			racingCarb_adjustAverage = raceCarb.FsmVariables.FindFsmFloat("AdjustAverage");
			racingCarb_idealSetting = raceCarb.FsmVariables.FindFsmFloat("IdealSetting");
			airFuelMixture = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Engine/Fuel").FindFsm("Mixture").FsmVariables
				.FindFsmFloat("AirFuelMixture");

			this.fuelInjectorParts = fuelInjectorParts;
			this.throttleBodyParts = throttleBodyParts;

			fuelInjectionParts = new ReplacementPart(
				new[]
				{
					new OldPart(Cache.Find("Electrics")),
					new OldPart(Cache.Find("Distributor")),
					new OldPart(Cache.Find("Racing Carburators")),
					new OldPart(Cache.Find("Fuelpump")),
					new OldPart(Cache.Find("Carburator"), false),
					new OldPart(Cache.Find("Twin Carburators"), false)
				},
				new[]
				{
					new NewPart(fuelInjectorParts[0]),
					new NewPart(fuelInjectorParts[1]),
					new NewPart(fuelInjectorParts[2]),
					new NewPart(fuelInjectorParts[3]),
					new NewPart(throttleBodyParts[0]),
					new NewPart(throttleBodyParts[1]),
					new NewPart(throttleBodyParts[2]),
					new NewPart(throttleBodyParts[3]),
					new NewPart(mod.fuelRail),
					new NewPart(mod.fuelPumpCover),
					new NewPart(mod.fuelInjectionManifold),
					new NewPart(mod.electricFuelPump),
					new NewPart(mod.smartEngineModule, true),
					new NewPart(mod.mountingPlate, true),
					new NewPart(mod.cableHarness, true),
				});

			fuelInjectionParts.AddAction(ReplacementPart.ActionType.AllFixed, ReplacementPart.PartType.NewPart,
				FuelInjectionInstalled);
			fuelInjectionParts.AddAction(ReplacementPart.ActionType.AnyUninstalled, ReplacementPart.PartType.NewPart,
				FuelInjectionUninstalled);
			fuelInjectionParts.AddAction(ReplacementPart.ActionType.AnyUnfixed, ReplacementPart.PartType.NewPart,
				FuelInjectionUninstalled);

			fuel_system_logic = mod.smartEngineModule.AddWhenInstalledMono<FuelSystemLogic>();
			fuel_system_logic.Init(this, mod);

			LoadChips();

			foreach (var chip in chips)
			{
				chip.AddPostInstallAction(delegate
				{
					foreach (var chipPart in chips.Where(chipPart =>
						         !chipPart.IsInstalled() && !chipPart.IsInstallBlocked()))
					{
						chipPart.BlockInstall(true);
					}
				});

				chip.AddPostUninstallAction(delegate
				{
					foreach (var chipPart in chips.Where(chipPart => chipPart.IsInstallBlocked()))
					{
						chipPart.BlockInstall(false);
					}
				});
			}
		}

		internal void FuelInjectionInstalled()
		{
			if (fuelInjectionParts.AreAllNewFixed() && chips.Any(chip => chip.InUse()))
			{
				fuelInjectionParts.SetFakedInstallStatus(true);
				mod.wires_injectors_pumps.enabled = true;
				mod.wires_sparkPlugs1.enabled = true;
				mod.wires_sparkPlugs2.enabled = true;
			}
			else
			{
				FuelInjectionUninstalled();
			}
		}

		internal void FuelInjectionUninstalled()
		{
			fuelInjectionParts.SetFakedInstallStatus(false);
			mod.wires_injectors_pumps.enabled = false;
			mod.wires_sparkPlugs1.enabled = false;
			mod.wires_sparkPlugs2.enabled = false;
		}

		public void Handle()
		{
			chip_programmer.Handle();
		}

		public void SaveOriginals()
		{
			/*
			try
			{
				OriginalPart fuelPump = allOriginalParts.Find(originalPart => originalPart.partName == "Fuelpump");
				originalPartsSave.fuelPump_installed = fuelPump.gameObjectInstalled;
				originalPartsSave.fuelPump_position = fuelPump.gameObject.transform.position;
				originalPartsSave.fuelPump_rotation = fuelPump.gameObject.transform.rotation;

				OriginalPart racingCarb =
					allOriginalParts.Find(originalPart => originalPart.partName == "Racing Carburators");
				originalPartsSave.racingCarb_installed = racingCarb.gameObjectInstalled;
				originalPartsSave.racingCarb_position = racingCarb.gameObject.transform.position;
				originalPartsSave.racingCarb_rotation = racingCarb.gameObject.transform.rotation;

				OriginalPart distributor =
					allOriginalParts.Find(originalPart => originalPart.partName == "Distributor");
				originalPartsSave.distributor_installed = distributor.gameObjectInstalled;
				originalPartsSave.distributor_position = distributor.gameObject.transform.position;
				originalPartsSave.distributor_rotation = distributor.gameObject.transform.rotation;

				OriginalPart electrics = allOriginalParts.Find(originalPart => originalPart.partName == "Electrics");
				originalPartsSave.electrics_installed = electrics.gameObjectInstalled;
				originalPartsSave.electrics_position = electrics.gameObject.transform.position;
				originalPartsSave.electrics_rotation = electrics.gameObject.transform.rotation;

				SaveLoad.SerializeSaveFile<OriginalPartsSave>(mod, originalPartsSave,
					Helper.CombinePaths(new string[]
						{ModLoader.GetModSettingsFolder(mod), "fuelSystem", orignal_parts_saveFile}));
			}
			catch (Exception ex)
			{
				Logger.New("Error while trying to save original parts replaced by fuel injection system",
					$"path of save file: {Helper.CombinePaths(new string[] {ModLoader.GetModSettingsFolder(mod), "fuelSystem", orignal_parts_saveFile})}",
					ex);
			}
			*/
		}

		public void LoadChips()
		{
			var chipsSavePath =
				Helper.CombinePathsAndCreateIfNotExists(ModLoader.GetModSettingsFolder(mod), "fuelMaps");

			string[] chipSaveFiles = ChipSave.LoadSaveFiles(chipsSavePath, "chip_*_saveFile.json");
			ChipPart.counter = 0;
			foreach (var saveFilePath in chipSaveFiles)
			{
				var fileName = saveFilePath.Replace($"{chipsSavePath}\\", "");
				var id = fileName.Replace("_saveFile.json", "");

				ChipPart chipPart = new ChipPart(
					id,
					$"Chip {ChipPart.counter + 1}",
					mod.smartEngineModule,
					mod.partBaseInfo);
				chips.Add(chipPart);
				chipPart.SetDefaultPosition(Shop.SpawnLocation.Fleetari.Counter);

				chipPart.AddPostInstallAction(delegate
				{
					FuelInjectionInstalled();
					if (chipPart.IsProgrammed())
					{
						fuel_system_logic.installedChip = chipPart;
					}
				});
				chipPart.AddPostUninstallAction(delegate
				{
					FuelInjectionUninstalled();
					fuel_system_logic.installedChip = null;
				});
			}
		}

		public void Save()
		{
			chip_programmer.Save();
			SaveOriginals();
		}

		internal bool IsFixed()
		{
			return fuelInjectionParts.AreAllNewFixed(true) && chips.Any(chip => chip.InUse());
		}
	}
}