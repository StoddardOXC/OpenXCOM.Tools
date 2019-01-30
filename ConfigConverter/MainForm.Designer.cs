namespace ConfigConverter
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnConvert;
		private System.Windows.Forms.TextBox tbInput;
		private System.Windows.Forms.Button btnInput;
		private System.Windows.Forms.Label lblInput;
		private System.Windows.Forms.Label lblResult;
		private System.Windows.Forms.Label lblInfo;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
				components.Dispose();

			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor.
		/// The Forms designer might not be able to load this method if it was
		/// changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnConvert = new System.Windows.Forms.Button();
			this.tbInput = new System.Windows.Forms.TextBox();
			this.btnInput = new System.Windows.Forms.Button();
			this.lblInput = new System.Windows.Forms.Label();
			this.lblResult = new System.Windows.Forms.Label();
			this.lblInfo = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(240, 140);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(85, 30);
			this.btnCancel.TabIndex = 0;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.OnCancelClick);
			// 
			// btnConvert
			// 
			this.btnConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnConvert.Enabled = false;
			this.btnConvert.Location = new System.Drawing.Point(95, 140);
			this.btnConvert.Name = "btnConvert";
			this.btnConvert.Size = new System.Drawing.Size(85, 30);
			this.btnConvert.TabIndex = 1;
			this.btnConvert.Text = "Convert";
			this.btnConvert.UseVisualStyleBackColor = true;
			this.btnConvert.Click += new System.EventHandler(this.OnConvertClick);
			// 
			// tbInput
			// 
			this.tbInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.tbInput.Location = new System.Drawing.Point(5, 85);
			this.tbInput.Name = "tbInput";
			this.tbInput.ReadOnly = true;
			this.tbInput.Size = new System.Drawing.Size(380, 19);
			this.tbInput.TabIndex = 3;
			// 
			// btnInput
			// 
			this.btnInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnInput.Location = new System.Drawing.Point(385, 85);
			this.btnInput.Name = "btnInput";
			this.btnInput.Size = new System.Drawing.Size(27, 18);
			this.btnInput.TabIndex = 4;
			this.btnInput.Text = "...";
			this.btnInput.UseVisualStyleBackColor = true;
			this.btnInput.Click += new System.EventHandler(this.OnFindInputClick);
			// 
			// lblInput
			// 
			this.lblInput.Location = new System.Drawing.Point(5, 70);
			this.lblInput.Name = "lblInput";
			this.lblInput.Size = new System.Drawing.Size(360, 15);
			this.lblInput.TabIndex = 2;
			this.lblInput.Text = "File to convert";
			// 
			// lblResult
			// 
			this.lblResult.Location = new System.Drawing.Point(5, 115);
			this.lblResult.Name = "lblResult";
			this.lblResult.Size = new System.Drawing.Size(360, 15);
			this.lblResult.TabIndex = 5;
			// 
			// lblInfo
			// 
			this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.lblInfo.Location = new System.Drawing.Point(5, 10);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(405, 50);
			this.lblInfo.TabIndex = 6;
			this.lblInfo.Text = "This app inputs MapEdit.dat and converts it out to MapTilesets.yml, a YAML config" +
	"uration file for MapView 2+\r\n\r\nImages.dat and Paths.pth must be in the directory" +
	" with MapEdit.dat";
			this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// MainForm
			// 
			this.AcceptButton = this.btnConvert;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(417, 174);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnConvert);
			this.Controls.Add(this.tbInput);
			this.Controls.Add(this.lblInput);
			this.Controls.Add(this.lblResult);
			this.Controls.Add(this.lblInfo);
			this.Controls.Add(this.btnInput);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "MainForm";
			this.Text = "ConfigConverter2";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
