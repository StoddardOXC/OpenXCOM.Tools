using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using DSShared.Windows;

using XCom;
using XCom.Interfaces;


namespace MapView
{
	/// <summary>
	/// This is the Tileset Editor for MapView ii. It replaces the Paths Editor
	/// of MapView i.
	/// </summary>
	internal sealed partial class MapTreeTilesetInputBox
		:
			Form
	{
		#region Enumerators
		/// <summary>
		/// The possible dialog-types.
		/// </summary>
		internal enum BoxType
		{
			AddTileset,
			EditTileset
		}

		/// <summary>
		/// The possible add-types.
		/// </summary>
		private enum AddType
		{
			MapNone,
			MapExists,
			MapCreate
		}
		#endregion Enumerators


		#region Fields (static)
		private const string AddTileset  = "Add Tileset";
		private const string EditTileset = "Edit Tileset";
		#endregion Fields (static)


		#region Properties (static)
		private static Dictionary<int, Tuple<string,string>> _copiedTerrains
				 = new Dictionary<int, Tuple<string,string>>();

		/// <summary>
		/// Is static to grant access to all instantiations.
		/// </summary>
		private static Dictionary<int, Tuple<string,string>> CopiedTerrains
		{
			get { return _copiedTerrains; }
			set { _copiedTerrains = value; }
		}
		#endregion Properties (static)


		#region Properties
		private BoxType InputBoxType
		{ get; set; }

		private AddType FileAddType
		{ get; set; }

		/// <summary>
		/// Gets/Sets the group-label.
		/// </summary>
		private string Group
		{
			get { return lblGroupCurrent.Text; }
			set { lblGroupCurrent.Text = value; }
		}

		/// <summary>
		/// Gets/Sets the category-label.
		/// </summary>
		private string Category
		{
			get { return lblCategoryCurrent.Text; }
			set { lblCategoryCurrent.Text = value; }
		}

		/// <summary>
		/// Gets/Sets the tileset-label.
		/// </summary>
		internal string Tileset
		{
			get { return tbTileset.Text; }
			private set { tbTileset.Text = value; }
		}

		/// <summary>
		/// Stores the original tileset-label, used only for 'EditTileset' to
		/// check if the label has changed when user clicks Accept.
		/// </summary>
		private string TilesetOriginal
		{ get; set; }

		/// <summary>
		/// Stores the original terrains-list of a tileset, used only for
		/// 'EditTileset' to check if the terrains have changed when user clicks
		/// Accept.
		/// </summary>
		private Dictionary<int, Tuple<string,string>> TerrainsOriginal
		{ get; set; }

		private string _basepath;
		/// <summary>
		/// Gets/Sets the basepath of the Tileset. Setter calls ListTerrains()
		/// which also sets the Descriptor.
		/// </summary>
		private string BasepathTileset
		{
			get { return _basepath; }
			set
			{
				_basepath = value;
				lblPathCurrent.Text = Path.Combine(_basepath, GlobalsXC.MapsDir);

				ListTerrains();
			}
		}

		/// <summary>
		/// The basepath that the user has set in the Configurator.
		/// </summary>
		private string BasepathConfigurator
		{ get; set; }

		private Descriptor Descriptor
		{ get; set; }

		private TileGroup TileGroup
		{ get; set; }

		private string LastTerrainDir
		{ get; set; }

		private bool Inited
		{ get; set; }

		private char[] Invalid
		{ get; set; }

		private bool IsOriginalDescriptor
		{ get; set; }
		#endregion Properties


		#region cTor
		/// <summary>
		/// Creates the Tileset Editor.
		/// </summary>
		/// <param name="boxType"></param>
		/// <param name="labelGroup"></param>
		/// <param name="labelCategory"></param>
		/// <param name="labelTileset"></param>
		internal MapTreeTilesetInputBox(
				BoxType boxType,
				string labelGroup,
				string labelCategory,
				string labelTileset)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("MapTreeTilesetInputBox cTor");
			//LogFile.WriteLine(". labelGroup= " + labelGroup);
			//LogFile.WriteLine(". labelCategory= " + labelCategory);
			//LogFile.WriteLine(". labelTileset= " + labelTileset);

			InitializeComponent();

			var regInfo = new RegistryInfo(RegistryInfo.TilesetEditor, this); // subscribe to Load and Closing events.
			regInfo.RegisterProperties();

			Group    = labelGroup;
			Category = labelCategory;
			Tileset  = labelTileset;

			Inited = true;	// don't let setting 'Tileset' run OnTilesetTextChanged() until
							// after 'BasePath' is initialized. Else ListTerrains() will throwup.

			var invalid = new List<char>();

			char[] chars = Path.GetInvalidFileNameChars();
			for (int i = 0; i != chars.Length; ++i)
				invalid.Add(chars[i]);

			invalid.Add(' '); // no spaces also.
			invalid.Add('.'); // and not dots.
			// TODO: hell i should just check for alpha-numeric and underscore. Old-school style. guaranteed.
			// Although users might not appreciate their old filenames getting too mangled.

			Invalid = invalid.ToArray();
			// TODO: should disallow filenames like 'CON' and 'PRN' etc. also

			SetPasteTip();

			lbTerrainsAllocated.DisplayMember = "Terrain";
			lbTerrainsAvailable.DisplayMember = "Terrain";

			TileGroup = ResourceInfo.TileGroupManager.TileGroups[Group] as TileGroup;

			string keydir = (TileGroup.Pal == Palette.UfoBattle) ? SharedSpace.ResourceDirectoryUfo
																 : SharedSpace.ResourceDirectoryTftd;
			BasepathConfigurator = SharedSpace.Instance.GetShare(keydir);
			//LogFile.WriteLine("BasepathConfig= " + BasepathConfig);
			rb_ConfigBasepath.Checked = true;

			switch (InputBoxType = boxType)
			{
				case BoxType.EditTileset:
				{
					Text = EditTileset;
					lblAddType.Text = "Modify existing tileset";
					lblTilesetCurrent.Text = Tileset;

					btnFindTileset  .Visible =
					btnFindDirectory.Visible =
					btnCreateMap    .Visible = false;

					TilesetOriginal = String.Copy(Tileset);

					var descriptor = TileGroup.Categories[Category][Tileset];

					int records = 0;

					TerrainsOriginal = new Dictionary<int, Tuple<string,string>>();
					for (int i = 0; i != descriptor.Terrains.Count; ++i)
					{
						TerrainsOriginal[i] = new Tuple<string,string>(
																String.Copy(descriptor.Terrains[i].Item1),
																String.Copy(descriptor.Terrains[i].Item2));
						records += descriptor.GetRecordCount(i);
					}
					lblMcdRecords.Text = records + " MCD Records";

					if (records > MapFileService.MAX_MCDRECORDS)
						lblMcdRecords.ForeColor = Color.MediumVioletRed;
					else
						lblMcdRecords.ForeColor = Color.Tan;

					BasepathTileset = descriptor.Basepath;
					break;
				}

				case BoxType.AddTileset:
					Text = AddTileset;
					lblAddType.Text = "Descriptor invalid";

					lblTerrainChanges.Visible =
					lblTilesetCurrent.Visible =
					lblMcdRecords    .Visible = false;

					btnCreateMap   .Enabled =
					btnTerrainCopy .Enabled =
					btnTerrainPaste.Enabled =
					btnTerrainClear.Enabled = false;

					rb_TilesetBasepath.Enabled = false;

					string keyBaseDir = null;
					switch (TileGroup.GroupType)
					{
						case GameType.Ufo:
							keyBaseDir = SharedSpace.ResourceDirectoryUfo;
							break;
						case GameType.Tftd:
							keyBaseDir = SharedSpace.ResourceDirectoryTftd;
							break;
					}
					BasepathTileset = SharedSpace.Instance.GetShare(keyBaseDir);
					break;
			}
			FileAddType = AddType.MapNone;

			tbTileset.Select();
		}
		#endregion cTor


		#region Eventcalls (override)
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			int lbWidth = gbTerrains.Width / 2 - pnlSpacer.Width * 2 / 3; // not sure why 2/3 works.
			lbTerrainsAllocated.Width = lbWidth - SystemInformation.VerticalScrollBarWidth / 2;
			lbTerrainsAvailable.Width = lbWidth + SystemInformation.VerticalScrollBarWidth / 2;

			lblAllocated.Left = lbTerrainsAllocated.Right - lblAllocated.Width - 5;
			lblAvailable.Left = lbTerrainsAvailable.Left;

			pnlSpacer.Left = gbTerrains.Width / 2 - pnlSpacer.Width / 2 - SystemInformation.VerticalScrollBarWidth / 2;
		}

		/// <summary>
		/// Checks if the box has been closed by Cancel/exit click and if so do
		/// terrain verifications.
		/// @note Terrains get changed on-the-fly and do not require an Accept
		/// click. But the Map needs to be reloaded when things go back to
		/// OnAdd/EditTilesetClick() in XCMainWindow.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK
				&& IsOriginalDescriptor)
			{
				if (Descriptor.Terrains.Count == 0)
				{
					e.Cancel = true;
					ShowErrorDialog("The Map must have at least one terrain allocated.");
				}
				else if (IsTerrainsChanged(TerrainsOriginal, Descriptor.Terrains))
				{
					DialogResult = DialogResult.OK; // force reload of the Tileset
				}
			}
			base.OnFormClosing(e);
		}
		#endregion Eventcalls (override)


		#region Eventcalls
		/// <summary>
		/// Opens a find directory dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFindDirectoryClick(object sender, EventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				fbd.SelectedPath = BasepathTileset;
				fbd.Description = String.Format(
											System.Globalization.CultureInfo.CurrentCulture,
											"Browse to a basepath folder. A valid basepath folder"
												+ " has the subfolders MAPS and ROUTES.");

				if (fbd.ShowDialog() == DialogResult.OK)
				{
					BasepathTileset = fbd.SelectedPath;
					OnTilesetTextboxChanged(null, EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Opens a find file dialog.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFindTilesetClick(object sender, EventArgs e)
		{
			using (var ofd = new OpenFileDialog())
			{
				ofd.Filter = "Map Files (*.map)|*.map|All Files (*.*)|*.*";
				ofd.Title  = "Select a Map file";
				ofd.InitialDirectory = Path.Combine(BasepathTileset, GlobalsXC.MapsDir);
				if (!Directory.Exists(ofd.InitialDirectory))
					ofd.InitialDirectory = BasepathTileset;

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					string pfeMap = ofd.FileName;

					string basepath = Path.GetDirectoryName(pfeMap);
					int pos = basepath.LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase);
					BasepathTileset = (pos != -1) ? basepath.Substring(0, pos)
												  : basepath;

					Tileset = Path.GetFileNameWithoutExtension(pfeMap);
					OnTilesetTextboxChanged(null, EventArgs.Empty);	// NOTE: This will fire OnTilesetLabelChanged() twice usually but
				}													// has to be here in case the basepath changed but the label didn't.
			}
		}

		/// <summary>
		/// Refreshes the terrains-lists and ensures that the tileset-label is
		/// valid to be a Mapfile.
		/// NOTE: The textbox forces UpperCASE.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTilesetTextboxChanged(object sender, EventArgs e)
		{
			if (Inited) // do not run until the textbox has been initialized.
			{
				if (!ValidateCharacters(Tileset))
				{
					ShowErrorDialog("Characters detected that are not allowed.");

					Tileset = InvalidateCharacters(Tileset); // recurse function after removing invalid chars.
					tbTileset.SelectionStart = tbTileset.TextLength;
				}
				else if (InputBoxType == BoxType.AddTileset)
				{
					ListTerrains();

					if (String.IsNullOrEmpty(Tileset))
					{
						btnCreateMap.Enabled = false;

						lblAddType.Text = "Descriptor invalid";
						FileAddType = AddType.MapNone;
					}
					else if (Descriptor == null || Descriptor.Label != Tileset)
					{
						btnCreateMap.Enabled = true;

						lblAddType.Text = "Create";
						FileAddType = AddType.MapNone;
					}
					else
					{
						btnCreateMap.Enabled = false;

						if (MapfileExists(Tileset))
						{
							lblAddType.Text = "Add using existing Map file";
							FileAddType = AddType.MapExists;
						}
						else
						{
							lblAddType.Text = "Add by creating a new Map file";
							FileAddType = AddType.MapCreate;
						}
					}
				}
			}
		}

		/// <summary>
		/// Lists the allocated and available terrains in their list-boxes. This
		/// function also sets the current Descriptor, which is essential to
		/// listing the terrains as well as to the proper functioning of various
		/// control-buttons and routines in this object.
		/// </summary>
		private void ListTerrains()
		{
			lbl_AllocatedPath.Text = String.Empty;

			btnMoveUp   .Enabled =
			btnMoveDown .Enabled =
			btnMoveRight.Enabled =
			btnMoveLeft .Enabled = false;

			lbTerrainsAllocated.BeginUpdate();
			lbTerrainsAvailable.BeginUpdate();
			lbTerrainsAllocated.Items.Clear();
			lbTerrainsAvailable.Items.Clear();


			IsOriginalDescriptor = false;

			switch (InputBoxType)
			{
				case BoxType.EditTileset:
					if (Tileset == TilesetOriginal										// use original Descriptor if label hasn't changed
						|| (!IsTilesetCategorized(Tileset) && !MapfileExists(Tileset)))	// or if (label is not in Category AND its Mapfile doesn't exist in the current tileset's basepath on disk)
					{																	// -> spellcast: ConfuseOpponent(OBJECT_SELF)
						Descriptor = TileGroup.Categories[Category][TilesetOriginal];
						IsOriginalDescriptor = true; // is Original or is Original w/ modified label
					}
					else
						Descriptor = null;
					break;

				case BoxType.AddTileset:
					if (IsTilesetCategorized(Tileset))
					{
						Descriptor = null;
					}
					break;
			}


			int records = 0;

			if (Descriptor != null)
			{
				for (int i = 0; i != Descriptor.Terrains.Count; ++i)
				{
					lbTerrainsAllocated.Items.Add(new tle(Descriptor.Terrains[i]));
					records += Descriptor.GetRecordCount(i);
				}
			}
			lblMcdRecords.Text = records + " MCD Records";

			if (records > MapFileService.MAX_MCDRECORDS)
				lblMcdRecords.ForeColor = Color.MediumVioletRed;
			else
				lblMcdRecords.ForeColor = Color.Tan;

			btnTerrainClear.Enabled =
			btnTerrainCopy .Enabled = (Descriptor != null && Descriptor.Terrains.Count != 0);
			btnTerrainPaste.Enabled = (Descriptor != null && CopiedTerrains.Count != 0);


			// Get the text of 'tbBasepath' (to reflect the currently selected radio-button)
			string dirTerrain = tbTerrainPath.Text;
			//LogFile.WriteLine("dirTerrain= " + dirTerrain);

			if (Directory.Exists(dirTerrain))
			{
				var terrains = Directory.GetFiles(
												dirTerrain,
												Globals.ALLFILES,
												SearchOption.TopDirectoryOnly)
											.Where(file => file.EndsWith(
																	GlobalsXC.PckExt,
																	StringComparison.OrdinalIgnoreCase));
				if (terrains.Any())
				{
					string basepath;
					if (rb_CustomBasepath.Checked) // get terrainlist from a custom basepath
					{
						// delete TERRAIN from the end of 'tbTerrainPath.Text'
						if (dirTerrain[dirTerrain.Length - 1] == Path.DirectorySeparatorChar) // TODO: Should check for AltDirectorySeparatorChar also.
						{
							dirTerrain = dirTerrain.Substring(0, dirTerrain.Length - 1);
						}

						if (dirTerrain.EndsWith(GlobalsXC.TerrainDir, StringComparison.Ordinal))
						{
							dirTerrain = dirTerrain.Substring(0, dirTerrain.Length - GlobalsXC.TerrainDir.Length - 1);
						}

						basepath = dirTerrain; // user-specified basepath
					}
					else if (rb_TilesetBasepath.Checked && Descriptor != null) // get terrainlist from the Descriptor's basepath
					{
						basepath = GlobalsXC.BASEPATH;
					}
					else //if (rb_ConfigBasepath.Checked) // get terrainlist from the Configurator's basepath
					{
						basepath = String.Empty;
					}

					string terr;
					foreach (var terrain in terrains)
					{
						terr = Path.GetFileNameWithoutExtension(terrain);

						if ((Descriptor == null || !IsTerrainInDescriptor(terr, basepath))
							&& !terr.Equals("BLANKS", StringComparison.OrdinalIgnoreCase))
						{
							lbTerrainsAvailable.Items.Add(new tle(new Tuple<string,string>(terr, basepath)));
						}
					}
				}
			}
			lbTerrainsAllocated.EndUpdate();
			lbTerrainsAvailable.EndUpdate();
		}


		/// <summary>
		/// Creates a tileset as a valid Descriptor. This is allowed iff a
		/// tileset is being Added/Created; it's disallowed if a tileset is only
		/// being Edited.
		/// NOTE: A Map's descriptor must be created/valid before terrains can
		/// be added.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCreateDescriptorClick(object sender, EventArgs e)
		{
			if (!IsTilesetCategorized(Tileset))
			{
				Descriptor = new Descriptor(		// be careful with that; it isn't being deleted if user clicks Cancel
										Tileset,	// or chooses instead to create yet another descriptor.
										new Dictionary<int, Tuple<string, string>>(),
										BasepathTileset,
										TileGroup.Pal);

				if (MapfileExists(Tileset))
				{
					lblAddType.Text = "Add using existing Map file";
					FileAddType = AddType.MapExists;
				}
				else
				{
					lblAddType.Text = "Add by creating a new Map file";
					FileAddType = AddType.MapCreate;
				}

				btnCreateMap.Enabled = false;
				ListTerrains();


				lblTilesetCurrent.Text = Tileset;

				lblTerrainChanges.Visible =
				lblTilesetCurrent.Visible =
				lblMcdRecords    .Visible = true;

				rb_TilesetBasepath.Enabled = true;
			}
			else
				ShowErrorDialog("The label is already assigned to a different tileset.");
		}

		/// <summary>
		/// If this inputbox is type AddTileset, the accept click must check to
		/// see if a descriptor has been created already with the CreateMap
		/// button first.
		/// 
		/// If this inputbox is type EditTileset, the accept click will create a
		/// descriptor if the tileset-label changed and delete the old
		/// descriptor, and add the new one to the current tilegroup/category.
		/// If the tileset-label didn't change, nothing more need be done since
		/// any terrains that were changed have already been changed by changes
		/// to the Allocated/Available listboxes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAcceptClick(object sender, EventArgs e)
		{
			switch (InputBoxType)
			{
				case BoxType.EditTileset:
					if (String.IsNullOrEmpty(Tileset))
					{
						ShowErrorDialog("The Map label cannot be blank.");
						tbTileset.Select();
					}
					else if (lbTerrainsAllocated.Items.Count == 0)
					{
						ShowErrorDialog("The Map must have at least one terrain allocated.");
					}
					else
					{
						if (Tileset == TilesetOriginal) // label didn't change; check if terrains changed ->
						{
							if (!IsTerrainsChanged(TerrainsOriginal, Descriptor.Terrains))
							{
								ShowInfoDialog("No changes were made.");
							}
							else
								DialogResult = DialogResult.OK;

							// NOTE: a Save of Map-file is *not* required here.
						}
						else // label changed; rewrite the descriptor ->
						{
							// NOTE: user cannot edit a Map-label to be another already existing file.
							// There are other ways to do that: either let the user delete the target-
							// Map-file from his/her disk, or better click Edit on *that* tileset.
							// NOTE: however, if while editing a tileset the user browses to another
							// tileset and edits that tileset's terrains, the changes are effective.

							if (MapfileExists(Tileset))
							{
								// TODO: Ask user if he/she wants to overwrite the Map-file.
								ShowErrorDialog("The Map file already exists on disk. The Tileset Editor is"
												+ " not sophisticated enough to deal with this eventuality."
												+ " Either edit that Map directly if it's already in the Maptree,"
												+ " or use Add Tileset to make it editable, or as a last"
												+ " resort delete it from your disk."
												+ Environment.NewLine + Environment.NewLine
												+ GetFullpathMapfile(Tileset));
							}
							else if (IsTilesetCategorized(Tileset))
							{
								ShowErrorDialog("The tileset already exists in the Maptree."
											+ Environment.NewLine + Environment.NewLine
											+ Tileset
											+ Environment.NewLine + Environment.NewLine
											+ "Options are to edit that one, delete that one,"
											+ " or choose a different label for this one.");
							}
							else
							{
								string pfeMap    = GetFullpathMapfile(Tileset);
								string pfeMapPre = GetFullpathMapfile(TilesetOriginal);

								File.Move(pfeMapPre, pfeMap);	// NOTE: This has to happen now because once the MapTree node
																// is selected it will try to load the .MAP file etc.

								if (File.Exists(pfeMap))		// NOTE: do *not* alter the descriptor if File.Move() went bork.
								{								// This is likely redundant: File.Move() should throw.
									string pfeRoutes    = GetFullpathRoutefile(Tileset);
									string pfeRoutesPre = GetFullpathRoutefile(TilesetOriginal);

									File.Move(pfeRoutesPre, pfeRoutes);

									Descriptor = new Descriptor(
															Tileset,
															TileGroup.Categories[Category][TilesetOriginal].Terrains,
															BasepathTileset,
															TileGroup.Pal);
									TileGroup.AddTileset(Descriptor, Category);			// NOTE: This could be done on return to XCMainWindow.OnEditTilesetClick()
																						// but then 'Descriptor' would have to be internal.
									TileGroup.DeleteTileset(TilesetOriginal, Category);	// NOTE: This could be done on return to XCMainWindow.OnEditTilesetClick()
																						// but then 'TilesetOriginal' would have to be internal.

									DialogResult = DialogResult.OK; // reload the Tileset in MainView.
								}
							}
						}
					}
					break;

				case BoxType.AddTileset:
					if (String.IsNullOrEmpty(Tileset))						// NOTE: The tileset-label should already have been
					{														// checked for validity by here before the Create button.
						ShowErrorDialog("The Map label cannot be blank.");	// But these handle the case when user immediately clicks the Ok button.
						tbTileset.Select();									// TODO: So disable the Ok button, unless a descriptor is valid.
					}
					else if (lbTerrainsAllocated.Items.Count == 0)
					{
						ShowErrorDialog("The Map must have at least one terrain allocated.");
					}
					else
					{
						switch (FileAddType)
						{
							case AddType.MapExists:
								TileGroup.AddTileset(Descriptor, Category);
								DialogResult = DialogResult.OK;
								break;

							case AddType.MapCreate:
								string pfeMap    = GetFullpathMapfile(Tileset);
								string pfeRoutes = GetFullpathRoutefile(Tileset);

								Directory.CreateDirectory(Path.GetDirectoryName(pfeRoutes));
								using (var fs = File.Create(pfeRoutes)) // create a blank Route-file and release its handle.
								{}

								Directory.CreateDirectory(Path.GetDirectoryName(pfeMap));
								using (var fs = File.Create(pfeMap))	// create the Map-file and release its handle.
								{										// NOTE: This has to happen now because once the MapTree node
									MapFileChild.CreateMap(				// is selected it will try to load the .MAP file etc.
														fs,
														10,10,1); // <- default new Map size
								}

								if (File.Exists(pfeMap) && File.Exists(pfeRoutes)) // NOTE: The descriptor has already been created with the Create descriptor button.
								{
									TileGroup.AddTileset(Descriptor, Category);
									DialogResult = DialogResult.OK; // load the Tileset in MainView.
								}
								break;
						}
					}
					break;
			}
		}


		private void OnTerrainLeftClick(object sender, EventArgs e)
		{
			if (!XCMainWindow.Instance.MaptreeChanged && InputBoxType == BoxType.EditTileset)
				 XCMainWindow.Instance.MaptreeChanged = true;

			int sel = lbTerrainsAvailable.SelectedIndex;

			var it = lbTerrainsAvailable.SelectedItem as tle;
			var terrain = new Tuple<string,string>(
											String.Copy(it.Terrain),
											String.Copy(it.Basepath));

			int id = Descriptor.Terrains.Count;
			Descriptor.Terrains[id] = terrain;

			ListTerrains();

			int count = lbTerrainsAvailable.Items.Count;
			if (sel == count)
				sel = count - 1;

			lbTerrainsAvailable.SelectedIndex = sel;
			lbTerrainsAvailable.Select();

			lbTerrainsAllocated.SelectedIndex = id;
		}

		private void OnTerrainRightClick(object sender, EventArgs e)
		{
			if (!XCMainWindow.Instance.MaptreeChanged && InputBoxType == BoxType.EditTileset)
				 XCMainWindow.Instance.MaptreeChanged = true;

			int sel = lbTerrainsAvailable.SelectedIndex;

			int id = lbTerrainsAllocated.SelectedIndex;
			for (int i = id; i != Descriptor.Terrains.Count - 1; ++i)
			{
				Descriptor.Terrains[i] = Descriptor.Terrains[i + 1];
			}
			Descriptor.Terrains.Remove(Descriptor.Terrains.Count - 1);

			ListTerrains();

			lbTerrainsAvailable.SelectedIndex = sel;
			lbTerrainsAvailable.Select();
		}

		private void OnTerrainUpClick(object sender, EventArgs e)
		{
			StepTerrainEntry(-1);
		}

		private void OnTerrainDownClick(object sender, EventArgs e)
		{
			StepTerrainEntry(+1);
		}

		private void StepTerrainEntry(int dir)
		{
			if (!XCMainWindow.Instance.MaptreeChanged && InputBoxType == BoxType.EditTileset)
				 XCMainWindow.Instance.MaptreeChanged = true;

			var terrains = Descriptor.Terrains;

			int id  = lbTerrainsAllocated.SelectedIndex;
			int id_ = id + dir;

			var t = terrains[id_];
			terrains[id_] = terrains[id];
			terrains[id]  = t;

			lbTerrainsAllocated.BeginUpdate();
			lbTerrainsAllocated.Items.Clear();
			for (id = 0; id != terrains.Count; ++id)
			{
				lbTerrainsAllocated.Items.Add(new tle(terrains[id]));
			}
			lbTerrainsAllocated.EndUpdate();

			lbTerrainsAllocated.SelectedIndex = id_;
			lbTerrainsAllocated.Select();
		}

		private void OnTerrainCopyClick(object sender, EventArgs e)
		{
			CopiedTerrains.Clear();

			for (int i = 0; i != Descriptor.Terrains.Count; ++i)
			{
				CopiedTerrains[i] = CloneTerrain(Descriptor.Terrains[i]);
			}

			SetPasteTip();
			btnTerrainPaste.Enabled = true;
			lbTerrainsAvailable.Select();
		}

		private void OnTerrainPasteClick(object sender, EventArgs e)
		{
			if (!XCMainWindow.Instance.MaptreeChanged && InputBoxType == BoxType.EditTileset)
				 XCMainWindow.Instance.MaptreeChanged = true;

			Descriptor.Terrains.Clear();

			for (int i = 0; i != CopiedTerrains.Count; ++i)
			{
				Descriptor.Terrains[i] = CloneTerrain(CopiedTerrains[i]);
			}

			ListTerrains();
			lbTerrainsAvailable.Select();
		}

		private void OnTerrainClearClick(object sender, EventArgs e)
		{
			if (!XCMainWindow.Instance.MaptreeChanged && InputBoxType == BoxType.EditTileset)
				 XCMainWindow.Instance.MaptreeChanged = true;

			Descriptor.Terrains.Clear();
			ListTerrains();
			lbTerrainsAvailable.Select();
		}

		private void SetPasteTip()
		{
			string tipPaste = String.Empty;
			for (int i = 0; i != CopiedTerrains.Count; ++i)
			{
				if (!String.IsNullOrEmpty(tipPaste))
					tipPaste += Environment.NewLine;

				tipPaste += CopiedTerrains[i].Item1;
			}
			toolTip1.SetToolTip(btnTerrainPaste, tipPaste);
		}

		/// <summary>
		/// @note Does not fire when a selected item is removed; the index does
		/// not change. Read: it should. But it doesn't.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAllocatedIndexChanged(object sender, EventArgs e)
		{
			int id = lbTerrainsAllocated.SelectedIndex;
			if (id != -1)
			{
				string basepath = ((tle)lbTerrainsAllocated.Items[id]).Basepath;
				if (String.IsNullOrEmpty(basepath))
				{
					lbl_AllocatedPath.Text = "in Configurator basepath";
				}
				else if (basepath == GlobalsXC.BASEPATH)
				{
					lbl_AllocatedPath.Text = "in Tileset basepath";
				}
				else
					lbl_AllocatedPath.Text = basepath + Path.DirectorySeparatorChar + GlobalsXC.TerrainDir;

				btnMoveRight.Enabled = true;

				if (Descriptor != null && Descriptor.Terrains.Count > 1)
				{
					btnMoveUp  .Enabled = (id != 0);
					btnMoveDown.Enabled = (id != Descriptor.Terrains.Count - 1);
				}
			}
		}

		private void OnAvailableIndexChanged(object sender, EventArgs e)
		{
			btnMoveLeft.Enabled = lbTerrainsAvailable.SelectedIndex != -1
							   && Descriptor != null;
		}


		/// <summary>
		/// Handles changes to the Terrains-basepath radio-buttons.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRadioTerrainChanged(object sender, EventArgs e)
		{
			_bypassListTerrains = true;

			int sel = lbTerrainsAllocated.SelectedIndex;

			if (sender == rb_CustomBasepath)
			{
				if (String.IsNullOrEmpty(LastTerrainDir))
					LastTerrainDir = BasepathConfigurator;

				tbTerrainPath.Text = Path.Combine(LastTerrainDir, GlobalsXC.TerrainDir);
				tbTerrainPath.ReadOnly = false;

				btnFindBasepath.Visible = true;
			}
			else if (sender == rb_TilesetBasepath)
			{
				tbTerrainPath.Text = Path.Combine(Descriptor.Basepath, GlobalsXC.TerrainDir);
				tbTerrainPath.ReadOnly = true;

				btnFindBasepath.Visible = false;
			}
			else //if (sender == rb_ConfigBasepath)
			{
				tbTerrainPath.Text = Path.Combine(BasepathConfigurator, GlobalsXC.TerrainDir);
				tbTerrainPath.ReadOnly = true;

				btnFindBasepath.Visible = false;
			}

			ListTerrains();					// -> have to do that so that user can switch a terrain's path-type even if
			_bypassListTerrains = false;	// their paths are identical (ie. when 'tbTerrainPath.Text' does not change).

			lbTerrainsAllocated.SelectedIndex = sel;
//			lbTerrainsAvailable.Select();
		}

		private bool _bypassListTerrains;

		private void OnTerrainPathChanged(object sender, EventArgs e)
		{
			if (!_bypassListTerrains)
				ListTerrains();
		}

		/// <summary>
		/// Opens a find directory dialog for Terrains.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFindTerrainClick(object sender, EventArgs e)
		{
			using (var fbd = new FolderBrowserDialog())
			{
				fbd.SelectedPath = LastTerrainDir;
				fbd.Description = String.Format(
											System.Globalization.CultureInfo.CurrentCulture,
											"Browse to a basepath folder. A valid basepath folder"
												+ " has the subfolder TERRAIN.");

				if (fbd.ShowDialog() == DialogResult.OK)
				{
					string path = fbd.SelectedPath;

					LastTerrainDir = path;

					string pathTerrain = Path.Combine(path, GlobalsXC.TerrainDir);
					tbTerrainPath.Text = pathTerrain;
					if (!Directory.Exists(pathTerrain)) // check that a subfolder is labeled "TERRAIN"
					{
						ShowWarningDialog("The subfolder TERRAIN does not exist.");
					}
				}
			}
			lbTerrainsAvailable.Select();
		}
		#endregion Eventcalls


		#region Methods
		/// <summary>
		/// Checks that a string can be a valid filename for Windows OS.
		/// NOTE: Check that 'chars' is not null or blank before call.
		/// </summary>
		/// <param name="chars"></param>
		/// <returns></returns>
		private bool ValidateCharacters(string chars)
		{
			return (chars.IndexOfAny(Invalid) == -1);
		}

		/// <summary>
		/// Removes invalid characters from a given string.
		/// </summary>
		/// <param name="chars"></param>
		/// <returns></returns>
		private string InvalidateCharacters(string chars)
		{
			int pos = -1;
			while ((pos = chars.IndexOfAny(Invalid)) != -1)
				chars = chars.Remove(pos, 1);

			return chars;
		}

		/// <summary>
		/// Gets the fullpath for a Map-file.
		/// </summary>
		/// <param name="labelMap"></param>
		/// <returns></returns>
		private string GetFullpathMapfile(string labelMap)
		{
			string dirMaps = Path.Combine(BasepathTileset, GlobalsXC.MapsDir);
			return Path.Combine(dirMaps, labelMap + GlobalsXC.MapExt);
		}

		/// <summary>
		/// Gets the fullpath for a Route-file.
		/// </summary>
		/// <param name="labelRoutes"></param>
		/// <returns></returns>
		private string GetFullpathRoutefile(string labelRoutes)
		{
			string dirRoutes = Path.Combine(BasepathTileset, GlobalsXC.RoutesDir);
			return Path.Combine(dirRoutes, labelRoutes + GlobalsXC.RouteExt);
		}

		/// <summary>
		/// Checks if a Map-file w/ label exists in the current basepath
		/// directory.
		/// </summary>
		/// <param name="labelMap">the label w/out extension of a Map-file to check for</param>
		/// <returns>true if the Map-file already exists on the hardrive</returns>
		private bool MapfileExists(string labelMap)
		{
			string pfeMap = null;
			if (!String.IsNullOrEmpty(labelMap))
			{
				pfeMap = GetFullpathMapfile(labelMap);
			}
			return (pfeMap != null && File.Exists(pfeMap));
		}

/*		/// <summary>
		/// Checks if a tileset w/ label exists anywhere in the TileGroups.
		/// </summary>
		/// <param name="labelMap">the label of a tileset to check for</param>
		/// <returns>true if a tileset w/ label already exists in the Groups</returns>
		private static bool IsTilesetGrouped(string labelMap)
		{
			foreach (var group in ResourceInfo.TileGroupManager.TileGroups)
			foreach (var category in group.Value.Categories)
			foreach (var descriptor in category.Value.Values)
			{
				if (descriptor.Label == labelMap)
					return true;
			}
			return false;
		} */

		/// <summary>
		/// Checks if a specified tileset-label exists in the current tileset's
		/// Category.
		/// @note A label shall be unique in its Category.
		/// </summary>
		/// <param name="labelMap">the tileset-label to check</param>
		/// <returns>true if the tileset-label already exists in the current
		/// tileset's Category</returns>
		private bool IsTilesetCategorized(string labelMap)
		{
			var category = ResourceInfo.TileGroupManager.TileGroups[Group].Categories[Category];
			return category.ContainsKey(labelMap);
		}

		/// <summary>
		/// Checks if two terrains-lists are (not) equivalent.
		/// </summary>
		/// <param name="a">first terrains-list</param>
		/// <param name="b">second terrains-list</param>
		/// <returns>true if the specified terrains-lists are different</returns>
		private static bool IsTerrainsChanged(
				IDictionary<int, Tuple<string, string>> a,
				IDictionary<int, Tuple<string, string>> b)
		{
			if (a.Count != b.Count)
				return true;

			for (int i = 0; i != a.Count; ++i)
			{
				if (   a[i].Item1 != b[i].Item1
					|| a[i].Item2 != b[i].Item2)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if a specified terrain is listed in the Allocated listbox and
		/// therefore should be bypassed by the Available listbox.
		/// </summary>
		/// <param name="terrain">terrain-file w/out extension</param>
		/// <param name="dirTerrain">pass in a blank string if Config-basepath
		/// is checked or "basepath" if Tileset-basepath is checked; else pass
		/// in the basepath of the TERRAIN directory</param>
		/// <returns></returns>
		private bool IsTerrainInDescriptor(string terrain, string dirTerrain)
		{
			for (int i = 0; i != Descriptor.Terrains.Count; ++i)
			{
				if (   Descriptor.Terrains[i].Item1 == terrain
					&& Descriptor.Terrains[i].Item2 == dirTerrain)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Deep clones a given terrain-tuple. jic.
		/// </summary>
		/// <param name="terrain">a terrain-tuple</param>
		/// <returns>deep clone of the terrain-tuple</returns>
		private Tuple<string,string> CloneTerrain(Tuple<string,string> terrain)
		{
			return new Tuple<string,string>(
										String.Copy(terrain.Item1),
										String.Copy(terrain.Item2));
		}


		/// <summary>
		/// Wrapper for MessageBox.Show().
		/// </summary>
		/// <param name="error">the error string to show</param>
		private void ShowErrorDialog(string error)
		{
			MessageBox.Show(
						this,
						error,
						"Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button1,
						0);
		}

		/// <summary>
		/// Wrapper for MessageBox.Show().
		/// </summary>
		/// <param name="warn">the warn string to show</param>
		private void ShowWarningDialog(string warn)
		{
			MessageBox.Show(
						this,
						warn,
						"Warning",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning,
						MessageBoxDefaultButton.Button1,
						0);
		}

		/// <summary>
		/// Wrapper for MessageBox.Show().
		/// </summary>
		/// <param name="info">the info string to show</param>
		private void ShowInfoDialog(string info)
		{
			MessageBox.Show(
						this,
						info,
						"Notice",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information,
						MessageBoxDefaultButton.Button1,
						0);
		}
		#endregion Methods
	}


//		/// <summary>
//		/// Calls OnCreateTilesetClick() if Enter is key-upped in the
//		/// tileset-label textbox.
//		/// NOTE: KeyDown event doesn't work for an Enter key. Be careful 'cause
//		/// the keydown gets intercepted by the form itself.
//		/// TODO: Bypass triggering OnAcceptClick() ... was raised by tbTileset.KeyUp event.
//		/// </summary>
//		/// <param name="sender"></param>
//		/// <param name="e"></param>
//		private void OnTilesetKeyUp(object sender, KeyEventArgs e)
//		{
//			//LogFile.WriteLine("");
//			//LogFile.WriteLine("OnTilesetLabelKeyUp");
//
//			if (InputBoxType == BoxType.AddTileset	// NOTE: have to remove this. If a user enters an invalid char in the label
//				&& btnCreateMap.Enabled				// then uses Enter to get rid of the error-popup, the KeyDown dismisses the
//				&& e.KeyCode == Keys.Enter)			// error but then the KeyUp will instantiate a descriptor ....
//			{										// Am sick of fighting with WinForms in an already complicated class like this.
//				OnCreateDescriptorClick(null, EventArgs.Empty);
//			}
//		}

// was OnCreateDescriptorClick() checks ->>
//			if (String.IsNullOrEmpty(Tileset)) // TODO: this should be checked before getting here.
//			{
//				LogFile.WriteLine(". The Map label cannot be blank.");
//				ShowErrorDialog("The Map label cannot be blank.");
//
//				tbTileset.Select();
//			}
//			else if (!ValidateCharacters(Tileset)) // TODO: this should be checked before getting here.
//			{
//				LogFile.WriteLine(". The Map label contains illegal characters.");
//				ShowErrorDialog("The Map label contains illegal characters.");
//
//				tbTileset.Select();
//				tbTileset.SelectionStart = tbTileset.TextLength;
//			}
//			else if (MapFileExists(Tileset))	// TODO: check to ensure that this Create function (and KeyUp-Enter events)
//			{									// cannot be called if a descriptor and/or a Map-file already exist.
//				LogFile.WriteLine(". The Map file already exists."); // NOTE: Don't worry about it yet; this does not create a Map-file.
//				ShowErrorDialog("The Map file already exists.");
//			}
//			else if (TileGroup.Categories[Category].ContainsKey(Tileset))	// safety -> TODO: the create map and tileset keyup events should
//			{																// be disabled if a Descriptor w/ tileset-label already exists
//				LogFile.WriteLine(". The Tileset label already exists.");
//				ShowErrorDialog("The Tileset label already exists.");
//			}
//			else
//			{}

//		// https://stackoverflow.com/questions/62771/how-do-i-check-if-a-given-string-is-a-legal-valid-file-name-under-windows#answer-62888
//		You may use any character in the current code page (Unicode/ANSI above 127), except:
//
//		< > : " / \ | ? *
//		Characters whose integer representations are 0-31 (less than ASCII space)
//		Any other character that the target file system does not allow (say, trailing periods or spaces)
//		Any of the DOS names: CON, PRN, AUX, NUL, COM1, COM2, COM3, COM4,
//		COM5, COM6, COM7, COM8, COM9, LPT1, LPT2, LPT3, LPT4, LPT5, LPT6,
//		LPT7, LPT8, LPT9 (and avoid AUX.txt, etc) and CLOCK$
//		The file name is all periods
//
//		Some optional things to check:
//
//		File paths (including the file name) may not have more than 260 characters (that don't use the \?\ prefix)
//		Unicode file paths (including the file name) with more than 32,000 characters when using \?\
//		(note that prefix may expand directory components and cause it to overflow the 32,000 limit)
//
//		also: https://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
//
//		also: https://stackoverflow.com/questions/422090/in-c-sharp-check-that-filename-is-possibly-valid-not-that-it-exists
//
//		Naming Files, Paths, and Namespaces
//		https://msdn.microsoft.com/en-us/library/aa365247(VS.85).aspx


	/// <summary>
	/// An object for parsing out a terrain-string to show in the terrain-
	/// listboxes while retaining a reference to its terrain-tuple.
	/// </summary>
	internal sealed class tle // TerrainListEntry
	{
		internal string Terrain
		{ get; private set; }

		internal string Basepath
		{ get; private set; }

		internal tle(Tuple<string,string> terrain)
		{
			Terrain  = terrain.Item1;
			Basepath = terrain.Item2;
		}

		/// <summary>
		/// Required for
		/// lbTerrainsAllocated.DisplayMember = "Terrain";
		/// lbTerrainsAvailable.DisplayMember = "Terrain";
		/// to work correctly.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Terrain;
		}
	}
}
