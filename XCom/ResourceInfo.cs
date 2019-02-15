using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using DSShared;


namespace XCom
{
	public static class ResourceInfo
	{
		#region Fields (static)
		private static readonly Dictionary<Palette, Dictionary<string, SpriteCollection>> _palSpritesets
						  = new Dictionary<Palette, Dictionary<string, SpriteCollection>>();

		public static bool ReloadSprites;
		#endregion


		#region Properties (static)
		public static TileGroupManager TileGroupManager
		{ get; private set; }
		#endregion


		#region Methods (static)
		/// <summary>
		/// Initializes/ loads info about XCOM resources.
		/// </summary>
		/// <param name="pathConfig"></param>
		public static void InitializeResources(PathInfo pathConfig)
		{
//			XConsole.Init(20);

			TileGroupManager = new TileGroupManager(new TilesetLoader(pathConfig.Fullpath));
		}

		/// <summary>
		/// Loads a given spriteset for UFO or TFTD. This could go in Descriptor
		/// except the XCOM cursor-sprites load w/out a descriptor. So do the
		/// 'ExtraSprites'.
		/// @note Both UFO and TFTD use 2-byte Tab-offsetLengths for 32x40 terrain pcks
		/// (TFTD unitsprites use 4-byte Tab-offsetLengths although Bigobs 32x48 uses 2-byte)
		/// (the UFO cursor uses 2-byte but the TFTD cursor uses 4-byte)
		/// </summary>
		/// <param name="terrain">the terrain file w/out extension</param>
		/// <param name="dirTerrain">path to the directory of the terrain file</param>
		/// <param name="offsetLength"></param>
		/// <param name="pal"></param>
		/// <returns>a SpriteCollection containing all the sprites for a given Terrain</returns>
		public static SpriteCollection LoadSpriteset(
				string terrain,
				string dirTerrain,
				int offsetLength,
				Palette pal)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("ResourceInfo.LoadSpriteset");

			if (!String.IsNullOrEmpty(dirTerrain))
			{
				//LogFile.WriteLine(". dirTerrain= " + dirTerrain);
				//LogFile.WriteLine(". terrain= " + terrain);

				var pfSpriteset = Path.Combine(dirTerrain, terrain);
				//LogFile.WriteLine(". pfSpriteset= " + pfSpriteset);

				string pfePck = pfSpriteset + GlobalsXC.PckExt;
				string pfeTab = pfSpriteset + GlobalsXC.TabExt;

				if (File.Exists(pfePck) && File.Exists(pfeTab))
				{
					if (!_palSpritesets.ContainsKey(pal))
						_palSpritesets.Add(pal, new Dictionary<string, SpriteCollection>());

					var spritesets = _palSpritesets[pal];

					if (ReloadSprites) // used by XCMainWindow.OnPckSavedEvent <- TileView's pck-editor
					{
						//LogFile.WriteLine(". ReloadSprites");

						if (spritesets.ContainsKey(pfSpriteset))
							spritesets.Remove(pfSpriteset);
					}

					if (!spritesets.ContainsKey(pfSpriteset))
					{
						//LogFile.WriteLine(". . key not found in spriteset dictionary -> add new SpriteCollection");

						using (var fsPck = File.OpenRead(pfePck))
						using (var fsTab = File.OpenRead(pfeTab))
						{
							var spriteset = new SpriteCollection(
																fsPck,
																fsTab,
																offsetLength,
																pal);
							if (spriteset.Borked)
							{
								MessageBox.Show(
											"The quantity of sprites in the PCK file does not match the"
												+ " quantity of sprites expected by the TAB file."
												+ Environment.NewLine + Environment.NewLine
												+ pfePck + Environment.NewLine
												+ pfeTab,
											"Error",
											MessageBoxButtons.OK,
											MessageBoxIcon.Error,
											MessageBoxDefaultButton.Button1,
											0);
							}
							spritesets.Add(pfSpriteset, spriteset); // NOTE: Add the spriteset even if it is Borked.
						}
					}
					// else WARN: Spriteset already found in the collection.

					return _palSpritesets[pal][pfSpriteset];
				}

				MessageBox.Show(
							"Can't find files for the spriteset"
								+ Environment.NewLine + Environment.NewLine
								+ pfePck + Environment.NewLine
								+ pfeTab
								+ Environment.NewLine + Environment.NewLine
								+ "Open the Map in the TilesetEditor and re-assign the basepath"
								+ " for the TERRAIN folder of the .PCK and .TAB files.",
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
			return null;
		}

		/// <summary>
		/// Gets the count of sprites in a sprite-collection.
		/// @note Used only by MapInfoOutputBox.Analyze()
		/// </summary>
		/// <param name="terrain">the terrain file w/out extension</param>
		/// <param name="dirTerrain">path to the directory of the terrain file</param>
		/// <param name="pal"></param>
		/// <returns>count of sprites</returns>
		internal static int GetSpritesetCount(
				string terrain,
				string dirTerrain,
				Palette pal)
		{
			var pfSpriteset = Path.Combine(dirTerrain, terrain);
			return _palSpritesets[pal][pfSpriteset].Count;
		}
		#endregion
	}
}
