using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using MapView.Forms.MainWindow;

using XCom;
using XCom.Interfaces.Base;


namespace MapView.Forms.MapObservers.TopViews
{
	/// <summary>
	/// The derived class for TopViewPanel.
	/// @note This is not a Panel. It is a UserControl inside of a Panel.
	/// </summary>
	internal sealed class TopViewPanel
		:
			TopViewPanelParent
	{
		#region Fields & Properties
		private ColorTools _toolWest;
		private ColorTools _toolNorth;
		private ColorTools _toolContent;

		internal ToolStripMenuItem Ground
		{ get; set; }

		internal ToolStripMenuItem North
		{ get; set; }

		internal ToolStripMenuItem West
		{ get; set; }

		internal ToolStripMenuItem Content
		{ get; set; }

		internal QuadrantPanel QuadrantsPanel
		{ get; set; }
		#endregion


		#region cTor
		/// <summary>
		/// TopViewPanel cTor.
		/// </summary>
		internal TopViewPanel()
		{
			MainViewUnderlay.Instance.MainViewOverlay.MouseDragEvent += PathSelectedLozenge;

			(this as Control).KeyDown += OnKeyDown;
		}
		#endregion


		#region Eventcalls
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control)
			{
				switch (e.KeyCode)
				{
					case Keys.S:
						XCMainWindow.Instance.OnSaveMapClick(null, EventArgs.Empty);
						break;

					case Keys.X:
						MainViewUnderlay.Instance.MainViewOverlay.Copy();
						MainViewUnderlay.Instance.MainViewOverlay.ClearSelection();
						break;

					case Keys.C:
						MainViewUnderlay.Instance.MainViewOverlay.Copy();
						break;

					case Keys.V:
						MainViewUnderlay.Instance.MainViewOverlay.Paste();
						break;
				}
			}
			else
			{
				switch (e.KeyCode)
				{
					case Keys.Delete:
						MainViewUnderlay.Instance.MainViewOverlay.ClearSelection();
						break;
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e); // fire the parent's OnMouseDown() handler also ...

			Select();

			if (e.Button == MouseButtons.Right)
			{
				ViewerFormsManager.TopView     .Control   .QuadrantsPanel.SetSelected(e.Button, 1);
				ViewerFormsManager.TopRouteView.ControlTop.QuadrantsPanel.SetSelected(e.Button, 1);
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e); // fire the DoubleBufferControl's OnPaint() handler also.

			var graphics = e.Graphics;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

			ControlPaint.DrawBorder3D(graphics, ClientRectangle, Border3DStyle.Etched);
		}
		#endregion


		#region Methods
		internal void DrawTileBlobs(
				MapTileBase tile,
				Graphics graphics,
				int x, int y)
		{
			var mapTile = (XCMapTile)tile;

			_toolWest    = _toolWest    ?? new ColorTools(TopPens[TopView.WestColor]);
			_toolNorth   = _toolNorth   ?? new ColorTools(TopPens[TopView.NorthColor]);
			_toolContent = _toolContent ?? new ColorTools(TopBrushes[TopView.ContentColor], _toolNorth.Pen.Width);

			if (Ground.Checked && mapTile.Ground != null)
				BlobService.DrawFloor(
									graphics,
									TopBrushes[TopView.FloorColor],
									x, y);

			if (Content.Checked && mapTile.Content != null)
				BlobService.DrawContent(
									graphics,
									_toolContent,
									x, y,
									mapTile.Content);

			if (West.Checked && mapTile.West != null)
				BlobService.DrawContent(
									graphics,
									_toolWest,
									x, y,
									mapTile.West);

			if (North.Checked && mapTile.North != null)
				BlobService.DrawContent(
									graphics,
									_toolNorth,
									x, y,
									mapTile.North);
		}
		#endregion
	}
}
