using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DonnerTech_ECU_Mod
{
    class PartBuySave
    {
#if DEBUG
        public bool bought_airrideFL { get; set; } = false;
        public bool boughtAwdGearbox { get; set; } = false;
        public bool boughtAwdDifferential { get; set; } = false;
        public bool boughtAwdPropshaft { get; set; } = false;
#endif
        public bool bought_rainLightSensorboard { get; set; } = false;
        public bool bought_reverseCamera { get; set; } = false;
        public bool boughtInfoPanel { get; set; } = false;  
        public bool boughtABSModule { get; set; } = false;
        public bool boughtESPModule { get; set; } = false;
        public bool boughtTCSModule { get; set; } = false;
        public bool boughtMountingPlate { get; set; } = false;
        public bool boughtCableHarness { get; set; } = false;
        public bool boughtSmartEngineModule { get; set; } = false;
        public bool boughtCruiseControlPanel { get; set; } = false;

        public bool bought_fuel_injectors_box { get; set; } = false;
        public bool bought_fuel_injection_manifold { get; set; } = false;
        public bool bought_fuel_pump_cover { get; set; } = false;
        public bool bought_fuel_rail { get; set; } = false;
        public bool bought_throttle_bodies_box { get; set; } = false;
        public bool bought_chip_programmer { get; set; } = false;
        public bool bought_electric_fuel_pump { get; set; } = false;
    }
}
