using DonnerTech_ECU_Mod.fuelsystem;
using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class ChipProgrammer : DerivablePart
	{
		protected override string partId => "chip_programmer";
		protected override string partName => "Chip Programmer";
		protected override Vector3 partInstallPosition => new Vector3(0, 0, 0);
		protected override Vector3 partInstallRotation => new Vector3(0, 0, 0);

		protected Keybind openUi;
		protected DonnerTech_ECU_Mod mod;
		private GameObject uiGameObject;
		private readonly ChipProgrammerLogic logic;
		private readonly BoxCollider chipCollider;

		public ChipProgrammer(DonnerTech_ECU_Mod mod, FuelSystem fuelSystem) : base(NullGamePart.GetInstance(), DonnerTech_ECU_Mod.partBaseInfo)
		{
			this.mod = mod;

			openUi = new Keybind(partId + "_openUi", "Open/Close", KeyCode.Keypad0);
			Keybind.Add(mod, openUi);

			uiGameObject =
				GameObject.Instantiate((mod.assetBundle.LoadAsset<GameObject>("ui_interface.prefab")));
			uiGameObject.name = "FuelSystem_Programmer_UI_GameObject";

			logic = AddComponent<ChipProgrammerLogic>();
			logic.Init(this, uiGameObject, fuelSystem);

			chipCollider = gameObject.AddComponent<BoxCollider>();
			chipCollider.isTrigger = true;
			chipCollider.size = new Vector3(0.1f, 0.1f, 0.1f);
		}

	}
}