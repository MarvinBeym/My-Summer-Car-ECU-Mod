using System.Linq;
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
using MscModApi.Parts.ReplacementPart;

namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class FuelSystem
	{
		public FuelSystemLogic fuel_system_logic;
		public ChipProgrammer chip_programmer;

		public List<Part> allParts = new List<Part>();

		public FsmFloat distributor_sparkAngle;

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


			airFuelMixture = Cache.Find("SATSUMA(557kg, 248)/CarSimulation/Engine/Fuel").FindFsm("Mixture").FsmVariables.FindFsmFloat("AirFuelMixture");

			this.fuelInjectorParts = fuelInjectorParts;
			this.throttleBodyParts = throttleBodyParts;

			var fuel_rail_part = mod.fuel_rail_part;
			var fuel_pump_cover_part = mod.fuel_pump_cover_part;
			var fuel_injection_manifold_part = mod.fuel_injection_manifold_part;

			var electric_fuel_pump_part = mod.electric_fuel_pump_part;

			allParts.AddRange(fuelInjectorParts);
			allParts.AddRange(throttleBodyParts);

			allParts.Add(fuel_rail_part);
			allParts.Add(fuel_pump_cover_part);
			allParts.Add(fuel_injection_manifold_part);
			allParts.Add(electric_fuel_pump_part);

			foreach (var chip in chips)
			{
				chip.AddPostInstallAction(delegate
				{
					foreach (var chipPart in chips.Where(chipPart => !chipPart.IsInstalled() && !chipPart.IsInstallBlocked()))
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

			fuelInjectionParts = new ReplacementPart(new []
			{
				new OldPart(Cache.Find("Electrics")),
				new OldPart(Cache.Find("Distributor")),
				new OldPart(Cache.Find("Racing Carburators")),
				new OldPart(Cache.Find("Fuelpump")),
				new OldPart(Cache.Find("Carburator"), false),
				new OldPart(Cache.Find("Twin Carburators"), false)

			}, allParts.ToArray());

			fuelInjectionParts.AddInstalledAction(ReplacementPart.ActionType.AllInstalled, ReplacementPart.PartType.NewPart,
				delegate
				{
					fuelInjectionParts.SetFakedInstallStatus(mod.smart_engine_module_part.IsFixed() && chips.Any(chip => chip.IsInstalled()));
					mod.wires_injectors_pumps.enabled = true;
					mod.wires_sparkPlugs1.enabled = true;
					mod.wires_sparkPlugs2.enabled = true;
				});
			fuelInjectionParts.AddInstalledAction(ReplacementPart.ActionType.AnyUninstalled,
				ReplacementPart.PartType.NewPart,
				delegate
				{
					fuelInjectionParts.SetFakedInstallStatus(false);
					mod.wires_injectors_pumps.enabled = false;
					mod.wires_sparkPlugs1.enabled = false;
					mod.wires_sparkPlugs2.enabled = false;
				});

			fuel_system_logic = mod.smart_engine_module_part.AddWhenInstalledMono<FuelSystemLogic>();
			fuel_system_logic.Init(this, mod);

			LoadChips();
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
			var chipsSavePath = Helper.CombinePathsAndCreateIfNotExists(ModLoader.GetModSettingsFolder(mod), "fuelMaps");

			string[] chipSaveFiles = ChipSave.LoadSaveFiles(chipsSavePath, "chip_*_saveFile.json");
			ChipPart.counter = 0;
			foreach (var saveFilePath in chipSaveFiles)
			{
				var fileName = saveFilePath.Replace($"{chipsSavePath}\\", "");
				var id = fileName.Replace("_saveFile.json", "");

				ChipPart chipPart = new ChipPart(
					id,
					$"Chip {ChipPart.counter + 1}",
					mod.smart_engine_module_part,
					mod.partBaseInfo);
				chipPart.AddPostInstallAction(delegate
				{
					fuelInjectionParts.SetFakedInstallStatus(
						fuelInjectionParts.AreAllNewFixed(true)
						&& mod.smart_engine_module_part.IsFixed()
						&& chips.Any(chip => chip.IsInstalled())
						);
				});
				chipPart.AddPostUninstallAction(delegate
				{
					fuelInjectionParts.SetFakedInstallStatus(false);
				});
				chips.Add(chipPart);
			}
		}

		public void Save()
		{
			chip_programmer.Save();
			SaveOriginals();
		}

		internal bool IsFixed()
		{
			return fuelInjectionParts.AreAllNewFixed(true) && chips.Any(chip => chip.IsInstalled());
		}
	}
}