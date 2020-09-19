using ModsShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DonnerTech_ECU_Mod
{
    class ProductBought
    {
        public ProductDetails product { get; set; }
        public bool bought { get; set; } = false;
        public string gameObjectName { get; set; }
    }
}
