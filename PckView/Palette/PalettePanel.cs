using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using XCom;


namespace PckView
{
	internal delegate void PaletteIdChangedEventHandler(int selectedId);


	internal sealed class PalettePanel
		:
			Panel
	{
		#region Events
		internal event PaletteIdChangedEventHandler PaletteIdChangedEvent;
		#endregion


		#region Fields (static)
		internal const int SwatchesPerSide = 16; // 16 swatches across the panel.
		#endregion


		#region Fields
		private int _swatchWidth;
		private int _swatchHeight;
		#endregion


		#region Properties (static)
		internal static PalettePanel Instance
		{ get; private set; }
		#endregion


		#region Properties
		private int _id = -1;
		internal int PaletteId
		{
			get { return _id; }
			set { _id = value; }
		}

		private int _clickX = -1;
		private int ClickX
		{
			get { return _clickX; }
			set { _clickX = value; }
		}
		private int _clickY = -1;
		private int ClickY
		{
			get { return _clickY; }
			set { _clickY = value; }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal PalettePanel()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer
				   | ControlStyles.AllPaintingInWmPaint
				   | ControlStyles.UserPaint
				   | ControlStyles.ResizeRedraw, true);

			PckViewForm.PaletteChangedEvent += OnPaletteChanged; // NOTE: lives the life of the app, so no leak.

			Instance = this;
		}
		#endregion


		#region Eventcalls (override)
		protected override void OnResize(EventArgs eventargs)
		{
			_swatchWidth  = Width  / SwatchesPerSide;
			_swatchHeight = Height / SwatchesPerSide;

			if (PaletteId != -1)
			{
				ClickX = PaletteId % SwatchesPerSide * _swatchWidth  + 1;
				ClickY = PaletteId / SwatchesPerSide * _swatchHeight + 1;
			}
			Refresh();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			int swatchX = e.X / _swatchWidth;
			int swatchY = e.Y / _swatchHeight;

			ClickX = swatchX * _swatchWidth  + 1;
			ClickY = swatchY * _swatchHeight + 1;

			PaletteId = swatchY * SwatchesPerSide + swatchX;

			PrintStatusPaletteId();
		}

		/// <summary>
		/// Draws the palette viewer.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
//			base.OnPaint(e);

			if (!DesignMode) // otherwise PaletteForm has probls drawing a PalettePanel in the designer.
			{
				var graphics = e.Graphics;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				for (int
						i = 0,
							y = 0;
						i != SwatchesPerSide;
						++i,
							y += _swatchHeight)
				{
					for (int
							j = 0,
								x = 0;
							j != SwatchesPerSide;
							++j,
								x += _swatchWidth)
					{
						graphics.FillRectangle(
											new SolidBrush(PckViewForm.Pal[i * SwatchesPerSide + j]),
											x,
											y,
											_swatchWidth,
											_swatchHeight);
					}
				}

				if (PaletteId != -1)
				{
					graphics.DrawRectangle(
										Pens.Red,
										ClickX,
										ClickY,
										_swatchWidth  - 1,
										_swatchHeight - 1);
					graphics.FillRectangle( // -> fill the darn hole that .NET leaves in the top-left corner.
										Brushes.Red,
										ClickX - 1,
										ClickY - 1,
										1,
										1);
				}
			}
		}
		#endregion


		#region Eventcalls
		private void OnPaletteChanged()
		{
			Refresh();
		}
		#endregion


		#region Methods
		internal void PrintStatusPaletteId()
		{
			if (PaletteId > -1 && PaletteId < 256
				&& PaletteIdChangedEvent != null)
			{
				PaletteIdChangedEvent(PaletteId);
				Refresh();
			}
		}

		/// <summary>
		/// Forces selection of a specific palette-id.
		/// </summary>
		/// <param name="palId">the palette id</param>
		internal void SelectPaletteId(int palId)
		{
			PaletteId = palId;

			ClickX = palId % SwatchesPerSide * _swatchWidth  + 1;
			ClickY = palId / SwatchesPerSide * _swatchHeight + 1;

			PrintStatusPaletteId();
		}
		#endregion
	}
}
