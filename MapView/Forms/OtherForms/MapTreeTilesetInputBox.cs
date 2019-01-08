using System;
using System.Collections.Generic;
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
		/// Stores the original terrain-list of a tileset, used only for
		/// 'EditTileset' to check if the terrains have changed when user clicks
		/// Accept.
		/// </summary>
		private List<string> TerrainsOriginal
		{ get; set; }

		private string _basepath;
		/// <summary>
		/// Gets/Sets the basepath. Calls ListTerrains() which also sets the
		/// Descriptor.
		/// </summary>
		private string BasePath
		{
			get { return _basepath; }
			set
			{
				_basepath = value;
				lblPathCurrent.Text = Path.Combine(_basepath, MapFileChild.MapsDir);

				ListTerrains();
			}
		}

		private Descriptor Descriptor
		{ get; set; }

		private TileGroup TileGroup
		{ get; set; }

		private bool Inited
		{ get; set; }

		private char[] Invalid
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

			SetPasteButtonStuffer();


			TileGroup = ResourceInfo.TileGroupInfo.TileGroups[Group] as TileGroup;

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

					var tileset = TileGroup.Categories[Category][Tileset];

					int records = 0;

					TerrainsOriginal = new List<string>();
					foreach (string terrain in tileset.Terrains)
					{
						TerrainsOriginal.Add(String.Copy(terrain));
						records += tileset.GetRecordCount(terrain);
					}

					lblMcdRecords.Text = records + " MCD Records";

					BasePath = tileset.BasePath;
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
					btnTerrainPaste.Enabled = false;

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
					BasePath = SharedSpace.Instance.GetShare(keyBaseDir);
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
				fbd.SelectedPath = BasePath;
				fbd.Description = String.Format(
							System.Globalization.CultureInfo.CurrentCulture,
							"Browse to a resource folder. A valid resource folder"
								+ " has the subfolders MAPS and ROUTES.");

				if (fbd.ShowDialog() == DialogResult.OK)
				{
					BasePath = fbd.SelectedPath;
					OnTilesetLabelChanged(null, EventArgs.Empty);
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
				ofd.Title  = "Select a Map File";
				ofd.InitialDirectory = Path.Combine(BasePath, MapFileChild.MapsDir);
				if (!Directory.Exists(ofd.InitialDirectory))
					ofd.InitialDirectory = BasePath;

				if (ofd.ShowDialog() == DialogResult.OK)
				{
					string pfeMap = ofd.FileName;

					string basepath = Path.GetDirectoryName(pfeMap);
					int pos = basepath.LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase);
					BasePath = (pos != -1) ? basepath.Substring(0, pos)
										   : basepath;

					Tileset = Path.GetFileNameWithoutExtension(pfeMap);
					OnTilesetLabelChanged(null, EventArgs.Empty);	// NOTE: This will fire OnTilesetLabelChanged() twice usually but
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
		private void OnTilesetLabelChanged(object sender, EventArgs e)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("OnTilesetLabelChanged");

			if (Inited) // do not run until text has been initialized.
			{
				if (!ValidateCharacters(Tileset))
				{
					ShowErrorDialog("Characters detected that are not allowed.");

					Tileset = InvalidateCharacters(Tileset); // recurse function after removing invalid chars.
					tbTileset.SelectionStart = tbTileset.TextLength;
				}
				else
				{
					switch (InputBoxType)
					{
						case BoxType.AddTileset:
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
							else // (Descriptor != null && Descriptor.Label == Tileset)
							{
								btnCreateMap.Enabled = false;

								if (MapFileExists(Tileset))
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
							break;
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
			//LogFile.WriteLine("");
			//LogFile.WriteLine("ListTerrains");

			btnMoveUp   .Enabled =
			btnMoveDown .Enabled =
			btnMoveRight.Enabled =
			btnMoveLeft .Enabled = false;

			lbTerrainsAllocated.Items.Clear();
			lbTerrainsAvailable.Items.Clear();


			switch (InputBoxType)
			{
				case BoxType.EditTileset:
					if (Tileset == TilesetOriginal
						|| (!IsTilesetInGroups(Tileset) && !MapFileExists(Tileset)))
					{
						Descriptor = TileGroup.Categories[Category][TilesetOriginal];
					}
					else
						Descriptor = null;
					break;

				case BoxType.AddTileset:
					if (IsTilesetInGroups(Tileset)) // completely disallowed. ie, user must delete the old tileset first
					{
						Descriptor = null;
					}
					break;
			}
			//LogFile.WriteLine(". Descriptor= " + ((Descriptor != null) ? Descriptor.Label : "NULL"));

			int records = 0;

			if (Descriptor != null)
			{
				foreach (string terrain in Descriptor.Terrains)
				{
					lbTerrainsAllocated.Items.Add(terrain);
					records += Descriptor.GetRecordCount(terrain);
				}
			}
			lblMcdRecords.Text = records + " MCD Records";

			btnTerrainCopy .Enabled =
			btnTerrainPaste.Enabled = (Descriptor != null);


//			string dirTerrain = Path.Combine(BasePath, Descriptor.PathTerrain);
			string dirTerrain = (TileGroup.Pal == Palette.UfoBattle) ? SharedSpace.ResourceDirectoryUfo
																	 : SharedSpace.ResourceDirectoryTftd;
			dirTerrain = Path.Combine(SharedSpace.Instance.GetShare(dirTerrain), Descriptor.PathTerrain);

			if (Directory.Exists(dirTerrain))
			{
				string terrain = String.Empty;
				string[] terrains = Directory.GetFiles(
													dirTerrain,
													"*.pck",
													SearchOption.TopDirectoryOnly);
				for (int i = 0; i != terrains.Length; ++i)
				{
					terrain = Path.GetFileNameWithoutExtension(terrains[i]).ToUpperInvariant();

					if ((Descriptor == null || !Descriptor.Terrains.Contains(terrain))
						&& terrain != "BLANKS")
					{
						lbTerrainsAvailable.Items.Add(terrain);
					}
				}
			}
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
			//LogFile.WriteLine("");
			//LogFile.WriteLine("OnCreateDescriptorClick");

			if (!IsTilesetInGroups(Tileset))
			{
				//LogFile.WriteLine(". descriptor Instantiated= " + Tileset);

				Descriptor = new Descriptor(		// be careful with that; it isn't being deleted if user clicks Cancel
										Tileset,	// or chooses instead to create yet another descriptor.
										new List<string>(),
										BasePath,
										TileGroup.Pal);

				if (MapFileExists(Tileset))
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
			//LogFile.WriteLine("");
			//LogFile.WriteLine("OnAcceptClick");
			//LogFile.WriteLine(". Tileset= " + Tileset);

			switch (InputBoxType)
			{
				case BoxType.EditTileset:
					//LogFile.WriteLine(". . MODE:EditTileset");

					if (String.IsNullOrEmpty(Tileset))
					{
						//LogFile.WriteLine(". The Map label cannot be blank.");
						ShowErrorDialog("The Map label cannot be blank.");

						tbTileset.Select();
					}
					else if (lbTerrainsAllocated.Items.Count == 0)
					{
						//LogFile.WriteLine(". The Map must have at least one terrain allocated.");
						ShowErrorDialog("The Map must have at least one terrain allocated.");
					}
					else
					{
						//foreach (string t in TerrainsOriginal)
						//	LogFile.WriteLine(". terrain original= " + t);
						//foreach (string t in Descriptor.Terrains)
						//	LogFile.WriteLine(". terrain= " + t);

						if (Tileset == TilesetOriginal) // label didn't change; check if terrains changed ->
						{
							//LogFile.WriteLine(". . label did *not* change");

							if (Descriptor.Terrains.SequenceEqual(TerrainsOriginal))
							{
								//LogFile.WriteLine(". . . No changes were made.");
								ShowInfoDialog("No changes were made.");
							}
							else
							{
								//LogFile.WriteLine(". . . DialogResult OK");
								DialogResult = DialogResult.OK;
							}

							// NOTE: a Save of Map-file is *not* required here.
						}
						else // label changed; rewrite the descriptor ->
						{
							//LogFile.WriteLine(". . change label");

							// NOTE: user cannot edit a Map-label to be another already existing file.
							// There are other ways to do that: either let the user delete the target-
							// Map-file from his/her disk, or better click Edit on *that* tileset.
							// NOTE: however, if while editing a tileset the user browses to another
							// tileset and edits that tileset's terrains, the changes are effective.

							if (MapFileExists(Tileset))
							{
								//LogFile.WriteLine(". The Map file already exists on disk.");
								ShowErrorDialog("The Map file already exists on disk. The Tileset Editor is"
												+ " not sophisticated enough to deal with this eventuality."
												+ " Either edit that Map directly if it's already in the Maptree,"
												+ " or use Add Tileset to make it editable, or as a last"
												+ " resort delete it from your disk."
												+ Environment.NewLine + Environment.NewLine
												+ GetFullPathMap(Tileset));
												// TODO: Ask user if he/she wants to overwrite the Map-file.
							}
							else
							{
								if (IsTilesetInGroups(Tileset))
								{
									//LogFile.WriteLine(". The tileset already exists in the Maptree.");
									ShowErrorDialog("The tileset already exists in the Maptree."
												+ Environment.NewLine + Environment.NewLine
												+ Tileset
												+ Environment.NewLine + Environment.NewLine
												+ "Options are to edit that one, delete that one,"
												+ " or choose a different label for this one.");
								}
								else
								{
									//LogFile.WriteLine(". . . tileset Created");

									string pfeMap    = GetFullPathMap(Tileset);
									string pfeMapPre = GetFullPathMap(TilesetOriginal);

									//LogFile.WriteLine(". . . . fileMapPre= " + pfeMapPre);
									//LogFile.WriteLine(". . . . fileMap= " + pfeMap);

									File.Move(pfeMapPre, pfeMap);	// NOTE: This has to happen now because once the MapTree node
																	// is selected it will try to load the .MAP file etc.

									if (File.Exists(pfeMap))		// NOTE: do *not* alter the descriptor if File.Move() went bork.
									{								// This is likely redundant: File.Move() should throw.
										string pfeRoutes    = GetFullPathRoutes(Tileset);
										string pfeRoutesPre = GetFullPathRoutes(TilesetOriginal);

										//LogFile.WriteLine(". . . . fileRoutesPre= " + pfeRoutesPre);
										//LogFile.WriteLine(". . . . fileRoutes= " + pfeRoutes);

										File.Move(pfeRoutesPre, pfeRoutes);

										var category = TileGroup.Categories[Category];
										Descriptor = new Descriptor(
																Tileset,
																category[TilesetOriginal].Terrains,
																BasePath,
																TileGroup.Pal);
										TileGroup.AddTileset(Descriptor, Category);			// NOTE: This could be done on return to XCMainWindow.OnEditTilesetClick()
																							// but then 'Descriptor' would have to be internal.
										TileGroup.DeleteTileset(TilesetOriginal, Category);	// NOTE: This could be done on return to XCMainWindow.OnEditTilesetClick()
																							// but then 'TilesetOriginal' would have to be internal.

										DialogResult = DialogResult.OK;
									}
								}
							}
						}
					}
					break;

				case BoxType.AddTileset:
					//LogFile.WriteLine(". . MODE:AddTileset");

					if (String.IsNullOrEmpty(Tileset))								// NOTE: The tileset-label should already have been
					{																// checked for validity by here before the Create button.
						//LogFile.WriteLine(". The Map label cannot be blank.");	// But these handle the case when user immediately clicks the Ok button.
						ShowErrorDialog("The Map label cannot be blank.");			// ... TODO: so disable the Ok button, unless a descriptor is valid

						tbTileset.Select();
					}
					else if (lbTerrainsAllocated.Items.Count == 0)
					{
						//LogFile.WriteLine(". The Map must have at least one terrain allocated.");
						ShowErrorDialog("The Map must have at least one terrain allocated.");
					}
					else
					{
						switch (FileAddType)
						{
							case AddType.MapExists:
								//LogFile.WriteLine(". . Map file EXISTS");

								TileGroup.AddTileset(Descriptor, Category);
								DialogResult = DialogResult.OK;
								break;

							case AddType.MapCreate:
								//LogFile.WriteLine(". . Map file does NOT exist - Create new Map file");

								string pfeMap = GetFullPathMap(Tileset);
								//LogFile.WriteLine(". . . fileMap= " + pfeMap);

								string pfeRoutes = GetFullPathRoutes(Tileset);
								//LogFile.WriteLine(". . . fileRoutes= " + pfeRoutes);

								Directory.CreateDirectory(Path.GetDirectoryName(pfeRoutes));
								using (var fs = File.Create(pfeRoutes)) // create a blank Route-file and release its handle.
								{}

								Directory.CreateDirectory(Path.GetDirectoryName(pfeMap));
								using (var fs = File.Create(pfeMap))	// create the Map-file and release its handle.
								{										// NOTE: This has to happen now because once the MapTree node
									MapFileChild.CreateMap(				// is selected it will try to load the .MAP file etc.
														fs,
														10, 10, 1); // <- default new Map size
								}

								if (File.Exists(pfeMap) && File.Exists(pfeRoutes)) // NOTE: The descriptor has already been created with the Create descriptor button.
								{
									//LogFile.WriteLine(". tileset Created");

									TileGroup.AddTileset(Descriptor, Category);
									DialogResult = DialogResult.OK;
								}
								break;
						}
					}
					break;
			}
		}

		private void OnTerrainLeftClick(object sender, EventArgs e)
		{
			XCMainWindow.Instance.MaptreeChanged = (InputBoxType == BoxType.EditTileset);

			Descriptor.Terrains.Add(lbTerrainsAvailable.SelectedItem as String);
			ListTerrains();
		}

		private void OnTerrainRightClick(object sender, EventArgs e)
		{
			XCMainWindow.Instance.MaptreeChanged = (InputBoxType == BoxType.EditTileset);

			Descriptor.Terrains.Remove(lbTerrainsAllocated.SelectedItem as String);
			ListTerrains();
		}

		private void OnTerrainUpClick(object sender, EventArgs e)
		{
			XCMainWindow.Instance.MaptreeChanged = (InputBoxType == BoxType.EditTileset);

			var terrains = Descriptor.Terrains;

			for (int id = 1; id != terrains.Count; ++id)
			{
				if (terrains[id] == lbTerrainsAllocated.SelectedItem as String)
				{
					string t = terrains[id - 1];
					terrains[id - 1] = terrains[id];
					terrains[id] = t;

					Descriptor.Terrains = terrains;

					lbTerrainsAllocated.Items.Clear();

					foreach (string terrain in terrains)
						lbTerrainsAllocated.Items.Add(terrain);

					lbTerrainsAllocated.SelectedItem = terrains[id - 1];
					break;
				}
			}
		}

		private void OnTerrainDownClick(object sender, EventArgs e)
		{
			XCMainWindow.Instance.MaptreeChanged = (InputBoxType == BoxType.EditTileset);

			var terrains = Descriptor.Terrains;

			for (int id = 0; id != terrains.Count - 1; ++id)
			{
				if (terrains[id] == lbTerrainsAllocated.SelectedItem as String)
				{
					string t = terrains[id + 1];
					terrains[id + 1] = terrains[id];
					terrains[id] = t;

					Descriptor.Terrains = terrains;
					
					lbTerrainsAllocated.Items.Clear();

					foreach (string terrain in terrains)
						lbTerrainsAllocated.Items.Add(terrain);

					lbTerrainsAllocated.SelectedItem = terrains[id + 1];
					break;
				}
			}
		}

		private void OnTerrainCopyClick(object sender, EventArgs e)
		{
			XCMainWindow.Instance.TilesetTerrains.Clear();

			foreach (var terrain in Descriptor.Terrains)
				XCMainWindow.Instance.TilesetTerrains.Add(terrain);

			SetPasteButtonStuffer();
//			btnTerrainPaste.Enabled = true;
		}

		private void OnTerrainPasteClick(object sender, EventArgs e)
		{
			XCMainWindow.Instance.MaptreeChanged = (InputBoxType == BoxType.EditTileset);

			Descriptor.Terrains.Clear();

			var terrains = XCMainWindow.Instance.TilesetTerrains;
			foreach (var terrain in terrains)
				Descriptor.Terrains.Add(terrain);

			ListTerrains();
		}

		private void SetPasteButtonStuffer()
		{
			btnTerrainPaste.Text = (XCMainWindow.Instance.TilesetTerrains.Count != 0) ? "Paste"
																					  : "Clear";
			string tipPaste = String.Empty;
			bool first = true;
			foreach (var terrain in XCMainWindow.Instance.TilesetTerrains)
			{
				if (first)
					first = false;
				else
					tipPaste += Environment.NewLine;

				tipPaste += terrain.ToLowerInvariant();
			}
			toolTip1.SetToolTip(btnTerrainPaste, tipPaste);
		}

		private void OnAllocatedIndexChanged(object sender, EventArgs e)
		{
			if (lbTerrainsAllocated.SelectedIndex != -1)
			{
				//LogFile.WriteLine("OnAllocatedIndexChanged");
				//LogFile.WriteLine(". descriptor= " + ((Descriptor != null) ? Descriptor.Label : "NULL"));

				btnMoveRight.Enabled = true;

				if (Descriptor != null && Descriptor.Terrains.Count > 1)
				{
					btnMoveUp  .Enabled = (lbTerrainsAllocated.SelectedIndex != 0);
					btnMoveDown.Enabled = (lbTerrainsAllocated.SelectedIndex != Descriptor.Terrains.Count - 1);
				}
			}
		}

		private void OnAvailableIndexChanged(object sender, EventArgs e)
		{
			//LogFile.WriteLine("OnAvailableIndexChanged");
			//LogFile.WriteLine(". descriptor= " + ((Descriptor != null) ? Descriptor.Label : "NULL"));

			btnMoveLeft.Enabled = (lbTerrainsAvailable.SelectedIndex != -1)
							   && (Descriptor != null);
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
		private string GetFullPathMap(string labelMap)
		{
			string dirMaps = Path.Combine(BasePath, MapFileChild.MapsDir);
			return Path.Combine(dirMaps, labelMap + MapFileChild.MapExt);
		}

		/// <summary>
		/// Gets the fullpath for a Routes-file.
		/// </summary>
		/// <param name="labelRoutes"></param>
		/// <returns></returns>
		private string GetFullPathRoutes(string labelRoutes)
		{
			string dirRoutes = Path.Combine(BasePath, RouteNodeCollection.RoutesDir);
			return Path.Combine(dirRoutes, labelRoutes + RouteNodeCollection.RouteExt);
		}

		/// <summary>
		/// Checks if a Map-file w/ 'label' exists in the current 'BasePath'
		/// directory.
		/// </summary>
		/// <param name="labelMap">the name w/out extension of a Map-file to check for</param>
		/// <returns></returns>
		private bool MapFileExists(string labelMap)
		{
			//LogFile.WriteLine("MapFileExists");

			string pfeMap = null;
			if (!String.IsNullOrEmpty(labelMap))
			{
				pfeMap = GetFullPathMap(labelMap);
				//LogFile.WriteLine(". pfeMap= " + pfeMap);
			}

			//LogFile.WriteLine(". ret= " + (pfeMap != null && File.Exists(pfeMap)));
			return (pfeMap != null && File.Exists(pfeMap));
		}

		private static bool IsTilesetInGroups(string labelMap)
		{
			//LogFile.WriteLine("IsDescriptorInGroups");

			foreach (var tileGroup in ResourceInfo.TileGroupInfo.TileGroups)
			foreach (var category in tileGroup.Value.Categories)
			foreach (var descriptor in category.Value.Values)
			{
				if (descriptor.Label == labelMap)
					return true;
			}
			return false;
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
	}
}

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
//			{

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
