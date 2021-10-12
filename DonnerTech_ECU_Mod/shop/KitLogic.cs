﻿
using MSCLoader;
using MscModApi;
using MscModApi.Parts;
using MscModApi.Tools;
using Tools;
using UnityEngine;

namespace ModShop
{
	public class KitLogic : MonoBehaviour
	{
		private Mod mod;
		private Kit kit;
		private RaycastHit hit;

		// Use this for initialization
		void Start()
		{
		}

		// Update is called once per frame
		void Update()
		{
			if (Camera.main != null && kit.spawnedCounter < kit.parts.Length &&
			    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 0.8f,
				    1 << LayerMask.NameToLayer("Parts")) != false)
			{
				GameObject gameObjectHit;
				gameObjectHit = hit.collider?.gameObject;
				if (gameObjectHit != null && hit.collider)
				{
					if (gameObjectHit.name == this.gameObject.name)
					{
						UserInteraction.GuiInteraction(
							string.Format(
								"Press [{0}] to {1}",
								cInput.GetText("Use"), 
								"Unpack " + kit.parts[kit.spawnedCounter].gameObject.name.Replace("(Clone)", "")
								)
							);
						if (Helper.UseButtonDown)
						{
							Part part = kit.parts[kit.spawnedCounter];

							part.SetPosition(hit.point);

							part.SetActive(true);
							kit.spawnedCounter++;
						}
					}
				}
			}

			if (kit.spawnedCounter >= kit.parts.Length)
			{
				this.gameObject.SetActive(false);
			}
		}

		public void Init(Mod mod, Kit kit)
		{
			this.mod = mod;
			this.kit = kit;
		}
	}
}