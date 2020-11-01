using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod.fuelsystem
{
    class OriginalPartsSave
    {
        public bool racingCarb_installed { get; set; } = false;
        public Vector3 racingCarb_position { get; set; } = Vector3.zero;
        public Quaternion racingCarb_rotation { get; set; } = Quaternion.identity;

        public bool fuelPump_installed { get; set; } = false;
        public Vector3 fuelPump_position { get; set; } = Vector3.zero;
        public Quaternion fuelPump_rotation { get; set; } = Quaternion.identity;

        public bool distributor_installed { get; set; } = false;
        public Vector3 distributor_position { get; set; } = Vector3.zero;
        public Quaternion distributor_rotation { get; set; } = Quaternion.identity;

        public bool electrics_installed { get; set; } = false;
        public Vector3 electrics_position { get; set; } = Vector3.zero;
        public Quaternion electrics_rotation { get; set; } = Quaternion.identity;

        public bool fuelStrainer_installed { get; set; } = false;
        public Vector3 fuelStrainer_position { get; set; } = Vector3.zero;
        public Quaternion fuelStrainer_rotation { get; set; } = Quaternion.identity;
    }
}
