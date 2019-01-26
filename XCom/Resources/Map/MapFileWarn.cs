using System;
using System.ComponentModel;
using System.Windows.Forms;


namespace XCom
{
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
			Text = "MapFileWarn - " + label;
			rtb_text.Text = text;
		}
		#endregion


		#region Windows Form Designer generated code

		private Container components = null;
		private Button btn_ok;
		private Label lbl_info;
		private RichTextBox rtb_text;

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
			this.btn_ok = new System.Windows.Forms.Button();
			this.lbl_info = new System.Windows.Forms.Label();
			this.rtb_text = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// btn_ok
			// 
			this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn_ok.Location = new System.Drawing.Point(220, 190);
			this.btn_ok.Name = "btn_ok";
			this.btn_ok.Size = new System.Drawing.Size(75, 23);
			this.btn_ok.TabIndex = 0;
			this.btn_ok.Text = "&ok";
			this.btn_ok.UseVisualStyleBackColor = true;
			this.btn_ok.Click += new System.EventHandler(this.btn_okClick);
			// 
			// lbl_info
			// 
			this.lbl_info.ForeColor = System.Drawing.Color.MediumVioletRed;
			this.lbl_info.Location = new System.Drawing.Point(10, 10);
			this.lbl_info.Name = "lbl_info";
			this.lbl_info.Size = new System.Drawing.Size(270, 15);
			this.lbl_info.TabIndex = 1;
			this.lbl_info.Text = "MCD records allocated by terrains exceeds 256.";
			// 
			// rtb_text
			// 
			this.rtb_text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.rtb_text.BackColor = System.Drawing.SystemColors.Control;
			this.rtb_text.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.rtb_text.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rtb_text.ForeColor = System.Drawing.SystemColors.ControlText;
			this.rtb_text.Location = new System.Drawing.Point(20, 30);
			this.rtb_text.Name = "rtb_text";
			this.rtb_text.Size = new System.Drawing.Size(380, 155);
			this.rtb_text.TabIndex = 2;
			this.rtb_text.Text = "text";
			// 
			// MapFileWarn
			// 
			this.AcceptButton = this.btn_ok;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this.btn_ok;
			this.ClientSize = new System.Drawing.Size(402, 219);
			this.Controls.Add(this.rtb_text);
			this.Controls.Add(this.lbl_info);
			this.Controls.Add(this.btn_ok);
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
