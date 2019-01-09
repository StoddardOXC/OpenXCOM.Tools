using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using DSShared;
using DSShared.Windows;

using PckView.Forms.SpriteBytes;

using XCom;
using XCom.Interfaces;

using YamlDotNet.RepresentationModel; // read values (deserialization)


namespace PckView
{
	public sealed partial class PckViewForm
		:
			Form
	{
		#region Events (static)
		internal static event PaletteChangedEventHandler PaletteChangedEvent;
		#endregion


		#region Fields (static)
		private readonly Palette DefaultPalette = Palette.UfoBattle;
		#endregion


		#region Fields
		private readonly PckViewPanel _pnlView = new PckViewPanel();
		private readonly EditorForm _feditor   = new EditorForm();

		private ConsoleForm _fconsole;

//		private TabControl _tcTabs; // for OnCompareClick()

		private MenuItem _miEdit;
		private MenuItem _miAdd;
		private MenuItem _miInsertBefore;
		private MenuItem _miInsertAfter;
		private MenuItem _miReplace;
		private MenuItem _miMoveLeft;
		private MenuItem _miMoveRight;
		private MenuItem _miDelete;
		private MenuItem _miExport;

//		private SharedSpace _share = SharedSpace.Instance;

		private Dictionary<Palette, MenuItem> _paletteItems = new Dictionary<Palette, MenuItem>();

		private bool _editorInited;

		private string _pfePck;
		private string _pfeTab;
		private string _pfePckOld;
		private string _pfeTabOld;
		#endregion


		#region Properties (static)
		internal static Palette Pal
		{ get; set; }
		#endregion


		#region Properties
		private string SpritesetDirectory
		{ get; set; }

		private string SpritesetLabel
		{ get; set; }

		/// <summary>
		/// For reloading the Map when PckView is invoked via TileView. That is,
		/// it's *not* a "do you want to save" alert.
		/// </summary>
		public bool SpritesChanged
		{ get; private set; }


		/// <summary>
		/// True if a Bigobs PCK+TAB set is opened.
		/// </summary>
		internal static bool IsBigobs
		{ get; private set; }
		#endregion


		#region cTor
		/// <summary>
		/// Creates the PckView window.
		/// </summary>
		public PckViewForm()
		{
			// NOTE: Set the debug-logfile-path in PckViewPanel, since it instantiates first.

			InitializeComponent();

			// WORKAROUND: See note in 'XCMainWindow' cTor.
			MaximumSize = new Size(0, 0); // fu.net

			LoadWindowMetrics();


			#region SharedSpace information
			_fconsole = new ConsoleSharedSpace(new SharedSpace()).Console;
			_fconsole.FormClosing += (sender, e) =>
									{
										e.Cancel = true;
										_fconsole.Hide();
									};

			FormClosed += (sender, e) => _fconsole.Close();


//			string dirApplication = Path.GetDirectoryName(Application.ExecutablePath);
//			string dirSettings    = Path.Combine(dirApplication, DSShared.PathInfo.SettingsDirectory);
//
//			_share.SetShare(
//						SharedSpace.ApplicationDirectory,
//						dirApplication);
//			_share.SetShare(
//						SharedSpace.SettingsDirectory,
//						dirSettings);

//			XConsole.AdZerg("Application directory: " + _share[SharedSpace.ApplicationDirectory]);
//			XConsole.AdZerg("Settings directory: "    + _share[SharedSpace.SettingsDirectory].ToString());
//			XConsole.AdZerg("Custom directory: "      + _share[SharedSpace.CustomDirectory].ToString());
			#endregion


			_pnlView.Dock = DockStyle.Fill;
			_pnlView.ContextMenu = ViewerContextMenu();
			_pnlView.SpritesetChangedEvent += OnSpritesetChanged;
			_pnlView.Click                 += OnSpriteClick;
			_pnlView.DoubleClick           += OnSpriteEditorClick;

			pnlView.Controls.Add(_pnlView);


//			_share[SharedSpace.Palettes] = new Dictionary<string, Palette>();

			PopulatePaletteMenu();

			Pal = DefaultPalette;
			Pal.SetTransparent(true);

			_paletteItems[Pal].Checked = true;

			_feditor.FormClosing += OnEditorFormClosing;


//			var regInfo = new RegistryInfo(RegistryInfo.PckView, this); // subscribe to Load and Closing events.
//			regInfo.RegisterProperties();
//			regInfo.AddProperty("SelectedPalette");
		}
		#endregion


		#region Load/save 'registry' info
		/// <summary>
		/// Positions the window at user-defined coordinates w/ size.
		/// </summary>
		private void LoadWindowMetrics()
		{
			string dirSettings = Path.Combine(
											Path.GetDirectoryName(Application.ExecutablePath),
											PathInfo.SettingsDirectory);
			string fileViewers = Path.Combine(dirSettings, PathInfo.ConfigViewers);
			if (File.Exists(fileViewers))
			{
				using (var sr = new StreamReader(File.OpenRead(fileViewers)))
				{
					var str = new YamlStream();
					str.Load(sr);

					var nodeRoot = str.Documents[0].RootNode as YamlMappingNode;
					foreach (var node in nodeRoot.Children)
					{
						string viewer = ((YamlScalarNode)node.Key).Value;
						if (String.Equals(viewer, RegistryInfo.PckView, StringComparison.Ordinal))
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
							if (!rectScreen.Contains(x + 200, y + 100)) // check to ensure that PckView is at least partly onscreen.
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
			}
		}

		/// <summary>
		/// Saves the window position and size to YAML.
		/// </summary>
		private void SaveWindowMetrics()
		{
			string dirSettings = Path.Combine(
											Path.GetDirectoryName(Application.ExecutablePath),
											PathInfo.SettingsDirectory);
			string fileViewers = Path.Combine(dirSettings, PathInfo.ConfigViewers);

			if (File.Exists(fileViewers))
			{
				WindowState = FormWindowState.Normal;

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

						if (String.Equals(line, RegistryInfo.PckView + ":", StringComparison.Ordinal))
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
		}
		#endregion


		/// <summary>
		/// Builds the RMB contextmenu.
		/// </summary>
		/// <returns></returns>
		private ContextMenu ViewerContextMenu()
		{
			var contextmenu = new ContextMenu();

			_miEdit = new MenuItem("Edit");
			_miEdit.Enabled = false;
			_miEdit.Click += OnSpriteEditorClick;
			contextmenu.MenuItems.Add(_miEdit);

			contextmenu.MenuItems.Add(new MenuItem("-"));

			_miAdd = new MenuItem("Add ...");
			_miAdd.Enabled = false;
			_miAdd.Click += OnAddSpritesClick;
			contextmenu.MenuItems.Add(_miAdd);

			_miInsertBefore = new MenuItem("Insert before ...");
			_miInsertBefore.Enabled = false;
			_miInsertBefore.Click += OnInsertSpritesBeforeClick;
			contextmenu.MenuItems.Add(_miInsertBefore);

			_miInsertAfter = new MenuItem("Insert after ...");
			_miInsertAfter.Enabled = false;
			_miInsertAfter.Click += OnInsertSpritesAfterClick;
			contextmenu.MenuItems.Add(_miInsertAfter);

			contextmenu.MenuItems.Add(new MenuItem("-"));

			_miReplace = new MenuItem("Replace ...");
			_miReplace.Enabled = false;
			_miReplace.Click += OnReplaceSpriteClick;
			contextmenu.MenuItems.Add(_miReplace);

			_miMoveLeft = new MenuItem("Move left");
			_miMoveLeft.Enabled = false;
			_miMoveLeft.Click += OnMoveLeftSpriteClick;
			contextmenu.MenuItems.Add(_miMoveLeft);

			_miMoveRight = new MenuItem("Move right");
			_miMoveRight.Enabled = false;
			_miMoveRight.Click += OnMoveRightSpriteClick;
			contextmenu.MenuItems.Add(_miMoveRight);

			contextmenu.MenuItems.Add(new MenuItem("-"));

//			_miDelete = new MenuItem("Delete\tDel");
			_miDelete = new MenuItem("Delete");
			_miDelete.Enabled = false;
			_miDelete.Click += OnDeleteSpriteClick;
			contextmenu.MenuItems.Add(_miDelete);

			contextmenu.MenuItems.Add(new MenuItem("-"));

			_miExport = new MenuItem("Export sprite ...");
			_miExport.Enabled = false;
			_miExport.Click += OnExportSpriteClick;
			contextmenu.MenuItems.Add(_miExport);

			return contextmenu;
		}

		/// <summary>
		/// Adds the palettes as menuitems to the palettes menu on the main
		/// menubar.
		/// </summary>
		private void PopulatePaletteMenu()
		{
			var pals = new List<Palette>();

			pals.Add(Palette.UfoBattle);
			pals.Add(Palette.UfoGeo);
			pals.Add(Palette.UfoGraph);
			pals.Add(Palette.UfoResearch);
			pals.Add(Palette.TftdBattle);
			pals.Add(Palette.TftdGeo);
			pals.Add(Palette.TftdGraph);
			pals.Add(Palette.TftdResearch);

			for (int i = 0; i != pals.Count; ++i)
			{
				var pal = pals[i];

				var itPal = new MenuItem(pal.Label);
				itPal.Tag = pal;
				miPaletteMenu.MenuItems.Add(itPal);

				itPal.Click += OnPaletteClick;
				_paletteItems[pal] = itPal;

				switch (i)
				{
					case 0: itPal.Shortcut = Shortcut.Ctrl1; break;
					case 1: itPal.Shortcut = Shortcut.Ctrl2; break;
					case 2: itPal.Shortcut = Shortcut.Ctrl3; break;
					case 3: itPal.Shortcut = Shortcut.Ctrl4; break;
					case 4: itPal.Shortcut = Shortcut.Ctrl5; break;
					case 5: itPal.Shortcut = Shortcut.Ctrl6; break;
					case 6: itPal.Shortcut = Shortcut.Ctrl7; break;
					case 7: itPal.Shortcut = Shortcut.Ctrl8; break;
				}
			}
//			((Dictionary<string, Palette>)_share[SharedSpace.Palettes])[pal.Label] = pal;
		}


		#region Eventcalls
		/// <summary>
		/// Focuses the viewer-panel after the app loads.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnShown(object sender, EventArgs e)
		{
			_pnlView.Select();
		}

		/// <summary>
		/// Enables (or disables) various menuitems.
		/// Called when the SpritesetChangedEvent is raised.
		/// </summary>
		/// <param name="valid"></param>
		private void OnSpritesetChanged(bool valid)
		{
			// under File menu
			miSave             .Enabled =
			miSaveAs           .Enabled =
			miExportSprites    .Enabled =
			miExportSpritesheet.Enabled =
			miImportSpritesheet.Enabled =

			// on Main menu
			miPaletteMenu      .Enabled =
			miTransparentMenu  .Enabled =
			miBytesMenu        .Enabled =

			// on Context menu
			_miAdd             .Enabled = valid;
		}

		/// <summary>
		/// Bring back the dinosaurs. Enables (or disables) several menuitems.
		/// Also freshens sprite data in the sprite-editor and byte-viewer if
		/// applicable.
		/// Called when the viewer-panel's Click event is raised.
		/// NOTE: This fires after PckViewPanel.OnMouseDown(). Thought you'd
		/// like to know.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSpriteClick(object sender, EventArgs e)
		{
			bool valid = (_pnlView.SelectedId != -1);

			// on Context menu
			_miEdit        .Enabled =
			_miInsertBefore.Enabled =
			_miInsertAfter .Enabled =
			_miReplace     .Enabled =
			_miDelete      .Enabled =
			_miExport      .Enabled = valid;
			_miMoveLeft    .Enabled = valid && (_pnlView.SelectedId != 0);
			_miMoveRight   .Enabled = valid && (_pnlView.SelectedId != _pnlView.Spriteset.Count - 1);
		}

		/// <summary>
		/// Opens the currently selected sprite in the sprite-editor.
		/// Called when the contextmenu's Click event or the viewer-panel's
		/// DoubleClick event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSpriteEditorClick(object sender, EventArgs e)
		{
			if (_pnlView.Spriteset != null && _pnlView.SelectedId != -1)
			{
				EditorPanel.Instance.Sprite = _pnlView.Spriteset[_pnlView.SelectedId];

				if (!_feditor.Visible)
				{
					_miEdit.Checked = true;	// TODO: show as Checked only if the currently
											// selected sprite is actually open in the editor.
					if (!_editorInited)
					{
						_editorInited = true;
						_feditor.Left = Left + 20;
						_feditor.Top  = Top  + 20;
					}
					_feditor.Show();
				}
				else
					_feditor.BringToFront();
			}
		}

		/// <summary>
		/// Cancels closing the editor and hides it instead.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnEditorFormClosing(object sender, CancelEventArgs e)
		{
			_miEdit.Checked = false;

			e.Cancel = true;
			_feditor.Hide();
		}

		/// <summary>
		/// Adds a sprite or sprites to the collection.
		/// Called when the contextmenu's Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAddSpritesClick(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
//				if (IsBigobs)
//					ofd.Title = "Add 32x48 8-bpp PNG file(s)";
//				else
				ofd.Title = "Add 32x40 8-bpp Image file(s)";

				ofd.Filter = "Image files (*.PNG *.GIF *.BMP)|*.PNG;*.GIF;*.BMP|"
						   + "PNG files (*.PNG)|*.PNG|GIF files (*.GIF)|*.GIF|BMP files (*.BMP)|*.BMP|"
						   + "All files (*.*)|*.*";
				ofd.Multiselect = true;

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					int id = _pnlView.Spriteset.Count - 1;

					foreach (string file in ofd.FileNames)
					{
						byte[] bindata = File.ReadAllBytes(file);
						Bitmap b = BitmapHandler.LoadBitmap(bindata);

//						var bitmap = new Bitmap(file);	// <- bork. Creates a 32-bpp Argb image if source is 8-bpp
														// PNG w/tranparency; BMP however retains 8-bpp indices.
						//LogFile.WriteLine("Width= " + bitmap.Width);
						//LogFile.WriteLine("Height= " + bitmap.Height);
						//LogFile.WriteLine("PixelFormat= " + bitmap.PixelFormat); // Format8bppIndexed

						var sprite = BitmapService.CreateSprite(
															b,
															++id,
															Pal,
															0, 0,
															XCImage.SpriteWidth,
															XCImage.SpriteHeight);
						_pnlView.Spriteset.Add(sprite);
					}

					OnSpriteClick(null, EventArgs.Empty);

					_pnlView.PrintStatusTotal();

					_pnlView.ForceResize();
					Refresh();
				}
			}
		}

		/// <summary>
		/// Inserts sprites into the currently loaded spriteset before the
		/// currently selected sprite.
		/// Called when the contextmenu's Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInsertSpritesBeforeClick(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Title = "Add 32x40 8-bpp Image file(s)";
				ofd.Filter = "Image files (*.PNG *.GIF *.BMP)|*.PNG;*.GIF;*.BMP|"
						   + "PNG files (*.PNG)|*.PNG|GIF files (*.GIF)|*.GIF|BMP files (*.BMP)|*.BMP|"
						   + "All files (*.*)|*.*";
				ofd.Multiselect = true;

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					InsertSprites(_pnlView.SelectedId, ofd.FileNames);

					_pnlView.SelectedId += ofd.FileNames.Length;
					EditorPanel.Instance.Sprite = _pnlView.Spriteset[_pnlView.SelectedId];

					InsertSpritesFinish();
				}
			}
		}

		/// <summary>
		/// Inserts sprites into the currently loaded spriteset after the
		/// currently selected sprite.
		/// Called when the contextmenu's Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnInsertSpritesAfterClick(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Title = "Add 32x40 8-bpp Image file(s)";
				ofd.Filter = "Image files (*.PNG *.GIF *.BMP)|*.PNG;*.GIF;*.BMP|"
						   + "PNG files (*.PNG)|*.PNG|GIF files (*.GIF)|*.GIF|BMP files (*.BMP)|*.BMP|"
						   + "All files (*.*)|*.*";
				ofd.Multiselect = true;

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					InsertSprites(_pnlView.SelectedId + 1, ofd.FileNames);
					InsertSpritesFinish();
				}
			}
		}

		/// <summary>
		/// Inserts sprites into the currently loaded spriteset starting at a
		/// given Id.
		/// Helper for OnInsertSpriteBeforeClick() and OnInsertSpriteAfterClick().
		/// </summary>
		/// <param name="id">the terrain-id to start inserting at</param>
		/// <param name="files">an array of filenames</param>
		private void InsertSprites(int id, string[] files)
		{
			int length = files.Length;
			for (int i = id; i != _pnlView.Spriteset.Count; ++i)
				_pnlView.Spriteset[i].TerrainId = i + length;

			foreach (string file in files)
			{
				byte[] bindata = File.ReadAllBytes(file);
				Bitmap b = BitmapHandler.LoadBitmap(bindata);

				var sprite = BitmapService.CreateSprite(
													b,
													id,
													Pal,
													0, 0,
													XCImage.SpriteWidth,
													XCImage.SpriteHeight);
				_pnlView.Spriteset.Insert(id++, sprite);
			}
		}

		/// <summary>
		/// Finishes the insert-sprite operation.
		/// </summary>
		private void InsertSpritesFinish()
		{
			OnSpriteClick(null, EventArgs.Empty);

			_pnlView.PrintStatusTotal();

			_pnlView.ForceResize();
			Refresh();
		}

		/// <summary>
		/// Replaces the selected sprite in the collection with a different
		/// sprite.
		/// Called when the contextmenu's Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnReplaceSpriteClick(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Title = "Add 32x40 8-bpp Image file";
				ofd.Filter = "Image files (*.PNG *.GIF *.BMP)|*.PNG;*.GIF;*.BMP|"
						   + "PNG files (*.PNG)|*.PNG|GIF files (*.GIF)|*.GIF|BMP files (*.BMP)|*.BMP|"
						   + "All files (*.*)|*.*";

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					byte[] bindata = File.ReadAllBytes(ofd.FileName);
					Bitmap b = BitmapHandler.LoadBitmap(bindata);

					var sprite = BitmapService.CreateSprite(
														b,
														_pnlView.SelectedId,
														Pal,
														0, 0,
														XCImage.SpriteWidth,
														XCImage.SpriteHeight);
					_pnlView.Spriteset[_pnlView.SelectedId] =
					EditorPanel.Instance.Sprite = sprite;

					Refresh();
				}
			}
		}

		/// <summary>
		/// Moves a sprite one slot to the left.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMoveLeftSpriteClick(object sender, EventArgs e)
		{
			MoveSprite(-1);
		}

		/// <summary>
		/// Moves a sprite one slot to the right.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnMoveRightSpriteClick(object sender, EventArgs e)
		{
			MoveSprite(+1);
		}

		/// <summary>
		/// Moves a sprite to the left or right by one slot.
		/// </summary>
		/// <param name="dir">-1 to move left, +1 to move right</param>
		private void MoveSprite(int dir)
		{
			var sprite = _pnlView.Spriteset[_pnlView.SelectedId];

			_pnlView.Spriteset[_pnlView.SelectedId]       = _pnlView.Spriteset[_pnlView.SelectedId + dir];
			_pnlView.Spriteset[_pnlView.SelectedId + dir] = sprite;

			_pnlView.Spriteset[_pnlView.SelectedId].TerrainId = _pnlView.SelectedId;
			_pnlView.SelectedId += dir;
			_pnlView.Spriteset[_pnlView.SelectedId].TerrainId = _pnlView.SelectedId;

			EditorPanel.Instance.Sprite = _pnlView.Spriteset[_pnlView.SelectedId];

			_pnlView.PrintStatusSpriteSelected();

			OnSpriteClick(null, EventArgs.Empty);
			Refresh();
		}

		/// <summary>
		/// Deletes the selected sprite from the collection.
		/// Called when the contextmenu's Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnDeleteSpriteClick(object sender, EventArgs e)
		{
			_pnlView.Spriteset.RemoveAt(_pnlView.SelectedId);

			for (int i = _pnlView.SelectedId; i != _pnlView.Spriteset.Count; ++i)
				_pnlView.Spriteset[i].TerrainId = i;

			EditorPanel.Instance.Sprite = null;

			_pnlView.SelectedId = -1;
			OnSpriteClick(null, EventArgs.Empty);

			_pnlView.PrintStatusTotal();

			_pnlView.ForceResize();
			Refresh();
		}

		/// <summary>
		/// Deletes the currently selected sprite w/ a keydown event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Delete:
					if (_miDelete.Enabled)
						OnDeleteSpriteClick(null, EventArgs.Empty);
					break;

				case Keys.Enter:
					if (_miEdit.Enabled)
						OnSpriteEditorClick(null, EventArgs.Empty);
					break;
			}
		}

		/// <summary>
		/// Exports the selected sprite in the collection to a PNG file.
		/// Called when the contextmenu's Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnExportSpriteClick(object sender, EventArgs e)
		{
			string digits = String.Empty;

			int count = _pnlView.Spriteset.Count;
			do
			{
				digits += "0";
			}
			while ((count /= 10) != 0);

			var sprite = _pnlView.Spriteset[_pnlView.SelectedId];
			string suffix = String.Format(
										System.Globalization.CultureInfo.InvariantCulture,
										"{0:" + digits + "}",
										_pnlView.SelectedId);

			using (var sfd = new SaveFileDialog())
			{
				sfd.Title      = "Export sprite to 8-bpp PNG file";
				sfd.Filter     = "PNG files (*.PNG)|*.PNG|All files (*.*)|*.*";
				sfd.DefaultExt = "PNG";
				sfd.FileName   = _pnlView.Spriteset.Label + suffix;

				if (sfd.ShowDialog() == DialogResult.OK)
					BitmapService.ExportSprite(sfd.FileName, sprite.Image);
			}
		}

		/// <summary>
		/// Opens a PCK sprite collection.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOpenClick(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Title  = "Select a PCK (terrain or unit) file";
				ofd.Filter = "PCK files (*.PCK)|*.PCK|All files (*.*)|*.*";

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					IsBigobs = false;
					LoadSpriteset(ofd.FileName);
				}
			}
		}

		/// <summary>
		/// Opens a PCK sprite collection.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnOpenBigobsClick(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Title  = "Select a PCK (bigobs) file";
				ofd.Filter = "PCK files (*.PCK)|*.PCK|All files (*.*)|*.*";

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					IsBigobs = true;
					LoadSpriteset(ofd.FileName);
				}
			}
		}

		/// <summary>
		/// Creates a brand sparkling new (blank) PCK sprite collection.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCreateClick(object sender, EventArgs e)
		{
			using (var sfd = new SaveFileDialog())
			{
				sfd.Title      = "Create a PCK file";
				sfd.Filter     = "PCK files (*.PCK)|*.PCK|All files (*.*)|*.*";
				sfd.DefaultExt = "PCK";

				if (sfd.ShowDialog() == DialogResult.OK)
				{
					string pfePck = sfd.FileName;
					string pfeTab = pfePck.Substring(0, pfePck.Length - 4) + SpriteCollection.TabExt;

					using (var bwPck = new BinaryWriter(File.Create(pfePck))) // blank files are ok.
					using (var bwTab = new BinaryWriter(File.Create(pfeTab)))
					{}


					// keep this simple. Assume 2-byte Tab file.

					SpritesetDirectory = Path.GetDirectoryName(pfePck);
					SpritesetLabel     = Path.GetFileNameWithoutExtension(pfePck);


					var pal = DefaultPalette;
					var spriteset = new SpriteCollection(
													SpritesetLabel,
													pal,
													2);

					OnPaletteClick(_paletteItems[pal], EventArgs.Empty);

					_pnlView.Spriteset = spriteset;
					OnSpriteClick(null, EventArgs.Empty);

					Text = "PckView - " + pfePck;
				}
			}
		}

		/// <summary>
		/// Saves all the sprites to the currently loaded PCK+TAB files.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSaveClick(object sender, EventArgs e)
		{
			// http://www.ufopaedia.org/index.php/Image_Formats
			// - that says that all TFTD terrains use 2-byte tab-offsets ...
//			const int tabOffset = 2;
//			if (   Pal.Equals(Palette.TftdBattle)
//				|| Pal.Equals(Palette.TftdGeo)
//				|| Pal.Equals(Palette.TftdGraph)
//				|| Pal.Equals(Palette.TftdResearch))
//			{
//				tabOffset = 4; // NOTE: I don't have TFTD and I do have no clue if this works correctly.
//			}

			BackupSpritesetFiles();

			if (SpriteCollection.SaveSpriteset(
											SpritesetDirectory,
											SpritesetLabel,
											_pnlView.Spriteset,
											((SpriteCollection)_pnlView.Spriteset).TabOffset)) //tabOffset
			{
				SpritesChanged = true; // NOTE: is used by MapView's TileView to flag the Map to reload.
			}
			else
			{
				ShowSaveError();
				RevertFiles();
			}
		}

		/// <summary>
		/// Saves all the sprites to potentially different PCK+TAB files.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSaveAsClick(object sender, EventArgs e)
		{
			using (var sfd = new SaveFileDialog())
			{
				sfd.Title    = "Save as";
				sfd.Filter   = "PCK files (*.PCK)|*.PCK|All files (*.*)|*.*";
				sfd.FileName = SpritesetLabel;

				if (sfd.ShowDialog() == DialogResult.OK)
				{
					string dir  = Path.GetDirectoryName(sfd.FileName);
					string file = Path.GetFileNameWithoutExtension(sfd.FileName);

					bool revertReady; // user requested to save the files to the same filenames.
					if (file.Equals(SpritesetLabel, StringComparison.OrdinalIgnoreCase))
					{
						BackupSpritesetFiles();
						revertReady = true;
					}
					else
						revertReady = false;

					if (SpriteCollection.SaveSpriteset(
													dir,
													file,
													_pnlView.Spriteset,
													((SpriteCollection)_pnlView.Spriteset).TabOffset))
					{
						if (!revertReady) // load the SavedAs files ->
							LoadSpriteset(Path.Combine(dir, file + SpriteCollection.PckExt));

						SpritesChanged = true;	// NOTE: is used by MapView's TileView to flag the Map to reload.
					}							// btw, reload MapView's Map in either case; the new terrain may also be in its Map's terrainset ...
					else
					{
						ShowSaveError();

						if (revertReady)
						{
							RevertFiles();
						}
						else
						{
							File.Delete(Path.Combine(dir, file + SpriteCollection.PckExt));
							File.Delete(Path.Combine(dir, file + SpriteCollection.TabExt));
						}
					}
				}
			}
		}

		/// <summary>
		/// Exports all sprites in the currently loaded spriteset to PNG files.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnExportSpritesClick(object sender, EventArgs e)
		{
			if (_pnlView.Spriteset != null && _pnlView.Spriteset.Count != 0)
			{
				using (var fbd = new FolderBrowserDialog())
				{
					string file = _pnlView.Spriteset.Label.ToUpperInvariant();

					fbd.Description = String.Format(
												System.Globalization.CultureInfo.CurrentCulture,
												"Export spriteset to 8-bpp PNG files"
													+ Environment.NewLine + Environment.NewLine
													+ "\t" + file);

					if (fbd.ShowDialog() == DialogResult.OK)
					{
						string path = fbd.SelectedPath;

						string digits = String.Empty;
						int count = _pnlView.Spriteset.Count;
						do
						{
							digits += "0";
							count /= 10;
						}
						while (count != 0);

						foreach (XCImage sprite in _pnlView.Spriteset)
						{
							string suffix = String.Format(
														System.Globalization.CultureInfo.InvariantCulture,
														"{0:" + digits + "}",
														sprite.TerrainId);
							string fullpath = Path.Combine(path, file + suffix + BitmapService.PngExt);
							BitmapService.ExportSprite(fullpath, sprite.Image);
						}
					}
				}
			}
		}

		/// <summary>
		/// Exports all sprites in the currently loaded spriteset to a PNG
		/// spritesheet file.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnExportSpritesheetClick(object sender, EventArgs e)
		{
			if (_pnlView.Spriteset != null && _pnlView.Spriteset.Count != 0)
			{
				using (var fbd = new FolderBrowserDialog())
				{
					string file = _pnlView.Spriteset.Label.ToUpperInvariant();

					fbd.Description = String.Format(
												System.Globalization.CultureInfo.CurrentCulture,
												"Export spriteset to an 8-bpp PNG spritesheet file"
													+ Environment.NewLine + Environment.NewLine
													+ "\t" + file);

					if (fbd.ShowDialog() == DialogResult.OK)
					{
						string fullpath = Path.Combine(fbd.SelectedPath, file + BitmapService.PngExt);
						if (!File.Exists(fullpath))
						{
							BitmapService.ExportSpritesheet(fullpath, (SpriteCollection)_pnlView.Spriteset, Pal, 8);
						}
						else
							MessageBox.Show(
										this,
										file + BitmapService.PngExt + " already exists.",
										"Error",
										MessageBoxButtons.OK,
										MessageBoxIcon.Error,
										MessageBoxDefaultButton.Button1,
										0);
					}
				}
			}
		}

		/// <summary>
		/// Imports (and replaces) the current spriteset from a BMP spritesheet.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnImportSpritesheetClick(object sender, EventArgs e)
		{
			if (_pnlView.Spriteset != null)
			{
				using (var ofd = new OpenFileDialog())
				{
					ofd.Title = "Import an 8-bpp spritesheet file";
					ofd.Filter = "Image files (*.PNG *.GIF *.BMP)|*.PNG;*.GIF;*.BMP|"
							   + "PNG files (*.PNG)|*.PNG|GIF files (*.GIF)|*.GIF|BMP files (*.BMP)|*.BMP|"
							   + "All files (*.*)|*.*";

					if (ofd.ShowDialog() == DialogResult.OK)
					{
						_pnlView.Spriteset.Clear();

						byte[] bindata = File.ReadAllBytes(ofd.FileName);
						Bitmap b = BitmapHandler.LoadBitmap(bindata);

						SpriteCollectionBase spriteset = BitmapService.CreateSpriteset(
																					b,
																					Pal,
																					XCImage.SpriteWidth,
																					XCImage.SpriteHeight);
						for (int i = 0; i != spriteset.Count; ++i)
							_pnlView.Spriteset.Add(spriteset[i]);

						InsertSpritesFinish();
					}
				}
			}
		}

		/// <summary>
		/// DISABLED. Uses an HQ2x algorithm to display the sprites.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnHq2xClick(object sender, EventArgs e) // disabled w/ Visible=FALSE in the designer.
		{
//			miPaletteMenu.Enabled = false;
//			miBytesMenu.Enabled = false;
//
//			_totalViewPck.Hq2x();
//
//			OnResize(null);
//			Refresh();
		}

		/// <summary>
		/// Closes the app.
		/// Called when the mainmenu's file-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnQuitClick(object sender, EventArgs e)
		{
			OnPckViewFormClosing(null, null);
			Close();
		}

		/// <summary>
		/// Closes the app after a .NET call to close (roughly).
		/// also, Helper for OnQuitClick().
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPckViewFormClosing(object sender, FormClosingEventArgs e)
		{
			SaveWindowMetrics();

			_feditor.ClosePalette();	// these are needed when PckView
			_feditor.Close();			// was opened via MapView.

			if (miBytes.Checked)
				SpriteBytesManager.HideBytesTable(true);
		}

		/// <summary>
		/// Changes the loaded palette.
		/// Called when the mainmenu's palette-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnPaletteClick(object sender, EventArgs e)
		{
			var pal = ((MenuItem)sender).Tag as Palette;
			if (pal != Pal)
			{
				_paletteItems[Pal].Checked = false;

				Pal = pal;
				Pal.SetTransparent(miTransparent.Checked);

				_paletteItems[Pal].Checked = true;

				_pnlView.Spriteset.Pal = Pal;

				var handler = PaletteChangedEvent;
				if (handler != null)
					handler();
			}
		}

		/// <summary>
		/// Toggles transparency of the currently loaded palette.
		/// Called when the mainmenu's transparency-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTransparencyClick(object sender, EventArgs e)
		{
			Pal.SetTransparent(miTransparent.Checked = !miTransparent.Checked);

			_pnlView.Spriteset.Pal = Pal;

			PalettePanel.Instance.PrintStatusPaletteId();	// update the palette-panel's statusbar
															// in case palette-id #0 is currently selected.
			var handler = PaletteChangedEvent;
			if (handler != null)
				handler();
		}

		/// <summary>
		/// Shows a richtextbox with all the bytes of the currently selected
		/// sprite laid out in a fairly readable fashion.
		/// Called when the mainmenu's bytes-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnBytesClick(object sender, EventArgs e)
		{
			if (!miBytes.Checked)
			{
				if (_pnlView.SelectedId != -1)
				{
					miBytes.Checked = true;
					SpriteBytesManager.LoadBytesTable(
												_pnlView.Spriteset[_pnlView.SelectedId],
												BytesClosingCallback);
				}
			}
			else
				SpriteBytesManager.HideBytesTable(miBytes.Checked = false);
		}

		/// <summary>
		/// Callback for ShowBytes().
		/// </summary>
		private void BytesClosingCallback()
		{
			miBytes.Checked = false;
		}

		/// <summary>
		/// Shows the CHM helpfile.
		/// Called when the mainmenu's help-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnHelpClick(object sender, EventArgs e)
		{
			string help = Path.GetDirectoryName(Application.ExecutablePath);
				   help = Path.Combine(help, "MapView.chm");
			Help.ShowHelp(this, "file://" + help, HelpNavigator.Topic, "html/pckview.htm");
		}

		/// <summary>
		/// Shows the about-box.
		/// Called when the mainmenu's help-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAboutClick(object sender, EventArgs e)
		{
			new About().ShowDialog(this);
		}

		/// <summary>
		/// Shows the console.
		/// Called when the mainmenu's help-menu Click event is raised.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConsoleClick(object sender, EventArgs e) // disabled in designer w/ Visible=FALSE
		{
//			if (_fconsole.Visible)
//				_fconsole.BringToFront();
//			else
//				_fconsole.Show();
		}

		private void OnCompareClick(object sender, EventArgs e) // disabled in designer w/ Visible=FALSE
		{
/*			var original = _pnlView.Spriteset; // store original spriteset

			OnOpenClick(null, EventArgs.Empty); // load a second spriteset
			var spriteset = _pnlView.Spriteset;

			_pnlView.Spriteset = original; // revert to original spriteset

			if (Controls.Contains(_pnlView))
			{
				Controls.Remove(_pnlView); // ...

				_tcTabs = new TabControl(); // create tabs
				_tcTabs.Dock = DockStyle.Fill;
				pnlView.Controls.Add(_tcTabs); // add the tabs to the stock panel

				var tabpage = new TabPage(); // create a page
				tabpage.Controls.Add(_pnlView); // add the viewer to the page
				tabpage.Text = "Original";
				_tcTabs.TabPages.Add(tabpage); // add the page to the tab-control

				var viewpanel = new PckViewPanel(); // create a second viewer
				viewpanel.ContextMenu = ViewerContextMenu(); // ...
				viewpanel.Dock = DockStyle.Fill;
				viewpanel.Spriteset = spriteset; // assign the second spriteset to the second viewer

				tabpage = new TabPage(); // create a second page
				tabpage.Controls.Add(viewpanel); // add the second viewer to the second page
				tabpage.Text = "Other";
				_tcTabs.TabPages.Add(tabpage); // add the second page to the tab-control.

				// that sounds like a bad idea. Sounds like plenty of stuff
				// would have to be tested and tracked, or disabled to ensure
				// that things still work correctly when some monkey goes, "Oh
				// cool... watch this!" **sproing***
			} */
		}
		#endregion


		#region Methods
		/// <summary>
		/// Sets the current palette.
		/// NOTE: Called only from TileView to set the palette externally.
		/// </summary>
		/// <param name="palette"></param>
		public void SetPalette(string palette)
		{
			foreach (var pal in _paletteItems.Keys)
			{
				if (pal.Label.Equals(palette))
				{
					OnPaletteClick(_paletteItems[pal], EventArgs.Empty);
					break;
				}
			}
		}

		/// <summary>
		/// Loads a PCK file. May be called from MapView.Forms.MapObservers.TileViews.TileView.OnPckEditorClick()
		/// NOTE: with a string like that you'd think this was .NET itself.
		/// </summary>
		/// <param name="pfePck">fullpath of a PCK file; check existence of the
		/// file before call</param>
		public void LoadSpriteset(string pfePck)
		{
			//LogFile.WriteLine("PckViewForm.LoadSpriteset");
			//LogFile.WriteLine(". " + pfePck);

			SpritesetDirectory = Path.GetDirectoryName(pfePck);
			SpritesetLabel     = Path.GetFileNameWithoutExtension(pfePck);

			string pfeTab = pfePck.Substring(0, pfePck.Length - 4) + SpriteCollection.TabExt;
			if (File.Exists(pfeTab))
			{
				if (IsBigobs) // Bigobs support for XCImage/PckImage ->
					XCImage.SpriteHeight = 48;
				else
					XCImage.SpriteHeight = 40;

				PckViewPanel.TileHeight = XCImage.SpriteHeight + PckViewPanel.SpriteMargin * 2 + 1;


				SpriteCollection spriteset = null;

				using (var fsPck = File.OpenRead(pfePck)) // try 2-byte sprite-offsets in .TAB file
				using (var fsTab = File.OpenRead(pfeTab)) // ie, UFO/TFTD terrain-sprites, UFO unit-sprites
				{
					spriteset = new SpriteCollection(
												fsPck,
												fsTab,
												2,
												Palette.UfoBattle);
				}

				if (spriteset.Borked)
				{
					using (var fsPck = File.OpenRead(pfePck)) // try 4-byte sprite-offsets in .TAB file
					using (var fsTab = File.OpenRead(pfeTab)) // ie, TFTD unit-sprites
					{
						spriteset = new SpriteCollection(
													fsPck,
													fsTab,
													4,
													Palette.TftdBattle);
					}
				}

				if (spriteset != null)
					spriteset.Label = SpritesetLabel;

				OnPaletteClick(
							_paletteItems[DefaultPalette],
							EventArgs.Empty);

				_pnlView.Spriteset = spriteset;

				Text = "PckView - " + pfePck;
			}
			else
			{
//				XConsole.AdZerg("ERROR: tab file does not exist: " + pfeTab);
				MessageBox.Show(
							this,
							"Tab file does not exist"
								+ Environment.NewLine + Environment.NewLine
								+ pfeTab,
							Text,
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
		}

		/// <summary>
		/// Backs up the PCK+TAB files before trying a Save or SaveAs.
		/// NOTE: A possible internal reason that a spriteset is invalid is that
		/// if the total length of its compressed PCK-data exceeds 2^16 bits
		/// (roughly). That is, the TAB file tracks the offsets and needs to
		/// know the total length of the PCK file, but UFO's TAB file stores the
		/// offsets in only 2-byte format (2^16 bits) so the arithmetic explodes
		/// with an overflow as soon as an offset for one of the sprites becomes
		/// too large. (Technically, the total PCK data can exceed 2^16 bits;
		/// but the *start offset* for a sprite cannot -- at least that's how it
		/// works in MapView I/II. Other apps like XCOM, OpenXcom, MCDEdit will
		/// use their own routines.)
		/// NOTE: It appears that TFTD's terrain files suffer this limitation
		/// also.
		/// </summary>
		private void BackupSpritesetFiles()
		{
			Directory.CreateDirectory(SpritesetDirectory); // in case user deleted the dir.

			_pfePck = Path.Combine(SpritesetDirectory, SpritesetLabel + SpriteCollection.PckExt);
			_pfeTab = Path.Combine(SpritesetDirectory, SpritesetLabel + SpriteCollection.TabExt);

			_pfePckOld =
			_pfeTabOld = String.Empty;

			string dirBackup = Path.Combine(SpritesetDirectory, "MV_Backup");

			if (File.Exists(_pfePck))
			{
				Directory.CreateDirectory(dirBackup);

				_pfePckOld = Path.Combine(dirBackup, SpritesetLabel + SpriteCollection.PckExt);
				File.Copy(_pfePck, _pfePckOld, true);
			}

			if (File.Exists(_pfeTab))
			{
				Directory.CreateDirectory(dirBackup);

				_pfeTabOld = Path.Combine(dirBackup, SpritesetLabel + SpriteCollection.TabExt);
				File.Copy(_pfeTab, _pfeTabOld, true);
			}
		}

		/// <summary>
		/// Reverts to the backup files if the TAB-offsets grow too large when
		/// making a Save/SaveAs.
		/// </summary>
		private void RevertFiles()
		{
			if (!String.IsNullOrEmpty(_pfePckOld))
				File.Copy(_pfePckOld, _pfePck, true);

			if (!String.IsNullOrEmpty(_pfeTabOld))
				File.Copy(_pfeTabOld, _pfeTab, true);
		}

		/// <summary>
		/// Shows user an error if saving goes bad due to the 2-byte TAB
		/// limitation.
		/// </summary>
		private void ShowSaveError()
		{
			MessageBox.Show(
						this,
						"The size of the encoded sprite-data has grown too large"
							+ " to be stored accurately by the Tab file. Try"
							+ " deleting sprite(s) or (less effective) using"
							+ " more transparency over all sprites."
							+ Environment.NewLine + Environment.NewLine
							+ "Files have *not* been saved.",
						"Error - excessive pixel data (overflow condition)",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button1,
						0);
		}
		#endregion
	}


	#region Delegates
	internal delegate void PaletteChangedEventHandler();
	#endregion
}
