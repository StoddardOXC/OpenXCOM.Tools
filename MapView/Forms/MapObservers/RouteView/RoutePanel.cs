using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using MapView.Forms.MapObservers.TopViews;

using XCom;


namespace MapView.Forms.MapObservers.RouteViews
{
	/// <summary>
	/// The derived class for RoutePanel. Handles drawing/painting the panel.
	/// </summary>
	internal sealed class RoutePanel
		:
			RoutePanelParent
	{
		#region Fields (static)
		private const int RoseMarginX = 25;
		private const int RoseMarginY = 5;

		private const int NodeValMax = 12;

		private const int Separator = 0;

		private const string Over   = "id"; // these are for the translucent overlay box ->
		private const string Rank   = "rank";
		private const string Spawn  = "spawn";
		private const string Patrol = "patrol";

		private const string textTile1 = ""; // "position" or "location" or ... "pos" or "loc" ...
		#endregion


		#region Fields
		private readonly Font _fontOverlay = new Font("Verdana", 7F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
		private readonly Font _fontRose    = new Font("Courier New", 22, FontStyle.Bold);

		private ColorTools _toolWall;
		private ColorTools _toolContent;

		private Graphics     _graphics;
		private GraphicsPath _nodeFill = new GraphicsPath();

		private bool _brushesInited;

		private SolidBrush _brushSelected;
		private SolidBrush _brushUnselected;
		private SolidBrush _brushSpawn;

		private Pen _penLinkSelected;
		private Pen _penLinkUnselected;
		#endregion


		#region Properties (static)
		internal static RouteNode NodeSelected
		{ private get; set; }
		#endregion


		#region Properties
		private Point _spot = new Point(-1, -1);
		/// <summary>
		/// The location of the tile that is highlighted by a mouse-overed Go
		/// button.
		/// </summary>
		internal Point SpotPosition
		{
			private get { return _spot; }
			set { _spot = value; }
		}
		#endregion


		/// <summary>
		/// You know the drill ... Paint it, Black
		/// black as night
		/// NOTE: Pens and Brushes need to be refreshed each call to draw since
		/// they can be changed in Options. Or not ....
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			_graphics = e.Graphics;
			_graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			ControlPaint.DrawBorder3D(_graphics, ClientRectangle, Border3DStyle.Etched);


//			try // TODO: i get the impression that many of the try/catch blocks can and should be replaced w/ standard code.
//			{
			if (MapFile != null)
			{
				BlobService.HalfWidth  = DrawAreaWidth;
				BlobService.HalfHeight = DrawAreaHeight;

				_penLinkSelected   = RoutePens[RouteView.SelectedLinkColor];
				_penLinkUnselected = RoutePens[RouteView.UnselectedLinkColor];

				DrawBlobs();

				DrawLinks();

				if (NodeSelected != null)
					DrawLinkLines(
							Origin.X + (SelectedPosition.X - SelectedPosition.Y)     * DrawAreaWidth,
							Origin.Y + (SelectedPosition.X + SelectedPosition.Y + 1) * DrawAreaHeight,
							NodeSelected,
							true);

				DrawNodes();

				DrawGridLines();

				if (Focused && _overCol != -1) // draw the selector lozenge
				{
					PathSelectorLozenge(
									Origin.X + (_overCol - _overRow) * DrawAreaWidth,
									Origin.Y + (_overCol + _overRow) * DrawAreaHeight);
					_graphics.DrawPath(
									new Pen( // TODO: make this a separate Option.
											RoutePens[RouteView.GridLineColor].Color,
											RoutePens[RouteView.GridLineColor].Width + 1),
									LozSelector);
				}

				if (MainViewUnderlay.Instance.MainViewOverlay.FirstClick)
				{
					_graphics.DrawPath(
									new Pen( // TODO: make this a separate Option.
											RouteBrushes[RouteView.SelectedNodeColor].Color,
											RoutePens[RouteView.GridLineColor].Width + 1),
									LozSelected);

					if (SpotPosition.X > -1)
					{
						PathSpottedLozenge(
										Origin.X + (SpotPosition.X - SpotPosition.Y) * DrawAreaWidth,
										Origin.Y + (SpotPosition.X + SpotPosition.Y) * DrawAreaHeight);
						_graphics.DrawPath(
										new Pen( // TODO: make this a separate Option.
												RouteBrushes[RouteView.SelectedNodeColor].Color,
												RoutePens[RouteView.GridLineColor].Width + 1),
										LozSpotted);
					}
				}

				if (ShowPriorityBars)
					DrawNodeImportanceMeters();

				DrawRose();

				if (ShowOverlay && CursorPosition.X != -1)
					DrawInfoOverlay();
			}
//			}
//			catch (Exception ex)
//			{
//				g.FillRectangle(new SolidBrush(Color.Black), g.ClipBounds);
//				g.DrawString(
//							ex.Message,
//							Font,
//							new SolidBrush(Color.White),
//							8, 8);
//				throw;
//			}
		}


		#region Draw Methods
		/// <summary>
		/// Draws any wall and/or content indicators.
		/// </summary>
		private void DrawBlobs()
		{
			if (_toolWall == null)
				_toolWall = new ColorTools(RoutePens[RouteView.WallColor]);

			if (_toolContent == null)
				_toolContent = new ColorTools(RouteBrushes[RouteView.ContentColor], _toolWall.Pen.Width);


			XCMapTile tile = null;
			for (int
					r = 0,
						startX = Origin.X,
						startY = Origin.Y;
					r != MapFile.MapSize.Rows;
					++r,
						startX -= DrawAreaWidth,
						startY += DrawAreaHeight)
			{
				for (int
						c = 0,
							x = startX,
							y = startY;
						c != MapFile.MapSize.Cols;
						++c,
							x += DrawAreaWidth,
							y += DrawAreaHeight)
				{
					if (MapFile[r, c] != null)
					{
						tile = MapFile[r, c] as XCMapTile;

						if (tile.Content != null)
							BlobService.DrawContent(_graphics, _toolContent, x, y, tile.Content);

						if (tile.West != null)
							BlobService.DrawContent(_graphics, _toolWall, x, y, tile.West);

						if (tile.North != null)
							BlobService.DrawContent(_graphics, _toolWall, x, y, tile.North);
					}
				}
			}
		}

		/// <summary>
		/// Draws unselected link-lines.
		/// </summary>
		private void DrawLinks()
		{
			RouteNode node;
			for (int
					rSrc = 0,
						x = Origin.X,
						y = Origin.Y;
					rSrc != MapFile.MapSize.Rows;
					++rSrc,
						x -= DrawAreaWidth,
						y += DrawAreaHeight)
			{
				for (int
						cSrc = 0,
							xSrc = x,
							ySrc = y;
						cSrc != MapFile.MapSize.Cols;
						++cSrc,
							xSrc += DrawAreaWidth,
							ySrc += DrawAreaHeight)
				{
					if (MapFile[rSrc, cSrc] != null
						&& (node = ((XCMapTile)MapFile[rSrc, cSrc]).Node) != null
						&& (NodeSelected == null || node != NodeSelected))
					{
						DrawLinkLines(xSrc, ySrc, node, false);
					}
				}
			}
		}

		/// <summary>
		/// Draws link-lines for a given node.
		/// </summary>
		/// <param name="xSrc"></param>
		/// <param name="ySrc"></param>
		/// <param name="node"></param>
		/// <param name="selected">(default true)</param>
		private void DrawLinkLines(
				int xSrc,
				int ySrc,
				RouteNode node,
				bool selected = true)
		{
			int xDst, yDst;
			RouteNode dest;
			byte destId;

			for (int slot = 0; slot != RouteNode.LinkSlots; ++slot)
			{
				var link = node[slot] as Link;
				if ((destId = link.Destination) != Link.NotUsed)
				{
					switch (destId)
					{
						case Link.ExitWest:
							if (node.Lev != MapFile.Level)
								continue;

							xDst = OffsetX + 1;
							yDst = OffsetY + 1;
							dest = null;
							break;

						case Link.ExitNorth:
							if (node.Lev != MapFile.Level)
								continue;

							xDst = Width - OffsetX * 2;
							yDst =         OffsetY + 1;
							dest = null;
							break;

						case Link.ExitEast:
							if (node.Lev != MapFile.Level)
								continue;

							xDst = Width  - OffsetX * 2;
							yDst = Height - OffsetY * 2;
							dest = null;
							break;

						case Link.ExitSouth:
							if (node.Lev != MapFile.Level)
								continue;

							xDst =          OffsetX + 1;
							yDst = Height - OffsetY * 2;
							dest = null;
							break;

						default:
							if ((dest = MapFile.Routes[destId]) == null
								|| dest.Lev != MapFile.Level
								|| (NodeSelected != null && dest == NodeSelected)
								|| RouteNodeCollection.IsNodeOutsideMapBounds(
																			dest,
																			MapFile.MapSize.Cols,
																			MapFile.MapSize.Rows,
																			MapFile.MapSize.Levs))
							{
								continue;
							}

							xDst = Origin.X + (dest.Col - dest.Row)     * DrawAreaWidth;
							yDst = Origin.Y + (dest.Col + dest.Row + 1) * DrawAreaHeight;
							break;
					}

					if (selected) // draw link-lines for a selected node ->
					{
						var pen = _penLinkSelected;

						if (SpotPosition.X != -1)
						{
							if (dest != null)
							{
								if (   SpotPosition.X != dest.Col
									|| SpotPosition.Y != dest.Row)
								{
									pen = _penLinkUnselected;
								}
							}
							else
							{
								switch (destId)							// See RouteView.SpotGoDestination() for
								{										// def'n of the following spot-positions ->
									case Link.ExitNorth:
										if (SpotPosition.X != -2)
											pen = _penLinkUnselected;
										break;
									case Link.ExitEast:
										if (SpotPosition.X != -3)
											pen = _penLinkUnselected;
										break;
									case Link.ExitSouth:
										if (SpotPosition.X != -4)
											pen = _penLinkUnselected;
										break;
									case Link.ExitWest:
										if (SpotPosition.X != -5)
											pen = _penLinkUnselected;
										break;
								}
							}
						}
						_graphics.DrawLine(
										pen,
										xSrc, ySrc,
										xDst, yDst);
					}
					else // draw link-lines for a non-selected node ->
						_graphics.DrawLine(
										_penLinkUnselected,
										xSrc, ySrc + DrawAreaHeight, // unselected nodes need an offset
										xDst, yDst);
				}
			}
		}

		/// <summary>
		/// Draws the nodes.
		/// </summary>
		private void DrawNodes()
		{
			if (!_brushesInited)
			{
				_brushesInited = true;

				_brushSelected   = RouteBrushes[RouteView.SelectedNodeColor];
				_brushUnselected = RouteBrushes[RouteView.UnselectedNodeColor];
				_brushSpawn      = RouteBrushes[RouteView.SpawnNodeColor];
			}

			_brushSelected.Color   = Color.FromArgb(Opacity, _brushSelected.Color); // NOTE: the opacity changes depending on Options.
			_brushUnselected.Color = Color.FromArgb(Opacity, _brushUnselected.Color);
			_brushSpawn.Color      = Color.FromArgb(Opacity, _brushSpawn.Color);


			int startX = Origin.X;
			int startY = Origin.Y;

			for (int row = 0; row != MapFile.MapSize.Rows; ++row)
			{
				for (int
						col = 0,
							x = startX,
							y = startY;
						col != MapFile.MapSize.Cols;
						++col,
							x += DrawAreaWidth,
							y += DrawAreaHeight)
				{
					var tile = MapFile[row, col] as XCMapTile;	// NOTE: MapFileBase has the current level stored and uses
					if (tile != null)							// it to return only tiles on the correct level here.
					{
						var node = tile.Node;
						if (node != null)
						{
							_nodeFill.Reset();
							_nodeFill.AddLine(
											x,                 y,
											x + DrawAreaWidth, y + DrawAreaHeight);
							_nodeFill.AddLine(
											x + DrawAreaWidth, y + DrawAreaHeight,
											x,                 y + DrawAreaHeight * 2);
							_nodeFill.AddLine(
											x,                 y + DrawAreaHeight * 2,
											x - DrawAreaWidth, y + DrawAreaHeight);
							_nodeFill.CloseFigure();

							if (NodeSelected != null && MapFile.Level == NodeSelected.Lev
								&& col == SelectedPosition.X
								&& row == SelectedPosition.Y)
							{
								_graphics.FillPath(_brushSelected, _nodeFill);
							}
							else if (node.Spawn != SpawnWeight.None)
							{
								_graphics.FillPath(_brushSpawn, _nodeFill);
							}
							else
								_graphics.FillPath(_brushUnselected, _nodeFill);


							for (int i = 0; i != RouteNode.LinkSlots; ++i) // check for and if applicable draw the up/down indicators.
							{
								var link = node[i] as Link;
								switch (link.Destination)
								{
									case Link.NotUsed:
									case Link.ExitEast:
									case Link.ExitNorth:
									case Link.ExitSouth:
									case Link.ExitWest:
										break;

									default:
										if (MapFile.Routes[link.Destination] != null)
										{
											int levelDestination = MapFile.Routes[link.Destination].Lev;
											if (levelDestination != MapFile.Level)
											{
												if (levelDestination < MapFile.Level) // draw arrow up.
												{
													_graphics.DrawLine( // start w/ a vertical line in the tile-lozenge
																	_penLinkUnselected,
																	x, y,
																	x, y + DrawAreaHeight * 2);
													_graphics.DrawLine( // then lines on the two top edges of the tile
																	_penLinkUnselected,
																	x + 2,                 y,
																	x + 2 - DrawAreaWidth, y + DrawAreaHeight);
													_graphics.DrawLine(
																	_penLinkUnselected,
																	x - 2,                 y,
																	x - 2 + DrawAreaWidth, y + DrawAreaHeight);
												}
												else //if (levelDestination > MapFile.Level) // draw arrow down.
												{
													_graphics.DrawLine( // start w/ a horizontal line in the tile-lozenge
																	_penLinkUnselected,
																	x - DrawAreaWidth, y + DrawAreaHeight,
																	x + DrawAreaWidth, y + DrawAreaHeight);
													_graphics.DrawLine( // then lines on the two bottom edges of the tile
																	_penLinkUnselected,
																	x + 2,                 y + DrawAreaHeight * 2,
																	x + 2 - DrawAreaWidth, y + DrawAreaHeight);
													_graphics.DrawLine(
																	_penLinkUnselected,
																	x - 2,                 y + DrawAreaHeight * 2,
																	x - 2 + DrawAreaWidth, y + DrawAreaHeight);
												}
											}
										}
										break;
								}
							}
						}
					}
				}
				startX -= DrawAreaWidth;
				startY += DrawAreaHeight;
			}
		}

		/// <summary>
		/// Draws the grid-lines.
		/// </summary>
		private void DrawGridLines()
		{
			Pen pen;
			for (int i = 0; i <= MapFile.MapSize.Rows; ++i)
			{
				if (i % 10 == 0) pen = RoutePens[RouteView.Grid10LineColor];
				else             pen = RoutePens[RouteView.GridLineColor];

				_graphics.DrawLine(
								pen,
								Origin.X - i * DrawAreaWidth,
								Origin.Y + i * DrawAreaHeight,
								Origin.X + ((MapFile.MapSize.Cols - i) * DrawAreaWidth),
								Origin.Y + ((MapFile.MapSize.Cols + i) * DrawAreaHeight));
			}

			for (int i = 0; i <= MapFile.MapSize.Cols; ++i)
			{
				if (i % 10 == 0) pen = RoutePens[RouteView.Grid10LineColor];
				else             pen = RoutePens[RouteView.GridLineColor];

				_graphics.DrawLine(
								pen,
								Origin.X + i * DrawAreaWidth,
								Origin.Y + i * DrawAreaHeight,
							   (Origin.X + i * DrawAreaWidth)  - MapFile.MapSize.Rows * DrawAreaWidth,
							   (Origin.Y + i * DrawAreaHeight) + MapFile.MapSize.Rows * DrawAreaHeight);
			}
		}

		/// <summary>
		/// Draws the node importance bars.
		/// </summary>
		private void DrawNodeImportanceMeters()
		{
			int startX = Origin.X;
			int startY = Origin.Y;

			for (int r = 0; r != MapFile.MapSize.Rows; ++r)
			{
				for (int
						c = 0,
							x = startX,
							y = startY;
						c != MapFile.MapSize.Cols;
						++c,
							x += DrawAreaWidth,
							y += DrawAreaHeight)
				{
					var tile = MapFile[r, c] as XCMapTile;
					if (tile != null)
					{
						var node = tile.Node;
						if (node != null)
						{
//								if (DrawAreaHeight >= NodeValMax)
//								{
								int infoboxX = x - DrawAreaWidth / 2 - 2;			// -2 to prevent drawing over the link-going-up
								int infoboxY = y + DrawAreaHeight - NodeValMax / 2;	//    vertical line indicator when panel is small sized.

								DrawImportanceMeter(
												infoboxX,
												infoboxY,
												(int)node.Spawn,
												Brushes.LightCoral); //IndianRed

								DrawImportanceMeter(
												infoboxX + 3,
												infoboxY,
												(int)node.Patrol,
												Brushes.DeepSkyBlue); //CornflowerBlue
//								}
						}
					}
				}
				startX -= DrawAreaWidth;
				startY += DrawAreaHeight;
			}
		}

		/// <summary>
		/// Helper for DrawNodeImportanceMeters().
		/// </summary>
		/// <param name="infoboxX"></param>
		/// <param name="infoboxY"></param>
		/// <param name="value"></param>
		/// <param name="color"></param>
		private void DrawImportanceMeter(
				int infoboxX,
				int infoboxY,
				int value,
				Brush color)
		{
			var p0 = new Point(
							infoboxX - 1, // ...
							infoboxY);
			var p1 = new Point(
							infoboxX + 3,
							infoboxY);
			var p2 = new Point(
							infoboxX + 3,
							infoboxY + NodeValMax - 1);
			var p3 = new Point(
							infoboxX,
							infoboxY + NodeValMax - 1);
			var p4 = new Point(
							infoboxX,
							infoboxY);

			var path = new GraphicsPath();

			path.AddLine(p0, p1);
			path.AddLine(p1, p2);
			path.AddLine(p2, p3);
			path.AddLine(p3, p4);

			_graphics.FillPath(Brushes.WhiteSmoke, path); // fill background.

			if (value > 0)
				_graphics.FillRectangle(
									color,
									infoboxX, infoboxY + NodeValMax - value - 2,
									2, value);

			_graphics.DrawPath(Pens.Black, path); // draw borders.
		}

		/// <summary>
		/// Draws the compass-rose.
		/// </summary>
		private void DrawRose()
		{
			_graphics.DrawString(
							"W",
							_fontRose,
							Brushes.Black,
							RoseMarginX,
							RoseMarginY);
			_graphics.DrawString(
							"N",
							_fontRose,
							Brushes.Black,
//							Width - TextRenderer.MeasureText("N", _fontRose).Width - RoseMarginX,
							Width - (int)_graphics.MeasureString("N", _fontRose).Width - RoseMarginX,
							RoseMarginY);
			_graphics.DrawString(
							"S",
							_fontRose,
							Brushes.Black,
							RoseMarginX,
							Height - _fontRose.Height - RoseMarginY);
			_graphics.DrawString(
							"E",
							_fontRose,
							Brushes.Black,
//							Width  - TextRenderer.MeasureText("E", _fontRose).Width - RoseMarginX,
							Width  - (int)_graphics.MeasureString("E", _fontRose).Width - RoseMarginX,
							Height - _fontRose.Height - RoseMarginY);
		}

		/// <summary>
		/// Draws tile/node information in the overlay.
		/// </summary>
		private void DrawInfoOverlay()
		{
			int x = CursorPosition.X;
			int y = CursorPosition.Y;

			var tile = GetTile(ref x, ref y); // x/y -> tile-location
			if (tile != null)
			{
				string textTile2 = "c " + x + "  r " + y + "  L " + (MapFile.MapSize.Levs - MapFile.Level);

				int textWidth1 = (int)_graphics.MeasureString(textTile1, _fontOverlay).Width;
				int textWidth2 = (int)_graphics.MeasureString(textTile2, _fontOverlay).Width;

//				int textWidth1 = TextRenderer.MeasureText(textTile1, font).Width;
//				int textWidth2 = TextRenderer.MeasureText(textTile2, font).Width;

				string textOver1   = String.Empty;
				string textRank1   = String.Empty;
				string textSpawn1  = String.Empty;
				string textPatrol1 = String.Empty;

				string textOver2   = String.Empty;
				string textRank2   = String.Empty;
				string textSpawn2  = String.Empty;
				string textPatrol2 = String.Empty;

				if (tile.Node != null)
				{
					textOver1   = Over;
					textRank1   = Rank;
					textSpawn1  = Spawn;
					textPatrol1 = Patrol;

					textOver2   = (tile.Node.Index).ToString(System.Globalization.CultureInfo.CurrentCulture);
					if (MapFile.Parts[0][0].Pal == Palette.UfoBattle)
						textRank2 = (RouteNodeCollection.NodeRankUfo[tile.Node.Rank]).ToString();
					else
						textRank2 = (RouteNodeCollection.NodeRankTftd[tile.Node.Rank]).ToString();
					textSpawn2  = (tile.Node.Spawn).ToString();
					textPatrol2 = (tile.Node.Patrol).ToString();

					int width;
					width = (int)_graphics.MeasureString(textOver1, _fontOverlay).Width;
					if (width > textWidth1) textWidth1 = width;
					width = (int)_graphics.MeasureString(textRank1, _fontOverlay).Width;
					if (width > textWidth1) textWidth1 = width;
					width = (int)_graphics.MeasureString(textSpawn1, _fontOverlay).Width;
					if (width > textWidth1) textWidth1 = width;
					width = (int)_graphics.MeasureString(textPatrol1, _fontOverlay).Width;
					if (width > textWidth1) textWidth1 = width;

					width = (int)_graphics.MeasureString(textOver2, _fontOverlay).Width;
					if (width > textWidth2) textWidth2 = width;
					width = (int)_graphics.MeasureString(textRank2, _fontOverlay).Width;
					if (width > textWidth2) textWidth2 = width;
					width = (int)_graphics.MeasureString(textSpawn2, _fontOverlay).Width;
					if (width > textWidth2) textWidth2 = width;
					width = (int)_graphics.MeasureString(textPatrol2, _fontOverlay).Width;
					if (width > textWidth2) textWidth2 = width;

					// time to move to a higher .NET framework.

//					width = TextRenderer.MeasureText(textOver1, font).Width;
//					width = TextRenderer.MeasureText(textPriority1, font).Width;
//					width = TextRenderer.MeasureText(textSpawn1, font).Width;
//					width = TextRenderer.MeasureText(textWeight1, font).Width;
//					width = TextRenderer.MeasureText(textOver2, font).Width;
//					width = TextRenderer.MeasureText(textPriority2, font).Width;
//					width = TextRenderer.MeasureText(textSpawn2, font).Width;
//					width = TextRenderer.MeasureText(textWeight2, font).Width;
				}

//				int textHeight = TextRenderer.MeasureText("X", font).Height;
				int textHeight = (int)_graphics.MeasureString("X", _fontOverlay).Height + 1; // pad +1
				var overlay = new Rectangle(
										CursorPosition.X + 18, CursorPosition.Y,
										textWidth1 + Separator + textWidth2 + 5, textHeight + 7); // trim right & bottom (else +8 for both w/h)

				if (tile.Node != null)
					overlay.Height += textHeight * 4;

				if (overlay.X + overlay.Width > ClientRectangle.Width)
					overlay.X = CursorPosition.X - overlay.Width - 8;

				if (overlay.Y + overlay.Height > ClientRectangle.Height)
					overlay.Y = CursorPosition.Y - overlay.Height;

				_graphics.FillRectangle(new SolidBrush(Color.FromArgb(180, Color.DarkBlue)), overlay);
				_graphics.FillRectangle(
									new SolidBrush(Color.FromArgb(120, Color.Linen)),
									overlay.X + 2,
									overlay.Y + 2,
									overlay.Width  - 4,
									overlay.Height - 4);

				int textLeft = overlay.X + 4;
				int textTop  = overlay.Y + 3;

				_graphics.DrawString(
								textTile1,
								_fontOverlay,
								Brushes.Yellow,
								textLeft,
								textTop);
				_graphics.DrawString(
								textTile2,
								_fontOverlay,
								Brushes.Yellow,
								textLeft + textWidth1 + Separator,
								textTop);

				if (tile.Node != null)
				{
					_graphics.DrawString(
									textOver1,
									_fontOverlay,
									Brushes.Yellow,
									textLeft,
									textTop + textHeight);
					_graphics.DrawString(
									textOver2,
									_fontOverlay,
									Brushes.Yellow,
									textLeft + textWidth1 + Separator,
									textTop  + textHeight);

					_graphics.DrawString(
									textRank1,
									_fontOverlay,
									Brushes.Yellow,
									textLeft,
									textTop + textHeight * 2);
					_graphics.DrawString(
									textRank2,
									_fontOverlay,
									Brushes.Yellow,
									textLeft + textWidth1 + Separator,
									textTop  + textHeight * 2);

					_graphics.DrawString(
									textSpawn1,
									_fontOverlay,
									Brushes.Yellow,
									textLeft,
									textTop + textHeight * 3);
					_graphics.DrawString(
									textSpawn2,
									_fontOverlay,
									Brushes.Yellow,
									textLeft + textWidth1 + Separator,
									textTop  + textHeight * 3);

					_graphics.DrawString(
									textPatrol1,
									_fontOverlay,
									Brushes.Yellow,
									textLeft,
									textTop + textHeight * 4);
					_graphics.DrawString(
									textPatrol2,
									_fontOverlay,
									Brushes.Yellow,
									textLeft + textWidth1 + Separator,
									textTop  + textHeight * 4);
				}
			}
		}
		#endregion
	}
}
