using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.fuelsystem
{
	class OriginalPartsSave
	{
		public bool racingCarb_installed = false;
		public Vector3 racingCarb_position = Vector3.zero;
		public Quaternion racingCarb_rotation = Quaternion.identity;

		public bool fuelPump_installed = false;
		public Vector3 fuelPump_position = Vector3.zero;
		public Quaternion fuelPump_rotation = Quaternion.identity;

		public bool distributor_installed = false;
		public Vector3 distributor_position = Vector3.zero;
		public Quaternion distributor_rotation = Quaternion.identity;

		public bool electrics_installed = false;
		public Vector3 electrics_position = Vector3.zero;
		public Quaternion electrics_rotation = Quaternion.identity;

		public bool fuelStrainer_installed = false;
		public Vector3 fuelStrainer_position = Vector3.zero;
		public Quaternion fuelStrainer_rotation = Quaternion.identity;
	}
}