using DonnerTech_ECU_Mod.fuelsystem;
using DonnerTech_ECU_Mod.shop;
using ModApi.Attachable;
using ModsShop;
using MSCLoader;
using Parts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tools;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    class Shop
    {
        private DonnerTech_ECU_Mod mod;
        private ShopItem modsShopItem;
        private AssetBundle assetBundle;
        private List<ProductInformation> shopItems;

        public Shop(DonnerTech_ECU_Mod mod, ShopItem modsShopItem, AssetBundle assetBundle, List<ProductInformation> shopItems)
        {
            this.mod = mod;
            this.modsShopItem = modsShopItem;
            this.assetBundle = assetBundle;
            this.shopItems = shopItems;
        }

        private void PurchaseMade(PurchaseInfo item)
        {
            item.gameObject.transform.position = ModsShop.FleetariSpawnLocation.desk;
            item.gameObject.SetActive(true);
            shopItems.ForEach(delegate (ProductInformation productInformation)
            {
                if (productInformation.gameObjectName == item.gameObject.name)
                {
                    if (productInformation.usingSimplePart)
                    {
                        SetPartBought(true, productInformation.part);
                    }
                    else
                    {
                        foreach (SimplePart part in productInformation.parts)
                        {
                            SetPartBought(true, part);
                        }

                    }

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
                string saveFile = Path.Combine(fuel_system_savePath, "chip" + nextChipId + "_saveFile.json");
                string fuelMap_saveFile = Path.Combine(fuel_system_savePath, "chip" + nextChipId + ".fuelmap");
                
                ChipPart chip_part = new ChipPart(
                    SimplePart.LoadData(mod, "chip" + nextChipId, null),
                    chip,
                    mod.smart_engine_module_part.rigidPart,
                    mod.fuel_system.chip_installLocation,
                    new Quaternion { eulerAngles = mod.fuel_system.chip_installRotation }
                );
                chip_part.SetDisassembleFunction(new Action(mod.fuel_system.DisassembleChip));
                chip_part.mapSaveFile = fuelMap_saveFile;
                chip_part.chipSave = new ChipSave();
                chip_part.saveFile = saveFile;
                mod.fuel_system.chip_parts.Add(chip_part);
            }
        }
        private void SetPartBought(bool bought, SimplePart part)
        {
            part.bought = true;
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
