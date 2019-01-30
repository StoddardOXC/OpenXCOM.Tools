namespace XCom.Interfaces.Base
{
	internal sealed class MapTileList
	{
		#region Fields
		private readonly MapTileBase[] _tiles;
		private readonly MapLocations  _locations;
		#endregion


		#region Properties
		/// <summary>
		/// Gets/Sets a MapTile according to a given location.
		/// </summary>
		public MapTileBase this[int row, int col, int lev]
		{
			get
			{
				if (   col > -1 && col < _locations.MaxCols
					&& row > -1 && row < _locations.MaxRows
					&& lev > -1 && lev < _locations.MaxLevs)
				{
					return _tiles[_locations.GetLocationId(row, col, lev)];
				}
				return null;
			}
			set
			{
				_tiles[_locations.GetLocationId(row, col, lev)] = value;
			}
		}
		#endregion


		#region cTor
		/// <summary>
		/// Instantiates a MapTileList object.
		/// </summary>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		/// <param name="levs"></param>
		internal MapTileList(int rows, int cols, int levs)
		{
			_tiles     = new MapTileBase[rows * cols * levs];
			_locations = new MapLocations(rows, cols, levs);
		}
		#endregion
	}
}
