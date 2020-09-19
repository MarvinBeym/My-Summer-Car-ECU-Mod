using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModApi;
using MSCLoader;
using System;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class ECU_InfoPanel_Logic : MonoBehaviour
    {
        private Mod mainMod;
        private DonnerTech_ECU_Mod donnerTech_ecu_mod;

        private GameObject infoPanel;
        private GameObject turnSignals;
        private GameObject beamShort;
        private GameObject beamLong;
        private GameObject powerON;
        private float blinkTimer = 0;
        private FsmFloat blinkDelay;
        private FsmBool blinkHazard;
        private FsmBool blinkLeft;
        private FsmBool blinkRight;
        private GameObject blinkerRight;
        private GameObject blinkerLeft;
        private GameObject combustion;
        private FsmInt odometerKM;
        private FsmFloat afRatio;

        private FsmFloat worldTime;
        private FsmFloat clockHours;
        private FsmFloat clockMinutes;

        private int currentPage = 0;
        private int[] pageIndex = new int[]
        {
            0,      //Main page
            1,      //Modules page
            2,      //Faults 1 page
            3,      //Faults 2 page
            4,      //Tuner page
            5,      //Turbocharger mod page
            6,      //Advanced assistance page
#if DEBUG
            7,      //Airride
#endif
        };
        //Animation
        private const float ecu_InfoPanel_Needle_maxAngle = 270;
        private const float ecu_InfoPanel_Needle_minAngle = 0;
        private float ecu_InfoPanel_Needle_maxRPM = 9000;
        private float rpmIncrementer = 0;
        private float rpmDecrementer = 9000;
        private float tenIncrementer = 0;
        private float hundredIncrementer = 0;
        private float thousandIncrementer = 0;

        //Fuel Calculation
        private float timerFuel = 0;
        private float fuelConsumption = 0;
        private float oldValue = 0;
        private float consumptionCounter = 0;
        private FsmFloat fuelLeft;

        //Display
        private bool isBooted = false;
        private bool isBooting = false;

        private SpriteRenderer ecu_InfoPanel_Needle;
        private SpriteRenderer ecu_InfoPanel_TurboWheel;
        private SpriteRenderer ecu_InfoPanel_Background;
        private SpriteRenderer ecu_InfoPanel_IndicatorLeft;
        private SpriteRenderer ecu_InfoPanel_IndicatorRight;
        private SpriteRenderer ecu_InfoPanel_Handbrake;
        private SpriteRenderer ecu_InfoPanel_LowBeam;
        private SpriteRenderer ecu_InfoPanel_HighBeam;

        private GameObject ecu_InfoPanel_NeedleObject;
        private GameObject ecu_InfoPanel_TurboWheelObject;

        //Reverse Camera stuff
        private MeshRenderer ecu_InfoPanel_Display_Reverse_Camera;

        //Airride stuff
        //camber -> wheelFL, .... 

        private ECU_Airride_Logic ecu_airride_logic;

        private string ecu_airride_operation = "";
        private float ecu_airride_timer = 0f;
        private FsmFloat travelRally;
        private FsmFloat rallyFrontRate;
        private FsmFloat rallyRearRate;
        private FsmFloat wheelPosRally;


        private float ecu_airride_wheelPosRally_min = -0.06f;
        private float ecu_airride_wheelPosRally_max = -0.18f;
        private float ecu_airride_wheelPosRally_default = -0.165f;

        //Lightsensor stuff
        private bool isNight = false;

        private TextMesh ecu_InfoPanel_Display_Value_01;
        private TextMesh ecu_InfoPanel_Display_Value_02;
        private TextMesh ecu_InfoPanel_Display_Value_03;
        private TextMesh ecu_InfoPanel_Display_Value_04;
        private TextMesh ecu_InfoPanel_Display_Value_05;
        private TextMesh ecu_InfoPanel_Display_Value_06;
        private TextMesh ecu_InfoPanel_Display_Value_07;
        private TextMesh ecu_InfoPanel_Display_Value_08;
        private TextMesh ecu_InfoPanel_Display_Value_09;
        private TextMesh ecu_InfoPanel_Display_Value_10;
        private TextMesh ecu_InfoPanel_Display_Value_11;
        private TextMesh ecu_InfoPanel_Display_Value_12;
        private TextMesh ecu_InfoPanel_Display_Value_13;
        private TextMesh ecu_InfoPanel_Display_Value_14;
        private TextMesh ecu_InfoPanel_Display_Value_15;
        private TextMesh ecu_InfoPanel_Display_Value_16;
        private TextMesh ecu_InfoPanel_Display_Gear;
        private TextMesh ecu_InfoPanel_Display_kmh;
        private TextMesh ecu_InfoPanel_Display_km;

        //Car
        private GameObject satsuma;
        private Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        private FsmString playerCurrentVehicle;
        private PlayMakerFSM carElectricsPower;
        private FsmFloat voltage;

        //Faults page (wear) (2)
        FsmFloat wearAlternator;
        FsmFloat wearClutch;
        FsmFloat wearCrankshaft;
        FsmFloat wearFuelpump;
        FsmFloat wearGearbox;
        FsmFloat wearHeadgasket;
        FsmFloat wearRockershaft;
        FsmFloat wearStarter;
        FsmFloat wearPiston1;
        FsmFloat wearPiston2;
        FsmFloat wearPiston3;
        FsmFloat wearPiston4;
        FsmFloat wearWaterpump;

        FsmFloat wearSpark1;
        FsmFloat wearSpark2;
        FsmFloat wearSpark3;
        FsmFloat wearSpark4;

        FsmFloat wearHeadlightBulbLeft;
        FsmFloat wearHeadlightBulbRight;


        FsmBool radiator_installed;
        FsmBool race_radiator_installed;
        FsmBool oilpan_installed;
        FsmBool fueltank_installed;
        FsmBool brakeMasterCylinder_installed;
        FsmBool clutchMasterCylinder_installed;
        FsmBool sparkplug1_installed;
        FsmBool sparkplug2_installed;
        FsmBool sparkplug3_installed;
        FsmBool sparkplug4_installed;
        FsmBool headlightBulbLeft_installed;
        FsmBool headlightBulbRight_installed;

        //Faults page (condition) (3)
        FsmFloat waterLevelRadiator;
        FsmFloat waterLevelRaceRadiator;
        FsmFloat oilLevel;
        FsmFloat oilContamination;
        FsmFloat oilGrade;
        FsmFloat fuelCoolantLevel;
        FsmFloat fuelDieselLevel;
        FsmFloat fuelGasolineLevel;
        FsmFloat fuelOilLevel;
        FsmFloat fuelUrineLevel;
        FsmFloat fuelLevel;
        FsmFloat brakeFluidF;
        FsmFloat brakeFluidR;
        FsmFloat clutchFluidLevel;


        //Tuner page (4)
        FsmBool rockershaft_installed;

        FsmFloat maxExhaust;
        FsmFloat minExhaust;

        FsmFloat maxIntake;
        FsmFloat minIntake;

        FsmFloat settingExhaustMax;
        FsmFloat settingExhaustMin;

        FsmFloat settingIntakeMax;
        FsmFloat settingIntakeMin;

        FsmFloat cylinder1exhaust;
        FsmFloat cylinder2exhaust;
        FsmFloat cylinder3exhaust;
        FsmFloat cylinder4exhaust;

        FsmFloat cylinder1intake;
        FsmFloat cylinder2intake;
        FsmFloat cylinder3intake;
        FsmFloat cylinder4intake;

        FsmBool raceCarb_installed;
        FsmFloat raceCarbAdjustAverage;
        FsmFloat raceCarbAdjust1;
        FsmFloat raceCarbAdjust2;
        FsmFloat raceCarbAdjust3;
        FsmFloat raceCarbAdjust4;
        FsmFloat raceCarbMin;
        FsmFloat raceCarbMax;

        FsmBool twinCarb_installed;
        FsmFloat twinCarbAdjust;
        FsmFloat twinCarbMax;
        FsmFloat twinCarbMin;

        FsmBool carb_installed;
        FsmFloat carbMax;
        FsmFloat carbMin;
        FsmFloat carbAdjust;

        FsmBool distributor_installed;
        FsmFloat distributorMaxAngle;
        FsmFloat distributorAngle;

        //Coolant
        private FsmFloat coolantTemp;
        private FsmFloat coolantPressurePSI;
        private FsmFloat coolantLevel;
        private RaycastHit hit;
        private AssetBundle assetBundle;

        private string selectededSetting = "";

        private string[] page1_GuiTexts = new string[]
        {
            "Enable ABS",
            "Enable ESP",
            "Enable TCS",
            "Enable 2StepRevLimiter",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "Enable Antilag",
            "",
            "",
            "Select 2Step RPM",
        };
        private string[] page4_GuiTexts = new string[]
        {
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "Start Autotune ECO",
            "Start Autotune RACE",
        };
        private string[] page6_GuiTexts = new string[]
        {
            "Enable Rainsensor",
            "Enable Lightsensor",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
        };
        private string[] page7_GuiTexts = new string[]
{
            "",
            "Lowest Pressure",
            "Highest Pressure",
            "Default Pressure",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
};

        //Tuner AutoTune
        private bool autoTune_running = false;
        private bool autoTuneRace_running = false;
        private bool autoTuneEco_running = false;
        private bool autoTune_done = false;
        private int autoTune_state = 0;
        private float autoTune_counter = 0;
        private float autoTune_timer = 0;

        //Assistancesystem
        private FsmFloat rainIntensity;
        PlayMakerFSM wiperLogicFSM;
        private bool rainsensor_enabled = false;
        private bool rainsensor_wasEnabled = false;


        private bool lightsensor_enabled = false;
        private bool lightsensor_wasEnabled = false;

        //ECU-Mod-Panel-Page0
        //ECU-Mod-Panel_Modules-Page1.png
        //ECU-Mod-Panel_Faults-Page2
        //ECU-Mod-Panel_Faults-Page3
        //ECU-Mod-Panel_Tuner-Page4
        //
        //
        //ECU-Panel Image Override
        private Sprite ecu_mod_panel_page0;
        private Sprite ecu_mod_panel_modules_page1;
        private Sprite ecu_mod_panel_faults_page2;
        private Sprite ecu_mod_panel_faults_page3;
        private Sprite ecu_mod_panel_tuner_page4;
        private Sprite ecu_mod_panel_turbo_page5;
        private Sprite ecu_mod_panel_assistance_page6;
        private Sprite ecu_mod_panel_airride_page7;

        private Sprite ecu_mod_panel_needle;
        private Sprite ecu_mod_panel_turbineWheel;
        private Sprite ecu_mod_panel_handbrake;
        private Sprite ecu_mod_panel_blinkerLeft;
        private Sprite ecu_mod_panel_blinkerRight;
        private Sprite ecu_mod_panel_highBeam;
        private Sprite ecu_mod_panel_lowBeam;


        //Turbocharger-Mod communication
        private PlayMakerFSM turbocharger_bigFSM;
        private FsmFloat turbocharger_big_rpm;
        private FsmFloat turbocharger_big_pressure;
        private FsmFloat turbocharger_big_max_boost;
        private FsmFloat turbocharger_big_wear;
        private FsmFloat turbocharger_big_exhaust_temp;
        private FsmFloat turbocharger_big_intake_temp;
        private FsmBool turbocharger_big_allInstalled;

        private PlayMakerFSM turbocharger_smallFSM;
        private FsmFloat turbocharger_small_rpm;
        private FsmFloat turbocharger_small_pressure;
        private FsmFloat turbocharger_small_max_boost;
        private FsmFloat turbocharger_small_wear;
        private FsmFloat turbocharger_small_exhaust_temp;
        private FsmFloat turbocharger_small_intake_temp;
        private FsmBool turbocharger_small_allInstalled;
        public int step2RevRpm = 6500;

        private void Start()
        {
            System.Collections.Generic.List<Mod> mods = ModLoader.LoadedMods;
            Mod[] modsArr = mods.ToArray();
            foreach(Mod mod in modsArr)
            {
                if(mod.Name == "DonnerTechRacing ECUs")
                {
                    mainMod = mod;
                    break;
                }
            }
            
            donnerTech_ecu_mod = (DonnerTech_ECU_Mod) mainMod;
            ecu_airride_logic = this.gameObject.AddComponent<ECU_Airride_Logic>();


            worldTime = PlayMakerGlobals.Instance.Variables.FindFsmFloat("GlobalTime");
            clockHours = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationHour");
            clockMinutes = PlayMakerGlobals.Instance.Variables.FindFsmFloat("TimeRotationMinute");
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");

            GameObject electrics = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics");
            PlayMakerFSM electricsFSM = electrics.GetComponent<PlayMakerFSM>();
            voltage = electricsFSM.FsmVariables.FindFsmFloat("Volts");
            GameObject dataBaseMechanics = GameObject.Find("Database/DatabaseMechanics");

            GameObject odometer = GameObject.Find("dashboard meters(Clone)/Gauges/Odometer");
            PlayMakerFSM odometerFSM = odometer.GetComponentInChildren<PlayMakerFSM>();
            odometerKM = odometerFSM.FsmVariables.FindFsmInt("OdometerReading");

            PlayMakerFSM[] dataBaseMechanicsFSMs = dataBaseMechanics.GetComponentsInChildren<PlayMakerFSM>();
            GameObject cooling = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Cooling").gameObject;
            PlayMakerFSM coolingFSM = PlayMakerFSM.FindFsmOnGameObject(cooling, "Cooling");
            coolantTemp = coolingFSM.FsmVariables.FindFsmFloat("CoolantTemp");
            coolantPressurePSI = coolingFSM.FsmVariables.FindFsmFloat("WaterPressurePSI");
            coolantLevel = coolingFSM.FsmVariables.FindFsmFloat("WaterLevel");
            //satsumaCarController.handbrakeInput


            foreach (PlayMakerFSM fsm in dataBaseMechanicsFSMs)
            {
                if (fsm.name == "FuelTank")
                {
                    fuelLeft = fsm.FsmVariables.FindFsmFloat("FuelLevel");
                    fuelCoolantLevel = fsm.FsmVariables.FindFsmFloat("FluidCoolant");
                    fuelDieselLevel = fsm.FsmVariables.FindFsmFloat("FluidDiesel");
                    fuelGasolineLevel = fsm.FsmVariables.FindFsmFloat("FluidGasoline");
                    fuelOilLevel = fsm.FsmVariables.FindFsmFloat("FluidOil");
                    fuelUrineLevel = fsm.FsmVariables.FindFsmFloat("FluidUrine");
                    fuelLevel = fsm.FsmVariables.FindFsmFloat("FuelLevel");
                    fueltank_installed = fsm.FsmVariables.FindFsmBool("Installed");
                    break;
                }
            }




            infoPanel = this.gameObject;
            ecu_InfoPanel_NeedleObject = GameObject.Find("ECU-Panel-Needle");
            ecu_InfoPanel_TurboWheelObject = GameObject.Find("ECU-Panel-TurboWheel");
            ecu_InfoPanel_Display_Reverse_Camera = GameObject.Find("ECU-Panel-Display-Reverse-Camera").GetComponent<MeshRenderer>();
            ecu_InfoPanel_Display_Reverse_Camera.enabled = false;
            TextMesh[] ecu_InfoPanel_TextMeshes = infoPanel.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in ecu_InfoPanel_TextMeshes)
            {
                switch (textMesh.name)
                {
                    case "ECU-Panel-Display-Value-01":
                        {
                            ecu_InfoPanel_Display_Value_01 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-02":
                        {
                            ecu_InfoPanel_Display_Value_02 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-03":
                        {
                            ecu_InfoPanel_Display_Value_03 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-04":
                        {
                            ecu_InfoPanel_Display_Value_04 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-05":
                        {
                            ecu_InfoPanel_Display_Value_05 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-06":
                        {
                            ecu_InfoPanel_Display_Value_06 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-07":
                        {
                            ecu_InfoPanel_Display_Value_07 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-08":
                        {
                            ecu_InfoPanel_Display_Value_08 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-09":
                        {
                            ecu_InfoPanel_Display_Value_09 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-10":
                        {
                            ecu_InfoPanel_Display_Value_10 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-11":
                        {
                            ecu_InfoPanel_Display_Value_11 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-12":
                        {
                            ecu_InfoPanel_Display_Value_12 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-13":
                        {
                            ecu_InfoPanel_Display_Value_13 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-14":
                        {
                            ecu_InfoPanel_Display_Value_14 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-15":
                        {
                            ecu_InfoPanel_Display_Value_15 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Value-16":
                        {
                            ecu_InfoPanel_Display_Value_16 = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-Gear":
                        {
                            ecu_InfoPanel_Display_Gear = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-km/h":
                        {
                            ecu_InfoPanel_Display_kmh = textMesh;
                            break;
                        }
                    case "ECU-Panel-Display-km":
                        {
                            ecu_InfoPanel_Display_km = textMesh;
                            break;
                        }
                }
                textMesh.gameObject.SetActive(false);
            }

            SpriteRenderer[] ecu_InfoPanel_SpriteRenderer = infoPanel.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in ecu_InfoPanel_SpriteRenderer)
            {
                switch (spriteRenderer.name)
                {
                    case "ECU-Panel-Needle":
                        {
                            ecu_InfoPanel_Needle = spriteRenderer;
                            break;
                        }
                    case "ECU-Panel-TurboWheel":
                        {
                            ecu_InfoPanel_TurboWheel = spriteRenderer;
                            break;
                        }
                    case "ECU-Panel-Background":
                        {
                            ecu_InfoPanel_Background = spriteRenderer;
                            break;
                        }
                    case "ECU-Panel-Indicator-Left":
                        {
                            ecu_InfoPanel_IndicatorLeft = spriteRenderer;
                            break;
                        }
                    case "ECU-Panel-Indicator-Right":
                        {
                            ecu_InfoPanel_IndicatorRight = spriteRenderer;
                            break;
                        }
                    case "ECU-Panel-Handbrake":
                        {
                            ecu_InfoPanel_Handbrake = spriteRenderer;
                            break;
                        }
                    case "ECU-Panel-LowBeam":
                        {
                            ecu_InfoPanel_LowBeam = spriteRenderer;
                            break;
                        }
                    case "ECU-Panel-HighBeam":
                        {
                            ecu_InfoPanel_HighBeam = spriteRenderer;
                            break;
                        }
                }
                spriteRenderer.enabled = false;
            }

            if (turnSignals == null)
            {
                turnSignals = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/TurnSignals");
                if (turnSignals != null)
                {
                    PlayMakerFSM blinkers = null;
                    PlayMakerFSM[] turnSignalComps = turnSignals.GetComponents<PlayMakerFSM>();
                    foreach(PlayMakerFSM turnSignalComp in turnSignalComps)
                    {
                        if(turnSignalComp.FsmName == "Blinkers")
                        {
                            blinkers = turnSignalComp;
                            break;
                        }
                    }
                    if( blinkers != null)
                    {
                        blinkDelay = blinkers.FsmVariables.FindFsmFloat("Delay");
                        blinkHazard = blinkers.FsmVariables.FindFsmBool("BlinkerHazards");
                        blinkLeft = blinkers.FsmVariables.FindFsmBool("BlinkerLeft");
                        blinkRight = blinkers.FsmVariables.FindFsmBool("BlinkerRight");
                    }
                    else
                    {
                        ModConsole.Error("Error while trying to find blinkers");
                    }

                }
            }



            if (powerON == null)
            {
                powerON = GameObject.Find("SATSUMA(557kg, 248)/Electricity/PowerON");
                
            }
            if(powerON != null)
            {
                for (int i = 0; i < powerON.transform.childCount; i++)
                {
                    GameObject tmp = powerON.transform.GetChild(i).gameObject;
                    if (tmp.name == "BeamsShort")
                    {
                        beamShort = tmp;
                    }
                    else if (tmp.name == "BeamsLong")
                    {
                        beamLong = tmp;
                    }
                }
            }

            PlayMakerFSM mechanicalWear = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/MechanicalWear").GetComponent<PlayMakerFSM>();

            wearAlternator = mechanicalWear.FsmVariables.FindFsmFloat("WearAlternator");
            wearClutch = mechanicalWear.FsmVariables.FindFsmFloat("WearClutch");
            wearCrankshaft = mechanicalWear.FsmVariables.FindFsmFloat("WearCrankshaft");
            wearFuelpump = mechanicalWear.FsmVariables.FindFsmFloat("WearFuelpump");
            wearGearbox = mechanicalWear.FsmVariables.FindFsmFloat("WearGearbox");
            wearHeadgasket = mechanicalWear.FsmVariables.FindFsmFloat("WearHeadgasket");
            wearPiston1 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston1");
            wearPiston2 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston2");
            wearPiston3 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston3");
            wearPiston4 = mechanicalWear.FsmVariables.FindFsmFloat("WearPiston4");
            wearRockershaft = mechanicalWear.FsmVariables.FindFsmFloat("WearRockershaft");
            wearStarter = mechanicalWear.FsmVariables.FindFsmFloat("WearStarter");
            wearWaterpump = mechanicalWear.FsmVariables.FindFsmFloat("WearWaterpump");

            PlayMakerFSM sparkPlug1FSM = GameObject.Find("Sparkplug1").GetComponent<PlayMakerFSM>();
            PlayMakerFSM sparkPlug2FSM = GameObject.Find("Sparkplug2").GetComponent<PlayMakerFSM>();
            PlayMakerFSM sparkPlug3FSM = GameObject.Find("Sparkplug3").GetComponent<PlayMakerFSM>();
            PlayMakerFSM sparkPlug4FSM = GameObject.Find("Sparkplug4").GetComponent<PlayMakerFSM>();

            wearSpark1 = sparkPlug1FSM.FsmVariables.FindFsmFloat("Wear");
            wearSpark2 = sparkPlug2FSM.FsmVariables.FindFsmFloat("Wear");
            wearSpark3 = sparkPlug3FSM.FsmVariables.FindFsmFloat("Wear");
            wearSpark4 = sparkPlug4FSM.FsmVariables.FindFsmFloat("Wear");

            sparkplug1_installed = sparkPlug1FSM.FsmVariables.FindFsmBool("Installed");
            sparkplug2_installed = sparkPlug2FSM.FsmVariables.FindFsmBool("Installed");
            sparkplug3_installed = sparkPlug3FSM.FsmVariables.FindFsmBool("Installed");
            sparkplug4_installed = sparkPlug4FSM.FsmVariables.FindFsmBool("Installed");

            PlayMakerFSM headlightBulbLeftFSM = GameObject.Find("HeadlightBulbLeft").GetComponent<PlayMakerFSM>();
            PlayMakerFSM headlightBulbRightFSM = GameObject.Find("HeadlightBulbRight").GetComponent<PlayMakerFSM>();

            wearHeadlightBulbLeft = headlightBulbLeftFSM.FsmVariables.FindFsmFloat("Wear");
            wearHeadlightBulbRight = headlightBulbRightFSM.FsmVariables.FindFsmFloat("Wear");

            headlightBulbLeft_installed = headlightBulbLeftFSM.FsmVariables.FindFsmBool("Installed");
            headlightBulbRight_installed = headlightBulbLeftFSM.FsmVariables.FindFsmBool("Installed");


            PlayMakerFSM race_radiatorFSM = GameObject.Find("Racing Radiator").GetComponent<PlayMakerFSM>();
            PlayMakerFSM radiatorFSM = GameObject.Find("Radiator").GetComponent<PlayMakerFSM>();
            PlayMakerFSM oilpanFSM = GameObject.Find("Oilpan").GetComponent<PlayMakerFSM>();
            PlayMakerFSM brakeMasterCylinderFSM = GameObject.Find("BrakeMasterCylinder").GetComponent<PlayMakerFSM>();
            PlayMakerFSM clutchMasterCylinderFSM = GameObject.Find("ClutchMasterCylinder").GetComponent<PlayMakerFSM>();
            waterLevelRaceRadiator = race_radiatorFSM.FsmVariables.FindFsmFloat("Water");
            waterLevelRadiator = radiatorFSM.FsmVariables.FindFsmFloat("Water");

            oilLevel = oilpanFSM.FsmVariables.FindFsmFloat("Oil");
            oilContamination = oilpanFSM.FsmVariables.FindFsmFloat("OilContamination");
            oilGrade = oilpanFSM.FsmVariables.FindFsmFloat("OilGrade");

            
            brakeFluidF = brakeMasterCylinderFSM.FsmVariables.FindFsmFloat("BrakeFluidF");
            brakeFluidR = brakeMasterCylinderFSM.FsmVariables.FindFsmFloat("BrakeFluidR");

            clutchFluidLevel = clutchMasterCylinderFSM.FsmVariables.FindFsmFloat("ClutchFluid");

            radiator_installed = radiatorFSM.FsmVariables.FindFsmBool("Installed");
            race_radiator_installed = race_radiatorFSM.FsmVariables.FindFsmBool("Installed");
            oilpan_installed = oilpanFSM.FsmVariables.FindFsmBool("Installed");
            brakeMasterCylinder_installed = brakeMasterCylinderFSM.FsmVariables.FindFsmBool("Installed");
            clutchMasterCylinder_installed = clutchMasterCylinderFSM.FsmVariables.FindFsmBool("Installed");

            PlayMakerFSM rockershaftFSM = GameObject.Find("RockerShaft").GetComponent<PlayMakerFSM>();
            
            rockershaft_installed = rockershaftFSM.FsmVariables.FindFsmBool("Installed");

            maxExhaust = rockershaftFSM.FsmVariables.FindFsmFloat("MaxExhaust");
            minExhaust = rockershaftFSM.FsmVariables.FindFsmFloat("MinExhaust");

            maxIntake = rockershaftFSM.FsmVariables.FindFsmFloat("MaxIntake");
            minIntake =rockershaftFSM.FsmVariables.FindFsmFloat("MinIntake");

            settingExhaustMax = rockershaftFSM.FsmVariables.FindFsmFloat("SettingExhaustMax");
            settingExhaustMin = rockershaftFSM.FsmVariables.FindFsmFloat("SettingExhaustMin");
            
            settingIntakeMax = rockershaftFSM.FsmVariables.FindFsmFloat("SettingIntakeMax");
            settingIntakeMin = rockershaftFSM.FsmVariables.FindFsmFloat("SettingIntakeMin");
            
            cylinder1exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl1exhaust");
            cylinder2exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl2exhaust");
            cylinder3exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl3exhaust");
            cylinder4exhaust = rockershaftFSM.FsmVariables.FindFsmFloat("cyl4exhaust");
            
            cylinder1intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl1intake");
            cylinder2intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl2intake");
            cylinder3intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl3intake");
            cylinder4intake = rockershaftFSM.FsmVariables.FindFsmFloat("cyl4intake");

            PlayMakerFSM carbFSM = GameObject.Find("Carburator").GetComponent<PlayMakerFSM>();
            carb_installed = carbFSM.FsmVariables.FindFsmBool("Installed");
            carbAdjust = carbFSM.FsmVariables.FindFsmFloat("IdleAdjust");
            carbMax = carbFSM.FsmVariables.FindFsmFloat("Max");
            carbMin = carbFSM.FsmVariables.FindFsmFloat("Min");




            PlayMakerFSM twinCarbFSM = GameObject.Find("Twin Carburators").GetComponent<PlayMakerFSM>();
            twinCarb_installed = twinCarbFSM.FsmVariables.FindFsmBool("Installed");
            twinCarbAdjust = twinCarbFSM.FsmVariables.FindFsmFloat("IdleAdjust");
            twinCarbMax = twinCarbFSM.FsmVariables.FindFsmFloat("Max");
            twinCarbMin = twinCarbFSM.FsmVariables.FindFsmFloat("Min");

            PlayMakerFSM raceCarbFSM = GameObject.Find("Racing Carburators").GetComponent<PlayMakerFSM>();
            raceCarb_installed = raceCarbFSM.FsmVariables.FindFsmBool("Installed");
            raceCarbAdjustAverage = raceCarbFSM.FsmVariables.FindFsmFloat("AdjustAverage");
            raceCarbAdjust1 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust1");
            raceCarbAdjust2 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust2");
            raceCarbAdjust3 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust3");
            raceCarbAdjust4 = raceCarbFSM.FsmVariables.FindFsmFloat("Adjust4");
            raceCarbMax = raceCarbFSM.FsmVariables.FindFsmFloat("Max");
            raceCarbMin = raceCarbFSM.FsmVariables.FindFsmFloat("Min");

            PlayMakerFSM distributorFSM = GameObject.Find("Distributor").GetComponent<PlayMakerFSM>();
            distributor_installed = distributorFSM.FsmVariables.FindFsmBool("Installed");
            distributorMaxAngle = distributorFSM.FsmVariables.FindFsmFloat("MaxAngle");
            distributorAngle = distributorFSM.FsmVariables.FindFsmFloat("SparkAngle");


            assetBundle = LoadAssets.LoadBundle(donnerTech_ecu_mod, "ecu-mod.unity3d");
            ecu_mod_panel_page0 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel-Page0.png");
            ecu_mod_panel_modules_page1 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel_Modules-Page1.png");
            ecu_mod_panel_faults_page2 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel_Faults-Page2.png");
            ecu_mod_panel_faults_page3 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel_Faults-Page3.png");
            ecu_mod_panel_tuner_page4 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel_Tuner-Page4.png");
            ecu_mod_panel_turbo_page5 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel-Turbocharger-Page5.png");
            ecu_mod_panel_assistance_page6 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel-Assistance-Page6.png");
            ecu_mod_panel_airride_page7 = assetBundle.LoadAsset<Sprite>("ECU-Mod-Panel-Airride-Page7.png");

            ecu_mod_panel_needle = assetBundle.LoadAsset<Sprite>("Rpm-Needle.png");
            ecu_mod_panel_turbineWheel = assetBundle.LoadAsset<Sprite>("TurbineWheel.png");

            ecu_mod_panel_handbrake = assetBundle.LoadAsset<Sprite>("Handbrake-Icon.png");
            ecu_mod_panel_blinkerLeft = assetBundle.LoadAsset<Sprite>("Indicator-Left-Icon.png");
            ecu_mod_panel_blinkerRight = assetBundle.LoadAsset<Sprite>("Indicator-Right-Icon.png");
            ecu_mod_panel_highBeam = assetBundle.LoadAsset<Sprite>("LowBeam-Icon.png");
            ecu_mod_panel_lowBeam = assetBundle.LoadAsset<Sprite>("Needle-Icon.png");

            rainIntensity = PlayMakerGlobals.Instance.Variables.FindFsmFloat("RainIntensity");
            GameObject buttonWipers = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)/pivot_meters/dashboard meters(Clone)/Knobs/ButtonsDash/ButtonWipers");
            PlayMakerFSM[] buttonWipersFSMs = buttonWipers.GetComponents<PlayMakerFSM>();
            foreach(PlayMakerFSM buttonWiperFSM in buttonWipersFSMs)
            {
                if(buttonWiperFSM.FsmName == "Function")
                {
                    wiperLogicFSM = buttonWiperFSM;
                    break;
                }
            }
            LoadECU_PanelImageOverride();
            assetBundle.Unload(false);

            PlayMakerFSM suspension = GameObject.Find("Suspension").GetComponent<PlayMakerFSM>();
            travelRally = suspension.FsmVariables.FindFsmFloat("TravelRally");
            wheelPosRally = suspension.FsmVariables.FindFsmFloat("WheelPosRally");

            rallyFrontRate = suspension.FsmVariables.FindFsmFloat("RallyFrontRate");
            rallyRearRate = suspension.FsmVariables.FindFsmFloat("RallyRearRate");

            FsmHook.FsmInject(GameObject.Find("StreetLights"), "Day", SwitchToDay);
            FsmHook.FsmInject(GameObject.Find("StreetLights"), "Night", SwitchToNight);
        }

        private void SwitchToDay()
        {
            isNight = false;
        }
        private void SwitchToNight()
        {
            isNight = true;
        }
        private void LoadECU_PanelImageOverride()
        {
            Sprite loadedSprite;
            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel-Page0.png"));
            if(loadedSprite != null)
            {
                ecu_mod_panel_page0 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel_Modules-Page1.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_modules_page1 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel_Faults-Page2.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_faults_page2 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel_Faults-Page3.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_faults_page3 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel_Tuner-Page4.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_tuner_page4 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel-Turbocharger-Page5.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_turbo_page5 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel-Assistance-Page6.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_assistance_page6 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "ECU-Mod-Panel-Airride-Page7.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_airride_page7 = loadedSprite;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "Handbrake-Icon.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_handbrake = loadedSprite;
                ecu_InfoPanel_Handbrake.sprite = ecu_mod_panel_handbrake;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "Indicator-Left-Icon.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_blinkerLeft = loadedSprite;
                ecu_InfoPanel_IndicatorLeft.sprite = ecu_mod_panel_blinkerLeft;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "Indicator-Right-Icon.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_blinkerRight = loadedSprite;
                ecu_InfoPanel_IndicatorRight.sprite = ecu_mod_panel_blinkerRight;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "LowBeam-Icon.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_lowBeam = loadedSprite;
                ecu_InfoPanel_LowBeam.sprite = ecu_mod_panel_lowBeam;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "HighBeam-Icon.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_highBeam = loadedSprite;
                ecu_InfoPanel_HighBeam.sprite = ecu_mod_panel_highBeam;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "Rpm-Needle.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_needle = loadedSprite;
                ecu_InfoPanel_Needle.sprite = ecu_mod_panel_needle;
            }

            loadedSprite = LoadNewSprite(Path.Combine(ModLoader.GetModAssetsFolder(donnerTech_ecu_mod), "OVERRIDE" + "_" + "TurbineWheel.png"));
            if (loadedSprite != null)
            {
                ecu_mod_panel_turbineWheel = loadedSprite;
                ecu_InfoPanel_TurboWheel.sprite = ecu_mod_panel_turbineWheel;
            }

        }
        public Sprite LoadNewSprite(string FilePath, float pivotX = 0.5f, float pivotY = 0.5f, float PixelsPerUnit = 100.0f)
        {
            if (File.Exists(FilePath) && Path.GetExtension(FilePath) == ".png")
            {
                Sprite NewSprite = new Sprite();
                Texture2D SpriteTexture = LoadTexture(FilePath);
                NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(pivotX, pivotY), PixelsPerUnit);

                return NewSprite;
            }
            else
            {
                return null;
            }
            
        }
        public Texture2D LoadTexture(string FilePath)
        {

            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
                if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                    return Tex2D;                 // If data = readable -> return texture
            }
            return null;                     // Return null if load failed
        }

        void Update()
        {
            if (ModLoader.IsModPresent("SatsumaTurboCharger"))
            {
                if (turbocharger_bigFSM == null)
                {
                    try
                    {
                        GameObject racingTurbo = GameObject.Find("Racing Turbocharger(Clone)");
                        if (racingTurbo != null)
                        {
                            turbocharger_bigFSM = racingTurbo.GetComponent<PlayMakerFSM>();
                            if (turbocharger_bigFSM != null)
                            {
                                turbocharger_big_rpm = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Rpm");
                                turbocharger_big_pressure = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Pressure");
                                turbocharger_big_max_boost = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Max boost");
                                turbocharger_big_wear = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Wear");
                                turbocharger_big_exhaust_temp = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Exhaust temperature");
                                turbocharger_big_intake_temp = turbocharger_bigFSM.FsmVariables.FindFsmFloat("Intake temperature");
                                turbocharger_big_allInstalled = turbocharger_bigFSM.FsmVariables.FindFsmBool("All installed");
                            }
                        }

                    }
                    catch
                    {

                    }
                }
                if (turbocharger_smallFSM == null)
                {
                    try
                    {
                        GameObject gtTurbo = GameObject.Find("GT Turbocharger(Clone)");
                        if (gtTurbo != null)
                        {
                            turbocharger_smallFSM = gtTurbo.GetComponent<PlayMakerFSM>();
                            if (turbocharger_smallFSM != null)
                            {
                                turbocharger_small_rpm = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Rpm");
                                turbocharger_small_pressure = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Pressure");
                                turbocharger_small_max_boost = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Max boost");
                                turbocharger_small_wear = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Wear");
                                turbocharger_small_exhaust_temp = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Exhaust temperature");
                                turbocharger_small_intake_temp = turbocharger_smallFSM.FsmVariables.FindFsmFloat("Intake temperature");
                                turbocharger_small_allInstalled = turbocharger_smallFSM.FsmVariables.FindFsmBool("All installed");
                            }
                        }

                    }
                    catch
                    {

                    }
                }
            }
            

            //ecu_InfoPanel_Background.sprite = ecu_mod_panel_page0;
            if (combustion == null)
            {
                combustion = GameObject.Find("CarSimulation/Engine/Combustion");
                if(combustion != null)
                {
                    PlayMakerFSM[] playMakerFSMs = combustion.GetComponents<PlayMakerFSM>();
                    foreach (PlayMakerFSM fsm in playMakerFSMs)
                    {
                        if (fsm.FsmName == "Cylinders")
                        {
                            afRatio = fsm.FsmVariables.FindFsmFloat("AFR");
                            break;
                        }
                    }
                }
            }

            if (hasPower && donnerTech_ecu_mod.GetInfoPanelScrewed())
            {
                if (!isBooted)
                {
                    HandleBootAnimation();
                }
                else
                {
                    HandleKeybinds();
                    HandleButtonPresses();

                    if (true/*donnerTech_ecu_mod.GetAirrideInstalledScrewed()*/)
                    {
                        HandleAirride();
                    } 

                    HandleReverseCamera();
                    if (rainsensor_enabled)
                        HandleRainsensorLogic();
                    else if (rainsensor_wasEnabled)
                    {
                        FsmBool wiperOn = wiperLogicFSM.FsmVariables.FindFsmBool("On");
                        FsmFloat wiperDelay = wiperLogicFSM.FsmVariables.FindFsmFloat("Delay");
                        wiperOn.Value = false;
                        wiperDelay.Value = 0f;
                        rainsensor_wasEnabled = false;
                    }
                    if (lightsensor_enabled)
                        HandleLightsensorLogic();
                    else if (lightsensor_wasEnabled)
                    {

                        lightsensor_wasEnabled = false;
                    }


                    DisplayGeneralInfomation();
                    if (currentPage == 0)
                    {
                        DisplayPage0Values();
                    }
                    else if(currentPage == 1)
                    {
                        HandleTouchPresses(page1_GuiTexts);
                        DisplayPage1Values();
                        
                    }
                    else if(currentPage == 2)
                    {
                        DisplayPage2Values();
                    }
                    else if(currentPage == 3)
                    {
                        DisplayPage3Values();
                    }
                    else if(currentPage == 4)
                    {
                        HandleTouchPresses(page4_GuiTexts);
                        DisplayPage4Values();
                        if (autoTuneEco_running)
                        {
                            RunEcoCalibrate();
                        }
                        else if (autoTuneRace_running)
                        {
                            RunRaceCalibrate();
                        }
                    }
                    else if(currentPage == 5)
                    {
                        DisplayPage5Values();
                    }
                    else if(currentPage == 6)
                    {
                        HandleTouchPresses(page6_GuiTexts);
                        DisplayPage6Values();
                    }
                    else if (currentPage == 7)
                    {
                        HandleTouchPresses(page7_GuiTexts);
                        DisplayPage7Values();
                    }
                }
            }
            else
            {
                try
                {
                    ecu_InfoPanel_NeedleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, 0, 0));
                    rpmIncrementer = 0;
                    rpmDecrementer = 9000;

                    tenIncrementer = 0;
                    hundredIncrementer = 0;
                    thousandIncrementer = 0;

                    ecu_InfoPanel_Needle.enabled = false;
                    ecu_InfoPanel_TurboWheel.enabled = false;
                    ecu_InfoPanel_Background.enabled = false;
                    ecu_InfoPanel_IndicatorLeft.enabled = false;
                    ecu_InfoPanel_IndicatorRight.enabled = false;
                    ecu_InfoPanel_Handbrake.enabled = false;
                    ecu_InfoPanel_LowBeam.enabled = false;
                    ecu_InfoPanel_HighBeam.enabled = false;
                    ecu_InfoPanel_Display_Value_01.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_02.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_03.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_04.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_05.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_06.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_07.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_08.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_09.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_10.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_11.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_12.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_13.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_14.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_15.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_16.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Gear.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_kmh.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_km.gameObject.SetActive(false);
                    ecu_InfoPanel_Display_Value_01.text = "";
                    ecu_InfoPanel_Display_Value_02.text = "";
                    ecu_InfoPanel_Display_Value_03.text = "";
                    ecu_InfoPanel_Display_Value_04.text = "";
                    ecu_InfoPanel_Display_Value_05.text = "";
                    ecu_InfoPanel_Display_Value_06.text = "";
                    ecu_InfoPanel_Display_Value_07.text = "";
                    ecu_InfoPanel_Display_Value_08.text = "";
                    ecu_InfoPanel_Display_Value_09.text = "";
                    ecu_InfoPanel_Display_Value_10.text = "";
                    ecu_InfoPanel_Display_Value_11.text = "";
                    ecu_InfoPanel_Display_Value_12.text = "";
                    ecu_InfoPanel_Display_Value_13.text = "";
                    ecu_InfoPanel_Display_Value_14.text = "";
                    ecu_InfoPanel_Display_Value_15.text = "";
                    ecu_InfoPanel_Display_Value_16.text = "";
                    ecu_InfoPanel_Display_Gear.text = "";
                    ecu_InfoPanel_Display_kmh.text = "";
                    ecu_InfoPanel_Display_km.text = "";

                    
                    ecu_InfoPanel_Display_Value_01.color = Color.white;
                    ecu_InfoPanel_Display_Value_02.color = Color.white;
                    ecu_InfoPanel_Display_Value_03.color = Color.white;
                    ecu_InfoPanel_Display_Value_04.color = Color.white;
                    ecu_InfoPanel_Display_Value_05.color = Color.white;
                    ecu_InfoPanel_Display_Value_06.color = Color.white;
                    ecu_InfoPanel_Display_Value_07.color = Color.white;
                    ecu_InfoPanel_Display_Value_08.color = Color.white;
                    ecu_InfoPanel_Display_Value_09.color = Color.white;
                    ecu_InfoPanel_Display_Value_10.color = Color.white;
                    ecu_InfoPanel_Display_Value_11.color = Color.white;
                    ecu_InfoPanel_Display_Value_12.color = Color.white;
                    ecu_InfoPanel_Display_Value_13.color = Color.white;
                    ecu_InfoPanel_Display_Value_14.color = Color.white;
                    ecu_InfoPanel_Display_Value_15.color = Color.white;
                    ecu_InfoPanel_Display_Value_16.color = Color.white;


                    autoTune_running = false;
                    autoTuneEco_running = false;
                    autoTuneRace_running = false;
                    autoTune_counter = 0;
                    autoTune_state = 0;
                    autoTune_done = false;
                    autoTune_timer = 0;
                    isBooting = false;
                    isBooted = false;
                    currentPage = 0;
                }
                catch
                {

                }

            }
            
        }

        private float counter = 0;
        private void HandleAirride()
        {
            
        }
        private void HandleReverseCamera()
        {
            if (donnerTech_ecu_mod.GetReverseCameraInstalledScrewed())
            {
                if (satsumaDriveTrain.gear == 0)
                {
                    ecu_InfoPanel_Display_Reverse_Camera.enabled = true;
                    donnerTech_ecu_mod.SetReverseCameraEnabled(true);
                }
                else
                {
                    ecu_InfoPanel_Display_Reverse_Camera.enabled = false;
                    donnerTech_ecu_mod.SetReverseCameraEnabled(false);
                }
            }
            else
            {
                ecu_InfoPanel_Display_Reverse_Camera.enabled = false;
                donnerTech_ecu_mod.SetReverseCameraEnabled(false);
            }
            
        }

        private void HandleRainsensorLogic()
        {
            rainsensor_wasEnabled = true;
            FsmBool wiperOn = wiperLogicFSM.FsmVariables.FindFsmBool("On");
            FsmFloat wiperDelay = wiperLogicFSM.FsmVariables.FindFsmFloat("Delay");
            if (rainsensor_enabled)
            {
                if (rainIntensity.Value >= 0.5f)
                {
                    wiperOn.Value = true;
                    wiperDelay.Value = 0f;
                }
                else if (rainIntensity.Value > 0f)
                {
                    wiperOn.Value = true;
                    wiperDelay.Value = 3f;
                }
                else
                {
                    wiperOn.Value = false;
                    wiperDelay.Value = 0f;
                }
            }
            else
            {
                wiperOn.Value = false;
                wiperDelay.Value = 0f;
            }

        }

        private void HandleLightsensorLogic()
        {
            lightsensor_wasEnabled = true;
            if (lightsensor_enabled)
            {

            }
            else
            {

            }
        }

        private void HandleBootAnimation()
        {
            if (isBooting)
            {
                Play_ECU_InfoPanel_Animation();
            }
            else
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_page0;

                ecu_InfoPanel_Needle.enabled = true;
                ecu_InfoPanel_TurboWheel.enabled = false;
                ecu_InfoPanel_Background.enabled = true;
                ecu_InfoPanel_IndicatorLeft.enabled = false;
                ecu_InfoPanel_IndicatorRight.enabled = false;
                ecu_InfoPanel_Handbrake.enabled = false;
                ecu_InfoPanel_LowBeam.enabled = false;
                ecu_InfoPanel_HighBeam.enabled = false;
                ecu_InfoPanel_Display_Value_01.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_02.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_03.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_04.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_05.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_06.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_07.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_08.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_09.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_10.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_11.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_12.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_13.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_14.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_15.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_16.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Gear.gameObject.SetActive(true);
                ecu_InfoPanel_Display_kmh.gameObject.SetActive(true);
                ecu_InfoPanel_Display_km.gameObject.SetActive(true);
                ecu_InfoPanel_Display_Value_01.text = "";
                ecu_InfoPanel_Display_Value_02.text = "";
                ecu_InfoPanel_Display_Value_03.text = "";
                ecu_InfoPanel_Display_Value_04.text = "";
                ecu_InfoPanel_Display_Value_05.text = "";
                ecu_InfoPanel_Display_Value_06.text = "";
                ecu_InfoPanel_Display_Value_07.text = "";
                ecu_InfoPanel_Display_Value_08.text = "";
                ecu_InfoPanel_Display_Value_09.text = "";
                ecu_InfoPanel_Display_Value_10.text = "";
                ecu_InfoPanel_Display_Value_11.text = "";
                ecu_InfoPanel_Display_Value_12.text = "";
                ecu_InfoPanel_Display_Value_13.text = "";
                ecu_InfoPanel_Display_Value_14.text = "";
                ecu_InfoPanel_Display_Value_15.text = "";
                ecu_InfoPanel_Display_Value_16.text = "";
                ecu_InfoPanel_Display_Gear.text = "";
                ecu_InfoPanel_Display_kmh.text = "";
                ecu_InfoPanel_Display_km.text = "";

                isBooting = true;
            }
        }

        private string ConvertToDigitalTime(float hours, float minutes)
        {
            string hour = ((360 - hours) / 30f + 2f).ToString("00");
            string minute = (Mathf.FloorToInt((360f - minutes) / 6f)).ToString("00");
            return (hour + ":" + minute);
        }

        private void HandleKeybinds()
        {
            if(playerCurrentVehicle.Value == "Satsuma" && hasPower)
            {
                if (donnerTech_ecu_mod.ecu_panel_ArrowUp.GetKeybindDown())
                {
                    Pressed_Button_ArrowUp();
                }
                else if (donnerTech_ecu_mod.ecu_panel_ArrowDown.GetKeybindDown())
                {
                    Pressed_Button_ArrowUp();
                }
                else if (donnerTech_ecu_mod.ecu_panel_Circle.GetKeybindDown())
                {
                    Pressed_Button_Circle();
                }
                else if (donnerTech_ecu_mod.ecu_panel_Cross.GetKeybindDown())
                {
                    Pressed_Button_Cross();
                }
                else if (donnerTech_ecu_mod.ecu_panel_Plus.GetKeybindDown())
                {
                    Pressed_Button_Plus();
                }
                else if (donnerTech_ecu_mod.ecu_panel_Minus.GetKeybindDown())
                {
                    Pressed_Button_Minus();
                }
            }
        }

        private void HandleTouchPresses(string[] guiTexts)
        {
            if (Camera.main != null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f, 1 << LayerMask.NameToLayer("DontCollide")) != false)
                {
                    GameObject gameObjectHit;
                    bool foundObject = false;
                    string guiText = "";
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null)
                    {
                        string valueToPass = null;
                        if (gameObjectHit.name == "ECU-Panel-Display-Value-01")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[0];
                            guiText = guiTexts[0];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-02")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[1];
                            guiText = guiTexts[1];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-03")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[2];
                            guiText = guiTexts[2];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-04")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[3];
                            guiText = guiTexts[3];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-05")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[4];
                            guiText = guiTexts[4];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-06")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[5];
                            guiText = guiTexts[5];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-07")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[6];
                            guiText = guiTexts[6];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-08")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[7];
                            guiText = guiTexts[7];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-09")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[8];
                            guiText = guiTexts[8];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-10")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[9];
                            guiText = guiTexts[9];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-11")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[10];
                            guiText = guiTexts[10];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-12")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[11];
                            guiText = guiTexts[11];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-13")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[12];
                            guiText = guiTexts[12];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-14")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[13];
                            guiText = guiTexts[13];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-15")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[14];
                            guiText = guiTexts[14];
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Display-Value-16")
                        {
                            foundObject = true;
                            valueToPass = guiTexts[15];
                            guiText = guiTexts[15];
                        }


                        if (foundObject)
                        {
                            ModClient.guiInteract(guiText);
                            if (useButtonDown || Input.GetMouseButtonDown(0))
                            {
                                Pressed_Display_Value(valueToPass, gameObjectHit);
                            }
                        }
                    }


                }
            }
        }

        private void Pressed_Display_Value(string value, GameObject gameObjectHit)
        {

            bool playSound = true;
            switch (value)
            {
                case "Enable ABS":
                    {
                        DonnerTech_ECU_Mod.ToggleABS();
                        break;
                    }
                case "Enable ESP":
                    {
                        DonnerTech_ECU_Mod.ToggleESP();
                        break;
                    }
                case "Enable TCS":
                    {
                        DonnerTech_ECU_Mod.ToggleTCS();
                        break;
                    }
                case "Enable 2StepRevLimiter":
                    {
                        donnerTech_ecu_mod.Toggle2StepRevLimiter();
                        break;
                    }
                case "Enable Antilag":
                    {
                        donnerTech_ecu_mod.ToggleALS();
                        break;
                    }
                case "Start Autotune ECO":
                    {
                        autoTuneEco_running = true;
                        RunEcoCalibrate();
                        break;
                    }
                case "Start Autotune RACE":
                    {
                        autoTuneRace_running = true;
                        RunRaceCalibrate();
                        break;
                    }
                case "Select 2Step RPM":
                    {
                        if(selectededSetting == "Select 2Step RPM")
                        {
                            selectededSetting = "";
                        }
                        else
                        {
                            selectededSetting = "Select 2Step RPM";
                        }
                        break;
                    }
                case "Enable Rainsensor":
                    {
                        rainsensor_enabled = !rainsensor_enabled;
                        break;
                    }
                case "Enable Lightsensor":
                    {
                        lightsensor_enabled = !lightsensor_enabled;
                        break;
                    }
                case "Lowest Pressure":
                    {
                        //ecu_airride_logic.increaseAirride(true, true, 0.05f);
                        ecu_airride_operation = "to lowest";
                        break;
                    }
                case "Highest Pressure":
                    {
                        ecu_airride_operation = "to highest";
                        break;
                    }
                case "Default Pressure":
                    {
                        ecu_airride_operation = "to default";
                        break;
                    }
                default:
                    {
                        playSound = false;
                        break;
                    }
            }

            if (playSound)
            {
                AudioSource audio = dashButtonAudioSource;
                audio.transform.position = gameObjectHit.transform.position;
                audio.Play();
            }
        }

        private void RunRaceCalibrate()
        {
            if (!autoTune_done)
            {
                autoTuneRace_running = true;
                autoTune_timer += Time.deltaTime;
                if (autoTune_state < 3)
                {
                    RunBaseCalibrate();
                }
                else if (autoTune_state != 6)
                {
                    RunCarbCalibrate(16.7f);
                }
                else if (autoTune_state == 6)
                {
                    autoTune_done = true;
                }
            }
            else
            {
                autoTune_running = false;
                autoTuneRace_running = false;
                autoTune_done = false;
                autoTune_state = 0;
                autoTune_counter = 0;
                autoTune_timer = 0;
            }
        }

        private void RunEcoCalibrate()
        {
            if (!autoTune_done)
            {
                autoTuneEco_running = true;
                autoTune_timer += Time.deltaTime;
                if (autoTune_state < 3)
                {
                    RunBaseCalibrate();
                }
                else if(autoTune_state != 6)
                {
                    RunCarbCalibrate(15.5f);
                }
                else if(autoTune_state == 6)
                {
                    autoTune_done = true;
                }
            }
            else
            {
                autoTune_running = false;
                autoTuneEco_running = false;
                autoTune_done = false;
                autoTune_state = 0;
                autoTune_counter = 0;
                autoTune_timer = 0;
            }
        }

        private void RunCarbCalibrate(float desiredSetting)
        {
            const float step = 0.2f;
            if (autoTune_timer >= 0.2)
            {
                autoTune_timer = 0;
                if (autoTune_state == 3)
                {
                    autoTune_counter = 10;
                    if (carb_installed.Value)
                    {
                        carbAdjust.Value = autoTune_counter;
                    }
                    else if (twinCarb_installed.Value)
                    {
                        twinCarbAdjust.Value = autoTune_counter;
                    }
                    else if (raceCarb_installed.Value)
                    {
                        raceCarbAdjust1.Value = autoTune_counter;
                        raceCarbAdjust2.Value = autoTune_counter;
                        raceCarbAdjust3.Value = autoTune_counter;
                        raceCarbAdjust4.Value = autoTune_counter;
                        raceCarbAdjustAverage.Value = autoTune_counter;
                    }
                    autoTune_state = 4;
                }
                else if (autoTune_state == 4)
                {
                    if (autoTune_counter < 22)
                    {
                        autoTune_counter += step;
                        if (carb_installed.Value)
                        {
                            carbAdjust.Value = autoTune_counter;
                        }
                        else if (twinCarb_installed.Value)
                        {
                            twinCarbAdjust.Value = autoTune_counter;
                        }
                        else if (raceCarb_installed.Value)
                        {
                            raceCarbAdjust1.Value = autoTune_counter;
                            raceCarbAdjust2.Value = autoTune_counter;
                            raceCarbAdjust3.Value = autoTune_counter;
                            raceCarbAdjust4.Value = autoTune_counter;
                            raceCarbAdjustAverage.Value = autoTune_counter;
                        }
                    }
                    else
                    {
                        autoTune_state = 5;
                    }
                }
                else if (autoTune_state == 5)
                {
                    autoTune_counter -= step;
                    if (carb_installed.Value)
                    {
                        carbAdjust.Value = autoTune_counter;
                    }
                    else if (twinCarb_installed.Value)
                    {
                        twinCarbAdjust.Value = autoTune_counter;
                    }
                    else if (raceCarb_installed.Value)
                    {
                        raceCarbAdjust1.Value = autoTune_counter;
                        raceCarbAdjust2.Value = autoTune_counter;
                        raceCarbAdjust3.Value = autoTune_counter;
                        raceCarbAdjust4.Value = autoTune_counter;
                        raceCarbAdjustAverage.Value = autoTune_counter;
                    }


                    if (autoTune_counter <= desiredSetting)
                    {
                        if (carb_installed.Value)
                        {
                            carbAdjust.Value = desiredSetting;
                        }
                        else if (twinCarb_installed.Value)
                        {
                            twinCarbAdjust.Value = desiredSetting;
                        }
                        else if (raceCarb_installed.Value)
                        {
                            raceCarbAdjust1.Value = desiredSetting;
                            raceCarbAdjust2.Value = desiredSetting;
                            raceCarbAdjust3.Value = desiredSetting;
                            raceCarbAdjust4.Value = desiredSetting;
                            raceCarbAdjustAverage.Value = desiredSetting;
                        }
                        autoTune_state = 6;
                    }
                }
            }
        }

        private void RunBaseCalibrate()
        {
            const int minValue = 3;
            const int maxValue = 9;
            const int idealExhaustValue = 7;
            const int idealIntakeValue = 8;
            const float step = 0.2f;
            if (autoTune_timer >= 0.2)
            {
                autoTune_timer = 0;
                if (autoTune_state == 0)
                {
                    autoTune_counter = minValue;
                    cylinder1exhaust.Value = autoTune_counter;
                    cylinder2exhaust.Value = autoTune_counter;
                    cylinder3exhaust.Value = autoTune_counter;
                    cylinder4exhaust.Value = autoTune_counter;

                    cylinder1intake.Value = autoTune_counter;
                    cylinder2intake.Value = autoTune_counter;
                    cylinder3intake.Value = autoTune_counter;
                    cylinder4intake.Value = autoTune_counter;

                    autoTune_state = 1;
                }
                else if (autoTune_state == 1)
                {
                    if (autoTune_counter < maxValue)
                    {
                        autoTune_counter += step;
                        cylinder1exhaust.Value = autoTune_counter;
                        cylinder2exhaust.Value = autoTune_counter;
                        cylinder3exhaust.Value = autoTune_counter;
                        cylinder4exhaust.Value = autoTune_counter;
                        cylinder1intake.Value = autoTune_counter;
                        cylinder2intake.Value = autoTune_counter;
                        cylinder3intake.Value = autoTune_counter;
                        cylinder4intake.Value = autoTune_counter;
                    }
                    else
                    {
                        autoTune_state = 2;
                    }
                }
                else if (autoTune_state == 2)
                {
                    if (autoTune_counter <= idealIntakeValue)
                    {
                        cylinder1intake.Value = idealIntakeValue;
                        cylinder2intake.Value = idealIntakeValue;
                        cylinder3intake.Value = idealIntakeValue;
                        cylinder4intake.Value = idealIntakeValue;
                    }
                    if (autoTune_counter <= idealExhaustValue)
                    {
                        cylinder1exhaust.Value = idealExhaustValue;
                        cylinder2exhaust.Value = idealExhaustValue;
                        cylinder3exhaust.Value = idealExhaustValue;
                        cylinder4exhaust.Value = idealExhaustValue;
                        autoTune_state = 3;
                    }
                    else
                    {
                        autoTune_counter -= step;
                    }
                }
            }
        }

        private void HandleButtonPresses()
        {
            if (Camera.main != null)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 1f, 1 << LayerMask.NameToLayer("DontCollide")) != false)
                {
                    /*
                    ECU-Panel-Button-ArrowDown
                    ECU-Panel-Button-ArrowUp

                    ECU-Panel-Button-Circle
                    ECU-Panel-Button-Cross
                    ECU-Panel-Button-Minus
                    ECU-Panel-Button-Plus
                    */
                    GameObject gameObjectHit;
                    bool foundObject = false;
                    string guiText = "";
                    gameObjectHit = hit.collider?.gameObject;
                    if (gameObjectHit != null)
                    {
                        Action actionToPerform = null;
                        if (gameObjectHit.name == "ECU-Panel-Button-ArrowDown")
                        {
                            foundObject = true;
                            actionToPerform = Pressed_Button_ArrowDown;
                            guiText = "Arrow down";
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Button-ArrowUp")
                        {
                            foundObject = true;
                            actionToPerform = Pressed_Button_ArrowUp;
                            guiText = "Arrow up";
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Button-Circle")
                        {
                            foundObject = true;
                            actionToPerform = Pressed_Button_Circle;
                            guiText = "Circle";
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Button-Cross")
                        {
                            foundObject = true;
                            actionToPerform = Pressed_Button_Cross;
                            guiText = "Cross";
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Button-Minus")
                        {
                            foundObject = true;
                            actionToPerform = Pressed_Button_Minus;
                            guiText = "Minus";
                        }
                        else if (gameObjectHit.name == "ECU-Panel-Button-Plus")
                        {
                            foundObject = true;
                            actionToPerform = Pressed_Button_Plus;
                            guiText = "Plus";
                        }

                        if (foundObject)
                        {
                            ModClient.guiInteract(guiText);
                            if (useButtonDown || Input.GetMouseButtonDown(0))
                            {
                                actionToPerform.Invoke();
                                AudioSource audio = dashButtonAudioSource;
                                audio.transform.position = gameObjectHit.transform.position;
                                audio.Play();
                            }
                        }
                    }


                }
            }
        }

        private void Pressed_Button_Plus()
        {
            if(currentPage == 1)
            {
                if (selectededSetting == "Select 2Step RPM")
                {
                    step2RevRpm += 100;
                }
                if (step2RevRpm >= 10000)
                    step2RevRpm = 10000;
            }
            else if(currentPage == 7)
            {
                ecu_airride_logic.increaseAirride(true, true, 0.05f);
            }
        }

        private void Pressed_Button_Minus()
        {
            
            if (currentPage == 1)
            {
                if (selectededSetting == "Select 2Step RPM")
                {
                    step2RevRpm -= 100;
                }
                if (step2RevRpm <= 2000)
                    step2RevRpm = 2000;
            }
            else if (currentPage == 7)
            {
                ecu_airride_logic.increaseAirride(true, true, -0.05f);
            }
        }

        private void Pressed_Button_Cross()
        {
            
        }

        private void Pressed_Button_Circle()
        {
            
        }

        private void Pressed_Button_ArrowUp()
        {
            currentPage++;
            if (!pageIndex.Contains(currentPage))
            {
                currentPage -= pageIndex.Length;
                if (!pageIndex.Contains(currentPage))
                {
                    currentPage++;
                }
            }
            ChangeInfoPanelPage(currentPage);
        }

        private void Pressed_Button_ArrowDown()
        {
            currentPage--;
            if (!pageIndex.Contains(currentPage))
            {
                currentPage += pageIndex.Length;
                if (!pageIndex.Contains(currentPage))
                {
                    currentPage--;
                }
            }
            ChangeInfoPanelPage(currentPage);
        }

        private void ChangeInfoPanelPage(int currentPage)
        {
            if(currentPage == 0)
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_page0;
                ecu_InfoPanel_Needle.enabled = true;
                ecu_InfoPanel_TurboWheel.enabled = false;
            }
            else if(currentPage == 1)
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_modules_page1;
                ecu_InfoPanel_Needle.enabled = true;
                ecu_InfoPanel_TurboWheel.enabled = false;
            }
            else if(currentPage == 2)
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_faults_page2;
                ecu_InfoPanel_Needle.enabled = false;
                ecu_InfoPanel_TurboWheel.enabled = false;
            }
            else if(currentPage == 3)
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_faults_page3;
                ecu_InfoPanel_Needle.enabled = false;
                ecu_InfoPanel_TurboWheel.enabled = false;
            }
            else if (currentPage == 4)
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_tuner_page4;
                ecu_InfoPanel_Needle.enabled = false;
                ecu_InfoPanel_TurboWheel.enabled = false;
            }
            else if(currentPage == 5)
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_turbo_page5;
                ecu_InfoPanel_Needle.enabled = false;
                ecu_InfoPanel_TurboWheel.enabled = true;
            }
            else if(currentPage == 6)
            {
                ecu_InfoPanel_Background.sprite = ecu_mod_panel_assistance_page6;
                ecu_InfoPanel_Needle.enabled = false;
                ecu_InfoPanel_TurboWheel.enabled = false;
            }

            ecu_InfoPanel_Display_Gear.text = "";
            ecu_InfoPanel_Display_kmh.text = "";
            ecu_InfoPanel_Display_km.text = "";

            ecu_InfoPanel_Display_Value_01.color = Color.white;
            ecu_InfoPanel_Display_Value_02.color = Color.white;
            ecu_InfoPanel_Display_Value_03.color = Color.white;
            ecu_InfoPanel_Display_Value_04.color = Color.white;
            ecu_InfoPanel_Display_Value_05.color = Color.white;
            ecu_InfoPanel_Display_Value_06.color = Color.white;
            ecu_InfoPanel_Display_Value_07.color = Color.white;
            ecu_InfoPanel_Display_Value_08.color = Color.white;
            ecu_InfoPanel_Display_Value_09.color = Color.white;
            ecu_InfoPanel_Display_Value_10.color = Color.white;
            ecu_InfoPanel_Display_Value_11.color = Color.white;
            ecu_InfoPanel_Display_Value_12.color = Color.white;
            ecu_InfoPanel_Display_Value_13.color = Color.white;
            ecu_InfoPanel_Display_Value_14.color = Color.white;
            ecu_InfoPanel_Display_Value_15.color = Color.white;
            ecu_InfoPanel_Display_Value_16.color = Color.white;
            selectededSetting = "";
        }

        private void DisplayPage0Values()
        {
            float[] fuelCalculated = FuelKMCalculate();
            ecu_InfoPanel_NeedleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
            ecu_InfoPanel_Display_Value_01.text = Convert.ToInt32(satsumaDriveTrain.rpm).ToString();
            ecu_InfoPanel_Display_Value_02.text = Convert.ToInt32(coolantTemp.Value).ToString();
            ecu_InfoPanel_Display_Value_04.text = Convert.ToInt32(coolantPressurePSI.Value).ToString();
            ecu_InfoPanel_Display_Value_05.text = "";
            ecu_InfoPanel_Display_Value_06.text = "";
            ecu_InfoPanel_Display_Value_07.text = "";
            ecu_InfoPanel_Display_Value_08.text = "";
            ecu_InfoPanel_Display_Value_09.text = "";
            ecu_InfoPanel_Display_Value_10.text = "";
            ecu_InfoPanel_Display_Value_11.text = "";
            ecu_InfoPanel_Display_Value_12.text = "";
            ecu_InfoPanel_Display_Value_13.text = ConvertToDigitalTime(clockHours.Value, clockMinutes.Value);
            ecu_InfoPanel_Display_Value_14.text = voltage.Value.ToString("00 .0") + "V";
            ecu_InfoPanel_Display_km.text = odometerKM.Value.ToString();
            ecu_InfoPanel_Display_kmh.text = Convert.ToInt32(satsumaDriveTrain.differentialSpeed).ToString();
            if (afRatio != null)
            {
                ecu_InfoPanel_Display_Value_03.text = afRatio.Value.ToString("00.00");
            }
            if (fuelCalculated != null)
            {
                if (fuelCalculated[1] >= 0 && fuelCalculated[1] <= 9000)
                {
                    ecu_InfoPanel_Display_Value_16.text = Convert.ToInt32(fuelCalculated[0]).ToString();
                    ecu_InfoPanel_Display_Value_15.text = Convert.ToInt32(fuelCalculated[1]).ToString();
                }
            }
            ecu_InfoPanel_Display_Gear.text = GearToString();
        }

        private void DisplayPage1Values()
        {
            
            ecu_InfoPanel_Display_Value_01.text = BoolToOnOffString(donnerTech_ecu_mod.GetAbsModuleEnabled());
            ecu_InfoPanel_Display_Value_02.text = BoolToOnOffString(donnerTech_ecu_mod.GetEspModuleEnabled());
            ecu_InfoPanel_Display_Value_03.text = BoolToOnOffString(donnerTech_ecu_mod.GetTcsModuleEnabled());
            ecu_InfoPanel_Display_Value_04.text = BoolToOnOffString(donnerTech_ecu_mod.GetStep2RevModuleEnabled());
            ecu_InfoPanel_Display_Value_05.text = "";
            ecu_InfoPanel_Display_Value_06.text = "";
            ecu_InfoPanel_Display_Value_07.text = "";
            ecu_InfoPanel_Display_Value_08.text = "";
            ecu_InfoPanel_Display_Value_09.text = "";
            ecu_InfoPanel_Display_Value_10.text = "";
            ecu_InfoPanel_Display_Value_11.text = "";
            ecu_InfoPanel_Display_Value_12.text = "";
            ecu_InfoPanel_Display_Value_13.text = BoolToOnOffString(donnerTech_ecu_mod.GetAlsModuleEnabled());
            ecu_InfoPanel_Display_Value_14.text = "";
            ecu_InfoPanel_Display_Value_15.text = "";
            ecu_InfoPanel_Display_Value_16.text = step2RevRpm.ToString();
            if (selectededSetting == "Select 2Step RPM")
            {
                ecu_InfoPanel_Display_Value_16.color = Color.green;
            }
            else
            {
                ecu_InfoPanel_Display_Value_16.color = Color.white;
            }
            
            ecu_InfoPanel_Display_km.text = odometerKM.Value.ToString();
            ecu_InfoPanel_Display_kmh.text = Convert.ToInt32(satsumaDriveTrain.differentialSpeed).ToString();
            ecu_InfoPanel_Display_Gear.text = GearToString();


            ecu_InfoPanel_NeedleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(-1f), 0));
           
        }

        private void DisplayPage2Values()
        {
            ecu_InfoPanel_Display_Value_01.text = ConvertFloatToWear(wearAlternator.Value);
            ecu_InfoPanel_Display_Value_02.text = ConvertFloatToWear(wearClutch.Value);
            ecu_InfoPanel_Display_Value_03.text = ConvertFloatToWear(wearCrankshaft.Value);
            ecu_InfoPanel_Display_Value_04.text = ConvertFloatToWear(wearFuelpump.Value);
            ecu_InfoPanel_Display_Value_05.text = ConvertFloatToWear(wearGearbox.Value);
            ecu_InfoPanel_Display_Value_06.text = ConvertFloatToWear(wearHeadgasket.Value);
            ecu_InfoPanel_Display_Value_07.text = ConvertFloatToWear(wearRockershaft.Value);
            ecu_InfoPanel_Display_Value_08.text = ConvertFloatToWear(wearStarter.Value);
            ecu_InfoPanel_Display_Value_09.text = ConvertFloatToWear(wearPiston1.Value);
            ecu_InfoPanel_Display_Value_10.text = ConvertFloatToWear(wearPiston2.Value);
            ecu_InfoPanel_Display_Value_11.text = ConvertFloatToWear(wearPiston3.Value);
            ecu_InfoPanel_Display_Value_12.text = ConvertFloatToWear(wearPiston4.Value);
            ecu_InfoPanel_Display_Value_13.text = ConvertFloatToWear(wearWaterpump.Value);
            ecu_InfoPanel_Display_Value_14.text = ConvertFloatToWear(wearHeadlightBulbLeft.Value);
            ecu_InfoPanel_Display_Value_15.text = ConvertFloatToWear(wearHeadlightBulbRight.Value);
            ecu_InfoPanel_Display_Value_16.text = "";
        }
        private void DisplayPage3Values()
        {
            if (radiator_installed.Value)
            {
                ecu_InfoPanel_Display_Value_01.text = ConvertFloatToPercantage(0.01f, 5.4f, waterLevelRadiator.Value);
            }
            else if(race_radiator_installed.Value)
            {
                ecu_InfoPanel_Display_Value_01.text = ConvertFloatToPercantage(0.01f, 7, waterLevelRaceRadiator.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_01.text = "";
            }

            if (oilpan_installed.Value)
            {
                ecu_InfoPanel_Display_Value_02.text = ConvertFloatToPercantage(0.01f, 3, oilLevel.Value);
                ecu_InfoPanel_Display_Value_03.text = ConvertFloatToPercantage(1, 100, oilContamination.Value);
                ecu_InfoPanel_Display_Value_04.text = oilGrade.Value.ToString("00.00");
            }
            else
            {
                ecu_InfoPanel_Display_Value_02.text = "";
                ecu_InfoPanel_Display_Value_03.text = "";
                ecu_InfoPanel_Display_Value_04.text = "";
            }


            if (clutchMasterCylinder_installed.Value)
            {
                ecu_InfoPanel_Display_Value_05.text = ConvertFloatToPercantage(0, 0.5f, clutchFluidLevel.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_05.text = "";
            }
            if (fueltank_installed.Value)
            {
                ecu_InfoPanel_Display_Value_06.text = ConvertFloatToPercantage(0, 36, fuelLevel.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_06.text = "";
            }

            if (brakeMasterCylinder_installed.Value)
            {
                ecu_InfoPanel_Display_Value_07.text = ConvertFloatToPercantage(0, 1, brakeFluidF.Value);
                ecu_InfoPanel_Display_Value_08.text = ConvertFloatToPercantage(0, 1, brakeFluidR.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_07.text = "";
                ecu_InfoPanel_Display_Value_08.text = "";
            }

            if (sparkplug1_installed.Value)
            {
                ecu_InfoPanel_Display_Value_09.text = ConvertFloatToPercantage(0, 100, wearSpark1.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_09.text = "";
            }
            if (sparkplug2_installed.Value)
            {
                ecu_InfoPanel_Display_Value_10.text = ConvertFloatToPercantage(0, 100, wearSpark2.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_10.text = "";
            }
            if (sparkplug3_installed.Value)
            {
                ecu_InfoPanel_Display_Value_11.text = ConvertFloatToPercantage(0, 100, wearSpark3.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_11.text = "";
            }
            if (sparkplug4_installed.Value)
            {
                ecu_InfoPanel_Display_Value_12.text = ConvertFloatToPercantage(0, 100, wearSpark4.Value);
            }
            else
            {
                ecu_InfoPanel_Display_Value_12.text = "";
            }

            ecu_InfoPanel_Display_Value_13.text = "";
            ecu_InfoPanel_Display_Value_14.text = "";
            ecu_InfoPanel_Display_Value_15.text = "";
            ecu_InfoPanel_Display_Value_16.text = "";
        }
        private void DisplayPage4Values()
        {
            ecu_InfoPanel_Display_Value_01.text = cylinder1exhaust.Value.ToString("00.00");
            ecu_InfoPanel_Display_Value_02.text = cylinder2exhaust.Value.ToString("00.00");
            ecu_InfoPanel_Display_Value_03.text = cylinder3exhaust.Value.ToString("00.00");
            ecu_InfoPanel_Display_Value_04.text = cylinder4exhaust.Value.ToString("00.00");
            ecu_InfoPanel_Display_Value_05.text = cylinder1intake.Value.ToString("00.00");
            ecu_InfoPanel_Display_Value_06.text = cylinder2intake.Value.ToString("00.00");
            ecu_InfoPanel_Display_Value_07.text = cylinder3intake.Value.ToString("00.00");
            ecu_InfoPanel_Display_Value_08.text = cylinder4intake.Value.ToString("00.00");

            if (raceCarb_installed.Value)
            {
                ecu_InfoPanel_Display_Value_09.text = raceCarbAdjust1.Value.ToString("00.00");
                ecu_InfoPanel_Display_Value_10.text = raceCarbAdjust2.Value.ToString("00.00");
                ecu_InfoPanel_Display_Value_11.text = raceCarbAdjust3.Value.ToString("00.00");
                ecu_InfoPanel_Display_Value_12.text = raceCarbAdjust4.Value.ToString("00.00");
            }
            else
            {
                ecu_InfoPanel_Display_Value_09.text = "";
                ecu_InfoPanel_Display_Value_10.text = "";
                ecu_InfoPanel_Display_Value_11.text = "";
                ecu_InfoPanel_Display_Value_12.text = "";
            }

            if (twinCarb_installed.Value)
            {
                ecu_InfoPanel_Display_Value_13.text = twinCarbAdjust.Value.ToString("00.00");
            }
            else
            {
                ecu_InfoPanel_Display_Value_13.text = "";
            }
            if (carb_installed.Value)
            {
                ecu_InfoPanel_Display_Value_14.text = carbAdjust.Value.ToString("00.00");
            }
            else
            {
                ecu_InfoPanel_Display_Value_14.text = "";
            }

            if (autoTuneEco_running)
            {
                ecu_InfoPanel_Display_Value_15.text = "RUNNING";
                ecu_InfoPanel_Display_Value_16.text = "";
            }
            else if (autoTuneRace_running)
            {
                ecu_InfoPanel_Display_Value_15.text = "";
                ecu_InfoPanel_Display_Value_16.text = "RUNNING";
            }
            else
            {
                ecu_InfoPanel_Display_Value_15.text = "READY";
                ecu_InfoPanel_Display_Value_16.text = "READY";
            }

            
        }
        private void DisplayPage5Values()
        {
            if (ModLoader.IsModPresent("SatsumaTurboCharger"))
            {
                ecu_InfoPanel_TurboWheelObject.transform.Rotate(0f, 0f, 40 * Time.deltaTime);
                
                if (turbocharger_bigFSM != null && turbocharger_big_allInstalled.Value)
                {
                    if (turbocharger_big_pressure.Value >= 0f)
                    {
                        ecu_InfoPanel_Display_Value_01.text = turbocharger_big_pressure.Value.ToString("0.00");
                    }
                    else
                    {
                        ecu_InfoPanel_Display_Value_01.text = "0.00";
                    }
                    ecu_InfoPanel_Display_Value_02.text = turbocharger_big_max_boost.Value.ToString("0.00");
                    ecu_InfoPanel_Display_Value_03.text = "";
                    ecu_InfoPanel_Display_Value_04.text = "";
                    ecu_InfoPanel_Display_Value_05.text = "";
                    ecu_InfoPanel_Display_Value_06.text = "";
                    ecu_InfoPanel_Display_Value_07.text = "";
                    ecu_InfoPanel_Display_Value_08.text = "";
                    ecu_InfoPanel_Display_Value_09.text = "";
                    ecu_InfoPanel_Display_Value_10.text = "";
                    ecu_InfoPanel_Display_Value_11.text = "";
                    ecu_InfoPanel_Display_Value_12.text = "";
                    ecu_InfoPanel_Display_Value_13.text = "";
                    ecu_InfoPanel_Display_Value_14.text = turbocharger_big_exhaust_temp.Value.ToString("000");
                    ecu_InfoPanel_Display_Value_15.text = turbocharger_big_intake_temp.Value.ToString("000");
                    ecu_InfoPanel_Display_Value_16.text = turbocharger_big_rpm.Value.ToString("0");
                    ecu_InfoPanel_Display_Gear.text = "";
                    ecu_InfoPanel_Display_kmh.text = "";
                    ecu_InfoPanel_Display_km.text = "";
                }
                else if (turbocharger_smallFSM != null && turbocharger_small_allInstalled.Value)
                {
                    if (turbocharger_small_pressure.Value >= 0f)
                    {
                        ecu_InfoPanel_Display_Value_01.text = turbocharger_small_pressure.Value.ToString("0.00");
                    }
                    else
                    {
                        ecu_InfoPanel_Display_Value_01.text = "0.00";
                    }
                    ecu_InfoPanel_Display_Value_02.text = turbocharger_small_max_boost.Value.ToString("0.00");
                    ecu_InfoPanel_Display_Value_03.text = "";
                    ecu_InfoPanel_Display_Value_04.text = "";
                    ecu_InfoPanel_Display_Value_05.text = "";
                    ecu_InfoPanel_Display_Value_06.text = "";
                    ecu_InfoPanel_Display_Value_07.text = "";
                    ecu_InfoPanel_Display_Value_08.text = "";
                    ecu_InfoPanel_Display_Value_09.text = "";
                    ecu_InfoPanel_Display_Value_10.text = "";
                    ecu_InfoPanel_Display_Value_11.text = "";
                    ecu_InfoPanel_Display_Value_12.text = "";
                    ecu_InfoPanel_Display_Value_13.text = "";
                    ecu_InfoPanel_Display_Value_14.text = turbocharger_small_exhaust_temp.Value.ToString("000");
                    ecu_InfoPanel_Display_Value_15.text = turbocharger_small_intake_temp.Value.ToString("000");
                    ecu_InfoPanel_Display_Value_16.text = turbocharger_small_rpm.Value.ToString("0");
                    ecu_InfoPanel_Display_Gear.text = "";
                    ecu_InfoPanel_Display_kmh.text = "";
                    ecu_InfoPanel_Display_km.text = "";
                }
                else
                {
                    ecu_InfoPanel_Display_Value_01.text = "";
                    ecu_InfoPanel_Display_Value_02.text = "";
                    ecu_InfoPanel_Display_Value_03.text = "";
                    ecu_InfoPanel_Display_Value_04.text = "";
                    ecu_InfoPanel_Display_Value_05.text = "";
                    ecu_InfoPanel_Display_Value_06.text = "";
                    ecu_InfoPanel_Display_Value_07.text = "";
                    ecu_InfoPanel_Display_Value_08.text = "";
                    ecu_InfoPanel_Display_Value_09.text = "";
                    ecu_InfoPanel_Display_Value_10.text = "";
                    ecu_InfoPanel_Display_Value_11.text = "";
                    ecu_InfoPanel_Display_Value_12.text = "";
                    ecu_InfoPanel_Display_Value_13.text = "";
                    ecu_InfoPanel_Display_Value_14.text = "";
                    ecu_InfoPanel_Display_Value_15.text = "";
                    ecu_InfoPanel_Display_Value_16.text = "";
                    ecu_InfoPanel_Display_Gear.text = "";
                    ecu_InfoPanel_Display_kmh.text = "";
                    ecu_InfoPanel_Display_km.text = "";
                }
                
            }
            else
            {
                ecu_InfoPanel_Display_Value_01.text = "";
                ecu_InfoPanel_Display_Value_02.text = "";
                ecu_InfoPanel_Display_Value_03.text = "";
                ecu_InfoPanel_Display_Value_04.text = "";
                ecu_InfoPanel_Display_Value_05.text = "";
                ecu_InfoPanel_Display_Value_06.text = "";
                ecu_InfoPanel_Display_Value_07.text = "";
                ecu_InfoPanel_Display_Value_08.text = "";
                ecu_InfoPanel_Display_Value_09.text = "";
                ecu_InfoPanel_Display_Value_10.text = "";
                ecu_InfoPanel_Display_Value_11.text = "";
                ecu_InfoPanel_Display_Value_12.text = "";
                ecu_InfoPanel_Display_Value_13.text = "";
                ecu_InfoPanel_Display_Value_14.text = "";
                ecu_InfoPanel_Display_Value_15.text = "";
                ecu_InfoPanel_Display_Value_16.text = "";
                ecu_InfoPanel_Display_Gear.text = "";
                ecu_InfoPanel_Display_kmh.text = "";
                ecu_InfoPanel_Display_km.text = "";
            }
        }
        private void DisplayPage6Values()
        {
            ecu_InfoPanel_Display_Value_01.text = BoolToOnOffString(rainsensor_enabled);
            ecu_InfoPanel_Display_Value_02.text = BoolToOnOffString(lightsensor_enabled);
            ecu_InfoPanel_Display_Value_03.text = "";
            ecu_InfoPanel_Display_Value_04.text = "";
            ecu_InfoPanel_Display_Value_05.text = "";
            ecu_InfoPanel_Display_Value_06.text = "";
            ecu_InfoPanel_Display_Value_07.text = "";
            ecu_InfoPanel_Display_Value_08.text = "";
            ecu_InfoPanel_Display_Value_09.text = "";
            ecu_InfoPanel_Display_Value_10.text = "";
            ecu_InfoPanel_Display_Value_11.text = "";
            ecu_InfoPanel_Display_Value_12.text = "";
            ecu_InfoPanel_Display_Value_13.text = "";
            ecu_InfoPanel_Display_Value_14.text = "";
            ecu_InfoPanel_Display_Value_15.text = "";
            ecu_InfoPanel_Display_Value_16.text = "";
            ecu_InfoPanel_Display_Gear.text = "";
            ecu_InfoPanel_Display_kmh.text = "";
            ecu_InfoPanel_Display_km.text = "";
        }

        private void DisplayPage7Values()
        {
            ecu_InfoPanel_Display_Value_01.text = "";
            ecu_InfoPanel_Display_Value_02.text = "Lowest";
            ecu_InfoPanel_Display_Value_03.text = "Highest";
            ecu_InfoPanel_Display_Value_04.text = "Default";
            ecu_InfoPanel_Display_Value_05.text = "";
            ecu_InfoPanel_Display_Value_06.text = "";
            ecu_InfoPanel_Display_Value_07.text = "";
            ecu_InfoPanel_Display_Value_08.text = "";
            ecu_InfoPanel_Display_Value_09.text = "";
            ecu_InfoPanel_Display_Value_10.text = "";
            ecu_InfoPanel_Display_Value_11.text = "";
            ecu_InfoPanel_Display_Value_12.text = "";
            ecu_InfoPanel_Display_Value_13.text = "";
            ecu_InfoPanel_Display_Value_14.text = "";
            ecu_InfoPanel_Display_Value_15.text = "";
            ecu_InfoPanel_Display_Value_16.text = "";
            ecu_InfoPanel_Display_Gear.text = "";
            ecu_InfoPanel_Display_kmh.text = "";
            ecu_InfoPanel_Display_km.text = "";
        }


        private void DisplayGeneralInfomation()
        {
            ecu_InfoPanel_LowBeam.enabled = beamShort.activeSelf;
            ecu_InfoPanel_HighBeam.enabled = beamLong.activeSelf;
            if (satsumaCarController.handbrakeInput > 0)
            {
                ecu_InfoPanel_Handbrake.enabled = true;
            }
            else
            {
                ecu_InfoPanel_Handbrake.enabled = false;
            }


            if (blinkerLeft == null)
            {
                blinkerLeft = GameObject.Find("BlinkLeft");
            }
            else
            {
                ecu_InfoPanel_IndicatorLeft.enabled = blinkerLeft.activeSelf;
            }
            if (blinkerRight == null)
            {
                blinkerRight = GameObject.Find("BlinkRight");
            }
            else
            {
                ecu_InfoPanel_IndicatorRight.enabled = blinkerRight.activeSelf;
            }
        }

        private string BoolToOnOffString(bool value)
        {
            if(value == true)
            {
                return "ON";
            }
            else
            {
                return "OFF";
            }
        }

        private string ConvertFloatToWear(float value)
        {
            if(value >= 80)
            {
                return "Good";
            }
            else if(value >= 60)
            {
                return "Warning";
            }
            else
            {
                return "DANGER";
            }
        }
        private string ConvertFloatToPercantage(float min, float max, float value)
        {
            float calculatedPercentage = ((value - min) * 100) / (max - min);
            int intPercentage = Convert.ToInt32(calculatedPercentage);

            return intPercentage.ToString("000") + "%";
        }

        private float GetRPMRotation(float rpmOverride)
        {
            
            if (rpmOverride >= 0)
            {
                float totalAngleSize = ecu_InfoPanel_Needle_minAngle - ecu_InfoPanel_Needle_maxAngle;
                float rpmNormalized = rpmOverride / ecu_InfoPanel_Needle_maxRPM;
                return ecu_InfoPanel_Needle_minAngle - rpmNormalized * totalAngleSize;
            }
            else
            {
                float totalAngleSize = ecu_InfoPanel_Needle_minAngle - ecu_InfoPanel_Needle_maxAngle;
                float rpmNormalized = satsumaDriveTrain.rpm / ecu_InfoPanel_Needle_maxRPM;
                return ecu_InfoPanel_Needle_minAngle - rpmNormalized * totalAngleSize;
            }
        }

        private string GearToString()
        {
            if (satsumaDriveTrain.gear == 0)
            {
                return "R";
            }
            else if (satsumaDriveTrain.gear == 1)
            {
                return "N";
            }
            else
                return (satsumaDriveTrain.gear - 1).ToString();
        }

        private void Play_ECU_InfoPanel_Animation()
        {
            const float rpmAdder = 300;
            const float tenAdder = 1;
            const float hundredAdder = 10;
            const float thousandAdder = 100;
            if (rpmIncrementer == 9000)
            {
                rpmDecrementer -= rpmAdder;
                tenIncrementer -= tenAdder;
                hundredIncrementer -= hundredAdder;
                thousandIncrementer -= thousandAdder;

                ecu_InfoPanel_NeedleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(rpmDecrementer), 0));

                ecu_InfoPanel_Display_Value_01.text = rpmDecrementer.ToString();
                ecu_InfoPanel_Display_Value_02.text = hundredIncrementer.ToString();
                ecu_InfoPanel_Display_Value_03.text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
                ecu_InfoPanel_Display_Value_04.text = tenIncrementer.ToString();
                ecu_InfoPanel_Display_Value_13.text = "";
                ecu_InfoPanel_Display_Value_14.text = tenIncrementer.ToString("00 .0") + "V";
                ecu_InfoPanel_Display_Value_15.text = thousandIncrementer.ToString();
                ecu_InfoPanel_Display_Value_16.text = tenIncrementer.ToString();
                ecu_InfoPanel_Display_kmh.text = hundredIncrementer.ToString();
                ecu_InfoPanel_Display_km.text = (thousandIncrementer * 10f).ToString();
            }
            else if (rpmIncrementer < 9000)
            {
                rpmIncrementer += rpmAdder;
                tenIncrementer += tenAdder;
                hundredIncrementer += hundredAdder;
                thousandIncrementer += thousandAdder;

                ecu_InfoPanel_NeedleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(rpmIncrementer), 0));

                ecu_InfoPanel_Display_Value_01.text = rpmIncrementer.ToString();
                ecu_InfoPanel_Display_Value_02.text = hundredIncrementer.ToString();
                ecu_InfoPanel_Display_Value_03.text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
                ecu_InfoPanel_Display_Value_04.text = tenIncrementer.ToString();
                ecu_InfoPanel_Display_Value_13.text = "Boot up";
                ecu_InfoPanel_Display_Value_14.text = tenIncrementer.ToString("00 .0") + "V";
                ecu_InfoPanel_Display_Value_15.text = thousandIncrementer.ToString();
                ecu_InfoPanel_Display_Value_16.text = tenIncrementer.ToString();
                ecu_InfoPanel_Display_kmh.text = hundredIncrementer.ToString();
                ecu_InfoPanel_Display_km.text = (thousandIncrementer * 10f).ToString();
            }
            if (rpmIncrementer >= 9000 && rpmDecrementer <= 0)
            {
                rpmIncrementer = 0;
                rpmDecrementer = 9000;
                isBooted = true;
                isBooting = false;
            }
        }

        private float[] FuelKMCalculate()
        {
            timerFuel += Time.deltaTime;

            oldValue -= fuelLeft.Value;
            if (oldValue > 0)
            {
                fuelConsumption = oldValue;
                consumptionCounter += fuelConsumption;
            }
            oldValue = fuelLeft.Value;

            if (timerFuel >= 1f)
            {

                float Lper100km = Mathf.Clamp((consumptionCounter * 60) * 60, 0, 7);
                float kmLeft = (fuelLeft.Value / Lper100km) * 100;
                //float kmLeft = (fuelLeft.Value / ((consumptionCounter * 60) * 60) * 1) * 100;
                timerFuel = 0;
                consumptionCounter = 0;
                return new float[] { Lper100km, kmLeft };
            }
            return null;

        }

        private bool hasPower
        {
            get
            {
                if (carElectricsPower == null)
                {
                    GameObject carElectrics = GameObject.Find("SATSUMA(557kg, 248)/Electricity");
                    carElectricsPower = PlayMakerFSM.FindFsmOnGameObject(carElectrics, "Power");
                    return carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK").Value;
                }
                else
                {
                    return carElectricsPower.FsmVariables.FindFsmBool("ElectricsOK").Value;
                }
                
                
            }
        }
        internal static bool useButtonDown
        {
            get
            {
                return cInput.GetKeyDown("Use");
            }
        }

        private static AudioSource dashButtonAudioSource
        {
            get
            {
                return GameObject.Find("dash_button").GetComponent<AudioSource>();
            }
        }

        public int GetStep2RevRpm()
        {
            return step2RevRpm;
        }
    }
}