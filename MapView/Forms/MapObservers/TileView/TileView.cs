using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using MapView.Forms.MainWindow;
using MapView.Forms.McdViewer;
using MapView.SettingServices;

using PckView;

using XCom;
using XCom.Interfaces.Base;


namespace MapView.Forms.MapObservers.TileViews
{
	public partial class TileView
		:
		MapObserverControl0
	{
		private IContainer components = null;

		private MenuItem mcdInfoTab;

		private TilePanel allTiles;
		private TilePanel[] panels;

		private McdViewerForm MCDInfoForm;

		private TabControl tabs;
		private TabPage allTab;
		private TabPage groundTab;
		private TabPage objectsTab;
		private TabPage nWallsTab;
		private TabPage wWallsTab;

		private Hashtable _brushes;

		private IMainShowAllManager _mainWindowsShowAllManager;

		public event SelectedTileTypeChangedEventHandler SelectedTileTypeChangedObserver;

		private void OnSelectedTileTypeChanged(TileBase tile)
		{
			var handler = SelectedTileTypeChangedObserver;
			if (handler != null)
				handler(tile);
		}

		public event MethodInvoker MapChanged;

		private void OnMapChanged()
		{
			MethodInvoker handler = MapChanged;
			if (handler != null)
				handler();
		}


		public TileView()
		{
			InitializeComponent();

			tabs.Selected += tabs_Selected;

			allTiles    = new TilePanel(TileType.All);
			var ground  = new TilePanel(TileType.Ground);
			var wWalls  = new TilePanel(TileType.WestWall);
			var nWalls  = new TilePanel(TileType.NorthWall);
			var objects = new TilePanel(TileType.Object);

			panels = new[]
			{
				allTiles,
				ground,
				wWalls,
				nWalls,
				objects
			};

			AddPanel(allTiles, allTab);

			AddPanel(ground,  groundTab);
			AddPanel(wWalls,  wWallsTab);
			AddPanel(nWalls,  nWallsTab);
			AddPanel(objects, objectsTab);
		}


		public void Initialize(IMainShowAllManager mainWindowsShowAllManager)
		{
			_mainWindowsShowAllManager = mainWindowsShowAllManager;
		}

		private void tabs_Selected(object sender, TabControlEventArgs e)
		{
			var f = FindForm();

			var tile = SelectedTile;
			if (tile != null && tile.Info is McdEntry)
			{
				f.Text = BuildTitleString(tile.TileListId, tile.Id);

				if (MCDInfoForm != null)
					MCDInfoForm.UpdateData((McdEntry)tile.Info);
			}
			else
			{
				f.Text = "Tile View";

				if (MCDInfoForm != null)
					MCDInfoForm.UpdateData(null);
			}
		}

		private void options_click(object sender, EventArgs e)
		{
			var f = new PropertyForm("TileViewOptions", Settings);
			f.Text = "Tile View Options";
			f.Show();
		}

		private void BrushChanged(object sender,string key, object val)
		{
			((SolidBrush)_brushes[key]).Color = (Color)val;
			Refresh();
		}

		public override void LoadDefaultSettings()
		{
			_brushes = new Hashtable();

			foreach (string st in Enum.GetNames(typeof(SpecialType)))
			{
				_brushes[st] = new SolidBrush(TilePanel.TileColors[(int)Enum.Parse(typeof(SpecialType), st)]);
				Settings.AddSetting(
								st,
								((SolidBrush)_brushes[st]).Color,
								"Color of specified tile type",
								"TileView",
								BrushChanged,
								false,
								null);
			}
			VolutarSettingService.LoadDefaultSettings(Settings);

			TilePanel.SetColors(_brushes);
		}

		private void AddPanel(TilePanel panel, Control page)
		{
			panel.Dock = DockStyle.Fill;
			page.Controls.Add(panel);
			panel.SelectedTileTypeChangedPanel += TileChanged;
		}

		private void TileChanged(TileBase tile)
		{
			var f = FindForm();
			if (tile != null && tile.Info is McdEntry)
			{
				f.Text = BuildTitleString(tile.TileListId, tile.Id);

				if (MCDInfoForm != null)
					MCDInfoForm.UpdateData((McdEntry)tile.Info);
			}
			else
			{
				f.Text = "Tile View";

				if (MCDInfoForm != null)
					MCDInfoForm.UpdateData(null);
			}

			OnSelectedTileTypeChanged(tile);
		}

		public override IMap_Base Map
		{
			set
			{
				base.Map = value;
				Tiles = (value != null) ? value.Tiles
										: null;
			}
		}

		private System.Collections.Generic.IList<TileBase> Tiles
		{
			set
			{
				for (int i = 0; i != panels.Length; ++i)
					panels[i].SetTiles(value);

				OnResize(null);
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TileBase SelectedTile
		{
			get { return panels[tabs.SelectedIndex].SelectedTile; }
			set
			{
				allTiles.SelectedTile = value;
				tabs.SelectedIndex = 0;

				Refresh();
			}
		}

		private void EditPckMenuItem_Click(object sender, EventArgs e)
		{
			var dep = GetSelectedDependencyName();
			if (dep != null)
			{
				var imageInfo = GameInfo.ImageInfo[dep];
				if (imageInfo != null)
				{
					var pathfilext = imageInfo.BasePath + imageInfo.BaseName + ".PCK";

					if (!File.Exists(pathfilext))
					{
						MessageBox.Show(
									this,
									"File does not exist: " + pathfilext,
									"Error",
									MessageBoxButtons.OK,
									MessageBoxIcon.Exclamation,
									MessageBoxDefaultButton.Button1,
									0);
					}
					else
					{
						_mainWindowsShowAllManager.HideAll();


						using (var editor = new PckViewForm())
						{
							var pckFile = imageInfo.GetPckFile();
							editor.SelectedPalette = pckFile.Pal.Name;
							editor.LoadPckFile(pathfilext, pckFile.Bpp);

							var parent = FindForm();

							Form owner = null;
							if (parent != null)
								owner = parent.Owner;

							if (owner == null)
								owner = parent;

							editor.ShowDialog(owner);
							if (editor.SavedFile)
							{
								GameInfo.ImageInfo.Images[dep].ClearMcdTable();
								GameInfo.ClearPckCache(imageInfo.BasePath, imageInfo.BaseName);

								OnMapChanged();
							}
						}

						_mainWindowsShowAllManager.RestoreAll();
					}
				}
			}
		}

		private void mcdInfoTab_Click(object sender, System.EventArgs e)
		{
			if (!mcdInfoTab.Checked)
			{
				mcdInfoTab.Checked = true;

				if (MCDInfoForm == null)
				{
					MCDInfoForm = new McdViewerForm();
					MCDInfoForm.Closing += infoTabClosing;

					var f = FindForm();

					var tile = SelectedTile;
					if (tile != null && tile.Info is McdEntry)
					{
						f.Text = BuildTitleString(tile.TileListId, tile.Id);
						MCDInfoForm.UpdateData((McdEntry)tile.Info);
					}
					else
					{
						f.Text = "Tile View";
						MCDInfoForm.UpdateData(null);
					}
				}

//				MCDInfoForm.Location = new Point( // this is f'd.
//											this.Location.X - MCDInfoForm.Width,
//											this.Location.Y);
				MCDInfoForm.Visible = true;

				MCDInfoForm.Show();
			}
		}

		private void infoTabClosing(object sender, CancelEventArgs e)
		{
			e.Cancel = true;
			MCDInfoForm.Visible = false;
			mcdInfoTab.Checked = false;
		}

		private string GetSelectedDependencyName()
		{
			var tile = SelectedTile;
			if (tile != null)
			{
				var map = Map as XCMapFile;
				if (map != null)
					return map.GetDependencyName(tile);
			}
			return null;
		}

		private string BuildTitleString(int id, int mcd)
		{
			var dep = GetSelectedDependencyName();
			return "Tile View : id " + id + " - mcd " + mcd + " - " + (dep ?? "unknown");
		}

		private void VolutarMcdEditMenuItem_Click(object sender, EventArgs e)
		{
			if ((Map as XCMapFile) != null)
			{
				var service = new VolutarSettingService(Settings);
				var path = service.FullPath;

				if (!string.IsNullOrEmpty(path))
					Process.Start(new ProcessStartInfo(path));
			}
		}
	}

	public delegate void SelectedTileTypeChangedEventHandler(TileBase tile);
}