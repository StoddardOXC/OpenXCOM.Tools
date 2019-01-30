using XCom.Interfaces.Base;


namespace XCom.Services
{
	public static class MapResizeService
	{
		public enum MapResizeZtype
		{
			MRZT_BOT,	// 0 - a simple addition or subtraction of z-levels (increase/decrease)
			MRZT_TOP	// 1 - this needs to create/delete levels at top and push existing levels down/up
		}


		internal static MapTileList ResizeMapDimensions(
				int rows,
				int cols,
				int levs,
				MapSize sizePre,
				MapTileList tileListPre,
				MapResizeZtype zType)
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

				switch (zType)
				{
					case MapResizeZtype.MRZT_BOT:
					{
						for (int lev = 0; lev != levs && lev != sizePre.Levs; ++lev)
						for (int row = 0; row != rows && row != sizePre.Rows; ++row)
						for (int col = 0; col != cols && col != sizePre.Cols; ++col)
						{
							tileListPost[row, col, lev] = tileListPre[row, col, lev];
						}
						break;
					}

					case MapResizeZtype.MRZT_TOP:
					{
						int levelsPre  = sizePre.Levs - 1;
						int levelsPost = levs - 1;

						for (int lev = 0; lev != levs && lev != sizePre.Levs; ++lev)
						for (int row = 0; row != rows && row != sizePre.Rows; ++row)
						for (int col = 0; col != cols && col != sizePre.Cols; ++col)
						{
							tileListPost[row, col, levelsPost - lev] = // copy tiles from bot to top.
							tileListPre [row, col, levelsPre  - lev];
						}
						break;
					}
				}
				return tileListPost;
			}
			return null;
		}
	}
}
