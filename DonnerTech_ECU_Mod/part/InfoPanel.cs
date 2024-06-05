using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using UnityEngine;

namespace DonnerTech_ECU_Mod.part
{
	public class InfoPanel : DerivablePart
	{
		protected override string partId => "info_panel";
		protected override string partName => "Info Panel";
		protected override Vector3 partInstallPosition => new Vector3(0.25f, -0.088f, -0.01f);
		protected override Vector3 partInstallRotation => new Vector3(0, 180, 180);

		protected bool workaroundChildDisableDone = false;
		public InfoPanel_Logic logic;


		public InfoPanel(GamePart parent, DonnerTech_ECU_Mod mod, AssetBundle assetBundle) : base(parent, DonnerTech_ECU_Mod.partBaseInfo)
		{
			logic = AddEventBehaviour<InfoPanel_Logic>(PartEvent.Type.Install);
			logic.Init(this, mod, assetBundle);

			AddEventListener(PartEvent.Time.Post, PartEvent.Type.Install,
				delegate { transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); });
			AddScrews(new[]
			{
				new Screw(new Vector3(0f, -0.025f, -0.082f), new Vector3(180, 0, 0))
			}, 0.8f, 8);


			TextMesh[] textMeshes = gameObject.GetComponentsInChildren<TextMesh>(true);
			foreach (TextMesh textMesh in textMeshes)
			{
				textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
			}

			SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				spriteRenderer.enabled = false;
			}

			if (spriteRenderers.Length > 0 && textMeshes.Length > 0)
			{
				workaroundChildDisableDone = true;
			}
		}

		public void Handle()
		{
			if (transform.localScale.x < 1.5f)
			{
				transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
			}

			if (!installed)
			{
				if (!workaroundChildDisableDone)
				{
					TextMesh[] textMeshes = gameObject.GetComponentsInChildren<TextMesh>(true);
					foreach (TextMesh textMesh in textMeshes)
					{
						textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
					}


					SpriteRenderer[] spriteRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
					foreach (var spriteRenderer in spriteRenderers)
					{
						spriteRenderer.enabled = false;
					}

					if (spriteRenderers.Length > 0 && textMeshes.Length > 0)
					{
						workaroundChildDisableDone = true;
					}
				}
			}
		}
	}
}