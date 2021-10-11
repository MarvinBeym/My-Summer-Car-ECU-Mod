using MSCLoader;
using MscPartApi;
using UnityEngine;

namespace ModShop
{
	public class Kit
	{
		private Mod mod;
		public GameObject kitBox;
		public Part[] parts;
		private KitLogic logic;
		public int spawnedCounter = 0;
		public bool bought;

		public Kit(Mod mod, GameObject kitBox, Part[] parts)
		{
			this.mod = mod;
			this.kitBox = kitBox;
			this.parts = parts;
			bought = parts[0].GetBought();
			if (!bought)
			{
				foreach (Part part in parts)
				{
					part.Uninstall();
					part.SetActive(false);
				}
			}

			logic = kitBox.AddComponent<KitLogic>();
			logic.Init(mod, this);
		}

		public void CheckUnpackedOnSave()
		{
			if (parts[0].GetBought())
			{
				if (spawnedCounter < parts.Length)
				{
					foreach (Part part in parts)
					{
						if (!part.IsInstalled() && !part.gameObject.activeSelf)
						{
							part.SetPosition(kitBox.transform.position);
							part.SetActive(true);
						}
					}
				}

				kitBox.SetActive(false);
				kitBox.transform.position = new Vector3(0, 0, 0);
				kitBox.transform.localPosition = new Vector3(0, 0, 0);
			}
		}
	}
}