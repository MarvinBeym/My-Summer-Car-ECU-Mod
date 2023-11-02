using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using HutongGames.PlayMaker;
using UnityEngine;

namespace MscModApi.Caching
{
	public class Game
	{
		private static FsmFloat _money;
        public static Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

        public static GameObject Find(string findText)
        {
            try
            {
                GameObject gameObject = gameObjects[findText];
                return gameObject;
            }
            catch
            {

            }

            GameObject foundObject = GameObject.Find(findText);
            gameObjects[findText] = foundObject;
            return foundObject;
        }
        public static float money
		{
			get
			{
				if (_money != null) return _money.Value;
				_money = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney");
				return (float) Math.Round(_money.Value, 1);
			}
			set
			{
				if (_money != null) _money.Value = value;
				_money = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerMoney");
				_money.Value = value;
			}
		}
	}
}