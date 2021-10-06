using DonnerTech_ECU_Mod.fuelsystem;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod.old_file_checker
{
    public class SaveFileRenamer: OldFileRenamer
    {
        public SaveFileRenamer(Mod mod, int guiWidth) : base(mod, guiWidth)
        {
            oldToNew.Add("ecu_mod_ABSModule_partSave.txt", "abs_module_saveFile.json");
            oldToNew.Add("ecu_mod_ESPModule_partSave.txt", "esp_module_saveFile.json");
            oldToNew.Add("ecu_mod_TCSModule_partSave.txt", "tcs_module_saveFile.json");
            oldToNew.Add("ecu_mod_CableHarness_partSave.txt", "cable_harness_saveFile.json");
            oldToNew.Add("ecu_mod_MountingPlate_partSave.txt", "mounting_plate_saveFile.json");
            oldToNew.Add("ecu_mod_ModShop_SaveFile.txt", "mod_shop_saveFile.json");
            oldToNew.Add("ecu_mod_SmartEngineModule_partSave.txt", "smart_engine_module_saveFile.json");
            oldToNew.Add("ecu_mod_CruiseControlPanel_partSave.txt", "cruise_control_panel_saveFile.json");
            oldToNew.Add("ecu_InfoPanel_partSave.txt", "info_panel_saveFile.json");
            oldToNew.Add("ecu_reverseCamera_saveFile.txt", "reverse_camera_saveFile.json");
            oldToNew.Add("ecu_rainLightSensorboard_saveFile.txt", "rain_light_sensor_board_saveFile.json");
            oldToNew.Add("ecu_mod_screwable_save.txt", "screwable_saveFile.json");

            oldToNew.Add("abs_module_saveFile.txt", "abs_module_saveFile.json");
            oldToNew.Add("esp_module_saveFile.txt", "esp_module_saveFile.json");
            oldToNew.Add("tcs_module_saveFile.txt", "tcs_module_saveFile.json");
            oldToNew.Add("cable_harness_saveFile.txt", "cable_harness_saveFile.json");
            oldToNew.Add("mounting_plate_saveFile.txt", "mounting_plate_saveFile.json");
            oldToNew.Add("mod_shop_saveFile.txt", "mod_shop_saveFile.json");
            oldToNew.Add("smart_engine_module_saveFile.txt", "smart_engine_module_saveFile.json");
            oldToNew.Add("cruise_control_panel_saveFile.txt", "cruise_control_panel_saveFile.json");
            oldToNew.Add("info_panel_saveFile.txt", "info_panel_saveFile.json");
            oldToNew.Add("reverse_camera_saveFile.txt", "reverse_camera_saveFile.json");
            oldToNew.Add("rain_light_sensor_board_saveFile.txt", "rain_light_sensor_board_saveFile.json");
            oldToNew.Add("screwable_saveFile.txt", "screwable_saveFile.json");

            oldToNew.Add("fuelSystem\\original_parts_saveFile.txt", "fuelSystem\\original_parts_saveFile.json");
            string fuel_system_savePath = Helper.CombinePaths(new string[] { ModLoader.GetModSettingsFolder(mod), "fuelSystem", "chips" });
            if (Directory.Exists(fuel_system_savePath))
            {
                string[] fuelSystemChipSaveFiles = ChipSave.LoadSaveFiles(fuel_system_savePath, "chip*_saveFile.txt");
                foreach (string saveFilePathFull in fuelSystemChipSaveFiles)
                {
                    string saveFilePath = Helper.CombinePaths(new string[] { "fuelSystem", "chips", Path.GetFileName(saveFilePathFull) });
                    oldToNew.Add(saveFilePath, saveFilePath.Replace(".txt", ".json"));
                }
            }


            RenameOldFiles(ModLoader.GetModSettingsFolder(mod), oldToNew);
        }
    }
}
