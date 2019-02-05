using System;
using System.Collections.Generic;
using System.IO;

using XCom.Resources.Map;


namespace XCom
{
	/// <summary>
	/// Descriptors describe a tileset: a Map, its route-nodes, and terrain. It
	/// also holds the path to its files' parent directory.
	/// A descriptor is accessed *only* through a Group and Category, and is
	/// identified by its tileset-label. This allows multiple tilesets (ie. with
	/// the same label) to be configured differently according to Category and
	/// Group.
	/// </summary>
	public sealed class Descriptor // *snap*
	{
		#region Fields
		private readonly string _dirTerr; // the Configurator's terrain-path for UFO or TFTD - depends on Palette.
		#endregion


		#region Properties
		public string Label
		{ get; private set; }

		public string Basepath
		{ get; internal set; }

		private Dictionary<int, Tuple<string,string>> _terrains = new Dictionary<int, Tuple<string,string>>();
		/// <summary>
		/// A dictionary of this tileset's terrains as IDs that keys a tuple
		/// that pairs terrain-labels with basepath-strings. A basepath-string
		/// can be blank (use config's basepath), "basepath" (use the tileset's
		/// basepath), or the basepath of any TERRAIN directory.
		/// </summary>
		public Dictionary<int, Tuple<string,string>> Terrains
		{
			get { return _terrains; }
			set { _terrains = value; }
		}

		public Palette Pal // TODO: Defining the palette in both a Descriptor and its TileGroup is redundant.
		{ get; private set; }
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="tileset"></param>
		/// <param name="terrains"></param>
		/// <param name="basepath"></param>
		/// <param name="palette"></param>
		public Descriptor(
				string tileset,
				Dictionary<int, Tuple<string,string>> terrains,
				string basepath,
				Palette palette)
		{
			//LogFile.WriteLine("Descriptor cTor tileset= " + tileset);
			//LogFile.WriteLine("");

			Label    = tileset;
			Terrains = terrains;
			Basepath = basepath;
			Pal      = palette;

			_dirTerr = (Pal == Palette.UfoBattle) ? SharedSpace.ResourceDirectoryUfo
												  : SharedSpace.ResourceDirectoryTftd;
			_dirTerr = SharedSpace.Instance.GetShare(_dirTerr);
			_dirTerr = (_dirTerr != null) ? _dirTerr = Path.Combine(_dirTerr, GlobalsXC.TerrainDir)
										  : _dirTerr = String.Empty; // -> the Share can return null if resource-type is notconfigured.
		}
		#endregion


		#region Methods
		public string GetTerrainDirectory(string path)
		{
			if (String.IsNullOrEmpty(path))								// use Configurator's basepath
				return _dirTerr;

			if (path == GlobalsXC.BASEPATH)								// use this Tileset's basepath
				return Path.Combine(Basepath, GlobalsXC.TerrainDir);

			return Path.Combine(path, GlobalsXC.TerrainDir);			// use the path specified.
		}

		/// <summary>
		/// Gets the MCD-records for a given terrain in this Descriptor.
		/// </summary>
		/// <param name="id">the position of the terrain in this tileset's terrains-list</param>
		/// <returns>an McdRecordCollection containing all the parts for the Terrain</returns>
		public McdRecordCollection GetTerrainRecords(int id)
		{
			var terrain = Terrains[id];
			string terr = terrain.Item1;
			string path = terrain.Item2;

			path = GetTerrainDirectory(path);

			var tiles = XCTileFactory.CreateTileparts(
													terr, path,
													ResourceInfo.LoadSpriteset(terr, path, 2, Pal));	// NOTE: That loads the sprites in addition to
			return new McdRecordCollection(tiles);														// getting the MCD-records. here just because it can be
		}																								// concealed inside a function called GetTerrainRecords()
																										// that returns an McdRecordCollection that's why.
		/// <summary>																					// Pretty clever huh - Dr.No look out!!science!!
		/// Gets the count of MCD-records in an MCD-file.
		/// </summary>
		/// <param name="id">the position of the terrain in this tileset's terrains-list</param>
		/// <param name="suppressError">true to suppress any error</param>
		/// <returns>count of MCD-records or 0 on fail</returns>
		public int GetRecordCount(int id, bool suppressError = false)
		{
			var terrain = Terrains[id];
			string terr = terrain.Item1;
			string path = terrain.Item2;

			path = GetTerrainDirectory(path);

			return XCTileFactory.GetRecordCount(terr, path, suppressError);
		}

		/// <summary>
		/// Gets the count of sprites in a given Terrain.
		/// @note Used only by MapInfoOutputBox.Analyze()
		/// </summary>
		/// <param name="id">the position of the terrain in this tileset's terrains-list</param>
		/// <returns>count of sprites</returns>
		public int GetSpriteCount(int id)
		{
			var terrain = Terrains[id];
			string terr = terrain.Item1;
			string path = terrain.Item2;

			path = GetTerrainDirectory(path);

			return ResourceInfo.GetSpritesetCount(terr, path, Pal);
		}
		#endregion


		#region Methods (override)
		/// <summary>
		/// Overrides Object.ToString()
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Label;
		}
		#endregion
	}
}
