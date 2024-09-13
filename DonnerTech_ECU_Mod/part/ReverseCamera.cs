using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class ReverseCamera : DerivablePart
	{
		protected override string partId => "reverse_camera";
		protected override string partName => "Reverse Camera";
		protected override Vector3 partInstallPosition => new Vector3(0, -0.343f, -0.157f);
		protected override Vector3 partInstallRotation => new Vector3(120, 0, 0);

		public ReverseCamera_Logic logic;


		public ReverseCamera(GamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0f, -0.015f, 0.0185f), new Vector3(0, 0, 0))
			}, 0.5f, 5);

			logic = AddEventBehaviour<ReverseCamera_Logic>(PartEvent.Type.InstallOnCar);
		}

		public void SetEnabled(bool enabled)
		{
			logic.SetEnabled(enabled);
		}
	}
}