using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using MapView.Forms.MapObservers.TopViews;

using XCom;


namespace MapView.Forms.MapObservers.RouteViews
{
	/// <summary>
	/// The base class for RoutePanel. Generally handles mousey things and
	/// calculating lozenges.
	/// @note This is not a Panel. It is a UserControl inside of a Panel.
	/// </summary>
	internal class RoutePanelParent
		:
			UserControl
	{
		#region Events
		public event EventHandler<RoutePanelEventArgs> RoutePanelMouseDownEvent;
		public event EventHandler<RoutePanelEventArgs> RoutePanelMouseUpEvent;
		#endregion


		#region Fields (static)
		internal protected const int OffsetX = 2; // these track the offset between the panel border
		internal protected const int OffsetY = 2; // and the lozenge-tip.
		#endregion


		#region Fields
		internal protected int _overCol = -1; // these track the location of the last mouse-overed tile
		internal protected int _overRow = -1; // NOTE: could be subsumed into 'CursorPosition' except ...
		#endregion


		#region Properties (static)
		/// <summary>
		/// Stores the x/y-position of the currently selected tile.
		/// </summary>
		internal protected static Point SelectedPosition
		{ get; set; }
		#endregion


		#region Properties
		private Point _pos = new Point(-1, -1);
		/// <summary>
		/// Tracks the tile-position of the mouse cursor. Used to print
		/// over-info, overlay-info, and to position the Overlay.
		/// </summary>
		public Point CursorPosition
		{
			get { return _pos; }
			set { _pos = value; }
		}

		private MapFileChild _mapFile;
		internal protected MapFileChild MapFile
		{
			get { return _mapFile; }
			set
			{
				_mapFile = value;
				OnResize(null);
			}
		}

		/// <summary>
		/// The top-left point of the panel.
		/// </summary>
		internal protected Point Origin
		{ get; set; }

		private int _drawAreaWidth = 8;
		/// <summary>
		/// Half the horizontal width of a tile-lozenge.
		/// </summary>
		internal protected int DrawAreaWidth
		{
			get { return _drawAreaWidth; }
			set { _drawAreaWidth = value; }
		}
		private int _drawAreaHeight = 4;
		/// <summary>
		/// Half the vertical height of a tile-lozenge.
		/// </summary>
		internal protected int DrawAreaHeight
		{
			get { return _drawAreaHeight; }
			set { _drawAreaHeight = value; }
		}


		private readonly GraphicsPath _lozSelector = new GraphicsPath(); // mouse-over lozenge
		internal protected GraphicsPath LozSelector
		{
			get { return _lozSelector; }
		}

		private readonly GraphicsPath _lozSelected = new GraphicsPath(); // click/drag lozenge
		internal protected GraphicsPath LozSelected
		{
			get { return _lozSelected; }
		}

		private readonly GraphicsPath _lozSpotted = new GraphicsPath(); // go-button lozenge
		internal protected GraphicsPath LozSpotted
		{
			get { return _lozSpotted; }
		}

		private readonly DrawBlobService _blobService = new DrawBlobService();
		internal protected DrawBlobService BlobService
		{
			get { return _blobService; }
		}

		private readonly Dictionary<string, Pen> _pens = new Dictionary<string, Pen>();
		internal protected Dictionary<string, Pen> RoutePens
		{
			get { return _pens; }
		}
		private readonly Dictionary<string, SolidBrush> _brushes = new Dictionary<string, SolidBrush>();
		internal protected Dictionary<string, SolidBrush> RouteBrushes
		{
			get { return _brushes; }
		}

		private int _opacity = 255; // cf. RouteView.LoadControl0Options()
		internal protected int Opacity
		{
			get { return _opacity; }
			set { _opacity = value.Clamp(0, 255); }
		}

		private bool _showOverlay = true; // cf. RouteView.LoadControl0Options()
		internal protected bool ShowOverlay
		{
			get { return _showOverlay; }
			set { _showOverlay = value; }
		}

		private bool _showPriorityBars = true; // cf. RouteView.LoadControl0Options()
		internal protected bool ShowPriorityBars
		{
			get { return _showPriorityBars; }
			set { _showPriorityBars = value; }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor. Instantiated as the parent of RoutePanel which uses a default
		/// cTor.
		/// </summary>
		internal protected RoutePanelParent()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer
				   | ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint
				   | ControlStyles.ResizeRedraw, true);

			MainViewUnderlay.Instance.MainViewOverlay.MouseDragEvent += PathSelectedLozenge;


			var timer1 = new Timer();	// because the mouse OnLeave event doesn't fire
			timer1.Interval = 250;		// when the mouse moves over a different form before
			timer1.Enabled = true;		// actually "leaving" this control.
			timer1.Tick += timer1_Tick;	// btw, this is only to stop the overlay from drawing
		}								// on both RouteView and TopRouteView(Route) simultaneously.
		#endregion						// so uh yeah it's overkill
										// Good Lord it works.

		#region Eventcalls
		private void timer1_Tick(object sender, EventArgs e)
		{
			if (!Bounds.Contains(PointToClient(Cursor.Position)))
				CursorPosition = new Point(
										_overCol = -1,
										_overRow = -1);
		}
		#endregion


		#region Eventcalls (override)
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			timer1_Tick(this, e);
		}


		protected override void OnResize(EventArgs e)
		{
//			base.OnResize(e);

			if (MapFile != null)
			{
				int width  = Width  - OffsetX * 2;
				int height = Height - OffsetY * 2;

				if (height > width / 2) // use width
				{
					DrawAreaWidth = width / (MapFile.MapSize.Rows + MapFile.MapSize.Cols);

					if (DrawAreaWidth % 2 != 0)
						--DrawAreaWidth;

					DrawAreaHeight = DrawAreaWidth / 2;
				}
				else // use height
				{
					DrawAreaHeight = height / (MapFile.MapSize.Rows + MapFile.MapSize.Cols);
					DrawAreaWidth  = DrawAreaHeight * 2;
				}

				Origin = new Point( // offset the left and top edges to account for the 3d panel border
								OffsetX + MapFile.MapSize.Rows * DrawAreaWidth,
								OffsetY);

				BlobService.HalfWidth  = DrawAreaWidth;
				BlobService.HalfHeight = DrawAreaHeight;

				PathSelectedLozenge();

				Refresh();
			}
		}

		/// <summary>
		/// Selects a tile on the mouse-down event.
		/// IMPORTANT: Any changes that are done here regarding node-selection
		/// should be reflected in RouteView.SelectNode() since that is an
		/// alternate way to select a tile/node.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
//			base.OnMouseDown(e);

			Select();

			if (MapFile != null) // safety.
			{
				var loc = GetTileLocation(e.X, e.Y);
				if (loc.X != -1)
				{
					MapFile.Location = new MapLocation( // fire LocationSelectedEvent
													loc.Y,
													loc.X,
													MapFile.Level);

					MainViewUnderlay.Instance.MainViewOverlay.ProcessTileSelection(loc, loc);	// set selected location for other viewers.
																								// NOTE: drag-selection is not allowed here.
					if (RoutePanelMouseDownEvent != null)
					{
						var args = new RoutePanelEventArgs();
						args.MouseButton = e.Button;
						args.Tile        = MapFile[loc.Y, loc.X];
						args.Location    = MapFile.Location;

						RoutePanelMouseDownEvent(this, args); // fire RouteView.OnRoutePanelMouseDown()
					}

					SelectedPosition = loc;	// NOTE: if a new 'SelectedPosition' is set before firing the
				}							// RoutePanelMouseDownEvent, OnPaint() will draw a frame with
			}								// incorrectly selected-link lines. So set the 'SelectedPosition'
		}									// *after* the event happens.

		/// <summary>
		/// Tracks x/y location for the mouseover lozenge.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			var end = GetTileLocation(e.X, e.Y);
			if (end.X != _overCol || end.Y != _overRow)
			{
				_overCol = end.X;
				_overRow = end.Y;
			}				
			base.OnMouseMove(e); // required to fire RouteView.OnRoutePanelMouseMove()
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
//			base.OnMouseUp(e);

			if (MapFile != null // safety.
				&& RoutePanelMouseUpEvent != null)
			{
				var loc = GetTileLocation(e.X, e.Y);
				if (loc.X != -1)
				{
					var args = new RoutePanelEventArgs();
					args.MouseButton = e.Button;
					args.Tile        = MapFile[loc.Y, loc.X];
					args.Location    = new MapLocation(
													loc.Y,
													loc.X,
													MapFile.Level);

					RoutePanelMouseUpEvent(this, args); // fire RouteView.OnRoutePanelMouseUp()
				}
			}
		}
		#endregion


		#region Methods
		/// <summary>
		/// Sets the graphics-path for a lozenge-border around the tile that
		/// is currently mouse-overed.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		internal protected void PathSelectorLozenge(int x, int y)
		{
			int halfWidth  = BlobService.HalfWidth;
			int halfHeight = BlobService.HalfHeight;

			var p0 = new Point(x,             y);
			var p1 = new Point(x + halfWidth, y + halfHeight);
			var p2 = new Point(x,             y + halfHeight * 2);
			var p3 = new Point(x - halfWidth, y + halfHeight);

			LozSelector.Reset();
			LozSelector.AddLine(p0, p1);
			LozSelector.AddLine(p1, p2);
			LozSelector.AddLine(p2, p3);
			LozSelector.CloseFigure();
		}

		/// <summary>
		/// Sets the graphics-path for a lozenge-border around all tiles that
		/// are selected or being selected.
		/// </summary>
		private void PathSelectedLozenge()
		{
			var start = MainViewUnderlay.Instance.MainViewOverlay.GetAbsoluteDragStart();
			var end   = MainViewUnderlay.Instance.MainViewOverlay.GetAbsoluteDragEnd();

			int halfWidth  = BlobService.HalfWidth;
			int halfHeight = BlobService.HalfHeight;

			var p0 = new Point(
							Origin.X + (start.X - start.Y) * halfWidth,
							Origin.Y + (start.X + start.Y) * halfHeight);
			var p1 = new Point(
							Origin.X + (end.X   - start.Y) * halfWidth  + halfWidth,
							Origin.Y + (end.X   + start.Y) * halfHeight + halfHeight);
			var p2 = new Point(
							Origin.X + (end.X   - end.Y)   * halfWidth,
							Origin.Y + (end.X   + end.Y)   * halfHeight + halfHeight * 2);
			var p3 = new Point(
							Origin.X + (start.X - end.Y)   * halfWidth  - halfWidth,
							Origin.Y + (start.X + end.Y)   * halfHeight + halfHeight);

			LozSelected.Reset();
			LozSelected.AddLine(p0, p1);
			LozSelected.AddLine(p1, p2);
			LozSelected.AddLine(p2, p3);
			LozSelected.CloseFigure();

			Refresh();
		}

		internal protected void PathSpottedLozenge(int x, int y)
		{
			int halfWidth  = BlobService.HalfWidth;
			int halfHeight = BlobService.HalfHeight;

			var p0 = new Point(x,             y);
			var p1 = new Point(x + halfWidth, y + halfHeight);
			var p2 = new Point(x,             y + halfHeight * 2);
			var p3 = new Point(x - halfWidth, y + halfHeight);

			LozSpotted.Reset();
			LozSpotted.AddLine(p0, p1);
			LozSpotted.AddLine(p1, p2);
			LozSpotted.AddLine(p2, p3);
			LozSpotted.CloseFigure();
		}

		/// <summary>
		/// Gets the tile contained at (x,y) wrt client-area in local screen
		/// coordinates.
		/// </summary>
		/// <param name="x">ref to the x-position of the mouse-cursor wrt
		/// Client-area (refout is the tile-x location)</param>
		/// <param name="y">ref to the y-position of the mouse-cursor wrt
		/// Client-area (refout is the tile-y location)</param>
		/// <returns>the corresponding XCMapTile or null if (x,y) is an invalid
		/// location for a tile</returns>
		internal protected XCMapTile GetTile(ref int x, ref int y)
		{
			var loc = GetTileLocation(x, y);
			x = loc.X;
			y = loc.Y;
			return (x != -1) ? MapFile[y, x] as XCMapTile
							 : null;
		}

		/// <summary>
		/// Converts a position from client-coordinates to a tile-location.
		/// </summary>
		/// <param name="x">the x-position of the mouse-cursor wrt Client-area</param>
		/// <param name="y">the y-position of the mouse-cursor wrt Client-area</param>
		/// <returns>the corresponding tile-location or (-1,-1) if the location
		/// is invalid</returns>
		internal protected Point GetTileLocation(int x, int y)
		{
			if (MapFile != null)
			{
				x -= Origin.X;
				y -= Origin.Y;

				double xd = (double)x / (DrawAreaWidth  * 2)
						  + (double)y / (DrawAreaHeight * 2);
				double yd = ((double)y * 2 - x) / (DrawAreaWidth * 2);

				var point = new Point(
									(int)Math.Floor(xd),
									(int)Math.Floor(yd));

				if (   point.Y > -1 && point.Y < MapFile.MapSize.Rows
					&& point.X > -1 && point.X < MapFile.MapSize.Cols)
				{
					return point;
				}
			}
			return new Point(-1, -1);
		}
		#endregion
	}
}
