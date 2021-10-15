using System.IO;
using MscModApi.Parts;

namespace DonnerTech_ECU_Mod.Parts
{
	public class ChipSave
	{
		public float sparkAngle = 20.0f;

		public bool programmed = false;
		public bool startAssistEnabled = false;
		public float[,] map = null;

		public static string[] LoadSaveFiles(string savePath, string filenaming)
		{
			return Directory.GetFiles(savePath, filenaming, SearchOption.AllDirectories);
		}
	}
}