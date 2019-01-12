using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using DSShared;
using DSShared.Windows;

using MapView.Forms.MainWindow;
//using MapView.Forms.XCError.WarningConsole;
//using MapView.OptionsServices;

using XCom;
using XCom.Resources.Map.RouteData;
using XCom.Interfaces.Base;

using YamlDotNet.RepresentationModel; // read values (deserialization)


namespace MapView
{
	/// <summary>
	/// Instantiates a MainView screen as the basis for all user-interaction.
	/// </summary>
	internal sealed partial class XCMainWindow
		:
			Form
	{
		#region Fields (static)
		private const double ScaleDelta = 0.125;
		#endregion


		#region Fields
		private readonly MainViewUnderlay      _mainViewUnderlay;

		private readonly ViewersManager        _viewersManager;
		private readonly MainMenusManager      _mainMenusManager;

//		private readonly LoadingForm           _loadingProgress;
//		private readonly ConsoleWarningHandler _warningHandler;

		private Form _foptions;
		private bool _closing;

		private bool _quit;

		private bool _bypassActivatedEvent;
		private bool _allowBringToFront;
		#endregion


		#region Properties (static)
		internal static XCMainWindow Instance
		{ get; set; }
		#endregion


		#region Properties
		private readonly OptionsManager _optionsManager;
		private Options Options
		{
			get { return _optionsManager["MainWindow"]; }
			set { _optionsManager["MainWindow"] = value; }
		}

		private List<string> _tilesetTerrains = new List<string>();
		internal List<string> TilesetTerrains
		{
			get { return _tilesetTerrains; }
			set { _tilesetTerrains = value; }
		}

		internal bool MaptreeChanged
		{ private get; set; }

		/// <summary>
		/// The currently searched and found and highlighted Treenode on the MapTree.
		/// </summary>
		internal TreeNode Searched
		{ private get; set; }

		private Color _colorSearch;
		#endregion


		#region cTor
		/// <summary>
		/// This is where the user-app end of things *really* starts.
		/// </summary>
		internal XCMainWindow()
		{
			Globals.RT = true; // is RunTime (ie. not DesignMode)

			string dirApplication = Path.GetDirectoryName(Application.ExecutablePath);
			string dirSettings    = Path.Combine(dirApplication, PathInfo.SettingsDirectory);
#if DEBUG
			LogFile.SetLogFilePath(dirApplication); // creates a logfile/ wipes the old one.
#endif

			LogFile.WriteLine("Starting MAIN MapView window ...");


			// TODO: further optimize this loading sequence ....

			var share = SharedSpace.Instance;

			share.SetShare(
						SharedSpace.ApplicationDirectory,
						dirApplication);
			share.SetShare(
						SharedSpace.SettingsDirectory,
						dirSettings);

			LogFile.WriteLine("App paths cached.");


			var pathOptions = new PathInfo(dirSettings, PathInfo.ConfigOptions);
			share.SetShare(PathInfo.ShareOptions, pathOptions);

			var pathResources = new PathInfo(dirSettings, PathInfo.ConfigResources);
			share.SetShare(PathInfo.ShareResources, pathResources);

			var pathTilesets = new PathInfo(dirSettings, PathInfo.ConfigTilesets);
			share.SetShare(PathInfo.ShareTilesets, pathTilesets);

			var pathViewers = new PathInfo(dirSettings, PathInfo.ConfigViewers);
			share.SetShare(PathInfo.ShareViewers, pathViewers);

			LogFile.WriteLine("PathInfo cached.");


			// Check if MapTilesets.yml and MapResources.yml exist yet, show the
			// Configuration window if not.
			// NOTE: MapResources.yml and MapTilesets.yml are created by ConfigurationForm
			if (!pathResources.FileExists() || !pathTilesets.FileExists())
			{
				LogFile.WriteLine("Resources or Tilesets file does not exist: run configurator.");

				using (var f = new ConfigurationForm())
					f.ShowDialog(this);
			}
			else
				LogFile.WriteLine("Resources and Tilesets files exist.");


			// Exit app if either MapResources.yml or MapTilesets.yml doesn't exist
			if (!pathResources.FileExists() || !pathTilesets.FileExists()) // safety. The Configurator shall demand that both these files get created.
			{
				LogFile.WriteLine("Resources or Tilesets file does not exist: quit MapView.");
				Environment.Exit(0);
			}



			// Check if MapViewers.yml exists yet, if not create it
			if (!pathViewers.FileExists())
			{
				CreateViewersFile();
				LogFile.WriteLine("Viewers file created.");
			}
			else
				LogFile.WriteLine("Viewers file exists.");



			InitializeComponent();
			LogFile.WriteLine("MainView initialized.");

			// WORKAROUND: The size of the form in the designer keeps increasing
			// (for whatever reason) based on the
			// 'SystemInformation.CaptionButtonSize.Height' value (the titlebar
			// height - note that 'SystemInformation.CaptionHeight' seems to be
			// 1 pixel larger, which (for whatever reason) is not the
			// 'SystemInformation.BorderSize.Height'). To prevent all that, in
			// the designer cap the form's Size by setting its MaximumSize to
			// the Size - but now run this code that sets the MaximumSize to
			// "0,0" (unlimited, as wanted). And, as a safety, do the same thing
			// with MinimumSize ... if desired.
			//
			// - observed & tested in SharpDevelop 5.1
			//
			// NOTE: This code (the constructor of a Form) shouldn't run when
			// opening the designer; it appears to run only when actually
			// running the application:

			MaximumSize =
			MinimumSize = new Size(0,0); // fu.net


//			tvMaps.NodeMouseClick += (sender, args) => tvMaps.SelectedNode = args.Node;

			// jijack: These two events keep getting deleted in my designer:
			tvMaps.BeforeSelect += OnMapTreeBeforeSelect;
			tvMaps.AfterSelect  += OnMapTreeAfterSelected;
			// welcome to your new home


			Instance = this;

			FormClosing += OnSaveOptionsFormClosing;


			_optionsManager = new OptionsManager(); // goes before LoadOptions()

			Options = new Options();

			LoadOptions();									// TODO: check if this should go after the managers load
			LogFile.WriteLine("MainView Options loaded.");	// since managers might be re-instantiating needlessly
															// when OnOptionsClick() runs ....

			_mainViewUnderlay = MainViewUnderlay.Instance; // create MainViewUnderlay and MainViewOverlay. or so ...
			_mainViewUnderlay.Dock = DockStyle.Fill;
			_mainViewUnderlay.BorderStyle = BorderStyle.Fixed3D;
			LogFile.WriteLine("MainView panel instantiated.");


			Palette.UfoBattle .SetTransparent(true);
			Palette.TftdBattle.SetTransparent(true);
			Palette.UfoBattle .Grayscale.SetTransparent(true);
			Palette.TftdBattle.Grayscale.SetTransparent(true);
			LogFile.WriteLine("Palette transparencies set.");

			Globals.LoadExtraSprites();	// sprites for TileView's eraser and QuadrantPanel's blank quads.
										// NOTE: transparency of the 'UfoBattle' palette must be set first.


//			var shareConsole = new ConsoleSharedSpace(share);
//			_warningHandler  = new ConsoleWarningHandler(consoleShare);


			_viewersManager = new ViewersManager(_optionsManager/*, shareConsole*/);
			LogFile.WriteLine("Viewer managers instantiated.");

			_mainMenusManager = new MainMenusManager(menuViewers, menuHelp);
			_mainMenusManager.PopulateMenus(/*shareConsole.Console,*/ Options);
			LogFile.WriteLine("MainView menus populated.");


			ViewerFormsManager.HideViewersManager = _mainMenusManager.CreateShowHideManager(); // subsidiary viewers hide when PckView is invoked from TileView.
			LogFile.WriteLine("HideViewersManager created.");


			ViewerFormsManager.ToolFactory = new ToolstripFactory(_mainViewUnderlay);
			ViewerFormsManager.Initialize();
			LogFile.WriteLine("ViewerFormsManager initialized.");


			_viewersManager.ManageViewers();


			ViewerFormsManager.TileView.Control.PckSavedEvent += OnPckSavedEvent;

			MainViewUnderlay.AnimationUpdateEvent += OnAnimationUpdate;	// FIX: "Subscription to static events without unsubscription may cause memory leaks."
																		// NOTE: it's not really a problem, since both publisher and subscriber are expected to
																		// live the lifetime of the app. And this class, XCMainWindow, never re-instantiates.
			tvMaps.TreeViewNodeSorter = StringComparer.OrdinalIgnoreCase;

			tscPanel.ContentPanel.Controls.Add(_mainViewUnderlay);

			ViewerFormsManager.ToolFactory.CreateToolstripSearchObjects(tsTools);
			ViewerFormsManager.ToolFactory.CreateToolstripZoomObjects(tsTools);
			ViewerFormsManager.ToolFactory.CreateToolstripEditorObjects(tsTools, false);
			ViewerFormsManager.ToolFactory.EnableToolStrip(false);


			// Read MapResources.yml to get the resources dir (for both UFO and TFTD).
			// NOTE: MapResources.yml is created by ConfigurationForm
			using (var sr = new StreamReader(File.OpenRead(pathResources.Fullpath)))
			{
				var str = new YamlStream();
				str.Load(sr);

				string key = null;
				string val = null;

				var nodeRoot = str.Documents[0].RootNode as YamlMappingNode;
				foreach (var node in nodeRoot.Children)
				{
					switch (node.Key.ToString())
					{
						case "ufo":
							key = SharedSpace.ResourceDirectoryUfo;
							break;
						case "tftd":
							key = SharedSpace.ResourceDirectoryTftd;
							break;
					}

					val = node.Value.ToString();
					val = (!val.Equals(PathInfo.NotConfigured)) ? val
																: null;

					share.SetShare(key, val);
				}
			}

			// Setup an XCOM cursor-sprite.
			// NOTE: This is the only stock XCOM resource that is required for
			// MapView to start. See ConfigurationForm ...
			// TODO: give user the option to choose which cursor-spriteset to use.
			var cuboid = ResourceInfo.LoadSpriteset(
												SharedSpace.CursorFilePrefix,
												share.GetShare(SharedSpace.ResourceDirectoryUfo),
												2,
												Palette.UfoBattle);
			if (cuboid != null)
			{
				_mainViewUnderlay.MainViewOverlay.Cuboid = new CuboidSprite(cuboid);
				LogFile.WriteLine("UFO Cursor loaded.");
			}
			else
				LogFile.WriteLine("UFO Cursor not found.");

			// NOTE: The TFTD cursorsprite takes precedence over the UFO cursorsprite.
			cuboid = ResourceInfo.LoadSpriteset(
											SharedSpace.CursorFilePrefix,
											share.GetShare(SharedSpace.ResourceDirectoryTftd),
											4,
											Palette.TftdBattle);
			if (cuboid != null)
			{
				_mainViewUnderlay.MainViewOverlay.Cuboid = new CuboidSprite(cuboid);
				LogFile.WriteLine("TFTD Cursor loaded.");
			}
			else
				LogFile.WriteLine("TFTD Cursor not found.");


			ResourceInfo.InitializeResources(pathTilesets); // load resources from YAML.
			LogFile.WriteLine("ResourceInfo initialized.");


			CreateTree();
			LogFile.WriteLine("Tilesets created and loaded to tree panel.");

			ShiftSplitter();


			if (pathOptions.FileExists())
			{
				_optionsManager.LoadOptions(pathOptions.Fullpath);
				LogFile.WriteLine("User options loaded.");
			}
			else
				LogFile.WriteLine("User options NOT loaded - no options file to load.");


			// I should rewrite the hq2x wrapper for .NET sometime -- not the code it's pretty insane
//			if (!File.Exists("hq2xa.dll")) miHq.Visible = false;

//			LogFile.WriteLine("Loading user-made plugins");

			/****************************************/
			// Copied from PckView
//			loadedTypes = new LoadOfType<DescriptorBase>();
//			sharedSpace["MapMods"] = loadedTypes.AllLoaded;

			// There are no currently loadable maps in this assembly so this is more for future use
//			loadedTypes.LoadFrom(Assembly.GetAssembly(typeof(XCom.Interfaces.Base.DescriptorBase)));

//			if (Directory.Exists(sharedSpace[SharedSpace.CustomDirectory].ToString()))
//			{
//				xConsole.AddLine("Custom directory exists: " + sharedSpace[SharedSpace.CustomDirectory].ToString());
//				foreach (string s in Directory.GetFiles(sharedSpace[SharedSpace.CustomDirectory].ToString()))
//					if (s.EndsWith(".dll"))
//					{
//						xConsole.AddLine("Loading dll: " + s);
//						loadedTypes.LoadFrom(Assembly.LoadFrom(s));
//					}
//			}
			/****************************************/


			LogFile.WriteLine("About to show MainView ...");
			Show();
		}
		#endregion


		/// <summary>
		/// Creates the Map-tree on the left side of MainView.
		/// </summary>
		private void CreateTree()
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("XCMainWindow.CreateTree");

			tvMaps.Nodes.Clear();

			var groups = ResourceInfo.TileGroupInfo.TileGroups;
			//LogFile.WriteLine(". groups= " + groups);

			SortableTreeNode nodeGroup;
			TileGroupBase tileGroup;

			SortableTreeNode nodeCategory;

			SortableTreeNode nodeTileset;
			Dictionary<string, Descriptor> descriptors;


			foreach (string keyGroup in groups.Keys)
			{
				//LogFile.WriteLine(". . keyGroup= " + keyGroup);

				tileGroup = groups[keyGroup];

				nodeGroup = new SortableTreeNode(tileGroup.Label);
				nodeGroup.Tag = tileGroup;
				tvMaps.Nodes.Add(nodeGroup);

				foreach (string keyCategory in tileGroup.Categories.Keys)
				{
					//LogFile.WriteLine(". . . keyCategory= " + keyCategory);

					nodeCategory = new SortableTreeNode(keyCategory);
					descriptors = tileGroup.Categories[keyCategory];
					nodeCategory.Tag = descriptors;
					nodeGroup.Nodes.Add(nodeCategory);

					foreach (string keyTileset in descriptors.Keys)
					{
						//LogFile.WriteLine(". . . . keyTileset= " + keyTileset);

						nodeTileset = new SortableTreeNode(keyTileset);
						nodeTileset.Tag = descriptors[keyTileset];
						nodeCategory.Nodes.Add(nodeTileset);
					}
				}
			}
		}

		/// <summary>
		/// A functor that sorts tree-nodes.
		/// </summary>
		private sealed class SortableTreeNode
			:
				TreeNode,
				IComparable
		{
			public SortableTreeNode(string text)
				:
					base(text)
			{}

			public int CompareTo(object other)
			{
				var node = other as SortableTreeNode;
				return (node != null) ? String.CompareOrdinal(Text, node.Text)
									  : -1;
			}
		}

		/// <summary>
		/// Shifts the splitter between the MapTree and the MapPanel to ensure
		/// that the longest tree-node's Text gets fully displayed.
		/// </summary>
		private void ShiftSplitter()
		{
			int width = tvMaps.Width, widthTest;

			foreach (TreeNode node0 in tvMaps.Nodes)
			{
				widthTest = TextRenderer.MeasureText(node0.Text, tvMaps.Font).Width + 18;
				if (widthTest > width)
					width = widthTest;

				foreach (TreeNode node1 in node0.Nodes)
				{
					widthTest = TextRenderer.MeasureText(node1.Text, tvMaps.Font).Width + 36;
					if (widthTest > width)
						width = widthTest;

					foreach (TreeNode node2 in node1.Nodes)
					{
						widthTest = TextRenderer.MeasureText(node2.Text, tvMaps.Font).Width + 54;
						if (widthTest > width)
							width = widthTest;
					}
				}
			}
			tvMaps.Width = width;
		}


		#region Options
		// headers
		private const string Global  = "Global";
		private const string MapView = "MapView";
		private const string Sprites = "Sprites";

		// options
		private const string Animation           = "Animation";
		private const string Doors               = "Doors";
		private const string SaveWindowPositions = "SaveWindowPositions"; // TODO: is not implemented; implement it or remove it.
		private const string AllowBringToFront   = "AllowBringToFront";
//		private const string SaveOnExit          = "SaveOnExit";

		private const string ShowGrid            = "ShowGrid";
		private const string GridLayerColor      = "GridLayerColor";
		private const string GridLayerOpacity    = "GridLayerOpacity";
		private const string GridLineColor       = "GridLineColor";
		private const string GridLineWidth       = "GridLineWidth";

		private const string GraySelection       = "GraySelection";

		private const string SpriteShade         = "SpriteShade";
		private const string Interpolation       = "Interpolation";


		/// <summary>
		/// Loads (a) MainView's screen-size and -position from YAML,
		///       (b) settings in MainView's Options screen.
		/// </summary>
		private void LoadOptions()
		{
			string file = Path.Combine(SharedSpace.Instance.GetShare(SharedSpace.SettingsDirectory), PathInfo.ConfigViewers);
			using (var sr = new StreamReader(File.OpenRead(file)))
			{
				var str = new YamlStream();
				str.Load(sr);

				var nodeRoot = str.Documents[0].RootNode as YamlMappingNode;
				foreach (var node in nodeRoot.Children)
				{
					string viewer = ((YamlScalarNode)node.Key).Value;
					if (String.Equals(viewer, RegistryInfo.MainView, StringComparison.Ordinal))
					{
						int x = 0;
						int y = 0;
						int w = 0;
						int h = 0;

						var invariant = System.Globalization.CultureInfo.InvariantCulture;

						var keyvals = nodeRoot.Children[new YamlScalarNode(viewer)] as YamlMappingNode;
						foreach (var keyval in keyvals) // NOTE: There is a better way to do this. See TilesetManager..cTor
						{
							switch (keyval.Key.ToString()) // TODO: Error handling. ->
							{
								case "left":
									x = Int32.Parse(keyval.Value.ToString(), invariant);
									break;
								case "top":
									y = Int32.Parse(keyval.Value.ToString(), invariant);
									break;
								case "width":
									w = Int32.Parse(keyval.Value.ToString(), invariant);
									break;
								case "height":
									h = Int32.Parse(keyval.Value.ToString(), invariant);
									break;
							}
						}

						var rectScreen = Screen.GetWorkingArea(new Point(x, y));
						if (!rectScreen.Contains(x + 200, y + 100)) // check to ensure that MainView is at least partly onscreen.
						{
							x = 100;
							y =  50;
						}

						Left = x;
						Top  = y;

						ClientSize = new Size(w, h);
					}
				}
			}

			// kL_note: This is for retrieving MainViewer size and position from
			// the Windows Registry:
//			using (var keySoftware = Registry.CurrentUser.CreateSubKey(DSShared.Windows.RegistryInfo.SoftwareRegistry))
//			using (var keyMapView = keySoftware.CreateSubKey(DSShared.Windows.RegistryInfo.MapViewRegistry))
//			using (var keyMainView = keyMapView.CreateSubKey("MainView"))
//			{
//				Left   = (int)keyMainView.GetValue("Left",   Left);
//				Top    = (int)keyMainView.GetValue("Top",    Top);
//				Width  = (int)keyMainView.GetValue("Width",  Width);
//				Height = (int)keyMainView.GetValue("Height", Height);
//				keyMainView.Close();
//				keyMapView.Close();
//				keySoftware.Close();
//			}

			var handler = new OptionChangedEventHandler(OnOptionChange);

			Options.AddOption(
							Animation,
							MainViewUnderlay.IsAnimated,
							"If true the sprites will animate (F1 - On, F2 - Off)",
							Global,
							handler);
			Options.AddOption(
							Doors,
							false,
							"If true the doors will animate if Animation is also on - if"
								+ " Animation is false the doors will show their alternate tile."
								+ " This setting may need to be re-toggled if Animation changes"
								+ " (F3 - On/Off)",
							Global,
							handler);
			Options.AddOption(
							SaveWindowPositions,
							true, //PathsEditor.SaveRegistry,
							"If true the window positions and sizes will be saved (disabled, always true)",
							Global,
							handler);
			Options.AddOption(
							AllowBringToFront,
							false,
							"If true any open subsidiary viewers will be brought to the top of"
								+ " the desktop whenever MainView takes focus - this implements"
								+ " a workaround that might help circumvent an issue in post"
								+ " Windows 7 OS, in which focus refuses to switch to MainView"
								+ " unless the subsidiary viewers are closed (tentative)",
							Global,
							handler);

			Options.AddOption(
							ShowGrid,
							MainViewUnderlay.Instance.MainViewOverlay.ShowGrid,
							"If true a grid will display at the current level of editing (F4 - On/Off)",
							MapView,
							handler);
//							null, MainViewUnderlay.Instance.MainViewOverlay);
			Options.AddOption(
							GridLayerColor,
							MainViewUnderlay.Instance.MainViewOverlay.GridLayerColor,
							"Color of the grid",
							MapView,
							null, MainViewUnderlay.Instance.MainViewOverlay);
			Options.AddOption(
							GridLayerOpacity,
							MainViewUnderlay.Instance.MainViewOverlay.GridLayerOpacity,
							"Opacity of the grid (0..255 default 200)",
							MapView,
							null, MainViewUnderlay.Instance.MainViewOverlay);
			Options.AddOption(
							GridLineColor,
							MainViewUnderlay.Instance.MainViewOverlay.GridLineColor,
							"Color of the lines that make up the grid",
							MapView,
							null, MainViewUnderlay.Instance.MainViewOverlay);
			Options.AddOption(
							GridLineWidth,
							MainViewUnderlay.Instance.MainViewOverlay.GridLineWidth,
							"Width of the grid lines in pixels",
							MapView,
							null, MainViewUnderlay.Instance.MainViewOverlay);
			Options.AddOption(
							GraySelection,
							MainViewUnderlay.Instance.MainViewOverlay.GraySelection,
							"If true the selection area will be drawn in grayscale",
							MapView,
							null, MainViewUnderlay.Instance.MainViewOverlay);

			Options.AddOption(
							SpriteShade,
							MainViewUnderlay.Instance.MainViewOverlay.SpriteShade,
							"The darkness of the tile sprites (10..100 default 0 off, unity is 33)"
								+ " Values outside the range turn sprite shading off",
							Sprites,
							null, MainViewUnderlay.Instance.MainViewOverlay);

			string desc = "The technique used for resizing sprites (0..7)" + Environment.NewLine
						+ "0 - default"                                    + Environment.NewLine
						+ "1 - low (default)"                              + Environment.NewLine
						+ "2 - high (recommended)"                         + Environment.NewLine
						+ "3 - bilinear (defaultiest)"                     + Environment.NewLine
						+ "4 - bicubic (very slow fullscreen)"             + Environment.NewLine
						+ "5 - nearest neighbor (fastest)"                 + Environment.NewLine
						+ "6 - high quality bilinear (smoothest)"          + Environment.NewLine
						+ "7 - high quality bicubic (best in a pig's eye)";
			Options.AddOption(
							Interpolation,
							MainViewUnderlay.Instance.MainViewOverlay.Interpolation,
							desc,
							Sprites,
							null, MainViewUnderlay.Instance.MainViewOverlay);

//			Options.AddOption(
//							SaveOnExit,
//							true,
//							"If true these settings will be saved on program exit", // hint: yes they will be.
//							Main);
		}

		/// <summary>
		/// Handles a MainView Options change by the user.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		private void OnOptionChange(
				string key,
				object value)
		{
			Options[key].Value = value;
			switch (key)
			{
				case Animation: // NOTE: 'miOn.Checked' and 'miOff.Checked' are used by the F1 and F2 keys to switch animations on/off.
					miOff.Checked = !(miOn.Checked = (bool)value);
					MainViewUnderlay.Animate(miOn.Checked);

					if (!miOn.Checked) // show the doorsprites closed in TileView and QuadrantPanel.
					{
						if (miDoors.Checked) // toggle off doors if general animations stop.
						{
							miDoors.Checked = false;
							ToggleDoorSprites(false);
						}
						ViewerFormsManager.TileView.Refresh();
						ViewerFormsManager.TopView     .Control   .QuadrantsPanel.Refresh();
						ViewerFormsManager.TopRouteView.ControlTop.QuadrantsPanel.Refresh();
					}
					else if (miOn.Checked && miDoors.Checked) // doors need to animate if they were already toggled on.
						ToggleDoorSprites(true);
					break;

				case Doors: // NOTE: 'miDoors.Checked' is used by the F3 key to toggle door animations.
					miDoors.Checked = (bool)value;

					if (miOn.Checked)
					{
						ToggleDoorSprites(miDoors.Checked);
					}
					else if (miDoors.Checked) // switch to the doors' alt-tile (whether ufo-door or hinge-door)
					{
						if (_mainViewUnderlay.MapBase != null) // NOTE: MapBase is null on MapView load.
						{
							foreach (Tilepart part in _mainViewUnderlay.MapBase.Parts)
								part.SetDoorToAlternateSprite();

							Refresh();
						}
					}
					else // switch doors to Image1.
						ToggleDoorSprites(false);
					break;

				case AllowBringToFront:
					_allowBringToFront = (bool)value;
					break;

				case SaveWindowPositions:
//					PathsEditor.SaveRegistry = (bool)value; // TODO: find a place to cache this value.
					break;

				case ShowGrid: // NOTE: 'miGrid.Checked' is used by the F4 key to toggle the grid on/off.
					_mainViewUnderlay.MainViewOverlay.ShowGrid = (miGrid.Checked = (bool)value);

//					MainViewUnderlay.Instance.MainViewOverlay.ShowGrid = (bool)value;
					break;

				case GridLayerColor:
					MainViewUnderlay.Instance.MainViewOverlay.GridLayerColor = (Color)value;
					break;

				case GridLayerOpacity:
					MainViewUnderlay.Instance.MainViewOverlay.GridLayerOpacity = (int)value;
					break;

				case GridLineColor:
					MainViewUnderlay.Instance.MainViewOverlay.GridLineColor = (Color)value;
					break;

				case GridLineWidth:
					MainViewUnderlay.Instance.MainViewOverlay.GridLineWidth = (int)value;
					break;

				case SpriteShade:
					MainViewUnderlay.Instance.MainViewOverlay.SpriteShade = (int)value;
					break;

				case Interpolation:
					MainViewUnderlay.Instance.MainViewOverlay.Interpolation = (int)value;
					break;

				// NOTE: 'GraySelection' is handled. reasons ...
			}
		}

		/// <summary>
		/// This has nothing to do with the Registry anymore, but it saves
		/// MainView's Options as well as its screen-size and -position to YAML
		/// when the app closes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnSaveOptionsFormClosing(object sender, CancelEventArgs args)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("XCMainWindow.OnSaveOptionsFormClosing");

			_quit = true;
			args.Cancel = false;

			if (SaveAlertMap() == DialogResult.Cancel) // NOTE: do not short-circuit these ->
			{
				_quit = false;
				args.Cancel = true;
			}

			if (SaveAlertRoutes() == DialogResult.Cancel)
			{
				_quit = false;
				args.Cancel = true;
			}

			if (SaveAlertMaptree() == DialogResult.Cancel)
			{
				_quit = false;
				args.Cancel = true;
			}

			if (_quit)
			{
				_mainMenusManager.IsQuitting();

				_optionsManager.SaveOptions(); // save MV_OptionsFile // TODO: Save settings when closing the Options form(s).

//				if (PathsEditor.SaveRegistry) // TODO: re-implement.
				{
					WindowState = FormWindowState.Normal;
					_viewersManager.CloseSubsidiaryViewers();

					string dirSettings = SharedSpace.Instance.GetShare(SharedSpace.SettingsDirectory);
					string src = Path.Combine(dirSettings, PathInfo.ConfigViewers);
					string dst = Path.Combine(dirSettings, PathInfo.ConfigViewersOld);

					File.Copy(src, dst, true);

					using (var sr = new StreamReader(File.OpenRead(dst))) // but now use dst as src ->

					using (var fs = new FileStream(src, FileMode.Create)) // overwrite previous viewers-config.
					using (var sw = new StreamWriter(fs))
					{
						while (sr.Peek() != -1)
						{
							string line = sr.ReadLine().TrimEnd();

							if (String.Equals(line, RegistryInfo.MainView + ":", StringComparison.Ordinal))
							{
								sw.WriteLine(line);

								line = sr.ReadLine();
								line = sr.ReadLine();
								line = sr.ReadLine();
								line = sr.ReadLine(); // heh

								sw.WriteLine("  left: "   + Math.Max(0, Location.X));	// =Left
								sw.WriteLine("  top: "    + Math.Max(0, Location.Y));	// =Top
								sw.WriteLine("  width: "  + ClientSize.Width);			// <- use ClientSize, since Width and Height
								sw.WriteLine("  height: " + ClientSize.Height);			// screw up due to the titlebar/menubar area.
							}
							else
								sw.WriteLine(line);
						}
					}
					File.Delete(dst);
				}

				// kL_note: This is for storing MainViewer size and position in
				// the Windows Registry:
//				if (PathsEditor.SaveRegistry)
//				{
//					using (var keySoftware = Registry.CurrentUser.CreateSubKey(DSShared.Windows.RegistryInfo.SoftwareRegistry))
//					using (var keyMapView = keySoftware.CreateSubKey(DSShared.Windows.RegistryInfo.MapViewRegistry))
//					using (var keyMainView = keyMapView.CreateSubKey("MainView"))
//					{
//						_mainViewsManager.CloseAllViewers();
//						WindowState = FormWindowState.Normal;
//						keyMainView.SetValue("Left",   Left);
//						keyMainView.SetValue("Top",    Top);
//						keyMainView.SetValue("Width",  Width);
//						keyMainView.SetValue("Height", Height - SystemInformation.CaptionButtonSize.Height);
//						keyMainView.Close();
//						keyMapView.Close();
//						keySoftware.Close();
//					}
//				}
			}
		}
		#endregion


		#region Eventcalls (override)
		/// <summary>
		/// Overrides the OnActivated event. Brings any other open viewers to
		/// the top of the desktop, along with this.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnActivated(EventArgs e)
		{
			if (_allowBringToFront)
			{
				if (!_bypassActivatedEvent)			// don't let 'TopMost_set' fire the OnActivated event.
				{
					_bypassActivatedEvent = true;	// don't let the loop over the viewers re-trigger this activated event.
													// NOTE: 'TopMost_set' won't, but other calls like BringToFront() or Select() can/will.
					bool doit = false;

					foreach (MenuItem it in menuViewers.MenuItems)
					{
						if (it.Checked)
						{
							doit = true;

							var f = it.Tag as Form;
							f.TopMost = true;
							f.TopMost = false;
						}
					}

					if (doit)
					{
						TopMost = true;		// NOTE: These are needed despite calling base.OnActivated() below_
						TopMost = false;	// IMPORTANT: trying to bring this form to the top
					}						// after the other forms apparently fails in Windows 10
											// - which makes it impossible for MainView to gain focus
											// when clicked (if there are other viewers open).

					base.OnActivated(e);	// <--||

					_bypassActivatedEvent = false;
				}
			}
			else
				base.OnActivated(e);
		}
		#endregion


		#region Eventcalls
		private void OnAnimationUpdate(object sender, EventArgs e)
		{
			ViewerFormsManager.TopView     .Control   .QuadrantsPanel.Refresh();
			ViewerFormsManager.TopRouteView.ControlTop.QuadrantsPanel.Refresh();
		}

		/// <summary>
		/// Fired by the F1 key to turn animations On.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOnClick(object sender, EventArgs e)
		{
			OnOptionChange(Animation, true);
		}

		/// <summary>
		/// Fired by the F2 key to turn animations Off.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOffClick(object sender, EventArgs e)
		{
			OnOptionChange(Animation, false);
		}

		/// <summary>
		/// Fired by the F3 key to toggle door animations.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToggleDoorsClick(object sender, EventArgs e)
		{
			OnOptionChange(Doors, !miDoors.Checked);
		}

		/// <summary>
		/// Fired by the F4 key to toggle the grid on/off.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnToggleGridClick(object sender, EventArgs e)
		{
			OnOptionChange(ShowGrid, !miGrid.Checked);
		}


		private void OnSaveAllClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MapBase != null)
			{
				_mainViewUnderlay.MapBase.SaveMap();
				_mainViewUnderlay.MapBase.SaveRoutes();
			}
			MaptreeChanged = !ResourceInfo.TileGroupInfo.SaveTileGroups();
		}

		internal void OnSaveMapClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MapBase != null)
				_mainViewUnderlay.MapBase.SaveMap();
		}

		internal void OnSaveRoutesClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MapBase != null)
				_mainViewUnderlay.MapBase.SaveRoutes();
		}

		private void OnSaveAsClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MapBase != null
				&& _mainViewUnderlay.MapBase.Descriptor != null)	// safety. Not sure if a 'MapBase' could be
			{														// instantiated without a 'Descriptor'.
				var sfd = new SaveFileDialog();

				sfd.FileName = _mainViewUnderlay.MapBase.Descriptor.Label + MapFileChild.MapExt;
				sfd.Filter = "Map files (*.MAP)|*.MAP|All files (*.*)|*.*";
				sfd.Title = "Save Map and subordinate Route file as ...";
				sfd.InitialDirectory = Path.Combine(_mainViewUnderlay.MapBase.Descriptor.BasePath, MapFileChild.MapsDir);

				if (sfd.ShowDialog() == DialogResult.OK)
				{
					string dir = Path.GetDirectoryName(sfd.FileName); // 'FileName' is fullpath.
					//LogFile.WriteLine("dir= " + dir);
					// NOTE: GetDirectoryName() will return a string ending with a
					// path-separator if it's the root dir, and without one if it's
					// not. But Path.Combine() doesn't figure out the difference.

					//LogFile.WriteLine("pathroot= " + Path.GetPathRoot(dir));
					if (dir != Path.GetPathRoot(dir))
					{
						string basepath = dir.Substring(0, dir.LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal));
						//LogFile.WriteLine("basepath= " + basepath);
						if (basepath.IndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) == -1)	// check if root dir, again
							basepath += Path.DirectorySeparatorChar.ToString();											// account for awkward path at the root dir.
																														// NOTE: But that's probly not valid for
																														// things like mounted or network drives.
						string dirMaps   = Path.Combine(basepath, MapFileChild.MapsDir);
						string dirRoutes = Path.Combine(basepath, RouteNodeCollection.RoutesDir);
						//LogFile.WriteLine("dirMaps= " + dirMaps);
						//LogFile.WriteLine("dirRoutes= " + dirRoutes);

						string file = Path.GetFileNameWithoutExtension(sfd.FileName);
						string pfMaps   = Path.Combine(dirMaps, file);
						string pfRoutes = Path.Combine(dirRoutes, file);
						//LogFile.WriteLine("pfMaps= " + pfMaps);
						//LogFile.WriteLine("pfRoutes= " + pfRoutes);

						_mainViewUnderlay.MapBase.SaveMap(pfMaps);
						_mainViewUnderlay.MapBase.SaveRoutes(pfRoutes);
					}
					else
						MessageBox.Show(
									this,
									"Saving to a root folder is not allowed. raesons.",
									"Error",
									MessageBoxButtons.OK,
									MessageBoxIcon.Error,
									MessageBoxDefaultButton.Button1,
									0);
				}
			}
		}

		private void OnSaveMaptreeClick(object sender, EventArgs e)
		{
			MaptreeChanged = !ResourceInfo.TileGroupInfo.SaveTileGroups();
		}


		private void OnRegenOccultClick(object sender, EventArgs e) // disabled in designer w/ Visible=FALSE
		{
			var mapFile = MainViewUnderlay.Instance.MapBase as MapFileChild;
			if (mapFile != null)
			{
				mapFile.CalculateOccultations();
				Refresh();
			}
		}


		/// <summary>
		/// Opens the Configuration Editor.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConfiguratorClick(object sender, EventArgs e)
		{
			string changed = null;

			if (_mainViewUnderlay.MapBase != null)
			{
				if (_mainViewUnderlay.MapBase.MapChanged)
					changed = "Map";

				if (_mainViewUnderlay.MapBase.RoutesChanged)
				{
					if (!String.IsNullOrEmpty(changed))
						changed += " and ";

					changed += "Routes";
				}
			}

			if (MaptreeChanged)
			{
				if (!String.IsNullOrEmpty(changed))
					changed += " and ";

				changed += "Maptree";
			}

			if (!String.IsNullOrEmpty(changed))
			{
				switch (MessageBox.Show(
									this,
									"Accepting the Configuration Editor can restart MapView."
										+ " The current " + changed + " should be saved or else"
										+ " any changes will be lost. How do you wish to proceed?"
										+ Environment.NewLine + Environment.NewLine
										+ "Abort\treturn to state"
										+ Environment.NewLine
										+ "Retry\tsave changes and open the Configurator"
										+ Environment.NewLine
										+ "Ignore\trisk losing changes and open the Configurator",
									"Changes detected",
									MessageBoxButtons.AbortRetryIgnore,
									MessageBoxIcon.Asterisk,
									MessageBoxDefaultButton.Button1,
									0))
				{
					case DialogResult.Abort:
						break;

					case DialogResult.Retry:
						if (_mainViewUnderlay.MapBase != null)
						{
							if (_mainViewUnderlay.MapBase.MapChanged)
								_mainViewUnderlay.MapBase.SaveMap();

							if (_mainViewUnderlay.MapBase.RoutesChanged)
								_mainViewUnderlay.MapBase.SaveRoutes();
						}

						if (MaptreeChanged)
						{
							ResourceInfo.TileGroupInfo.SaveTileGroups();
							MaptreeChanged = false;
						}

						OnConfiguratorClick(null, EventArgs.Empty); // recurse.
						break;

					case DialogResult.Ignore:
						_mainViewUnderlay.MapBase.ClearMapChanged();
						_mainViewUnderlay.MapBase.ClearRoutesChanged();
						MaptreeChanged = false;

						OnConfiguratorClick(null, EventArgs.Empty); // recurse.
						break;
				}
			}
			else
			{
				using (var f = new ConfigurationForm(true))
				{
					if (f.ShowDialog(this) == DialogResult.OK)
					{
						Application.Restart();
						Environment.Exit(0);
					}
				}
			}
		}


		private void OnQuitClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("XCMainWindow.OnQuitClick");

			OnSaveOptionsFormClosing(null, new CancelEventArgs()); // set '_quit' flag

			if (_quit)
				Environment.Exit(0); // god, that works so much better than Application.Exit()
		}


		private void OnOptionsClick(object sender, EventArgs e)
		{
			var it = (MenuItem)sender;
			if (!it.Checked)
			{
				it.Checked = true;

				_foptions = new OptionsForm("MainViewOptions", Options);
				_foptions.Text = "MainView Options";

				_foptions.Show();

				_foptions.FormClosing += (sender1, e1) =>
										{
											if (!_closing)
												OnOptionsClick(sender, e);

											_closing = false;
										};
			}
			else
			{
				_closing = true;

				it.Checked = false;
				_foptions.Close();
			}
		}


		private void OnSaveImageClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MapBase != null)
			{
				sfdSaveDialog.FileName = _mainViewUnderlay.MapBase.Descriptor.Label;
				if (sfdSaveDialog.ShowDialog() == DialogResult.OK)
				{
//					_loadingProgress.Show();

//					try
//					{
					_mainViewUnderlay.MapBase.SaveGifFile(sfdSaveDialog.FileName);
//					}
//					finally
//					{
//						_loadingProgress.Hide();
//					}
				}
			}
		}

		private void OnHq2xClick(object sender, EventArgs e) // disabled in designer w/ Visible=FALSE.
		{
//			var map = _mainViewPanel.MapBase as MapFileChild;
//			if (map != null)
//			{
//				map.HQ2X();
//				_mainViewPanel.OnResize();
//			}
		}


		private void OnMapResizeClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MainViewOverlay.MapBase != null)
			{
				using (var f = new MapResizeInputBox())
				{
					f.MapBase = _mainViewUnderlay.MainViewOverlay.MapBase;

					if (f.ShowDialog(this) == DialogResult.OK)
					{
						f.MapBase.MapResize(
										f.Rows,
										f.Cols,
										f.Levs,
										f.CeilingChecked);

						_mainViewUnderlay.ForceResize();

						_mainViewUnderlay.MainViewOverlay.FirstClick = false;

						tsslDimensions.Text = f.MapBase.MapSize.ToString();
						tsslPosition.Text = String.Empty;

						ViewerFormsManager.SetObservers(f.MapBase);

//						ViewerFormsManager.RouteView.Control.ClearSelectedLocation(); // ... why not

						ViewerFormsManager.TopView     .Control   .TopViewPanel.ClearSelectorLozenge();
						ViewerFormsManager.TopRouteView.ControlTop.TopViewPanel.ClearSelectorLozenge();
					}
				}
			}
		}


		private void OnInfoClick(object sender, EventArgs e)
		{
			if (_mainViewUnderlay.MapBase != null)
			{
				var f = new MapInfoOutputBox();
				f.Show();
				f.Analyze(_mainViewUnderlay.MapBase as MapFileChild);
			}
		}


		private void OnExportClick(object sender, EventArgs e) // disabled in designer w/ Visible=FALSE.
		{
//			if (mapList.SelectedNode.Parent == null) // top level node - bad
//				throw new Exception("miExport_Click: Should not be here");
//
//			ExportForm ef = new ExportForm();
//			List<string> maps = new List<string>();
//
//			if (mapList.SelectedNode.Parent.Parent == null)//tileset
//			{
//				foreach (TreeNode tn in mapList.SelectedNode.Nodes)
//					maps.Add(tn.Text);
//			}
//			else // map
//				maps.Add(mapList.SelectedNode.Text);
//
//			ef.Maps = maps;
//			ef.ShowDialog();
		}

		private void OnOpenClick(object sender, EventArgs e) // disabled in designer w/ Visible=FALSE.
		{}

		private void OnSelectionBoxClick(object sender, EventArgs e) // NOTE: is disabled w/ Visible=FALSE in designer.
		{
//			_mainViewPanel.MapView.DrawSelectionBox = !_mainViewPanel.MapView.DrawSelectionBox;
//			tsbSelectionBox.Checked = !tsbSelectionBox.Checked;
		}


		internal void OnZoomInClick(object sender, EventArgs e)
		{
			if (Globals.Scale < Globals.ScaleMaximum)
			{
				Globals.Scale += Math.Min(
										Globals.ScaleMaximum - Globals.Scale,
										ScaleDelta);
				Zoom();
			}
		}

		internal void OnZoomOutClick(object sender, EventArgs e)
		{
			if (Globals.Scale > Globals.ScaleMinimum)
			{
				Globals.Scale -= Math.Min(
										Globals.Scale - Globals.ScaleMinimum,
										ScaleDelta);
				Zoom();
			}
		}

		private void Zoom()
		{
			ViewerFormsManager.ToolFactory.SetAutozoomChecked(false);
			Globals.AutoScale = false;

			_mainViewUnderlay.SetOverlaySize();
			_mainViewUnderlay.UpdateScrollers();

			Refresh();
		}

		internal void OnAutoScaleClick(object sender, EventArgs e)
		{
			Globals.AutoScale = ViewerFormsManager.ToolFactory.ToggleAutozoomChecked();
			if (Globals.AutoScale)
			{
				_mainViewUnderlay.SetScale();
				_mainViewUnderlay.SetOverlaySize();
			}
			_mainViewUnderlay.UpdateScrollers();
		}


		/// <summary>
		/// Searches the MapTree for a given string.
		/// </summary>
		/// <param name="text"></param>
		internal void Search(string text)
		{
			if (!String.IsNullOrEmpty(text))
			{
				TreeNode
					start0,
					start = null;

				if (Searched != null)
					start0 = Searched;
				else
					start0 = tvMaps.SelectedNode;

				if (start0 != null)
				{
					start = start0;

					if (start.Nodes.Count != 0)
					{
						start = start.Nodes[0];
					}
					else if (start.NextNode != null)
					{
						start = start.NextNode;
					}
					else if (start.Parent != null)
					{
						if (start.Parent.NextNode != null)
						{
							start = start.Parent.NextNode;
						}
						else if (start.Parent.Parent != null
							&& start.Parent.Parent.NextNode != null)
						{
							start = start.Parent.Parent.NextNode;
						}
						else
							start = tvMaps.Nodes[0];
					}
					else
						start = tvMaps.Nodes[0];

					if (start != null) // jic.
					{
						var node = SearchTreeview(
												text.ToLower(),
												tvMaps.Nodes,
												start,
												start0);
						if (node != null)
						{
							if (Searched != null)
								Searched.ForeColor = _colorSearch;

							Searched = node;

							_colorSearch = Searched.ForeColor;
							Searched.ForeColor = Color.BlueViolet;
							Searched.EnsureVisible();
						}

						_active   =
						_hardstop = false;
					}
				}
			}
		}

		bool _active, _hardstop;

		/// <summary>
		/// Searches through the MapTree given a node to start at.
		/// </summary>
		/// <param name="text">the text to search for (lowercase)</param>
		/// <param name="nodes">the collection of nodes to search through</param>
		/// <param name="start">the node to start at</param>
		/// <param name="start0">the node to stop at</param>
		/// <returns>a found node or null</returns>
		private TreeNode SearchTreeview(
				string text,
				TreeNodeCollection nodes,
				TreeNode start,
				TreeNode start0)
		{
			if (!_hardstop)
			{
				TreeNode child;

				foreach (TreeNode node in nodes)
				{
					if (!_active)
					{
						_active = (node == start);
					}
					else if (node == start0)
					{
						_hardstop = true;	// <- whatever you were doing in the way of recursions stop it.
						return null;		// -> not found after wrapping. NOTE: Does not highlight the
					}						// current node even if that node has the searched for text.

					if (_active && node.Text.ToLower().Contains(text))
					{
						return node;
					}

					if (node.Nodes.Count != 0)
					{
						if ((child = SearchTreeview(text, node.Nodes, start, start0)) != null)
						{
							return child;
						}
					}
					else if (node.NextNode == null												// if no more nodes at the current level
						&& (node.Parent == null        || node.Parent.NextNode == null)			// and no parent OR parent is last node at its level
						&& (node.Parent.Parent == null || node.Parent.Parent.NextNode == null))	// and no parent-of-parent OR parent-of-parent is last node at its level
					{
						return SearchTreeview(text, tvMaps.Nodes, tvMaps.Nodes[0], start0);
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Clears the searched, found, and highlighted Treenode.
		/// </summary>
		internal void ClearSearched()
		{
			if (Searched != null)
			{
				Searched.ForeColor = _colorSearch;
				Searched = null;
			}
		}

		// debug versions of functions above^
//		int _recurse = -1;
/*		internal void Search(string text)
		{
			if (!String.IsNullOrEmpty(text))
			{
				TreeNode start0, start;

				if (Searched != null)
					start0 = Searched;
				else
					start0 = tvMaps.SelectedNode;

				if (start0 != null)
				{
					start = start0;

					LogFile.WriteLine("");
					LogFile.WriteLine("start= " + start.Text);

					if (start.Nodes.Count != 0)
					{
						start = start.Nodes[0];
						LogFile.WriteLine(". start.Nodes.Count != 0 start= " + start.Text);
					}
					else if (start.NextNode != null)
					{
						start = start.NextNode;
						LogFile.WriteLine(". start.NextNode != null start= " + start.Text);
					}
					else if (start.Parent != null)
					{
						LogFile.WriteLine(". start.Parent != null");

						if (start.Parent.NextNode != null)
						{
							start = start.Parent.NextNode;
							LogFile.WriteLine(". . start.Parent.NextNode != null start= " + start.Text);
						}
						else if (start.Parent.Parent != null
							&& start.Parent.Parent.NextNode != null)
						{
							start = start.Parent.Parent.NextNode;
							LogFile.WriteLine(". . start.Parent.Parent != null && start.Parent.Parent.NextNode != null start= " + start.Text);
						}
						else // wrap
						{
//							LogFile.WriteLine(". . . start.Parent.Parent == null RETURN");
//							return;
							start = tvMaps.Nodes[0];
							LogFile.WriteLine(". . . start.Parent.Parent == null start= " + start.Text);
						}
					}
					else // wrap
					{
//						LogFile.WriteLine(". start.NextNode == null && start.Parent == null RETURN");
//						return;
						start = tvMaps.Nodes[0];
						LogFile.WriteLine(". start.NextNode == null && start.Parent == null start= " + start.Text);
					}

					if (start != null) // jic.
					{
						var node = SearchTreeview(text.ToLower(), tvMaps.Nodes, start, start0);
						if (node != null)
						{
							if (Searched != null)
								Searched.BackColor = DefaultBackColor;

							Searched = node;
							Searched.BackColor = Color.BlueViolet;
							Searched.EnsureVisible();
						}
						_active   =
						_hardstop = false;
					}
				}
			}
		} */
/*		private TreeNode SearchTreeview(
				string text,
				TreeNodeCollection nodes,
				TreeNode start,
				TreeNode start0)
		{
			if (!_hardstop)
			{
				int recurse = ++_recurse; // debug.

				TreeNode child;

				foreach (TreeNode node in nodes)
				{
					if (!_active)
					{
						_active = (node == start);
						if (_active) LogFile.WriteLine(recurse + " set Active");
					}
					else if (node == start0)
					{
						LogFile.WriteLine(recurse + " node == start0 ret NULL");
						_hardstop = true;	// <- whatever you were doing in the way of recursions stop it.
						return null;		// not found after wrapping. NOTE: Does not highlight the current node even if that node has the searched for text.
					}

					if (_active && node.Text.ToLower().Contains(text))
					{
						LogFile.WriteLine(recurse + " get " + node.Text);
						return node;
					}

					if (node.Nodes.Count != 0)
					{
						if ((child = SearchTreeview(text, node.Nodes, start, start0)) != null)
						{
							LogFile.WriteLine(recurse + " get child " + child.Text);
							return child;
						}
					}
					else if (node.NextNode == null												// if no more nodes at the current level
						&& (node.Parent == null        || node.Parent.NextNode == null)			// and no parent OR parent is last node at its level
						&& (node.Parent.Parent == null || node.Parent.Parent.NextNode == null))	// and no parent-of-parent OR parent-of-parent is last node at its level
					{
						LogFile.WriteLine(recurse + " search from Nodes[0]");
						return SearchTreeview(text, tvMaps.Nodes, tvMaps.Nodes[0], start0);
					}
				}
			}

			LogFile.WriteLine("ret NULL");
			return null;
		} */


		/// <summary>
		/// Opens a context-menu on RMB-click.
		/// NOTE: A MouseDown event occurs *before* the treeview's BeforeSelect
		/// and AfterSelected events occur ....
		/// NOTE: A MouseClick event occurs *after* the treeview's BeforeSelect
		/// and AfterSelected events occur. So the selected Map will change
		/// *before* a context-menu is shown, which is good.
		/// NOTE: A MouseClick event won't work if the tree is blank. So use MouseDown.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMapTreeMouseDown(object sender, MouseEventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnMapTreeMouseDown");
			//if (tvMaps.SelectedNode != null) LogFile.WriteLine(". selected= " + tvMaps.SelectedNode.Text);

			switch (e.Button)
			{
				case MouseButtons.Right:
					var badnode = tvMaps.GetNodeAt(e.Location);		// The right-clicked node is NOT selected
					if (badnode != null)
					{
						badnode.BackColor = SystemColors.Control;	// so quit highlighting it as if it were.
						badnode.ForeColor = SystemColors.ControlText;
					}

//					var goodnode = tvMaps.SelectedNode;				// The truly selected node is going to stop being highlighted
//					goodnode.BackColor = SystemColors.Highlight;	// even with this attempt to force it to stay highlighted.
//					goodnode.ForeColor = SystemColors.HighlightText;


					if (_mainViewUnderlay.MapBase == null														// prevents a bunch of problems, like looping dialogs when
						|| (!_mainViewUnderlay.MapBase.MapChanged && !_mainViewUnderlay.MapBase.RoutesChanged))	// returning from the Tileset Editor and the Maptree-node
					{																							// gets re-selected, causing this class-object to react as
						cmMapTreeMenu.MenuItems.Clear();														// if a different Map is going to load ... cf, LoadSelectedMap()

						cmMapTreeMenu.MenuItems.Add("Add Group ...", new EventHandler(OnAddGroupClick));

						if (tvMaps.SelectedNode != null)
						{
							switch (tvMaps.SelectedNode.Level)
							{
								case 0: // group-node.
									cmMapTreeMenu.MenuItems.Add("-");
									cmMapTreeMenu.MenuItems.Add("Edit Group ...", new EventHandler(OnEditGroupClick));
									cmMapTreeMenu.MenuItems.Add("Delete Group",   new EventHandler(OnDeleteGroupClick));
									cmMapTreeMenu.MenuItems.Add("-");
									cmMapTreeMenu.MenuItems.Add("Add Category ...", new EventHandler(OnAddCategoryClick));
									break;

								case 1: // category-node.
									cmMapTreeMenu.MenuItems.Add("-");
									cmMapTreeMenu.MenuItems.Add("Edit Category ...", new EventHandler(OnEditCategoryClick));
									cmMapTreeMenu.MenuItems.Add("Delete Category",   new EventHandler(OnDeleteCategoryClick));
									cmMapTreeMenu.MenuItems.Add("-");
									cmMapTreeMenu.MenuItems.Add("Add Tileset ...", new EventHandler(OnAddTilesetClick));
									break;

								case 2: // tileset-node.
									cmMapTreeMenu.MenuItems.Add("-");
									cmMapTreeMenu.MenuItems.Add("Edit Tileset ...", new EventHandler(OnEditTilesetClick));
									cmMapTreeMenu.MenuItems.Add("Delete Tileset",   new EventHandler(OnDeleteTilesetClick));
									break;
							}
						}

						cmMapTreeMenu.Show(tvMaps, e.Location);
					}
					else
					{
						switch (MessageBox.Show(
											this,
											"Modifying the Maptree can cause the Tilesets to reload."
												+ " The current Map and/or its Routes should be saved or else"
												+ " any changes would be lost. How do you wish to proceed?"
												+ Environment.NewLine + Environment.NewLine
												+ "Abort\treturn to state"
												+ Environment.NewLine
												+ "Retry\tsave changes and show the Maptree-menu"
												+ Environment.NewLine
												+ "Ignore\trisk losing changes and show the Maptree-menu",
											"Changes detected",
											MessageBoxButtons.AbortRetryIgnore,
											MessageBoxIcon.Asterisk,
											MessageBoxDefaultButton.Button1,
											0))
						{
							case DialogResult.Abort:
								break;

							case DialogResult.Retry:
								if (_mainViewUnderlay.MapBase.MapChanged)
									_mainViewUnderlay.MapBase.SaveMap();

								if (_mainViewUnderlay.MapBase.RoutesChanged)
									_mainViewUnderlay.MapBase.SaveRoutes();

								OnMapTreeMouseDown(null, e); // recurse.
								break;

							case DialogResult.Ignore:
								_mainViewUnderlay.MapBase.ClearMapChanged();
								_mainViewUnderlay.MapBase.ClearRoutesChanged();

								OnMapTreeMouseDown(null, e); // recurse.
								break;
						}
					}
					break;
			}
		}

		/// <summary>
		/// Adds a group to the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAddGroupClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnAddGroupClick");

			using (var f = new MapTreeInputBox(
											"Enter the label for a new Map group."
												+ " It needs to start with UFO or TFTD, since"
												+ " the prefix will set the default path and"
												+ " palette of its tilesets.",
											"Note that groups that do not contain"
												+ " tilesets will not be saved.",
											MapTreeInputBox.BoxType.AddGroup,
											String.Empty))
			{
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					MaptreeChanged = true;

					ResourceInfo.TileGroupInfo.AddTileGroup(f.Input);

					CreateTree();
					SelectGroupNode(f.Input);
				}
			}
		}

		/// <summary>
		/// Edits the label of a group on the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEditGroupClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnEditGroupClick");

			using (var f = new MapTreeInputBox(
											"Enter a new label for the Map group."
												+ " It needs to start with UFO or TFTD, since"
												+ " the prefix will set the default path and"
												+ " palette of its tilesets.",
											"Note that groups that do not contain"
												+ " tilesets will not be saved.",
											MapTreeInputBox.BoxType.EditGroup,
											String.Empty))
			{
				string labelGroup = tvMaps.SelectedNode.Text;

				f.Input = labelGroup;
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					MaptreeChanged = true;

					ResourceInfo.TileGroupInfo.EditTileGroup(
														f.Input,
														labelGroup);
					CreateTree();
					SelectGroupNode(f.Input);
				}
			}
		}

		/// <summary>
		/// Deletes a group from the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeleteGroupClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnDeleteGroupClick");

			// TODO: Make a custom box for delete Group/Category/Tileset.

			string labelGroup = tvMaps.SelectedNode.Text;

			string notice = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"Are you sure you want to remove this Map group?"
											+ " This will also remove all its categories and"
											+ " tilesets, but files on disk are unaffected.{0}{0}"
											+ "group\t{1}",
										Environment.NewLine,
										labelGroup);
			if (MessageBox.Show(
							this,
							notice,
							"Warning",
							MessageBoxButtons.OKCancel,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0) == DialogResult.OK)
			{
				MaptreeChanged = true;

				ResourceInfo.TileGroupInfo.DeleteTileGroup(labelGroup);

				CreateTree();
				SelectGroupNodeTop();
			}
		}

		/// <summary>
		/// Adds a category to a group on the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAddCategoryClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnAddCategoryClick");

			string labelGroup = tvMaps.SelectedNode.Text;

			using (var f = new MapTreeInputBox(
											"Enter the label for a new Map category.",
											"Note that categories that do not contain"
												+ " tilesets will not be saved.",
											MapTreeInputBox.BoxType.AddCategory,
											labelGroup))
			{
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					MaptreeChanged = true;

					var tilegroup = ResourceInfo.TileGroupInfo.TileGroups[labelGroup];
					tilegroup.AddCategory(f.Input);

					CreateTree();
					SelectCategoryNode(f.Input);
				}
			}
		}

		/// <summary>
		/// Edits the label of a category on the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEditCategoryClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnEditCategoryClick");

			string labelGroup = tvMaps.SelectedNode.Parent.Text;

			using (var f = new MapTreeInputBox(
											"Enter a new label for the Map category.",
											"Note that categories that do not contain"
												+ " tilesets will not be saved.",
											MapTreeInputBox.BoxType.EditCategory,
											labelGroup))
			{
				string labelCategory = tvMaps.SelectedNode.Text;

				f.Input = labelCategory;
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					MaptreeChanged = true;

					var tilegroup = ResourceInfo.TileGroupInfo.TileGroups[labelGroup];
					tilegroup.EditCategory(
										f.Input,
										labelCategory);
					CreateTree();
					SelectCategoryNode(f.Input);
				}
			}
		}

		/// <summary>
		/// Deletes a category from the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeleteCategoryClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnDeleteCategoryClick");

			// TODO: Make a custom box for delete Group/Category/Tileset.

			string labelGroup    = tvMaps.SelectedNode.Parent.Text;
			string labelCategory = tvMaps.SelectedNode.Text;

			string notice = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"Are you sure you want to remove this Map category?"
											+ " This will also remove all its tilesets, but"
											+ " files on disk are unaffected.{0}{0}"
											+ "group\t{1}{0}"
											+ "category\t{2}",
										Environment.NewLine,
										labelGroup, labelCategory);
			if (MessageBox.Show(
							this,
							notice,
							"Warning",
							MessageBoxButtons.OKCancel,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0) == DialogResult.OK)
			{
				MaptreeChanged = true;

				var tilegroup = ResourceInfo.TileGroupInfo.TileGroups[labelGroup];
				tilegroup.DeleteCategory(labelCategory);

				CreateTree();
				SelectCategoryNodeTop(labelGroup);
			}
		}

		/// <summary>
		/// Adds a tileset and its characteristics to the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAddTilesetClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnAddTilesetClick");

			string labelGroup    = tvMaps.SelectedNode.Parent.Text;
			string labelCategory = tvMaps.SelectedNode.Text;
			string labelTileset  = String.Empty;

			using (var f = new MapTreeTilesetInputBox(
												MapTreeTilesetInputBox.BoxType.AddTileset,
												labelGroup,
												labelCategory,
												labelTileset))
			{
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					//LogFile.WriteLine(". f.Tileset= " + f.Tileset);

					MaptreeChanged = true;

					CreateTree();
					SelectTilesetNode(f.Tileset);
				}
			}
		}

		/// <summary>
		/// Edits the characteristics of a tileset on the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEditTilesetClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnEditTilesetClick");

			string labelGroup    = tvMaps.SelectedNode.Parent.Parent.Text;
			string labelCategory = tvMaps.SelectedNode.Parent.Text;
			string labelTileset  = tvMaps.SelectedNode.Text;

			using (var f = new MapTreeTilesetInputBox(
												MapTreeTilesetInputBox.BoxType.EditTileset,
												labelGroup,
												labelCategory,
												labelTileset))
			{
				if (f.ShowDialog(this) == DialogResult.OK)
				{
					//LogFile.WriteLine(". f.Tileset= " + f.Tileset);

					MaptreeChanged = true;

					CreateTree();
					SelectTilesetNode(f.Tileset);
				}
			}
		}

		/// <summary>
		/// Deletes a tileset from the map-tree.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeleteTilesetClick(object sender, EventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnDeleteTilesetClick");

			// TODO: Make a custom box for delete Group/Category/Tileset.

			string labelGroup    = tvMaps.SelectedNode.Parent.Parent.Text;
			string labelCategory = tvMaps.SelectedNode.Parent.Text;
			string labelTileset  = tvMaps.SelectedNode.Text;

			string notice = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"Are you sure you want to remove this Map tileset?"
											+ " Files on disk are unaffected.{0}{0}"
											+ "group\t{1}{0}"
											+ "category\t{2}{0}"
											+ "tileset\t{3}",
										Environment.NewLine,
										labelGroup, labelCategory, labelTileset);
			if (MessageBox.Show(
							this,
							notice,
							"Warning",
							MessageBoxButtons.OKCancel,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0) == DialogResult.OK)
			{
				MaptreeChanged = true;

				var tilegroup = ResourceInfo.TileGroupInfo.TileGroups[labelGroup];
				tilegroup.DeleteTileset(labelTileset, labelCategory);

				CreateTree();
				SelectTilesetNodeTop(labelCategory);
			}
		}


		// TODO: consolidate the select node functions into a single function.

		/// <summary>
		/// Selects the top treenode in the Maps tree if one exists.
		/// </summary>
		private void SelectGroupNodeTop()
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("SelectGroupNodeTop");

			if (tvMaps.Nodes.Count != 0)
				tvMaps.SelectedNode = tvMaps.Nodes[0];
		}

		/// <summary>
		/// Selects the top category treenode in the Maps tree if one exists
		/// under a given group treenode.
		/// NOTE: Assumes that the parent-group node is valid.
		/// </summary>
		/// <param name="labelGroup"></param>
		private void SelectCategoryNodeTop(string labelGroup)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("SelectCategoryNodeTop");

			foreach (TreeNode nodeGroup in tvMaps.Nodes)
			{
				if (nodeGroup.Text == labelGroup)
				{
					var groupCollection = nodeGroup.Nodes;
					tvMaps.SelectedNode = (groupCollection.Count != 0) ? groupCollection[0]
																	   : nodeGroup;
				}
			}
		}

		/// <summary>
		/// Selects the top tileset treenode in the Maps tree if one exists
		/// under a given category treenode.
		/// NOTE: Assumes that the parent-parent-group and parent-category nodes
		/// are valid.
		/// </summary>
		/// <param name="labelCategory"></param>
		private void SelectTilesetNodeTop(string labelCategory)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("SelectTilesetNodeTop");

			foreach (TreeNode nodeGroup in tvMaps.Nodes)
			{
				var groupCollection = nodeGroup.Nodes;
				foreach (TreeNode nodeCategory in groupCollection)
				{
					if (nodeCategory.Text == labelCategory)
					{
						var categoryCollection = nodeCategory.Nodes;
						tvMaps.SelectedNode = (categoryCollection.Count != 0) ? categoryCollection[0]
																			  : nodeCategory;
					}
				}
			}
		}

		/// <summary>
		/// Selects a treenode in the Maps tree given a group-label.
		/// </summary>
		/// <param name="labelGroup"></param>
		private void SelectGroupNode(string labelGroup)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("SelectGroupNode");

			foreach (TreeNode nodeGroup in tvMaps.Nodes)
			{
				if (nodeGroup.Text == labelGroup)
				{
					tvMaps.SelectedNode = nodeGroup;
					nodeGroup.Expand();
					break;
				}
			}
		}

		/// <summary>
		/// Selects a treenode in the Maps tree given a category-label.
		/// </summary>
		/// <param name="labelCategory"></param>
		private void SelectCategoryNode(string labelCategory)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("SelectCategoryNode");

			bool found = false;

			foreach (TreeNode nodeGroup in tvMaps.Nodes)
			{
				if (found) break;

				var groupCollection = nodeGroup.Nodes;
				foreach (TreeNode nodeCategory in groupCollection)
				{
					if (nodeCategory.Text == labelCategory)
					{
						found = true;

						tvMaps.SelectedNode = nodeCategory;
						nodeCategory.Expand();
						break;
					}
				}
			}
		}

		/// <summary>
		/// Selects a treenode in the Maps tree given a tileset-label.
		/// </summary>
		/// <param name="labelTileset"></param>
		private void SelectTilesetNode(string labelTileset)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("SelectTilesetNode");

			bool found = false;

			foreach (TreeNode nodeGroup in tvMaps.Nodes)
			{
				if (found) break;

				//LogFile.WriteLine(". group= " + nodeGroup.Text);

				var groupCollection = nodeGroup.Nodes;
				foreach (TreeNode nodeCategory in groupCollection)
				{
					if (found) break;

					//LogFile.WriteLine(". . category= " + nodeCategory.Text);

					var categoryCollection = nodeCategory.Nodes;
					foreach (TreeNode nodeTileset in categoryCollection)
					{
						//LogFile.WriteLine(". . . tileset= " + nodeTileset.Text);

						if (nodeTileset.Text == labelTileset)
						{
							found = true;

							tvMaps.SelectedNode = nodeTileset;
							break;
						}
					}
				}
			}
		}


//		private bool _bypassSaveAlert;	// when reloading the MapTree after making a tileset edit
										// the treeview's BeforeSelect event fires. This needlessly
										// asks to save the Map (if it had already changed) and
										// results in an endless cycle of confirmation dialogs ...
										// so bypass all that.
										//
										// Congratulations. Another programming language/framework
										// I've come to hate. The BeforeSelect event fires twice
										// (at least) rendering the boolean entirely obsolete.
		/// <summary>
		/// Asks user to save before switching Maps if applicable.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMapTreeBeforeSelect(object sender, CancelEventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnMapTreeBeforeSelect");
			//if (tvMaps.SelectedNode != null) LogFile.WriteLine(". selected= " + tvMaps.SelectedNode.Text);

			e.Cancel  = (SaveAlertMap()    == DialogResult.Cancel);
			e.Cancel |= (SaveAlertRoutes() == DialogResult.Cancel); // NOTE: that bitwise had better execute ....
		}

		/// <summary>
		/// Loads the selected Map.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMapTreeAfterSelected(object sender, TreeViewEventArgs e)
		{
			//LogFile.WriteLine("XCMainWindow.OnMapTreeAfterSelected");
			//if (tvMaps.SelectedNode != null) LogFile.WriteLine(". selected= " + tvMaps.SelectedNode.Text);

			ClearSearched();
			LoadSelectedMap();
		}

		/// <summary>
		/// Reloads the map when a save is done in PckView (via TileView).
		/// </summary>
		private void OnPckSavedEvent()
		{
			bool cancel  = (SaveAlertMap()    == DialogResult.Cancel);
				 cancel |= (SaveAlertRoutes() == DialogResult.Cancel); // NOTE: that bitwise had better execute ....

			if (!cancel)
			{
				ResourceInfo.ReloadSprites = true;
				LoadSelectedMap();
			}
			ResourceInfo.ReloadSprites = false;
		}
		#endregion


		#region Methods (static)
		/// <summary>
		/// Transposes all the default viewer positions and sizes from the
		/// embedded MapViewers manifest to '/settings/MapViewers.yml'.
		/// Based on InstallationForm.
		/// </summary>
		private static void CreateViewersFile()
		{
			var info = (PathInfo)SharedSpace.Instance[PathInfo.ShareViewers];
			info.CreateDirectory();

			string pfe = info.Fullpath;

			using (var sr = new StreamReader(Assembly.GetExecutingAssembly()
													 .GetManifestResourceStream("MapView._Embedded.MapViewers.yml")))
			using (var fs = new FileStream(pfe, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				while (sr.Peek() != -1)
					sw.WriteLine(sr.ReadLine());
			}
		}
		#endregion


		#region Methods
		/// <summary>
		/// Loads the Map that's selected in the Maptree.
		/// </summary>
		private void LoadSelectedMap()
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("XCMainWindow.LoadSelectedMap");

			var descriptor = tvMaps.SelectedNode.Tag as Descriptor;
			if (descriptor != null)
			{
				//LogFile.WriteLine(". descriptor= " + descriptor);

				var @base = MapFileService.LoadTileset(descriptor);
				if (@base != null)
				{
					miSaveAll    .Enabled =
					miSaveMap    .Enabled =
					miSaveRoutes .Enabled =
					miSaveAs     .Enabled =
					miSaveImage  .Enabled =
					miResize     .Enabled =
					miInfo       .Enabled = true;

//					miRegenOccult.Enabled = true; // disabled in designer w/ Visible=FALSE.
//					miExport     .Enabled = true; // disabled in designer w/ Visible=FALSE.

					_mainViewUnderlay.MainViewOverlay.FirstClick = false;


					if (@base.Parts[0][0].Pal == Palette.UfoBattle)
					{
						_mainViewUnderlay.MainViewOverlay.SpriteBrushes = Palette.BrushesUfoBattle;
					}
					else
						_mainViewUnderlay.MainViewOverlay.SpriteBrushes = Palette.BrushesTftdBattle;

					_mainViewUnderlay.MapBase = @base;

					ViewerFormsManager.ToolFactory.EnableToolStrip(true);

					RouteCheckService.CheckNodeBounds(@base as MapFileChild);

					Text = "Map Editor - " + descriptor.BasePath;

					tsslMapLabel.Text = descriptor.Label;
					tsslDimensions.Text = (@base != null) ? @base.MapSize.ToString()
														  : "size: n/a";
					tsslPosition.Text = String.Empty;

					ViewerFormsManager.RouteView   .Control     .ClearSelectedInfo();
					ViewerFormsManager.TopRouteView.ControlRoute.ClearSelectedInfo();

					ViewerFormsManager.RouteView   .Control     .DisableOg();
					ViewerFormsManager.TopRouteView.ControlRoute.DisableOg();

					Options[Doors].Value = false; // toggle off door-animations; not sure that this is necessary to do.
					miDoors.Checked = false;
					ToggleDoorSprites(false);

					if (!menuViewers.Enabled) // open/close the forms that appear in the Viewers menu.
						_mainMenusManager.StartViewers();

					ViewerFormsManager.SetObservers(@base); // reset all observer events
				}
			}
		}

		/// <summary>
		/// Toggles the door-sprites to animate or not.
		/// </summary>
		/// <param name="animate">true to animate any doors</param>
		private void ToggleDoorSprites(bool animate)
		{
			if (_mainViewUnderlay.MapBase != null) // NOTE: MapBase is null on MapView load.
			{
				foreach (Tilepart part in _mainViewUnderlay.MapBase.Parts)
					part.SetDoorSprites(animate);

				Refresh();
			}
		}

		/// <summary>
		/// Shows the user a dialog-box asking to Save if the currently
		/// displayed Map has changed.
		/// NOTE: Is called when either (a) MapView is closing (b) another Map
		/// is about to load.
		/// </summary>
		/// <returns></returns>
		private DialogResult SaveAlertMap()
		{
			if (_mainViewUnderlay.MapBase != null && _mainViewUnderlay.MapBase.MapChanged)
			{
				switch (MessageBox.Show(
									this,
									"Do you want to save changes to the Map?",
									"Map Changed",
									MessageBoxButtons.YesNoCancel,
									MessageBoxIcon.Question,
									MessageBoxDefaultButton.Button1,
									0))
				{
					case DialogResult.Yes:		// save & clear MapChanged flag
						_mainViewUnderlay.MapBase.SaveMap();
						break;

					case DialogResult.No:		// don't save & clear MapChanged flag
						_mainViewUnderlay.MapBase.ClearMapChanged();
						break;

					case DialogResult.Cancel:	// dismiss confirmation dialog & leave state unaffected
						return DialogResult.Cancel;
				}
			}
			return DialogResult.OK;
		}

		/// <summary>
		/// Shows the user a dialog-box asking to Save if the currently
		/// displayed Routes has changed.
		/// NOTE: Is called when either (a) MapView is closing (b) another Map
		/// is about to load.
		/// </summary>
		/// <returns></returns>
		private DialogResult SaveAlertRoutes()
		{
			if (_mainViewUnderlay.MapBase != null && _mainViewUnderlay.MapBase.RoutesChanged)
			{
				switch (MessageBox.Show(
									this,
									"Do you want to save changes to the Routes?",
									"Routes Changed",
									MessageBoxButtons.YesNoCancel,
									MessageBoxIcon.Question,
									MessageBoxDefaultButton.Button1,
									0))
				{
					case DialogResult.Yes:		// save & clear RoutesChanged flag
						_mainViewUnderlay.MapBase.SaveRoutes();
						break;

					case DialogResult.No:		// don't save & clear RoutesChanged flag
						_mainViewUnderlay.MapBase.ClearRoutesChanged();
						break;

					case DialogResult.Cancel:	// dismiss confirmation dialog & leave state unaffected
						return DialogResult.Cancel;
				}
			}
			return DialogResult.OK;
		}

		/// <summary>
		/// Shows the user a dialog-box asking to Save the Maptree if it has
		/// changed.
		/// NOTE: Is called when either (a) MapView is closing (b) MapView is
		/// reloading due to a configuration change (ie. only if resource-paths
		/// have been changed, since the only other relevant option - if the
		/// tilesets-config file - is changed then saving the current one is
		/// pointless).
		/// </summary>
		/// <returns></returns>
		private DialogResult SaveAlertMaptree()
		{
			if (MaptreeChanged)
			{
				switch (MessageBox.Show(
									this,
									"Do you want to save changes to the Map Tree?",
									"Maptree Changed",
									MessageBoxButtons.YesNoCancel,
									MessageBoxIcon.Question,
									MessageBoxDefaultButton.Button1,
									0))
				{
					case DialogResult.Yes:		// save & clear MaptreeChanged flag
						OnSaveMaptreeClick(null, EventArgs.Empty);
						break;

					case DialogResult.No:		// don't save & clear MaptreeChanged flag
						MaptreeChanged = false; // kinda irrelevant since this class-object is about to disappear.
						break;

					case DialogResult.Cancel:	// dismiss confirmation dialog & leave state unaffected
						return DialogResult.Cancel;
				}
			}
			return DialogResult.OK;
		}

		/// <summary>
		/// Prints the currently selected location to the status bar.
		/// NOTE: The 'lev' should be inverted before it's passed in.
		/// </summary>
		/// <param name="col"></param>
		/// <param name="row"></param>
		/// <param name="lev"></param>
		internal void StatusBarPrintPosition(int col, int row, int lev)
		{
			if (_mainViewUnderlay.MainViewOverlay.FirstClick)
				tsslPosition.Text = String.Format(
												System.Globalization.CultureInfo.CurrentCulture,
												"c {0}  r {1}  L {2}",
												col, row, lev);
		}

		internal void StatusBarPrintScale()
		{
			tsslScale.Text = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"scale {0:0.00}",
										Globals.Scale);
		}
		#endregion
	}
}
