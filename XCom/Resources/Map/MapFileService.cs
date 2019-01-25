using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using XCom.Interfaces.Base;


namespace XCom
{
	/// <summary>
	/// Instantiates a Map with its Routes and Terrains.
	/// </summary>
	public static class MapFileService
	{
		#region Methods
		/// <summary>
		/// Loads a tileset. Called by XCMainWindow.LoadSelectedMap()
		/// @note Check that 'descriptor' is not null before call.
		/// </summary>
		/// <param name="descriptor"></param>
		/// <returns></returns>
		public static MapFileBase LoadTileset(Descriptor descriptor)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("MapFileService.LoadTileset descriptor= " + descriptor);

			string dirMap = Path.Combine(descriptor.BasePath, GlobalsXC.MapsDir);
			string pfeMap = Path.Combine(
									dirMap,
									descriptor.Label + GlobalsXC.MapExt);
			//LogFile.WriteLine(". pfeMap= " + pfeMap);

			if (File.Exists(pfeMap))
			{
				//LogFile.WriteLine(". . Map file exists");

				var parts = new List<TilepartBase>();

				for (int i = 0; i != descriptor.Terrains.Count; ++i) // push together the tileparts of all allocated terrains
				{
					var MCD = descriptor.GetTerrainRecords(i);
					foreach (Tilepart part in MCD)
						parts.Add(part);
				}

				if (parts.Count != 0)
				{
					if (parts.Count > 256) // issue warning ->
					{
						string text = String.Empty;

						int lengthTotal = 0;
						for (int i = 0; i != descriptor.Terrains.Count; ++i) // do it again ...
						{
							string terrain = descriptor.Terrains[i].Item1;
							if (terrain.Length > lengthTotal)
								lengthTotal = terrain.Length;
						}

						for (int i = 0; i != descriptor.Terrains.Count; ++i) // do it again ...
						{
							string terrain = descriptor.Terrains[i].Item1;
							string st = terrain;

							int length = terrain.Length;
							while (length++ != lengthTotal)
								st += " ";

							int records = descriptor.GetRecordCount(i);
							text += st + " - " + records + Environment.NewLine;
						}
						text += Environment.NewLine + "total - " + parts.Count;

						MapFileError.Instance.Show();
						MapFileError.Instance.SetText(descriptor.Label, text);
					}

					var RMP = new RouteNodeCollection(descriptor.Label, descriptor.BasePath);
					var MAP = new MapFileChild(
											descriptor,
											parts,
											RMP);
					return MAP;
				}

				//LogFile.WriteLine(". . . descriptor has no terrains");
				MessageBox.Show(
							"There are no terrains allocated or they do not contain MCD records.",
							"Warning",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0);
			}
			else
			{
				//LogFile.WriteLine(". . Mapfile does NOT exist");
				MessageBox.Show(
							"The Mapfile does not exist.",
							"Warning",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0);
			}

			return null;
		}
		#endregion
	}
}
