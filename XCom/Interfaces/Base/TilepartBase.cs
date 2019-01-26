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
		/// Gets the sprite-array used to animate this tile.
		/// </summary>
		public XCImage[] Anisprites
		{ get; set; }

		/// <summary>
		/// Gets a sprite at the specified animation frame.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public XCImage this[int id]
		{
			get { return Anisprites[id]; }
		}

		/// <summary>
		/// The ID of this tilepart that's unique to its terrain/MCD-record.
		/// </summary>
		public int TerId
		{ get; private set; }

		/// <summary>
		/// The ID of this tilepart that's unique to the Map across all
		/// allocated terrains.
		/// </summary>
		public int SetId
		{ get; set; }


		/// <summary>
		/// Instantiates a blank tile.
		/// </summary>
		/// <param name="id"></param>
		internal TilepartBase(int id)
		{
			TerId = id;
			SetId = -1;
		}
	}
}
