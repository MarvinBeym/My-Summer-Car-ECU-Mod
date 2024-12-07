using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part.Module
{
	public abstract class ModulePart : DerivablePart
	{
		public abstract bool enabled { get; set; }


		protected ModulePart(DerivablePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			SetupEventListeners();
		}

		protected ModulePart(GamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			SetupEventListeners();
		}

		private void SetupEventListeners()
		{
			AddEventListener(PartEvent.Time.Post, PartEvent.Type.Uninstall, Disable, false);
			AddEventListener(PartEvent.Time.Post, PartEvent.Type.Unbolted, Disable, false);
			AddEventListener(PartEvent.Time.Post, PartEvent.Type.UninstallFromCar, Disable, false);
		}

		protected void Disable()
		{
			enabled = false;
		}

		public void Toggle()
		{
			enabled = !enabled;
		}
	}
}