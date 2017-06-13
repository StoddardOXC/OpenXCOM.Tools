using System;
using System.Drawing;


namespace XCom.Interfaces
{
	public class XCImage
	{
		#region Properties
		public byte[] Bindata
		{ get; protected set; }

		public int FileId
		{ get; set; }

		public Bitmap Image
		{ get; protected set; }

		public Bitmap SpriteGray
		{ get; protected set; }

		private Palette _palette;
		public Palette Pal
		{
			get { return _palette; }
			set
			{
				_palette = value;

				if (Image != null)
					Image.Palette = _palette.ColorTable;
			}
		}
		#endregion


		#region cTor
		/// <summary>
		/// Creates an XCImage.
		/// NOTE: Entries must not be compressed.
		/// </summary>
		/// <param name="bindata"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="pal"></param>
		/// <param name="id"></param>
		internal XCImage(
				byte[] bindata,
				int width,
				int height,
				Palette pal,
				int id)
		{
			FileId  = id;
			Bindata = bindata;
			Pal     = pal;

			if (Pal != null)
				Image = XCBitmap.MakeBitmap8(
										width,
										height,
										bindata,
										Pal.ColorTable);
		}
//		private XCImage(Bitmap image, int id)
//		{
//			Image = image;
//			FileId = id;
//		}
//		public XCImage()
//			:
//				this(
//					new byte[]{},
//					0, 0,
//					null,
//					-1)
//		{}
		#endregion


//		#region Methods
//		/// <summary>
//		/// Clones this image for use by PckView.
//		/// </summary>
//		/// <returns>pointer to a new XCImage or null</returns>
//		public XCImage Clone()
//		{
//			if (Bindata != null)
//			{
//				var bindata = new byte[Bindata.Length];
//				for (int i = 0; i != bindata.Length; ++i)
//					bindata[i] = Bindata[i];
//
//				return new XCImage(
//								bindata,
//								Image.Width,
//								Image.Height,
//								Pal,
//								FileId);
//			}
//
//			// TODO: this arbitrary Clone() method should probably be disallowed:
//			return (Image != null) ? new XCImage(Image.Clone() as Bitmap, FileId)
//								   : null;
//		}

//		internal void HQ2X()
//		{
//			Image = Bmp.HQ2X(/*Image*/);
//		}
//		#endregion
	}
}
