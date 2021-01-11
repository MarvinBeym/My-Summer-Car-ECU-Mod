﻿using ModsShop;
using Parts;
using UnityEngine;

namespace ModShop
{
    class ProductInformation
    {
        public GameObject gameObject;
        public AdvPart part;
        public AdvPart[] parts;
        public bool usingSimplePart = false;

        public string productName;
        public float price;
        public string iconName;
        public bool bought;
        public string gameObjectName;

        public ProductDetails product;
        public ProductInformation(Kit kit, string productName, float price, string iconName)
        {
            this.gameObject = kit.kitBox;
            parts = kit.parts;
            this.productName = productName;
            this.price = price;
            this.iconName = iconName;
            this.bought = kit.bought;
        }

        public ProductInformation(Box box, string productName, float price, string iconName)
        {
            this.gameObject = box.box;
            parts = box.parts;
            this.productName = productName;
            this.price = price;
            this.iconName = iconName;
            this.bought = box.bought;
        }

        public ProductInformation(AdvPart part, string productName, float price, string iconName)
        {
            usingSimplePart = true;
            this.part = part;
            this.gameObject = part.activePart;
            this.productName = productName;
            this.price = price;
            this.iconName = iconName;
            this.bought = part.bought;
        }


    }
}
