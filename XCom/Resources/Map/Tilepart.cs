using System;

using XCom.Interfaces;
using XCom.Interfaces.Base;


namespace XCom
{
	public sealed class Tilepart
		:
			TilepartBase
	{
		#region Fields & Properties
		private readonly SpriteCollection _spriteset;

		public Tilepart Dead
		{ get; internal set; }

		internal Tilepart Alternate
		{ get; set; }
		#endregion


		#region cTor
		/// <summary>
		/// cTor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="spriteset"></param>
		/// <param name="record"></param>
		internal Tilepart(
				int id,
				SpriteCollection spriteset,
				McdRecord record)
			:
				base(id)
		{
			_spriteset = spriteset;
			Record     = record;

			Anisprites = new XCImage[8]; // every tile-part contains refs to 8 sprites.
			InitializeSprites();
		}
		#endregion


		#region Methods
		// re. Animating Sprites
		// Basically this is how animations operate. For *any* animations to
		// happen the Animation option has to be turned on. Non-door sprites
		// always keep their array of sprites and will cycle because turning on
		// the Animation option starts a timer that does that (see
		// 'MapView.MainViewPanel').
		// 
		// UfoDoor sprites will animate when the Animation option is on and the
		// Doors option is turned on; but whether or not they animate is
		// controlled by setting their sprite-arrays to either the first image
		// or an array of images, like non-door records do.
		// 
		// HumanDoors, which also need the Animation option on to animate as
		// well as the Doors option on, will cycle by flipping their sprite
		// back and forth between their first sprite and their Alt-tile's first
		// sprite; they stop animating by setting the entire array to their
		// first sprite only.

		/// <summary>
		/// Initializes this tilepart's array of sprites.
		/// </summary>
		private void InitializeSprites()
		{
			if (Record.UfoDoor || Record.HumanDoor)
			{
				for (int i = 0; i != 8; ++i)
					Anisprites[i] = _spriteset[Record.Image1];
			}
			else
			{
				Anisprites[0] = _spriteset[Record.Image1];
				Anisprites[1] = _spriteset[Record.Image2];
				Anisprites[2] = _spriteset[Record.Image3];
				Anisprites[3] = _spriteset[Record.Image4];
				Anisprites[4] = _spriteset[Record.Image5];
				Anisprites[5] = _spriteset[Record.Image6];
				Anisprites[6] = _spriteset[Record.Image7];
				Anisprites[7] = _spriteset[Record.Image8];
			}
		}


		/// <summary>
		/// Toggles this tilepart's array of sprites if it's a door-part.
		/// </summary>
		/// <param name="animate">true to animate</param>
		public void SetDoorSprites(bool animate)
		{
			if (Record.UfoDoor || Record.HumanDoor)
			{
				if (animate)
				{
					if (Record.UfoDoor || Alternate == null)
					{
						Anisprites[0] = _spriteset[Record.Image1];
						Anisprites[1] = _spriteset[Record.Image2];
						Anisprites[2] = _spriteset[Record.Image3];
						Anisprites[3] = _spriteset[Record.Image4];
						Anisprites[4] = _spriteset[Record.Image5];
						Anisprites[5] = _spriteset[Record.Image6];
						Anisprites[6] = _spriteset[Record.Image7];
						Anisprites[7] = _spriteset[Record.Image8];
					}
					else
					{
						byte alt = Alternate.Record.Image1;
						for (int i = 4; i != 8; ++i)
							Anisprites[i] = _spriteset[alt];
					}
				}
				else
				{
					for (int i = 0; i != 8; ++i)
						Anisprites[i] = _spriteset[Record.Image1];
				}
			}
		}

		public void SetDoorToAlternateSprite()
		{
			if (Record.UfoDoor || Record.HumanDoor)
			{
				byte alt = Alternate.Record.Image1;
				for (int i = 0; i != 8; ++i)
					Anisprites[i] = _spriteset[alt];
			}
		}
		#endregion
	}
}
