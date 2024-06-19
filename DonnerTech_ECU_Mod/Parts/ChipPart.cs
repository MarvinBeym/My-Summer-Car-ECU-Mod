using System;
using System.Collections.Generic;
using System.IO;
using DonnerTech_ECU_Mod.part;
using MSCLoader;
using MscModApi.Parts;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.Parts
{
	public class ChipPart : Part
	{
		//May need to be reset onload
		internal static int counter = 0;

		//May need to be reset onload
		internal static GameObject prefab;
		private static readonly Vector3 installPosition = new Vector3(0.008f, 0.001f, -0.058f);
		private static readonly Vector3 installRotation = new Vector3(0, 90, -90);

		private ChipSave chipSave;
		private readonly ChipProgrammer chipProgrammer;

		public ChipPart(
			string id,
			string name,
			Part parentPart,
			PartBaseInfo partBaseInfo,
			ChipProgrammer chipProgrammer
		) : base(id, name, prefab, parentPart, installPosition, installRotation, partBaseInfo)
		{
			this.chipProgrammer = chipProgrammer;

			AddEventListener(PartEvent.Time.Pre, PartEvent.Type.Save, () =>
			{
				if (!installedOnProgrammer) return;

				active = true;

				var chipProgrammerPosition = chipProgrammer.gameObject.transform.position;
				position = new Vector3(
					chipProgrammerPosition.x,
					chipProgrammerPosition.y + 0.05f,
					chipProgrammerPosition.z
				);
			});

			counter++;
		}

		public override void CustomSaveLoading(Mod mod, string saveFileName)
		{
			chipSave = Helper.LoadSaveOrReturnNew<ChipSave>(mod, Helper.CombinePaths("fuelMaps", saveFileName));
		}

		public override void CustomSaveSaving(Mod mod, string saveFileName)
		{
			Directory.CreateDirectory(Helper.CombinePaths(ModLoader.GetModSettingsFolder(mod), "fuelMaps"));
			SaveLoad.SerializeSaveFile<ChipSave>(mod, chipSave, Helper.CombinePaths("fuelMaps", saveFileName));
		}

		public float GetFuelMapValue(int throttleIndex, int rpmIndex)
		{
			return fuelMap[throttleIndex, rpmIndex];
		}

		public bool programmed
		{
			get => chipSave.programmed;
			set => chipSave.programmed = value;
		}

		public float sparkAngle
		{
			get => chipSave.sparkAngle;
			set => chipSave.sparkAngle = value;
		}

		public float[,] fuelMap
		{
			get => chipSave.map;
			set => chipSave.map = value;
		}

		public bool installedOnProgrammer => chipProgrammer.chipOnProgrammer == this;

		public bool startAssist
		{
			get => chipSave.startAssistEnabled;
			set => chipSave.startAssistEnabled = value;
		}


		public bool inUse => installed && programmed && !installedOnProgrammer && fuelMap != null;
	}
}