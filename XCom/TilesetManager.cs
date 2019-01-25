using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using YamlDotNet.RepresentationModel;


namespace XCom
{
	/// <summary>
	/// A TilesetManager reads, stores, and manages all the tileset-data taken
	/// from the user-file MapTilesets.yml. It's the user-configuration for all
	/// the Maps.
	/// NOTE: Tilesets are converted into Descriptors and Tilesets are no longer
	/// used after loading is finished.
	/// </summary>
	public sealed class TilesetManager
	{
		#region Fields (static)
		// const-strings that appear in MapTilesets.yml
		private const string TILESETS = "tilesets";
		private const string GROUP    = "group";
		private const string CATEGORY = "category";
		private const string TYPE     = "type";
		private const string TERRAINS = "terrains";
		private const string BASEPATH = "basepath";
		#endregion


		#region Fields & Properties
		private List<Tileset> _tilesets = new List<Tileset>();
		internal List<Tileset> Tilesets
		{
			get { return _tilesets; }
		}

		private readonly List<string> _groups = new List<string>();
		internal List<string> Groups
		{
			get { return _groups; }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor. Reads MapTilesets.yml and imports all its data to a Tileset-
		/// object.
		/// </summary>
		/// <param name="fullpath">path+file+extension of MapTilesets.yml</param>
		public TilesetManager(string fullpath)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("TilesetManager cTor");

			// TODO: if exists(fullpath)
			// else error out.

			var progress = ProgressBarForm.Instance;
			progress.SetInfo("Parsing MapTilesets ...");

			var typeCount = 0; // TODO: optimize the reading (here & below) into a buffer.
			using (var sr = File.OpenText(fullpath))
			{
				string line = String.Empty;
				while ((line = sr.ReadLine()) != null)
				{
					if (line.Contains("- type"))
						++typeCount;
				}
			}
			progress.SetTotal(typeCount);


//mappings  will be deserialized as Dictionary<object,object>
//sequences will be deserialized as List<object>
//scalars   will be deserialized as string

			bool warned = false;

			bool isUfoConfigured  = !String.IsNullOrEmpty(SharedSpace.Instance.GetShare(SharedSpace.ResourceDirectoryUfo));
			bool isTftdConfigured = !String.IsNullOrEmpty(SharedSpace.Instance.GetShare(SharedSpace.ResourceDirectoryTftd));

			string nodeGroup, nodeCategory, nodeLabel, terr, path, nodeBasepath;

			YamlSequenceNode nodeTerrains;
			YamlScalarNode  nodetry1;
			YamlMappingNode nodetry2;

			Tuple<string,string> terrain;
			Dictionary<int, Tuple<string,string>> terrains;


			using (var sr = new StreamReader(File.OpenRead(fullpath)))
			{
				var str = new YamlStream();
				str.Load(sr);

				var nodeRoot = str.Documents[0].RootNode as YamlMappingNode;
//				foreach (var node in nodeRoot.Children) // parses YAML document divisions, ie "---"
//				{
				//LogFile.WriteLine(". node.Key(ScalarNode)= " + (YamlScalarNode)node.Key); // "tilesets"


				var nodeTilesets = nodeRoot.Children[new YamlScalarNode(TILESETS)] as YamlSequenceNode;
				foreach (YamlMappingNode nodeTileset in nodeTilesets) // iterate over all the tilesets
				{
					//LogFile.WriteLine(". . nodeTilesets= " + nodeTilesets); // lists all data in the tileset

					// IMPORTANT: ensure that tileset-labels (ie, type) and terrain-labels
					// (ie, terrains) are stored and used only as UpperCASE strings.


					// get the Group of the tileset
					nodeGroup = nodeTileset.Children[new YamlScalarNode(GROUP)].ToString();
					//LogFile.WriteLine(". . group= " + nodeGroup); // eg. "ufoShips"

					if (   (isUfoConfigured  && nodeGroup.StartsWith("ufo",  StringComparison.OrdinalIgnoreCase))
						|| (isTftdConfigured && nodeGroup.StartsWith("tftd", StringComparison.OrdinalIgnoreCase)))
					{
						if (!Groups.Contains(nodeGroup))
							Groups.Add(nodeGroup);


						// get the Category of the tileset ->
						nodeCategory = nodeTileset.Children[new YamlScalarNode(CATEGORY)].ToString();
						//LogFile.WriteLine(". . category= " + nodeCategory); // eg. "Ufo"


						// get the Label of the tileset ->
						nodeLabel = nodeTileset.Children[new YamlScalarNode(TYPE)].ToString();
						nodeLabel = nodeLabel.ToUpperInvariant();
						//LogFile.WriteLine("\n. . type= " + nodeLabel); // eg. "UFO_110"


						// get the Terrains of the tileset ->
//						var terrainList = new List<string>();
						terrains = new Dictionary<int, Tuple<string,string>>();

						nodeTerrains = nodeTileset.Children[new YamlScalarNode(TERRAINS)] as YamlSequenceNode;
						for (int i = 0; i != nodeTerrains.Children.Count; ++i)
						{
							terr = null;
							path = null; // NOTE: 'path' will *not* be appended w/ "TERRAIN" here.

							nodetry1 = nodeTerrains[i] as YamlScalarNode;
							//LogFile.WriteLine(". . . nodetry1= " + nodetry1); // eg. "U_EXT02"

							if (nodetry1 != null) // ie. ':' not found. Use Configurator basepath ...
							{
								terr = nodetry1.ToString();
								path = String.Empty;
							}
							else // has ':' + path
							{
								nodetry2 = nodeTerrains[i] as YamlMappingNode;
								//LogFile.WriteLine(". . . nodetry2= " + nodetry2); // eg. "{ { U_EXT02, basepath } }"

								foreach (var keyval in nodetry2.Children) // note: there's only one keyval in a terrain-node.
								{
									terr = keyval.Key.ToString();
									path = keyval.Value.ToString();
								}
							}

							terr = terr.ToUpperInvariant();

							//LogFile.WriteLine(". terr= " + terr);
							//LogFile.WriteLine(". path= " + path);

							terrain = new Tuple<string,string>(terr, path);
							terrains[i] = terrain;
						}


						// get the BasePath of the tileset ->
						nodeBasepath = String.Empty;
						var basepath = new YamlScalarNode(BASEPATH);
						if (nodeTileset.Children.ContainsKey(basepath))
						{
							nodeBasepath = nodeTileset.Children[basepath].ToString();
							//LogFile.WriteLine(". . basepath= " + nodeBasepath);
						}
						//else LogFile.WriteLine(". . basepath not found.");


						var tileset = new Tileset(
												nodeLabel,
												nodeGroup,
												nodeCategory,
												terrains, //terrainList
												nodeBasepath);
						Tilesets.Add(tileset);

						progress.UpdateProgress();
					}
					else if (!warned)
					{
						warned = true;
						MessageBox.Show(
									"This warning can be ignored safely on your firstrun of MapView2."
										+ Environment.NewLine + Environment.NewLine
										+ "A group was found for which the Resource paths (UFO or TFTD) have not been"
										+ " configured. SAVING THE MAPTREE WILL REMOVE SUCH GROUPS FROM MapTilesets.yml"
										+ " - Proceed with caution. Perhaps backup your current MapTilesets.yml in the"
										+ " /settings subfolder."
										+ Environment.NewLine + Environment.NewLine
										+ "The default MapTilesets.yml (tileset configs) defines both UFO and"
										+ " TFTD tilesets and can be regenerated with the Configurator later."
										+ " But if you have defined any custom tilesets it's strongly advised"
										+ " to backup that file.",
									"Warning",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning,
									MessageBoxDefaultButton.Button1,
									0);
					}
				}
			}
			progress.Hide();
		}
		#endregion
	}
}
