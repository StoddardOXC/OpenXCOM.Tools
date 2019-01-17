using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using XCom;
using XCom.Interfaces;


namespace PckView
{
	internal delegate void SpritesetChangedEventHandler(bool valid);


	internal sealed class PckViewPanel
		:
			Panel
	{
		#region Events
		internal event SpritesetChangedEventHandler SpritesetChangedEvent;
		#endregion


		#region Fields (static)
		private const int SpriteMargin = 2;

		private const string Total = "Total ";
		private const string None  = "n/a";

		private const int _tileWidth  = XCImage.SpriteWidth  + SpriteMargin * 2 + 1;
		private static int TileHeight = 40; // default to 40px (terrain/unit height - alt is Bigobs 48px)

		private const int TableOffsetHori = 3;
		private const int TableOffsetVert = 2;
		#endregion


		#region Fields
		private readonly VScrollBar _scrollBar = new VScrollBar();
		private readonly StatusBar  _statusBar = new StatusBar();

		private readonly StatusBarPanel _sbpTilesTotal   = new StatusBarPanel();
		private readonly StatusBarPanel _sbpTileOver     = new StatusBarPanel();
		private readonly StatusBarPanel _sbpTileSelected = new StatusBarPanel();
		private readonly StatusBarPanel _sbpSpritesLabel = new StatusBarPanel();

		private int _tilesX = 1;
		private int _startY;

		private Pen   _penBlack        = new Pen(Brushes.Black, 1);
		private Pen   _penControlLight = new Pen(SystemColors.ControlLight, 1);
		private Brush _brushCrimson    = new SolidBrush(Color.Crimson);

		/// <summary>
		/// The LargeChange value for the scrollbar will return "1" when the bar
		/// isn't visible. Therefore this value needs to be used instead of the
		/// actual LargeValue in order to calculate the panel's various dynamics.
		/// </summary>
		private int _largeChange;
		#endregion


		#region Properties (static)
		internal static PckViewPanel Instance
		{ get; set; }
		#endregion


		#region Properties
		private SpriteCollectionBase _spriteset;
		internal SpriteCollectionBase Spriteset
		{
			get { return _spriteset; }
			set
			{
				_spriteset = value;

				_spriteset.Pal = PckViewForm.Pal;

				TileHeight = XCImage.SpriteHeight + SpriteMargin * 2 + 1;

				_largeChange           =
				_scrollBar.LargeChange = TileHeight;

				UpdateScrollbar(true);

				EditorPanel.Instance.Sprite = null;

				_sbpSpritesLabel.Text = _spriteset.Label + (PckViewForm.IsBigobs ? " (32x48)" : " (32x40)");

				SelectedId =
				OverId     = -1;
				PrintStatusTotal();

				if (SpritesetChangedEvent != null)
					SpritesetChangedEvent(value != null);

				// TODO: update PaletteViewer if the spriteset palette changes.
				Refresh();
			}
		}

		internal int SelectedId
		{ get; set; }

		private int OverId
		{ get; set; }

		/// <summary>
		/// Used by UpdateScrollBar() to determine its Maximum value.
		/// </summary>
		private int TableHeight
		{
			get // TODO: calculate and cache this value in the OnResize event.
			{
				SetTilesX();

				int height = 0;
				if (Spriteset != null)
					height = (Spriteset.Count / _tilesX + 2) * TileHeight;

				return height;
			}
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal PckViewPanel()
		{
#if DEBUG
			LogFile.SetLogFilePath(System.IO.Path.GetDirectoryName(Application.ExecutablePath)); // creates a logfile/ wipes the old one.
#endif
			SetStyle(ControlStyles.OptimizedDoubleBuffer
				   | ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint
				   | ControlStyles.ResizeRedraw, true);

			_scrollBar.Dock = DockStyle.Right;
			_scrollBar.SmallChange = 1;
			_scrollBar.ValueChanged += OnScrollBarValueChanged;

			_sbpTilesTotal.Text = String.Format(
											System.Globalization.CultureInfo.InvariantCulture,
											Total + None);
			_sbpTilesTotal.Width   = 85;
			_sbpTileOver.Width     = 75;
			_sbpTileSelected.Width = 100;

			_sbpSpritesLabel.AutoSize = StatusBarPanelAutoSize.Spring;
			_sbpSpritesLabel.Alignment = HorizontalAlignment.Center;

			_statusBar.Dock = DockStyle.Bottom;
			_statusBar.ShowPanels = true;
			_statusBar.Panels.Add(_sbpTilesTotal);
			_statusBar.Panels.Add(_sbpTileOver);
			_statusBar.Panels.Add(_sbpTileSelected);
			_statusBar.Panels.Add(_sbpSpritesLabel);

			Controls.AddRange(new Control[]
			{
				_scrollBar,
				_statusBar
			});


			SelectedId = -1;
			PrintStatusSpriteSelected();

			OverId = -1;
			PrintStatusSpriteOver();

			PckViewForm.PaletteChangedEvent += OnPaletteChanged; // NOTE: lives the life of the app, so no leak.

			Instance = this;
		}
		#endregion


		internal void ForceResize()
		{
			OnResize(EventArgs.Empty);
		}

		#region Eventcalls (override)
		protected override void OnResize(EventArgs eventargs)
		{
			base.OnResize(eventargs);

			if (FindForm().WindowState != FormWindowState.Minimized)
			{
				UpdateScrollbar(false);
				ScrollToTile(SelectedId);
			}
		}

		/// <summary>
		/// Scrolls the Overlay-panel with the mousewheel after OnSpriteClick
		/// has given it focus (see).
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
//			base.OnMouseWheel(e);

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
					if (_scrollBar.Value + (_scrollBar.LargeChange - 1) + _scrollBar.LargeChange > _scrollBar.Maximum)
						_scrollBar.Value = _scrollBar.Maximum - (_scrollBar.LargeChange - 1);
					else
						_scrollBar.Value += _scrollBar.LargeChange;
				}
			}
		}

		/// <summary>
		/// Selects and shows status-information for a sprite. Overrides core
		/// implementation for the MouseDown event.
		/// NOTE: This fires before PckViewForm.OnSpriteClick().
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
//			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left
				&& Spriteset != null && Spriteset.Count != 0)
			{
				// IMPORTANT: 'SelectedId' is currently allowed only 1 entry.

				int terrainId = GetTileId(e);
				if (terrainId != SelectedId)
				{
					XCImage sprite = null;

					if ((SelectedId = terrainId) != -1)
					{
						SelectedId = Spriteset[SelectedId].TerrainId; // use the proper Id of the sprite itself.
						sprite = Spriteset[SelectedId];

//						if (ModifierKeys == Keys.Control)
//						{
//							SpriteSelected spritePre = null;
//							foreach (var sprite in _selectedSprites)
//								if (sprite.X == tileX && sprite.Y == tileY)
//									spritePre = sprite;
//							if (spritePre != null)
//								_selectedSprites.Remove(spritePre);
//							else
//								_selectedSprites.Add(selected);
//						}
//						else
//							Selected.Add(selected);
					}

					EditorPanel.Instance.Sprite = sprite;

					PrintStatusSpriteSelected();
					Refresh();
				}
				ScrollToTile(SelectedId);
			}
		}

		/// <summary>
		/// Shows status-information for a sprite. Overrides core implementation
		/// for the MouseMove event.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
//			base.OnMouseMove(e);

			if (Spriteset != null && Spriteset.Count != 0)
			{
				int terrainId = GetTileId(e);
				if (terrainId != OverId)
				{
					OverId = terrainId;
					PrintStatusSpriteOver();
				}
			}
		}

		/// <summary>
		/// Clears the overId in the statusbar when the mouse-cursor leaves the
		/// panel.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeave(EventArgs e)
		{
//			base.OnMouseLeave(e);

			OverId = -1;
			PrintStatusSpriteOver();
		}

		/// <summary>
		/// Let's draw this puppy.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
//			base.OnPaint(e);

			if (Spriteset != null && Spriteset.Count != 0)
			{
				var graphics = e.Graphics;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;


				if (!_scrollBar.Visible) // indicate the reserved width for scrollbar.
					graphics.DrawLine(
									_penControlLight,
									Width - _scrollBar.Width - 1, 0,
									Width - _scrollBar.Width - 1, Height);


//				var selectedIds = new List<int>(); // track currently selected spriteIds.
//				foreach (var sprite in Selected)
//					selectedIds.Add(sprite.Sprite.TerrainId);

				for (int id = 0; id != Spriteset.Count; ++id) // fill selected tiles and draw sprites.
				{
					int tileX = id % _tilesX;
					int tileY = id / _tilesX;

//					if (selectedIds.Contains(id))
					if (id == SelectedId)
						graphics.FillRectangle(
											_brushCrimson,
											TableOffsetHori + _tileWidth * tileX,
											TableOffsetVert + TileHeight * tileY + _startY,
											TableOffsetHori + _tileWidth - SpriteMargin * 2,
											TableOffsetVert + TileHeight - SpriteMargin - 1);

					graphics.DrawImage(
									Spriteset[id].Sprite,
									TableOffsetHori + tileX * _tileWidth + SpriteMargin,
									TableOffsetVert + tileY * TileHeight + SpriteMargin + _startY);
				}


				graphics.FillRectangle(
									new SolidBrush(_penBlack.Color),
									TableOffsetHori - 1,
									TableOffsetVert + _startY - 1,
									1, 1); // so bite me.

				for (int tileX = 0; tileX <= _tilesX; ++tileX) // draw vertical lines
					graphics.DrawLine(
									_penBlack,
									new Point(
											TableOffsetHori + _tileWidth * tileX,
											TableOffsetVert + _startY),
									new Point(
											TableOffsetHori + _tileWidth * tileX,
											TableOffsetVert - _startY + Height));

				int tilesY = Spriteset.Count / _tilesX;
				if (Spriteset.Count % _tilesX != 0)
					++tilesY;

				for (int tileY = 0; tileY <= tilesY; ++tileY) // draw horizontal lines
					graphics.DrawLine(
									_penBlack,
									new Point(
											TableOffsetHori,
											TableOffsetVert + TileHeight * tileY + _startY),
									new Point(
											TableOffsetHori + _tileWidth  * _tilesX,
											TableOffsetVert + TileHeight * tileY + _startY));
			}
		}
		#endregion


		#region Eventcalls
		private void OnPaletteChanged()
		{
			Refresh();
		}

		/// <summary>
		/// Fires when anything changes the Value of the scroll-bar.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnScrollBarValueChanged(object sender, EventArgs e)
		{
			_startY = -_scrollBar.Value;
			Refresh();
		}
		#endregion


		#region Methods
		/// <summary>
		/// Gets the terrain-id of a sprite at coordinates x/y.
		/// </summary>
		/// <param name="e"></param>
		/// <returns>the terrain-id or -1 if out of bounds</returns>
		private int GetTileId(MouseEventArgs e)
		{
			if (e.X < _tilesX * _tileWidth + TableOffsetHori - 1) // not out of bounds to right
			{
				int tileX = (e.X - TableOffsetHori + 1)           / _tileWidth;
				int tileY = (e.Y - TableOffsetHori + 1 - _startY) / TileHeight;

				int terrainId = tileY * _tilesX + tileX;
				if (terrainId < Spriteset.Count) // not out of bounds below
					return terrainId;
			}
			return -1;
		}

		/// <summary>
		/// Checks if a selected tile is fully visible in the view-panel and
		/// scrolls the table to show it if not.
		/// </summary>
		/// <param name="terrainId"></param>
		private void ScrollToTile(int terrainId)
		{
			if (terrainId != -1)
			{
				int tileY = terrainId / _tilesX;

				int cutoff = tileY * TileHeight;
				if (cutoff < -_startY)		// <- check cutoff high
				{
					_scrollBar.Value = cutoff;
				}
				else						// <- check cutoff low
				{
					cutoff = (tileY + 1) * TileHeight - Height + _statusBar.Height + TableOffsetVert + 1;
					if (cutoff > -_startY)
					{
						_scrollBar.Value = cutoff;
					}
				}
			}
		}

		/// <summary>
		/// Updates the scrollbar after a resize event or a spriteset changed
		/// event.
		/// </summary>
		/// <param name="resetTrack">true to set the thing to the top of the track</param>
		private void UpdateScrollbar(bool resetTrack)
		{
			int range = 0;
			if (Spriteset != null && Spriteset.Count != 0)
			{
				if (resetTrack)
					_scrollBar.Value = 0;

				range = TableOffsetVert + TableHeight + _largeChange - Height - _statusBar.Height;
				if (range < _largeChange)
					range = 0;
			}
			_scrollBar.Maximum = range;
			_scrollBar.Visible = (range != 0);
		}

		private void SetTilesX()
		{
			int tilesX = 1;

			if (Spriteset != null && Spriteset.Count != 0)
			{
				tilesX = (Width - TableOffsetHori - _scrollBar.Width - 1) / _tileWidth;

				if (tilesX > Spriteset.Count)
					tilesX = Spriteset.Count;
			}
			_tilesX = tilesX;
		}

		/// <summary>
		/// Prints the quantity of sprites in the currently loaded spriteset to
		/// the statusbar. Note that this will clear the sprite-over info.
		/// </summary>
		internal void PrintStatusTotal()
		{
			PrintStatusSpriteOver();
			PrintStatusSpriteSelected();

			_sbpTilesTotal.Text = String.Format(
											System.Globalization.CultureInfo.InvariantCulture,
											Total + "{0}", Spriteset.Count);
		}

		/// <summary>
		/// Updates the status-information for the sprite that is currently
		/// selected.
		/// </summary>
		internal void PrintStatusSpriteSelected()
		{
			int selectedId = SelectedId + 1;
			string selected = (selectedId != 0) ? selectedId.ToString(System.Globalization.CultureInfo.InvariantCulture)
												: None;
			_sbpTileSelected.Text = String.Format(
												System.Globalization.CultureInfo.InvariantCulture,
												"Selected {0}", selected);
		}

		/// <summary>
		/// Updates the status-information for the sprite that the cursor is
		/// currently over.
		/// </summary>
		private void PrintStatusSpriteOver()
		{
			int overId = OverId + 1;
			string over = (overId != 0) ? overId.ToString(System.Globalization.CultureInfo.InvariantCulture)
										: None;
			_sbpTileOver.Text = String.Format(
										System.Globalization.CultureInfo.InvariantCulture,
										"Over {0}", over);
		}
		#endregion
	}
}

//		/// <summary>
//		/// Deletes the currently selected sprite and selects another one.
//		/// </summary>
//		internal void SpriteDelete()
//		{
//			if (Selected.Count != 0)
//			{
//				var lowestId = Int32.MaxValue;
//
//				var selectedIds = new List<int>();
//				foreach (var sprite in Selected)
//					selectedIds.Add(sprite.Id);
//
//				selectedIds.Sort();
//				selectedIds.Reverse();
//
//				foreach (var id in selectedIds)
//				{
//					if (id < lowestId)
//						lowestId = id;
//
//					Spriteset.Remove(id);
//				}
//
//				if (lowestId > 0 && lowestId == Spriteset.Count)
//					lowestId = Spriteset.Count - 1;
//
//				Selected.Clear();
//	
//				if (Spriteset.Count != 0)
//				{
//					var selected = new SelectedSprite();
//					selected.Y   = lowestId / _tilesX;
//					selected.X   = lowestId - selected.Y;
//					selected.Id  = selected.X + selected.Y * _tilesX;
//	
//					Selected.Add(selected);
//				}
//			}
//		}

//		internal void Hq2x()
//		{
//			_collection.HQ2X();
//		}
//		internal void Hq2x()
//		{
//			_viewPanel.Hq2x();
//		}
