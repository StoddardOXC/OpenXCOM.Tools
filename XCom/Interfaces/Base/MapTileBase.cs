using System;


namespace XCom.Interfaces.Base
{
	/// <summary>
	/// Objects of this class are drawn to the screen with the MainViewPanel.
	/// </summary>
	public abstract class MapTileBase
	{
		/// <summary>
		/// An array of TilepartBase[] in the correct draw order. This array
		/// should be iterated over when drawing to the screen.
		/// @note This is used only by MapFileBase.SaveGifFile().
		/// </summary>
		public abstract TilepartBase[] UsedParts
		{ get; }

		/// <summary>
		/// A tile is flagged as occulted if it has tiles with ground-parts
		/// above and to the south or east.
		/// </summary>
		public bool Occulted
		{ get; set; }
	}
}
