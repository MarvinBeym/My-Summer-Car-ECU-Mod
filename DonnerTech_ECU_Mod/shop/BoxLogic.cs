using UnityEngine;
using ModApi;
using MSCLoader;
using System.Linq;
using MscPartApi;
using Tools;


namespace ModShop
{
	public class BoxLogic : MonoBehaviour
	{
		private Box box;
		private string actionToDisplay;

		private Part[] parts;

		// Use this for initialization
		void Start()
		{
		}

		// Update is called once per frame
		void Update()
		{
			if (Helper.DetectRaycastHitObject(this.gameObject) && box.spawnedCounter < parts.Length)
			{
				ModClient.guiInteraction = string.Format("Press [{0}] to {1}", cInput.GetText("Use"), actionToDisplay);
				if (Helper.UseButtonDown)
				{
					Part part = parts[box.spawnedCounter];

					part.SetPosition(gameObject.transform.position);

					part.SetActive(true);
					box.spawnedCounter++;
				}
			}

			if (box.spawnedCounter >= parts.Length)
			{
				this.gameObject.SetActive(false);
			}
		}

		public void Init(Part[] parts, string actionToDisplay, Box box)
		{
			this.box = box;
			this.parts = parts;
			this.actionToDisplay = actionToDisplay;
		}

		public void CheckBoxPosReset(bool boughtBox)
		{
			if (boughtBox)
			{
				if (!parts.Any(part => part.IsInstalled() || part.gameObject.activeSelf))
				{
					this.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
				}
			}
		}
	}
}