using System;
//using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using DSShared;

using XCom;

using YamlDotNet.Serialization; // write values (serialization)


namespace MapView
{
	internal sealed partial class ConfigurationForm
		:
			Form
	{
		#region Fields
		private PathInfo _pathResources = SharedSpace.Instance[PathInfo.ShareResources] as PathInfo;
		private PathInfo _pathTilesets  = SharedSpace.Instance[PathInfo.ShareTilesets]  as PathInfo;

		private bool _bork;
		#endregion


		#region Properties
		private string Ufo
		{
			get { return tbUfo.Text; }
			set { tbUfo.Text = value; }
		}

		private string Tftd
		{
			get { return tbTftd.Text; }
			set { tbTftd.Text = value; }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="restart">true if MapView needs to restart to affect
		/// changes (default false)</param>
		internal ConfigurationForm(bool restart = false)
		{
			InitializeComponent();

			if (restart)
			{
				toolTip1.SetToolTip(cbResources, "auto restart! Create paths to"
											+ " stock UFO/TFTD installations");
				toolTip1.SetToolTip(rbTilesets, "auto restart! WARNING : This will"
									+ " replace any custom tileset configuration");
			}

			// WORKAROUND: See note in 'XCMainWindow' cTor.
			var size = new System.Drawing.Size();
			size.Width  =
			size.Height = 0;
			MaximumSize = size; // fu.net

			if (!_pathResources.FileExists())
			{
				cbResources.Enabled = false;
			}
			else
				cbResources.Checked = false;

			if (!_pathTilesets.FileExists())
			{
				cbTilesets   .Enabled =
				rbTilesets   .Enabled =
				rbTilesetsTpl.Enabled = false;
			}
			else
			{
				cbTilesets.Checked = false;
				rbTilesetsTpl.Select();
			}


			// NOTE: Add your own personal XCOM resources-dir here if desired:
/*			var dirsUfo = new List<string>();
			dirsUfo.Add(@"C:\0xC_kL\data");
//			dirsUfo.Add(@"C:\MapView_test");

			foreach (string dir in dirsUfo)
			{
				if (Directory.Exists(dir))
				{
					Ufo = dir;
					break;
				}
			} */
		}
		#endregion


		#region Eventcalls
		private void OnResourcesCheckedChanged(object sender, EventArgs e)
		{
			gbResources.Visible = cbResources.Checked;

			btnOk.Enabled = cbResources.Checked
						 || cbTilesets .Checked;
		}

		private void OnTilesetsCheckedChanged(object sender, EventArgs e)
		{
			rbTilesets   .Visible =
			rbTilesetsTpl.Visible = cbTilesets.Checked;

			btnOk.Enabled = cbTilesets .Checked
						 || cbResources.Checked;
		}

		/// <summary>
		/// Opens a dialog to find a UFO installation folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFindUfoClick(object sender, EventArgs e)
		{
			using (var f = folderBrowser)
			{
				f.Description = "Select UFO directory";

				if (f.ShowDialog(this) == DialogResult.OK)
					Ufo = f.SelectedPath;
			}
		}

		/// <summary>
		/// Opens a dialog to find a TFTD installation folder.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFindTftdClick(object sender, EventArgs e)
		{
			using (var f = folderBrowser)
			{
				f.Description = "Select TFTD directory";

				if (f.ShowDialog(this) == DialogResult.OK)
					Tftd = f.SelectedPath;
			}
		}

		/// <summary>
		/// Applies new configuration settings and closes the form.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAcceptClick(object sender, EventArgs e)
		{
			_bork = false;

			if (cbResources.Checked) // handle XCOM resource path(s) configuration ->
			{
				Ufo  = Ufo.Trim();
				Tftd = Tftd.Trim();

				if (Ufo.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)) // NOTE: drive-root directories do funny things. Like append '\'
					Ufo = Ufo.Substring(0, Ufo.Length - 1);

				if (Tftd.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
					Tftd = Tftd.Substring(0, Tftd.Length - 1);

				if (String.IsNullOrEmpty(Ufo) && String.IsNullOrEmpty(Tftd))
				{
					ShowErrorDialog("Both folders cannot be blank.");
				}
				else if (!String.IsNullOrEmpty(Ufo) && !Directory.Exists(Ufo))
				{
					ShowErrorDialog("The UFO folder does not exist.");
				}
				else if (!String.IsNullOrEmpty(Tftd) && !Directory.Exists(Tftd))
				{
					ShowErrorDialog("The TFTD folder does not exist.");
				}
				else // check for a valid XCOM CursorSprite and create MapResources.yml ->
				{
					string CursorPck = SharedSpace.CursorFilePrefix + GlobalsXC.PckExt;
					string CursorTab = SharedSpace.CursorFilePrefix + GlobalsXC.TabExt;

					if (   (!File.Exists(Path.Combine(Ufo,  CursorPck)) || !File.Exists(Path.Combine(Ufo,  CursorTab)))
						&& (!File.Exists(Path.Combine(Tftd, CursorPck)) || !File.Exists(Path.Combine(Tftd, CursorTab))))
					{
						ShowErrorDialog("A valid UFO or TFTD resource directory must exist with"
											+ Environment.NewLine + Environment.NewLine
											+ CursorPck + Environment.NewLine
											+ CursorTab);
					}
					else
					{
						_pathResources.CreateDirectory();

						using (var fs = new FileStream(_pathResources.Fullpath, FileMode.Create))
						using (var sw = new StreamWriter(fs))
						{
							object node = new
							{
								ufo  = (!String.IsNullOrEmpty(Ufo)  ? Ufo
																	: PathInfo.NotConfigured),
								tftd = (!String.IsNullOrEmpty(Tftd) ? Tftd
																	: PathInfo.NotConfigured)
							};

							var ser = new Serializer();
							ser.Serialize(sw, node);
						}

						DialogResult = DialogResult.OK;
					}
				}
			}

			if (!_bork && cbTilesets.Checked) // deal with MapTilesets.yml/.tpl ->
			{
				_pathTilesets.CreateDirectory();

				string pfeTilesets = _pathTilesets.Fullpath;

				if (rbTilesets.Checked) // make a backup of the user's MapTilesets.yml if it exists.
				{
					if (File.Exists(pfeTilesets))
						File.Copy(pfeTilesets, Path.Combine(_pathTilesets.DirectoryPath, PathInfo.ConfigTilesetsOld), true);
				}
				else // rbTilesetsTpl.Checked
					pfeTilesets = Path.Combine(_pathTilesets.DirectoryPath, PathInfo.ConfigTilesetsTpl);

				using (var sr = new StreamReader(Assembly.GetExecutingAssembly()
												.GetManifestResourceStream("MapView._Embedded.MapTilesets.yml")))
				using (var fs = new FileStream(pfeTilesets, FileMode.Create))
				using (var sw = new StreamWriter(fs))
					while (sr.Peek() != -1)
						sw.WriteLine(sr.ReadLine());

				if (rbTilesets.Checked)
				{
					DialogResult = DialogResult.OK;
				}
				else // rbTilesetsTpl.Checked
				{
					ShowInfoDialog("Tileset template has been created "
									+ Environment.NewLine + Environment.NewLine
									+ pfeTilesets);

					if (!cbResources.Checked)
						Close();
				}
			}
		}

		/// <summary>
		/// Closes the form.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCancelClick(object sender, EventArgs e)
		{
			Close();
		}
		#endregion


		#region Methods
		/// <summary>
		/// Wrapper for MessageBox.Show()
		/// </summary>
		/// <param name="error">the error-string to show</param>
		private void ShowErrorDialog(string error)
		{
			_bork = true;
			MessageBox.Show(
						this,
						error,
						"Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button1,
						0);
		}

		/// <summary>
		/// Wrapper for MessageBox.Show()
		/// </summary>
		/// <param name="info">the info-string to show</param>
		private void ShowInfoDialog(string info)
		{
			MessageBox.Show(
						this,
						info,
						"Notice",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information,
						MessageBoxDefaultButton.Button1,
						0);
		}
		#endregion
	}
}
