using ModsShop;
using MSCLoader;
using System;
using System.Collections.Generic;
using MscPartApi;
using UnityEngine;
using Tools;


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
                    if (productInformation.usingPart)
                    {
                        productInformation.part.SetBought(true);
                    }
                    else
                    {
                        foreach(Part part in productInformation.parts)
                        {
                            part.SetBought(true);
                        }
                        
                    }
                    
                }
            });
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
    }
}
