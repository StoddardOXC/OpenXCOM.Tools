using System;
using System.Reflection;

using XCom;


namespace MapView
{
	internal static class Globals
	{
		#region Fields (static)
		internal const double ScaleMinimum = 0.25;
		internal const double ScaleMaximum = 3.00;

		internal const string ALLFILES = "*.*";
		#endregion


		#region Properties (static)
		/// <summary>
		/// A boolean that says that code is running in user-runtime mode rather
		/// than designer-mode. This is set true in MapView's XCMainWindow.cTor
		/// and is currently used only to stop the designer from trying to
		/// draw the QuadrantPanel in TopView and TopRouteView(Top). Because it
		/// won't.
		/// </summary>
		internal static bool RT
		{ get; set; }

		private static double _scale = 1.0;
		/// <summary>
		/// The scale-factor for sprites and clicks (etc) in MainView only.
		/// </summary>
		internal static double Scale
		{
			get { return _scale; }
			set { _scale = value; }
		}

		private static bool _autoScale = true;
		internal static bool AutoScale
		{
			get { return _autoScale; }
			set { _autoScale = value; }
		}

		internal static XCom.SpriteCollection ExtraSprites
		{ get; private set; }
		#endregion


		#region Methods (static)
		/// <summary>
		/// Loads the sprites for TopView's blank quads and TileView's eraser.
		/// </summary>
		internal static void LoadExtraSprites()
		{
			using (var fsPck = Assembly.GetExecutingAssembly()
							  .GetManifestResourceStream("MapView._Embedded.Extra.PCK"))
			using (var fsTab = Assembly.GetExecutingAssembly()
							  .GetManifestResourceStream("MapView._Embedded.Extra.TAB"))
			{
				ExtraSprites = new SpriteCollection(
												fsPck,
												fsTab,
												2,
												Palette.UfoBattle);
			}
		}

		/// <summary>
		/// Clamps a value between min and max inclusively. Note that no check
		/// is done to ensure that min is less than max.
		/// </summary>
		/// <param name="val"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns>min if val is less than min; max if value is greater than
		/// max; else the value itself</returns>
		internal static T Clamp<T>(
				this T val,
				T min,
				T max) where T
			:
				IComparable<T>
		{
			if (val.CompareTo(min) < 0)
				return min;

			if (val.CompareTo(max) > 0)
				return max;

			return val;
		}
		#endregion
	}
}
