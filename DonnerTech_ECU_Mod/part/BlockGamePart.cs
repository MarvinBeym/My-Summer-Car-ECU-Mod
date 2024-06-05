using System;
using HutongGames.PlayMaker;
using MSCLoader;
using MscModApi.Caching;
using MscModApi.Parts;
using MscModApi.Parts.ReplacePart;
using MscModApi.Tools;

namespace DonnerTech_ECU_Mod.part
{
	public class BlockGamePart : GamePart
	{
		protected new readonly bool simpleBoltedStateDetection;
		public BlockGamePart()
		{
			var mainFsmPartName = "Database/DatabaseMotor/Block";
			this.simpleBoltedStateDetection = true;
			InitEventStorage();
			mainFsmGameObject = Cache.Find(mainFsmPartName);
			if (!mainFsmGameObject)
			{
				throw new Exception($"Unable to find main fsm part GameObject using '{mainFsmPartName}'");
			}

			dataFsm = mainFsmGameObject.FindFsm("Data");
			if (!dataFsm)
			{
				throw new Exception($"Unable to find data fsm on GameObject with name '{mainFsmGameObject.name}'");
			}

			gameObject = dataFsm.FsmVariables.FindFsmGameObject("ThisPart").Value;
			if (!gameObject)
			{
				throw new Exception(
					$"Unable to find part GameObject on GameObject with name '{mainFsmGameObject.name}'");
			}

			boltedState = dataFsm.FsmVariables.FindFsmBool("Bolted");
			damagedState = dataFsm.FsmVariables.FindFsmBool("Damaged") ?? new FsmBool("Damaged");
			installedState = dataFsm.FsmVariables.FindFsmBool("Installed") ?? new FsmBool("Installed");
			purchasedState = dataFsm.FsmVariables.FindFsmBool("Purchased") ?? new FsmBool("Purchased");

			boltCheckFsm = gameObject.FindFsm("BoltCheck");

			if (!boltCheckFsm.Fsm.Initialized)
			{
				boltCheckFsm.InitializeFSM();
			}

			tightness = boltCheckFsm.FsmVariables.FindFsmFloat("Tightness");

			if (tightness == null)
			{
				throw new Exception($"Unable to find tightness on bolt check fsm of part '{gameObject.name}'");
			}

			if (boltedState != null)
			{
				if (simpleBoltedStateDetection)
				{
					SetupSimpleBoltedStateDetection();
				}
				else
				{
					SetupAdvancedBoltedStateDetection();
				}
			}
			else
			{
				boltedState = new FsmBool(); //Avoiding null
			}
		}

		public override void Uninstall()
		{
			//Do nothing
		}

		public override bool installBlocked
		{
			get => false;
		}
	}


}