using System;
using System.Collections.Generic;
using System.IO;

using XCom.Interfaces;


namespace XCom
{
	// kL_note: It appears that RLE-encoded .Pck images cannot use color-indices
	// of 0xFE or 0xFF. Because, in the Pck-packed file, 0xFF is used to denote
	// the end of each image's bindata, and 0xFE is used to flag a quantity of
	// pixels as transparent by using 2 bytes: 0xFE itself, and the next byte is
	// the quantity of pixels that are transparent and hence are not written.

	public sealed class PckImage
		:
			XCImage
	{
		#region Fields (static)
		private const byte ByteMaximumValue = 0xFF; // ... trying to keep my head straight.
		/// <summary>
		/// A flag that is inserted into the file-data that indicates that an
		/// image's data has ended.
		/// </summary>
		public const byte SpriteStopByte = 0xFF;
		/// <summary>
		/// A flag that is inserted into the file-data that indicates that the
		/// following pixels are transparent.
		/// </summary>
		public const byte SpriteTransparencyByte = 0xFE;	// the PCK-file uses 0xFE to flag a succeeding quantity of pixels
															// as transparent. That is, it is *not* a color-id entry; it's
															// just a flag in the Pck-file. Stop using it as a color-id entry.
		/// <summary>
		/// Tracks the id of an image across all loaded terrainsets. Used only
		/// by 'MapInfoOutputBox'.
		/// </summary>
		private static int _idCanonical;
		#endregion


		#region Fields
		private readonly SpriteCollection _spriteset; // currently used only for ToString() override.
		#endregion


		#region Properties
		/// <summary>
		/// MapId is used only by 'MapInfoOutputBox'.
		/// </summary>
		public int MapId
		{ get; private set; }
		#endregion


		#region cTor
		/// <summary>
		/// Instantiates a PckImage, based on an XCImage.
		/// </summary>
		/// <param name="bindata">the COMPRESSED source data</param>
		/// <param name="pal"></param>
		/// <param name="terrainId"></param>
		/// <param name="spriteset"></param>
		internal PckImage(
				byte[] bindata,
				Palette pal,
				int terrainId,
				SpriteCollection spriteset)
			:
				base(
					new byte[XCImage.SpriteWidth * XCImage.SpriteHeight],	// new byte[]{}
					XCImage.SpriteWidth,									// 0
					XCImage.SpriteHeight,									// 0
					null, // do *not* pass 'pal' in here. See XCImage..cTor
					terrainId)
		{
			//LogFile.WriteLine("PckImage..cTor");

			_spriteset = spriteset; // for ToString() only.
			MapId = _idCanonical++; // for 'MapInfoOutputBox' only.

			Pal = pal;

			_spriteset.BorkedBigobs = false;

			for (int id = 0; id != Bindata.Length; ++id)
				Bindata[id] = Palette.TransparentId; // Safety: byte arrays get initialized w/ "0" by default

			int posSrc = 0;
			int posDst = 0;

			if (bindata[0] != SpriteTransparencyByte)
				posDst = bindata[posSrc++] * XCImage.SpriteWidth;

			for (; posSrc != bindata.Length; ++posSrc)
			{
				switch (bindata[posSrc])
				{
					default:
						//LogFile.WriteLine(". Bindata.Length= " + Bindata.Length + " dst= " + dst);
						//LogFile.WriteLine(". bindata.Length= " + bindata.Length + " id= "  + id);

						if (posDst < Bindata.Length)
						{
							Bindata[posDst++] = bindata[posSrc];
							break;
						}

						// probly trying to load a 32x48 Bigobs pck in a 32x40 spriteset.
						// Note that this cannot be resolved absolutely.
						_spriteset.BorkedBigobs = true;
						return;

					case SpriteTransparencyByte: // skip quantity of pixels
						posDst += bindata[++posSrc];
						break;

					case SpriteStopByte: // end of image
						break;
				}
			}

//			if (!_spriteset.BorkedBigobs) // check if trying to load a 32x40 Terrain/Unit pck in a 32x48 spriteset.
//			{
//				// there's no way to determine this.
//			}
//			else
//			{
			Sprite = BitmapService.CreateColorized(
												XCImage.SpriteWidth,
												XCImage.SpriteHeight,
												Bindata,
												Pal.ColorTable);
			SpriteGray = BitmapService.CreateColorized(
												XCImage.SpriteWidth,
												XCImage.SpriteHeight,
												Bindata,
												Pal.Grayscale.ColorTable);
//			}
		}
		#endregion


		#region Methods (static)
		internal static int SaveSpritesetSprite(BinaryWriter bw, XCImage sprite)
		{
			var binlist = new List<byte>();

			int lenTransparent = 0;
			bool first = true;

			for (int id = 0; id != sprite.Bindata.Length; ++id)
			{
				byte b = sprite.Bindata[id];

				if (b == Palette.TransparentId)
					++lenTransparent;
				else
				{
					if (lenTransparent != 0)
					{
						if (first)
						{
							first = false;

							binlist     .Add((byte)(lenTransparent / sprite.Sprite.Width));	// qty of initial transparent rows
							lenTransparent = (byte)(lenTransparent % sprite.Sprite.Width);	// qty of transparent pixels starting on the next row
						}

						while (lenTransparent >= ByteMaximumValue)
						{
							lenTransparent -= ByteMaximumValue;

							binlist.Add(SpriteTransparencyByte);
							binlist.Add(ByteMaximumValue);
						}

						if (lenTransparent != 0)
						{
							binlist.Add(SpriteTransparencyByte);
							binlist.Add((byte)lenTransparent);
						}
						lenTransparent = 0;
					}
					binlist.Add(b);
				}
			}

			// So, question. Is one obligated to account for transparent pixels
			// to the end of an image, or can one just assume that the program
			// that reads and decompresses the data will force them to transparent ...
			//
			// It looks like both OpenXcom and MapView will fill the sprite with
			// all transparent pixels when each sprite is initialized. Therefore,
			// it's not *required* to encode any pixels that are transparent to
			// the end of the sprite.
			//
			// And when looking at some of the stock PCK's things look non-standardized.
			// It's sorta like if there's at least one full row of transparent
			// pixels at the end of an image, it gets 0xFE,0xFF tacked on before
			// the final 0xFF (end of image) marker.
			//
			// Note that this algorithm can and will tack on multiple 0xFE,0xFF
			// if there's more than 256 transparent pixels at the end of an image.


//			bool appendStopByte = false;
//			while (lenTransparent >= ByteMaximumValue)
//			{
//				lenTransparent -= ByteMaximumValue;
//
//				binlist.Add(SpriteTransparencyByte);
//				binlist.Add(ByteMaximumValue);
//
//				appendStopByte = true;
//			}
//
//			if (appendStopByte
//				|| (byte)binlist[binlist.Count - 1] != SpriteStopByte)
//			{
			binlist.Add(SpriteStopByte);
//			}

			// Okay. That seems to be the algorithm that was used. Ie, no need
			// to go through that final looping mechanism.
			//
			// In fact I'll bet it's even better than stock, since it no longer
			// appends superfluous 0xFE,0xFF markers at all.

			bw.Write(binlist.ToArray());

			return binlist.Count;
		}
		#endregion


		#region Methods (override)
		public override string ToString()
		{
			string ret = String.Empty;

			if (_spriteset != null)
				ret += _spriteset.ToString();

			ret += TerrainId + Environment.NewLine;

			for (int i = 0; i != Bindata.Length; ++i)
			{
				ret += Bindata[i];

				switch (Bindata[i])
				{
					case SpriteStopByte:
						ret += Environment.NewLine;
						break;

					default:
						ret += " ";
						break;
				}
			}
			return ret;
		}

		public override bool Equals(object obj)
		{
			if (obj is PckImage)
				return ToString().Equals(obj.ToString());

			return false;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
		#endregion
	}
}

//		public override void Hq2x()
//		{
//			if (Width == 32) // hasn't been done yet
//				base.Hq2x();
//		}

//		public static Type GetCollectionType()
//		{
//			return typeof(SpriteCollection);
//		}

//		public void ReImage()
//		{
//			_image = Bmp.MakeBitmap8(
//								Width,
//								Height,
//								_expanded,
//								Palette.Colors);
//			_gray = Bmp.MakeBitmap8(
//								Width,
//								Height,
//								_expanded,
//								Palette.Grayscale.Colors);
//		}

//		public void MoveImage(byte offset)
//		{
//			_id[_moveId] = (byte)(_moveVal - offset);
//			int ex = 0;
//			int startIdx = 0;
//			for (int i = 0; i != _expanded.Length; ++i)
//				_expanded[i] = TransparentIndex;
//
//			if (_id[0] != FileTransparencyByte)
//				ex = _id[startIdx++] * Width;
//
//			for (int i = startIdx; i < _id.Length; ++i)
//			{
//				switch (_id[i])
//				{
//					case FileTransparencyByte: // skip quantity of pixels
//						ex += _id[i + 1];
//						++i;
//						break;
//
//					case FileStopByte: // end of image
//						break;
//
//					default:
//						_expanded[ex++] = _id[i];
//						break;
//				}
//			}
//		
//			_image = Bmp.MakeBitmap8(
//								Width,
//								Height,
//								_expanded,
//								Palette.Colors);
//			_gray = Bmp.MakeBitmap8(
//								Width,
//								Height,
//								_expanded,
//								Palette.Grayscale.Colors);
//		}

//		public int MapId
//		{
//			get { return _mapId; }
//			set { _mapId = value; }
//		}
