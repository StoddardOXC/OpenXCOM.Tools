using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

using MapView.Forms.MainWindow;

using XCom;
using XCom.Interfaces;


namespace MapView.Forms.MapObservers.TopViews
{
	internal sealed class QuadrantPanelDrawService
		:
			IDisposable
	{
		#region Fields
		private const int MarginHori = 5;
		private const int MarginVert = 3;

		internal const int QuadWidthTotal = XCImage.SpriteWidth + MarginHori * 2;

		internal const int StartX = 26;
		private  const int StartY =  3;

		private const int SwatchHeight = 5;

		// NOTE: keep the door-string and its placement consistent with
		// TilePanel.OnPaint().
		private const string Door = "door";
		private static int DoorWidth;
		private const int PrintOffsetY = 2;

		private const string Floor   = "fLoOr";
		private const string West    = "WEst";
		private const string North   = "noRtH";
		private const string Content = "ConTeNt";

		private static int FloorWidth;
		private static int WestWidth;
		private static int NorthWidth;
		private static int ContentWidth;

		private static bool Inited;
		#endregion


		#region Properties
		internal SolidBrush Brush
		{ get; set; }

		internal Dictionary<string, SolidBrush> Brushes
		{ get; set; }

		internal Dictionary<string, Pen> Pens
		{ get; set; }

		internal Font Font
		{ get; set; }
		#endregion

		private readonly GraphicsPath _pathFloor   = new GraphicsPath();
		private readonly GraphicsPath _pathWest    = new GraphicsPath();
		private readonly GraphicsPath _pathNorth   = new GraphicsPath();
		private readonly GraphicsPath _pathContent = new GraphicsPath();


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal QuadrantPanelDrawService()
		{
			Font  = new Font("Comic Sans MS", 7);
			Brush = new SolidBrush(Color.LightBlue);

			for (int quad = 0; quad != 4; ++quad) // cache each quadrant's rectangular bounding path
			{
				var p0 = new Point(
								StartX + QuadWidthTotal * quad - 1,
								StartY);
				var p1 = new Point(
								StartX + QuadWidthTotal * quad + XCImage.SpriteWidth + 1,
								StartY);
				var p2 = new Point(
								StartX + QuadWidthTotal * quad + XCImage.SpriteWidth + 1,
								StartY + XCImage.SpriteHeight40 + 1);
				var p3 = new Point(
								StartX + QuadWidthTotal * quad,
								StartY + XCImage.SpriteHeight40 + 1);
				var p4 = new Point(
								StartX + QuadWidthTotal * quad,
								StartY);

				var path = new GraphicsPath();

				path.AddLine(p0, p1); // NOTE: 'p4' appears to be needed since the origin of 'p0'
				path.AddLine(p1, p2); // does not get drawn.
				path.AddLine(p2, p3);
				path.AddLine(p3, p4); // NOTE: try DrawRectangle() it's even worse.

				switch (quad)
				{
					case 0: _pathFloor   = path; break;
					case 1: _pathWest    = path; break;
					case 2: _pathNorth   = path; break;
					case 3: _pathContent = path; break;
				}
			}
		}
		#endregion


		#region Methods
		internal void Draw(
				Graphics graphics,
				XCMapTile tile,
				QuadrantType selectedQuadrant)
		{
			if (!Globals.RT) return; // don't try to draw the QuadrantPanel in the designer.

			if (!Inited)
			{
				Inited = true;

				DoorWidth    = (int)graphics.MeasureString(Door,    Font).Width;
				FloorWidth   = (int)graphics.MeasureString(Floor,   Font).Width;
				WestWidth    = (int)graphics.MeasureString(West,    Font).Width;
				NorthWidth   = (int)graphics.MeasureString(North,   Font).Width;
				ContentWidth = (int)graphics.MeasureString(Content, Font).Width;
			}

			var topView = ViewerFormsManager.TopView.Control;

			// fill the background of the selected quadrant type
			switch (selectedQuadrant)
			{
				case QuadrantType.Ground:
					if (topView.GroundVisible)
						graphics.FillPath(Brush, _pathFloor);
					break;

				case QuadrantType.West:
					if (topView.WestVisible)
						graphics.FillPath(Brush, _pathWest);
					break;

				case QuadrantType.North:
					if (topView.NorthVisible)
						graphics.FillPath(Brush, _pathNorth);
					break;

				case QuadrantType.Content:
					if (topView.ContentVisible)
						graphics.FillPath(Brush, _pathContent);
					break;
			}

			// fill the background of !Visible quads incl/ the selected-quad
			if (!topView.GroundVisible)
				graphics.FillPath(System.Drawing.Brushes.DarkGray, _pathFloor);

			if (!topView.WestVisible)
				graphics.FillPath(System.Drawing.Brushes.DarkGray, _pathWest);

			if (!topView.NorthVisible)
				graphics.FillPath(System.Drawing.Brushes.DarkGray, _pathNorth);

			if (!topView.ContentVisible)
				graphics.FillPath(System.Drawing.Brushes.DarkGray, _pathContent);


			// draw the Sprites
			int anistep = MainViewUnderlay.AniStep;

			// Ground ->
			if (tile != null && tile.Ground != null)
			{
				graphics.DrawImage(
								tile.Ground[anistep].Sprite,
								StartX,
								StartY - tile.Ground.Record.TileOffset);

				if (tile.Ground.Record.HumanDoor || tile.Ground.Record.UfoDoor)
					DrawDoorString(graphics, QuadrantType.Ground);
			}
			else
				graphics.DrawImage(
								Globals.ExtraSprites[3].Sprite,
								StartX,
								StartY);

			// Westwall ->
			if (tile != null && tile.West != null)
			{
				graphics.DrawImage(
								tile.West[anistep].Sprite,
								StartX + QuadWidthTotal,
								StartY - tile.West.Record.TileOffset);

				if (tile.West.Record.HumanDoor || tile.West.Record.UfoDoor)
					DrawDoorString(graphics, QuadrantType.West);
			}
			else
				graphics.DrawImage(
								Globals.ExtraSprites[1].Sprite,
								StartX + QuadWidthTotal,
								StartY);

			// Northwall ->
			if (tile != null && tile.North != null)
			{
				graphics.DrawImage(
								tile.North[anistep].Sprite,
								StartX + QuadWidthTotal * 2,
								StartY - tile.North.Record.TileOffset);

				if (tile.North.Record.HumanDoor || tile.North.Record.UfoDoor)
					DrawDoorString(graphics, QuadrantType.North);
			}
			else
				graphics.DrawImage(
								Globals.ExtraSprites[2].Sprite,
								StartX + QuadWidthTotal * 2,
								StartY);

			// Content ->
			if (tile != null && tile.Content != null)
			{
				graphics.DrawImage(
								tile.Content[anistep].Sprite,
								StartX + QuadWidthTotal * 3,
								StartY - tile.Content.Record.TileOffset);

				if (tile.Content.Record.HumanDoor || tile.Content.Record.UfoDoor)
					DrawDoorString(graphics, QuadrantType.Content);
			}
			else
				graphics.DrawImage(
								Globals.ExtraSprites[4].Sprite,
								StartX + QuadWidthTotal * 3,
								StartY);


			// draw each quadrant's bounding rectangle
			graphics.DrawPath(System.Drawing.Pens.Black, _pathFloor);
			graphics.DrawPath(System.Drawing.Pens.Black, _pathWest);
			graphics.DrawPath(System.Drawing.Pens.Black, _pathNorth);
			graphics.DrawPath(System.Drawing.Pens.Black, _pathContent);


			// draw the quad-type label under each quadrant
			DrawTypeString(graphics, QuadrantType.Ground);
			DrawTypeString(graphics, QuadrantType.West);
			DrawTypeString(graphics, QuadrantType.North);
			DrawTypeString(graphics, QuadrantType.Content);


			// fill the color-swatch under each quadrant-label
			if (Brushes != null && Pens != null)
			{
				FillSwatchColor(graphics, QuadrantType.Ground);
				FillSwatchColor(graphics, QuadrantType.West);
				FillSwatchColor(graphics, QuadrantType.North);
				FillSwatchColor(graphics, QuadrantType.Content);
			}
		}

		/// <summary>
		/// Draws the "door" string if applicable.
		/// </summary>
		private void DrawDoorString(Graphics graphics, QuadrantType quadType)
		{
			graphics.DrawString(
							Door,
							Font,
							System.Drawing.Brushes.Black,
							StartX + (XCImage.SpriteWidth - DoorWidth) / 2 + QuadWidthTotal * (int)quadType + 1,
							StartY +  XCImage.SpriteHeight40 - Font.Height + PrintOffsetY);
		}

		/// <summary>
		/// Draws the type of quadrant under its rectangle.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="quadType"></param>
		private void DrawTypeString(Graphics graphics, QuadrantType quadType)
		{
			string type = String.Empty;
			int width = 0;

			switch (quadType)
			{
				case QuadrantType.Ground:
					type  = Floor;
					width = FloorWidth;
					break;
				case QuadrantType.West:
					type  = West;
					width = WestWidth;
					break;
				case QuadrantType.North:
					type  = North;
					width = NorthWidth;
					break;
				case QuadrantType.Content:
					type  = Content;
					width = ContentWidth;
					break;
			}

			graphics.DrawString(
							type,
							Font,
							System.Drawing.Brushes.Black,
							StartX + (XCImage.SpriteWidth - width) / 2 + QuadWidthTotal * (int)quadType + 1,
							StartY +  XCImage.SpriteHeight40 + MarginVert);
		}

		/// <summary>
		/// Fills the swatch under a given quadrant.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="quadType"></param>
		private void FillSwatchColor(Graphics graphics, QuadrantType quadType)
		{
			SolidBrush brush = null;
			switch (quadType)
			{
				case QuadrantType.Ground:
					brush = Brushes[TopView.FloorColor];
					break;
				case QuadrantType.West:
					brush = new SolidBrush(Pens[TopView.WestColor].Color);
					break;
				case QuadrantType.North:
					brush = new SolidBrush(Pens[TopView.NorthColor].Color);
					break;
				case QuadrantType.Content:
					brush = Brushes[TopView.ContentColor];
					break;
			}

			graphics.FillRectangle(
								brush,
								new RectangleF(
											StartX + QuadWidthTotal * (int)quadType,
											StartY + XCImage.SpriteHeight40 + MarginVert + Font.Height + 1,
											XCImage.SpriteWidth,
											SwatchHeight));
		}

		/// <summary>
		/// This isn't really necessary since the GraphicsPath's last the
		/// lifetime of the app. But FxCop gets antsy ....
		/// NOTE: Dispose() is never called. cf ColorTools. cf DrawBlobService.
		/// WARNING: This is NOT a robust implementation perhaps. But it
		/// satisifes the core of the matter and could likely be used for
		/// further development if that's ever required.
		/// </summary>
		public void Dispose()
		{
			if (_pathFloor   != null) _pathFloor  .Dispose();
			if (_pathWest    != null) _pathWest   .Dispose();
			if (_pathNorth   != null) _pathNorth  .Dispose();
			if (_pathContent != null) _pathContent.Dispose();

			if (Font  != null) Font .Dispose();
			if (Brush != null) Brush.Dispose();

			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
