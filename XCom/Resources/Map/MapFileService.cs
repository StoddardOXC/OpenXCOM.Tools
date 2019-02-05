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
		#region Fields
		public const int MAX_MCDRECORDS = 254;
		#endregion


		#region Methods
		/// <summary>
		/// Loads a tileset. Called by XCMainWindow.LoadSelectedMap()
		/// @note Check that 'descriptor' is not null before call.
		/// </summary>
		/// <param name="descriptor"></param>
		/// <param name="treechanged"></param>
		/// <returns></returns>
		public static MapFileBase LoadTileset(Descriptor descriptor, ref bool treechanged)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("MapFileService.LoadTileset descriptor= " + descriptor);

			string pfeMap = descriptor.Basepath;
			if (!String.IsNullOrEmpty(pfeMap)) // -> the BasePath can be null if resource-type is notconfigured.
			{
				pfeMap = Path.Combine(
									Path.Combine(descriptor.Basepath, GlobalsXC.MapsDir),
									descriptor.Label + GlobalsXC.MapExt);
			}
			//LogFile.WriteLine(". pfeMap= " + pfeMap);

			if (!File.Exists(pfeMap)) // Open a folderbrowser for user to point to a basepath ->
			{
				if (MessageBox.Show(
							"The Mapfile does not exist."
								+ Environment.NewLine + Environment.NewLine
								+ "Do you want to browse for a different basepath to the .MAP and .RMP files?",
							"Warning",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0) == DialogResult.Yes)
				{
					using (var fbd = new FolderBrowserDialog())
					{
						string basepath = descriptor.Basepath;
						if (!String.IsNullOrEmpty(basepath)
							&& Directory.Exists(basepath))
						{
							fbd.SelectedPath = basepath;
						}
						// TODO: Check descriptor's Palette and default to Ufo/Tftd Resource dir instead.

						fbd.Description = String.Format(
													System.Globalization.CultureInfo.CurrentCulture,
													"Browse to a basepath folder. A valid basepath folder"
														+ " has the subfolders MAPS and ROUTES.");

						if (fbd.ShowDialog() == DialogResult.OK)
						{
							pfeMap = Path.Combine(fbd.SelectedPath, GlobalsXC.MapsDir);
							pfeMap = Path.Combine(pfeMap, descriptor.Label + GlobalsXC.MapExt);

							if (File.Exists(pfeMap))
							{
								descriptor.Basepath = fbd.SelectedPath;
								treechanged = true;
							}
							else
								MessageBox.Show(
											descriptor.Label + GlobalsXC.MapExt
												+ " was not found in that basepath.",
											"Error",
											MessageBoxButtons.OK,
											MessageBoxIcon.Error,
											MessageBoxDefaultButton.Button1,
											0);
						}
					}
				}
			}

			if (File.Exists(pfeMap))
			{
				//LogFile.WriteLine(". . Map file exists");

				var tileparts = new List<TilepartBase>();

				for (int i = 0; i != descriptor.Terrains.Count; ++i) // push together the tileparts of all allocated terrains
				{
					//LogFile.WriteLine(". . . terrain= " + descriptor.Terrains[i].Item1 + " : " + descriptor.Terrains[i].Item2);

					var MCD = descriptor.GetTerrainRecords(i); // NOTE: Calls ResourceInfo.LoadSpriteset() also.
					foreach (Tilepart tilepart in MCD)
						tileparts.Add(tilepart);
				}

				if (tileparts.Count != 0)
				{
					if (tileparts.Count > MAX_MCDRECORDS) // issue warning ->
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
						text += Environment.NewLine + "total - " + tileparts.Count;

						MapFileWarn.Instance.Show();
						MapFileWarn.Instance.SetText(descriptor.Label, text);
					}

					var RMP = new RouteNodeCollection(descriptor.Label, descriptor.Basepath);
					var MAP = new MapFileChild(
											descriptor,
											tileparts,
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

			return null;
		}
		#endregion
	}
}
