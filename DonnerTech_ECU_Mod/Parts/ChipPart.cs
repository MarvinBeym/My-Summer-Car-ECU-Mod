using MscModApi.Parts;
using UnityEngine;

namespace DonnerTech_ECU_Mod.Parts
{
	public class ChipPart : Part
	{
		internal static int counter = 0;
		internal bool chipInstalledOnProgrammer;
		internal new ChipSave partSave;

		public ChipPart(string id, string name, GameObject part, Part parentPart, Vector3 installPosition,
			Vector3 installRotation,
			PartBaseInfo partBaseInfo, bool uninstallWhenParentUninstalls = true,
			bool disableCollisionWhenInstalled = true) : base(id, name, part, parentPart,
			installPosition, installRotation, partBaseInfo, uninstallWhenParentUninstalls,
			disableCollisionWhenInstalled)
		{
			counter++;
		}
	}
}