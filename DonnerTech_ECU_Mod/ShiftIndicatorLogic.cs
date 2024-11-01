using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonnerTech_ECU_Mod.part;
using MscModApi.Caching;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class ShiftIndicatorLogic : MonoBehaviour
	{
		private InfoPanel infoPanel;

		private MeshRenderer renderer;
		private Gradient gradient;

		private float blinkTimer = 0;

		void Update()
		{
			if (CarH.hasPower && infoPanel.bolted && CarH.running && infoPanel.isBooted)
			{
				float gradientValue = CarH.drivetrain.rpm / 10000;

				if (CarH.drivetrain.rpm >= 7500)
				{
					blinkTimer += Time.deltaTime;

					if (blinkTimer <= 0.15f)
					{
						renderer.material.color = Color.black;
					}

					if (blinkTimer >= 0.3f)
					{
						blinkTimer = 0;

						renderer.material.color = gradient.Evaluate(gradientValue);
					}
				}
				else
				{
					renderer.material.color = gradient.Evaluate(gradientValue);
				}
			}
			else
			{
				renderer.material.color = Color.black;
			}
		}

		public void Init(InfoPanel infoPanel)
		{
			this.infoPanel = infoPanel;
			renderer = infoPanel.transform.FindChild("shiftIndicator").GetComponent<MeshRenderer>();
			InitGradient();
		}

		public void InitGradient()
		{
			gradient = new Gradient();
			GradientColorKey[] colorKey = new GradientColorKey[3];
			colorKey[0].color = new Color(1.0f, 0.64f, 0.0f); //Orange
			colorKey[0].time = (float)infoPanel.shiftIndicatorBaseLine / 10000;

			colorKey[1].color = Color.green;
			colorKey[1].time = (float)infoPanel.shiftIndicatorGreenLine / 10000;

			colorKey[2].color = Color.red;
			colorKey[2].time = (float)infoPanel.shiftIndicatorRedLine / 10000;

			GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
			alphaKey[0].alpha = 1f - (float)infoPanel.shiftIndicatorBaseLine / 10000;
			alphaKey[0].time = (float)infoPanel.shiftIndicatorBaseLine / 10000;

			alphaKey[1].alpha = 1f - (float)infoPanel.shiftIndicatorGreenLine / 10000;
			alphaKey[1].time = (float)infoPanel.shiftIndicatorGreenLine / 10000;

			alphaKey[2].alpha = 1f - (float)infoPanel.shiftIndicatorRedLine / 10000;
			alphaKey[2].time = (float)infoPanel.shiftIndicatorRedLine / 10000;

			gradient.SetKeys(colorKey, alphaKey);
		}
	}
}
