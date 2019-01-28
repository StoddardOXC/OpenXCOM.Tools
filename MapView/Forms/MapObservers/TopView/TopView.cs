using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

//using Microsoft.Win32;

using MapView.Forms.MainWindow;

using XCom;


namespace MapView.Forms.MapObservers.TopViews
{
	internal sealed partial class TopView
		:
			MapObserverControl0
	{
		#region Fields (static)
		private const string Floor   = "Floor";
		private const string West    = "West";
		private const string North   = "North";
		private const string Content = "Content";
		#endregion


		#region Fields
		private Dictionary<string, Pen> _topPens;
		private Dictionary<string, SolidBrush> _topBrushes;

//		private readonly Dictionary<ToolStripMenuItem, int> _visQuadsDictionary = new Dictionary<ToolStripMenuItem, int>();
		#endregion


		#region Properties
		private readonly TopViewPanel _topViewPanel;
		internal TopViewPanel TopViewPanel
		{
			get { return _topViewPanel; }
		}

		internal QuadrantPanel QuadrantsPanel
		{
			get { return quadrants; }
		}

		public bool GroundVisible
		{
			get { return _topViewPanel.Ground.Checked; }
		}

		public bool NorthVisible
		{
			get { return _topViewPanel.North.Checked; }
		}

		public bool WestVisible
		{
			get { return _topViewPanel.West.Checked; }
		}

		public bool ContentVisible
		{
			get { return _topViewPanel.Content.Checked; }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor. Instantiates the TopView viewer and its components/controls.
		/// </summary>
		internal TopView()
		{
			InitializeComponent();

			SuspendLayout();

			quadrants.SelectedQuadrant = QuadrantType.Ground;


			_topViewPanel = new TopViewPanel();

			_topViewPanel.Dock = DockStyle.Fill;
//			_topViewPanel.Width  = 100;//pMain.Width;
//			_topViewPanel.Height = 100;//pMain.Height;

//			pMain.AutoScroll = true;
			pMain.Controls.Add(_topViewPanel);

			pMain.Resize += (sender, e) => _topViewPanel.ResizeObserver(pMain.Width, pMain.Height);

			var visQuads = tsddbVisibleQuads.DropDown.Items;

			_topViewPanel.Ground  = new ToolStripMenuItem(Floor);
			_topViewPanel.West    = new ToolStripMenuItem(West);
			_topViewPanel.North   = new ToolStripMenuItem(North);
			_topViewPanel.Content = new ToolStripMenuItem(Content);

			visQuads.Add(_topViewPanel.Ground);
			visQuads.Add(_topViewPanel.West);
			visQuads.Add(_topViewPanel.North);
			visQuads.Add(_topViewPanel.Content);

			_topViewPanel.Ground.ShortcutKeys = Keys.F1;
			_topViewPanel.Ground.Checked = true;
//			_visQuadsDictionary[_topViewPanel.Ground] = 0;

			_topViewPanel.West.ShortcutKeys = Keys.F2;
			_topViewPanel.West.Checked = true;
//			_visQuadsDictionary[_topViewPanel.West] = 1;

			_topViewPanel.North.ShortcutKeys = Keys.F3;
			_topViewPanel.North.Checked = true;
//			_visQuadsDictionary[_topViewPanel.North] = 2;

			_topViewPanel.Content.ShortcutKeys = Keys.F4;
			_topViewPanel.Content.Checked = true;
//			_visQuadsDictionary[_topViewPanel.Content] = 3;

			foreach (ToolStripMenuItem it in visQuads)
				it.Click += OnToggleQuadrantVisibilityClick;

			_topViewPanel.QuadrantsPanel = QuadrantsPanel;

			Panels.Add("QuadrantsPanel", QuadrantsPanel);
			Panels.Add("TopViewPanel", _topViewPanel);

			ResumeLayout();
		}
		#endregion


		#region EventCalls
		/// <summary>
		/// Handles a click on any of the quadrant-visibility menuitems.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToggleQuadrantVisibilityClick(object sender, EventArgs e)
		{
			var it = sender as ToolStripMenuItem;
			if (it == _topViewPanel.Ground)
			{
				ViewerFormsManager.TopView     .Control   ._topViewPanel.Ground.Checked =
				ViewerFormsManager.TopRouteView.ControlTop._topViewPanel.Ground.Checked = !it.Checked;
			}
			else if (it == _topViewPanel.West)
			{
				ViewerFormsManager.TopView     .Control   ._topViewPanel.West.Checked =
				ViewerFormsManager.TopRouteView.ControlTop._topViewPanel.West.Checked = !it.Checked;
			}
			else if (it == _topViewPanel.North)
			{
				ViewerFormsManager.TopView     .Control   ._topViewPanel.North.Checked =
				ViewerFormsManager.TopRouteView.ControlTop._topViewPanel.North.Checked = !it.Checked;
			}
			else //if (it == _topViewPanel.Content)
			{
				ViewerFormsManager.TopView     .Control   ._topViewPanel.Content.Checked =
				ViewerFormsManager.TopRouteView.ControlTop._topViewPanel.Content.Checked = !it.Checked;
			}

			if (it.Text == Floor)
				((MapFileChild)MapBase).CalculateOccultations(!it.Checked);

			MainViewUnderlay.Instance.Refresh();

			ViewerFormsManager.TopView     .Control   .Refresh();
			ViewerFormsManager.TopRouteView.ControlTop.Refresh();
		}


		private static Form _foptions; // static to be used by both TopViewOptions
		private static bool _closing;  // and TopRouteView(Top)Options

		/// <summary>
		/// Handles a click on the Options menuitem.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOptionsClick(object sender, EventArgs e)
		{
			var it = sender as ToolStripMenuItem;
			if (!it.Checked)
			{
				ViewerFormsManager.TopView     .Control   .tsmiOptions.Checked =
				ViewerFormsManager.TopRouteView.ControlTop.tsmiOptions.Checked = true;

				_foptions = new OptionsForm("TopViewOptions", Options);
				_foptions.Text = "TopView Options";

				_foptions.Show();

				_foptions.FormClosing += (sender1, e1) => // a note describing why this is here could be helpful ...
										{
											if (!_closing)
												OnOptionsClick(sender, e);

											_closing = false;
										};
			}
			else
			{
				ViewerFormsManager.TopView     .Control   .tsmiOptions.Checked =
				ViewerFormsManager.TopRouteView.ControlTop.tsmiOptions.Checked = false;

				_closing = true;
				_foptions.Close();
			}
		}
		#endregion


		#region Methods
		/// <summary>
		/// Creates the tool-objects in the toolstrip.
		/// </summary>
		/// <param name="tools"></param>
		internal void InitializeToolstrip(ToolstripFactory tools)
		{
			tools.CreateToolstripEditorObjects(tsTools, true);
		}

		/// <summary>
		/// Selects a quadrant in the QuadrantsPanel given a selected tiletype.
		/// </summary>
		/// <param name="parttype"></param>
		internal void SelectQuadrant(PartType parttype)
		{
			switch (parttype)
			{
				case PartType.Ground:
					QuadrantsPanel.SelectedQuadrant = QuadrantType.Ground;
					break;

				case PartType.WestWall:
					QuadrantsPanel.SelectedQuadrant = QuadrantType.West;
					break;

				case PartType.NorthWall:
					QuadrantsPanel.SelectedQuadrant = QuadrantType.North;
					break;

				case PartType.Object:
					QuadrantsPanel.SelectedQuadrant = QuadrantType.Content;
					break;
			}
		}
		#endregion


		#region Options
		// headers
		private const string Tile     = "Tile";
		private const string Selector = "Selector";
		private const string Grid     = "Grid";

		// options
		internal const string FloorColor        = "FloorColor";
		internal const string WestColor         = "WestColor";
		internal const string NorthColor        = "NorthColor";
		internal const string ContentColor      = "ContentColor";

		private  const string WestWidth         = "WestWidth";
		private  const string NorthWidth        = "NorthWidth";

		internal const string SelectorColor     = "SelectorColor";
		internal const string SelectorWidth     = "SelectorWidth";

		internal const string SelectedColor     = "SelectedColor";
		private  const string SelectedWidth     = "SelectedWidth";

		private  const string SelectedTypeColor = "SelectedTypeColor";

		internal const string GridColor         = "GridColor";
		private  const string GridWidth         = "GridWidth";
		internal const string Grid10Color       = "Grid10Color";
		private  const string Grid10Width       = "Grid10Width";

//		internal const string TileMinHeight     = "TileMinHeight";


		/// <summary>
		/// Loads default options for TopView in TopRouteView screens.
		/// </summary>
		protected internal override void LoadControl0Options()
		{
			_topBrushes = new Dictionary<string, SolidBrush>();
			_topPens    = new Dictionary<string, Pen>();

			_topBrushes.Add(FloorColor, new SolidBrush(Color.BurlyWood));
			_topBrushes.Add(ContentColor, new SolidBrush(Color.MediumSeaGreen));
			_topBrushes.Add(SelectedTypeColor, QuadrantsPanel.SelectColor);

			var penWest = new Pen(Color.Khaki, 4);
			_topPens.Add(WestColor, penWest);
			_topPens.Add(WestWidth, penWest);

			var penNorth = new Pen(Color.Wheat, 4);
			_topPens.Add(NorthColor, penNorth);
			_topPens.Add(NorthWidth, penNorth);

			var penOver = new Pen(Color.Black, 2);
			_topPens.Add(SelectorColor, penOver);
			_topPens.Add(SelectorWidth, penOver);

			var penSelected = new Pen(Color.RoyalBlue, 2);
			_topPens.Add(SelectedColor, penSelected);
			_topPens.Add(SelectedWidth, penSelected);

			var penGrid = new Pen(Color.Black, 1);
			_topPens.Add(GridColor, penGrid);
			_topPens.Add(GridWidth, penGrid);

			var pen10Grid = new Pen(Color.Black, 2);
			_topPens.Add(Grid10Color, pen10Grid);
			_topPens.Add(Grid10Width, pen10Grid);

			OptionChangedEventHandler bc = OnBrushChanged;
			OptionChangedEventHandler pc = OnPenColorChanged;
			OptionChangedEventHandler pw = OnPenWidthChanged;
//			ValueChangedEventHandler  dh = OnDiamondHeight;

			Options.AddOption(FloorColor,        Color.BurlyWood,                 "Color of the floor tile indicator",           Tile,     bc);
			Options.AddOption(WestColor,         Color.Khaki,                     "Color of the west tile indicator",            Tile,     pc);
			Options.AddOption(NorthColor,        Color.Wheat,                     "Color of the north tile indicator",           Tile,     pc);
			Options.AddOption(ContentColor,      Color.MediumSeaGreen,            "Color of the content tile indicator",         Tile,     bc);
			Options.AddOption(WestWidth,         3,                               "Width of the west tile indicator in pixels",  Tile,     pw);
			Options.AddOption(NorthWidth,        3,                               "Width of the north tile indicator in pixels", Tile,     pw);

			Options.AddOption(SelectorColor,     Color.Black,                     "Color of the mouse-over indicator",           Selector, pc);
			Options.AddOption(SelectorWidth,     2,                               "Width of the mouse-over indicator in pixels", Selector, pw);
			Options.AddOption(SelectedColor,     Color.RoyalBlue,                 "Color of the selection line",                 Selector, pc);
			Options.AddOption(SelectedWidth,     2,                               "Width of the selection line in pixels",       Selector, pw);
			Options.AddOption(SelectedTypeColor, Color.LightBlue,                 "Background color of the selected tiletype",   Selector, bc);

			Options.AddOption(GridColor,         Color.Black,                     "Color of the grid lines",                     Grid,     pc);
			Options.AddOption(GridWidth,         1,                               "Width of the grid lines in pixels",           Grid,     pw);
			Options.AddOption(Grid10Color,       Color.Black,                     "Color of every tenth grid line",              Grid,     pc);
			Options.AddOption(Grid10Width,       2,                               "Width of every tenth grid line in pixels",    Grid,     pw);
//			Options.AddOption(TileMinHeight,     _topViewPanel.TileLozengeHeight, "Minimum height of the grid tiles in pixels",  Grid,     dh);

			QuadrantsPanel.Pens   =
			_topViewPanel.TopPens = _topPens;

			QuadrantsPanel.Brushes   =
			_topViewPanel.TopBrushes = _topBrushes;

			Invalidate();
		}

		/// <summary>
		/// Fires when a brush-color changes in Options.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		private void OnBrushChanged(string key, object val)
		{
			_topBrushes[key].Color = (Color)val;

			if (key == SelectedTypeColor)
				QuadrantsPanel.SelectColor = _topBrushes[key];

			Refresh();
		}

		/// <summary>
		/// Fires when a pen-color changes in Options.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		private void OnPenColorChanged(string key, object val)
		{
			_topPens[key].Color = (Color)val;
			Refresh();
		}

		/// <summary>
		/// Fires when a pen-width changes in Options.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="val"></param>
		private void OnPenWidthChanged(string key, object val)
		{
			_topPens[key].Width = (int)val;
			Refresh();
		}

		/// <summary>
		/// Gets the brushes/colors for the Floor and Content blobs.
		/// Used by the Help screen.
		/// </summary>
		/// <returns>a hashtable of the brushes</returns>
		internal Dictionary<string, SolidBrush> GetFloorContentBrushes()
		{
			return _topBrushes;
		}

		/// <summary>
		/// Gets the pens/colors for the Westwall and Northwall blobs.
		/// Used by the Help screen.
		/// </summary>
		/// <returns>a hashtable of the brushes</returns>
		internal Dictionary<string, Pen> GetWallPens()
		{
			return _topPens;
		}

//		/// <summary>
//		/// Fires when the minimum diamond-height changes in Options.
//		/// </summary>
//		/// <param name="sender"></param>
//		/// <param name="keyword"></param>
//		/// <param name="val"></param>
//		private void OnDiamondHeight(object sender, string keyword, object val)
//		{
//			_topViewPanel.TileLozengeHeight = (int)val;
//		}
		#endregion
	}
}

/*		/// <summary>
		/// Loads the VisibleQuadrants flags for the MenuItem-toggles.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnExtraRegistrySettingsLoad(DSShared.Windows.RegistryEventArgs e)
		{
			switch (e.Key)
			{
				case "vis0":
					_topViewPanel.Ground.Checked = e.Value;
					break;
				case "vis1":
					_topViewPanel.West.Checked = e.Value;
					break;
				case "vis2":
					_topViewPanel.North.Checked = e.Value;
					break;
				case "vis3":
					_topViewPanel.Content.Checked = e.Value;
					break;
			}

//			QuadrantsPanel.Height = 74;

//			var regkey = e.OpenRegistryKey;
//			foreach (var it in _visQuadsDictionary.Keys)
//				it.Checked = Boolean.Parse((string)regkey.GetValue("vis" + _visQuadsDictionary[it], "true"));
		} */

/*		/// <summary>
		/// Saves the VisibleQuadrants flags for the menu.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnExtraRegistrySettingsSave(DSShared.Windows.RegistryEventArgs e)
		{
//			var regkey = e.OpenRegistryKey;
//
//			foreach (var it in _visQuadsDictionary.Keys)
//				regkey.SetValue("vis" + _visQuadsDictionary[it], it.Checked);
		} */
