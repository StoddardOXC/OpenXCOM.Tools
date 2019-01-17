using System;
using System.Collections.Generic;

using XCom.Interfaces;


namespace XCom
{
	public class SpriteCollectionBase
		:
			List<XCImage>
	{
		#region Properties
		public string Label
		{ get; set; }

		private Palette _pal;
		public Palette Pal
		{
			get { return _pal; }
			set
			{
				_pal = value;

				foreach (XCImage sprite in this)
					sprite.Sprite.Palette = _pal.ColorTable;
			}
		}

		public new XCImage this[int id]
		{
			get { return (id > -1 && id < Count) ? base[id]
												 : null; }
			set
			{
				if (id > -1 && id < Count)
					base[id] = value;
				else
				{
					value.TerrainId = Count;
					Add(value);
				}
			}
		}
		#endregion
	}
}

//		private int _scale = 1;
//		public void HQ2X()
//		{
//			foreach (XCImage image in this)
//				image.HQ2X();
//			_scale *= 2;
//		}
