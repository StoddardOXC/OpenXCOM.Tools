using System;
using System.ComponentModel;
using System.Windows.Forms;


namespace XCom.Resources.Map.RouteData
{
	/// <summary>
	/// A form that outputs data about out-of-bounds route-nodes and allows user
	/// to delete said nodes.
	/// </summary>
	internal sealed class RouteCheckInfobox
		:
			Form
	{
		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal RouteCheckInfobox()
		{
			InitializeComponent();

			DialogResult = DialogResult.No;
		}
		#endregion


		#region Methods (events)
		private void btn_AcceptClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
		}
		#endregion


		#region Methods
		internal void SetText(string label, string text)
		{
			lbl_InfoBody.Text = label;
			rtb_Text    .Text = text;
		}
		#endregion


		#region Windows Form Designer generated code

		private Container components = null;
		private Label lbl_InfoBody;
		private RichTextBox rtb_Text;
		private Button btn_Yes;
		private Button btn_No;

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
			this.btn_Yes = new System.Windows.Forms.Button();
			this.rtb_Text = new System.Windows.Forms.RichTextBox();
			this.lbl_InfoBody = new System.Windows.Forms.Label();
			this.btn_No = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// btn_Yes
			// 
			this.btn_Yes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btn_Yes.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn_Yes.Location = new System.Drawing.Point(220, 250);
			this.btn_Yes.Name = "btn_Yes";
			this.btn_Yes.Size = new System.Drawing.Size(75, 23);
			this.btn_Yes.TabIndex = 2;
			this.btn_Yes.Text = "yep";
			this.btn_Yes.UseVisualStyleBackColor = true;
			this.btn_Yes.Click += new System.EventHandler(this.btn_AcceptClick);
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
			this.rtb_Text.Location = new System.Drawing.Point(25, 65);
			this.rtb_Text.Name = "rtb_Text";
			this.rtb_Text.ReadOnly = true;
			this.rtb_Text.ShortcutsEnabled = false;
			this.rtb_Text.Size = new System.Drawing.Size(370, 180);
			this.rtb_Text.TabIndex = 1;
			this.rtb_Text.Text = "";
			// 
			// lbl_InfoBody
			// 
			this.lbl_InfoBody.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lbl_InfoBody.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lbl_InfoBody.Location = new System.Drawing.Point(10, 10);
			this.lbl_InfoBody.Name = "lbl_InfoBody";
			this.lbl_InfoBody.Size = new System.Drawing.Size(390, 45);
			this.lbl_InfoBody.TabIndex = 0;
			// 
			// btn_No
			// 
			this.btn_No.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btn_No.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn_No.Location = new System.Drawing.Point(310, 250);
			this.btn_No.Name = "btn_No";
			this.btn_No.Size = new System.Drawing.Size(75, 23);
			this.btn_No.TabIndex = 3;
			this.btn_No.Text = "negatory";
			this.btn_No.UseVisualStyleBackColor = true;
			// 
			// RouteCheckInfobox
			// 
			this.AcceptButton = this.btn_Yes;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this.btn_No;
			this.ClientSize = new System.Drawing.Size(402, 279);
			this.Controls.Add(this.btn_No);
			this.Controls.Add(this.lbl_InfoBody);
			this.Controls.Add(this.rtb_Text);
			this.Controls.Add(this.btn_Yes);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RouteCheckInfobox";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Node check";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion
	}
}
