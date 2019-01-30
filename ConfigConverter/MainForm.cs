using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace ConfigConverter
{
	/// <summary>
	/// A converter for turning MapEdit.dat into a YAML file. Roughly.
	/// @note The files Images.dat and Paths.pth are required in the directory
	/// with MapEdit.dat.
	/// </summary>
	public partial class MainForm
		:
			Form
	{
		#region Fields
		const string PrePad = "#----- ";
		int PrePadLength = PrePad.Length;

		string _dir;

		string[] _linesPaths, _linesImages, _linesMapEdit;

		static StringComparer ignorecase = StringComparer.OrdinalIgnoreCase;
		Dictionary<string,string> Vars     = new Dictionary<string,string>(ignorecase);
		Dictionary<string,string> Terrains = new Dictionary<string,string>(ignorecase);
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
			input.Filter = "MapView DAT files(*.dat)|*.dat|All files(*.*)|*.*";

			if (input.ShowDialog() == DialogResult.OK)
			{
				tbInput.Text = input.FileName;

				if (Path.GetFileName(tbInput.Text) == "MapEdit.dat")
				{
					_dir = Path.GetDirectoryName(tbInput.Text);
					if (!String.IsNullOrEmpty(_dir) && File.Exists(Path.Combine(_dir, "Images.dat")))
					{
						if (File.Exists(Path.Combine(_dir, "Paths.pth")))
						{
							btnConvert.Enabled = true;
						}
						else
							MessageBox.Show(
										"Can't find Paths.pth ...",
										"Error",
										MessageBoxButtons.OK,
										MessageBoxIcon.Error,
										MessageBoxDefaultButton.Button1,
										0);
					}
					else
						MessageBox.Show(
									"Can't find Images.dat ...",
									"Error",
									MessageBoxButtons.OK,
									MessageBoxIcon.Error,
									MessageBoxDefaultButton.Button1,
									0);
				}
				else
					MessageBox.Show(
								"File is not recognized as MapEdit.dat",
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

			string pfeImages = Path.Combine(_dir, "Images.dat");
			string pfePaths  = Path.Combine(_dir, "Paths.pth");

			if (!File.Exists(tbInput.Text))
			{
				MessageBox.Show(
							"Can't find MapEdit.dat ...",
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
			else if (!File.Exists(pfeImages))
			{
				MessageBox.Show(
							"Can't find Images.dat ...",
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
			else if (!File.Exists(pfePaths))
			{
				MessageBox.Show(
							"Can't find Paths.pth ...",
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
			else
			{
				_linesPaths   = File.ReadAllLines(pfePaths);
				_linesImages  = File.ReadAllLines(pfeImages);
				_linesMapEdit = File.ReadAllLines(tbInput.Text);

				string
					configufo  = String.Empty,
					configtftd = String.Empty;

				using (var sw = new StreamWriter(File.Open(
														Path.Combine(_dir, "convert.log"),
														FileMode.Create,
														FileAccess.Write,
														FileShare.None)))
				{
					ParsePaths(sw);
					ParseImages(sw);

					//sw.WriteLine("");
					//foreach (var key0 in Vars)
					//	sw.WriteLine(key0);

					//sw.WriteLine("");
					//foreach (var key0 in Terrains)
					//	sw.WriteLine(key0);


					string val, key, l;
					string
						GROUP    = String.Empty,
						BASEPATH = String.Empty,
						CATEGORY = String.Empty,
						TILESET  = String.Empty;

					var TERRAINS = new List<string>();

					int pos;

					//sw.WriteLine("");
					foreach (var line in _linesMapEdit)
					{
						if (!String.IsNullOrEmpty(l = line.Trim())
							&& !l.StartsWith("#", StringComparison.OrdinalIgnoreCase))
						{
							//sw.WriteLine("l= " + l);

							if (l.StartsWith("$", StringComparison.OrdinalIgnoreCase))
							{
								// ${tftdPath}:${tftd}

								// ${rootPath}:${tftdPath}\MAPS\
								// ${rmpPath}:${tftdPath}\ROUTES\
								// ${blankPath}:C:\Users\Bruno\Desktop\dashivas_mapview\BLANKS\TFTD\
								// ${deps}:SAND ROCKS WEEDS PYRAMID UFOBITS

								// ${varPath59}:E:\local\UFO\ROUTES\
								// ${varPath58}:E:\local\UFO\MAPS\
								// ${varPath64}:E:\local\mods\Final_Mod_Pack\ROUTES\
								// ${varPath63}:E:\local\mods\Final_Mod_Pack\MAPS\
								// ${varPath65}:E:\local\mods\City_Addon_Terrain\MAPS\
								// ${varPath60}:E:\tools\MapView_PckView\BLANKS\UFO\
								// ${varPath62}:E:\local\mods\Hardmode_Expansion\ROUTES\
								// ${varPath61}:E:\local\mods\Hardmode_Expansion\MAPS\
								// ${varPath66}:E:\local\mods\City_Addon_Terrain\ROUTES\
								// ${varDeps67}:JUNGLE

								// ${ufoPath}:${ufo}

								// ${rootPathUFO}:${ufoPath}\MAPS\
								// ${rmpPathUFO}:${ufoPath}\ROUTES\
								// ${blankUFO}:C:\0xC_kL\editors\MapView_kL\BLANKS\UFO
								// ${dep}:COMROADS COMBITS COMMERCE COMFRN COMDECOR

								pos = l.IndexOf(':');
								key = l.Substring(0, pos);
								val = l.Substring(pos + 1, l.Length - pos - 1);

								if (val.StartsWith("$", StringComparison.OrdinalIgnoreCase))
								{
									string key1 = val.Substring(0, val.IndexOf('}') + 1);
									//sw.WriteLine(". . key1= " + key1);
									if (Vars.ContainsKey(key1))
									{
										val = val.Replace(key1, Vars[key1]); // var/val in MapEdit.dat or Paths.pth
									}
									else
									{
										MessageBox.Show(terminate(key1));
										Environment.Exit(0);
									}
								}

								if (val.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
								{
									val = val.Substring(0, val.Length - 1);
								}

								//sw.WriteLine(". key= " + key + " val= " + val);
								Vars[key] = val;
							}
							else if (l.StartsWith("Tileset", StringComparison.OrdinalIgnoreCase)) // is Group
							{
								// Tileset:TFTD - Terrain

								GROUP = l.Substring(l.IndexOf(':') + 1); // user-defined group-label: eg, "UFO - Terrain", "TFTD - Ships", etc.
								//sw.WriteLine("");
								//sw.WriteLine(GROUP);
							}
							else if (l.StartsWith("palette", StringComparison.OrdinalIgnoreCase))
							{
								string pal = l.Substring(l.IndexOf(':') + 1); // ufo or tftd.
								//sw.WriteLine(pal);

								//sw.WriteLine(". pal.StartsWith(ufo)= " + pal.StartsWith("ufo", StringComparison.OrdinalIgnoreCase));
								//sw.WriteLine(". !GROUP.StartsWith(ufo)= " + !GROUP.StartsWith("ufo", StringComparison.OrdinalIgnoreCase));

								if (      pal.StartsWith("ufo", StringComparison.OrdinalIgnoreCase) // good Lord ...
									&& !GROUP.StartsWith("ufo", StringComparison.OrdinalIgnoreCase))
								{
									GROUP = GROUP.Insert(0, "ufo_");
								}
								else if ( pal.StartsWith("tftd", StringComparison.OrdinalIgnoreCase)
									&& !GROUP.StartsWith("tftd", StringComparison.OrdinalIgnoreCase))
								{
									GROUP = GROUP.Insert(0, "tftd_");
								}
								//sw.WriteLine(". " + GROUP);
							}
							else if (l.StartsWith("rootpath", StringComparison.OrdinalIgnoreCase)) // is BasePath
							{
								// rootpath:${rootPath}
								// rootpath:${varPath58}
								// rmpPath:${varPath59}   -> ignore
								// blankPath:${varPath60} -> ignore

								BASEPATH = l.Substring(l.IndexOf(':') + 1);

								if (BASEPATH.StartsWith("$", StringComparison.OrdinalIgnoreCase))
								{
									string key1 = BASEPATH.Substring(0, BASEPATH.IndexOf('}') + 1);
									//sw.WriteLine(". . key1= " + key1);
									if (Vars.ContainsKey(key1))
									{
										BASEPATH = BASEPATH.Replace(key1, Vars[key1]); // var/val in Images.dat
									}
									else
									{
										MessageBox.Show(terminate(key1));
										Environment.Exit(0);
									}
								}

								if (BASEPATH.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
								{
									BASEPATH = BASEPATH.Substring(0, BASEPATH.Length - 1);
								}

								if (BASEPATH.EndsWith("MAPS", StringComparison.OrdinalIgnoreCase))
								{
									BASEPATH = BASEPATH.Substring(0, BASEPATH.Length - 4);
								}

								if (BASEPATH.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
								{
									BASEPATH = BASEPATH.Substring(0, BASEPATH.Length - 1);
								}
								//sw.WriteLine("BASEPATH= " + BASEPATH);
							}
							else if (l.StartsWith("files", StringComparison.OrdinalIgnoreCase)) // is Category
							{
								// files:comrcurban

								CATEGORY = l.Substring(l.IndexOf(':') + 1); // don't bother looking up in Vars dictionary
								//sw.WriteLine("CATEGORY= " + CATEGORY);
							}
							else if (!l.StartsWith("type",      StringComparison.OrdinalIgnoreCase)
								&&   !l.StartsWith("rmpPath",   StringComparison.OrdinalIgnoreCase)
								&&   !l.StartsWith("blankPath", StringComparison.OrdinalIgnoreCase)
								&& (pos = l.IndexOf(':')) != -1) // is Tileset (this has to happen last!)
							{
								//sw.WriteLine("l= " + l);

								// COMRCURBAN00:${dep}
								// XBASE_00:${dep}
								// XBASE_01:${dep}
								// XBASE_02:${dep}

								// AVENGER:AVENGER
								// CHIMERAUFO:CHIMERA
								// CH0A:CH2 CH3 CHI1 CHI2 CHI3 CHI4 CHI5

								TILESET = l.Substring(0, pos);
								//sw.WriteLine("TILESET= " + TILESET);

								TERRAINS.Clear();

								string terrains = l.Substring(pos + 1);
								//sw.WriteLine(". terrains= " + terrains);

								if (terrains.StartsWith("$", StringComparison.OrdinalIgnoreCase))
								{
									if (Vars.ContainsKey(terrains))
									{
										terrains = Vars[terrains];
									}
									else
									{
										MessageBox.Show(terminate(terrains));
										Environment.Exit(0);
									}
								}

								//sw.WriteLine("");

								string ufo;
								bool isUfo = Vars.TryGetValue("${ufo}", out ufo)
										  && GROUP.StartsWith("ufo", StringComparison.OrdinalIgnoreCase);

								string tftd;
								bool isTftd = Vars.TryGetValue("${tftd}", out tftd)
										   && GROUP.StartsWith("tftd", StringComparison.OrdinalIgnoreCase);

								string[] array = terrains.Split(' ');
								for (int a = 0; a != array.Length; ++a)
								{
									string terr = array[a];
									string path;
									//sw.WriteLine(". . terr= " + terr);

									if (Terrains.TryGetValue(terr, out path)) // psst. your .dat files are effed.
									{
										//sw.WriteLine(". . . path= " + path);
										//sw.WriteLine(". . . BASEPATH= " + BASEPATH);

										if (isUfo && path == ufo)
										{
											//sw.WriteLine(". . . . ufo configured");
											configufo = ufo;
										}
										else if (isTftd && path == tftd)
										{
											//sw.WriteLine(". . . . tftd configured");
											configtftd = tftd;
										}
										else if (path == BASEPATH)
										{
											//sw.WriteLine(". . . . use Map basepath");
											terr += ": basepath";
										}
										else
										{
											//sw.WriteLine(". . . . use fullpath");
											terr += ": " + path;
										}

										TERRAINS.Add(terr);
									}
									//sw.WriteLine(". . terr2= " + terr);
								}

								if ((isUfo && BASEPATH == Vars["${ufo}"]) || (isTftd && BASEPATH == Vars["${tftd}"]))
								{
									BASEPATH = String.Empty;
								}

								string label = TILESET;

								int incr = -1; // check for duplicate Tileset labels ->
								bool found = false;
								while (!found)
								{
									found = true;
									foreach (var tileset in tilesets)
									{
										if (tileset.Label == label)
										{
											MessageBox.Show("WARNING"
															+ Environment.NewLine + Environment.NewLine
															+ "A Map label " + label + " is duplicated in MapEdit.dat"
															+ Environment.NewLine + Environment.NewLine
															+ "It will be changed to " + TILESET + "_" + (incr + 1));

											label = TILESET + "_" + (++incr);
											found = false;

											break;
										}
									}
								}
								TILESET = label;

								tilesets.Add(new Tileset(
														TILESET,
														GROUP,
														CATEGORY,
														new List<string>(TERRAINS), // copy that, Roger.
														BASEPATH));
							}
						}
					}

					//sw.WriteLine("");

/*					foreach (Tileset tileset in tilesets)
					{
						sw.WriteLine("Tileset: " + tileset.Label);
						sw.WriteLine(". group: " + tileset.Group);
						sw.WriteLine(". categ: " + tileset.Category);
						sw.WriteLine(". basep: " + tileset.BasePath);

						foreach (string terrain in tileset.Terrains)
							sw.WriteLine(". . ter: " + terrain);
					} */
				}


				// YAML the tilesets ....
				using (var fs = new FileStream(Path.Combine(_dir, "MapTilesets.yml"), FileMode.Create))
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

					bool blankline;
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

						if (!String.IsNullOrEmpty(tileset.BasePath))
							sw.WriteLine("    basepath: " + tileset.BasePath);
					}
//					  - type: UFO_110
//					    terrains:
//					      - U_EXT02
//					      - U_WALL02
//					      - U_BITS
//					    category: UFO
//					    group: ufoShips
//					    basepath: <>
				}

				// See also:
				// http://stackoverflow.com/questions/37116684/build-a-yaml-document-dynamically-from-c-sharp/37128416

				lblResult.Text = "Finished";

				btnConvert.Enabled =
				btnInput  .Enabled = false;

				btnCancel.Select();

				if (!String.IsNullOrEmpty(configufo) || !String.IsNullOrEmpty(configtftd))
				{
					if (configufo  == null) configufo  = String.Empty;
					if (configtftd == null) configtftd = String.Empty;

					string text = "Finished" + Environment.NewLine + Environment.NewLine
								+ "MapView2's Configurator expects your resources' basepath(s) to be"
								+ Environment.NewLine;

					text += Environment.NewLine + "UFO -\t"  + configufo;
					text += Environment.NewLine + "TFTD -\t" + configtftd;

					text += Environment.NewLine + Environment.NewLine
						  + "Press Ctrl+c to copy this text.";

					MessageBox.Show(text);
				}
			}
		}
		#endregion


		#region Methods
		void ParsePaths(TextWriter sw)
		{
			//sw.WriteLine("\nParsePaths");
			string key, val, l;

			foreach (var line in _linesPaths)
			{
				// ${ufo}:C:\0xC_kL\data

				if (!String.IsNullOrEmpty(l = line.Trim())
					&& l.StartsWith("$", StringComparison.OrdinalIgnoreCase))
				{
					int pos = l.IndexOf(':');
					key = l.Substring(0, pos);
					val = l.Substring(pos + 1);//, l.Length - pos - 1);

					if (val.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						val = val.Substring(0, val.Length - 1);
					}

					//sw.WriteLine(". key= " + key + " val= " + val);
					Vars[key] = val;
				}
			}
		}

		void ParseImages(TextWriter sw)
		{
			//sw.WriteLine("\nParseImages");
			string key, val, terr, path, l;

			foreach (var line in _linesImages)
			{
				if (!String.IsNullOrEmpty(l = line.Trim()))
				{
					if (l.StartsWith("$", StringComparison.OrdinalIgnoreCase))
					{
						// ${var56}:E:\local\UFO\TERRAIN\
						// ${ufoImg}:${ufo}\TERRAIN\

						int pos = l.IndexOf(':');
						key = l.Substring(0, pos);
						val = l.Substring(pos + 1);//, l.Length - pos - 1);
						//sw.WriteLine(". key= " + key + " val= " + val);

						if (val.StartsWith("$", StringComparison.OrdinalIgnoreCase))
						{
							string key1 = val.Substring(0, val.IndexOf('}') + 1);
							//sw.WriteLine(". . key1= " + key1);
							if (Vars.ContainsKey(key1))
							{
								val = val.Replace(key1, Vars[key1]); // var/val in Paths.pth
							}
							else
							{
								MessageBox.Show(terminate(key1));
								Environment.Exit(0);
							}
						}

						if (val.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							val = val.Substring(0, val.Length - 1);
						}

						//sw.WriteLine(". key2= " + key + " val2= " + val);
						Vars[key] = val;
					}
					else
					{
						// AVENGER:${ufoImg}

						int pos = l.IndexOf(':');
						terr = l.Substring(0, pos);
						path = l.Substring(pos + 1);//, l.Length - pos - 1);

						if (path.StartsWith("$", StringComparison.OrdinalIgnoreCase))
						{
							string key1 = path.Substring(0, path.IndexOf('}') + 1);
							//sw.WriteLine(". . key1= " + key1);
							if (Vars.ContainsKey(key1))
							{
								path = path.Replace(key1, Vars[key1]); // var/val in Images.dat
							}
							else
							{
								MessageBox.Show(terminate(key1));
								Environment.Exit(0);
							}
						}

						if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							path = path.Substring(0, path.Length - 1);
						}

						if (path.EndsWith("TERRAIN", StringComparison.OrdinalIgnoreCase))
						{
							path = path.Substring(0, path.Length - 7);
						}

						if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
						{
							path = path.Substring(0, path.Length - 1);
						}

						//sw.WriteLine(". terr= " + terr + " path= " + path);
						if (Terrains.ContainsKey(terr))
						{
							MessageBox.Show("WARNING / ERROR"
											+ Environment.NewLine + Environment.NewLine
											+ "The terrain " + terr + " is redefined in Images.dat"
											+ Environment.NewLine + Environment.NewLine
											+ "pre -\t" + Terrains[terr]
											+ Environment.NewLine
											+ "post - \t" + path);
						}
						Terrains[terr] = path;
					}
				}
			}
		}

		string terminate(string key)
		{
			return "ERROR" + Environment.NewLine + Environment.NewLine
				 + "The value of " + key + " can't be found"
				 + Environment.NewLine + Environment.NewLine
				 + "gah";
		}

		/// <summary>
		/// Adds padding such as " ---#" out to 80 characters.
		/// </summary>
		/// <param name="len"></param>
		/// <returns></returns>
		string Padder(int len)
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
		struct Tileset
		{
			internal string Label
			{ get; private set; }
			internal string Group
			{ get; private set; }
			internal string Category
			{ get; private set; }
			internal List<string> Terrains
			{ get; private set; }

			internal string BasePath
			{ get; private set; }

			internal Tileset(
					string label,
					string group,
					string category,
					List<string> terrains,
					string basepath)
				:
					this()
			{
				Label    = label;
				Group    = group;
				Category = category;
				Terrains = terrains;

				BasePath = basepath;
			}
		}
		#endregion
	}
}
