using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using XCom.Interfaces.Base;
using XCom.Resources.Map.RouteData;
using XCom.Services;


namespace XCom
{
	public sealed class MapFileChild
		:
			MapFileBase
	{
		#region Properties
		private string FullPath
		{ get; set; }

		public Dictionary<int, Tuple<string,string>> Terrains
		{ get; private set; }

		public RouteNodeCollection Routes
		{ get; private set; }
		#endregion


		public bool IsLoadChanged;

		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="descriptor"></param>
		/// <param name="parts"></param>
		/// <param name="routes"></param>
		internal MapFileChild(
				Descriptor descriptor,
				List<TilepartBase> parts,
				RouteNodeCollection routes)
			:
				base(descriptor, parts)
		{
			string dirMap = Path.Combine(Descriptor.BasePath, GlobalsXC.MapsDir);
			FullPath = Path.Combine(
								dirMap,
								Descriptor.Label + GlobalsXC.MapExt);

			Terrains = Descriptor.Terrains;

			Routes = routes;

			if (File.Exists(FullPath))
			{
				for (int i = 0; i != parts.Count; ++i)
					parts[i].SetId = i;

				ReadMapFile(parts);
				SetupRouteNodes();
				CalculateOccultations();
			}
			else
			{
				string error = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"The file does not exist{0}{0}{1}",
										Environment.NewLine,
										FullPath);
				MessageBox.Show(
							error,
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
		}
		#endregion


		#region Methods
		/// <summary>
		/// Reads a .MAP file.
		/// </summary>
		/// <param name="parts">a list of tileset-parts</param>
		private void ReadMapFile(List<TilepartBase> parts)
		{
			using (var bs = new BufferedStream(File.OpenRead(FullPath)))
			{
				int rows = bs.ReadByte();
				int cols = bs.ReadByte();
				int levs = bs.ReadByte();

				MapTiles = new MapTileList(rows, cols, levs);
				MapSize  = new MapSize(rows, cols, levs);

				for (int lev = 0; lev != levs; ++lev)
				for (int row = 0; row != rows; ++row)
				for (int col = 0; col != cols; ++col)
				{
					this[row, col, lev] = CreateTile(
												parts,
												bs.ReadByte(),
												bs.ReadByte(),
												bs.ReadByte(),
												bs.ReadByte());
				}
			}
		}


		private bool _bypass;

		private const int IdOffset = 2; // #0 and #1 are reserved for the 2 BLANKS tiles.

		/// <summary>
		/// Creates a tile with its four parts.
		/// </summary>
		/// <param name="parts">a list of total tileparts that can be used</param>
		/// <param name="quad1">the floor</param>
		/// <param name="quad2">the westwall</param>
		/// <param name="quad3">the northwall</param>
		/// <param name="quad4">the content</param>
		/// <returns></returns>
		private XCMapTile CreateTile(
				IList<TilepartBase> parts,
				int quad1,
				int quad2,
				int quad3,
				int quad4)
		{
			if (!_bypass)
			{
				int high = quad1;
				if (quad2 > high) high = quad2;
				if (quad3 > high) high = quad3;
				if (quad4 > high) high = quad4;

				if (high - IdOffset >= parts.Count)
				{
					_bypass = true;

					IsLoadChanged = true;

					MessageBox.Show(
								"There are tileset parts being referenced that are"
									+ " outside the bounds of the Map's allocated MCDs."
									+ " They will be cleared so that the Map can be"
									+ " displayed.",
								"Warning",
								MessageBoxButtons.OK,
								MessageBoxIcon.Asterisk,
								MessageBoxDefaultButton.Button1,
								0);
				}
			}

			if (quad1 < 0 || quad1 - IdOffset >= parts.Count) quad1 = 0; // TODO: if any quads are < 0 MapChanged should be flagged.
			if (quad2 < 0 || quad2 - IdOffset >= parts.Count) quad2 = 0;
			if (quad3 < 0 || quad3 - IdOffset >= parts.Count) quad3 = 0;
			if (quad4 < 0 || quad4 - IdOffset >= parts.Count) quad4 = 0;

			var floor     = (quad1 > 1) ? (Tilepart)parts[quad1 - IdOffset]
										: null;
			var westwall  = (quad2 > 1) ? (Tilepart)parts[quad2 - IdOffset]
										: null;
			var northwall = (quad3 > 1) ? (Tilepart)parts[quad3 - IdOffset]
										: null;
			var content   = (quad4 > 1) ? (Tilepart)parts[quad4 - IdOffset]
										: null;

			return new XCMapTile(
								floor,
								westwall,
								northwall,
								content);
		}

		/// <summary>
		/// Assigns route-nodes to tiles when this MapFile object is
		/// instantiated.
		/// </summary>
		private void SetupRouteNodes()
		{
			MapTileBase tile;
			foreach (RouteNode node in Routes)
			{
				if ((tile = this[node.Row, node.Col, node.Lev]) != null)
					((XCMapTile)tile).Node = node;
			}
		}

		/// <summary>
		/// Generates occultation data for all tiles in the Map.
		/// </summary>
		/// <param name="forceVis">true to force visibility</param>
		public void CalculateOccultations(bool forceVis = false)
		{
			if (MapSize.Levs > 1) // NOTE: Maps shall be at least 10x10x1 ...
			{
				MapTileBase tile = null;

				for (int lev = MapSize.Levs - 1; lev != 0; --lev)
				for (int row = 0; row != MapSize.Rows - 2; ++row)
				for (int col = 0; col != MapSize.Cols - 2; ++col)
				{
					if ((tile = this[row, col, lev]) != null) // safety. The tile should always be valid.
					{
						if (!forceVis
							&& ((XCMapTile)this[row,     col,     lev - 1]).Ground != null // above

							&& ((XCMapTile)this[row + 1, col,     lev - 1]).Ground != null // south
							&& ((XCMapTile)this[row + 2, col,     lev - 1]).Ground != null

							&& ((XCMapTile)this[row,     col + 1, lev - 1]).Ground != null // east
							&& ((XCMapTile)this[row,     col + 2, lev - 1]).Ground != null

							&& ((XCMapTile)this[row + 1, col + 1, lev - 1]).Ground != null // southeast
							&& ((XCMapTile)this[row + 1, col + 2, lev - 1]).Ground != null
							&& ((XCMapTile)this[row + 2, col + 1, lev - 1]).Ground != null
							&& ((XCMapTile)this[row + 2, col + 2, lev - 1]).Ground != null)
						{
							tile.Occulted = true;
						}
						else
							tile.Occulted = false;
					}
				}
			}
		}

		/// <summary>
		/// Gets the terrain-type given a tile-part.
		/// </summary>
		/// <param name="part"></param>
		/// <returns></returns>
		public string GetTerrainLabel(TilepartBase part)
		{
			int id = -1;
			foreach (var part1 in Parts)
			{
				if (part1.TerId == 0)
					++id;

				if (part1 == part)
					break;
			}

			if (id != -1 && id < Terrains.Count)
				return Terrains[id].Item1;

			return null;
		}

		public Tuple<string,string> GetTerrain(TilepartBase part)
		{
			int id = -1;
			foreach (var part_ in Parts)
			{
				if (part_.TerId == 0)
					++id;

				if (part_ == part)
					break;
			}

			if (id != -1 && id < Terrains.Count)
				return Terrains[id];

			return null;
		}

		/// <summary>
		/// Adds a route-node to the map-tile at a given location.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		public RouteNode AddRouteNode(MapLocation location)
		{
			var node = Routes.AddNode(
									(byte)location.Row,
									(byte)location.Col,
									(byte)location.Lev);

			return (((XCMapTile)this[(int)node.Row,
									 (int)node.Col,
									      node.Lev]).Node = node);
		}

		/// <summary>
		/// Writes a blank Map to the stream provided.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		/// <param name="levs"></param>
		public static void CreateMap(
				Stream str,
				byte rows,
				byte cols,
				byte levs)
		{
			using (var bw = new BinaryWriter(str))
			{
				bw.Write(rows);
				bw.Write(cols);
				bw.Write(levs);

				for (int lev = 0; lev != levs; ++lev)
				for (int row = 0; row != rows; ++row)
				for (int col = 0; col != cols; ++col)
				{
					bw.Write((int)0);
				}
			}
		}

		/// <summary>
		/// Saves the .MAP file.
		/// </summary>
		public override void SaveMap()
		{
			SaveMapData(FullPath);
		}

		/// <summary>
		/// Saves the .MAP file as a different file.
		/// </summary>
		/// <param name="pf">the path+file to save as</param>
		public override void SaveMap(string pf)
		{
			string pfe = pf + GlobalsXC.MapExt;
			Directory.CreateDirectory(Path.GetDirectoryName(pfe));
			SaveMapData(pfe);
		}

		/// <summary>
		/// Saves the current mapdata to a .MAP file.
		/// </summary>
		/// <param name="pfe">path+file+extension</param>
		private void SaveMapData(string pfe)
		{
			using (var fs = File.Create(pfe))
			{
				fs.WriteByte((byte)MapSize.Rows); // http://www.ufopaedia.org/index.php/MAPS
				fs.WriteByte((byte)MapSize.Cols); // - says this header is "height, width and depth (in that order)"
				fs.WriteByte((byte)MapSize.Levs);

				int id;

				for (int lev = 0; lev != MapSize.Levs; ++lev)
				for (int row = 0; row != MapSize.Rows; ++row)
				for (int col = 0; col != MapSize.Cols; ++col)
				{
					var tile = this[row, col, lev] as XCMapTile;

					if (tile.Ground == null || (id = tile.Ground.SetId + IdOffset) > (int)byte.MaxValue)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)id);

					if (tile.West == null || (id = tile.West.SetId + IdOffset) > (int)byte.MaxValue)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)id);

					if (tile.North == null || (id = tile.North.SetId + IdOffset) > (int)byte.MaxValue)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)id);

					if (tile.Content == null || (id = tile.Content.SetId + IdOffset) > (int)byte.MaxValue)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)id);
				}
			}
		}

		/// <summary>
		/// Saves the .RMP file.
		/// </summary>
		public override void SaveRoutes()
		{
			Routes.SaveRoutes();
		}

		/// <summary>
		/// Saves the .RMP file as a different file.
		/// </summary>
		/// <param name="pf">the path+file to save as</param>
		public override void SaveRoutes(string pf)
		{
			Routes.SaveRoutes(pf);
		}

		/// <summary>
		/// Resizes the current Map.
		/// </summary>
		/// <param name="rows">total rows in the new Map</param>
		/// <param name="cols">total columns in the new Map</param>
		/// <param name="levs">total levels in the new Map</param>
		/// <param name="zType">MRZT_TOP to add or subtract delta-levels
		/// starting at the top level, MRZT_BOT to add or subtract delta-levels
		/// starting at the ground level - but only if a height difference is
		/// found for either case</param>
		/// <returns>a bitwise int of changes
		///          0x0 - no changes
		///          0x1 - Map changed
		///          0x2 - Routes changed</returns>
		public override int MapResize(
				int rows,
				int cols,
				int levs,
				MapResizeService.MapResizeZtype zType)
		{
			int bit = 0x0;

			var tileList = MapResizeService.GetResizedTileList(
															rows, cols, levs,
															MapSize,
															MapTiles,
															zType);
			if (tileList != null)
			{
				bit |= 0x1;

				int
					preRows = MapSize.Rows,
					preCols = MapSize.Cols,
					preLevs = MapSize.Levs;

				if (zType == MapResizeService.MapResizeZtype.MRZT_TOP // adjust route-nodes ->
					&& Routes.Any())
				{
					bit |= 0x2;

					int delta = (levs - preLevs);	// NOTE: map levels are inverted so adding or subtracting levels
													// to the top needs to push any existing node-levels down or up.
					foreach (RouteNode node in Routes)
					{
						if (node.Lev < 128) // allow nodes that are OoB to come back into view
						{
							if ((node.Lev += delta) < 0)	// NOTE: node x/y/z are stored as bytes.
								node.Lev += 256;			// -> ie. level -1 = level 255
						}
						else
						{
							if ((node.Lev += delta - 256) < 0)	// nodes above the highest Maplevel maintain
								node.Lev += 256;				// their relative z-level
						}
					}
				}

				MapSize = new MapSize(rows, cols, levs);
				MapTiles = tileList;

				if (RouteCheckService.CheckNodeBounds(this))
					bit |= 0x2;

				for (int lev = 0; lev != levs; ++lev)
				for (int row = 0; row != rows; ++row)
				for (int col = 0; col != cols; ++col)
					((XCMapTile)this[row, col, lev]).Node = null;

				SetupRouteNodes();

				Level = 0; // fires a LevelChangedEvent.
			}
			return bit;
		}
		#endregion
	}
}

//		public void HQ2X()
//		{
//			foreach (string dep in _deps) // instead i would want to make an image of the whole map and run that through hq2x
//				foreach (var image in GameInfo.GetPckPack(dep))
//					image.HQ2X();
//
//			PckImage.Width  *= 2;
//			PckImage.Height *= 2;
//		}
