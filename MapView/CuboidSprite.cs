using System;
using System.Drawing;

using XCom;


namespace MapView
{
	internal class CuboidSprite
	{
		private readonly SpriteCollection _spriteset;


		#region cTor
		/// <summary>
		/// Constructs a CuboidSprite.
		/// The CuboidSprite is the actual cuboid selector in XCOM resources.
		/// </summary>
		/// <param name="spriteset"></param>
		internal CuboidSprite(SpriteCollection spriteset)
		{
			_spriteset = spriteset;

//			foreach (PckImage sprite in spriteset)
//			{
//				var b     = sprite.Image;
//				var pal   = b.Palette;	// <- this is only a copy.
//				var trans = pal.Entries[0];
//				int red   = trans.R;
//				int green = trans.G;
//				int blue  = trans.B;
//
//				trans = Color.FromArgb(0, red, green, blue);
//
//				b.Palette = pal;		// <- hence, that.
//			}
			// But it turns out to be unnecessary since the currently loaded
			// ufo/tftd palettes have set their id[0] transparent regardless.
		}
		#endregion


		#region Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="halfWidth"></param>
		/// <param name="halfHeight"></param>
		/// <param name="front">true to draw the front sprite, else back</param>
		/// <param name="red">true to draw the red sprite, else blue</param>
		internal void DrawCuboid(
				Graphics graphics,
				int x, int y,
				int halfWidth,
				int halfHeight,
				bool front,
				bool red)
		{
			#if __MonoCS__
			Pen p = new Pen( front ? 
				(red ? Color.Red    : Color.Green):
				(red ? Color.Purple : Color.Blue ), 2);
			graphics.DrawRectangle (p, x, y, halfWidth * 2, halfHeight * 5);
			#else 
			int id = 0;
			if (front)
				id = (red ? 3 : 5);
			else
				id = (red ? 0 : 2);
			graphics.DrawImage(
							_spriteset[id].Image,
							x, y,
							halfWidth  * 2,		// NOTE: the values for width and height
							halfHeight * 5);	// are based on a sprite that's 32x40.
			#endif
		}

		internal void DrawTargeter(
				Graphics graphics,
				int x, int y,
				int halfWidth,
				int halfHeight)
		{
			#if __MonoCS__
			Pen p = new Pen( Color.Yellow, 5 );
			graphics.DrawRectangle (p, x, y, halfWidth * 2, halfHeight * 5);
			#else 
			graphics.DrawImage(
							_spriteset[7].Image, // yellow targeter sprite
							x, y,
							halfWidth  * 2,		// NOTE: the values for width and height
							halfHeight * 5);	// are based on a sprite that's 32x40.
			#endif
		}
		#endregion
	}
}

/*		private enum CursorState
		{
			Standard,
			Aim,
			MindControl,
			Waypoint,
			Throw
		}; */

/*		/// <summary>
		/// Draws the back half of the cuboid.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
//		/// <param name="over"></param>
		/// <param name="red">true to draw the red sprite, else blue</param>
		/// <param name="halfWidth"></param>
		/// <param name="halfHeight"></param>
		internal void DrawCuboidBack(
				Graphics graphics,
				int x, int y,
//				bool over, // always false.
				bool red,
				int halfWidth,
				int halfHeight)
		{
			int id = (red) ? 0
						   : 2;
//			int id = 2;
//			if (topLevel)// && _state != CursorState.Aim)
//			{
////				id = (over) ? 1 : 0;
//				id = 0;
//			}

			var image = _pckPack[id].Image;
			graphics.DrawImage(
							image,
							x, y,
							halfWidth  * 2, // NOTE: the values for width and height are based on a sprite that's 32x40.
							halfHeight * 5);
//							(int)(image.Width  * Globals.Scale),
//							(int)(image.Height * Globals.Scale));
		}

		/// <summary>
		/// Draws the front half of the cuboid.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
//		/// <param name="aniStep"></param>
//		/// <param name="over"></param>
		/// <param name="red">true to draw the red sprite, else blue</param>
		/// <param name="halfWidth"></param>
		/// <param name="halfHeight"></param>
		internal void DrawCuboidFront(
				Graphics graphics,
				int x, int y,
//				int aniStep,
//				bool over, // always false.
				bool red,
				int halfWidth,
				int halfHeight)
		{
			int id = (red) ? 3
						   : 5;

//			int id = 5;
//			if (topLevel)
//			{
//				switch (_state)
//				{
//					case CursorState.Aim:
//						id = 7 + aniStep % 4;
//						break;
//					case CursorState.MindControl:
//						id = 11 + aniStep % 2;
//						break;
//					case CursorState.Throw:
//						id = 15 + aniStep % 2;
//						break;
//					case CursorState.Waypoint:
//						id = 13 + aniStep % 2;
//						break;
//					default:
////						id = (over) ? 4 : 3;
//						id = 3;
//						break;
//				}
//			}

			var image = _pckPack[id].Image;
			graphics.DrawImage(
							image,
							x, y,
							halfWidth  * 2, // NOTE: the values for width and height are based on a sprite that's 32x40.
							halfHeight * 5);
//							(int)(image.Width  * Globals.Scale),
//							(int)(image.Height * Globals.Scale));
		} */

//		_state = CursorState.Standard;
//		private CursorState _state;
//		public CursorState State
//		{
//			get { return _state; }
//			set { _state = value; }
//		}
//		public SpriteCollection PckCursorSprites
//		{
//			get { return _file; }
//		}
