using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class SparkAngleInputForce : MonoBehaviour
	{

		public InputField inputField;
		private int min = 0;
		private int max = 20;
		public void OnValueChange()
		{
			try
			{
				float value = Convert.ToSingle(inputField.text);
				if (value >= max)
				{
					if (Regex.Match(inputField.text, @"\.\d").Success)
					{
						inputField.text = max.ToString("00.0");
					}
					else
					{
						inputField.text = max.ToString();
					}
				}
				else if (value <= 0)
                {
					if (Regex.Match(inputField.text, @"\.\d").Success)
					{
						inputField.text = min.ToString("0.0");
					}
					else
					{
						inputField.text = min.ToString();
					}
				}
			}
			catch
			{
				return;
			}

		}

		private char OnValidateInput(string input, int charIndex, char addedChar)
		{
			if (Char.IsNumber(addedChar) || addedChar == '.')
			{
				return addedChar;

			}
			return '\0';
		}

		void Start()
		{
			inputField.onValueChange.AddListener(delegate { OnValueChange(); });
			inputField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return OnValidateInput(input, charIndex, addedChar); };
		}

		void Update()
		{

		}
	}

}