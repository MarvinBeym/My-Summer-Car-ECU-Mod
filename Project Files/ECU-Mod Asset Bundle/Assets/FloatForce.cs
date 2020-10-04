using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class FloatForce : MonoBehaviour {

	public InputField inputField;
	private float inputValue;
	// Use this for initialization
	private Color orange = new Color32(255, 133, 105, 255);
	private Color khaki = new Color32(240, 230, 140, 255);
	public void OnValueChange()
	{
		if(inputField.text == "")
        {
			inputField.image.color = Color.white;
			return;
		}
        if (inputField.text.StartsWith("0"))
        {
			inputField.text = inputField.text.Remove(0, 1);
        }

        try
        {
			float value = Convert.ToSingle(inputField.text);
			Debug.Log(value);
			if (value > 22)
			{
				inputField.image.color = Color.red;
				if (Regex.Match(inputField.text, @"\.\d").Success)
				{
					inputField.text = "22.0";
				}
				else
				{
					inputField.text = "22";
				}
			}
			else if(value < 10)
            {
				
				inputField.image.color = Color.cyan;
				if (Regex.Match(inputField.text, @"\.\d").Success)
				{
					inputField.text = "10.0";
				}
				else
				{
					inputField.text = "10";
				}
			}

			if(value <= 15f)
            {
				//Lean
				inputField.image.color = Color.cyan;
			}
			else if(value <= 16f) {
				inputField.image.color = Color.green;
			}
			else if(value <= 17f)
            {
				inputField.image.color = orange;
			}
            else
            {
				inputField.image.color = Color.red;
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

	void Start () {
		inputField.onValueChange.AddListener(delegate { OnValueChange(); });
		inputField.onValidateInput += delegate (string input, int charIndex, char addedChar) { return OnValidateInput(input, charIndex, addedChar); };
	}



    // Update is called once per frame
    void Update () {
		/*
				//inputField.image.color = Color.green; //Set background color
		
		if (inputValue >= 20.0f)
        {
			inputField.text = "20.0";

		}
		else if(inputValue <= 0)
        {
			inputField.text = "00.0";

		}
		*/
	}
}
