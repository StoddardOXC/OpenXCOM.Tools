using System;
using System.Collections;

using XCom.GameFiles.Map;


namespace XCom
{
	/// <summary>
	/// Describes information about imagesets: the path to the PCK, TAB, and MCD files.
	/// </summary>
	public sealed class ImageDescriptor
		:
			IComparable // TODO: should override Equals, ==, !=, <, >
	{
		private readonly Hashtable _mcdTable;

		public string BaseName
		{ get; private set; }

		public string BasePath
		{ get; set; }


		public ImageDescriptor(string baseName, string basePath)
		{
			BaseName = baseName;
			BasePath = basePath;

			_mcdTable = new Hashtable(3);
		}


		internal PckSpriteCollection GetPckPack(Palette pal)
		{
			return GameInfo.CachePckPack(BasePath, BaseName, 2, pal);
		}

		public PckSpriteCollection GetPckPack()
		{
			return GetPckPack(GameInfo.DefaultPalette);
		}

		internal McdRecordCollection GetMcdRecords(Palette pal, XCTileFactory tileFactory)
		{
			if (_mcdTable[pal] == null)
			{
				var tiles = tileFactory.CreateTiles(BaseName, BasePath, GetPckPack(pal));
				_mcdTable[pal] = new McdRecordCollection(tiles);
			}
			return (McdRecordCollection)_mcdTable[pal];
		}

		public McdRecordCollection GetMcdRecords()
		{
			return GetMcdRecords(GameInfo.DefaultPalette, new XCTileFactory());
		}

		public override string ToString()
		{
			return BaseName;
		}

		public int CompareTo(object obj)
		{
			return String.CompareOrdinal(BaseName, obj.ToString());
		}

		public void ClearMcdTable()
		{
			_mcdTable.Clear();
		}
	}
}
