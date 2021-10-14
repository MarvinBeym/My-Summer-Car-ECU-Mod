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

namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class FuelSystem
	{
		public FuelSystemLogic fuel_system_logic;
		public ChipProgrammer chip_programmer;

		public Part fuel_injector1_part;
		public Part fuel_injector2_part;
		public Part fuel_injector3_part;
		public Part fuel_injector4_part;

		public Part throttle_body1_part;
		public Part throttle_body2_part;
		public Part throttle_body3_part;
		public Part throttle_body4_part;

		public Part fuel_rail_part;
		public Part fuel_pump_cover_part;
		public Part fuel_injection_manifold_part;
		public Part electric_fuel_pump_part;
		public List<Part> allParts = new List<Part>();

		public FsmFloat distributor_sparkAngle;

		public GameObject fuelStrainer_gameObject;
		public GameObject fuelStrainer_trigger;
		public FsmBool fuelStrainer_installed;
		public FsmBool fuelStrainer_bolted;
		public FsmBool fuelStrainer_detach;

		public GameObject carb_trigger;
		public FsmBool carb_installed;

		public GameObject twinCarb_trigger;
		public FsmBool twinCarb_installed;

		public FsmFloat racingCarb_adjustAverage;
		public FsmFloat racingCarb_tolerance;

		public List<ChipPart> chips = new List<ChipPart>();

		public bool fuel_injection_manifold_applied = false;

		private PartBaseInfo partBaseInfo;
		public DonnerTech_ECU_Mod mod;

		public bool allInstalled
		{
			get { return allParts.All(c => c.IsFixed()); }
		}

		public FuelSystem(DonnerTech_ECU_Mod mod, PartBaseInfo partBaseInfo, Part[] fuelInjectorParts,
			Part[] throttleBodyParts)
		{
			this.mod = mod;
			this.partBaseInfo = partBaseInfo;

			chip_programmer = new ChipProgrammer(mod, this);

			PlayMakerFSM distributor = Cache.Find("Distributor").GetComponent<PlayMakerFSM>();

			PlayMakerFSM carb = Cache.Find("Carburator").GetComponent<PlayMakerFSM>();
			PlayMakerFSM twinCarb = Cache.Find("Twin Carburators").GetComponent<PlayMakerFSM>();
			PlayMakerFSM raceCarb = Cache.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
			//PlayMakerFSM fuelStrainer = Cache.Find("FuelStrainer").GetComponent<PlayMakerFSM>();

			//fuelStrainer_gameObject = Cache.Find("fuel strainer(Clone)");
			//fuelStrainer_detach = GetDetachFsmBool(fuelStrainer_gameObject);
			//fuelStrainer_trigger = distributor.FsmVariables.FindFsmGameObject("Trigger").Value;
			//fuelStrainer_installed = distributor.FsmVariables.FindFsmBool("Installed");
			//fuelStrainer_bolted = distributor.FsmVariables.FindFsmBool("Bolted");

			distributor_sparkAngle = distributor.FsmVariables.FindFsmFloat("SparkAngle");

			carb_trigger = carb.FsmVariables.FindFsmGameObject("Trigger").Value;
			carb_installed = carb.FsmVariables.FindFsmBool("Installed");

			twinCarb_trigger = twinCarb.FsmVariables.FindFsmGameObject("Trigger").Value;
			twinCarb_installed = twinCarb.FsmVariables.FindFsmBool("Installed");

			racingCarb_adjustAverage = raceCarb.FsmVariables.FindFsmFloat("AdjustAverage");
			racingCarb_tolerance = raceCarb.FsmVariables.FindFsmFloat("Tolerance");


			fuel_injector1_part = fuelInjectorParts[0];
			fuel_injector2_part = fuelInjectorParts[1];
			fuel_injector3_part = fuelInjectorParts[2];
			fuel_injector4_part = fuelInjectorParts[3];

			throttle_body1_part = throttleBodyParts[0];
			throttle_body2_part = throttleBodyParts[1];
			throttle_body3_part = throttleBodyParts[2];
			throttle_body4_part = throttleBodyParts[3];

			fuel_rail_part = mod.fuel_rail_part;
			fuel_pump_cover_part = mod.fuel_pump_cover_part;
			fuel_injection_manifold_part = mod.fuel_injection_manifold_part;

			electric_fuel_pump_part = mod.electric_fuel_pump_part;

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
				Cache.Find("Electrics"),
				Cache.Find("Distributor"),
				Cache.Find("Racing Carburators"),
				Cache.Find("Fuelpump")
			}, allParts.ToArray());

			fuelInjectionParts.AddInstalledAction(ReplacementPart.ActionType.AllInstalled, ReplacementPart.PartType.NewPart,
				delegate
				{
					mod.wires_injectors_pumps.enabled = true;
					mod.wires_sparkPlugs1.enabled = true;
					mod.wires_sparkPlugs2.enabled = true;
				});
			fuelInjectionParts.AddInstalledAction(ReplacementPart.ActionType.AnyUninstalled,
				ReplacementPart.PartType.NewPart,
				delegate
				{
					mod.wires_injectors_pumps.enabled = false;
					mod.wires_sparkPlugs1.enabled = false;
					mod.wires_sparkPlugs2.enabled = false;
				});

			fuel_system_logic = mod.smart_engine_module_part.AddWhenInstalledMono<FuelSystemLogic>();
			fuel_system_logic.Init(this, mod);

			LoadChips();
		}

		internal ReplacementPart fuelInjectionParts;

		public void Handle()
		{
			chip_programmer.Handle();

			/*
bool allInstalled_tmp = allInstalled;
			bool anyInstalled_tmp = anyInstalled;

			if (fuelInjectionParts.AreAllNewFixed() && mod.smart_engine_module_part.IsFixed())
			{

			}
			else
			{

			}

			if (anyInstalled_tmp)

				if (mod.smart_engine_module_part.IsFixed())
				{
					if (allInstalled_tmp && !allInstalled_applied)
					{

						if (fuel_system_logic.fuelMap != null)
						{
							foreach (OriginalPart originalPart in allOriginalParts)
							{
								originalPart.SetFakedInstallStatus(true);
							}
						}
						else
						{
							allInstalled_applied = false;
							foreach (OriginalPart originalPart in allOriginalParts)
							{
								originalPart.SetFakedInstallStatus(false);
							}
						}
					}
					else if (!allInstalled)
					{
						allInstalled_applied = false;
						mod.wires_injectors_pumps.enabled = false;
						mod.wires_sparkPlugs1.enabled = false;
						mod.wires_sparkPlugs2.enabled = false;
					}
				}
			*/
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
				chips.Add(chipPart);
			}
		}

		public void Save()
		{
			chip_programmer.Save();
			SaveOriginals();
		}
	}
}