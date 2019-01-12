using System;


namespace XCom.Interfaces.Base
{
	/// <summary>
	/// Provides all the necessary information to animate a tilepart. No it
	/// doesn't.
	/// </summary>
	public class TilepartBase
	{
		/// <summary>
		/// The object that has information about the IG mechanics of this tile.
		/// </summary>
		public McdRecord Record
		{ get; protected set; }

		/// <summary>
		/// Gets the image-array used to animate this tile.
		/// </summary>
		public XCImage[] Images
		{ get; set; }

		/// <summary>
		/// Gets an image at the specified animation frame.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public XCImage this[int id]
		{
			get { return Images[id]; }
		}

		/// <summary>
		/// The ID of this tilepart that's unique to its terrain/MCD-record.
		/// </summary>
		public int TerrainId
		{ get; private set; }

		/// <summary>
		/// The ID of this tilepart that's unique to the Map across all
		/// allocated terrains.
		/// </summary>
		public int TerrainsetId
		{ get; set; }


		/// <summary>
		/// Instantiates a blank tile.
		/// </summary>
		/// <param name="id"></param>
		internal TilepartBase(int id)
		{
			TerrainId = id;
			TerrainsetId = -1;
		}
	}
}
