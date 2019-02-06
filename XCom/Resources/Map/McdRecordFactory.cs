using System;
using System.Collections.Generic;


namespace XCom
{
	internal static class McdRecordFactory
	{
		// TODO: do some basic checks, like issue a warning if the Die or
		// Alternate MCD entry is outside the range, etc.

		internal static McdRecord CreateRecord(IList<byte> bindata)
		{
			var record = new McdRecord();

			record.Sprite1 = bindata[0];
			record.Sprite2 = bindata[1];
			record.Sprite3 = bindata[2];
			record.Sprite4 = bindata[3];
			record.Sprite5 = bindata[4];
			record.Sprite6 = bindata[5];
			record.Sprite7 = bindata[6];
			record.Sprite8 = bindata[7];

			record.Loft1  = bindata[8];
			record.Loft2  = bindata[9];
			record.Loft3  = bindata[10];
			record.Loft4  = bindata[11];
			record.Loft5  = bindata[12];
			record.Loft6  = bindata[13];
			record.Loft7  = bindata[14];
			record.Loft8  = bindata[15];
			record.Loft9  = bindata[16];
			record.Loft10 = bindata[17];
			record.Loft11 = bindata[18];
			record.Loft12 = bindata[19];

			record.ScanG = (ushort)(bindata[21] * 255 + bindata[20]);

			record.Unknown22 = bindata[22];
			record.Unknown23 = bindata[23];
			record.Unknown24 = bindata[24];
			record.Unknown25 = bindata[25];
			record.Unknown26 = bindata[26];
			record.Unknown27 = bindata[27];
			record.Unknown28 = bindata[28];
			record.Unknown29 = bindata[29];

			record.UfoDoor    = bindata[30] == 1;
			record.StopLOS    = bindata[31] == 1; // unsigned char Stop_LOS;            // You cannot see through this tile.
			record.NoGround   = bindata[32] == 1; // unsigned char No_Floor;            // If 1, then a non-flying unit can't stand here
			record.BigWall    = bindata[33] == 1;
			record.GravLift   = bindata[34] == 1; // unsigned char Gravlift;
			record.HumanDoor  = bindata[35] == 1;
			record.BlockFire  = bindata[36] == 1; // unsigned char Block_Fire;          // If 1, fire won't go through the tile
			record.BlockSmoke = bindata[37] == 1; // unsigned char Block_Smoke;         // If 1, smoke won't go through the tile

			record.Unknown38 = bindata[38]; // unsigned char u39;

			record.TU_Walk     = bindata[39];
			record.TU_Fly      = bindata[40]; // unsigned char TU_Fly;                  // remember, 0xFF means it's impassable!
			record.TU_Slide    = bindata[41]; // unsigned char TU_Slide;                // sliding things include snakemen and silacoids
			record.Armor       = bindata[42];
			record.HE_Block    = bindata[43]; // unsigned char HE_Block;                // How much of an explosion this tile will block
			record.DieTile     = bindata[44];
			record.Flammable   = bindata[45];
			record.Alt_MCD     = bindata[46];
			record.Unknown47   = bindata[47]; // unsigned char u48;
			record.StandOffset = (sbyte)bindata[48];
			record.TileOffset  = (sbyte)bindata[49];
			record.Unknown50   = bindata[50];        // unsigned char u51;
			record.LightBlock  = (sbyte)bindata[51]; // unsigned char Light_Block;      // The amount of light it blocks, from 0 to 10
			record.Footstep    = (sbyte)bindata[52];

			record.PartType      = (PartType)bindata[53];
			record.HE_Type       = (sbyte)bindata[54]; // unsigned char HE_Type;        // 0=HE 1=Smoke
			record.HE_Strength   = (sbyte)bindata[55];
			record.SmokeBlockage = (sbyte)bindata[56]; // unsigned char Smoke_Blockage; // ? Not sure about this
			record.Fuel          = (sbyte)bindata[57];
			record.LightSource   = (sbyte)bindata[58]; // unsigned char Light_Source;   // The amount of light this tile produces
			record.TargetType    = (SpecialType)(sbyte)bindata[59];
			record.Unknown60     = bindata[60];        // unsigned char u61;
			record.Unknown61     = bindata[61];        // unsigned char u62;


			#region Arrays
			record.Images = string.Format(
										System.Globalization.CultureInfo.InvariantCulture,
										"{0,-20}{1} {2} {3} {4} {5} {6} {7} {8}" + Environment.NewLine,
										"images:",
										record.Sprite1,
										record.Sprite2,
										record.Sprite3,
										record.Sprite4,
										record.Sprite5,
										record.Sprite6,
										record.Sprite7,
										record.Sprite8);

			record.ScanGReference = string.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"{0,-20}{1} : {2} -> {3}" + Environment.NewLine,
										"scang reference:",
										bindata[20],
										bindata[21],
										bindata[20] * 256 + bindata[21]);

			record.LoftReference = string.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"{0,-20}{1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12}" + Environment.NewLine,
										"loft references:",
										bindata[8],
										bindata[9],
										bindata[10],
										bindata[11],
										bindata[12],
										bindata[13],
										bindata[14],
										bindata[15],
										bindata[16],
										bindata[17],
										bindata[18],
										bindata[19]);
			#endregion

			record.ByteTable = BytesTable(bindata);
			return record;
		}

		/// <summary>
		/// Creates a table of the byte-data for the MCD-info screen.
		/// </summary>
		/// <returns></returns>
		private static string BytesTable(IList<byte> bindata)
		{
			string text = String.Empty;

			const int wrap = 8;
			int wrapCount  = 0;
			int row        = 0;

			foreach (byte b in bindata)
			{
				if (wrapCount % wrap == 0)
				{
					if (++row < 10)
						text += " ";

					text += row + ": ";
				}

				if (b < 10)
					text += "  ";
				else if (b < 100)
					text += " ";

				text += " " + b;
				text += (++wrapCount % wrap == 0) ? Environment.NewLine
												  : " ";
			}
			return text;
		}
	}
}
