using System;
using System.Collections.Generic;

using XCom.Interfaces.Base;


namespace XCom
{
	public enum QuadrantType
	{
		Ground,
		West,
		North,
		Content
	};


	public sealed class XCMapTile
		:
			MapTileBase
	{
		#region Properties
		private TilepartBase _ground;
		public TilepartBase Ground
		{
			get { return _ground; }
			set { SetQuadrantPart(QuadrantType.Ground, value); }
		}

		private TilepartBase _west;
		public TilepartBase West
		{
			get { return _west; }
			set { SetQuadrantPart(QuadrantType.West, value); }
		}

		private TilepartBase _north;
		public TilepartBase North
		{
			get { return _north; }
			set { SetQuadrantPart(QuadrantType.North, value); }
		}

		private TilepartBase _content;
		public TilepartBase Content
		{
			get { return _content; }
			set { SetQuadrantPart(QuadrantType.Content, value); }
		}

		public TilepartBase this[QuadrantType quad]
		{
			get
			{
				switch (quad)
				{
					case QuadrantType.Ground:  return Ground;
					case QuadrantType.West:    return West;
					case QuadrantType.North:   return North;
					case QuadrantType.Content: return Content;
				}
				return null;
			}
			set { SetQuadrantPart(quad, value); }
		}

		/// <summary>
		/// @note This is used only by MapFileBase.SaveGifFile().
		/// </summary>
		public override TilepartBase[] UsedParts
		{
			get
			{
				var parts = new List<TilepartBase>();

				if (Ground  != null) parts.Add(Ground);
				if (West    != null) parts.Add(West);
				if (North   != null) parts.Add(North);
				if (Content != null) parts.Add(Content);

				return parts.ToArray();
			}
		}

		public RouteNode Node
		{ get; set; }

		/// <summary>
		/// Used only by 'MapInfoOutputBox'.
		/// </summary>
		public bool Vacant
		{ get; set; }

		public static XCMapTile VacantTile
		{
			get
			{
				var tile = new XCMapTile(null, null, null, null);
				tile.Vacant = true;
				return tile;
			}
		}
		#endregion


		#region cTor
		public XCMapTile(
				TilepartBase ground,
				TilepartBase west,
				TilepartBase north,
				TilepartBase content)
		{
			_ground  = ground; // NOTE: Don't even try ... don't even think about it.
			_west    = west;
			_north   = north;
			_content = content;

			Vacancy();
		}
		#endregion


		#region Methods
		private void SetQuadrantPart(QuadrantType quad, TilepartBase part)
		{
			switch (quad)
			{
				case QuadrantType.Ground:  _ground  = part; break;
				case QuadrantType.West:    _west    = part; break;
				case QuadrantType.North:   _north   = part; break;
				case QuadrantType.Content: _content = part; break;
			}
		}

		/// <summary>
		/// Sets the tile as Vacant if it has no tileparts.
		/// </summary>
		public void Vacancy()
		{
			Vacant = Ground  == null
				  && West    == null
				  && North   == null
				  && Content == null;
		}
		#endregion
	}
}
