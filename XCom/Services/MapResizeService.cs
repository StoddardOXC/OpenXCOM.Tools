using XCom.Interfaces.Base;


namespace XCom.Services
{
	internal static class MapResizeService
	{
		internal static MapTileList ResizeMapDimensions(
				int rows,
				int cols,
				int levs,
				MapSize sizePre,
				MapTileList tileListPre,
				bool ceiling)
		{
			if (   rows > 0
				&& cols > 0
				&& levs > 0)
			{
				var tileListPost = new MapTileList(rows, cols, levs);

				for (int lev = 0; lev != levs; ++lev)
				for (int row = 0; row != rows; ++row)
				for (int col = 0; col != cols; ++col)
					tileListPost[row, col, lev] = XCMapTile.VacantTile;

				int levelPre;
				int levelPost;

				for (int lev = 0; lev != levs && lev < sizePre.Levs; ++lev)
				for (int row = 0; row != rows && row < sizePre.Rows; ++row)
				for (int col = 0; col != cols && col < sizePre.Cols; ++col)
				{
					if (ceiling)
					{
						levelPost = levs         - lev - 1;
						levelPre  = sizePre.Levs - lev - 1;
					}
					else
					{
						levelPost =
						levelPre  = lev;
					}
					tileListPost[row, col, levelPost] = tileListPre[row, col, levelPre];
				}

				return tileListPost;
			}
			return null;
		}
	}
}
