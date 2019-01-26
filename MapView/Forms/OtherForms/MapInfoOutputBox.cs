using System;
using System.Collections;
using System.Windows.Forms;

using XCom;
using XCom.Interfaces.Base;


namespace MapView
{
	internal sealed class MapInfoOutputBox
		:
			Form
	{
		internal MapInfoOutputBox()
		{
			InitializeComponent();

			btnCancel.Left = (groupAnalyze.Width - btnCancel.Width) / 2;
		}


		internal void Analyze(MapFileChild file)
		{
			groupInfo.Text = "Map: " + file.Descriptor.Label;

			lbl2Dimensions.Text = file.MapSize.Cols + ","
								+ file.MapSize.Rows + ","
								+ file.MapSize.Levs;

			lbl2Tilesets.Text = String.Empty;

			int recordsTotal = 0;
			int spritesTotal = 0;

			bool first = true;
			for (int i = 0; i != file.Terrains.Count; ++i)
			{
				if (first)
					first = false;
				else
					lbl2Tilesets.Text += ";";

				lbl2Tilesets.Text += file.Terrains[i].Item1;

				spritesTotal += file.Descriptor.GetSpriteCount(i);
				recordsTotal += file.Descriptor.GetRecordCount(i);
			}

			var width = TextRenderer.MeasureText(lbl2Tilesets.Text, lbl2Tilesets.Font).Width;
			if (width > lbl2Tilesets.Width)
				Width += width - lbl2Tilesets.Width;

			Refresh();
			groupAnalyze.Visible = true;

			int parts = 0;

			var recordsTable = new Hashtable();
			var spritesTable = new Hashtable();

			pBar.Maximum = file.MapSize.Cols * file.MapSize.Rows * file.MapSize.Levs;
			pBar.Value = 0;

			for (int col = 0; col != file.MapSize.Cols; ++col)
			for (int row = 0; row != file.MapSize.Rows; ++row)
			for (int lev = 0; lev != file.MapSize.Levs; ++lev)
			{
				var tile = file[row, col, lev] as XCMapTile;
				if (!tile.Vacant)
				{
					if (tile.Ground != null)
					{
						++parts;
						Count(tile.Ground, recordsTable, spritesTable);

						var part = tile.Ground as Tilepart;
						if (part != null)
							Count(part.Dead, recordsTable, spritesTable);
					}

					if (tile.West != null)
					{
						++parts;
						Count(tile.West, recordsTable, spritesTable);

						var part = tile.West as Tilepart;
						if (part != null)
							Count(part.Dead, recordsTable, spritesTable);
					}

					if (tile.North != null)
					{
						++parts;
						Count(tile.North, recordsTable, spritesTable);

						var part = tile.North as Tilepart;
						if (part != null)
							Count(part.Dead, recordsTable, spritesTable);
					}

					if (tile.Content != null)
					{
						++parts;
						Count(tile.Content, recordsTable, spritesTable);

						var part = tile.Content as Tilepart;
						if (part != null)
							Count(part.Dead, recordsTable, spritesTable);
					}
				}
				++pBar.Value;
				pBar.Refresh();
			}

			var pct = Math.Round(100.0 * (double)recordsTable.Keys.Count / (double)recordsTotal, 2);
			lbl2McdRecords.Text = recordsTable.Keys.Count + "/" + recordsTotal + " - " + pct + "%";

			pct = Math.Round(100.0 * (double)parts / (pBar.Maximum * 4), 2);
			lbl2PartsFilled.Text = parts + "/" + (pBar.Maximum * 4) + " - " + pct + "%";

			pct = Math.Round(100.0 * (double)spritesTable.Keys.Count / (double)spritesTotal, 2);
			lbl2PckImages.Text = spritesTable.Keys.Count + "/" + spritesTotal + " - " + pct + "%";


			groupAnalyze.Text = String.Empty; // hide the progress bar and show the Cancel btn.
			pBar.Visible = false;
			btnCancel.Visible = true;

			Refresh();
		}

		private static void Count(
				TilepartBase part,
				IDictionary recordsTable,
				IDictionary spritesTable)
		{
			if (part != null)
			{
				recordsTable[part.Record.SetId] = true;

				var sprites = part.Anisprites;
				foreach (PckImage sprite in sprites)
					spritesTable[sprite.SetId] = true;
			}
		}

		private void btnClose(object sender, EventArgs e)
		{
			Close();
		}


		#region Windows Form Designer generated code

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();

			base.Dispose(disposing);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lbl1Dimensions = new System.Windows.Forms.Label();
			this.lbl2Dimensions = new System.Windows.Forms.Label();
			this.lbl2Tilesets = new System.Windows.Forms.Label();
			this.lbl1Tilesets = new System.Windows.Forms.Label();
			this.lbl2PckImages = new System.Windows.Forms.Label();
			this.lbl1PckImages = new System.Windows.Forms.Label();
			this.lbl2McdRecords = new System.Windows.Forms.Label();
			this.lbl1McdRecords = new System.Windows.Forms.Label();
			this.lbl2PartsFilled = new System.Windows.Forms.Label();
			this.lbl1PartsFilled = new System.Windows.Forms.Label();
			this.pBar = new System.Windows.Forms.ProgressBar();
			this.groupAnalyze = new System.Windows.Forms.GroupBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.groupInfo = new System.Windows.Forms.GroupBox();
			this.groupAnalyze.SuspendLayout();
			this.groupInfo.SuspendLayout();
			this.SuspendLayout();
			// 
			// lbl1Dimensions
			// 
			this.lbl1Dimensions.Location = new System.Drawing.Point(10, 15);
			this.lbl1Dimensions.Name = "lbl1Dimensions";
			this.lbl1Dimensions.Size = new System.Drawing.Size(115, 15);
			this.lbl1Dimensions.TabIndex = 0;
			this.lbl1Dimensions.Text = "Dimensions (c,r,L)";
			// 
			// lbl2Dimensions
			// 
			this.lbl2Dimensions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lbl2Dimensions.Location = new System.Drawing.Point(130, 15);
			this.lbl2Dimensions.Name = "lbl2Dimensions";
			this.lbl2Dimensions.Size = new System.Drawing.Size(255, 15);
			this.lbl2Dimensions.TabIndex = 1;
			// 
			// lbl2Tilesets
			// 
			this.lbl2Tilesets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lbl2Tilesets.Location = new System.Drawing.Point(130, 30);
			this.lbl2Tilesets.Name = "lbl2Tilesets";
			this.lbl2Tilesets.Size = new System.Drawing.Size(255, 15);
			this.lbl2Tilesets.TabIndex = 3;
			// 
			// lbl1Tilesets
			// 
			this.lbl1Tilesets.Location = new System.Drawing.Point(10, 30);
			this.lbl1Tilesets.Name = "lbl1Tilesets";
			this.lbl1Tilesets.Size = new System.Drawing.Size(115, 15);
			this.lbl1Tilesets.TabIndex = 2;
			this.lbl1Tilesets.Text = "Tilesets";
			// 
			// lbl2PckImages
			// 
			this.lbl2PckImages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lbl2PckImages.Location = new System.Drawing.Point(130, 45);
			this.lbl2PckImages.Name = "lbl2PckImages";
			this.lbl2PckImages.Size = new System.Drawing.Size(255, 15);
			this.lbl2PckImages.TabIndex = 5;
			// 
			// lbl1PckImages
			// 
			this.lbl1PckImages.Location = new System.Drawing.Point(10, 45);
			this.lbl1PckImages.Name = "lbl1PckImages";
			this.lbl1PckImages.Size = new System.Drawing.Size(115, 15);
			this.lbl1PckImages.TabIndex = 4;
			this.lbl1PckImages.Text = "Sprites";
			// 
			// lbl2McdRecords
			// 
			this.lbl2McdRecords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lbl2McdRecords.Location = new System.Drawing.Point(130, 60);
			this.lbl2McdRecords.Name = "lbl2McdRecords";
			this.lbl2McdRecords.Size = new System.Drawing.Size(255, 15);
			this.lbl2McdRecords.TabIndex = 7;
			// 
			// lbl1McdRecords
			// 
			this.lbl1McdRecords.Location = new System.Drawing.Point(10, 60);
			this.lbl1McdRecords.Name = "lbl1McdRecords";
			this.lbl1McdRecords.Size = new System.Drawing.Size(115, 15);
			this.lbl1McdRecords.TabIndex = 6;
			this.lbl1McdRecords.Text = "MCD Records";
			// 
			// lbl2PartsFilled
			// 
			this.lbl2PartsFilled.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lbl2PartsFilled.Location = new System.Drawing.Point(130, 75);
			this.lbl2PartsFilled.Name = "lbl2PartsFilled";
			this.lbl2PartsFilled.Size = new System.Drawing.Size(255, 15);
			this.lbl2PartsFilled.TabIndex = 9;
			// 
			// lbl1PartsFilled
			// 
			this.lbl1PartsFilled.Location = new System.Drawing.Point(10, 75);
			this.lbl1PartsFilled.Name = "lbl1PartsFilled";
			this.lbl1PartsFilled.Size = new System.Drawing.Size(115, 15);
			this.lbl1PartsFilled.TabIndex = 8;
			this.lbl1PartsFilled.Text = "Tileparts";
			// 
			// pBar
			// 
			this.pBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pBar.Location = new System.Drawing.Point(5, 15);
			this.pBar.Name = "pBar";
			this.pBar.Size = new System.Drawing.Size(382, 25);
			this.pBar.TabIndex = 1;
			// 
			// groupAnalyze
			// 
			this.groupAnalyze.Controls.Add(this.btnCancel);
			this.groupAnalyze.Controls.Add(this.pBar);
			this.groupAnalyze.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.groupAnalyze.Location = new System.Drawing.Point(0, 100);
			this.groupAnalyze.Margin = new System.Windows.Forms.Padding(0);
			this.groupAnalyze.Name = "groupAnalyze";
			this.groupAnalyze.Padding = new System.Windows.Forms.Padding(5, 3, 5, 4);
			this.groupAnalyze.Size = new System.Drawing.Size(392, 44);
			this.groupAnalyze.TabIndex = 11;
			this.groupAnalyze.TabStop = false;
			this.groupAnalyze.Text = "Analyzing";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(0, 10);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(85, 30);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Visible = false;
			this.btnCancel.Click += new System.EventHandler(this.btnClose);
			// 
			// groupInfo
			// 
			this.groupInfo.Controls.Add(this.lbl2PckImages);
			this.groupInfo.Controls.Add(this.lbl1Tilesets);
			this.groupInfo.Controls.Add(this.lbl2Tilesets);
			this.groupInfo.Controls.Add(this.lbl1Dimensions);
			this.groupInfo.Controls.Add(this.lbl2McdRecords);
			this.groupInfo.Controls.Add(this.lbl1PckImages);
			this.groupInfo.Controls.Add(this.lbl1McdRecords);
			this.groupInfo.Controls.Add(this.lbl2PartsFilled);
			this.groupInfo.Controls.Add(this.lbl1PartsFilled);
			this.groupInfo.Controls.Add(this.lbl2Dimensions);
			this.groupInfo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupInfo.Location = new System.Drawing.Point(0, 3);
			this.groupInfo.Margin = new System.Windows.Forms.Padding(0);
			this.groupInfo.Name = "groupInfo";
			this.groupInfo.Size = new System.Drawing.Size(392, 97);
			this.groupInfo.TabIndex = 12;
			this.groupInfo.TabStop = false;
			this.groupInfo.Text = "Map";
			// 
			// MapInfoOutputBox
			// 
			this.AcceptButton = this.btnCancel;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(392, 144);
			this.Controls.Add(this.groupInfo);
			this.Controls.Add(this.groupAnalyze);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(400, 170);
			this.Name = "MapInfoOutputBox";
			this.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
			this.ShowIcon = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Map Info";
			this.groupAnalyze.ResumeLayout(false);
			this.groupInfo.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private System.ComponentModel.Container components = null;

		private GroupBox groupInfo;
		private Label lbl1Dimensions;
		private Label lbl2Dimensions;
		private Label lbl1Tilesets;
		private Label lbl2Tilesets;
		private Label lbl1PckImages;
		private Label lbl2PckImages;
		private Label lbl1McdRecords;
		private Label lbl2McdRecords;
		private Label lbl1PartsFilled;
		private Label lbl2PartsFilled;
		private GroupBox groupAnalyze;
		private ProgressBar pBar;
		private Button btnCancel;
	}
}
