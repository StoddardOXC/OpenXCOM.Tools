using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using MapView.Forms.MainWindow;

using XCom;
using XCom.Interfaces;
using XCom.Interfaces.Base;


namespace MapView.Forms.MapObservers.TileViews
{
	internal delegate void TileSelectedEventHandler(TilepartBase part);


	/// <summary>
	/// A separate panel is created for each tab-page in the Tile viewer.
	/// </summary>
	internal sealed class TilePanel
		:
			Panel
	{
		internal event TileSelectedEventHandler TileSelectedEvent;


		#region Fields & Properties
		private TilepartBase[] _parts;

		private readonly VScrollBar _scrollBar;

		private const int SpriteMargin = 2;
		private const int SpriteWidth  = XCImage.SpriteWidth    + SpriteMargin * 2;
		private const int SpriteHeight = XCImage.SpriteHeight40 + SpriteMargin * 2;

		private const int _largeChange = SpriteHeight;	// apparently .NET won't return an accurate value
														// for LargeChange unless the scrollbar is visible.

		private Pen   _penBlack        = new Pen(Brushes.Black, 1);
		private Pen   _penRed          = new Pen(Brushes.Red, 3);
		private Pen   _penControlLight = new Pen(SystemColors.ControlLight, 1);
		private Brush _brushBlack      = new SolidBrush(Color.Black);

		private static Hashtable _specialTypeBrushes;

		private int _tilesX = 1;
		private int _startY;
		private int _id;

		private PartType _quadType;

		private int TableHeight
		{
			get // TODO: calculate and cache this value in the OnResize and loading events.
			{
				if (_parts != null && _parts.Length != 0)
				{
					_tilesX = (Width - TableOffset - _scrollBar.Width - 1) / SpriteWidth;	// reserve width for the scrollbar.
					if (_tilesX != 0)														// <- happens when minimizing the TileView form.
					{																		// NOTE: that could be intercepted and disallowed w/
						if (_tilesX > _parts.Length)										// 'if (WindowState != FormWindowState.Minimized)'
							_tilesX = _parts.Length;										// in the OnResize().

						int extra = 0;
						if (_parts.Length % _tilesX != 0)
							extra = 1;

						return (_parts.Length / _tilesX + extra) * SpriteHeight + TableOffset;
					}
				}
				_tilesX = 1;
				return 0;
			}
		}

		/// <summary>
		/// Gets the selected-tilepart-id.
		/// Sets the selected-tilepart-id when a valid QuadrantPanel quad is
		/// double-clicked.
		/// </summary>
		internal TilepartBase PartSelected
		{
			get
			{
				if (_id > -1 && _id < _parts.Length)
					return _parts[_id];

				return null;
			}
			set
			{
				if (value != null)
				{
					_id = value.TerrainsetId + 1; // +1 to account for the eraser - not sure.

					if (TileSelectedEvent != null)
						TileSelectedEvent(PartSelected);

					ScrollToTile();
				}
				else
					_id = 0;
			}
		}
		#endregion


		/// <summary>
		/// Sets a pointer to a hashtable of the special property brushes/colors.
		/// The owner of the object is TileView.
		/// </summary>
		/// <param name="brushes"></param>
		internal static void SetSpecialPropertyBrushes(Hashtable brushes)
		{
			_specialTypeBrushes = brushes;
		}

//		private static SpriteCollection _extraFile;
//		public static SpriteCollection ExtraFile
//		{
//			get { return _extraFile; }
//			set { _extraFile = value; }
//		}


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="quadType"></param>
		internal TilePanel(PartType quadType)
		{
			_quadType = quadType;

			Dock = DockStyle.Fill;

			_scrollBar = new VScrollBar();
			_scrollBar.Dock = DockStyle.Right;
			_scrollBar.LargeChange = _largeChange;
			_scrollBar.SmallChange = 1;
			_scrollBar.ValueChanged += OnScrollBarValueChanged;

			Controls.Add(_scrollBar);

			MainViewUnderlay.AnimationUpdateEvent += OnAnimationUpdate; // FIX: "Subscription to static events without unsubscription may cause memory leaks."

			SetStyle(ControlStyles.OptimizedDoubleBuffer
				   | ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint
				   | ControlStyles.ResizeRedraw, true);
		}
		#endregion


		#region Event Calls
		/// <summary>
		/// Fires when anything changes the Value of the scroll-bar.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnScrollBarValueChanged(object sender, EventArgs e)
		{
			if (_parts != null && _parts.Length != 0)
			{
				_startY = -_scrollBar.Value;
				Refresh();
			}
		}


		private bool _resetTrack;

		/// <summary>
		/// Handles client resizing. Sets the scrollbar's Maximum value.
		/// </summary>
		/// <param name="eventargs"></param>
		protected override void OnResize(EventArgs eventargs)
		{
			base.OnResize(eventargs);

			int range = 0;
			if (_parts != null && _parts.Length != 0)
			{
				if (_resetTrack)
				{
					_resetTrack = false;
					_scrollBar.Value = 0;
				}

				range = TableHeight + _largeChange - Height;
				if (range < _largeChange)
					range = 0;
			}
			_scrollBar.Maximum = range;
			_scrollBar.Visible = (range != 0);
		}

		/// <summary>
		/// Scrolls the table by the mousewheel.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			var args = e as HandledMouseEventArgs;
			if (args != null)
				args.Handled = true;

			if (_scrollBar.Visible)
			{
				if (e.Delta > 0)
				{
					if (_scrollBar.Value - _scrollBar.LargeChange < 0)
						_scrollBar.Value = 0;
					else
						_scrollBar.Value -= _scrollBar.LargeChange;
				}
				else if (e.Delta < 0)
				{
					if (_scrollBar.Value + _scrollBar.LargeChange + (_scrollBar.LargeChange - 1) > _scrollBar.Maximum)
						_scrollBar.Value = _scrollBar.Maximum - (_scrollBar.LargeChange - 1);
					else
						_scrollBar.Value += _scrollBar.LargeChange;
				}
			}
		}

		/// <summary>
		/// Focuses this panel and selects a tile.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			Focus();

			int id = GetOverId(e);
			if (id != -1 && id < _parts.Length)
			{
				_id = id;

				if (TileSelectedEvent != null)
					TileSelectedEvent(PartSelected);

				ScrollToTile();
				Refresh();
			}
		}

		/// <summary>
		/// Opens the MCD-info screen when a valid tile is double-clicked.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			int id = GetOverId(e);
			if (id != -1 && id < _parts.Length)
			{
				switch (e.Button)
				{
					case MouseButtons.Left:
						ViewerFormsManager.TileView.Control.OnPckEditorClick(null, EventArgs.Empty);
						break;

					case MouseButtons.Right:
						ViewerFormsManager.TileView.Control.OnMcdInfoClick(null, EventArgs.Empty);
						break;
				}
			}
		}

		private int GetOverId(MouseEventArgs e)
		{
			if (_parts != null && _parts.Length != 0
				&& e.X < SpriteWidth * _tilesX + TableOffset - 1) // not out of bounds to right
			{
				int tileX = (e.X - TableOffset + 1)           / SpriteWidth;
				int tileY = (e.Y - TableOffset + 1 - _startY) / SpriteHeight;

				return tileX + tileY * _tilesX;
			}
			return -1;
		}


		private const string Door = "door";

		private static bool Inited;
		private static int TextWidth;

		private const int TableOffset = 2;

		/// <summary>
		/// this.Fill(black)
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			if (_parts != null && _parts.Length != 0)
			{
				var graphics = e.Graphics;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				int x = 0;
				int y = 0;
				int top;
				int left;

				if (!Inited)
				{
					Inited = true;

//					TextWidth = TextRenderer.MeasureText(Door, Font).Width;		// =30
					TextWidth = (int)graphics.MeasureString(Door, Font).Width;	// =24
				}

				foreach (var tile in _parts)
				{
					left = SpriteWidth  * x + TableOffset;
					top  = SpriteHeight * y + TableOffset + _startY;

					var rect = new Rectangle(
										left, top,
										SpriteWidth, SpriteHeight);

					if (tile != null) // draw tile-sprite ->
					{
						string specialType = tile.Record.TargetType.ToString();	// first fill Special Property color
						if (_specialTypeBrushes.ContainsKey(specialType))
							graphics.FillRectangle((SolidBrush)_specialTypeBrushes[specialType], rect);

						graphics.DrawImage(										// then draw the sprite itself
										tile[MainViewUnderlay.AniStep].Image,
										left + SpriteMargin,
										top  + SpriteMargin - tile.Record.TileOffset);

						// NOTE: keep the door-string and its placement consistent with
						// QuadrantPanelDrawService.Draw().
						if (tile.Record.HumanDoor || tile.Record.UfoDoor)		// finally print "door" if it's a door
							graphics.DrawString(
											Door,
											Font,
											_brushBlack,
											left + (SpriteWidth  - TextWidth) / 2,
											top  +  SpriteHeight - Font.Height);
					}
					else // draw the eraser ->
					{
						graphics.FillRectangle(Brushes.AliceBlue, rect);

						if (Globals.ExtraSprites != null)
							graphics.DrawImage(
											Globals.ExtraSprites[0].Image,
											left, top);
					}

					x = (x + 1) % _tilesX;
					if (x == 0)
						y++;
				}

//				graphics.DrawRectangle(
//									_brush,
//									(_sel % _across) * (_width + _space),
//									_startY + (_sel / _across) * (_height + _space),
//									_width  + _space,
//									_height + _space)

				if (!_scrollBar.Visible) // indicate the reserved width for scrollbar.
					graphics.DrawLine(
									_penControlLight,
									Width - _scrollBar.Width - 1, 0,
									Width - _scrollBar.Width - 1, Height);

				graphics.FillRectangle(
									new SolidBrush(_penBlack.Color),
									TableOffset - 1,
									TableOffset + _startY - 1,
									1, 1); // so bite me.

				int height = TableHeight;

				for (int i = 0; i <= _tilesX; ++i)							// draw vertical lines
					graphics.DrawLine(
									_penBlack,
									TableOffset + SpriteWidth * i, TableOffset + _startY,
									TableOffset + SpriteWidth * i, /*TableOffset +*/ _startY + height);

				for (int i = 0; i <= height; i += SpriteHeight)				// draw horizontal lines
					graphics.DrawLine(
									_penBlack,
									TableOffset,                         TableOffset + _startY + i,
									TableOffset + SpriteWidth * _tilesX, TableOffset + _startY + i);

				graphics.DrawRectangle(										// draw selected rectangle
									_penRed,
									TableOffset + _id % _tilesX * SpriteWidth,
									TableOffset + _id / _tilesX * SpriteHeight + _startY,
									SpriteWidth, SpriteHeight);

			}
		}

		/// <summary>
		/// whee. Handles animations.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAnimationUpdate(object sender, EventArgs e)
		{
			Refresh();
		}
		#endregion


		#region Methods
		internal void SetTiles(IList<TilepartBase> parts)
		{
			if (parts != null)// && _tiles.Length != 0)	// NOTE: This check for Length should be enough
			{											// to cover all other checks for Length==0.
				if (_quadType == PartType.All)			// Except that the eraser needs to be added anyway ....
				{
					_parts = new TilepartBase[parts.Count + 1];
					_parts[0] = null;

					for (int i = 0; i != parts.Count; ++i)
						_parts[i + 1] = parts[i];
				}
				else
				{
					int qtyTiles = 0;

					for (int i = 0; i != parts.Count; ++i)
						if (parts[i].Record.PartType == _quadType)
							++qtyTiles;

					_parts = new TilepartBase[qtyTiles + 1];
					_parts[0] = null;

					for (int i = 0, j = 1; i != parts.Count; ++i)
						if (parts[i].Record.PartType == _quadType)
							_parts[j++] = parts[i];
				}

				if (_id >= _parts.Length)
					_id = 0;
			}
			else
			{
				_parts = null;
				_id = 0;
			}

			_resetTrack = true;
			OnResize(null);
		}

		/// <summary>
		/// Checks if a selected tile is fully visible in the view-panel and
		/// scrolls the table to show it if not.
		/// </summary>
		private void ScrollToTile()
		{
			int tileY = _id / _tilesX;

			int cutoff = SpriteHeight * tileY;
			if (cutoff < -_startY)		// <- check cutoff high
			{
				_scrollBar.Value = cutoff;
			}
			else						// <- check cutoff low
			{
				cutoff = SpriteHeight * (tileY + 1) - Height + TableOffset + 1;
				if (cutoff > -_startY)
				{
					_scrollBar.Value = cutoff;
				}
			}
		}
		#endregion
	}
}
