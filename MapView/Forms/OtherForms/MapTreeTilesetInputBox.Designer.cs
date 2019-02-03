namespace MapView
{
	partial class MapTreeTilesetInputBox
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblHeaderGroup = new System.Windows.Forms.Label();
			this.tbTileset = new System.Windows.Forms.TextBox();
			this.pnlBottom = new System.Windows.Forms.Panel();
			this.lblAddType = new System.Windows.Forms.Label();
			this.pnlTop = new System.Windows.Forms.Panel();
			this.gbTerrains = new System.Windows.Forms.GroupBox();
			this.lbTerrainsAllocated = new System.Windows.Forms.ListBox();
			this.lbTerrainsAvailable = new System.Windows.Forms.ListBox();
			this.pnlTerrainsHeader = new System.Windows.Forms.Panel();
			this.lbl_ListAvailable = new System.Windows.Forms.Label();
			this.rb_CustomBasepath = new System.Windows.Forms.RadioButton();
			this.rb_TilesetBasepath = new System.Windows.Forms.RadioButton();
			this.rb_ConfigBasepath = new System.Windows.Forms.RadioButton();
			this.lbl_PathAllocated_ = new System.Windows.Forms.Label();
			this.lbl_PathAvailable = new System.Windows.Forms.Label();
			this.lbl_PathAllocated = new System.Windows.Forms.Label();
			this.btnFindBasepath = new System.Windows.Forms.Button();
			this.tbTerrainPath = new System.Windows.Forms.TextBox();
			this.lblTerrainChanges = new System.Windows.Forms.Label();
			this.lblAllocated = new System.Windows.Forms.Label();
			this.lblAvailable = new System.Windows.Forms.Label();
			this.pnlSpacer = new System.Windows.Forms.Panel();
			this.btnTerrainClear = new System.Windows.Forms.Button();
			this.btnTerrainPaste = new System.Windows.Forms.Button();
			this.btnTerrainCopy = new System.Windows.Forms.Button();
			this.btnMoveLeft = new System.Windows.Forms.Button();
			this.btnMoveDown = new System.Windows.Forms.Button();
			this.btnMoveRight = new System.Windows.Forms.Button();
			this.btnMoveUp = new System.Windows.Forms.Button();
			this.gbTileset = new System.Windows.Forms.GroupBox();
			this.lblTilesetCount_ = new System.Windows.Forms.Label();
			this.lblTilesetCount = new System.Windows.Forms.Label();
			this.btnCreateMap = new System.Windows.Forms.Button();
			this.btnFindDirectory = new System.Windows.Forms.Button();
			this.lblTilesetMap = new System.Windows.Forms.Label();
			this.btnFindTileset = new System.Windows.Forms.Button();
			this.lblPathCurrent = new System.Windows.Forms.Label();
			this.lblTilesetPath = new System.Windows.Forms.Label();
			this.gbHeader = new System.Windows.Forms.GroupBox();
			this.lblMcdRecords = new System.Windows.Forms.Label();
			this.lblTilesetCurrent = new System.Windows.Forms.Label();
			this.lblGroupCurrent = new System.Windows.Forms.Label();
			this.lblHeaderCategory = new System.Windows.Forms.Label();
			this.lblCategoryCurrent = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.pnlBottom.SuspendLayout();
			this.pnlTop.SuspendLayout();
			this.gbTerrains.SuspendLayout();
			this.pnlTerrainsHeader.SuspendLayout();
			this.pnlSpacer.SuspendLayout();
			this.gbTileset.SuspendLayout();
			this.gbHeader.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(435, 0);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(80, 25);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "Ok";
			this.btnOk.Click += new System.EventHandler(this.OnAcceptClick);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(525, 0);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 25);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			// 
			// lblHeaderGroup
			// 
			this.lblHeaderGroup.Location = new System.Drawing.Point(10, 15);
			this.lblHeaderGroup.Name = "lblHeaderGroup";
			this.lblHeaderGroup.Size = new System.Drawing.Size(65, 15);
			this.lblHeaderGroup.TabIndex = 0;
			this.lblHeaderGroup.Text = "GROUP";
			// 
			// tbTileset
			// 
			this.tbTileset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tbTileset.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.tbTileset.Location = new System.Drawing.Point(55, 35);
			this.tbTileset.Name = "tbTileset";
			this.tbTileset.Size = new System.Drawing.Size(465, 19);
			this.tbTileset.TabIndex = 4;
			this.tbTileset.TextChanged += new System.EventHandler(this.OnTilesetTextboxChanged);
			// 
			// pnlBottom
			// 
			this.pnlBottom.Controls.Add(this.lblAddType);
			this.pnlBottom.Controls.Add(this.btnOk);
			this.pnlBottom.Controls.Add(this.btnCancel);
			this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.pnlBottom.Location = new System.Drawing.Point(0, 514);
			this.pnlBottom.Name = "pnlBottom";
			this.pnlBottom.Size = new System.Drawing.Size(612, 30);
			this.pnlBottom.TabIndex = 0;
			// 
			// lblAddType
			// 
			this.lblAddType.Location = new System.Drawing.Point(5, 5);
			this.lblAddType.Name = "lblAddType";
			this.lblAddType.Size = new System.Drawing.Size(200, 15);
			this.lblAddType.TabIndex = 0;
			this.lblAddType.Text = "lblAddType";
			// 
			// pnlTop
			// 
			this.pnlTop.Controls.Add(this.gbTerrains);
			this.pnlTop.Controls.Add(this.gbTileset);
			this.pnlTop.Controls.Add(this.gbHeader);
			this.pnlTop.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pnlTop.Location = new System.Drawing.Point(0, 0);
			this.pnlTop.Name = "pnlTop";
			this.pnlTop.Size = new System.Drawing.Size(612, 514);
			this.pnlTop.TabIndex = 0;
			// 
			// gbTerrains
			// 
			this.gbTerrains.Controls.Add(this.lbTerrainsAllocated);
			this.gbTerrains.Controls.Add(this.lbTerrainsAvailable);
			this.gbTerrains.Controls.Add(this.pnlTerrainsHeader);
			this.gbTerrains.Controls.Add(this.pnlSpacer);
			this.gbTerrains.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbTerrains.Location = new System.Drawing.Point(0, 130);
			this.gbTerrains.Name = "gbTerrains";
			this.gbTerrains.Size = new System.Drawing.Size(612, 384);
			this.gbTerrains.TabIndex = 2;
			this.gbTerrains.TabStop = false;
			this.gbTerrains.Text = "Terrains";
			// 
			// lbTerrainsAllocated
			// 
			this.lbTerrainsAllocated.Dock = System.Windows.Forms.DockStyle.Left;
			this.lbTerrainsAllocated.FormattingEnabled = true;
			this.lbTerrainsAllocated.ItemHeight = 12;
			this.lbTerrainsAllocated.Location = new System.Drawing.Point(3, 110);
			this.lbTerrainsAllocated.Name = "lbTerrainsAllocated";
			this.lbTerrainsAllocated.Size = new System.Drawing.Size(267, 271);
			this.lbTerrainsAllocated.TabIndex = 0;
			this.lbTerrainsAllocated.SelectedIndexChanged += new System.EventHandler(this.OnAllocatedIndexChanged);
			// 
			// lbTerrainsAvailable
			// 
			this.lbTerrainsAvailable.Dock = System.Windows.Forms.DockStyle.Right;
			this.lbTerrainsAvailable.FormattingEnabled = true;
			this.lbTerrainsAvailable.ItemHeight = 12;
			this.lbTerrainsAvailable.Location = new System.Drawing.Point(345, 110);
			this.lbTerrainsAvailable.Name = "lbTerrainsAvailable";
			this.lbTerrainsAvailable.Size = new System.Drawing.Size(264, 271);
			this.lbTerrainsAvailable.TabIndex = 1;
			this.lbTerrainsAvailable.SelectedIndexChanged += new System.EventHandler(this.OnAvailableIndexChanged);
			// 
			// pnlTerrainsHeader
			// 
			this.pnlTerrainsHeader.Controls.Add(this.lbl_ListAvailable);
			this.pnlTerrainsHeader.Controls.Add(this.rb_CustomBasepath);
			this.pnlTerrainsHeader.Controls.Add(this.rb_TilesetBasepath);
			this.pnlTerrainsHeader.Controls.Add(this.rb_ConfigBasepath);
			this.pnlTerrainsHeader.Controls.Add(this.lbl_PathAllocated_);
			this.pnlTerrainsHeader.Controls.Add(this.lbl_PathAvailable);
			this.pnlTerrainsHeader.Controls.Add(this.lbl_PathAllocated);
			this.pnlTerrainsHeader.Controls.Add(this.btnFindBasepath);
			this.pnlTerrainsHeader.Controls.Add(this.tbTerrainPath);
			this.pnlTerrainsHeader.Controls.Add(this.lblTerrainChanges);
			this.pnlTerrainsHeader.Controls.Add(this.lblAllocated);
			this.pnlTerrainsHeader.Controls.Add(this.lblAvailable);
			this.pnlTerrainsHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlTerrainsHeader.Location = new System.Drawing.Point(3, 15);
			this.pnlTerrainsHeader.Name = "pnlTerrainsHeader";
			this.pnlTerrainsHeader.Size = new System.Drawing.Size(606, 95);
			this.pnlTerrainsHeader.TabIndex = 0;
			// 
			// lbl_ListAvailable
			// 
			this.lbl_ListAvailable.Location = new System.Drawing.Point(5, 15);
			this.lbl_ListAvailable.Name = "lbl_ListAvailable";
			this.lbl_ListAvailable.Size = new System.Drawing.Size(115, 20);
			this.lbl_ListAvailable.TabIndex = 1;
			this.lbl_ListAvailable.Text = "Available terrains in";
			this.lbl_ListAvailable.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// rb_CustomBasepath
			// 
			this.rb_CustomBasepath.Location = new System.Drawing.Point(435, 15);
			this.rb_CustomBasepath.Name = "rb_CustomBasepath";
			this.rb_CustomBasepath.Size = new System.Drawing.Size(125, 20);
			this.rb_CustomBasepath.TabIndex = 4;
			this.rb_CustomBasepath.TabStop = true;
			this.rb_CustomBasepath.Text = "Custom basepath";
			this.rb_CustomBasepath.UseVisualStyleBackColor = true;
			this.rb_CustomBasepath.CheckedChanged += new System.EventHandler(this.OnRadioTerrainChanged);
			// 
			// rb_TilesetBasepath
			// 
			this.rb_TilesetBasepath.Location = new System.Drawing.Point(310, 15);
			this.rb_TilesetBasepath.Name = "rb_TilesetBasepath";
			this.rb_TilesetBasepath.Size = new System.Drawing.Size(125, 20);
			this.rb_TilesetBasepath.TabIndex = 3;
			this.rb_TilesetBasepath.TabStop = true;
			this.rb_TilesetBasepath.Text = "Tileset basepath |";
			this.rb_TilesetBasepath.UseVisualStyleBackColor = true;
			this.rb_TilesetBasepath.CheckedChanged += new System.EventHandler(this.OnRadioTerrainChanged);
			// 
			// rb_ConfigBasepath
			// 
			this.rb_ConfigBasepath.Location = new System.Drawing.Point(155, 15);
			this.rb_ConfigBasepath.Name = "rb_ConfigBasepath";
			this.rb_ConfigBasepath.Size = new System.Drawing.Size(160, 20);
			this.rb_ConfigBasepath.TabIndex = 2;
			this.rb_ConfigBasepath.TabStop = true;
			this.rb_ConfigBasepath.Text = "Configurator basepath |";
			this.rb_ConfigBasepath.UseVisualStyleBackColor = true;
			this.rb_ConfigBasepath.CheckedChanged += new System.EventHandler(this.OnRadioTerrainChanged);
			// 
			// lbl_PathAllocated_
			// 
			this.lbl_PathAllocated_.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lbl_PathAllocated_.Location = new System.Drawing.Point(155, 65);
			this.lbl_PathAllocated_.Name = "lbl_PathAllocated_";
			this.lbl_PathAllocated_.Size = new System.Drawing.Size(445, 15);
			this.lbl_PathAllocated_.TabIndex = 9;
			this.lbl_PathAllocated_.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lbl_PathAvailable
			// 
			this.lbl_PathAvailable.Location = new System.Drawing.Point(5, 40);
			this.lbl_PathAvailable.Name = "lbl_PathAvailable";
			this.lbl_PathAvailable.Size = new System.Drawing.Size(140, 20);
			this.lbl_PathAvailable.TabIndex = 5;
			this.lbl_PathAvailable.Text = "Path (available terrains)";
			this.lbl_PathAvailable.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lbl_PathAllocated
			// 
			this.lbl_PathAllocated.Location = new System.Drawing.Point(5, 65);
			this.lbl_PathAllocated.Name = "lbl_PathAllocated";
			this.lbl_PathAllocated.Size = new System.Drawing.Size(140, 15);
			this.lbl_PathAllocated.TabIndex = 8;
			this.lbl_PathAllocated.Text = "Path (allocated terrain)";
			this.lbl_PathAllocated.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnFindBasepath
			// 
			this.btnFindBasepath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFindBasepath.Location = new System.Drawing.Point(575, 39);
			this.btnFindBasepath.Name = "btnFindBasepath";
			this.btnFindBasepath.Size = new System.Drawing.Size(25, 20);
			this.btnFindBasepath.TabIndex = 7;
			this.btnFindBasepath.Text = "...";
			this.btnFindBasepath.UseVisualStyleBackColor = true;
			this.btnFindBasepath.Visible = false;
			this.btnFindBasepath.Click += new System.EventHandler(this.OnFindTerrainClick);
			// 
			// tbTerrainPath
			// 
			this.tbTerrainPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tbTerrainPath.Location = new System.Drawing.Point(155, 40);
			this.tbTerrainPath.Name = "tbTerrainPath";
			this.tbTerrainPath.Size = new System.Drawing.Size(415, 19);
			this.tbTerrainPath.TabIndex = 6;
			this.tbTerrainPath.Text = "tbTerrainPath";
			this.tbTerrainPath.TextChanged += new System.EventHandler(this.OnTerrainPathChanged);
			// 
			// lblTerrainChanges
			// 
			this.lblTerrainChanges.Dock = System.Windows.Forms.DockStyle.Top;
			this.lblTerrainChanges.ForeColor = System.Drawing.Color.SlateBlue;
			this.lblTerrainChanges.Location = new System.Drawing.Point(0, 0);
			this.lblTerrainChanges.Name = "lblTerrainChanges";
			this.lblTerrainChanges.Size = new System.Drawing.Size(606, 15);
			this.lblTerrainChanges.TabIndex = 0;
			this.lblTerrainChanges.Text = "Changes to terrains take effect immediately.";
			this.lblTerrainChanges.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// lblAllocated
			// 
			this.lblAllocated.Location = new System.Drawing.Point(205, 80);
			this.lblAllocated.Name = "lblAllocated";
			this.lblAllocated.Size = new System.Drawing.Size(55, 15);
			this.lblAllocated.TabIndex = 10;
			this.lblAllocated.Text = "allocated";
			this.lblAllocated.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblAvailable
			// 
			this.lblAvailable.Location = new System.Drawing.Point(345, 80);
			this.lblAvailable.Name = "lblAvailable";
			this.lblAvailable.Size = new System.Drawing.Size(55, 15);
			this.lblAvailable.TabIndex = 11;
			this.lblAvailable.Text = "available";
			// 
			// pnlSpacer
			// 
			this.pnlSpacer.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.pnlSpacer.Controls.Add(this.btnTerrainClear);
			this.pnlSpacer.Controls.Add(this.btnTerrainPaste);
			this.pnlSpacer.Controls.Add(this.btnTerrainCopy);
			this.pnlSpacer.Controls.Add(this.btnMoveLeft);
			this.pnlSpacer.Controls.Add(this.btnMoveDown);
			this.pnlSpacer.Controls.Add(this.btnMoveRight);
			this.pnlSpacer.Controls.Add(this.btnMoveUp);
			this.pnlSpacer.Location = new System.Drawing.Point(280, 110);
			this.pnlSpacer.Name = "pnlSpacer";
			this.pnlSpacer.Size = new System.Drawing.Size(55, 190);
			this.pnlSpacer.TabIndex = 3;
			// 
			// btnTerrainClear
			// 
			this.btnTerrainClear.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnTerrainClear.Location = new System.Drawing.Point(5, 160);
			this.btnTerrainClear.Name = "btnTerrainClear";
			this.btnTerrainClear.Size = new System.Drawing.Size(45, 25);
			this.btnTerrainClear.TabIndex = 6;
			this.btnTerrainClear.Text = "Clear";
			this.btnTerrainClear.UseVisualStyleBackColor = true;
			this.btnTerrainClear.Click += new System.EventHandler(this.OnTerrainClearClick);
			// 
			// btnTerrainPaste
			// 
			this.btnTerrainPaste.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnTerrainPaste.Location = new System.Drawing.Point(5, 135);
			this.btnTerrainPaste.Name = "btnTerrainPaste";
			this.btnTerrainPaste.Size = new System.Drawing.Size(45, 25);
			this.btnTerrainPaste.TabIndex = 5;
			this.btnTerrainPaste.Text = "Paste";
			this.btnTerrainPaste.UseVisualStyleBackColor = true;
			this.btnTerrainPaste.Click += new System.EventHandler(this.OnTerrainPasteClick);
			// 
			// btnTerrainCopy
			// 
			this.btnTerrainCopy.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnTerrainCopy.Location = new System.Drawing.Point(5, 110);
			this.btnTerrainCopy.Name = "btnTerrainCopy";
			this.btnTerrainCopy.Size = new System.Drawing.Size(45, 25);
			this.btnTerrainCopy.TabIndex = 4;
			this.btnTerrainCopy.Text = "Copy";
			this.btnTerrainCopy.UseVisualStyleBackColor = true;
			this.btnTerrainCopy.Click += new System.EventHandler(this.OnTerrainCopyClick);
			// 
			// btnMoveLeft
			// 
			this.btnMoveLeft.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnMoveLeft.Enabled = false;
			this.btnMoveLeft.Location = new System.Drawing.Point(5, 0);
			this.btnMoveLeft.Name = "btnMoveLeft";
			this.btnMoveLeft.Size = new System.Drawing.Size(45, 25);
			this.btnMoveLeft.TabIndex = 0;
			this.btnMoveLeft.Text = "Left";
			this.btnMoveLeft.UseVisualStyleBackColor = true;
			this.btnMoveLeft.Click += new System.EventHandler(this.OnTerrainLeftClick);
			// 
			// btnMoveDown
			// 
			this.btnMoveDown.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnMoveDown.Enabled = false;
			this.btnMoveDown.Location = new System.Drawing.Point(5, 80);
			this.btnMoveDown.Name = "btnMoveDown";
			this.btnMoveDown.Size = new System.Drawing.Size(45, 25);
			this.btnMoveDown.TabIndex = 3;
			this.btnMoveDown.Text = "Down";
			this.btnMoveDown.UseVisualStyleBackColor = true;
			this.btnMoveDown.Click += new System.EventHandler(this.OnTerrainDownClick);
			// 
			// btnMoveRight
			// 
			this.btnMoveRight.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnMoveRight.Enabled = false;
			this.btnMoveRight.Location = new System.Drawing.Point(5, 25);
			this.btnMoveRight.Name = "btnMoveRight";
			this.btnMoveRight.Size = new System.Drawing.Size(45, 25);
			this.btnMoveRight.TabIndex = 1;
			this.btnMoveRight.Text = "Right";
			this.btnMoveRight.UseVisualStyleBackColor = true;
			this.btnMoveRight.Click += new System.EventHandler(this.OnTerrainRightClick);
			// 
			// btnMoveUp
			// 
			this.btnMoveUp.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.btnMoveUp.Enabled = false;
			this.btnMoveUp.Location = new System.Drawing.Point(5, 55);
			this.btnMoveUp.Name = "btnMoveUp";
			this.btnMoveUp.Size = new System.Drawing.Size(45, 25);
			this.btnMoveUp.TabIndex = 2;
			this.btnMoveUp.Text = "Up";
			this.btnMoveUp.UseVisualStyleBackColor = true;
			this.btnMoveUp.Click += new System.EventHandler(this.OnTerrainUpClick);
			// 
			// gbTileset
			// 
			this.gbTileset.Controls.Add(this.lblTilesetCount_);
			this.gbTileset.Controls.Add(this.lblTilesetCount);
			this.gbTileset.Controls.Add(this.btnCreateMap);
			this.gbTileset.Controls.Add(this.btnFindDirectory);
			this.gbTileset.Controls.Add(this.tbTileset);
			this.gbTileset.Controls.Add(this.lblTilesetMap);
			this.gbTileset.Controls.Add(this.btnFindTileset);
			this.gbTileset.Controls.Add(this.lblPathCurrent);
			this.gbTileset.Controls.Add(this.lblTilesetPath);
			this.gbTileset.Dock = System.Windows.Forms.DockStyle.Top;
			this.gbTileset.Location = new System.Drawing.Point(0, 50);
			this.gbTileset.Name = "gbTileset";
			this.gbTileset.Size = new System.Drawing.Size(612, 80);
			this.gbTileset.TabIndex = 1;
			this.gbTileset.TabStop = false;
			this.gbTileset.Text = "Tileset";
			// 
			// lblTilesetCount_
			// 
			this.lblTilesetCount_.Location = new System.Drawing.Point(210, 60);
			this.lblTilesetCount_.Name = "lblTilesetCount_";
			this.lblTilesetCount_.Size = new System.Drawing.Size(95, 15);
			this.lblTilesetCount_.TabIndex = 8;
			this.lblTilesetCount_.Text = "lblTilesetCount_";
			this.lblTilesetCount_.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblTilesetCount
			// 
			this.lblTilesetCount.Location = new System.Drawing.Point(10, 60);
			this.lblTilesetCount.Name = "lblTilesetCount";
			this.lblTilesetCount.Size = new System.Drawing.Size(195, 15);
			this.lblTilesetCount.TabIndex = 7;
			this.lblTilesetCount.Text = "Count of tilesets using Path+Map";
			this.lblTilesetCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnCreateMap
			// 
			this.btnCreateMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCreateMap.Location = new System.Drawing.Point(525, 30);
			this.btnCreateMap.Name = "btnCreateMap";
			this.btnCreateMap.Size = new System.Drawing.Size(50, 25);
			this.btnCreateMap.TabIndex = 5;
			this.btnCreateMap.Text = "Create";
			this.toolTip1.SetToolTip(this.btnCreateMap, "a Map descriptor must be created before terrains can be added");
			this.btnCreateMap.UseVisualStyleBackColor = true;
			this.btnCreateMap.Click += new System.EventHandler(this.OnCreateDescriptorClick);
			// 
			// btnFindDirectory
			// 
			this.btnFindDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFindDirectory.Location = new System.Drawing.Point(580, 10);
			this.btnFindDirectory.Name = "btnFindDirectory";
			this.btnFindDirectory.Size = new System.Drawing.Size(25, 20);
			this.btnFindDirectory.TabIndex = 2;
			this.btnFindDirectory.Text = "...";
			this.btnFindDirectory.UseVisualStyleBackColor = true;
			this.btnFindDirectory.Click += new System.EventHandler(this.OnFindDirectoryClick);
			// 
			// lblTilesetMap
			// 
			this.lblTilesetMap.Location = new System.Drawing.Point(10, 35);
			this.lblTilesetMap.Name = "lblTilesetMap";
			this.lblTilesetMap.Size = new System.Drawing.Size(40, 20);
			this.lblTilesetMap.TabIndex = 3;
			this.lblTilesetMap.Text = "Map";
			this.lblTilesetMap.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// btnFindTileset
			// 
			this.btnFindTileset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFindTileset.Location = new System.Drawing.Point(580, 30);
			this.btnFindTileset.Name = "btnFindTileset";
			this.btnFindTileset.Size = new System.Drawing.Size(25, 25);
			this.btnFindTileset.TabIndex = 6;
			this.btnFindTileset.Text = "...";
			this.btnFindTileset.UseVisualStyleBackColor = true;
			this.btnFindTileset.Click += new System.EventHandler(this.OnFindTilesetClick);
			// 
			// lblPathCurrent
			// 
			this.lblPathCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lblPathCurrent.Location = new System.Drawing.Point(55, 15);
			this.lblPathCurrent.Name = "lblPathCurrent";
			this.lblPathCurrent.Size = new System.Drawing.Size(525, 15);
			this.lblPathCurrent.TabIndex = 1;
			this.lblPathCurrent.Text = "lblPathCurrent";
			this.lblPathCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblTilesetPath
			// 
			this.lblTilesetPath.Location = new System.Drawing.Point(10, 15);
			this.lblTilesetPath.Name = "lblTilesetPath";
			this.lblTilesetPath.Size = new System.Drawing.Size(40, 15);
			this.lblTilesetPath.TabIndex = 0;
			this.lblTilesetPath.Text = "Path";
			this.lblTilesetPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// gbHeader
			// 
			this.gbHeader.Controls.Add(this.lblMcdRecords);
			this.gbHeader.Controls.Add(this.lblTilesetCurrent);
			this.gbHeader.Controls.Add(this.lblGroupCurrent);
			this.gbHeader.Controls.Add(this.lblHeaderGroup);
			this.gbHeader.Controls.Add(this.lblHeaderCategory);
			this.gbHeader.Controls.Add(this.lblCategoryCurrent);
			this.gbHeader.Dock = System.Windows.Forms.DockStyle.Top;
			this.gbHeader.Location = new System.Drawing.Point(0, 0);
			this.gbHeader.Name = "gbHeader";
			this.gbHeader.Size = new System.Drawing.Size(612, 50);
			this.gbHeader.TabIndex = 0;
			this.gbHeader.TabStop = false;
			this.gbHeader.Text = "Maptree";
			// 
			// lblMcdRecords
			// 
			this.lblMcdRecords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblMcdRecords.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblMcdRecords.ForeColor = System.Drawing.Color.Tan;
			this.lblMcdRecords.Location = new System.Drawing.Point(370, 30);
			this.lblMcdRecords.Name = "lblMcdRecords";
			this.lblMcdRecords.Size = new System.Drawing.Size(235, 15);
			this.lblMcdRecords.TabIndex = 5;
			this.lblMcdRecords.Text = "lblMcdRecords";
			this.lblMcdRecords.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblTilesetCurrent
			// 
			this.lblTilesetCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblTilesetCurrent.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTilesetCurrent.ForeColor = System.Drawing.Color.DarkSeaGreen;
			this.lblTilesetCurrent.Location = new System.Drawing.Point(370, 15);
			this.lblTilesetCurrent.Name = "lblTilesetCurrent";
			this.lblTilesetCurrent.Size = new System.Drawing.Size(235, 15);
			this.lblTilesetCurrent.TabIndex = 4;
			this.lblTilesetCurrent.Text = "lblTilesetCurrent";
			this.lblTilesetCurrent.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// lblGroupCurrent
			// 
			this.lblGroupCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lblGroupCurrent.Location = new System.Drawing.Point(90, 15);
			this.lblGroupCurrent.Name = "lblGroupCurrent";
			this.lblGroupCurrent.Size = new System.Drawing.Size(280, 15);
			this.lblGroupCurrent.TabIndex = 1;
			this.lblGroupCurrent.Text = "lblGroupCurrent";
			// 
			// lblHeaderCategory
			// 
			this.lblHeaderCategory.Location = new System.Drawing.Point(10, 30);
			this.lblHeaderCategory.Name = "lblHeaderCategory";
			this.lblHeaderCategory.Size = new System.Drawing.Size(65, 15);
			this.lblHeaderCategory.TabIndex = 2;
			this.lblHeaderCategory.Text = "CATEGORY";
			// 
			// lblCategoryCurrent
			// 
			this.lblCategoryCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lblCategoryCurrent.Location = new System.Drawing.Point(90, 30);
			this.lblCategoryCurrent.Name = "lblCategoryCurrent";
			this.lblCategoryCurrent.Size = new System.Drawing.Size(280, 15);
			this.lblCategoryCurrent.TabIndex = 3;
			this.lblCategoryCurrent.Text = "lblCategoryCurrent";
			// 
			// toolTip1
			// 
			this.toolTip1.AutoPopDelay = 10000;
			this.toolTip1.InitialDelay = 500;
			this.toolTip1.ReshowDelay = 100;
			this.toolTip1.UseAnimation = false;
			// 
			// MapTreeTilesetInputBox
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(612, 544);
			this.Controls.Add(this.pnlTop);
			this.Controls.Add(this.pnlBottom);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(480, 400);
			this.Name = "MapTreeTilesetInputBox";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.pnlBottom.ResumeLayout(false);
			this.pnlTop.ResumeLayout(false);
			this.gbTerrains.ResumeLayout(false);
			this.pnlTerrainsHeader.ResumeLayout(false);
			this.pnlTerrainsHeader.PerformLayout();
			this.pnlSpacer.ResumeLayout(false);
			this.gbTileset.ResumeLayout(false);
			this.gbTileset.PerformLayout();
			this.gbHeader.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label lblHeaderGroup;
		private System.Windows.Forms.TextBox tbTileset;
		private System.Windows.Forms.Panel pnlBottom;
		private System.Windows.Forms.Panel pnlTop;
		private System.Windows.Forms.Label lblCategoryCurrent;
		private System.Windows.Forms.Label lblGroupCurrent;
		private System.Windows.Forms.Label lblHeaderCategory;
		private System.Windows.Forms.Button btnFindTileset;
		private System.Windows.Forms.Label lblTilesetMap;
		private System.Windows.Forms.Label lblPathCurrent;
		private System.Windows.Forms.Label lblTilesetPath;
		private System.Windows.Forms.GroupBox gbTerrains;
		private System.Windows.Forms.GroupBox gbHeader;
		private System.Windows.Forms.GroupBox gbTileset;
		private System.Windows.Forms.ListBox lbTerrainsAvailable;
		private System.Windows.Forms.ListBox lbTerrainsAllocated;
		private System.Windows.Forms.Button btnMoveDown;
		private System.Windows.Forms.Button btnMoveUp;
		private System.Windows.Forms.Button btnMoveRight;
		private System.Windows.Forms.Button btnMoveLeft;
		private System.Windows.Forms.Panel pnlSpacer;
		private System.Windows.Forms.Button btnFindDirectory;
		private System.Windows.Forms.Button btnCreateMap;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label lblAvailable;
		private System.Windows.Forms.Label lblAllocated;
		private System.Windows.Forms.Panel pnlTerrainsHeader;
		private System.Windows.Forms.Label lblTilesetCurrent;
		private System.Windows.Forms.Label lblAddType;
		private System.Windows.Forms.Label lblTerrainChanges;
		private System.Windows.Forms.Button btnTerrainCopy;
		private System.Windows.Forms.Button btnTerrainPaste;
		private System.Windows.Forms.Label lblMcdRecords;
		private System.Windows.Forms.TextBox tbTerrainPath;
		private System.Windows.Forms.RadioButton rb_CustomBasepath;
		private System.Windows.Forms.RadioButton rb_TilesetBasepath;
		private System.Windows.Forms.RadioButton rb_ConfigBasepath;
		private System.Windows.Forms.Button btnFindBasepath;
		private System.Windows.Forms.Label lbl_PathAllocated;
		private System.Windows.Forms.Label lbl_PathAllocated_;
		private System.Windows.Forms.Label lbl_PathAvailable;
		private System.Windows.Forms.Button btnTerrainClear;
		private System.Windows.Forms.Label lbl_ListAvailable;
		private System.Windows.Forms.Label lblTilesetCount;
		private System.Windows.Forms.Label lblTilesetCount_;
	}
}
