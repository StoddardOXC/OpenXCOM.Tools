using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using YamlDotNet.RepresentationModel; // read values (deserialization)


namespace DSShared.Windows
{
	/// <summary>
	/// A class to help facilitate the saving and loading of values into the
	/// registry in a central location.
	/// NOTE: MapViewII does not use the Windows Registry at all - this info is
	/// rather for initialization/configuration files.
	/// </summary>
	public sealed class RegistryInfo
	{
		#region Fields (static)
		// viewer labels (keys)
		public const string MainView      = "MainView";
		public const string TopView       = "TopView";
		public const string RouteView     = "RouteView";
		public const string TopRouteView  = "TopRouteView";
		public const string TileView      = "TileView";

		public const string TilesetEditor = "TilesetEditor";

		public const string Options       = "Options";
		public const string Console       = "Console";

		public const string PckView       = "PckView";
		public const string SpriteEditor  = "SpriteEditor";
		public const string PaletteViewer = "PaletteViewer";

		// viewer property metrics
		private const string PropLeft   = "Left";
		private const string PropTop    = "Top";
		private const string PropWidth  = "Width";
		private const string PropHeight = "Height";
		#endregion


		#region Fields
		private readonly string _viewer;
		private readonly Form _f;

		private readonly Dictionary<string, PropertyInfo> _infoDictionary = new Dictionary<string, PropertyInfo>();

//		private bool _saveOnClose = true;
		#endregion


		#region cTor
		/// <summary>
		/// Main cTor. Uses the specified string as a key to its Form.
		/// </summary>
		/// <param name="viewer">the label of the viewer to save/load</param>
		/// <param name="f">the form-object corresponding to the label</param>
		public RegistryInfo(string viewer, Form f)
		{
			_viewer = viewer;

			_f = f;
			_f.StartPosition = FormStartPosition.Manual;
			_f.Load         += OnLoad;
			_f.FormClosing  += OnFormClosing;
		}
		#endregion


		#region Methods
		/// <summary>
		/// Adds properties to be saved/loaded.
		/// </summary>
		public void RegisterProperties()
		{
			//DSLogFile.WriteLine("RegisterProperties");
			PropertyInfo info;

			string[] keys =
			{
				PropLeft,
				PropTop,
				PropWidth,
				PropHeight
			};

			foreach (string key in keys)
			{
				//DSLogFile.WriteLine(". . key= " + key);
				if ((info = _f.GetType().GetProperty(key)) != null) // safety.
				{
					//DSLogFile.WriteLine(". . . info= " + info.Name);
					_infoDictionary[info.Name] = info; // set a ref to each metric (x,y,w,h) via Reflection.
				}
			}
		}
		#endregion


		#region Eventcalls
		/// <summary>
		/// Loads values for the subsidiary viewers from "MapViewers.yml".
		/// - TopView       (size and position, but not Quadrant visibilities)
		/// - RouteView     (size and position)
		/// - TopRouteView  (size and position)
		/// - TileView      (size and position)
		/// - Console       (size and position)
		/// - TilesetEditor (size and position)
		/// - SpriteEditor  (size and position)
		/// - PaletteViewer (size and position)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLoad(object sender, EventArgs e)
		{
			string dirSettings = Path.Combine(
											Path.GetDirectoryName(Application.ExecutablePath),
											PathInfo.SettingsDirectory);
			string pfeViewers  = Path.Combine(
											dirSettings,
											PathInfo.ConfigViewers);
			if (File.Exists(pfeViewers))
			{
				using (var sr = new StreamReader(File.OpenRead(pfeViewers)))
				{
					var fileViewers = new YamlStream();
					fileViewers.Load(sr);

					var nodeRoot = (YamlMappingNode)fileViewers.Documents[0].RootNode; // TODO: Error handling. ->
					foreach (var node in nodeRoot.Children)
					{
						string viewer = ((YamlScalarNode)node.Key).Value;
						if (String.Equals(viewer, _viewer, StringComparison.Ordinal))
						{
							LoadWindowMetrics((YamlMappingNode)nodeRoot.Children[new YamlScalarNode(viewer)]);
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Helper for OnLoad().
		/// </summary>
		/// <param name="keyvals">yaml-mapped keyval pairs</param>
		private void LoadWindowMetrics(YamlMappingNode keyvals)
		{
			//DSLogFile.WriteLine("ImportValues");
			string key;
			int val;

			var cultureInfo = CultureInfo.InvariantCulture;

			foreach (var keyval in keyvals)
			{
				key = cultureInfo.TextInfo.ToTitleCase(keyval.Key.ToString());
				//DSLogFile.WriteLine(". key= " + key);

				val = Int32.Parse(keyval.Value.ToString(), cultureInfo);
				//DSLogFile.WriteLine(". val= " + val);

				switch (key) // check to ensure that viewer is at least partly onscreen.
				{
					case PropLeft:
					{
						var rectScreen = Screen.GetWorkingArea(new System.Drawing.Point(val, 0));
						if (!rectScreen.Contains(val + 200, 0))
							val = 100;
						break;
					}

					case PropTop:
					{
						var rectScreen = Screen.GetWorkingArea(new System.Drawing.Point(0, val));
						if (!rectScreen.Contains(0, val + 100))
							val = 50;
						break;
					}
				}
				_infoDictionary[key].SetValue(_f, val, null); // set each metric (x,y,w,h) via Reflection.
			}
		}

		/// <summary>
		/// Saves values for the subsidiary viewers to "MapViewers.yml".
		/// - TopView       (size and position, but does not save Quadrant visibilities)
		/// - RouteView     (size and position)
		/// - TopRouteView  (size and position)
		/// - TileView      (size and position)
		/// - Console       (size and position)
		/// - TilesetEditor (size and position)
		/// - SpriteEditor  (size and position)
		/// - PaletteViewer (size and position)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnFormClosing(object sender, EventArgs e)
		{
			//DSLogFile.WriteLine("OnClose _viewer= " + _viewer);

			_f.WindowState = FormWindowState.Normal;

//			if (_saveOnClose)
//			{
			string dirSettings   = Path.Combine(
											Path.GetDirectoryName(Application.ExecutablePath),
											PathInfo.SettingsDirectory);
			string pfeViewers    = Path.Combine(
											dirSettings,
											PathInfo.ConfigViewers);
			string pfeViewersOld = Path.Combine(
											dirSettings,
											PathInfo.ConfigViewersOld);

			if (File.Exists(pfeViewers))
			{
				File.Copy(pfeViewers, pfeViewersOld, true);

				using (var sr = new StreamReader(File.OpenRead(pfeViewersOld)))	// but now use dst as src ->
				using (var fs = new FileStream(pfeViewers, FileMode.Create))	// overwrite previous config.
				using (var sw = new StreamWriter(fs))
				{
					bool found = false;

					while (sr.Peek() != -1)
					{
						string line = sr.ReadLine().TrimEnd();
						//DSLogFile.WriteLine(". line= " + line);

						// At present, MainView and PckView are the only viewers that roll their own metrics.
						// - see the XCMainWindow cTor & FormClosing eventcalls.
						// - see the PckViewForm  cTor & FormClosing eventcalls.

						if (String.Equals(line, _viewer + ":", StringComparison.Ordinal))
						{
							found = true;

							//DSLogFile.WriteLine(". . write= " + line);
							sw.WriteLine(line);

							line = sr.ReadLine();
							line = sr.ReadLine();
							line = sr.ReadLine();
							line = sr.ReadLine(); // heh

							sw.WriteLine("  left: "   + Math.Max(0, _f.Location.X)); // =Left
							sw.WriteLine("  top: "    + Math.Max(0, _f.Location.Y)); // =Top
							sw.WriteLine("  width: "  + _f.Width);
							sw.WriteLine("  height: " + _f.Height);
						}
						else
							sw.WriteLine(line);
					}

					if (!found)
					{
						sw.WriteLine(_viewer + ":");

						sw.WriteLine("  left: "   + Math.Max(0, _f.Location.X)); // =Left
						sw.WriteLine("  top: "    + Math.Max(0, _f.Location.Y)); // =Top
						sw.WriteLine("  width: "  + _f.Width);
						sw.WriteLine("  height: " + _f.Height);
					}
				}
				File.Delete(pfeViewersOld);
			}
//			}
		}
		#endregion
	}
}
