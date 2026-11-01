using System.Globalization;
using MSCLoader;
using MscModApi.Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	public class InfoPanelValueLogic : MonoBehaviour
	{
		public void Start()
		{
			Collider currentCollider = gameObject.GetComponent<Collider>();

			if (currentCollider != null) {
				Destroy(currentCollider);
			}

			BoxCollider collider = gameObject.AddComponent<BoxCollider>();

			collider.isTrigger = true;
			collider.center = new Vector3(0.016f, -0.002f, 0.002f);
			collider.size = new Vector3(0.035f, 0.016f, 0.01f);
		}

		public void Update()
		{
			if (!gameObject.IsLookingAt()) {
				return;
			}
			ModConsole.Print(gameObject.name);
		}
	}
}