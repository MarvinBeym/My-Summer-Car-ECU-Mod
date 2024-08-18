using System.Linq;
using HutongGames.PlayMaker;
using MscModApi.Parts;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DonnerTech_ECU_Mod.part;
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

		public FsmFloat distributor_sparkAngle;
		public FsmFloat racingCarb_adjustAverage;
		public FsmFloat racingCarb_idealSetting;

		public FsmFloat airFuelMixture;
		protected ReadOnlyCollection<BasicPart> fuelInjectorParts;
		public ReadOnlyCollection<BasicPart> throttleBodyParts;
		public List<ChipPart> chips = new List<ChipPart>();


		public DonnerTech_ECU_Mod mod;

		internal ReplacedGameParts fuelInjectionParts;
		protected FuelInjectionManifold fuelInjectionManifold;

		public FuelSystem(DonnerTech_ECU_Mod mod, ReadOnlyCollection<BasicPart> fuelInjectorParts, ReadOnlyCollection<BasicPart> throttleBodyParts, FuelInjectionManifold fuelInjectionManifold)
		{
			this.mod = mod;
			this.fuelInjectionManifold = fuelInjectionManifold;

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

			List<Part> optionalNewParts = new List<Part>
			{
				mod.smartEngineModule,
				mod.mountingPlate,
				mod.cableHarness,
				mod.fuelRail,
			};

			foreach (var fuelInjectorPart in fuelInjectorParts)
			{
				optionalNewParts.Add((Part)fuelInjectorPart);
			}
			foreach (var throttleBodyPart in throttleBodyParts)
			{
				optionalNewParts.Add((Part)throttleBodyPart);
			}


			fuelInjectionParts = new ReplacedGameParts(
				"fuelInjectionParts",
				this.mod,
				new[]
				{
					new GamePart("Database/DatabaseMechanics/Electrics"),
					new GamePart("Database/DatabaseMotor/Distributor"),
					new GamePart("Database/DatabaseOrders/Racing Carburators"),
					new GamePart("Database/DatabaseMotor/Fuelpump")
				},
				new[]
				{
					new GamePart("Database/DatabaseMotor/Carburator"),
					new GamePart("Database/DatabaseOrders/Twin Carburators")
				},
				new List<Part>
				{
					mod.fuelPumpCover,
					mod.fuelInjectionManifold,
					mod.electricFuelPump,
				},
				optionalNewParts
			);


			
			fuelInjectionParts.AddEventListener(ReplacedGamePartsEvent.Type.AllNewBolted, () =>
			{
				fuelInjectionManifold.wiresVisible = replaced;

				if (!replaced)
				{
					fuelInjectionParts.SetReplacedState(false);
				}
			});
			

			fuelInjectionParts.AddEventListener(ReplacedGamePartsEvent.Type.AnyNewUnbolted, () =>
			{
				fuelInjectionManifold.wiresVisible = false;
			});

			fuelInjectionParts.AddEventListener(ReplacedGamePartsEvent.Type.AnyNewUninstalled, () =>
			{
				fuelInjectionManifold.wiresVisible = false;
			});

			fuel_system_logic = mod.smartEngineModule.AddEventBehaviour<FuelSystemLogic>(PartEvent.Type.Install);
			fuel_system_logic.Init(this, mod);
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

				mod.CreateChipPart(id, Shop.SpawnLocation.Fleetari.Counter);
			}
		}

		public void Save()
		{
			SaveOriginals();
		}

		internal bool replaced => fuelInjectionParts.replaced;

		public void AddChip(ChipPart chip)
		{
			chips.Add(chip);

			chip.AddEventListener(PartEvent.Time.Post, PartEvent.Type.Install, delegate
			{
				foreach (var chipPart in chips.Where(chipPart =>
					         !chipPart.installed && !chipPart.installBlocked))
				{
					chipPart.installBlocked = true;
				}

				if (!chip.inUse || !fuelInjectionParts.replaced)
				{
					return;
				}
				fuelInjectionParts.SetReplacedState(true);
				fuel_system_logic.installedChip = chip;
				fuelInjectionManifold.wiresVisible = true;
				fuelInjectionParts.AddNewPart(chip, true);

			});
			chip.AddEventListener(PartEvent.Time.Post, PartEvent.Type.Uninstall, delegate
			{
				foreach (var chipPart in chips.Where(chipPart => chipPart.installBlocked))
				{
					chipPart.installBlocked = false;
				}
				fuelInjectionParts.SetReplacedState(false);
				fuel_system_logic.installedChip = null;
				fuelInjectionManifold.wiresVisible = false;
				fuelInjectionParts.RemoveNewPart(chip, true);
			});
		}
	}
}