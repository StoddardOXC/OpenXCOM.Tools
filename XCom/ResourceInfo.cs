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
		public static TileGroupManager TileGroupInfo
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

			TileGroupInfo = new TileGroupManager(new TilesetManager(pathConfig.Fullpath));
		}

		/// <summary>
		/// Loads a given spriteset for UFO or TFTD. This could go in Descriptor
		/// except the XCOM cursor-sprites load w/out a descriptor. So do the
		/// 'ExtraSprites'.
		/// </summary>
		/// <param name="terrain"></param>
		/// <param name="dirTerrain"></param>
		/// <param name="offsetLength"></param>
		/// <param name="pal"></param>
		/// <returns></returns>
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
				//LogFile.WriteLine(". path= " + path);
				//LogFile.WriteLine(". file= " + file);

				var pfSpriteset = Path.Combine(dirTerrain, terrain);
				//LogFile.WriteLine(". pf= " + pf);

				string pfePck = pfSpriteset + SpriteCollection.PckExt;
				string pfeTab = pfSpriteset + SpriteCollection.TabExt;

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
							spritesets.Add(pfSpriteset, new SpriteCollection(
																		fsPck,
																		fsTab,
																		offsetLength,
																		pal));
						}
					}
					return _palSpritesets[pal][pfSpriteset];
				}

				MessageBox.Show(
							"Can't find files for spriteset"
								+ Environment.NewLine + Environment.NewLine
								+ pfePck + Environment.NewLine
								+ pfeTab,
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
			return null;
		}
		#endregion
	}
}
