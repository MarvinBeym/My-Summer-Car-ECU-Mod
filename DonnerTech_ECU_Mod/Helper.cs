using MSCLoader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using HutongGames.PlayMaker;
using MscModApi.Caching;


namespace Tools
{
	public static class Helper
	{
		private static AudioSource dashButtonAudioSource;

		public static PlayMakerFSM FindFsmOnGameObject(GameObject gameObject, string fsmName)
		{
			foreach (PlayMakerFSM fSM in gameObject.GetComponents<PlayMakerFSM>())
			{
				if (fSM.FsmName == fsmName)
				{
					return fSM;
				}
			}

			return null;
		}

		public static bool CheckContainsState(PlayMakerFSM fSM, string stateName)
		{
			foreach (FsmState state in fSM.FsmStates)
			{
				if (state.Name == stateName)
				{
					return true;
				}
			}

			return false;
		}

		public static GameObject GetGameObjectFromFsm(GameObject fsmGameObject, string fsmToUse = "Data")
		{
			foreach (PlayMakerFSM fsm in fsmGameObject.GetComponents<PlayMakerFSM>())
			{
				if (fsm.FsmName == fsmToUse)
				{
					return fsm.FsmVariables.FindFsmGameObject("ThisPart").Value;
				}
			}

			Logger.New("Unable to find base gameobject on supplied fsm gameobject",
				fsmGameObject.name + "fsmToUse: " + fsmToUse);
			return null;
		}
	}
}