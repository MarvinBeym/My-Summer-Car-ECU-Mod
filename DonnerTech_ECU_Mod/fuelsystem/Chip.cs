using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MscModApi;
using MscModApi.Parts;
using MscModApi.Tools;
using Tools;


namespace DonnerTech_ECU_Mod.fuelsystem
{
	public class Chip
	{
		internal ChipSave chipSave;
		internal string mapSaveFile;
		internal Part part;
		internal bool chipInstalledOnProgrammer;

		public Chip(Part part)
		{
			this.part = part;
		}

		internal void SaveFuelMap(Mod mod)
		{
			try
			{
				SaveLoad.SerializeSaveFile<ChipSave>(mod, chipSave, mapSaveFile);
			}
			catch (Exception ex)
			{
				Logger.New("Unable to save chips, there was an error while trying to save the chip",
					$"save file: {mapSaveFile}", ex);
			}
		}
	}
}