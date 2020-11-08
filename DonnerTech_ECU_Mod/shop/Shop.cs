using DonnerTech_ECU_Mod.fuelsystem;
using DonnerTech_ECU_Mod.shop;
using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    class Shop
    {
        private DonnerTech_ECU_Mod mod;
        private ShopItem modsShopItem;
        private AssetBundle assetBundle;
        private PartBuySave partBuySave;
        List<ProductBought> products = new List<ProductBought>();
        private List<ProductInformation> shopItems;

        public Shop(DonnerTech_ECU_Mod mod, ShopItem modsShopItem, AssetBundle assetBundle, PartBuySave partBuySave, List<ProductInformation> shopItems)
        {
            this.mod = mod;
            this.modsShopItem = modsShopItem;
            this.assetBundle = assetBundle;
            this.partBuySave = partBuySave;
            this.shopItems = shopItems;
        }

        private void PurchaseMade(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            shopItems.ForEach(delegate (ProductInformation productInformation)
            {
                if(productInformation.gameObjectName == item.gameObject.name)
                {
                    SetPartBought(true, productInformation);
                }
            });
        }
        
        private void PurchaseMadeChip(PurchaseInfo item)
        {
            
            int chip_count_before = mod.fuel_system.chip_parts.Count;
            for (int i = 0; i < item.qty; i++)
            {
                GameObject chip = GameObject.Instantiate(item.gameObject);

                int nextChipId = (chip_count_before) + i;
                Helper.SetObjectNameTagLayer(chip, "Chip" + nextChipId);

                string fuel_system_savePath = Helper.CreatePathIfNotExists(Helper.CombinePaths(new string[] { ModLoader.GetModConfigFolder(mod), "fuelSystem", "chips" }));
                string saveFile = Path.Combine(fuel_system_savePath, "chip" + nextChipId + "_saveFile.txt");
                string fuelMap_saveFile = Path.Combine(fuel_system_savePath, "chip" + nextChipId + ".fuelmap");
                
                ChipPart chip_part = new ChipPart(
                    SimplePart.LoadData(mod, saveFile, true),
                    chip,
                    mod.smart_engine_module_part.rigidPart,
                    new Trigger("chip" + nextChipId, mod.smart_engine_module_part.rigidPart, mod.fuel_system.chip_installLocation, new Quaternion(0, 0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), false),
                    mod.fuel_system.chip_installLocation,
                    new Quaternion { eulerAngles = mod.fuel_system.chip_installRotation }
                );
                chip_part.SetDisassembleFunction(new Action(mod.fuel_system.DisassembleChip));
                chip_part.fuelMap_saveFile = fuelMap_saveFile;
                chip_part.chipSave = new ChipSave();
                chip_part.saveFile = saveFile;
                mod.fuel_system.chip_parts.Add(chip_part);
            }
        }
        
        private void SetPartBought(bool bought, ProductInformation productInformation)
        {
            productInformation.bought = bought;
            switch (productInformation.productName)
            {
                case "ABS Module": partBuySave.boughtABSModule = bought; break;
                case "ESP Module": partBuySave.boughtESPModule = bought; break;
                case "TCS Module": partBuySave.boughtTCSModule = bought; break;
                case "ECU Cable Harness": partBuySave.boughtCableHarness = bought; break;
                case "ECU Mounting Plate": partBuySave.boughtMountingPlate = bought; break;
                case "Smart Engine Module ECU": partBuySave.boughtSmartEngineModule = bought; break;
                case "Cruise Control Panel with Controller": partBuySave.boughtCruiseControlPanel = bought; break;
                case "ECU Info Panel": partBuySave.boughtInfoPanel = bought; break;
                case "Rain & Light Sensorboard": partBuySave.bought_rainLightSensorboard = bought; break;
                case "Reverse Camera": partBuySave.bought_reverseCamera = bought; break;
#if DEBUG
                case "Airride fl": partBuySave.bought_airrideFL = bought; break;
                case "AWD Gearbox": partBuySave.boughtAwdGearbox = bought; break;
                case "AWD Differential": partBuySave.boughtAwdDifferential = bought; break;
                case "AWD Propshaft": partBuySave.boughtAwdPropshaft = bought; break;
#endif
                case "Fuel Injectors": partBuySave.bought_fuel_injectors_box = bought; break;
                case "Fuel Pump Cover": partBuySave.bought_fuel_pump_cover = bought; break;
                case "Fuel Injection Manifold": partBuySave.bought_fuel_injection_manifold = bought; break;
                case "Throttle Bodies": partBuySave.bought_throttle_bodies_box = bought; break;
                case "Fuel Rail": partBuySave.bought_fuel_rail = bought; break;
                case "Chip Programmer": partBuySave.bought_chip_programmer = bought; break;
                case "Electric Fuel Pump": partBuySave.bought_electric_fuel_pump = bought; break;
            }
    }

        private void AddToShop(ProductInformation productInformation)
        {
            if (!productInformation.bought)
            {
                this.modsShopItem.Add(mod, productInformation.product, ModsShop.ShopType.Fleetari, PurchaseMade, productInformation.gameObject);
                productInformation.gameObject.SetActive(false);
            }
        }

        public void SetupShopItems()
        {

            
            shopItems.ForEach(delegate (ProductInformation productInformation)
            {
                Sprite productIcon = null;
                if (productInformation.iconName != null && productInformation.iconName != "")
                {
                    productIcon = assetBundle.LoadAsset<Sprite>(productInformation.iconName);
                }
                ProductDetails product = new ModsShop.ProductDetails
                {
                    productName = productInformation.productName,
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = productIcon,
                    productPrice = productInformation.price
                };
                productInformation.product = product;
                productInformation.gameObjectName = productInformation.gameObject.name;
                AddToShop(productInformation);
            });

            ProductDetails chip_product = new ModsShop.ProductDetails
            {
                productName = "Programmable Chip",
                multiplePurchases = true,
                productCategory = "DonnerTech Racing",
                productIcon = assetBundle.LoadAsset<Sprite>("chip_productImage.png"),
                productPrice = 500
            };

            this.modsShopItem.Add(mod, chip_product, ModsShop.ShopType.Fleetari, PurchaseMadeChip, mod.chip);
        }
    }
}
