using MSCLoader;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class ReverseCamera_Logic : MonoBehaviour
	{
		private Mod mainMod;
		private DonnerTech_ECU_Mod donnerTech_ecu_mod;

		private GameObject reverse_camera;

		private Camera camera;
		private Light light;

		// Use this for initialization
		void Start()
		{
			System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
			Mod[] modsArr = mods.ToArray();
			foreach (Mod mod in modsArr)
			{
				if (mod.Name == "DonnerTechRacing ECUs")
				{
					mainMod = mod;
					break;
				}
			}

			donnerTech_ecu_mod = (DonnerTech_ECU_Mod) mainMod;

			reverse_camera = this.gameObject;

			camera = gameObject.GetComponent<Camera>();
			light = gameObject.GetComponent<Light>();
		}

		public void SetEnabled(bool enabled)
		{
			if (camera != null)
				camera.enabled = enabled;
			if (light != null)
				light.enabled = enabled;
		}
	}
}