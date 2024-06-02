using System;
using System.Collections.Generic;
using System.IO;
using MSCLoader;
using MscModApi.Parts;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.Parts
{
	public class ChipPart : Part
	{
		internal static int counter = 0;
		internal static GameObject prefab;
		private static readonly Vector3 installPosition = new Vector3(0.008f, 0.001f, -0.058f);
		private static readonly Vector3 installRotation = new Vector3(0, 90, -90);

		private bool installedOnProgrammer;
		private ChipSave chipSave;

		public ChipPart(string id, string name, Part parentPart,
			PartBaseInfo partBaseInfo) : base(id, name, prefab, parentPart,
			installPosition, installRotation, partBaseInfo)
		{
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

		public float[,] GetFuelMap()
		{
			return chipSave.map;
		}

		public float GetFuelMapValue(int throttleIndex, int rpmIndex)
		{
			return GetFuelMap()[throttleIndex, rpmIndex];
		}

		public bool IsProgrammed()
		{
			return chipSave.programmed;
		}

		public float GetSparkAngle()
		{
			return chipSave.sparkAngle;
		}

		public bool IsStartAssistEnabled()
		{
			return chipSave.startAssistEnabled;
		}

		public void SetProgrammed(bool programmed)
		{
			chipSave.programmed = programmed;
		}

		public void SetSparkAngle(float sparkAngle)
		{
			chipSave.sparkAngle = sparkAngle;
		}

		public void SetFuelMap(float[,] map)
		{
			chipSave.map = map;
		}


		public void SetFuelMapValue(int y, int x, float value)
		{
			chipSave.map[y, x] = value;
		}

		public void SetInstalledOnProgrammer(bool installed)
		{
			this.installedOnProgrammer = installed;
		}

		public bool IsInstalledOnProgrammer()
		{
			return installedOnProgrammer;
		}

		internal void SetStartAssistEnabled(bool enabled)
		{
			chipSave.startAssistEnabled = enabled;
		}

		internal bool InUse()
		{
			return IsInstalled() && IsProgrammed() && !IsInstalledOnProgrammer() && GetFuelMap() != null &&
			       IsProgrammed();
		}
	}
}