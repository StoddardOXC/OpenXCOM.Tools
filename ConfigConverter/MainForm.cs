using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace ConfigConverter
{
	/// <summary>
	/// A converter for turning MapEdit.cfg/dat into a YAML file. Roughly.
	/// </summary>
	public partial class MainForm
		:
			Form
	{
		#region Fields
		private const string PrePad = "#----- ";
		private int PrePadLength = PrePad.Length;
		#endregion


		#region cTor
		/// <summary>
		/// Instantiates the ConfigConverter.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}
		#endregion


		#region EventCalls
		/// <summary>
		/// Closes the converter when the Cancel button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Opens a file browser when the find button is clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnFindInputClick(object sender, EventArgs e)
		{
			var input = new OpenFileDialog();
			input.Filter = "MapView DAT files(*.dat)|*.dat|MapView CFG files(*.cfg)|*.cfg|All files(*.*)|*.*";
			input.FilterIndex = 2;

			if (input.ShowDialog() == DialogResult.OK)
			{
				tbInput.Text = input.FileName;
				string file = Path.GetFileName(tbInput.Text);
				if (file == "MapEdit.dat" || file == "MapEdit.cfg")
				{
					btnConvert.Enabled = true;
				}
				else
					MessageBox.Show(
								"File is not recognized as a MapView config file.",
								"Error",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error,
								MessageBoxDefaultButton.Button1,
								0);
			}
		}

		/// <summary>
		/// Runs through the file parsing data into Tilesets. Then writes it to
		/// a YAML file.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnConvertClick(object sender, EventArgs e)
		{
			var tilesets = new List<Tileset>();


			using (var sw = new StreamWriter(File.Open(
													"convert.log",
													FileMode.Create,
													FileAccess.Write,
													FileShare.None)))
			{
				sw.WriteLine("Converting ...");
				sw.WriteLine("");

				using (var sr = new StreamReader(File.OpenRead(tbInput.Text)))
				{
					string terrains = String.Empty;

					string line = String.Empty;
					while ((line = sr.ReadLine()) != null)
					{
						line = line.Trim();
						sw.WriteLine("line= " + line);

						if (line.StartsWith("Tileset", StringComparison.Ordinal))
						{
							string GROUP = line.Substring(line.IndexOf(':') + 1); // user-defined group-label: eg, "UFO - Terrain", "TFTD - Ships", etc.

							while ((line = sr.ReadLine()) != null)
							{
								line = line.Trim();
								sw.WriteLine(". line= " + line);

								if (line.IndexOf("end", 0, StringComparison.OrdinalIgnoreCase) != -1)
									break;

								if (!String.IsNullOrEmpty(line))
								{
									if (line.Contains("files"))
									{
										string CATEGORY = line.Substring(line.IndexOf(':') + 1); // user-defined category-label: eg, "Jungle", "Xcom Craft", etc.

										sw.WriteLine("Create TERRAINS_alias");
										var TERRAINS_alias = new List<string>();

										while ((line = sr.ReadLine()) != null) // NOTE: this won't work if a tileset uses both aliases and literals for its terrains.
										{
											line = line.Trim();
											sw.WriteLine(". . line= " + line);

											if (line.Contains("$")) // tileset(s) is/are using aliased definitions for terrains
											{
												while (true)
												{
													if (line.StartsWith("$", StringComparison.Ordinal)) // line is terrain
													{
														sw.WriteLine("Clear TERRAINS_alias");
														TERRAINS_alias.Clear();

														terrains = line.Substring(line.IndexOf(':') + 1);

														string[] terrainsArray = terrains.Split(' ');
														for (int i = 0; i != terrainsArray.Length; ++i)
														{
															sw.WriteLine("Add TERRAIN_alias " + terrainsArray[i]);
															TERRAINS_alias.Add(terrainsArray[i]); // terrain-labels
														}
														break;
													}
													// else, line is tileset ->

													string TILESET = line.Substring(0, line.IndexOf(':')); // tileset-label

													sw.WriteLine("Add TILESET " + TILESET);
													tilesets.Add(new Tileset(
																			TILESET,
																			GROUP,
																			CATEGORY,
																			new List<string>(TERRAINS_alias))); // copy that, Roger.


													line = sr.ReadLine().Trim();

													while (String.IsNullOrEmpty(line))
														line = sr.ReadLine().Trim();
													sw.WriteLine(". . . line= " + line);

													if (line.IndexOf("end", 0, StringComparison.OrdinalIgnoreCase) != -1)
														break;
												}
											}
											else if (line.Contains(":")) // tileset(s) is/are using literal definitions for terrains
											{
												while (true)
												{
													var TERRAINS_literal = new List<string>();

													terrains = line.Substring(line.IndexOf(':') + 1);

													string[] terrainsArray = terrains.Split(' ');
													for (int i = 0; i != terrainsArray.Length; ++i)
													{
														sw.WriteLine("Add TERRAIN_literal " + terrainsArray[i]);
														TERRAINS_literal.Add(terrainsArray[i]); // terrain-labels
													}

													string TILESET = line.Substring(0, line.IndexOf(':')); // tileset-label

													sw.WriteLine("Add TILESET " + TILESET);
													tilesets.Add(new Tileset(
																			TILESET,
																			GROUP,
																			CATEGORY,
																			TERRAINS_literal));


													line = sr.ReadLine().Trim();

													while (String.IsNullOrEmpty(line))
														line = sr.ReadLine().Trim();
													sw.WriteLine(". . . line= " + line);

													if (line.IndexOf("end", 0, StringComparison.OrdinalIgnoreCase) != -1)
														break;
												}
											}

											if (line.IndexOf("end", 0, StringComparison.OrdinalIgnoreCase) != -1)
												break;
										}
									}
								}
							}
						}
					}
				}

				sw.WriteLine("");

				foreach (Tileset tileset in tilesets)
				{
					sw.WriteLine("Tileset: " + tileset.Label);
					sw.WriteLine(". group: " + tileset.Group);
					sw.WriteLine(". categ: " + tileset.Category);

					foreach (string terrain in tileset.Terrains)
						sw.WriteLine(". . ter: " + terrain);
				}
			}


			// YAML the tilesets ....
			using (var fs = new FileStream("MapTilesets.yml", FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				sw.WriteLine("# This is MapTilesets for MapViewII.");
				sw.WriteLine("#");
				sw.WriteLine("# 'tilesets' - a list that contains all the blocks.");
				sw.WriteLine("# 'type'     - the label of MAP/RMP files for the block.");
				sw.WriteLine("# 'terrains' - the label(s) of PCK/TAB/MCD files for the block. A terrain may be" + Environment.NewLine
						   + "#              defined in one of three formats:"                                  + Environment.NewLine
						   + "#              - LABEL"                                                           + Environment.NewLine
						   + "#              - LABEL: basepath"                                                 + Environment.NewLine
						   + "#              - LABEL: <basepath>"                                               + Environment.NewLine
						   + "#              The first gets the terrain from the Configurator's basepath. The"  + Environment.NewLine
						   + "#              second gets the terrain from the current Map's basepath. The"      + Environment.NewLine
						   + "#              third gets the terrain from the specified basepath (don't use"     + Environment.NewLine
						   + "#              quotes). A terrain must be in a subdirectory labeled TERRAIN.");
				sw.WriteLine("# 'category' - a header for the tileset, is arbitrary here.");
				sw.WriteLine("# 'group'    - a header for the categories, is arbitrary except that the first"   + Environment.NewLine
						   + "#              letters designate the game-type and must be either 'ufo' or"       + Environment.NewLine
						   + "#              'tftd' (case insensitive, with or without a following space).");
				sw.WriteLine("# 'basepath' - the path to the parent directory of the tileset's Map and Route"   + Environment.NewLine
						   + "#              files (default: the resource directory(s) that was/were specified" + Environment.NewLine
						   + "#              when MapView was installed/configured). Note that Maps are"        + Environment.NewLine
						   + "#              expected to be in a subdir called MAPS, Routes in a subdir called" + Environment.NewLine
						   + "#              ROUTES, but that terrains - PCK/TAB/MCD files - are referenced by" + Environment.NewLine
						   + "#              default in the basepath that is set by the Configurator and have"  + Environment.NewLine
						   + "#              to be in a subdir labeled TERRAIN of that path. But see"           + Environment.NewLine
						   + "#              'terrains' above.");
				sw.WriteLine("");
				sw.WriteLine("tilesets:");

				string headerGroup    = String.Empty;
				string headerCategory = String.Empty;

				bool blankline = false;
				foreach (Tileset tileset in tilesets)
				{
					blankline = false;
					if (headerGroup != tileset.Group)
					{
						headerGroup = tileset.Group;
						blankline = true;

						sw.WriteLine("");
						sw.WriteLine(PrePad + headerGroup + Padder(headerGroup.Length + PrePadLength));
					}

					if (headerCategory != tileset.Category)
					{
						headerCategory = tileset.Category;

						if (!blankline)
							sw.WriteLine("");
						sw.WriteLine(PrePad + headerCategory + Padder(headerCategory.Length + PrePadLength));
					}

					sw.WriteLine("  - type: " + tileset.Label);
					sw.WriteLine("    terrains:");

					foreach (string terrain in tileset.Terrains)
						sw.WriteLine("      - " + terrain);

					sw.WriteLine("    category: " + tileset.Category);
					sw.WriteLine("    group: " + tileset.Group);
				}
//				  - type: UFO_110
//				    terrains:
//				      - U_EXT02
//				      - U_WALL02
//				      - U_BITS
//				    category: UFO
//				    group: ufoShips
			}

			// See also:
			// http://stackoverflow.com/questions/37116684/build-a-yaml-document-dynamically-from-c-sharp/37128416

			lblResult.Text = "Finished";

			btnConvert.Enabled =
			btnInput.Enabled   = false;

			btnCancel.Select();
		}
		#endregion


		#region Methods
		/// <summary>
		/// Adds padding such as " ---#" out to 80 characters.
		/// </summary>
		/// <param name="len"></param>
		/// <returns></returns>
		private string Padder(int len)
		{
			string pad = String.Empty;
			if (len < 79)
				pad = " ";

			for (int i = 78; i > len; --i)
			{
				pad += "-";
			}

			if (len < 79)
				pad += "#";

			return pad;
		}

		/// <summary>
		/// The Tileset struct is the basic stuff of a tileset.
		/// </summary>
		private struct Tileset
		{
			internal string Label
			{ get; private set; }
			internal string Group
			{ get; private set; }
			internal string Category
			{ get; private set; }
			internal List<string> Terrains
			{ get; private set; }

			internal Tileset(
					string label,
					string group,
					string category,
					List<string> terrains)
				:
					this()
			{
				Label    = label;
				Group    = group;
				Category = category;
				Terrains = terrains;
			}
		}
		#endregion
	}
}
