using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    class Shop
    {
        private DonnerTech_ECU_Mod donnerTech_ECU_Mod;
        private ShopItem modsShopItem;
        private AssetBundle assetBundle;
        private PartBuySave partBuySave;
        private IDictionary<string, Part> shopItems;
        List<ProductBought> products = new List<ProductBought>();

        public Shop(DonnerTech_ECU_Mod donnerTech_ECU_Mod, ShopItem modsShopItem, AssetBundle assetBundle, PartBuySave partBuySave, IDictionary<string, Part> shopItems)
        {
            this.donnerTech_ECU_Mod = donnerTech_ECU_Mod;
            this.modsShopItem = modsShopItem;
            this.assetBundle = assetBundle;
            this.partBuySave = partBuySave;
            this.shopItems = shopItems;
        }

        private void PurchaseMade(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            products.ForEach(delegate (ProductBought product)
            {
               
                if(product.gameObjectName == item.gameObject.name)
                {
                    SetPartBought(true, product.product.productName);
                    return;

                }
                
            });
        }

        private void SetPartBought(bool bought, string productName)
        {

            switch (productName)
            {
                case "ABS Module ECU": partBuySave.boughtABSModule = bought; break;
                case "ESP Module ECU": partBuySave.boughtESPModule = bought; break;
                case "TCS Module ECU": partBuySave.boughtTCSModule = bought; break;
                case "ECU Cable Harness": partBuySave.boughtCableHarness = bought; break;
                case "ECU Mounting Plate": partBuySave.boughtMountingPlate = bought; break;
                case "Smart Engine Module ECU": partBuySave.boughtSmartEngineModule = bought; break;
                case "Cruise Control Panel with Controller": partBuySave.boughtCruiseControlPanel = bought; break;
                case "ECU Info Panel": partBuySave.boughtInfoPanel = bought; break;
                case "Rain & Light Sensorboard": partBuySave.bought_rainLightSensorboard = bought; break;
                case "Reverse Camera": partBuySave.bought_reverseCamera = bought; break;
#if DEBUG
                case "Airride FL": partBuySave.bought_airrideFL = bought; break;
                case "AWD Gearbox": partBuySave.boughtAwdGearbox = bought; break;
                case "AWD Differential": partBuySave.boughtAwdDifferential = bought; break;
                case "AWD Propshaft": partBuySave.boughtAwdPropshaft = bought; break;
#endif
            }
    }

        private void AddToShop(bool bought, ProductBought product)
        {
            if (!product.bought)
            {
                product.gameObjectName = this.shopItems[product.product.productName].activePart.name;
                this.modsShopItem.Add(this.donnerTech_ECU_Mod, product.product, ModsShop.ShopType.Fleetari, PurchaseMade, this.shopItems[product.product.productName].activePart);
                shopItems[product.product.productName].activePart.SetActive(false);
            }
        }

        public void SetupShopItems()
        {
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "ABS Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_abs_module_productImage.png"),
                    productPrice = 800
                }, bought = partBuySave.boughtABSModule
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "ESP Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_esp_module_productImage.png"),

                    productPrice = 1200
                }, bought = partBuySave.boughtESPModule
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "TCS Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_tcs_module_productImage.png"),
                    productPrice = 1800
                }, bought = partBuySave.boughtTCSModule
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "ECU Cable Harness",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_cable_harness_productImage.png"),
                    productPrice = 300
                }, bought = partBuySave.boughtCableHarness
                });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "ECU Mounting Plate",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_mounting_plate_productImage.png"),
                    productPrice = 100
                }, bought = partBuySave.boughtMountingPlate
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "Smart Engine Module ECU",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_smart_engine_module_productImage.png"),
                    productPrice = 4600
                }, bought = partBuySave.boughtSmartEngineModule
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "Cruise Control Panel with Controller",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_cruise_control_productImage.png"),
                    productPrice = 2000
                }, bought = partBuySave.boughtCruiseControlPanel
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "ECU Info Panel",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_info_panel_productImage.png"),
                    productPrice = 4000
                }, bought = partBuySave.boughtInfoPanel
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "Rain & Light Sensorboard",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_rainLightSensorboard_productImage.png"),
                    productPrice = 1000
                }, bought = partBuySave.bought_rainLightSensorboard
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "Reverse Camera",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    productIcon = assetBundle.LoadAsset<Sprite>("ecu_reverseCamera_productImage.png"),
                    productPrice = 1500
                }, bought = partBuySave.bought_reverseCamera
            });

#if DEBUG
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "Airride FL",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assetBundle.LoadAsset<Sprite>("ecu_reverseCamera_productImage.png"),
                    productPrice = 500
                }, bought = partBuySave.bought_airrideFL
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "AWD Gearbox",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assetBundle.LoadAsset<Sprite>("CruiseControlPanel_ProductImage.png"),
                    productPrice = 4000
                }, bought = partBuySave.boughtAwdGearbox
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "AWD Differential",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assetBundle.LoadAsset<Sprite>("CruiseControlPanel_ProductImage.png"),
                    productPrice = 4000
                }, bought = partBuySave.boughtAwdDifferential
            });
            products.Add(new ProductBought
            {
                product = new ModsShop.ProductDetails
                {
                    productName = "AWD Propshaft",
                    multiplePurchases = false,
                    productCategory = "DonnerTech Racing",
                    //productIcon = assetBundle.LoadAsset<Sprite>("CruiseControlPanel_ProductImage.png"),
                    productPrice = 4000
                }, bought = partBuySave.boughtAwdPropshaft
            });
#endif
            products.ForEach(delegate (ProductBought product)
            {
                AddToShop(product.bought, product);
            });
        }
    }
}
