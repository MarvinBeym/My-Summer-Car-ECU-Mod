using ModsShop;
using MscModApi;
using MscModApi.Parts;
using UnityEngine;

namespace ModShop
{
	class ProductInformation
	{
		public GameObject gameObject;
		public Part part;
		public Part[] parts;
		public bool usingPart = false;

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

		public ProductInformation(Part part, string productName, float price, string iconName)
		{
			usingPart = true;
			this.part = part;
			this.gameObject = part.gameObject;
			this.productName = productName;
			this.price = price;
			this.iconName = iconName;
			this.bought = part.GetBought();
		}
	}
}