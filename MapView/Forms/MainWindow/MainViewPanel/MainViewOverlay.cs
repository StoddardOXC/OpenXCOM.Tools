//#define LOCKBITS // toggle this to change OnPaint routine in standard build.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using MapView.Forms.MainWindow;

using XCom;
using XCom.Interfaces;
using XCom.Interfaces.Base;


namespace MapView
{
	#region Delegates
	internal delegate void MouseDragEventHandler();
	#endregion


	internal sealed class MainViewOverlay
		:
			Panel // god I hate these double-panels!!!! cf. MainViewUnderlay
	{
		#region Events
		internal event MouseDragEventHandler MouseDragEvent;
		#endregion


		#region Fields (static)
		internal const int HalfWidthConst  = 16;
		internal const int HalfHeightConst =  8;
		#endregion


		#region Fields
		private bool _suppressTargeter;// = true;

		private int _col; // these are used to print the clicked location
		private int _row;
		private int _lev;

		private int _colOver; // these are used to track the mouseover location
		private int _rowOver;
		#endregion


		#region Properties
		internal MapFileBase MapBase
		{ get; set; }

		private Point _origin = new Point(0, 0);
		internal Point Origin
		{
			get { return _origin; }
			set { _origin = value; }
		}

		internal int HalfWidth
		{ private get; set; }

		internal int HalfHeight
		{ private get; set; }


		private bool _firstClick;
		/// <summary>
		/// Flag that tells the viewers, including Main, that it's okay to draw
		/// a lozenge for a selected tile; ie, that an initial tile has actually
		/// been selected.
		/// Can also happen on MainView when GraySelected is false.
		/// </summary>
		internal bool FirstClick
		{
			get { return _firstClick; }
			set
			{
				_firstClick = value;

				if (!_firstClick)
				{
					_dragStart = new Point(-1, -1);
					_dragEnd   = new Point(-1, -1);
				}
			}
		}

		internal CuboidSprite Cuboid
		{ private get; set; }

		/// <summary>
		/// List of SolidBrushes used to draw sprites from XCImage.Bindata (in
		/// Linux). Can be either UfoBattle palette brushes or TftdBattle
		/// palette brushes.
		/// </summary>
		internal List<Brush> SpriteBrushes
		{ private get; set; }
		#endregion


		#region Fields (graphics)
		private GraphicsPath _layerFill = new GraphicsPath();

		private Graphics _graphics;
		private ImageAttributes _spriteAttributes = new ImageAttributes();

		private Brush _brushLayer;

		private int _anistep;
		private int _cols, _rows;
		#endregion


		#region Properties (options)
		private Color _colorLayer = Color.MediumVioletRed;							// initial color for the grid-layer Option
		public Color GridLayerColor													// <- public for Reflection.
		{
			get { return _colorLayer; }
			set
			{
				_colorLayer = value;
				_brushLayer = new SolidBrush(Color.FromArgb(GridLayerOpacity, _colorLayer));
				Refresh();
			}
		}

		private int _opacity = 180;													// initial opacity for the grid-layer Option
		public int GridLayerOpacity													// <- public for Reflection.
		{
			get { return _opacity; }
			set
			{
				_opacity = value.Clamp(0, 255);
				_brushLayer = new SolidBrush(Color.FromArgb(_opacity, ((SolidBrush)_brushLayer).Color));
				Refresh();
			}
		}

		private Pen _penGrid = new Pen(Color.Black, 1);								// initial pen for grid-lines Option
		public Color GridLineColor													// <- public for Reflection.
		{
			get { return _penGrid.Color; }
			set
			{
				_penGrid.Color = value;
				Refresh();
			}
		}
		public int GridLineWidth													// <- public for Reflection.
		{
			get { return (int)_penGrid.Width; }
			set
			{
				_penGrid.Width = value;
				Refresh();
			}
		}

		private Pen _penGrid10 = new Pen(Color.Black, 2);							// initial pen for x10 grid-lines Option
		public Color Grid10LineColor												// <- public for Reflection.
		{
			get { return _penGrid10.Color; }
			set
			{
				_penGrid10.Color = value;
				Refresh();
			}
		}
		public int Grid10LineWidth													// <- public for Reflection.
		{
			get { return (int)_penGrid10.Width; }
			set
			{
				_penGrid10.Width = value;
				Refresh();
			}
		}

		private bool _showGrid = true;												// initial val for show-grid Option
		public bool ShowGrid														// <- public for Reflection.
		{
			get { return _showGrid; }
			set
			{
				_showGrid = value;
				Refresh();
			}
		}

		private Pen _penSelect = new Pen(Color.Tomato, 2);							// initial pen for selection-border Option
		public Color SelectionLineColor												// <- public for Reflection.
		{
			get { return _penSelect.Color; }
			set
			{
				_penSelect.Color = value;
				Refresh();
			}
		}
		public int SelectionLineWidth												// <- public for Reflection.
		{
			get { return (int)_penSelect.Width; }
			set
			{
				_penSelect.Width = value;
				Refresh();
			}
		}

		private bool _graySelection = true;											// initial val for gray-selection Option
		// NOTE: Remove suppression for Release cfg. .. not workie.
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
		"CA1811:AvoidUncalledPrivateCode",
		Justification = "Because the setter is called dynamically w/ Reflection" +
		"or other: not only is it used it needs to be public.")]
		public bool GraySelection													// <- public for Reflection.
		{
			get { return _graySelection; }
			set
			{
				_graySelection = value;
				Refresh();
			}
		}

#if !LOCKBITS
		private bool _spriteShadeEnabled = true;
#endif
		// NOTE: Options don't like floats afaict, hence this workaround w/
		// 'SpriteShade' and 'SpriteShadeLocal' ->
		private int _spriteShade;													// 0 = initial val for sprite shade Option
		public int SpriteShade														// <- public for Reflection.
		{
			get { return _spriteShade; }
			set
			{
				_spriteShade = value;

				if (_spriteShade > 9 && _spriteShade < 101)
				{
#if !LOCKBITS
					_spriteShadeEnabled = true;
#endif
					SpriteShadeLocal = _spriteShade * 0.03f;
				}
#if !LOCKBITS
				else
					_spriteShadeEnabled = false;
#endif
				Refresh();
			}
		}
		private float _spriteShadeLocal = 1.0f;										// initial val for local sprite shade
		private float SpriteShadeLocal
		{
			get { return _spriteShadeLocal; }
			set { _spriteShadeLocal = value; }
		}

		// NOTE: Options don't like enums afaict, hence this workaround w/
		// 'Interpolation' and 'InterpolationLocal' ->
		private int _interpolation;													// 0 = initial val for interpolation Option
		public int Interpolation													// <- public for Reflection.
		{
			get { return _interpolation; }
			set
			{
				_interpolation = value.Clamp(0, 7);
				InterpolationLocal = (InterpolationMode)_interpolation;
				Refresh();
			}
		}
		private InterpolationMode _interpolationLocal = InterpolationMode.Default;	// initial val for local interpolation
		private InterpolationMode InterpolationLocal
		{
			get { return _interpolationLocal; }
			set { _interpolationLocal = value; }
		}

		/// <summary>
		/// If true draws a translucent red box around selected tiles
		/// -> superceded by using GraySelection(false) property.
		/// </summary>
//		private bool _drawSelectionBox;
//		public bool DrawSelectionBox
//		{
//			get { return _drawSelectionBox; }
//			set
//			{
//				_drawSelectionBox = value;
//				Refresh();
//			}
//		}
		#endregion


		#region cTor
		internal MainViewOverlay()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer
				   | ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint
				   | ControlStyles.ResizeRedraw, true);

			_brushLayer = new SolidBrush(Color.FromArgb(GridLayerOpacity, GridLayerColor));
		}
		#endregion


		#region Eventcalls and Methods for the edit-functions
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Control)
			{
				switch (e.KeyCode)
				{
					case Keys.S:
						XCMainWindow.Instance.OnSaveMapClick(null, EventArgs.Empty);
						break;

					case Keys.X:
						Copy();
						ClearSelection();
						break;

					case Keys.C:
						Copy();
						break;

					case Keys.V:
						Paste();
						break;
				}
			}
			else
			{
				switch (e.KeyCode)
				{
					case Keys.Delete:
						ClearSelection();
						break;
				}
			}

//			base.OnKeyDown(e);
		}

		internal void ClearSelection()
		{
			if (MapBase != null && FirstClick)
			{
				MapBase.MapChanged = true;

				XCMapTile tile;

				var start = GetAbsoluteDragStart();
				var end   = GetAbsoluteDragEnd();

				for (int col = start.X; col <= end.X; ++col)
				for (int row = start.Y; row <= end.Y; ++row)
				{
					tile = (XCMapTile)MapBase[row, col];

					tile.Ground  = null;
					tile.West    = null;
					tile.North   = null;
					tile.Content = null;

					tile.Vacant = true;
				}

				((MapFileChild)MapBase).CalculateOccultations();

				RefreshViewers();
			}
		}


		private MapTileBase[,] _copied;

		internal void Copy()
		{
			if (MapBase != null && FirstClick)
			{
				ToolstripFactory.Instance.EnablePasteButton();

				var start = GetAbsoluteDragStart();
				var end   = GetAbsoluteDragEnd();

				_copied = new MapTileBase[end.Y - start.Y + 1,
										  end.X - start.X + 1];

				XCMapTile @base, copy;

				for (int col = start.X; col <= end.X; ++col)
				for (int row = start.Y; row <= end.Y; ++row)
				{
					@base = (XCMapTile)MapBase[row, col];
					copy = new XCMapTile(
									@base.Ground,
									@base.West,
									@base.North,
									@base.Content);

					_copied[row - start.Y,
							col - start.X] = copy;
				}
			}
		}

		internal void Paste()
		{
			if (MapBase != null && _copied != null && FirstClick)
			{
				MapBase.MapChanged = true;

				XCMapTile tile, tileCopy;
				for (int
						row = DragStart.Y;
						row != MapBase.MapSize.Rows && (row - DragStart.Y) < _copied.GetLength(0);
						++row)
				{
					for (int
							col = DragStart.X;
							col != MapBase.MapSize.Cols && (col - DragStart.X) < _copied.GetLength(1);
							++col)
					{
						if ((tile = MapBase[row, col] as XCMapTile) != null)
						{
							if ((tileCopy = _copied[row - DragStart.Y,
													col - DragStart.X] as XCMapTile) != null)
							{
								tile.Ground  = tileCopy.Ground;
								tile.Content = tileCopy.Content;
								tile.West    = tileCopy.West;
								tile.North   = tileCopy.North;

								tile.Vacancy();
							}
						}
					}
				}

				((MapFileChild)MapBase).CalculateOccultations();

				RefreshViewers();
			}
		}

		internal void FillSelectedTiles()
		{
			if (MapBase != null && FirstClick)
			{
				MapBase.MapChanged = true;

				var start = GetAbsoluteDragStart();
				var end   = GetAbsoluteDragEnd();

				var quad = ViewerFormsManager.TopView .Control.QuadrantsPanel.SelectedQuadrant;
				var part = ViewerFormsManager.TileView.Control.SelectedTilepart;

				for (int col = start.X; col <= end.X; ++col)
				for (int row = start.Y; row <= end.Y; ++row)
				{
					((XCMapTile)MapBase[row, col])[quad] = part;
				}

				((MapFileChild)MapBase).CalculateOccultations();

				RefreshViewers();
			}
		}

		private void RefreshViewers()
		{
			Refresh();

			ViewerFormsManager.TopView     .Refresh();
			ViewerFormsManager.RouteView   .Refresh();
			ViewerFormsManager.TopRouteView.Refresh();
		}
		#endregion


		#region Mouse & Drag-points
		/// <summary>
		/// Scrolls the z-axis for MainView.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if      (e.Delta < 0) MapBase.LevelUp();
			else if (e.Delta > 0) MapBase.LevelDown();
		}


		private bool _isMouseDrag;

		/// <summary>
		/// Selects a tile and/or starts a drag-select procedure.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			Select();

			if (MapBase != null)
			{
				var start = GetTileLocation(e.X, e.Y);
				if (   start.X > -1 && start.X < MapBase.MapSize.Cols
					&& start.Y > -1 && start.Y < MapBase.MapSize.Rows)
				{
					MapBase.Location = new MapLocation(
													start.Y,
													start.X,
													MapBase.Level);
					_isMouseDrag = true;
					ProcessTileSelection(start, start);
				}
			}
		}

		/// <summary>
		/// uh.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			_isMouseDrag = false;
		}

		/// <summary>
		/// Updates the drag-selection process.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (MapBase != null)
			{
				var end = GetTileLocation(e.X, e.Y);

				_suppressTargeter = false;
				_colOver = end.X;
				_rowOver = end.Y;

				if (_isMouseDrag
					&& (end.X != DragEnd.X || end.Y != DragEnd.Y))
				{
					ProcessTileSelection(DragStart, end);
				}
				else
					Refresh(); // mouseover refresh for MainView.
			}
		}

//		/// <summary>
//		/// Hides the cuboid-targeter when the mouse leaves this control.
//		/// </summary>
//		/// <param name="e"></param>
//		protected override void OnMouseLeave(EventArgs e)
//		{
//			_suppressTargeter = true;
//			Refresh();
//		}

		/// <summary>
		/// Sets drag-start and drag-end and fires a MouseDragEvent (path
		/// selected lozenge).
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		internal void ProcessTileSelection(Point start, Point end)
		{
			if (DragStart != start || DragEnd != end)
			{
				DragStart = start;	// these ensure that the start and end points stay
				DragEnd   = end;	// within the bounds of the currently loaded map.
	
				if (MouseDragEvent != null) // path the selected-lozenge
					MouseDragEvent();

				RefreshViewers();
			}
		}

		/// <summary>
		/// Gets the drag-start point as a lesser value than the drag-end point.
		/// See 'DragStart' for bounds.
		/// </summary>
		/// <returns></returns>
		internal Point GetAbsoluteDragStart()
		{
			return new Point(
						Math.Min(DragStart.X, DragEnd.X),
						Math.Min(DragStart.Y, DragEnd.Y));
		}

		/// <summary>
		/// Gets the drag-end point as a greater value than the drag-start point.
		/// See 'DragEnd' for bounds.
		/// </summary>
		/// <returns></returns>
		internal Point GetAbsoluteDragEnd()
		{
			return new Point(
						Math.Max(DragStart.X, DragEnd.X),
						Math.Max(DragStart.Y, DragEnd.Y));
		}


		private Point _dragStart = new Point(-1, -1);
		private Point _dragEnd   = new Point(-1, -1);

		/// <summary>
		/// Gets/Sets the drag-start point.
		/// </summary>
		internal Point DragStart
		{
			get { return _dragStart; }
			private set
			{
				_dragStart = value;

				if      (_dragStart.Y < 0) _dragStart.Y = 0;
				else if (_dragStart.Y >= MapBase.MapSize.Rows) _dragStart.Y = MapBase.MapSize.Rows - 1;

				if      (_dragStart.X < 0) _dragStart.X = 0;
				else if (_dragStart.X >= MapBase.MapSize.Cols) _dragStart.X = MapBase.MapSize.Cols - 1;
			}
		}

		/// <summary>
		/// Gets/Sets the drag-end point.
		/// </summary>
		internal Point DragEnd
		{
			get { return _dragEnd; }
			private set
			{
				_dragEnd = value;

				if      (_dragEnd.Y < 0) _dragEnd.Y = 0;
				else if (_dragEnd.Y >= MapBase.MapSize.Rows) _dragEnd.Y = MapBase.MapSize.Rows - 1;

				if      (_dragEnd.X < 0) _dragEnd.X = 0;
				else if (_dragEnd.X >= MapBase.MapSize.Cols) _dragEnd.X = MapBase.MapSize.Cols - 1;
			}
		}
		#endregion


		#region Eventcalls
		/// <summary>
		/// Fires when a location is selected in MainView.
		/// </summary>
		/// <param name="args"></param>
		internal void OnLocationSelectedMain(LocationSelectedEventArgs args)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("MainViewOverlay.OnLocationSelectedMain");

			FirstClick = true;
			_suppressTargeter = false;

			_col = args.Location.Col;
			_row = args.Location.Row;
			_lev = args.Location.Lev;
			//LogFile.WriteLine(". " + _col + "," + _row + "," + _lev);

			XCMainWindow.Instance.StatusBarPrintPosition(
													_col, _row,
													MapBase.MapSize.Levs - _lev);
		}

		/// <summary>
		/// Fires when the map level changes in MainView.
		/// </summary>
		/// <param name="args"></param>
		internal void OnLevelChangedMain(LevelChangedEventArgs args)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("MainViewOverlay.OnLevelChangedMain");

			_suppressTargeter = true;

			_lev = args.Level;
			//LogFile.WriteLine(". " + _col + "," + _row + "," + _lev);

			XCMainWindow.Instance.StatusBarPrintPosition(
													_col, _row,
													MapBase.MapSize.Levs - _lev);
			Refresh();
		}
		#endregion


		#region Draw
#if !LOCKBITS
		int _halfwidth2, _halfheight5; // standard draw only.

		/// <summary>
		/// Dimension (scaled both x and y) of a drawn sprite.
		/// </summary>
		private int _d; // Mono draw only.
#else
		Bitmap _b;
#endif

		/// <summary>
		/// Draws the Map in MainView.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
//			base.OnPaint(e);

			if (MapBase != null)
			{
				_graphics = e.Graphics;
				_graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
#if !LOCKBITS
				if (!XCMainWindow.UseMonoDraw)
				{
					_graphics.InterpolationMode = InterpolationLocal;

					if (_spriteShadeEnabled)
						_spriteAttributes.SetGamma(SpriteShadeLocal, ColorAdjustType.Bitmap);
				}
#endif

				// Image Processing using C# - https://www.codeproject.com/Articles/33838/Image-Processing-using-C
				// ColorMatrix Guide - https://docs.rainmeter.net/tips/colormatrix-guide/

				ControlPaint.DrawBorder3D(_graphics, ClientRectangle, Border3DStyle.Etched);


				_anistep = MainViewUnderlay.AniStep;

				_cols = MapBase.MapSize.Cols;
				_rows = MapBase.MapSize.Rows;

#if !LOCKBITS
				if (XCMainWindow.UseMonoDraw)
				{
					_d = (int)(Globals.Scale - 0.1) + 1; // NOTE: Globals.ScaleMinimum is 0.25; don't let it drop to negative value.
					DrawPicasso();
				}
				else
				{
					_halfwidth2  = HalfWidth  * 2;
					_halfheight5 = HalfHeight * 5;
					DrawRembrandt();
				}

#else
				_b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
				BuildPanelImage();
//				_graphics.DrawImage(_b, 0, 0, _b.Width, _b.Height);
				_graphics.DrawImageUnscaled(_b, Point.Empty);	// uh does not draw the image unscaled. it
#endif															// still uses the DPI in the Graphics object ...
			}
		}

#if !LOCKBITS
		/// <summary>
		/// Draws the panel using the standard algorithm.
		/// @note This is nearly identical to DrawPicasso; they are separated
		/// only because they'd cause multiple calls to DrawTile() conditioned
		/// on the setting of 'UseMonoDraw' inside the lev/row/col loops.
		/// </summary>
		private void DrawRembrandt()
		{
			var dragrect = new Rectangle(-1,-1, 0,0); // This is different between REMBRANDT and PICASSO ->
			if (FirstClick)
			{
				var start = GetAbsoluteDragStart();
				var end   = GetAbsoluteDragEnd();

				dragrect.X = start.X;
				dragrect.Y = start.Y;
				dragrect.Width  = end.X - start.X + 1;
				dragrect.Height = end.Y - start.Y + 1;
			}


			MapTileBase tile;

			bool isTargeted = Focused
						   && !_suppressTargeter
						   && ClientRectangle.Contains(PointToClient(Cursor.Position));

			for (int
				lev = MapBase.MapSize.Levs - 1;
				lev >= MapBase.Level && lev != -1;
				--lev)
			{
				if (_showGrid && lev == MapBase.Level)
					DrawGrid();

				for (int
						row = 0,
							startY = Origin.Y + (HalfHeight * lev * 3),
							startX = Origin.X;
						row != _rows;
						++row,
							startY += HalfHeight,
							startX -= HalfWidth)
				{
					for (int
							col = 0,
								x = startX,
								y = startY;
							col != _cols;
							++col,
								x += HalfWidth,
								y += HalfHeight)
					{
						bool isClicked = FirstClick
									  && (   (col == DragStart.X && row == DragStart.Y)
										  || (col == DragEnd.X   && row == DragEnd.Y));

						if (isClicked)
						{
							Cuboid.DrawCuboid(
											_graphics,
											x, y,
											HalfWidth,
											HalfHeight,
											false,
											lev == MapBase.Level);
						}

						if (!(tile = MapBase[row, col, lev]).Occulted
							|| lev == MapBase.Level)
						{
							// This is different between REMBRANDT and PICASSO ->
							DrawTile(
									(XCMapTile)tile,
									x, y,
									_graySelection
										&& lev == MapBase.Level
										&& dragrect.Contains(col, row));
						}

						if (isClicked)
						{
							Cuboid.DrawCuboid(
											_graphics,
											x, y,
											HalfWidth,
											HalfHeight,
											true,
											lev == MapBase.Level);
						}
						else if (isTargeted
							&& col == _colOver
							&& row == _rowOver
							&& lev == MapBase.Level)
						{
							Cuboid.DrawTargeter(
											_graphics,
											x, y,
											HalfWidth,
											HalfHeight);
						}
					}
				}
			}

			if (    dragrect.Width > 2 || dragrect.Height > 2 // This is different between REMBRANDT and PICASSO ->
				|| (dragrect.Width > 1 && dragrect.Height > 1))
			{
				DrawSelectionBorder(dragrect);
			}
		}

		/// <summary>
		/// Draws the panel using the Mono algorithm.
		/// @note This is nearly identical to DrawRembrandt; they are separated
		/// only because they'd cause multiple calls to DrawTile() conditioned
		/// on the setting of 'UseMonoDraw' inside the lev/row/col loops.
		/// </summary>
		private void DrawPicasso()
		{
			MapTileBase tile;

			bool isTargeted = Focused
						   && !_suppressTargeter
						   && ClientRectangle.Contains(PointToClient(Cursor.Position));

			for (int
				lev = MapBase.MapSize.Levs - 1;
				lev >= MapBase.Level && lev != -1;
				--lev)
			{
				if (_showGrid && lev == MapBase.Level)
					DrawGrid();

				for (int
						row = 0,
							startY = Origin.Y + (HalfHeight * lev * 3),
							startX = Origin.X;
						row != _rows;
						++row,
							startY += HalfHeight,
							startX -= HalfWidth)
				{
					for (int
							col = 0,
								x = startX,
								y = startY;
							col != _cols;
							++col,
								x += HalfWidth,
								y += HalfHeight)
					{
						bool isClicked = FirstClick
									  && (   (col == DragStart.X && row == DragStart.Y)
										  || (col == DragEnd.X   && row == DragEnd.Y));

						if (isClicked)
						{
							Cuboid.DrawCuboid(
											_graphics,
											x, y,
											HalfWidth,
											HalfHeight,
											false,
											lev == MapBase.Level);
						}

						if (!(tile = MapBase[row, col, lev]).Occulted
							|| lev == MapBase.Level)
						{
							// This is different between REMBRANDT and PICASSO ->
							DrawTile((XCMapTile)tile, x, y);
						}

						if (isClicked)
						{
							Cuboid.DrawCuboid(
											_graphics,
											x, y,
											HalfWidth,
											HalfHeight,
											true,
											lev == MapBase.Level);
						}
						else if (isTargeted
							&& col == _colOver
							&& row == _rowOver
							&& lev == MapBase.Level)
						{
							Cuboid.DrawTargeter(
											_graphics,
											x, y,
											HalfWidth,
											HalfHeight);
						}
					}
				}
			}

			if (FirstClick) // This is different between REMBRANDT and PICASSO ->
			{
				var start = GetAbsoluteDragStart();
				var end   = GetAbsoluteDragEnd();

				int width  = end.X - start.X + 1;
				int height = end.Y - start.Y + 1;

				if (    width > 2 || height > 2
					|| (width > 1 && height > 1))
				{
					var dragrect = new Rectangle(
											start.X, start.Y,
											width, height);
					DrawSelectionBorder(dragrect);
				}
			}
		}

#else
		BitmapData _data; IntPtr _scan0;
		private void BuildPanelImage()
		{
			Graphics graphics = Graphics.FromImage(_b);
			graphics.Clear(Color.Transparent);

			_data = _b.LockBits(
							new Rectangle(0, 0, _b.Width, _b.Height),
							ImageLockMode.WriteOnly,
							PixelFormat.Format32bppArgb);
			_scan0 = _data.Scan0;


			MapTileBase tile;

			bool isTargeted = Focused
						   && !_suppressTargeter
						   && ClientRectangle.Contains(PointToClient(Cursor.Position));

			for (int
				lev = MapBase.MapSize.Levs - 1;
				lev >= MapBase.Level && lev != -1;
				--lev)
			{
//				if (_showGrid && lev == MapBase.Level)
//					DrawGrid(graphics);

				for (int
						row = 0,
							startY = Origin.Y + (HalfHeight * lev * 3),
							startX = Origin.X;
						row != _rows;
						++row,
							startY += HalfHeight,
							startX -= HalfWidth)
				{
					for (int
							col = 0,
								x = startX,
								y = startY;
							col != _cols;
							++col,
								x += HalfWidth,
								y += HalfHeight)
					{
//						bool isClicked = FirstClick
//									  && (   (col == DragStart.X && row == DragStart.Y)
//										  || (col == DragEnd.X   && row == DragEnd.Y));

//						if (isClicked)
//						{
//							Cuboid.DrawCuboid(
//											graphics,
//											x, y,
//											HalfWidth,
//											HalfHeight,
//											false,
//											lev == MapBase.Level);
//						}

						tile = MapBase[row, col, lev];
						if (lev == MapBase.Level || !tile.Occulted)
						{
							DrawTile(
									(XCMapTile)tile,
									x, y);
						}

//						if (isClicked)
//						{
//							Cuboid.DrawCuboid(
//											graphics,
//											x, y,
//											HalfWidth,
//											HalfHeight,
//											true,
//											lev == MapBase.Level);
//						}
//						else if (isTargeted
//							&& col == _colOver
//							&& row == _rowOver
//							&& lev == MapBase.Level)
//						{
//							Cuboid.DrawTargeter(
//											graphics,
//											x, y,
//											HalfWidth,
//											HalfHeight);
//						}
					}
				}
			}
			_b.UnlockBits(_data);

			if (FirstClick)
			{
				var start = GetAbsoluteDragStart();
				var end   = GetAbsoluteDragEnd();

				int width  = end.X - start.X + 1;
				int height = end.Y - start.Y + 1;

				if (    width > 2 || height > 2
					|| (width > 1 && height > 1))
				{
					var dragrect = new Rectangle(
											start.X, start.Y,
											width, height);
					DrawSelectionBorder(dragrect, graphics);
				}
			}

			graphics.Dispose();
		}
#endif

#if !LOCKBITS
		/// <summary>
		/// Draws the grid-lines and the grid-sheet.
		/// </summary>
		private void DrawGrid()
		{
			int x = Origin.X + HalfWidth;
			int y = Origin.Y + HalfHeight * (MapBase.Level + 1) * 3;

			int x1 = _rows * HalfWidth;
			int y1 = _rows * HalfHeight;

			var pt0 = new Point(x, y);
			var pt1 = new Point(
							x + _cols * HalfWidth,
							y + _cols * HalfHeight);
			var pt2 = new Point(
							x + (_cols - _rows) * HalfWidth,
							y + (_rows + _cols) * HalfHeight);
			var pt3 = new Point(x - x1, y + y1);

			_layerFill.Reset();
			_layerFill.AddLine(pt0, pt1);
			_layerFill.AddLine(pt1, pt2);
			_layerFill.AddLine(pt2, pt3);
			_layerFill.CloseFigure();

			_graphics.FillPath(_brushLayer, _layerFill); // the grid-sheet

			// draw the grid-lines ->
			Pen pen;
			for (int i = 0; i <= _rows; ++i)
			{
				if (i % 10 == 0) pen = _penGrid10;
				else             pen = _penGrid;

				_graphics.DrawLine(
								pen,
								x - HalfWidth  * i,
								y + HalfHeight * i,
								x + (_cols - i) * HalfWidth,
								y + (_cols + i) * HalfHeight);
			}

			for (int i = 0; i <= _cols; ++i)
			{
				if (i % 10 == 0) pen = _penGrid10;
				else             pen = _penGrid;

				_graphics.DrawLine(
								pen,
								x + HalfWidth  * i,
								y + HalfHeight * i,
								x - x1 + HalfWidth  * i,
								y + y1 + HalfHeight * i);
			}
		}
#else
		/// <summary>
		/// Draws the grid-lines and the grid-sheet.
		/// </summary>
		/// <param name="graphics"></param>
		private void DrawGrid(Graphics graphics)
		{
			int x = Origin.X + HalfWidth;
			int y = Origin.Y + HalfHeight * (MapBase.Level + 1) * 3;

			int x1 = _rows * HalfWidth;
			int y1 = _rows * HalfHeight;

			var pt0 = new Point(x, y);
			var pt1 = new Point(
							x + _cols * HalfWidth,
							y + _cols * HalfHeight);
			var pt2 = new Point(
							x + (_cols - _rows) * HalfWidth,
							y + (_rows + _cols) * HalfHeight);
			var pt3 = new Point(x - x1, y + y1);

			_layerFill.Reset();
			_layerFill.AddLine(pt0, pt1);
			_layerFill.AddLine(pt1, pt2);
			_layerFill.AddLine(pt2, pt3);
			_layerFill.CloseFigure();

			graphics.FillPath(_brushLayer, _layerFill); // the grid-sheet

			// draw the grid-lines ->
			Pen pen;
			for (int i = 0; i <= _rows; ++i)
			{
				if (i % 10 == 0) pen = _penGrid10;
				else             pen = _penGrid;

				graphics.DrawLine(
								_penGrid,
								x - HalfWidth  * i,
								y + HalfHeight * i,
								x + (_cols - i) * HalfWidth,
								y + (_cols + i) * HalfHeight);
			}

			for (int i = 0; i <= _cols; ++i)
			{
				if (i % 10 == 0) pen = _penGrid10;
				else             pen = _penGrid;

				graphics.DrawLine(
								_penGrid,
								x + HalfWidth  * i,
								y + HalfHeight * i,
								x - x1 + HalfWidth  * i,
								y + y1 + HalfHeight * i);
			}
		}
#endif

		/// <summary>
		/// Draws the tileparts in the Tile if 'UseMono' or LOCKBITS.
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void DrawTile(
				XCMapTile tile,
				int x, int y)
		{
			// NOTE: The width and height args are based on a sprite that's 32x40.
			// Going back to a universal sprite-size would do this:
			//   (int)(sprite.Width  * Globals.Scale)
			//   (int)(sprite.Height * Globals.Scale)
			// with its attendent consequences.

			TilepartBase part;

			var topView = ViewerFormsManager.TopView.Control;
			if (topView.GroundVisible
				&& (part = tile.Ground) != null)
			{
				DrawSprite(
						part[_anistep].Bindata,
						x, y - part.Record.TileOffset * HalfHeight / HalfHeightConst);
			}

			if (topView.WestVisible
				&& (part = tile.West) != null)
			{
				DrawSprite(
						part[_anistep].Bindata,
						x, y - part.Record.TileOffset * HalfHeight / HalfHeightConst);
			}

			if (topView.NorthVisible
				&& (part = tile.North) != null)
			{
				DrawSprite(
						part[_anistep].Bindata,
						x, y - part.Record.TileOffset * HalfHeight / HalfHeightConst);
			}

			if (topView.ContentVisible
				&& (part = tile.Content) != null)
			{
				DrawSprite(
						part[_anistep].Bindata,
						x, y - part.Record.TileOffset * HalfHeight / HalfHeightConst);
			}
		}

#if !LOCKBITS
		/// <summary>
		/// Draws the tileparts in the Tile if not 'UseMono'.
		/// </summary>
		/// <param name="tile"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="gray">true to draw the grayscale version of any tile-sprites</param>
		private void DrawTile(
				XCMapTile tile,
				int x, int y,
				bool gray)
		{
			// NOTE: The width and height args are based on a sprite that's 32x40.
			// Going back to a universal sprite-size would do this:
			//   (int)(sprite.Width  * Globals.Scale)
			//   (int)(sprite.Height * Globals.Scale)
			// with its attendent consequences.

			TilepartBase part;
			var rect = new Rectangle(
								x, y,
								_halfwidth2, _halfheight5);

			var topView = ViewerFormsManager.TopView.Control;
			if (topView.GroundVisible
				&& (part = tile.Ground) != null)
			{
				var sprite = (gray) ? part[_anistep].SpriteGr
									: part[_anistep].Sprite;
				rect.Y -= part.Record.TileOffset * HalfHeight / HalfHeightConst;
				DrawSprite(sprite, rect);
			}

			if (topView.WestVisible
				&& (part = tile.West) != null)
			{
				var sprite = (gray) ? part[_anistep].SpriteGr
									: part[_anistep].Sprite;
				rect.Y -= part.Record.TileOffset * HalfHeight / HalfHeightConst;
				DrawSprite(sprite, rect);
			}

			if (topView.NorthVisible
				&& (part = tile.North) != null)
			{
				var sprite = (gray) ? part[_anistep].SpriteGr
									: part[_anistep].Sprite;
				rect.Y -= part.Record.TileOffset * HalfHeight / HalfHeightConst;
				DrawSprite(sprite, rect);
			}

			if (topView.ContentVisible
				&& (part = tile.Content) != null)
			{
				var sprite = (gray) ? part[_anistep].SpriteGr
									: part[_anistep].Sprite;
				rect.Y -= part.Record.TileOffset * HalfHeight / HalfHeightConst;
				DrawSprite(sprite, rect);
			}
		}

		/// <summary>
		/// Draws a tilepart's sprite w/ FillRectangle().
		/// </summary>
		/// <param name="bindata">binary data of XCImage (list of palette-ids)</param>
		/// <param name="x">x-pixel start</param>
		/// <param name="y">y-pixel start</param>
		private void DrawSprite(IList<byte> bindata, int x, int y)
		{
			int palid;

			int i = -1, w,h;
			for (h = 0; h != XCImage.SpriteHeight40; ++h)
			for (w = 0; w != XCImage.SpriteWidth;    ++w)
			{
				palid = bindata[++i];
				if (palid != Palette.TransparentId) // <- this is the fix for Mono.
				{
					_graphics.FillRectangle(
										SpriteBrushes[palid],
										x + (int)(w * Globals.Scale),
										y + (int)(h * Globals.Scale),
										_d, _d);
				}
			}
		}

		/// <summary>
		/// Draws a tilepart's sprite w/ DrawImage().
		/// </summary>
		/// <param name="sprite"></param>
		/// <param name="rect">destination rectangle</param>
		private void DrawSprite(Image sprite, Rectangle rect)
		{
			if (_spriteShadeEnabled)
				_graphics.DrawImage(
								sprite,
								rect,
								0, 0, XCImage.SpriteWidth, XCImage.SpriteHeight40,
								GraphicsUnit.Pixel,
								_spriteAttributes);
			else
				_graphics.DrawImage(sprite, rect);
		}
#else
		/// <summary>
		/// Draws a tilepart's sprite w/ LockBits().
		/// </summary>
		/// <param name="bindata">binary data of XCImage (list of palette-ids)</param>
		/// <param name="x0">x-pixel start</param>
		/// <param name="y0">y-pixel start</param>
		private void DrawSprite(IList<byte> bindata, int x0, int y0)
		{
//			var data = _b.LockBits(
//								new Rectangle(0, 0, _b.Width, _b.Height),
//								ImageLockMode.WriteOnly,
//								PixelFormat.Format32bppArgb);
//			var scan0 = data.Scan0;

			unsafe
			{
//				byte* dstPos;
//				if (dstLocked.Stride > 0)
//					dstPos = (byte*)dstStart.ToPointer();
//				else
//					dstPos = (byte*)dstStart.ToPointer() + dstLocked.Stride * (_b.Height - 1);
//				uint dstStride = (uint)Math.Abs(dstLocked.Stride);

				var start = (byte*)_scan0.ToPointer();

				uint stride = (uint)_data.Stride;

				byte palid;
				int i = -1;

				byte* pos;

//x + (int)(w * Globals.Scale),
//y + (int)(h * Globals.Scale),
//_d = (int)(Globals.Scale - 0.1) + 1; // NOTE: Globals.ScaleMinimum is 0.25; don't let it drop to negative value.

				uint x,y,offset;
				for (y = 0; y != XCImage.SpriteHeight40; ++y)
				for (x = 0; x != XCImage.SpriteWidth;    ++x)
				{
					palid = bindata[++i];

					if (palid != Palette.TransparentId)
					{
						pos = start
							+ (((uint)y0 + y) * stride)
							+ (((uint)x0 + x) * 4);
						for (offset = 0; offset != 4; ++offset) // 4 bytes in dest-pixel.
						{
							switch (offset)
							{
								case 0: pos[offset] = Palette.UfoBattle[palid].B; break;
								case 1: pos[offset] = Palette.UfoBattle[palid].G; break;
								case 2: pos[offset] = Palette.UfoBattle[palid].R; break;
								case 3: pos[offset] = 255;                        break;
							}
						}
					}
				}
			}
//			_b.UnlockBits(data);
		}
#endif

#if !LOCKBITS
		/// <summary>
		/// Draws a colored lozenge around any selected Tiles.
		/// </summary>
		/// <param name="dragrect"></param>
		private void DrawSelectionBorder(Rectangle dragrect)
		{
			var t = GetClientCoordinates(new Point(dragrect.Left,  dragrect.Top));
			var r = GetClientCoordinates(new Point(dragrect.Right, dragrect.Top));
			var b = GetClientCoordinates(new Point(dragrect.Right, dragrect.Bottom));
			var l = GetClientCoordinates(new Point(dragrect.Left,  dragrect.Bottom));

			t.X += HalfWidth;
			r.X += HalfWidth;
			b.X += HalfWidth;
			l.X += HalfWidth;

			_graphics.DrawLine(_penSelect, t, r);
			_graphics.DrawLine(_penSelect, r, b);
			_graphics.DrawLine(_penSelect, b, l);
			_graphics.DrawLine(_penSelect, l, t);
		}
#else
		/// <summary>
		/// Draws a colored lozenge around any selected Tiles.
		/// </summary>
		/// <param name="dragrect"></param>
		/// <param name="graphics"></param>
		private void DrawSelectionBorder(Rectangle dragrect, Graphics graphics)
		{
			var t = GetClientCoordinates(new Point(dragrect.Left,  dragrect.Top));
			var r = GetClientCoordinates(new Point(dragrect.Right, dragrect.Top));
			var b = GetClientCoordinates(new Point(dragrect.Right, dragrect.Bottom));
			var l = GetClientCoordinates(new Point(dragrect.Left,  dragrect.Bottom));

			t.X += HalfWidth;
			r.X += HalfWidth;
			b.X += HalfWidth;
			l.X += HalfWidth;

			graphics.DrawLine(_penSelect, t, r);
			graphics.DrawLine(_penSelect, r, b);
			graphics.DrawLine(_penSelect, b, l);
			graphics.DrawLine(_penSelect, l, t);
		}
#endif
		#endregion


		#region Coordinate conversion
		/// <summary>
		/// Converts a point from tile-location to client-coordinates.
		/// </summary>
		/// <param name="point">the x/y-position of a tile</param>
		/// <returns></returns>
		private Point GetClientCoordinates(Point point)
		{
			int verticalOffset = HalfHeight * (MapBase.Level + 1) * 3;
			return new Point(
							Origin.X + (point.X - point.Y) * HalfWidth,
							Origin.Y + (point.X + point.Y) * HalfHeight + verticalOffset);
		}

		/// <summary>
		/// Converts a position from client-coordinates to tile-location.
		/// </summary>
		/// <param name="x">the x-position of the mouse cursor</param>
		/// <param name="y">the y-position of the mouse cursor</param>
		/// <returns></returns>
		private Point GetTileLocation(int x, int y)
		{
			x -= Origin.X;
			y -= Origin.Y;

			double halfWidth  = HalfWidth;
			double halfHeight = HalfHeight;

			double verticalOffset = (MapBase.Level + 1) * 3;

			double xd = Math.Floor(x - halfWidth);						// x=0 is the axis from the top to the bottom of the map-lozenge.
			double yd = Math.Floor(y - halfHeight * verticalOffset);	// y=0 is measured from the top of the map-lozenge downward.

			double x1 = xd / (halfWidth  * 2)
					  + yd / (halfHeight * 2);
			double y1 = (yd * 2 - xd) / (halfWidth * 2);

			return new Point(
						(int)Math.Floor(x1),
						(int)Math.Floor(y1));
		}
		#endregion
	}
}
