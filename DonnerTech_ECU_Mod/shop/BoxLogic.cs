using UnityEngine;

using MSCLoader;
using System.Linq;
using MscModApi;
using MscModApi.Parts;
using MscModApi.Tools;


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
			if (gameObject.IsLookingAt() && box.spawnedCounter < parts.Length)
			{
				UserInteraction.GuiInteraction(string.Format("Press [{0}] to {1}", cInput.GetText("Use"), actionToDisplay));
				if (UserInteraction.UseButtonDown)
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
	}
}