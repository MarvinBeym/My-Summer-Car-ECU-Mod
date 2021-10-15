﻿using MSCLoader;
using MscModApi;

using System;
using System.Collections.Generic;
using System.Linq;
using MscModApi.Parts;
using MscModApi.Tools;
using UnityEngine;


namespace ModShop
{
	public class Box
	{
		public GameObject box;
		public int spawnedCounter = 0;
		public Part[] parts;
		public BoxLogic logic;

		private bool checkedUnpacked = false;

		public Box(string partId, string partName, GameObject box, GameObject part_gameObject, int numberOfParts,
			Part parent, Vector3[] installLocations, Vector3[] installRotations,
			bool dontCollideOnRigid = true)
		{
			var partBaseInfo = parent.partBaseInfo;

			this.box = box;

			parts = new Part[numberOfParts];

			for (int i = 0; i < numberOfParts; i++)
			{
				int iOffset = i + 1;

				parts[i] = new Part(
					$"{partId}_{i}", partName + " " + iOffset, part_gameObject,
					parent, installLocations[i], installRotations[i], partBaseInfo);

				if (!parts[i].IsBought())
				{
					parts[i].Uninstall();
					parts[i].SetActive(false);
				}
			}

			logic = box.AddComponent<BoxLogic>();
			logic.Init(parts, "Unpack " + partName, this);
		}

		public void CheckUnpackedOnSave()
		{
			if (!checkedUnpacked && parts[0].IsBought())
			{
				checkedUnpacked = true;
				if (spawnedCounter < parts.Length)
				{
					foreach (var part in parts)
					{
						if (part.IsInstalled() || part.gameObject.activeSelf) continue;
						part.SetPosition(box.transform.position);
						part.SetActive(true);
					}
				}

				box.SetActive(false);
				box.transform.position = new Vector3(0, 0, 0);
				box.transform.localPosition = new Vector3(0, 0, 0);
			}
		}

		internal void AddScrews(Screw[] screws, float overrideScale = 0f, float overrideSize = 0f)
		{
			foreach (var part in parts)
			{
				part.AddScrews(screws.CloneToNew(), overrideScale, overrideSize);
			}
		}

		public bool IsBought()
		{
			return parts.All(part => part.IsBought());
		}
	}
}