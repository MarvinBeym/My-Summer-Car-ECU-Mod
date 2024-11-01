using DonnerTech_ECU_Mod.part;
using DonnerTech_ECU_Mod.part.Module;
using MSCLoader;
using MscModApi.Caching;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class ReverseCamera_Logic : MonoBehaviour
	{
		private ReverseCamera reverseCamera;
		private InfoPanel infoPanel;

		public void Init(ReverseCamera reverseCamera)
		{
			this.reverseCamera = reverseCamera;
			infoPanel = reverseCamera.infoPanel;
		}

		void Update()
		{
			reverseCamera.enabled = CarH.hasPower && infoPanel.bolted && infoPanel.isBooted && CarH.drivetrain.gear == 0;
		}
	}
}