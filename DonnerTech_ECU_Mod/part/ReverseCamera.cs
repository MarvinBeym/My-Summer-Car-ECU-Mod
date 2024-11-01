using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class ReverseCamera : Module.ModulePart
	{
		protected override string partId => "reverse_camera";
		protected override string partName => "Reverse Camera";
		protected override Vector3 partInstallPosition => new Vector3(0, -0.343f, -0.157f);
		protected override Vector3 partInstallRotation => new Vector3(120, 0, 0);

		private Camera camera;
		private Light light;
		public InfoPanel infoPanel;

		private ReverseCamera_Logic logic;

		public ReverseCamera(GamePart parent, InfoPanel infoPanel) : base(parent)
		{
			this.infoPanel = infoPanel;
			AddScrews(new[]
			{
				new Screw(new Vector3(0f, -0.015f, 0.0185f), new Vector3(0, 0, 0))
			}, 0.5f, 5);


			camera = GetComponent<Camera>();
			light = GetComponent<Light>();

			logic = AddEventBehaviour<ReverseCamera_Logic>(PartEvent.Type.InstallOnCar);
			logic.Init(this);

			AddEventListener(PartEvent.Time.Pre, PartEvent.Type.Unbolted, () =>
			{
				enabled = false;
			});
		}

		public override bool enabled
		{
			get => camera.enabled;
			set
			{
				infoPanel.reverseCameraRenderer.enabled = value;
				camera.enabled = value;
				light.enabled = value;
			}
		}
	}
}