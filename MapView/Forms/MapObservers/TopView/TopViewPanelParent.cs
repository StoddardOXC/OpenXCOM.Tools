using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using XCom;
using XCom.Interfaces.Base;


namespace MapView.Forms.MapObservers.TopViews
{
	/// <summary>
	/// The base class for TopViewPanel.
	/// </summary>
	internal class TopViewPanelParent
		:
			MapObserverControl1
	{
		#region Fields & Properties
		private readonly GraphicsPath _lozSelector = new GraphicsPath(); // mouse-over cursor lozenge
		private readonly GraphicsPath _lozSelected = new GraphicsPath(); // selected tile or tiles being drag-selected

		[Browsable(false), DefaultValue(null)]
		internal protected Dictionary<string, Pen> TopPens
		{ get; set; }

		[Browsable(false), DefaultValue(null)]
		internal protected Dictionary<string, SolidBrush> TopBrushes
		{ get; set; }


		private int _col = -1; // these track the location of the mouse-cursor
		private int _row = -1;

		private const int OffsetX = 2; // these are the offsets between the
		private const int OffsetY = 2; // panel border and the lozenge-tip(s).

		private int _originX;	// since the lozenge is drawn with its Origin at 0,0 of the
								// panel, the entire lozenge needs to be displaced to the right.
//		private int _originY;	// But this isn't really used. It's set to 'OffsetY' and stays that way. -> done.


		private readonly DrawBlobService _blobService = new DrawBlobService();
		internal protected DrawBlobService BlobService
		{
			get { return _blobService; }
		}

		[Browsable(false), DefaultValue(null)]
		public override MapFileBase MapBase
		{
			set
			{
				base.MapBase = value;

				_blobService.HalfWidth = 8;

				ResizeObserver(Parent.Width, Parent.Height);
				Refresh();
			}
		}

//		private int _lozHeightMin = 4;
//		internal protected int TileLozengeHeight
//		{
//			get { return _lozHeightMin; }
//			set
//			{
//				_lozHeightMin = value;
//				ResizeObserver(Width, Height);
//			}
//		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor. Instantiated only as the parent of TopViewPanel.
		/// </summary>
		internal protected TopViewPanelParent()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer
				   | ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint
				   | ControlStyles.ResizeRedraw, true);
		}
		#endregion


		/// <summary>
		/// Called by TopView's resize event. Also fired by TileLozengeHeight
		/// change, or by a straight MapBase change.
		/// </summary>
		/// <param name="width">the width to resize to</param>
		/// <param name="height">the height to resize to</param>
		internal protected void ResizeObserver(int width, int height)
		{
			if (MapBase != null)
			{
				int halfWidth  = _blobService.HalfWidth;
				int halfHeight = _blobService.HalfHeight;
				
				int halfWidthPre = halfWidth;

				width  -= OffsetX * 2; // don't clip the right or bottom tip of the big-lozenge.
				height -= OffsetY * 2;

				if (MapBase.MapSize.Rows > 0 || MapBase.MapSize.Cols > 0) // safety vs. div-by-0
				{
					if (height > width / 2) // use width
					{
						halfWidth = width / (MapBase.MapSize.Rows + MapBase.MapSize.Cols);

						if (halfWidth % 2 != 0)
							--halfWidth;

						halfHeight = halfWidth / 2;
					}
					else // use height
					{
						halfHeight = height / (MapBase.MapSize.Rows + MapBase.MapSize.Cols);
						halfWidth  = halfHeight * 2;
					}
				}

//				if (halfHeight < _lozHeightMin)
//				{
//					halfWidth  = _lozHeightMin * 2;
//					halfHeight = _lozHeightMin;
//				}
				if (halfHeight < 1)
				{
					halfHeight = 1;
					halfWidth  = 2;
				}

				_blobService.HalfWidth  = halfWidth;
				_blobService.HalfHeight = halfHeight;

				_originX = OffsetX + MapBase.MapSize.Rows * halfWidth;
//				_originY = OffsetY;

				if (halfWidthPre != halfWidth)
				{
					Width  = (MapBase.MapSize.Rows + MapBase.MapSize.Cols) * halfWidth;
					Height = (MapBase.MapSize.Rows + MapBase.MapSize.Cols) * halfHeight;

					Refresh();
				}
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			PathSelectedLozenge();
		}

		/// <summary>
		/// Sets the graphics-path for a lozenge-border around all tiles that
		/// are selected or being selected.
		/// </summary>
		internal protected void PathSelectedLozenge()
		{
			var start = MainViewUnderlay.Instance.MainViewOverlay.GetAbsoluteDragStart();
			var end   = MainViewUnderlay.Instance.MainViewOverlay.GetAbsoluteDragEnd();

			int halfWidth  = _blobService.HalfWidth;
			int halfHeight = _blobService.HalfHeight;

			var p0 = new Point(
							_originX + (start.X - start.Y) * halfWidth,
							OffsetY  + (start.X + start.Y) * halfHeight);
			var p1 = new Point(
							_originX + (end.X   - start.Y) * halfWidth  + halfWidth,
							OffsetY  + (end.X   + start.Y) * halfHeight + halfHeight);
			var p2 = new Point(
							_originX + (end.X   - end.Y)   * halfWidth,
							OffsetY  + (end.X   + end.Y)   * halfHeight + halfHeight * 2);
			var p3 = new Point(
							_originX + (start.X - end.Y)   * halfWidth  - halfWidth,
							OffsetY  + (start.X + end.Y)   * halfHeight + halfHeight);

			_lozSelected.Reset();
			_lozSelected.AddLine(p0, p1);
			_lozSelected.AddLine(p1, p2);
			_lozSelected.AddLine(p2, p3);
			_lozSelected.CloseFigure();

			Refresh();
		}

		/// <summary>
		/// Sets the graphics-path for a lozenge-border around the tile that
		/// is currently mouse-overed.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private void PathSelectorLozenge(int x, int y)
		{
			int halfWidth  = _blobService.HalfWidth;
			int halfHeight = _blobService.HalfHeight;

			var p0 = new Point(x,             y);
			var p1 = new Point(x + halfWidth, y + halfHeight);
			var p2 = new Point(x,             y + halfHeight * 2);
			var p3 = new Point(x - halfWidth, y + halfHeight);

			_lozSelector.Reset();
			_lozSelector.AddLine(p0, p1);
			_lozSelector.AddLine(p1, p2);
			_lozSelector.AddLine(p2, p3);
			_lozSelector.CloseFigure();
		}

		internal void ClearSelectorLozenge()
		{
			_col =
			_row = -1;
		}

		/// <summary>
		/// Overrides DoubleBufferControl.RenderGraphics() - ie, OnPaint().
		/// </summary>
		/// <param name="graphics"></param>
		protected override void RenderGraphics(Graphics graphics)
		{
			graphics.FillRectangle(SystemBrushes.Control, ClientRectangle);

			if (MapBase != null)
			{
				int halfWidth  = _blobService.HalfWidth;
				int halfHeight = _blobService.HalfHeight;

				// draw tile-blobs ->
				for (int
						r = 0,
							startX = _originX,
							startY = OffsetY;
						r != MapBase.MapSize.Rows;
						++r,
							startX -= halfWidth,
							startY += halfHeight)
				{
					for (int
							c = 0,
								x = startX,
								y = startY;
							c != MapBase.MapSize.Cols;
							++c,
								x += halfWidth,
								y += halfHeight)
					{
						var mapTile = MapBase[r, c] as MapTileBase;
						if (mapTile != null)
							((TopViewPanel)this).DrawTileBlobs(mapTile, graphics, x, y);
					}
				}

				// draw grid-lines ->
				Pen pen;
				for (int i = 0; i <= MapBase.MapSize.Rows; ++i) // draw horizontal grid-lines (ie. upperleft to lowerright)
				{
					if (i % 10 == 0) pen = TopPens[TopView.Grid10Color];
					else             pen = TopPens[TopView.GridColor];

					graphics.DrawLine(
									pen,
									_originX - i * halfWidth,
									OffsetY  + i * halfHeight,
									_originX + (MapBase.MapSize.Cols - i) * halfWidth,
									OffsetY  + (MapBase.MapSize.Cols + i) * halfHeight);
				}

				for (int i = 0; i <= MapBase.MapSize.Cols; ++i) // draw vertical grid-lines (ie. lowerleft to upperright)
				{
					if (i % 10 == 0) pen = TopPens[TopView.Grid10Color];
					else             pen = TopPens[TopView.GridColor];

					graphics.DrawLine(
									pen,
									_originX + i * halfWidth,
									OffsetY  + i * halfHeight,
									_originX + i * halfWidth  - MapBase.MapSize.Rows * halfWidth,
									OffsetY  + i * halfHeight + MapBase.MapSize.Rows * halfHeight);
				}


				// draw the selector lozenge ->
				if (Focused
					&& _col > -1 && _col < MapBase.MapSize.Cols
					&& _row > -1 && _row < MapBase.MapSize.Rows)
				{
					PathSelectorLozenge(
									_originX + (_col - _row) * halfWidth,
									OffsetY  + (_col + _row) * halfHeight);
					graphics.DrawPath(TopPens[TopView.SelectorColor], _lozSelector);
				}

				// draw tiles-selected lozenge ->
				if (MainViewUnderlay.Instance.MainViewOverlay.FirstClick)
					graphics.DrawPath(TopPens[TopView.SelectedColor], _lozSelected);
			}
		}


		private bool _isMouseDrag;

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (MapBase != null)
			{
				var start = GetTileLocation(e.X, e.Y);
				if (   start.Y > -1 && start.Y < MapBase.MapSize.Rows
					&& start.X > -1 && start.X < MapBase.MapSize.Cols)
				{
					// as long as MainViewOverlay.OnLocationSelectedMain()
					// fires before the subsidiary viewers' OnLocationSelectedObserver()
					// functions fire, FirstClick is set okay by the former.
					//
					// See also, RouteView.OnLocationSelectedObserver()
					// ps. The FirstClick flag for TopView should be set either in 
					// this class's OnLocationSelectedObserver() handler or even
					// QuadrantPanel.OnLocationSelectedObserver() ... anyway.
					//
					// or better: Make a flag of it in MapFileBase where Location is actually
					// set and all these OnLocationSelected events really fire out of !
//					MainViewUnderlay.Instance.MainViewOverlay.FirstClick = true;

					MapBase.Location = new MapLocation(
													start.Y, start.X,
													MapBase.Level);
					_isMouseDrag = true;
					MainViewUnderlay.Instance.MainViewOverlay.ProcessTileSelection(start, start);
				}
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_isMouseDrag = false;
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			var end = GetTileLocation(e.X, e.Y);
			if (end.X != _col || end.Y != _row)
			{
				_col = end.X;
				_row = end.Y;

				if (_isMouseDrag)
				{
					var overlay = MainViewUnderlay.Instance.MainViewOverlay;
					overlay.ProcessTileSelection(overlay.DragStart, end);
				}
				else
					Refresh(); // mouseover refresh for TopView.
			}
		}

		/// <summary>
		/// Converts a position from screen-coordinates to tile-location.
		/// </summary>
		/// <param name="x">the x-position of the mouse-cursor</param>
		/// <param name="y">the y-position of the mouse-cursor</param>
		/// <returns></returns>
		private Point GetTileLocation(int x, int y)
		{
			x -= _originX;
			y -= OffsetY;

			double halfWidth  = _blobService.HalfWidth;
			double halfHeight = _blobService.HalfHeight;

			double x1 = x / (halfWidth  * 2)
					  + y / (halfHeight * 2);
			double x2 = (y * 2 - x) / (halfWidth * 2);

			return new Point(
						(int)Math.Floor(x1),
						(int)Math.Floor(x2));
		}


/*		/// <summary>
		/// Inherited from IMapObserver through MapObserverControl0.
		/// </summary>
		/// <param name="args"></param>
		public override void OnLocationSelectedObserver(LocationSelectedEventArgs args)
		{
			LogFile.WriteLine("");
			LogFile.WriteLine("TopViewPanelParent.OnLocationSelectedObserver");

			var pt = e.MapLocation;
//			Text = "c " + pt.Col + "  r " + pt.Row; // I don't think this actually prints anywhere.

			var halfWidth  = _drawService.HalfWidth;
			var halfHeight = _drawService.HalfHeight;

			int xc = (pt.Col - pt.Row) * halfWidth;
			int yc = (pt.Col + pt.Row) * halfHeight;

			_lozSel.Reset();
			_lozSel.AddLine(
					xc, yc,
					xc + halfWidth, yc + halfHeight);
			_lozSel.AddLine(
					xc + halfWidth, yc + halfHeight,
					xc, yc + 2 * halfHeight);
			_lozSel.AddLine(
					xc, yc + 2 * halfHeight,
					xc - halfWidth, yc + halfHeight);
			_lozSel.CloseFigure();

			OnMouseDrag();

			Refresh(); // I don't think this is needed.
		} */

		// NOTE: there is no OnLevelChangedObserver for TopView.


//		/// <summary>
//		/// Scrolls the z-axis for TopRouteView. Sort of .... no, well no it doesn't.
//		/// </summary>
//		/// <param name="e"></param>
//		protected override void OnMouseWheel(MouseEventArgs e)
//		{
//			base.OnMouseWheel(e);
//			if		(e.Delta < 0) base.Map.Up();
//			else if	(e.Delta > 0) base.Map.Down();
//		}
	}
}
