namespace MapView
{
	partial class XCMainWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();

			base.Dispose(disposing);
		}


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(XCMainWindow));
			this.mmMain = new System.Windows.Forms.MainMenu(this.components);
			this.menuFile = new System.Windows.Forms.MenuItem();
			this.miOpen = new System.Windows.Forms.MenuItem();
			this.miSaveAll = new System.Windows.Forms.MenuItem();
			this.miSeparator1 = new System.Windows.Forms.MenuItem();
			this.miSaveMap = new System.Windows.Forms.MenuItem();
			this.miSaveRoutes = new System.Windows.Forms.MenuItem();
			this.miSaveAs = new System.Windows.Forms.MenuItem();
			this.miSeparator2 = new System.Windows.Forms.MenuItem();
			this.miSaveMaptree = new System.Windows.Forms.MenuItem();
			this.miSeparator3 = new System.Windows.Forms.MenuItem();
			this.miSaveImage = new System.Windows.Forms.MenuItem();
			this.miExport = new System.Windows.Forms.MenuItem();
			this.miResize = new System.Windows.Forms.MenuItem();
			this.miInfo = new System.Windows.Forms.MenuItem();
			this.miRegenOccult = new System.Windows.Forms.MenuItem();
			this.miHq = new System.Windows.Forms.MenuItem();
			this.miSeparator4 = new System.Windows.Forms.MenuItem();
			this.miQuit = new System.Windows.Forms.MenuItem();
			this.menuEdit = new System.Windows.Forms.MenuItem();
			this.miOptions = new System.Windows.Forms.MenuItem();
			this.miConfigurator = new System.Windows.Forms.MenuItem();
			this.menuAnimation = new System.Windows.Forms.MenuItem();
			this.miOn = new System.Windows.Forms.MenuItem();
			this.miOff = new System.Windows.Forms.MenuItem();
			this.miDoors = new System.Windows.Forms.MenuItem();
			this.miGrid = new System.Windows.Forms.MenuItem();
			this.menuViewers = new System.Windows.Forms.MenuItem();
			this.menuHelp = new System.Windows.Forms.MenuItem();
			this.tvMaps = new System.Windows.Forms.TreeView();
			this.sfdSaveDialog = new System.Windows.Forms.SaveFileDialog();
			this.ssMain = new System.Windows.Forms.StatusStrip();
			this.tsslScale = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsslMapLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsslDimensions = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsslPosition = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsslSelectionSize = new System.Windows.Forms.ToolStripStatusLabel();
			this.tscPanel = new System.Windows.Forms.ToolStripContainer();
			this.tsTools = new System.Windows.Forms.ToolStrip();
			this.tsbSelectionBox = new System.Windows.Forms.ToolStripButton();
			this.cmMapTreeMenu = new System.Windows.Forms.ContextMenu();
			this.csSplitter = new DSShared.Windows.CollapsibleSplitter();
			this.ssMain.SuspendLayout();
			this.tscPanel.TopToolStripPanel.SuspendLayout();
			this.tscPanel.SuspendLayout();
			this.tsTools.SuspendLayout();
			this.SuspendLayout();
			// 
			// mmMain
			// 
			this.mmMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.menuFile,
			this.menuEdit,
			this.menuAnimation,
			this.menuViewers,
			this.menuHelp});
			// 
			// menuFile
			// 
			this.menuFile.Index = 0;
			this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.miOpen,
			this.miSaveAll,
			this.miSeparator1,
			this.miSaveMap,
			this.miSaveRoutes,
			this.miSaveAs,
			this.miSeparator2,
			this.miSaveMaptree,
			this.miSeparator3,
			this.miSaveImage,
			this.miExport,
			this.miResize,
			this.miInfo,
			this.miRegenOccult,
			this.miHq,
			this.miSeparator4,
			this.miQuit});
			this.menuFile.Text = "&File";
			// 
			// miOpen
			// 
			this.miOpen.Index = 0;
			this.miOpen.Text = "&Open";
			this.miOpen.Visible = false;
			this.miOpen.Click += new System.EventHandler(this.OnOpenClick);
			// 
			// miSaveAll
			// 
			this.miSaveAll.Enabled = false;
			this.miSaveAll.Index = 1;
			this.miSaveAll.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
			this.miSaveAll.Text = "Save &All";
			this.miSaveAll.Click += new System.EventHandler(this.OnSaveAllClick);
			// 
			// miSeparator1
			// 
			this.miSeparator1.Index = 2;
			this.miSeparator1.Text = "-";
			// 
			// miSaveMap
			// 
			this.miSaveMap.Enabled = false;
			this.miSaveMap.Index = 3;
			this.miSaveMap.Shortcut = System.Windows.Forms.Shortcut.CtrlM;
			this.miSaveMap.Text = "Save &Map";
			this.miSaveMap.Click += new System.EventHandler(this.OnSaveMapClick);
			// 
			// miSaveRoutes
			// 
			this.miSaveRoutes.Enabled = false;
			this.miSaveRoutes.Index = 4;
			this.miSaveRoutes.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
			this.miSaveRoutes.Text = "Save &Routes";
			this.miSaveRoutes.Click += new System.EventHandler(this.OnSaveRoutesClick);
			// 
			// miSaveAs
			// 
			this.miSaveAs.Enabled = false;
			this.miSaveAs.Index = 5;
			this.miSaveAs.Text = "Save As ...";
			this.miSaveAs.Click += new System.EventHandler(this.OnSaveAsClick);
			// 
			// miSeparator2
			// 
			this.miSeparator2.Index = 6;
			this.miSeparator2.Text = "-";
			// 
			// miSaveMaptree
			// 
			this.miSaveMaptree.Index = 7;
			this.miSaveMaptree.Shortcut = System.Windows.Forms.Shortcut.CtrlT;
			this.miSaveMaptree.Text = "Save Map&tree";
			this.miSaveMaptree.Click += new System.EventHandler(this.OnSaveMaptreeClick);
			// 
			// miSeparator3
			// 
			this.miSeparator3.Index = 8;
			this.miSeparator3.Text = "-";
			// 
			// miSaveImage
			// 
			this.miSaveImage.Enabled = false;
			this.miSaveImage.Index = 9;
			this.miSaveImage.Shortcut = System.Windows.Forms.Shortcut.CtrlI;
			this.miSaveImage.Text = "Save &Image ...";
			this.miSaveImage.Click += new System.EventHandler(this.OnSaveImageClick);
			// 
			// miExport
			// 
			this.miExport.Index = 10;
			this.miExport.Text = "&Export";
			this.miExport.Visible = false;
			this.miExport.Click += new System.EventHandler(this.OnExportClick);
			// 
			// miResize
			// 
			this.miResize.Enabled = false;
			this.miResize.Index = 11;
			this.miResize.Shortcut = System.Windows.Forms.Shortcut.CtrlZ;
			this.miResize.Text = "Resi&ze Map";
			this.miResize.Click += new System.EventHandler(this.OnMapResizeClick);
			// 
			// miInfo
			// 
			this.miInfo.Enabled = false;
			this.miInfo.Index = 12;
			this.miInfo.Shortcut = System.Windows.Forms.Shortcut.CtrlF;
			this.miInfo.Text = "Map In&fo";
			this.miInfo.Click += new System.EventHandler(this.OnInfoClick);
			// 
			// miRegenOccult
			// 
			this.miRegenOccult.Enabled = false;
			this.miRegenOccult.Index = 13;
			this.miRegenOccult.Text = "Regen &Occult";
			this.miRegenOccult.Visible = false;
			this.miRegenOccult.Click += new System.EventHandler(this.OnRegenOccultClick);
			// 
			// miHq
			// 
			this.miHq.Index = 14;
			this.miHq.Text = "Hq&2x";
			this.miHq.Visible = false;
			this.miHq.Click += new System.EventHandler(this.OnHq2xClick);
			// 
			// miSeparator4
			// 
			this.miSeparator4.Index = 15;
			this.miSeparator4.Text = "-";
			// 
			// miQuit
			// 
			this.miQuit.Index = 16;
			this.miQuit.Shortcut = System.Windows.Forms.Shortcut.CtrlQ;
			this.miQuit.Text = "&Quit";
			this.miQuit.Click += new System.EventHandler(this.OnQuitClick);
			// 
			// menuEdit
			// 
			this.menuEdit.Index = 1;
			this.menuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.miOptions,
			this.miConfigurator});
			this.menuEdit.Text = "&Edit";
			// 
			// miOptions
			// 
			this.miOptions.Index = 0;
			this.miOptions.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.miOptions.Text = "&Options";
			this.miOptions.Click += new System.EventHandler(this.OnOptionsClick);
			// 
			// miConfigurator
			// 
			this.miConfigurator.Index = 1;
			this.miConfigurator.Text = "&Configurator";
			this.miConfigurator.Click += new System.EventHandler(this.OnConfiguratorClick);
			// 
			// menuAnimation
			// 
			this.menuAnimation.Index = 2;
			this.menuAnimation.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
			this.miOn,
			this.miOff,
			this.miDoors,
			this.miGrid});
			this.menuAnimation.Text = "menuAnimation";
			this.menuAnimation.Visible = false;
			// 
			// miOn
			// 
			this.miOn.Checked = true;
			this.miOn.Index = 0;
			this.miOn.Shortcut = System.Windows.Forms.Shortcut.F1;
			this.miOn.Text = "miOn";
			this.miOn.Click += new System.EventHandler(this.OnOnClick);
			// 
			// miOff
			// 
			this.miOff.Index = 1;
			this.miOff.Shortcut = System.Windows.Forms.Shortcut.F2;
			this.miOff.Text = "miOff";
			this.miOff.Click += new System.EventHandler(this.OnOffClick);
			// 
			// miDoors
			// 
			this.miDoors.Index = 2;
			this.miDoors.Shortcut = System.Windows.Forms.Shortcut.F3;
			this.miDoors.Text = "miDoors";
			this.miDoors.Click += new System.EventHandler(this.OnToggleDoorsClick);
			// 
			// miGrid
			// 
			this.miGrid.Index = 3;
			this.miGrid.Shortcut = System.Windows.Forms.Shortcut.F4;
			this.miGrid.Text = "miGrid";
			this.miGrid.Click += new System.EventHandler(this.OnToggleGridClick);
			// 
			// menuViewers
			// 
			this.menuViewers.Enabled = false;
			this.menuViewers.Index = 3;
			this.menuViewers.Text = "&Viewers";
			// 
			// menuHelp
			// 
			this.menuHelp.Index = 4;
			this.menuHelp.Text = "&Help";
			// 
			// tvMaps
			// 
			this.tvMaps.BackColor = System.Drawing.SystemColors.Control;
			this.tvMaps.Dock = System.Windows.Forms.DockStyle.Left;
			this.tvMaps.HideSelection = false;
			this.tvMaps.Indent = 15;
			this.tvMaps.Location = new System.Drawing.Point(0, 0);
			this.tvMaps.Name = "tvMaps";
			this.tvMaps.Size = new System.Drawing.Size(240, 454);
			this.tvMaps.TabIndex = 0;
			this.tvMaps.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMapTreeMouseDown);
			// 
			// sfdSaveDialog
			// 
			this.sfdSaveDialog.DefaultExt = "PNG";
			this.sfdSaveDialog.Filter = "PNG files|*.PNG|All files (*.*)|*.*";
			this.sfdSaveDialog.RestoreDirectory = true;
			// 
			// ssMain
			// 
			this.ssMain.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ssMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsslScale,
			this.tsslMapLabel,
			this.tsslDimensions,
			this.tsslPosition,
			this.tsslSelectionSize});
			this.ssMain.Location = new System.Drawing.Point(248, 432);
			this.ssMain.Name = "ssMain";
			this.ssMain.Size = new System.Drawing.Size(384, 22);
			this.ssMain.TabIndex = 2;
			this.ssMain.Text = "statusStrip1";
			// 
			// tsslScale
			// 
			this.tsslScale.AutoSize = false;
			this.tsslScale.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.tsslScale.Name = "tsslScale";
			this.tsslScale.Size = new System.Drawing.Size(70, 17);
			// 
			// tsslMapLabel
			// 
			this.tsslMapLabel.AutoSize = false;
			this.tsslMapLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.tsslMapLabel.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tsslMapLabel.Name = "tsslMapLabel";
			this.tsslMapLabel.Size = new System.Drawing.Size(119, 17);
			this.tsslMapLabel.Spring = true;
			// 
			// tsslDimensions
			// 
			this.tsslDimensions.AutoSize = false;
			this.tsslDimensions.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.tsslDimensions.Name = "tsslDimensions";
			this.tsslDimensions.Size = new System.Drawing.Size(80, 17);
			// 
			// tsslPosition
			// 
			this.tsslPosition.AutoSize = false;
			this.tsslPosition.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.tsslPosition.Name = "tsslPosition";
			this.tsslPosition.Size = new System.Drawing.Size(100, 17);
			// 
			// tsslSelectionSize
			// 
			this.tsslSelectionSize.AutoSize = false;
			this.tsslSelectionSize.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
			| System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
			this.tsslSelectionSize.Name = "tsslSelectionSize";
			this.tsslSelectionSize.Size = new System.Drawing.Size(62, 17);
			// 
			// tscPanel
			// 
			// 
			// tscPanel.BottomToolStripPanel
			// 
			this.tscPanel.BottomToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// tscPanel.ContentPanel
			// 
			this.tscPanel.ContentPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.tscPanel.ContentPanel.Size = new System.Drawing.Size(384, 407);
			this.tscPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// tscPanel.LeftToolStripPanel
			// 
			this.tscPanel.LeftToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.tscPanel.Location = new System.Drawing.Point(248, 0);
			this.tscPanel.Name = "tscPanel";
			// 
			// tscPanel.RightToolStripPanel
			// 
			this.tscPanel.RightToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.tscPanel.Size = new System.Drawing.Size(384, 432);
			this.tscPanel.TabIndex = 4;
			// 
			// tscPanel.TopToolStripPanel
			// 
			this.tscPanel.TopToolStripPanel.Controls.Add(this.tsTools);
			this.tscPanel.TopToolStripPanel.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			// 
			// tsTools
			// 
			this.tsTools.Dock = System.Windows.Forms.DockStyle.None;
			this.tsTools.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tsTools.GripMargin = new System.Windows.Forms.Padding(0);
			this.tsTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsbSelectionBox});
			this.tsTools.Location = new System.Drawing.Point(3, 0);
			this.tsTools.Name = "tsTools";
			this.tsTools.Padding = new System.Windows.Forms.Padding(0);
			this.tsTools.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.tsTools.Size = new System.Drawing.Size(107, 25);
			this.tsTools.TabIndex = 3;
			// 
			// tsbSelectionBox
			// 
			this.tsbSelectionBox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbSelectionBox.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tsbSelectionBox.Image = ((System.Drawing.Image)(resources.GetObject("tsbSelectionBox.Image")));
			this.tsbSelectionBox.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbSelectionBox.Name = "tsbSelectionBox";
			this.tsbSelectionBox.Size = new System.Drawing.Size(84, 22);
			this.tsbSelectionBox.Text = "Selection Box";
			this.tsbSelectionBox.ToolTipText = "Draws a selection box in the floor";
			this.tsbSelectionBox.Visible = false;
			this.tsbSelectionBox.Click += new System.EventHandler(this.OnSelectionBoxClick);
			// 
			// csSplitter
			// 
			this.csSplitter.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
			this.csSplitter.ControlToHide = this.tvMaps;
			this.csSplitter.Location = new System.Drawing.Point(240, 0);
			this.csSplitter.MinimumSize = new System.Drawing.Size(5, 5);
			this.csSplitter.Name = "cSplitList";
			this.csSplitter.Size = new System.Drawing.Size(8, 454);
			this.csSplitter.TabIndex = 1;
			this.csSplitter.TabStop = false;
			// 
			// XCMainWindow
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.ClientSize = new System.Drawing.Size(632, 454);
			this.Controls.Add(this.tscPanel);
			this.Controls.Add(this.ssMain);
			this.Controls.Add(this.csSplitter);
			this.Controls.Add(this.tvMaps);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximumSize = new System.Drawing.Size(640, 480);
			this.Menu = this.mmMain;
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "XCMainWindow";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Map Editor";
			this.ssMain.ResumeLayout(false);
			this.ssMain.PerformLayout();
			this.tscPanel.TopToolStripPanel.ResumeLayout(false);
			this.tscPanel.TopToolStripPanel.PerformLayout();
			this.tscPanel.ResumeLayout(false);
			this.tscPanel.PerformLayout();
			this.tsTools.ResumeLayout(false);
			this.tsTools.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private System.Windows.Forms.TreeView tvMaps;
		private System.Windows.Forms.ContextMenu cmMapTreeMenu;

		private System.Windows.Forms.ToolStripContainer tscPanel;

		private System.Windows.Forms.ToolStripButton tsbSelectionBox;

		private System.Windows.Forms.MainMenu mmMain;

		private System.Windows.Forms.MenuItem menuFile;
		private System.Windows.Forms.MenuItem miOpen;
		private System.Windows.Forms.MenuItem miSaveAll;
		private System.Windows.Forms.MenuItem miSeparator1;
		private System.Windows.Forms.MenuItem miSaveMap;
		private System.Windows.Forms.MenuItem miSaveRoutes;
		private System.Windows.Forms.MenuItem miSaveAs;
		private System.Windows.Forms.MenuItem miSeparator2;
		private System.Windows.Forms.MenuItem miSaveMaptree;
		private System.Windows.Forms.MenuItem miSeparator3;
		private System.Windows.Forms.MenuItem miSaveImage;
		private System.Windows.Forms.MenuItem miResize;
		private System.Windows.Forms.MenuItem miInfo;
		private System.Windows.Forms.MenuItem miRegenOccult;
		private System.Windows.Forms.MenuItem miExport;
		private System.Windows.Forms.MenuItem miHq;
		private System.Windows.Forms.MenuItem miSeparator4;
		private System.Windows.Forms.MenuItem miQuit;

		private System.Windows.Forms.MenuItem menuViewers;

		private System.Windows.Forms.MenuItem menuEdit;
		private System.Windows.Forms.MenuItem miOptions;
		private System.Windows.Forms.MenuItem miConfigurator;

		private System.Windows.Forms.MenuItem menuHelp;

		private System.Windows.Forms.MenuItem menuAnimation;
		private System.Windows.Forms.MenuItem miOn;
		private System.Windows.Forms.MenuItem miOff;
		private System.Windows.Forms.MenuItem miDoors;
		private System.Windows.Forms.MenuItem miGrid; // for lack of a better place to put it.


		private System.Windows.Forms.SaveFileDialog sfdSaveDialog;
		private DSShared.Windows.CollapsibleSplitter csSplitter;

		private System.Windows.Forms.ToolStrip tsTools;

		private System.Windows.Forms.StatusStrip ssMain;

		private System.Windows.Forms.ToolStripStatusLabel tsslScale;
		private System.Windows.Forms.ToolStripStatusLabel tsslMapLabel;
		private System.Windows.Forms.ToolStripStatusLabel tsslDimensions;
		private System.Windows.Forms.ToolStripStatusLabel tsslPosition;
		private System.Windows.Forms.ToolStripStatusLabel tsslSelectionSize;
		#endregion
	}
}
