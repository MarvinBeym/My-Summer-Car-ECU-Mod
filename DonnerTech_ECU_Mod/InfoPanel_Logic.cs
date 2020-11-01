using DonnerTech_ECU_Mod.info_panel_pages;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModApi;
using MSCLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    public class InfoPanel_Logic : MonoBehaviour
    {
        private Mod mainMod;
        private DonnerTech_ECU_Mod mod;

        private GameObject infoPanel;
        private GameObject turnSignals;
        private GameObject beamShort;
        private GameObject beamLong;
        private GameObject blinkerRight;
        private GameObject blinkerLeft;

        //Pages
        private InfoPanelPage[] pages;
        private Page4 page4;
        private int currentPage = 0;
        //Animation
        private const float ecu_InfoPanel_Needle_maxAngle = 270;
        private const float ecu_InfoPanel_Needle_minAngle = 0;
        private const float ecu_InfoPanel_Needle_maxRPM = 9000;
        private float rpmIncrementer = 0;
        private float rpmDecrementer = 9000;
        private float tenIncrementer = 0;
        private float hundredIncrementer = 0;
        private float thousandIncrementer = 0;


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

        private Airride_Logic ecu_airride_logic;

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

        private Dictionary<string, TextMesh> display_values = new Dictionary<string, TextMesh>();

        //Car
        private GameObject satsuma;
        private Drivetrain satsumaDriveTrain;
        private CarController satsumaCarController;
        private Axles satsumaAxles;
        private FsmString playerCurrentVehicle;

        private RaycastHit hit;
        public AssetBundle assetBundle;

        private string selectedSetting = "";

        private FsmFloat rainIntensity;
        PlayMakerFSM wiperLogicFSM;
        public bool rainsensor_enabled { get; set; } = false;
        private bool rainsensor_wasEnabled = false;


        public bool lightsensor_enabled { get; set; } = false;
        private bool lightsensor_wasEnabled = false;


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

        private MeshRenderer shift_indicator_renderer;
        private Gradient shift_indicator_gradient;

        private float shift_indicator_blink_timer = 0;
        private int shift_indicator_baseLine = 3500;
        public int shift_indicator_greenLine = 6500;
        public int shift_indicator_redLine = 7500;

        private void Start()
        {
            assetBundle = LoadAssets.LoadBundle(mod, "ecu-mod.unity3d");
            infoPanel = this.gameObject;

            ecu_airride_logic = this.gameObject.AddComponent<Airride_Logic>();
            satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            satsumaDriveTrain = satsuma.GetComponent<Drivetrain>();
            satsumaCarController = satsuma.GetComponent<CarController>();
            satsumaAxles = satsuma.GetComponent<Axles>();

            playerCurrentVehicle = FsmVariables.GlobalVariables.FindFsmString("PlayerCurrentVehicle");

            ecu_InfoPanel_NeedleObject = GameObject.Find("ECU-Panel-Needle");
            ecu_InfoPanel_TurboWheelObject = GameObject.Find("ECU-Panel-TurboWheel");
            ecu_InfoPanel_Display_Reverse_Camera = GameObject.Find("ECU-Panel-Display-Reverse-Camera").GetComponent<MeshRenderer>();
            ecu_InfoPanel_Display_Reverse_Camera.enabled = false;

            
            shift_indicator_renderer = GameObject.Find("ECU-Shift-Indicator").GetComponent<MeshRenderer>();
            SetupShiftIndicator();

            TextMesh[] ecu_InfoPanel_TextMeshes = infoPanel.GetComponentsInChildren<TextMesh>();
            foreach (TextMesh textMesh in ecu_InfoPanel_TextMeshes)
            {
                switch (textMesh.name)
                {
                    case "ECU-Panel-Display-Gear":
                        display_values.Add("value_gear", textMesh);
                        break;
                    case "ECU-Panel-Display-km/h":
                        display_values.Add("value_kmh", textMesh);
                        break;
                    case "ECU-Panel-Display-km":
                        display_values.Add("value_km", textMesh);
                        break;
                }
                for (int i = 1; i <= 16; i++)
                {
                    if(textMesh.name == ("ECU-Panel-Display-Value-" + i.ToString().PadLeft(2, '0')))
                    {
                        display_values.Add("value_" + i, textMesh);
                        continue;
                    }
                }
                textMesh.gameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            SpriteRenderer[] ecu_InfoPanel_SpriteRenderer = infoPanel.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer spriteRenderer in ecu_InfoPanel_SpriteRenderer)
            {
                switch (spriteRenderer.name)
                {
                    case "ECU-Panel-Needle":
                        ecu_InfoPanel_Needle = spriteRenderer;
                        break;
                    case "ECU-Panel-TurboWheel":
                        ecu_InfoPanel_TurboWheel = spriteRenderer;
                        break;
                    case "ECU-Panel-Background":
                        ecu_InfoPanel_Background = spriteRenderer;
                        break;
                    case "ECU-Panel-Indicator-Left":
                        ecu_InfoPanel_IndicatorLeft = spriteRenderer;
                        break;
                    case "ECU-Panel-Indicator-Right":
                        ecu_InfoPanel_IndicatorRight = spriteRenderer;
                        break;
                    case "ECU-Panel-Handbrake":
                        ecu_InfoPanel_Handbrake = spriteRenderer;
                        break;
                    case "ECU-Panel-LowBeam":
                        ecu_InfoPanel_LowBeam = spriteRenderer;
                        break;
                    case "ECU-Panel-HighBeam":
                        ecu_InfoPanel_HighBeam = spriteRenderer;
                        break;
                }
                spriteRenderer.enabled = false;
            }

            turnSignals = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/TurnSignals");
            PlayMakerFSM blinkers = null;
            PlayMakerFSM[] turnSignalComps = turnSignals.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM turnSignalComp in turnSignalComps)
            {
                if (turnSignalComp.FsmName == "Blinkers")
                {
                    blinkers = turnSignalComp;
                    break;
                }
            }

            GameObject powerON = GameObject.Find("SATSUMA(557kg, 248)/Electricity/PowerON");
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

            ecu_mod_panel_needle = assetBundle.LoadAsset<Sprite>("Rpm-Needle.png");
            ecu_mod_panel_turbineWheel = assetBundle.LoadAsset<Sprite>("TurbineWheel.png");

            ecu_mod_panel_handbrake = assetBundle.LoadAsset<Sprite>("Handbrake-Icon.png");
            ecu_mod_panel_blinkerLeft = assetBundle.LoadAsset<Sprite>("Indicator-Left-Icon.png");
            ecu_mod_panel_blinkerRight = assetBundle.LoadAsset<Sprite>("Indicator-Right-Icon.png");
            ecu_mod_panel_highBeam = assetBundle.LoadAsset<Sprite>("HighBeam-Icon.png");
            ecu_mod_panel_lowBeam = assetBundle.LoadAsset<Sprite>("LowBeam-Icon.png");

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

            PlayMakerFSM suspension = GameObject.Find("Suspension").GetComponent<PlayMakerFSM>();
            travelRally = suspension.FsmVariables.FindFsmFloat("TravelRally");
            wheelPosRally = suspension.FsmVariables.FindFsmFloat("WheelPosRally");

            rallyFrontRate = suspension.FsmVariables.FindFsmFloat("RallyFrontRate");
            rallyRearRate = suspension.FsmVariables.FindFsmFloat("RallyRearRate");
            
            FsmHook.FsmInject(GameObject.Find("StreetLights"), "Day", SwitchToDay);
            FsmHook.FsmInject(GameObject.Find("StreetLights"), "Night", SwitchToNight);

            page4 = new Page4(4, "tuner_page", "ECU-Mod-Panel_Tuner-Page4", this.mod, display_values);
            
            pages = new InfoPanelPage[]
            {
                new Page0(0, "main_page", "ECU-Mod-Panel-Page0", this.mod, this.ecu_InfoPanel_NeedleObject, display_values),
                 new Page1(1, "modules_page", "ECU-Mod-Panel_Modules-Page1", this.mod, this.ecu_InfoPanel_NeedleObject, display_values),
                 new Page2(2, "faults_page", "ECU-Mod-Panel_Faults-Page2", this.mod, display_values),
                 new Page3(3, "faults2_page", "ECU-Mod-Panel_Faults-Page3", this.mod, display_values),
                 page4,
                 new Page5(5, "turbocharger_page", "ECU-Mod-Panel-Turbocharger-Page5", this.mod, this.ecu_InfoPanel_TurboWheelObject, display_values),
                 new Page6(6, "assistance_page", "ECU-Mod-Panel-Assistance-Page6", this.mod, display_values),
#if DEBUG 
                new Page7(7, "airride_page", "ECU-Mod-Panel-Airride-Page7", this.mod, display_values),
#endif
            };

            assetBundle.Unload(false);
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
            ecu_mod_panel_handbrake = LoadNewSprite(ecu_mod_panel_handbrake, Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE" + "_" + "handbrake_icon.png"));
            ecu_InfoPanel_Handbrake.sprite = ecu_mod_panel_handbrake;

            ecu_mod_panel_blinkerLeft = LoadNewSprite(ecu_mod_panel_blinkerLeft, Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE" + "_" + "blinker_left_icon.png"));
            ecu_InfoPanel_IndicatorLeft.sprite = ecu_mod_panel_blinkerLeft;

            ecu_mod_panel_blinkerRight = LoadNewSprite(ecu_mod_panel_blinkerRight, Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE" + "_" + "blinker_right_icon.png"));
            ecu_InfoPanel_IndicatorRight.sprite = ecu_mod_panel_blinkerRight;

            ecu_mod_panel_lowBeam = LoadNewSprite(ecu_mod_panel_lowBeam, Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE" + "_" + "low_beam_icon.png"));
            ecu_InfoPanel_LowBeam.sprite = ecu_mod_panel_lowBeam;

            ecu_mod_panel_highBeam = LoadNewSprite(ecu_mod_panel_highBeam, Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE" + "_" + "high_beam_icon.png"));
            ecu_InfoPanel_HighBeam.sprite = ecu_mod_panel_highBeam;

            ecu_mod_panel_needle = LoadNewSprite(ecu_mod_panel_needle, Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE" + "_" + "needle_icon.png"));
            ecu_InfoPanel_Needle.sprite = ecu_mod_panel_needle;

            ecu_mod_panel_turbineWheel = LoadNewSprite(ecu_mod_panel_turbineWheel, Path.Combine(ModLoader.GetModAssetsFolder(mod), "OVERRIDE" + "_" + "turbine_icon.png"));
            ecu_InfoPanel_TurboWheel.sprite = ecu_mod_panel_turbineWheel;
        }
        public Sprite LoadNewSprite(Sprite sprite, string FilePath, float pivotX = 0.5f, float pivotY = 0.5f, float PixelsPerUnit = 100.0f)
        {
            if (File.Exists(FilePath) && Path.GetExtension(FilePath) == ".png")
            {
                Sprite NewSprite = new Sprite();
                Texture2D SpriteTexture = LoadTexture(FilePath);
                NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(pivotX, pivotY), PixelsPerUnit);

                return NewSprite;
            }
            return sprite;
        }
        public Texture2D LoadTexture(string FilePath)
        {
            Texture2D Tex2D;
            byte[] FileData;

            if (File.Exists(FilePath))
            {
                FileData = File.ReadAllBytes(FilePath);
                Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(FileData))
                    return Tex2D;
            }
            return null;
        }

        void Update()
        {

            if (mod.hasPower && mod.info_panel_part.InstalledScrewed())
            {
                if (!isBooted)
                {
                    HandleBootAnimation();
                }
                else
                {
                    HandleShiftIndicator();
                    HandleKeybinds();
                    HandleButtonPresses();

                    if (true/*mod.GetAirrideInstalledScrewed()*/)
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
                        beamShort.SetActive(false);
                        lightsensor_wasEnabled = false;
                    }


                    DisplayGeneralInfomation();
                    pages[currentPage].Handle();
                }
            }
            else
            {
                try
                {
                    shift_indicator_renderer.material.color = Color.black;
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

                    foreach(KeyValuePair<string, TextMesh> display_value in display_values)
                    {
                        display_value.Value.text = "";
                        display_value.Value.color = Color.white;
                    }

                    page4.ResetAutoTune();

                    isBooting = false;
                    isBooted = false;
                    currentPage = 0;
                }
                catch
                {

                }

            }
            
        }
        private void HandleAirride()
        {
            
        }
        private void HandleReverseCamera()
        {
            if (!mod.reverse_camera_part.InstalledScrewed())
            {
                ecu_InfoPanel_Display_Reverse_Camera.enabled = false;
                mod.SetReverseCameraEnabled(false);
                return;
            }
            if (satsumaDriveTrain.gear == 0)
            {
                ecu_InfoPanel_Display_Reverse_Camera.enabled = true;
                mod.SetReverseCameraEnabled(true);
                return;
            }
            ecu_InfoPanel_Display_Reverse_Camera.enabled = false;
            mod.SetReverseCameraEnabled(false);
            return;
        }

        private void HandleRainsensorLogic()
        {
            rainsensor_wasEnabled = true;
            FsmBool wiperOn = wiperLogicFSM.FsmVariables.FindFsmBool("On");
            FsmFloat wiperDelay = wiperLogicFSM.FsmVariables.FindFsmFloat("Delay");
            if (rainsensor_enabled)
            {
                switch (rainIntensity.Value)
                {
                    case float f when f >= 0.5f:
                        wiperOn.Value = true;
                        wiperDelay.Value = 0f;
                        break;
                    case float f when f > 0f:
                        wiperOn.Value = true;
                        wiperDelay.Value = 3f;
                        break;
                    default:
                        wiperOn.Value = false;
                        wiperDelay.Value = 0f;
                        break;
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
                if (this.isNight)
                {
                    beamShort.SetActive(true);
                    return;
                }
                beamShort.SetActive(false);
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
                currentPage = 0;
                ChangeInfoPanelPage(currentPage);
                ecu_InfoPanel_Background.sprite = pages[currentPage].pageSprite;

                ecu_InfoPanel_Needle.enabled = pages[currentPage].needleUsed;
                ecu_InfoPanel_TurboWheel.enabled = pages[currentPage].turbineUsed;
                ecu_InfoPanel_Background.enabled = true;
                ecu_InfoPanel_IndicatorLeft.enabled = false;
                ecu_InfoPanel_IndicatorRight.enabled = false;
                ecu_InfoPanel_Handbrake.enabled = false;
                ecu_InfoPanel_LowBeam.enabled = false;
                ecu_InfoPanel_HighBeam.enabled = false;
                foreach(KeyValuePair<string, TextMesh> display_value in display_values)
                {
                    display_value.Value.gameObject.GetComponent<MeshRenderer>().enabled = true;
                    display_value.Value.text = "";
                }
                isBooting = true;
            }
        }

        private void SetupShiftIndicator()
        {
            shift_indicator_gradient = new Gradient();
            GradientColorKey[] colorKey = new GradientColorKey[3];
            colorKey[0].color = new Color(1.0f, 0.64f, 0.0f); //Orange
            colorKey[0].time = (float)shift_indicator_baseLine / 10000;

            colorKey[1].color = Color.green;
            colorKey[1].time = (float) shift_indicator_greenLine / 10000;

            colorKey[2].color = Color.red;
            colorKey[2].time = (float)shift_indicator_redLine / 10000;

            // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
            GradientAlphaKey[] alphaKey = new GradientAlphaKey[3];
            alphaKey[0].alpha = 1f - (float)shift_indicator_baseLine / 10000;
            alphaKey[0].time = (float)shift_indicator_baseLine / 10000;

            alphaKey[1].alpha = 1f - (float)shift_indicator_greenLine / 10000;
            alphaKey[1].time = (float)shift_indicator_greenLine / 10000;

            alphaKey[2].alpha = 1f - (float) shift_indicator_redLine / 10000;
            alphaKey[2].time = (float) shift_indicator_redLine / 10000;

            shift_indicator_gradient.SetKeys(colorKey, alphaKey);
        }

        private void HandleShiftIndicator()
        {
            if(satsumaDriveTrain.rpm > 0)
            {
                float gradientValue = satsumaDriveTrain.rpm / 10000;
                
                if (satsumaDriveTrain.rpm >= 7500)
                {
                    shift_indicator_blink_timer += Time.deltaTime;

                    if (shift_indicator_blink_timer <= 0.15f)
                    {

                        shift_indicator_renderer.material.color = Color.black;
                    }
                    if (shift_indicator_blink_timer >= 0.3f)
                    {
                        shift_indicator_blink_timer = 0;

                        shift_indicator_renderer.material.color = shift_indicator_gradient.Evaluate(gradientValue);
                    }
                }
                else
                {
                    shift_indicator_renderer.material.color = shift_indicator_gradient.Evaluate(gradientValue);
                }
            }
            else
            {
                shift_indicator_renderer.material.color = Color.black;
            }

        }


        private void HandleKeybinds()
        {
            if(playerCurrentVehicle.Value == "Satsuma")
            {
                if (mod.info_panel_arrowUp.GetKeybindDown())
                {
                    Pressed_Button_ArrowUp();
                }
                else if (mod.info_panel_arrowDown.GetKeybindDown())
                {
                    Pressed_Button_ArrowUp();
                }
                else if (mod.info_panel_circle.GetKeybindDown())
                {
                    Pressed_Button_Circle();
                }
                else if (mod.info_panel_cross.GetKeybindDown())
                {
                    Pressed_Button_Cross();
                }
                else if (mod.info_panel_plus.GetKeybindDown())
                {
                    Pressed_Button_Plus();
                }
                else if (mod.info_panel_minus.GetKeybindDown())
                {
                    Pressed_Button_Minus();
                }
            }
        }

        public void HandleTouchPresses(string[] guiTexts, InfoPanelPage page)
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
                        for (int i = 1; i <= guiTexts.Length; i++)
                        {
                            if (gameObjectHit.name == ("ECU-Panel-Display-Value-" + i.ToString().PadLeft(2, '0')))
                            {
                                foundObject = true;
                                valueToPass = guiTexts[i - 1];
                                guiText = guiTexts[i - 1];
                                break;
                            }
                        }
                        if (foundObject)
                        {
                            ModClient.guiInteract(guiText);
                            if (mod.useButtonDown || mod.leftMouseDown)
                            {
                                page.Pressed_Display_Value(valueToPass, gameObjectHit);
                            }
                        }
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
                            if (mod.useButtonDown || mod.leftMouseDown)
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
            switch (selectedSetting)
            {
                case "Select 2Step RPM":
                    mod.step2_rpm.Value += 100;
                    if (mod.step2_rpm.Value >= 10000) { mod.step2_rpm.Value = 10000; }
                    break;
                case "Select Shift Indicator green line":
                    shift_indicator_greenLine += 100;
                    if(shift_indicator_greenLine >= shift_indicator_redLine) { shift_indicator_greenLine -= 100; }
                    SetupShiftIndicator();
                    break;
                case "Select Shift Indicator red line":
                    shift_indicator_redLine += 100;
                    SetupShiftIndicator();
                    break;
            }
        }

        private void Pressed_Button_Minus()
        {
            switch (selectedSetting)
            {
                case "Select 2Step RPM":
                    mod.step2_rpm.Value -= 100;
                    if (mod.step2_rpm.Value <= 2000) { mod.step2_rpm.Value = 2000; }
                    break;
                case "Select Shift Indicator green line":
                    shift_indicator_greenLine -= 100;
                    if(shift_indicator_greenLine <= shift_indicator_baseLine) { shift_indicator_greenLine += 100; }
                    SetupShiftIndicator();
                    break;
                case "Select Shift Indicator red line":
                    shift_indicator_redLine -= 100;
                    if (shift_indicator_redLine <= shift_indicator_greenLine) { shift_indicator_redLine += 100; }
                    SetupShiftIndicator();
                    break;
            }
            /*else if (currentPage == 7)
            {
                ecu_airride_logic.increaseAirride(true, true, -0.05f);
            }
            */
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
            if(currentPage > pages.Length - 1)
            {
                currentPage = 0;
            }
            ChangeInfoPanelPage(currentPage);
        }

        private void Pressed_Button_ArrowDown()
        {
            currentPage--;
            if(currentPage < 0)
            {
                currentPage = pages.Length - 1;
            }
            ChangeInfoPanelPage(currentPage);
        }

        private void ChangeInfoPanelPage(int currentPage)
        {
            ecu_InfoPanel_Background.sprite = pages[currentPage].pageSprite;
            ecu_InfoPanel_Needle.enabled = pages[currentPage].needleUsed;
            ecu_InfoPanel_TurboWheel.enabled = pages[currentPage].turbineUsed;

            foreach(KeyValuePair<string, TextMesh> display_value in display_values)
            {
                display_value.Value.text = "";
                display_value.Value.color = Color.white;
            }
            selectedSetting = "";
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

                display_values["value_1"].text = rpmDecrementer.ToString();
                display_values["value_2"].text = hundredIncrementer.ToString();
                display_values["value_3"].text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
                display_values["value_4"].text = tenIncrementer.ToString();
                display_values["value_13"].text = "";
                display_values["value_14"].text = tenIncrementer.ToString("00 .0") + "V";
                display_values["value_15"].text = thousandIncrementer.ToString();
                display_values["value_16"].text = tenIncrementer.ToString();
                display_values["value_kmh"].text = hundredIncrementer.ToString();
                display_values["value_km"].text = (thousandIncrementer * 10f).ToString();
            }
            else if (rpmIncrementer < 9000)
            {
                rpmIncrementer += rpmAdder;
                tenIncrementer += tenAdder;
                hundredIncrementer += hundredAdder;
                thousandIncrementer += thousandAdder;

                ecu_InfoPanel_NeedleObject.transform.localRotation = Quaternion.Euler(new Vector3(-90f, GetRPMRotation(rpmIncrementer), 0));

                display_values["value_1"].text = rpmIncrementer.ToString();
                display_values["value_2"].text = hundredIncrementer.ToString();
                display_values["value_3"].text = tenIncrementer.ToString() + "." + tenIncrementer.ToString();
                display_values["value_4"].text = tenIncrementer.ToString();
                display_values["value_13"].text = "Boot up";
                display_values["value_14"].text = tenIncrementer.ToString("00 .0") + "V";
                display_values["value_15"].text = thousandIncrementer.ToString();
                display_values["value_16"].text = tenIncrementer.ToString();
                display_values["value_kmh"].text = hundredIncrementer.ToString();
                display_values["value_km"].text = (thousandIncrementer * 10f).ToString();
            }
            if (rpmIncrementer >= 9000 && rpmDecrementer <= 0)
            {
                rpmIncrementer = 0;
                rpmDecrementer = 9000;
                isBooted = true;
                isBooting = false;
            }
        }

        public void Init(DonnerTech_ECU_Mod mod)
        {
            this.mod = mod;
        }

        private AudioSource dashButtonAudioSource
        {
            get
            {
                return GameObject.Find("dash_button").GetComponent<AudioSource>();
            }
        }

        public string GetSelectedSetting()
        {
            return selectedSetting;
        }
        public void SetSelectedSetting(string selectedSetting)
        {
            this.selectedSetting = selectedSetting;
        }
    }
}