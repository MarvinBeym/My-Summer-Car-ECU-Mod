using DonnerTech_ECU_Mod.info_panel_pages;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using System;
using System.Collections.Generic;
using MscModApi.Caching;
using MscModApi.Tools;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
	public class Airride_Logic : MonoBehaviour
	{
		private GameObject wheelFL, wheelFR, wheelRL, wheelRR;
		private PlayMakerFSM suspension;

		private Airride airride;
		private DonnerTech_ECU_Mod mod;
		private FsmFloat travelRally;
		private FsmFloat rallyFrontRate;
		private FsmFloat rallyRearRate;
		private FsmFloat wheelPosRally;

		private FsmFloat steerLimit;

		private List<Transform> wheel_transforms = new List<Transform>();
		private List<Wheel> wheels = new List<Wheel>();

		private enum Selection
		{
			All,
			Front,
			Rear,
		}

		private GameObject GetChildByName(GameObject gameObject, string name)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				GameObject child = gameObject.transform.GetChild(i).gameObject;
				if (child.name == "name")
				{
					return child;
				}
			}

			return null;
		}

		// Use this for initialization
		void Start()
		{
			GameObject fl = CarH.satsuma.FindChild("FL");
			GameObject rl = CarH.satsuma.FindChild("RL");
			GameObject fr = CarH.satsuma.FindChild("FR");
			GameObject rr = CarH.satsuma.FindChild("RR");

			suspension = Cache.Find("Suspension").GetComponent<PlayMakerFSM>();
			wheel_transforms.Add(fl.transform);
			wheel_transforms.Add(rl.transform);
			wheel_transforms.Add(fr.transform);
			wheel_transforms.Add(rr.transform);

			GameObject wheelFL = Cache.Find("SATSUMA(557kg, 248)/FL/AckerFL/wheelFL");
			GameObject wheelRL = Cache.Find("SATSUMA(557kg, 248)/RL/wheelRL");
			GameObject wheelFR = Cache.Find("SATSUMA(557kg, 248)/FR/AckerFR/wheelFR");
			GameObject wheelRR = Cache.Find("SATSUMA(557kg, 248)/RR/wheelRR");
			wheels.Add(wheelFL.GetComponent<Wheel>());
			wheels.Add(wheelRL.GetComponent<Wheel>());
			wheels.Add(wheelFR.GetComponent<Wheel>());
			wheels.Add(wheelRR.GetComponent<Wheel>());

			PlayMakerFSM[] playMakerFSMs = CarH.satsuma.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM fsm in playMakerFSMs)
			{
				if (fsm.FsmName == "SteerLimit")
				{
					steerLimit = fsm.FsmVariables.FindFsmFloat("MaxAngle");
					break;
				}
			}


			rallyFrontRate = suspension.FsmVariables.FindFsmFloat("RallyFrontRate");
			rallyRearRate = suspension.FsmVariables.FindFsmFloat("RallyRearRate");
		}

		// Update is called once per frame
		void Update()
		{
			if (CarH.running)
			{
				if (airride.action == Airride.Action.None)
				{
					if (mod.highestKeybind.GetKeybindUp())
					{
						airride.action = Airride.Action.Highest;
					}

					if (mod.lowestKeybind.GetKeybindUp())
					{
						airride.action = Airride.Action.Lowest;
					}

					if (mod.increaseKeybind.GetKeybind())
					{
						airride.action = Airride.Action.Increase;
					}

					if (mod.decreaseKeybind.GetKeybind())
					{
						airride.action = Airride.Action.Decrease;
					}

					if (mod.increaseKeybind.GetKeybindUp())
					{
						airride.CompressorSound(false);
					}

					if (mod.decreaseKeybind.GetKeybindUp())
					{
						airride.AirrideSound(false);
					}
				}


				if (airride.action != Airride.Action.None)
				{
					switch (airride.action)
					{
						case Airride.Action.Lowest:
							ToLowest();
							break;
						case Airride.Action.Highest:
							ToHighest();
							break;
						case Airride.Action.Increase:
							Increase();
							break;
						case Airride.Action.Decrease:
							Decrease();
							break;
					}
				}
			}
		}

		private const float maxRate = 25000;
		private const float minRate = 15000;
		private const float ratePerSecond = 5000;

		private const float maxPos = 0.12f;
		private const float minPos = 0f;
		private const float posPerSecond = 0.05f;

		public float rate = maxRate;
		public float pos = minPos;
		public float leftCamber = 0;
		public float rightCamber = 0;
		public float camberPerSecond = 5f;
		public float camberMax = 15;

		private float minSteerLimit = 5;
		private float maxSteerLimit = 33;
		private float steerLimitStep = 10;

		private void ToLowest()
		{
			if (!airride.atMin)
			{
				airride.atMax = false;
				airride.AirrideSound();
				rate = Mathf.Clamp(rate - ratePerSecond * Time.deltaTime, minRate, maxRate);
				pos = Mathf.Clamp(pos + posPerSecond * Time.deltaTime, minPos, maxPos);
				leftCamber = Mathf.Clamp(leftCamber - camberPerSecond * Time.deltaTime, -camberMax, 0);
				rightCamber = Mathf.Clamp(rightCamber + camberPerSecond * Time.deltaTime, 0, camberMax);

				steerLimit.Value = Mathf.Clamp(steerLimit.Value - steerLimitStep * Time.deltaTime, minSteerLimit,
					maxSteerLimit);
				SetPosition(pos);
				SetCamber(leftCamber, rightCamber);
				SetSuspensionRate(rate);

				if (pos == maxPos && rate == minRate && leftCamber == -camberMax && rightCamber == camberMax &&
				    steerLimit.Value == minSteerLimit)
				{
					airride.atMin = true;
					airride.AirrideSound(false);
					airride.action = Airride.Action.None;
				}

				return;
			}

			airride.action = Airride.Action.None;
		}


		private void ToHighest()
		{
			if (!airride.atMax)
			{
				airride.atMin = false;
				airride.CompressorSound();
				rate = Mathf.Clamp(rate + ratePerSecond * Time.deltaTime, minRate, maxRate);
				pos = Mathf.Clamp(pos - posPerSecond * Time.deltaTime, minPos, maxPos);
				leftCamber = Mathf.Clamp(leftCamber + camberPerSecond * Time.deltaTime, -camberMax, 0);
				rightCamber = Mathf.Clamp(rightCamber - camberPerSecond * Time.deltaTime, 0, camberMax);

				steerLimit.Value = Mathf.Clamp(steerLimit.Value + steerLimitStep * Time.deltaTime, minSteerLimit,
					maxSteerLimit);
				SetPosition(pos);
				SetCamber(leftCamber, rightCamber);
				SetSuspensionRate(rate);

				if (pos == minPos && rate == maxRate && leftCamber == 0 && rightCamber == 0 &&
				    steerLimit.Value == maxSteerLimit)
				{
					airride.atMax = true;
					airride.CompressorSound(false);
					airride.action = Airride.Action.None;
				}

				return;
			}

			airride.action = Airride.Action.None;
		}

		private void Increase()
		{
			if (!airride.atMax)
			{
				airride.atMin = false;
				airride.CompressorSound();
				rate = Mathf.Clamp(rate + ratePerSecond * Time.deltaTime, minRate, maxRate);
				pos = Mathf.Clamp(pos - posPerSecond * Time.deltaTime, minPos, maxPos);
				leftCamber = Mathf.Clamp(leftCamber + camberPerSecond * Time.deltaTime, -camberMax, 0);
				rightCamber = Mathf.Clamp(rightCamber - camberPerSecond * Time.deltaTime, 0, camberMax);

				steerLimit.Value = Mathf.Clamp(steerLimit.Value + steerLimitStep * Time.deltaTime, minSteerLimit,
					maxSteerLimit);
				SetPosition(pos);
				SetCamber(leftCamber, rightCamber);
				SetSuspensionRate(rate);

				if (pos == minPos && rate == maxRate && leftCamber == 0 && rightCamber == 0 &&
				    steerLimit.Value == maxSteerLimit)
				{
					airride.atMax = true;
					airride.CompressorSound(false);
				}
			}

			airride.action = Airride.Action.None;
		}

		private void Decrease()
		{
			if (!airride.atMin)
			{
				airride.atMax = false;
				airride.AirrideSound();
				rate = Mathf.Clamp(rate - ratePerSecond * Time.deltaTime, minRate, maxRate);
				pos = Mathf.Clamp(pos + posPerSecond * Time.deltaTime, minPos, maxPos);
				leftCamber = Mathf.Clamp(leftCamber - camberPerSecond * Time.deltaTime, -camberMax, 0);
				rightCamber = Mathf.Clamp(rightCamber + camberPerSecond * Time.deltaTime, 0, camberMax);

				steerLimit.Value = Mathf.Clamp(steerLimit.Value - steerLimitStep * Time.deltaTime, minSteerLimit,
					maxSteerLimit);
				SetPosition(pos);
				SetCamber(leftCamber, rightCamber);
				SetSuspensionRate(rate);

				if (pos == maxPos && rate == minRate && leftCamber == -camberMax && rightCamber == camberMax &&
				    steerLimit.Value == minSteerLimit)
				{
					airride.atMin = true;
					airride.AirrideSound(false);
				}
			}

			airride.action = Airride.Action.None;
		}

		private void SetSuspensionRate(float rate)
		{
			rallyFrontRate.Value = rate;
			rallyRearRate.Value = rate;
		}

		private void SetPosition(float position)
		{
			wheel_transforms.ForEach(delegate(Transform wheel)
			{
				wheel.localPosition = new Vector3(wheel.localPosition.x, position, wheel.localPosition.z);
			});
		}

		private void SetCamber(float leftCamber, float rightCamber)
		{
			wheels[0].camber = leftCamber;
			wheels[1].camber = leftCamber;

			wheels[2].camber = rightCamber;
			wheels[3].camber = rightCamber;
		}

		public void Init(Airride airride, DonnerTech_ECU_Mod mod)
		{
			this.airride = airride;
			this.mod = mod;
		}
	}
}