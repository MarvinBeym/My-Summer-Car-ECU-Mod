using MSCLoader;
using UnityEngine;

namespace DonnerTech_ECU_Mod.info_panel_pages
{
	public class InfoPanelValueLogic : MonoBehaviour
	{
		public void Start()
		{
			//For some reason the gameObject already has a BoxCollider attached. Likely added manually in prefab/ Unity project asset bundle
			//Asset bundle project is broken as .obj files are all missing from project files folder.
			//Needs to be re-extracted from blender project and unity project recreated & exported to assetbundle.
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.isTrigger = true;
			boxCollider.center = new Vector3(0.016f, -0.002f, 0.002f);
			boxCollider.size = new Vector3(0.035f, 0.016f, 0.01f);
		}

		public void OnMouseUpAsButton()
		{
			ModConsole.Print(gameObject.name);
		}
	}
}