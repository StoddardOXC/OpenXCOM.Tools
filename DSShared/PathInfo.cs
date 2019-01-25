using System;
using System.IO;


namespace DSShared
{
	/// <summary>
	/// Class to help pass around (/settings) file-paths.
	/// </summary>
	public class PathInfo
	{
		#region Fields (static)
		// path-keys in SharedSpace
		public const string ShareOptions   = "MV_OptionsFile";		// -> MapOptions.cfg

		public const string ShareResources = "MV_ResourcesFile";	// -> MapResources.yml
		public const string ShareTilesets  = "MV_TilesetsFile";		// -> MapTilesets.yml
		public const string ShareViewers   = "MV_ViewersFile";		// -> MapViewers.yml

		// Configuration files
		public const string ConfigOptions     = "MapOptions.cfg";	// stores user-settings for the viewers

		public const string ConfigResources   = "MapResources.yml";	// stores the installation paths of UFO/TFTD

		public const string ConfigTilesets    = "MapTilesets.yml";	// tilesets file configuration
		public const string ConfigTilesetsOld = "MapTilesets.old";	// tilesets file backup
		public const string ConfigTilesetsTpl = "MapTilesets.tpl";	// tilesets file template

		public const string ConfigViewers     = "MapViewers.yml";	// various window positions and sizes
		public const string ConfigViewersOld  = "MapViewers.old";	// backup


		public const string NotConfigured = "notconfigured"; // used in MapResources.yml in case UFO or TFTD installation is not configured.

		public const string SettingsDirectory = "settings";
		#endregion


		#region Properties
		private readonly string _file;
//		private readonly string _extension;

		private readonly string _path;
		/// <summary>
		/// Directory path.
		/// </summary>
		public string DirectoryPath
		{
			get { return _path; }
		}

		/// <summary>
		/// Gets the fullpath.
		/// kL_question: Can a file or directory end with "." (no, disallowed by Windows OS)
		/// </summary>
		public string Fullpath
		{
			get
			{
				return Path.Combine(_path, _file);

//				string fullpath = Path.Combine(_path, _file);
//				if (_extension.Length != 0)
//					fullpath += "." + _extension;
//				return fullpath;
			}
		}
		#endregion


		#region cTor
		/// <summary>
		/// Initializes a new instance of the <see cref="T:DSShared.PathInfo"/> class.
		/// </summary>
		/// <param name="path">the path</param>
		/// <param name="file">the file with any extension</param>
		public PathInfo(
				string path,
				string file)
		{
			_path = path;
			_file = file;
		}
		#endregion


		#region Methods
		/// <summary>
		/// Checks if the file exists.
		/// </summary>
		/// <returns></returns>
		public bool FileExists()
		{
			return File.Exists(Fullpath);
		}

		/// <summary>
		/// Creates the directory if it does not exist.
		/// </summary>
		public void CreateDirectory()
		{
			Directory.CreateDirectory(_path);
		}
		#endregion
	}
}

//		/// <summary>
//		/// Initializes a new instance of the <see cref="T:DSShared.PathInfo"/> class.
//		/// </summary>
//		/// <param name="fullPath">the full path</param>
//		public PathInfo(string fullPath)
//			:
//				this(fullPath, true)
//		{}

//		/// <summary>
//		/// Initializes a new instance of the <see cref="T:DSShared.PathInfo"/> class.
//		/// </summary>
//		/// <param name="fullPath">the full path</param>
//		/// <param name="parseFile">if set to <c>true</c> the path will be broken down into
//		/// filename and extension parts. You should pass false if the path string does not
//		/// describe a file location</param>
//		public PathInfo(string fullPath, bool parseFile)
//		{
//			if (parseFile && fullPath.IndexOf(".", StringComparison.Ordinal) > 0)
//			{
//				_path = fullPath.Substring(0, fullPath.LastIndexOf(@"\", StringComparison.Ordinal));
//				_file = fullPath.Substring(fullPath.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
//				_file = _file.Substring(0, _file.LastIndexOf(".", StringComparison.Ordinal));
//				_ext  = fullPath.Substring(fullPath.LastIndexOf(".", StringComparison.Ordinal) + 1);
//			}
//			else
//			{
//				_path = fullPath;
//				_file = String.Empty;
//				_ext  = String.Empty;
//			}
//		}

//		/// <summary>
//		/// Extension part of the path.
//		/// </summary>
//		public string Ext
//		{
//			get { return _ext; }
//			set { _ext = value; }
//		}

//		/// <summary>
//		/// Filename part of the path without extension.
//		/// </summary>
//		public string File
//		{
//			get { return _file; }
//			set { _file = value; }
//		}

//		/// <summary>
//		/// String representation of this path with the supplied extension added
//		/// on instead of the one the object was constructed with.
//		/// </summary>
//		/// <param name="ext">the extension that will replace the current one</param>
//		/// <returns></returns>
//		public string ToString(string ext)
//		{
//			return _path + @"\" + _file + "." + ext;
//		}
