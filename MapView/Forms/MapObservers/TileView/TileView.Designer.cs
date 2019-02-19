namespace MapView.Forms.MapObservers.TileViews
{
	partial class TileView
	{
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TileView));
			this.tcTileTypes = new System.Windows.Forms.TabControl();
			this.tpAll = new System.Windows.Forms.TabPage();
			this.tpFloors = new System.Windows.Forms.TabPage();
			this.tpWestwalls = new System.Windows.Forms.TabPage();
			this.tpNorthwalls = new System.Windows.Forms.TabPage();
			this.tpObjects = new System.Windows.Forms.TabPage();
			this.tsMain = new System.Windows.Forms.ToolStrip();
			this.tsddbPck = new System.Windows.Forms.ToolStripDropDownButton();
			this.tsmiEditPck = new System.Windows.Forms.ToolStripMenuItem();
			this.tsddbMcd = new System.Windows.Forms.ToolStripDropDownButton();
			this.tsmiMcdInfo = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiVolutarMcdEditor = new System.Windows.Forms.ToolStripMenuItem();
			this.tsddbEdit = new System.Windows.Forms.ToolStripDropDownButton();
			this.tsmiOptions = new System.Windows.Forms.ToolStripMenuItem();
			this.tcTileTypes.SuspendLayout();
			this.tsMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// tcTileTypes
			// 
			this.tcTileTypes.Controls.Add(this.tpAll);
			this.tcTileTypes.Controls.Add(this.tpFloors);
			this.tcTileTypes.Controls.Add(this.tpWestwalls);
			this.tcTileTypes.Controls.Add(this.tpNorthwalls);
			this.tcTileTypes.Controls.Add(this.tpObjects);
			this.tcTileTypes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tcTileTypes.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tcTileTypes.Location = new System.Drawing.Point(0, 25);
			this.tcTileTypes.Name = "tcTileTypes";
			this.tcTileTypes.SelectedIndex = 0;
			this.tcTileTypes.Size = new System.Drawing.Size(640, 455);
			this.tcTileTypes.TabIndex = 0;
			// 
			// tpAll
			// 
			this.tpAll.Location = new System.Drawing.Point(4, 21);
			this.tpAll.Name = "tpAll";
			this.tpAll.Size = new System.Drawing.Size(632, 430);
			this.tpAll.TabIndex = 0;
			this.tpAll.Text = "ALL";
			// 
			// tpFloors
			// 
			this.tpFloors.Location = new System.Drawing.Point(4, 21);
			this.tpFloors.Name = "tpFloors";
			this.tpFloors.Size = new System.Drawing.Size(632, 430);
			this.tpFloors.TabIndex = 1;
			this.tpFloors.Text = "fLoOr";
			// 
			// tpWestwalls
			// 
			this.tpWestwalls.Location = new System.Drawing.Point(4, 21);
			this.tpWestwalls.Name = "tpWestwalls";
			this.tpWestwalls.Size = new System.Drawing.Size(632, 430);
			this.tpWestwalls.TabIndex = 2;
			this.tpWestwalls.Text = "WEst";
			// 
			// tpNorthwalls
			// 
			this.tpNorthwalls.Location = new System.Drawing.Point(4, 21);
			this.tpNorthwalls.Name = "tpNorthwalls";
			this.tpNorthwalls.Size = new System.Drawing.Size(632, 430);
			this.tpNorthwalls.TabIndex = 3;
			this.tpNorthwalls.Text = "noRtH";
			// 
			// tpObjects
			// 
			this.tpObjects.Location = new System.Drawing.Point(4, 21);
			this.tpObjects.Name = "tpObjects";
			this.tpObjects.Size = new System.Drawing.Size(632, 430);
			this.tpObjects.TabIndex = 4;
			this.tpObjects.Text = "ConTeNt";
			// 
			// tsMain
			// 
			this.tsMain.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsddbPck,
			this.tsddbMcd,
			this.tsddbEdit});
			this.tsMain.Location = new System.Drawing.Point(0, 0);
			this.tsMain.Name = "tsMain";
			this.tsMain.Size = new System.Drawing.Size(640, 25);
			this.tsMain.TabIndex = 1;
			this.tsMain.Text = "tsMain";
			// 
			// tsddbPck
			// 
			this.tsddbPck.AutoToolTip = false;
			this.tsddbPck.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsddbPck.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsmiEditPck});
			this.tsddbPck.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tsddbPck.Image = ((System.Drawing.Image)(resources.GetObject("tsddbPck.Image")));
			this.tsddbPck.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsddbPck.Name = "tsddbPck";
			this.tsddbPck.Size = new System.Drawing.Size(40, 22);
			this.tsddbPck.Text = "PCK";
			// 
			// tsmiEditPck
			// 
			this.tsmiEditPck.Name = "tsmiEditPck";
			this.tsmiEditPck.Size = new System.Drawing.Size(213, 22);
			this.tsmiEditPck.Text = "Open spriteset in PckView";
			this.tsmiEditPck.Click += new System.EventHandler(this.OnPckEditorClick);
			// 
			// tsddbMcd
			// 
			this.tsddbMcd.AutoToolTip = false;
			this.tsddbMcd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsddbMcd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsmiMcdInfo,
			this.tsmiVolutarMcdEditor});
			this.tsddbMcd.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tsddbMcd.Image = ((System.Drawing.Image)(resources.GetObject("tsddbMcd.Image")));
			this.tsddbMcd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsddbMcd.Name = "tsddbMcd";
			this.tsddbMcd.Size = new System.Drawing.Size(43, 22);
			this.tsddbMcd.Text = "MCD";
			// 
			// tsmiMcdInfo
			// 
			this.tsmiMcdInfo.Name = "tsmiMcdInfo";
			this.tsmiMcdInfo.Size = new System.Drawing.Size(182, 22);
			this.tsmiMcdInfo.Text = "Tilepart Information";
			this.tsmiMcdInfo.Click += new System.EventHandler(this.OnMcdInfoClick);
			// 
			// tsmiVolutarMcdEditor
			// 
			this.tsmiVolutarMcdEditor.Name = "tsmiVolutarMcdEditor";
			this.tsmiVolutarMcdEditor.Size = new System.Drawing.Size(182, 22);
			this.tsmiVolutarMcdEditor.Text = "Volutar MCD Editor";
			this.tsmiVolutarMcdEditor.Click += new System.EventHandler(this.OnVolutarMcdEditorClick);
			// 
			// tsddbEdit
			// 
			this.tsddbEdit.AutoToolTip = false;
			this.tsddbEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsddbEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.tsmiOptions});
			this.tsddbEdit.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tsddbEdit.Image = ((System.Drawing.Image)(resources.GetObject("tsddbEdit.Image")));
			this.tsddbEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsddbEdit.Name = "tsddbEdit";
			this.tsddbEdit.Size = new System.Drawing.Size(38, 22);
			this.tsddbEdit.Text = "Edit";
			// 
			// tsmiOptions
			// 
			this.tsmiOptions.Name = "tsmiOptions";
			this.tsmiOptions.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.tsmiOptions.Size = new System.Drawing.Size(154, 22);
			this.tsmiOptions.Text = "Options";
			this.tsmiOptions.Click += new System.EventHandler(this.OnOptionsClick);
			// 
			// TileView
			// 
			this.Controls.Add(this.tcTileTypes);
			this.Controls.Add(this.tsMain);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "TileView";
			this.Size = new System.Drawing.Size(640, 480);
			this.tcTileTypes.ResumeLayout(false);
			this.tsMain.ResumeLayout(false);
			this.tsMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.ComponentModel.IContainer components = null;

		private System.Windows.Forms.ToolStrip tsMain;
		private System.Windows.Forms.ToolStripDropDownButton tsddbMcd;
		private System.Windows.Forms.ToolStripMenuItem tsmiMcdInfo;
		private System.Windows.Forms.ToolStripDropDownButton tsddbPck;
		private System.Windows.Forms.ToolStripMenuItem tsmiEditPck;
		private System.Windows.Forms.ToolStripDropDownButton tsddbEdit;
		private System.Windows.Forms.ToolStripMenuItem tsmiOptions;
		private System.Windows.Forms.ToolStripMenuItem tsmiVolutarMcdEditor;
		private System.Windows.Forms.TabControl tcTileTypes;
		private System.Windows.Forms.TabPage tpAll;
		private System.Windows.Forms.TabPage tpFloors;
		private System.Windows.Forms.TabPage tpObjects;
		private System.Windows.Forms.TabPage tpNorthwalls;
		private System.Windows.Forms.TabPage tpWestwalls;
	}
}
