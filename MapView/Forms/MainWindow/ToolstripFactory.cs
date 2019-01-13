using System;
using System.Collections.Generic;
using System.Windows.Forms;

using MapView.Properties;


namespace MapView.Forms.MainWindow
{
	internal sealed class ToolstripFactory
	{
		#region Properties (static)
		internal static ToolstripFactory Instance
		{ get; set; }
		#endregion


		#region Fields
		private readonly MainViewUnderlay _mainViewUnderlay;
		private readonly List<ToolStripButton> _pasters = new List<ToolStripButton>();


		private readonly ToolStripTextBox _tstbSearch = new ToolStripTextBox();	// NOTE: These instantiations of toolstrip-objects that
																				// are classvars are for MainView, while the toolstrip-
		private ToolStripButton _tsbZoomAuto = new ToolStripButton();			// objects for TopView and TopRouteView(Top) are
		private ToolStripButton _tsbZoomOut  = new ToolStripButton();			// instantiated in the functions below.
		private ToolStripButton _tsbZoomIn   = new ToolStripButton();

		private ToolStripButton _tsbDown     = new ToolStripButton();
		private ToolStripButton _tsbUp       = new ToolStripButton();
		private ToolStripButton _tsbCut      = new ToolStripButton();
		private ToolStripButton _tsbCopy     = new ToolStripButton();
		private ToolStripButton _tsbPaste    = new ToolStripButton();
		private ToolStripButton _tsbFill     = new ToolStripButton();
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="panel"></param>
		internal ToolstripFactory(MainViewUnderlay panel)
		{
			Instance = this;
			_mainViewUnderlay = panel;
		}
		#endregion


		#region Methods
		/// <summary>
		/// Adds a textfield for search to the specified toolstrip.
		/// NOTE: Appears only in MainView.
		/// </summary>
		/// <param name="toolStrip"></param>
		internal void CreateToolstripSearchObjects(ToolStrip toolStrip)
		{
			var tsbClearSearch = new ToolStripButton();

			var tsItems = new ToolStripItem[]
			{
				_tstbSearch,
				tsbClearSearch
			};
			toolStrip.Items.AddRange(tsItems);

			// Search textfield
			_tstbSearch.Name             = "tstbSearch";
			_tstbSearch.Text             = "search";
			_tstbSearch.KeyPress        += OnSearchKeyPress;

			// ClearSearch btn
			tsbClearSearch.Name         = "tsbClearSearch";
			tsbClearSearch.DisplayStyle = ToolStripItemDisplayStyle.Image;
			tsbClearSearch.Image        = Resources.DeleteRed;
			tsbClearSearch.ToolTipText  = "reset highlight";
			tsbClearSearch.Click       += OnClearHighlightClick;
		}


		/// <summary>
		/// Adds buttons for zooming the map-scale to the specified toolstrip.
		/// NOTE: Appears only in MainView.
		/// </summary>
		/// <param name="toolStrip"></param>
		internal void CreateToolstripZoomObjects(ToolStrip toolStrip)
		{
			var tsItems = new ToolStripItem[]
			{
				new ToolStripSeparator(),
				_tsbZoomAuto,
				_tsbZoomOut,
				_tsbZoomIn
			};
			toolStrip.Items.AddRange(tsItems);

			// AutoZoom btn
			_tsbZoomAuto.Name         = "tsbZoomAuto";
			_tsbZoomAuto.DisplayStyle = ToolStripItemDisplayStyle.Image;
			_tsbZoomAuto.Image        = Resources.Search1;
			_tsbZoomAuto.Checked      = true;
			_tsbZoomAuto.ToolTipText  = "autoscale";
			_tsbZoomAuto.Click       += XCMainWindow.Instance.OnAutoScaleClick;

			// ZoomOut btn
			_tsbZoomOut.Name          = "tsbZoomOut";
			_tsbZoomOut.DisplayStyle  = ToolStripItemDisplayStyle.Image;
			_tsbZoomOut.Image         = Resources.ZoomOut1;
			_tsbZoomOut.ToolTipText   = "scale Out";
			_tsbZoomOut.Click        += XCMainWindow.Instance.OnZoomOutClick;

			// ZoomIn btn
			_tsbZoomIn.Name           = "tsbZoomIn";
			_tsbZoomIn.DisplayStyle   = ToolStripItemDisplayStyle.Image;
			_tsbZoomIn.Image          = Resources.ZoomIn1;
			_tsbZoomIn.ToolTipText    = "scale In";
			_tsbZoomIn.Click         += XCMainWindow.Instance.OnZoomInClick;
		}


		/// <summary>
		/// Adds buttons for Up,Down,Cut,Copy,Paste and Fill to the specified
		/// toolstrip in MainView as well as TopView and TopRouteView(Top).
		/// </summary>
		/// <param name="toolStrip"></param>
		/// <param name="tertiary">false for MainView's toolstrip, true for TopView's and TopRouteView's</param>
		internal void CreateToolstripEditorObjects(ToolStrip toolStrip, bool tertiary)
		{
			ToolStripButton tsbDown;
			ToolStripButton tsbUp;
			ToolStripButton tsbCut;
			ToolStripButton tsbCopy;
			ToolStripButton tsbPaste;
			ToolStripButton tsbFill;

			if (tertiary)
			{
				tsbDown  = new ToolStripButton(); // NOTE: MainView's toolstrip buttons are classvars because
				tsbUp    = new ToolStripButton(); // they will be disabled when the app loads and will be enabled
				tsbCut   = new ToolStripButton(); // when the user clicks and loads a Map. The tertiary viewers'
				tsbCopy  = new ToolStripButton(); // toolstrip buttons don't need to be disabled because those
				tsbPaste = new ToolStripButton(); // viewers appear to the user after user clicks and loads a
				tsbFill  = new ToolStripButton(); // Map (at which the buttons will already be enabled).
			}
			else
			{
				tsbDown  = _tsbDown;
				tsbUp    = _tsbUp;
				tsbCut   = _tsbCut;
				tsbCopy  = _tsbCopy;
				tsbPaste = _tsbPaste;
				tsbFill  = _tsbFill;
			}

			var tsItems = new ToolStripItem[]
			{
				new ToolStripSeparator(), // NOTE: c# cant figure out how to use 1 separator 4 times.
				tsbDown,
				tsbUp,
				new ToolStripSeparator(),
				tsbCut,
				tsbCopy,
				tsbPaste,
				new ToolStripSeparator(),
				tsbFill,
				new ToolStripSeparator()
			};
			toolStrip.Items.AddRange(tsItems);

			// LevelDown btn
			tsbDown.Name          = "tsbDown";
			tsbDown.DisplayStyle  = ToolStripItemDisplayStyle.Image;
			tsbDown.Image         = Resources.down;
			tsbDown.ToolTipText   = "level down";
			tsbDown.Click        += OnDownClick;

			// LevelUp btn
			tsbUp.Name            = "tsbUp";
			tsbUp.DisplayStyle    = ToolStripItemDisplayStyle.Image;
			tsbUp.Image           = Resources.up;
			tsbUp.ToolTipText     = "level up";
			tsbUp.Click          += OnUpClick;

			// Cut btn
			tsbCut.Name           = "tsbCut";
			tsbCut.DisplayStyle   = ToolStripItemDisplayStyle.Image;
			tsbCut.Image          = Resources.cut;
			tsbCut.ToolTipText    = "cut";
			tsbCut.Click         += _mainViewUnderlay.OnCut;
//			tsbCut.Click         += (sender, e) => // -> example of ... lambda usage
//									{
//										EnablePasteButton();
//										_mainViewUnderlay.OnCut(sender, e);
//									};

			// Copy btn
			tsbCopy.Name          = "tsbCopy";
			tsbCopy.DisplayStyle  = ToolStripItemDisplayStyle.Image;
			tsbCopy.Image         = Resources.copy;
			tsbCopy.ToolTipText   = "copy";
			tsbCopy.Click        += _mainViewUnderlay.OnCopy;
//			tsbCopy.Click        += (sender, e) => // -> example of ... lambda usage
//									{
//										EnablePasteButton();
//										_mainViewUnderlay.OnCopy(sender, e);
//									};

			// Paste btn
			tsbPaste.Name         = "tsbPaste";
			tsbPaste.DisplayStyle = ToolStripItemDisplayStyle.Image;
			tsbPaste.Image        = Resources.paste;
			tsbPaste.ToolTipText  = "paste";
			tsbPaste.Enabled      = false;
			tsbPaste.Click       += _mainViewUnderlay.OnPaste;

			_pasters.Add(tsbPaste);

			// Fill btn
			tsbFill.Name          = "tsbFill";
			tsbFill.DisplayStyle  = ToolStripItemDisplayStyle.Image;
			tsbFill.Image         = Resources.philup;
			tsbFill.ToolTipText   = "fill";
			tsbFill.Click        += _mainViewUnderlay.OnFill;
		}


		/// <summary>
		/// Sets the Autozoom button checked/unchecked.
		/// </summary>
		internal void SetAutozoomChecked(bool check)
		{
			_tsbZoomAuto.Checked = check;
		}

		/// <summary>
		/// Toggles the Autozoom button checked/unchecked.
		/// </summary>
		/// <returns>true if checked</returns>
		internal bool ToggleAutozoomChecked()
		{
			return (_tsbZoomAuto.Checked = !_tsbZoomAuto.Checked);
		}

		/// <summary>
		/// Enables the paste-button in any viewer after cut or copy is
		/// clicked or Ctrl+X / Ctrl+C is pressed on the keyboard.
		/// </summary>
		internal void EnablePasteButton()
		{
			foreach (var tsb in _pasters)
				tsb.Enabled = true;
		}

		/// <summary>
		/// Enables or disables toolstrip objects. This disables all of MainView's
		/// toolstrip (except search) when Mapview loads. These buttons are
		/// subsequently enabled when a map is loaded.
		/// NOTE: Subsidiary viewers don't need to bother with this - they do
		/// not even show until a Map is loaded.
		/// </summary>
		/// <param name="enable"></param>
		internal void EnableToolStrip(bool enable)
		{
			_tsbZoomAuto.Enabled =
			_tsbZoomOut .Enabled =
			_tsbZoomIn  .Enabled =
			_tsbDown    .Enabled =
			_tsbUp      .Enabled =
			_tsbCut     .Enabled =
			_tsbCopy    .Enabled =
//			_tsbPaste   .Enabled = // do not enable Paste until a Cut or Copy has occured
			_tsbFill    .Enabled = enable;
		}
		#endregion


		#region Eventcalls (editstrip)
		private void OnDownClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MainViewOverlay.MapBase != null)
				_mainViewUnderlay.MainViewOverlay.MapBase.LevelDown();
		}

		private void OnUpClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MainViewOverlay.MapBase != null)
				_mainViewUnderlay.MainViewOverlay.MapBase.LevelUp();
		}
		#endregion


		#region Eventcalls (searchstrip)
		/// <summary>
		/// Handler for pressing the Enter-key when the search-textbox is focused.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSearchKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				XCMainWindow.Instance.Search(_tstbSearch.Text);
				e.Handled = true;
			}
		}

		/// <summary>
		/// Clears the searched, found, and highlighted Treenode.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		internal void OnClearHighlightClick(object sender, EventArgs e)
		{
			XCMainWindow.Instance.ClearSearched();
			_tstbSearch.Focus();
		}
		#endregion
	}
}
