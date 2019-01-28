using System;
using System.ComponentModel;
using System.Windows.Forms;


namespace XCom
{
	/// <summary>
	/// An outputbox that warns if a Tileset's total terrains has exceeded 254
	/// MCD-records. The IDs are stored in 1 byte but the first two records are
	/// reserved for the two BLANKS records. Note that this limit can be safely
	/// exceeded as long as no tileparts that exceed the limit have been placed
	/// on the Map.
	/// </summary>
	internal sealed class MapFileWarn
		:
			Form
	{
		#region Fields & Properties
		private static MapFileWarn _instance;
		internal static MapFileWarn Instance
		{
			get
			{
				if (_instance == null)
					_instance = new MapFileWarn();
				return _instance;
			}
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		private MapFileWarn()
		{
			InitializeComponent();
//			Select(); // MainView will take focus.
		}
		#endregion


		#region Methods (override)
		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();

			e.Cancel = true;
//			base.OnClosing(e);
		}
		#endregion


		#region Methods (events)
		private void btn_okClick(object sender, EventArgs e)
		{
			OnClosing(new CancelEventArgs());
		}
		#endregion


		#region Methods
		internal void SetText(string label, string text)
		{
			Text = "Mapfile Warning - " + label;
			rtb_Text.Text = text;
		}
		#endregion


		#region Windows Form Designer generated code

		private Container components = null;
		private Label lbl_InfoHeader;
		private Label lbl_InfoBody;
		private RichTextBox rtb_Text;
		private Button btn_Ok;

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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapFileWarn));
			this.btn_Ok = new System.Windows.Forms.Button();
			this.lbl_InfoHeader = new System.Windows.Forms.Label();
			this.rtb_Text = new System.Windows.Forms.RichTextBox();
			this.lbl_InfoBody = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btn_Ok
			// 
			this.btn_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btn_Ok.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn_Ok.Location = new System.Drawing.Point(220, 250);
			this.btn_Ok.Name = "btn_Ok";
			this.btn_Ok.Size = new System.Drawing.Size(75, 23);
			this.btn_Ok.TabIndex = 0;
			this.btn_Ok.Text = "&ok";
			this.btn_Ok.UseVisualStyleBackColor = true;
			this.btn_Ok.Click += new System.EventHandler(this.btn_okClick);
			// 
			// lbl_InfoHeader
			// 
			this.lbl_InfoHeader.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbl_InfoHeader.ForeColor = System.Drawing.Color.MediumVioletRed;
			this.lbl_InfoHeader.Location = new System.Drawing.Point(10, 10);
			this.lbl_InfoHeader.Name = "lbl_InfoHeader";
			this.lbl_InfoHeader.Size = new System.Drawing.Size(390, 15);
			this.lbl_InfoHeader.TabIndex = 1;
			this.lbl_InfoHeader.Text = "MCD records allocated by terrains exceeds 254.";
			// 
			// rtb_Text
			// 
			this.rtb_Text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_Text.BackColor = System.Drawing.SystemColors.Control;
			this.rtb_Text.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.rtb_Text.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rtb_Text.ForeColor = System.Drawing.SystemColors.ControlText;
			this.rtb_Text.Location = new System.Drawing.Point(20, 90);
			this.rtb_Text.Name = "rtb_Text";
			this.rtb_Text.Size = new System.Drawing.Size(380, 155);
			this.rtb_Text.TabIndex = 2;
			this.rtb_Text.Text = "text";
			// 
			// lbl_InfoBody
			// 
			this.lbl_InfoBody.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lbl_InfoBody.Location = new System.Drawing.Point(10, 30);
			this.lbl_InfoBody.Name = "lbl_InfoBody";
			this.lbl_InfoBody.Size = new System.Drawing.Size(390, 50);
			this.lbl_InfoBody.TabIndex = 3;
			this.lbl_InfoBody.Text = resources.GetString("lbl_InfoBody.Text");
			// 
			// MapFileWarn
			// 
			this.AcceptButton = this.btn_Ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this.btn_Ok;
			this.ClientSize = new System.Drawing.Size(402, 279);
			this.Controls.Add(this.lbl_InfoBody);
			this.Controls.Add(this.rtb_Text);
			this.Controls.Add(this.lbl_InfoHeader);
			this.Controls.Add(this.btn_Ok);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapFileWarn";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Mapfile Warning";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion
	}
}
