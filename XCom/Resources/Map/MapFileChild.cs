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
		#region Fields (static)
		public const string MapExt  = ".MAP";
		public const string MapsDir = "MAPS";
		#endregion


		#region Properties
		private string FullPath
		{ get; set; }

		public List<string> Terrains
		{ get; private set; }

		public RouteNodeCollection Routes
		{ get; private set; }
		#endregion


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
			string dirMap = Path.Combine(Descriptor.BasePath, MapsDir);
			FullPath = Path.Combine(
								dirMap,
								Descriptor.Label + MapExt);

			Terrains = Descriptor.Terrains;

			Routes = routes;

			if (File.Exists(FullPath))
			{
				for (int i = 0; i != parts.Count; ++i)
					parts[i].TerrainsetId = i;

				ReadMapFile(parts);
				SetupRouteNodes(routes);
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

		private const int OffsetId = 2;

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

				if (high - OffsetId >= parts.Count)
				{
					_bypass    =
					MapChanged = true;

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

			if (quad1 < 0 || quad1 - OffsetId >= parts.Count) quad1 = 0; // TODO: if any quads are < 0 MapChanged should be flagged.
			if (quad2 < 0 || quad2 - OffsetId >= parts.Count) quad2 = 0;
			if (quad3 < 0 || quad3 - OffsetId >= parts.Count) quad3 = 0;
			if (quad4 < 0 || quad4 - OffsetId >= parts.Count) quad4 = 0;

			var floor     = (quad1 > 1) ? (Tilepart)parts[quad1 - OffsetId]
										: null;
			var westwall  = (quad2 > 1) ? (Tilepart)parts[quad2 - OffsetId]
										: null;
			var northwall = (quad3 > 1) ? (Tilepart)parts[quad3 - OffsetId]
										: null;
			var content   = (quad4 > 1) ? (Tilepart)parts[quad4 - OffsetId]
										: null;

			return new XCMapTile(
								floor,
								westwall,
								northwall,
								content);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="routes"></param>
		private void SetupRouteNodes(RouteNodeCollection routes)
		{
			foreach (RouteNode node in routes)
			{
				var tile = this[node.Row, node.Col, node.Lev];
				if (tile != null)
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

			foreach (var tile1 in Parts)
			{
				if (tile1.TerrainId == 0)
					++id;

				if (tile1 == part)
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
			RoutesChanged = true;

			var node = Routes.AddNode(
									(byte)location.Row,
									(byte)location.Col,
									(byte)location.Lev);

			return (((XCMapTile)this[node.Row,
									 node.Col,
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
			MapChanged = false;
		}

		/// <summary>
		/// Saves the .MAP file as a different file.
		/// </summary>
		/// <param name="pf">the path+file to save as</param>
		public override void SaveMap(string pf)
		{
			string pfe = pf + MapExt;
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

				for (int lev = 0; lev != MapSize.Levs; ++lev)
				for (int row = 0; row != MapSize.Rows; ++row)
				for (int col = 0; col != MapSize.Cols; ++col)
				{
					var tile = this[row, col, lev] as XCMapTile;

					if (tile.Ground == null)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)(tile.Ground.TerrainsetId + 2)); // why "+2" -> reserved for the 2 Blank tiles.

					if (tile.West == null)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)(tile.West.TerrainsetId + 2));

					if (tile.North == null)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)(tile.North.TerrainsetId + 2));

					if (tile.Content == null)
						fs.WriteByte(0);
					else
						fs.WriteByte((byte)(tile.Content.TerrainsetId + 2));
				}
			}
		}

		/// <summary>
		/// Saves the .RMP file.
		/// </summary>
		public override void SaveRoutes()
		{
			Routes.SaveRoutes();
			RoutesChanged = false;
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
		/// Clears the 'MapChanged' flag.
		/// </summary>
		public override void ClearMapChanged()
		{
			MapChanged = false;
		}

		/// <summary>
		/// Clears the 'RoutesChanged' flag.
		/// </summary>
		public override void ClearRoutesChanged()
		{
			RoutesChanged = false;
		}

		/// <summary>
		/// Resizes the current Map.
		/// </summary>
		/// <param name="rows">total rows in the new Map</param>
		/// <param name="cols">total columns in the new Map</param>
		/// <param name="levs">total levels in the new Map</param>
		/// <param name="ceiling">true to add extra levels above the top level,
		/// false to add extra levels below the ground level - but only if a
		/// height difference is found for either case</param>
		public override void MapResize(
				int rows,
				int cols,
				int levs,
				bool ceiling)
		{
			var tileList = MapResizeService.ResizeMapDimensions(
															rows, cols, levs,
															MapSize,
															MapTiles,
															ceiling);
			if (tileList != null)
			{
				MapChanged = true;

				if (levs != MapSize.Levs && ceiling) // adjust route-nodes ->
				{
					int delta = levs - MapSize.Levs;	// NOTE: map levels are reversed
					foreach (RouteNode node in Routes)	// so adding levels to the ceiling needs to push the existing nodes down.
						node.Lev += delta;
				}

				if (   cols < MapSize.Cols // check for and ask if user wants to delete any route-nodes outside the new bounds
					|| rows < MapSize.Rows
					|| levs < MapSize.Levs)
				{
					RouteCheckService.CheckNodeBounds(this);
				}

				MapTiles = tileList;
				MapSize = new MapSize(rows, cols, levs);

				Level = 0; // fires a LevelChangedEvent.
			}
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
