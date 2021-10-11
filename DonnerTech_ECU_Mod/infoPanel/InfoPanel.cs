﻿using ModApi.Attachable;
using MSCLoader;
using ScrewablePartAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.infoPanel
{
	public class InfoPanel
	{
		private GameObject gameObject;
		private DonnerTech_ECU_Mod mod;

		private bool workaroundChildDisableDone = false;
		public MscPartApi.Part part;
		public InfoPanel_Logic logic;

		public InfoPanel(DonnerTech_ECU_Mod mod, MscPartApi.Part part, AssetBundle assetBundle)
		{
			this.mod = mod;
			this.part = part;

			TextMesh[] textMeshes = part.gameObject.GetComponentsInChildren<TextMesh>(true);
			foreach (TextMesh textMesh in textMeshes)
			{
				textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
			}

			SpriteRenderer[] spriteRenderers = part.gameObject.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer spriteRenderer in spriteRenderers)
			{
				spriteRenderer.enabled = false;
			}

			if (spriteRenderers.Length > 0 && textMeshes.Length > 0)
			{
				workaroundChildDisableDone = true;
			}


			logic = part.AddWhenInstalledMono<InfoPanel_Logic>();
			logic.Init(this, mod, assetBundle);

			UnityEngine.Object.Destroy(gameObject);
		}

		public void Handle()
		{
			if (!part.IsInstalled())
			{
				if (part.gameObject.transform.localScale.x < 1.5f)
				{
					part.gameObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
				}

				if (!workaroundChildDisableDone)
				{
					TextMesh[] textMeshes = part.gameObject.GetComponentsInChildren<TextMesh>(true);
					foreach (TextMesh textMesh in textMeshes)
					{
						textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
					}


					SpriteRenderer[] spriteRenderers = part.gameObject.GetComponentsInChildren<SpriteRenderer>(true);
					foreach (SpriteRenderer spriteRenderer in spriteRenderers)
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