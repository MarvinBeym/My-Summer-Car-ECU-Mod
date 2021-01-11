﻿using ModsShop;
using MSCLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using Tools;
using Parts;

namespace ModShop
{
    class Shop
    {
        private Mod mod;
        private ShopItem modsShopItem;
        private AssetBundle assetBundle;
        private List<ProductInformation> shopItems;

        public Shop(Mod mod, ShopItem modsShopItem, AssetBundle assetBundle, List<ProductInformation> shopItems)
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
                if(productInformation.gameObjectName == item.gameObject.name)
                {
                    if (productInformation.usingSimplePart)
                    {
                        SetPartBought(true, productInformation.part);
                    }
                    else
                    {
                        foreach(AdvPart part in productInformation.parts)
                        {
                            SetPartBought(true, part);
                        }
                        
                    }
                    
                }
            });
        }
        
        private void SetPartBought(bool bought, AdvPart part)
        {
            part.bought = bought;
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
        }

        internal static void Save(Mod mod, string saveFile, AdvPart[] parts)
        {
            try
            {
                Dictionary<string, bool> save = new Dictionary<string, bool>();
                foreach (AdvPart part in parts)
                {
                    save[part.boughtId] = part.bought;
                }
                SaveLoad.SerializeSaveFile<Dictionary<string, bool>>(mod, save, saveFile);
            }
            catch (Exception ex)
            {
                Logger.New("Error while trying to save shop information", ex);
            }
        }
    }
}
