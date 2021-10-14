using System.Collections.Generic;
using MSCLoader;
using MscModApi.Parts;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.Parts
{
	public class ChipPart : Part
	{
		private static readonly Vector3 installPosition = new Vector3(0.008f, 0.001f, -0.058f);
		private static readonly Vector3 installRotation = new Vector3(0, 90, -90);
		internal static GameObject prefab;
		internal static int counter = 0;
		internal bool chipInstalledOnProgrammer;
		internal ChipSave chipSave;

		public ChipPart(string id, string name, Part parentPart,
			PartBaseInfo partBaseInfo) : base(id, name, prefab, parentPart,
			installPosition, installRotation, partBaseInfo)
		{
			counter++;
		}

		public override void CustomSaveLoading(Mod mod, string saveFileName)
		{
			chipSave = Helper.LoadSaveOrReturnNew<ChipSave>(mod, Helper.CombinePathsAndCreateIfNotExists("fuelMaps", saveFileName));
		}

		public override void CustomSaveSaving(Mod mod, string saveFileName)
		{
			SaveLoad.SerializeSaveFile<ChipSave>(mod, chipSave, Helper.CombinePathsAndCreateIfNotExists("fuelMaps", saveFileName));
		}
	}
}