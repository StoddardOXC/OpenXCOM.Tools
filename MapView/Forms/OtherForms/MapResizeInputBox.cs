using System;
using System.Windows.Forms;

using XCom.Interfaces.Base;
using XCom.Services;


namespace MapView
{
	internal sealed class MapResizeInputBox
		:
			Form
	{
		#region Fields & Properties
		private MapFileBase _mapBase;
		internal MapFileBase MapBase
		{
			get { return _mapBase; }
			set
			{
				if ((_mapBase = value) != null)
				{
					oldR.Text =
					txtR.Text = _mapBase.MapSize.Rows.ToString(System.Globalization.CultureInfo.InvariantCulture);
					oldC.Text =
					txtC.Text = _mapBase.MapSize.Cols.ToString(System.Globalization.CultureInfo.InvariantCulture);
					oldL.Text =
					txtL.Text = _mapBase.MapSize.Levs.ToString(System.Globalization.CultureInfo.InvariantCulture);
				}
			}
		}

		private int _rows;
		internal int Rows
		{
			get { return _rows; }
		}

		private int _cols;
		internal int Cols
		{
			get { return _cols; }
		}

		private int _levs;
		internal int Levs
		{
			get { return _levs; }
		}

		internal MapResizeService.MapResizeZtype ZType
		{
			get
			{
				if (cbCeiling.Enabled && cbCeiling.Checked)
					return MapResizeService.MapResizeZtype.MRZT_TOP;

				return MapResizeService.MapResizeZtype.MRZT_BOT;
			}
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		internal MapResizeInputBox()
		{
			InitializeComponent();

			btnCancel.Select();
			DialogResult = DialogResult.Cancel;
		}
		#endregion


		#region EventCalls
		private void OnOkClick(object sender, EventArgs e)
		{
			if (   String.IsNullOrEmpty(txtC.Text)
				|| String.IsNullOrEmpty(txtR.Text)
				|| String.IsNullOrEmpty(txtL.Text))
			{
				ShowErrorDialog("All fields must have a value.", "Error");
			}
			else if (!Int32.TryParse(txtC.Text, out _cols) || _cols < 1
				||   !Int32.TryParse(txtR.Text, out _rows) || _rows < 1
				||   !Int32.TryParse(txtL.Text, out _levs) || _levs < 1)
			{
				ShowErrorDialog("Values must be positive integers greater than 0.", "Error");
			}
			else if (_cols % 10 != 0 || _rows % 10 != 0)
			{
				ShowErrorDialog("Columns and Rows must be evenly divisible by 10.", "Error");
			}
			else if (_cols == int.Parse(oldC.Text, System.Globalization.CultureInfo.InvariantCulture)
				&&   _rows == int.Parse(oldR.Text, System.Globalization.CultureInfo.InvariantCulture)
				&&   _levs == int.Parse(oldL.Text, System.Globalization.CultureInfo.InvariantCulture))
			{
				ShowErrorDialog("The new size is the same as the old size.", "uh ...");
			}
			else // finally (sic) ->
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		/// <summary>
		/// Closes this form and discards any changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCancelClick(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Shows the AddToCeiling checkbox if the height has delta.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLevelTextChanged(object sender, EventArgs e)
		{
			if (!String.IsNullOrEmpty(txtL.Text)) // NOTE: btnOkClick will deal with a blank string.
			{
				int height;
				if (Int32.TryParse(txtL.Text, out height))
				{
					if (height > 0)
					{
						int delta = (height - _mapBase.MapSize.Levs);
						if (cbCeiling.Enabled = (delta != 0))
						{
							if (delta > 0)
							{
								cbCeiling.Text = "add to top";
							}
							else
								cbCeiling.Text = "subtract from top";
						}
					}
					else
						ShowErrorDialog("Height must be 1 or more.", "Error");
				}
				else
					ShowErrorDialog("Height must a positive integer.", "Error");
			}
		}
		#endregion


		#region Methods
		/// <summary>
		/// Wrapper for MessageBox.Show().
		/// </summary>
		/// <param name="error">the error string to show</param>
		/// <param name="caption">the dialog's caption text</param>
		private void ShowErrorDialog(
				string error,
				string caption)
		{
			MessageBox.Show(
						this,
						error,
						caption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button1,
						0);
		}
		#endregion


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
			this.oldC = new System.Windows.Forms.TextBox();
			this.oldR = new System.Windows.Forms.TextBox();
			this.oldL = new System.Windows.Forms.TextBox();
			this.txtR = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.txtC = new System.Windows.Forms.TextBox();
			this.txtL = new System.Windows.Forms.TextBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.cbCeiling = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// oldC
			// 
			this.oldC.Location = new System.Drawing.Point(20, 55);
			this.oldC.Name = "oldC";
			this.oldC.ReadOnly = true;
			this.oldC.Size = new System.Drawing.Size(45, 19);
			this.oldC.TabIndex = 4;
			// 
			// oldR
			// 
			this.oldR.Location = new System.Drawing.Point(70, 55);
			this.oldR.Name = "oldR";
			this.oldR.ReadOnly = true;
			this.oldR.Size = new System.Drawing.Size(45, 19);
			this.oldR.TabIndex = 5;
			// 
			// oldL
			// 
			this.oldL.Location = new System.Drawing.Point(120, 55);
			this.oldL.Name = "oldL";
			this.oldL.ReadOnly = true;
			this.oldL.Size = new System.Drawing.Size(45, 19);
			this.oldL.TabIndex = 6;
			// 
			// txtR
			// 
			this.txtR.Location = new System.Drawing.Point(70, 95);
			this.txtR.Name = "txtR";
			this.txtR.Size = new System.Drawing.Size(45, 19);
			this.txtR.TabIndex = 11;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(20, 40);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(45, 15);
			this.label3.TabIndex = 1;
			this.label3.Text = "c";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(70, 40);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(45, 15);
			this.label4.TabIndex = 2;
			this.label4.Text = "r";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(120, 40);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(45, 15);
			this.label5.TabIndex = 3;
			this.label5.Text = "L";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// txtC
			// 
			this.txtC.Location = new System.Drawing.Point(20, 95);
			this.txtC.Name = "txtC";
			this.txtC.Size = new System.Drawing.Size(45, 19);
			this.txtC.TabIndex = 10;
			// 
			// txtL
			// 
			this.txtL.Location = new System.Drawing.Point(120, 95);
			this.txtL.Name = "txtL";
			this.txtL.Size = new System.Drawing.Size(45, 19);
			this.txtL.TabIndex = 12;
			this.txtL.TextChanged += new System.EventHandler(this.OnLevelTextChanged);
			// 
			// btnOk
			// 
			this.btnOk.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnOk.Location = new System.Drawing.Point(125, 130);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(85, 40);
			this.btnOk.TabIndex = 14;
			this.btnOk.Text = "OK";
			this.btnOk.Click += new System.EventHandler(this.OnOkClick);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(220, 130);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(85, 40);
			this.btnCancel.TabIndex = 15;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.OnCancelClick);
			// 
			// cbCeiling
			// 
			this.cbCeiling.Checked = true;
			this.cbCeiling.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbCeiling.Enabled = false;
			this.cbCeiling.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbCeiling.Location = new System.Drawing.Point(170, 95);
			this.cbCeiling.Name = "cbCeiling";
			this.cbCeiling.Size = new System.Drawing.Size(120, 21);
			this.cbCeiling.TabIndex = 13;
			this.cbCeiling.Text = "top";
			this.cbCeiling.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(120, 80);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 15);
			this.label1.TabIndex = 9;
			this.label1.Text = "L";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(70, 80);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(45, 15);
			this.label6.TabIndex = 8;
			this.label6.Text = "r";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(20, 80);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(45, 15);
			this.label7.TabIndex = 7;
			this.label7.Text = "c";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(5, 5);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(305, 28);
			this.label2.TabIndex = 0;
			this.label2.Text = "Columns and Rows must be multiples of 10 (10, 20, 30, etc) and Levels must be 1 o" +
	"r more.";
			// 
			// MapResizeInputBox
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(314, 176);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.cbCeiling);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.txtL);
			this.Controls.Add(this.txtC);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.txtR);
			this.Controls.Add(this.oldC);
			this.Controls.Add(this.oldL);
			this.Controls.Add(this.oldR);
			this.Font = new System.Drawing.Font("Verdana", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MapResizeInputBox";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Modify Map Size";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.ComponentModel.Container components = null;

		private Label label1;
		private Label label3;
		private Label label4;
		private Label label5;
		private TextBox txtC;
		private TextBox txtR;
		private TextBox txtL;
		private Button btnOk;
		private Button btnCancel;
		private TextBox oldC;
		private TextBox oldR;
		private TextBox oldL;
		private Label label6;
		private Label label7;
		private Label label2;
		private CheckBox cbCeiling;
	}
}
