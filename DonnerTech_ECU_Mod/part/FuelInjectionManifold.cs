using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class FuelInjectionManifold : DerivablePart
	{
		protected override string partId => "fuel_injection_manifold";
		protected override string partName => "Fuel Injection Manifold";
		protected override Vector3 partInstallPosition => new Vector3(-0.009f, -0.0775f, 0.02f);
		protected override Vector3 partInstallRotation => new Vector3(90, 0, 0);

		protected GameObject injectorsPumpsWiresGameObject;
		protected GameObject sparkPlugs1WiresGameObject;
		protected GameObject sparkPlugs2WiresGameObject;

		public FuelInjectionManifold(AssetBundle assetBundle, GamePart cylinderHead) : base(cylinderHead, DonnerTech_ECU_Mod.partBaseInfo)
		{
			AddScrews(new[]
			{
				new Screw(new Vector3(0.0875f, -0.001f, 0.0025f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(0.053f, -0.043f, 0.0025f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.051f, -0.043f, 0.0025f), new Vector3(0, 0, 0), Screw.Type.Nut),
				new Screw(new Vector3(-0.0865f, -0.001f, 0.0025f), new Vector3(0, 0, 0), Screw.Type.Nut)
			}, 0.6f, 8);


			injectorsPumpsWiresGameObject = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>("wires_injectors_pumps.prefab")); 
			sparkPlugs1WiresGameObject = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>("wires_sparkPlugs_1.prefab")); 
			sparkPlugs2WiresGameObject = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>("wires_sparkPlugs_2.prefab"));

			injectorsPumpsWiresGameObject.transform.parent = transform;
			sparkPlugs1WiresGameObject.transform.parent = cylinderHead.gameObject.transform;
			sparkPlugs2WiresGameObject.transform.parent = CarH.satsuma.transform;

			injectorsPumpsWiresGameObject.SetNameLayerTag("wires_injectors_pumps");
			sparkPlugs1WiresGameObject.SetNameLayerTag("wires_sparkPlugs1");
			sparkPlugs2WiresGameObject.SetNameLayerTag("wires_sparkPlugs2");

			injectorsPumpsWiresGameObject.transform.localPosition = new Vector3(0.0085f, 0.053f, 0.0366f);
			sparkPlugs1WiresGameObject.transform.localPosition = new Vector3(-0.001f, 0.088f, 0.055f);
			sparkPlugs2WiresGameObject.transform.localPosition = new Vector3(0.105f, 0.233f, 0.97f);

			injectorsPumpsWiresGameObject.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0, 0, 0) };
			sparkPlugs1WiresGameObject.transform.localRotation = new Quaternion { eulerAngles = new Vector3(90, 0, 0) };
			sparkPlugs2WiresGameObject.transform.localRotation = new Quaternion { eulerAngles = new Vector3(0, 180, 0) };

			wiresVisible = false;
		}

		public bool wiresVisible
		{
			set
			{
				injectorsPumpsWiresGameObject.SetActive(value);
				sparkPlugs1WiresGameObject.SetActive(value);
				sparkPlugs2WiresGameObject.SetActive(value);
			}
			get => injectorsPumpsWiresGameObject.activeSelf;
		}
	}
}