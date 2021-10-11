using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class ChipSave
	{
		public float sparkAngle = 20.0f;

		public bool chipProgrammed = false;
		public bool startAssistEnabled = false;
		public float[,] map = null;

		public static string[] LoadSaveFiles(string savePath, string filenaming)
		{
			return Directory.GetFiles(savePath, filenaming, SearchOption.AllDirectories);
		}
	}
}