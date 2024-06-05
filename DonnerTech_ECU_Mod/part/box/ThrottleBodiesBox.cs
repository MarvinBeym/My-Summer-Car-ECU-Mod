using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using MscModApi.Shopping;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class ThrottleBodiesBox : Box
	{
		public ThrottleBodiesBox(FuelInjectionManifold parent, GameObject boxModel, GameObject partGameObject) : base("Throttle Bodies",
			"throttle_body",
			"Throttle Body",
			boxModel,
			partGameObject,
			4,
			parent,
			new[]
			{
				new Vector3(0.095f, 0.034f, 0.0785f),
				new Vector3(0.033f, 0.034f, 0.0785f),
				new Vector3(-0.033f, 0.034f, 0.0785f),
				new Vector3(-0.095f, 0.034f, 0.0785f),
	},
			new[]
			{
				new Vector3(-40, 0, 0),
				new Vector3(-40, 0, 0),
				new Vector3(-40, 0, 0),
				new Vector3(-40, 0, 0),
			}, Shop.SpawnLocation.Fleetari.Counter)
		{
			AddScrews(
				new[]
				{
					new Screw(new Vector3(0.016f, -0.016f, 0.0020f), new Vector3(0, 0, 0)),
					new Screw(new Vector3(-0.016f, 0.016f, 0.0020f), new Vector3(0, 0, 0)),
				}, 0.6f, 8);
			
		}
	}
}