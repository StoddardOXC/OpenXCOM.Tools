using System;
using System.Collections.Generic;


namespace XCom
{
	public sealed class SharedSpace
	{
		#region Fields (static)
		public const string ApplicationDirectory  = "ApplicationDirectory";
		public const string SettingsDirectory     = "SettingsDirectory";
		public const string ResourceDirectoryUfo  = "ResourceDirectoryUfo";
		public const string ResourceDirectoryTftd = "ResourceDirectoryTftd";

		public const string Palettes              = "Palettes";

		public const string CursorFilePrefix      = @"UFOGRAPH/CURSOR"; // the cursor is determined in XCMainWindow.cTor
		#endregion


		#region Fields
		private readonly Dictionary<string, object> _share = new Dictionary<string, object>();
		#endregion


		#region Properties (static)
		/// <summary>
		/// Gets the currently instantiated SharedSpace from anywhere.
		/// </summary>
		private static SharedSpace _instance;
		public static SharedSpace Instance
		{
			get
			{
				if (_instance == null)
					_instance = new SharedSpace();

				return _instance;
			}
		}
		#endregion


		#region Properties
		/// <summary>
		/// Gets/Sets the value as an object or null.
		/// </summary>
		public object this[string key]
		{
			get { return (_share.ContainsKey(key)) ? _share[key] : null; }
			set { _share[key] = value; }
		}
		#endregion


//		public SharedSpace()
//		{}

		// TODO: Since SharedSpace holds only string-values factor away the
		// boxing and just use strings. Actually, Palettes is a dictionary ...
		// but it should be changed into a variable that's local to PckView
		// anyway.
		//
		// NOTE: which means that SharedSpace and PathInfo have very similar
		// usages and ought be merged.
		//
		// NOTE: PathInfo objects are returned as objects also.


		#region Methods
		/// <summary>
		/// Allocates a key-val pair in the SharedSpace and returns the value
		/// that is assigned. This does not change the value of an existing key
		/// unless its value is null.
		/// </summary>
		/// <param name="key">the key to look for</param>
		/// <param name="value">the object to add if the current value doesn't
		/// exist or is null</param>
		public void SetShare(string key, object value)
		{
			if (!_share.ContainsKey(key))
			{
				_share.Add(key, value);
			}
			else if (_share[key] == null)
			{
				_share[key] = value;
			}
		}

		/// <summary>
		/// Gets the value as a string.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetShare(string key)
		{
			return _share[key] as String;
		}
		#endregion
	}
}
