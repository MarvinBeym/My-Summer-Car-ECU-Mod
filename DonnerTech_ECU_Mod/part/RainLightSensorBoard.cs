using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class RainLightSensorBoard : DerivablePart
	{
		protected override string partId => "rain_light_sensorboard";
		protected override string partName => "Rain & Light Sensorboard";
		protected override Vector3 partInstallPosition => new Vector3(-0.0015f, 0.086f, 0.1235f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		private RainSensorLogic rainSensorLogic;
		private LightSensorLogic lightSensorLogic;


		public RainLightSensorBoard(GamePart parent) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0.078f, 0.0185f, 0f), new Vector3(-90, 0, 0)),
				new Screw(new Vector3(-0.078f, 0.0185f, 0f), new Vector3(-90, 0, 0))
			}, 0.5f, 8);

			rainSensorLogic = AddEventBehaviour<RainSensorLogic>(PartEvent.Type.Install);
			rainSensorLogic.Init(this);

			lightSensorLogic = AddEventBehaviour<LightSensorLogic>(PartEvent.Type.Install);
			lightSensorLogic.Init(this);
		}

		public bool lightSensorEnabled
		{
			get => lightSensorLogic.enabled;
			set => rainSensorLogic.enabled = value;
		}

		public bool rainSensorEnabled
		{
			get => rainSensorLogic.enabled;
			set => rainSensorLogic.enabled = value;
		}
	}
}