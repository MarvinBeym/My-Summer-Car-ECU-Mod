using MSCLoader;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class ReverseCamera_Logic : MonoBehaviour
    {
        private Mod mainMod;
        private DonnerTech_ECU_Mod donnerTech_ecu_mod;

        private GameObject reverse_camera;

        private Camera reverse_camera_camera;
        private Light reverse_camera_light;

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

            donnerTech_ecu_mod = (DonnerTech_ECU_Mod)mainMod;

            reverse_camera = this.gameObject;

            reverse_camera_camera = reverse_camera.GetComponent<Camera>();
            reverse_camera_light = reverse_camera.GetComponent<Light>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetReverseCameraEnabled(bool enabled)
        {
            if(reverse_camera_camera != null)
                reverse_camera_camera.enabled = enabled;
            if(reverse_camera_light != null)
                reverse_camera_light.enabled = enabled;
        }
    }
}