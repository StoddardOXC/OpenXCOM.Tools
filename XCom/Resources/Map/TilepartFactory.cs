using System;
using System.Windows.Forms;
using System.IO;


namespace XCom.Resources.Map
{
	public static class TilepartFactory
	{
		#region Fields (static)
		private const int Length = 62; // there are 62 bytes in each MCD record.
		#endregion


		#region Methods (static)
		/// <summary>
		/// Creates an array of tileparts from a given terrain and spriteset.
		/// </summary>
		/// <param name="terrain">the terrain file w/out extension</param>
		/// <param name="dirTerrain">path to the directory of the terrain file</param>
		/// <param name="spriteset">a SpriteCollection containing the needed sprites</param>
		/// <returns>an array of Tileparts</returns>
		internal static Tilepart[] CreateTileparts(
				string terrain,
				string dirTerrain,
				SpriteCollection spriteset)
		{
			if (spriteset != null)
			{
				string pfeMcd = Path.Combine(dirTerrain, terrain + GlobalsXC.McdExt);

				if (!File.Exists(pfeMcd))
				{
					MessageBox.Show(
								"Can't find file for MCD data."
									+ Environment.NewLine + Environment.NewLine
									+ pfeMcd,
								"Error",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error,
								MessageBoxDefaultButton.Button1,
								0);
				}
				else
				{
					using (var bs = new BufferedStream(File.OpenRead(pfeMcd)))
					{
						var tileparts = new Tilepart[(int)bs.Length / Length]; // TODO: Error if this don't work out right.

						for (int id = 0; id != tileparts.Length; ++id)
						{
							var bindata = new byte[Length];
							bs.Read(bindata, 0, Length);
							var record = McdRecordFactory.CreateRecord(bindata);

							var part = new Tilepart(id, spriteset, record);

							tileparts[id] = part;
						}

						for (int id = 0; id != tileparts.Length; ++id)
						{
							tileparts[id].Dead      = GetDeadPart(     terrain, id, tileparts[id].Record, tileparts);
							tileparts[id].Alternate = GetAlternatePart(terrain, id, tileparts[id].Record, tileparts);
						}

						return tileparts;
					}
				}
			}
			return new Tilepart[0];
		}

		/// <summary>
		/// Gets the count of MCD-records in an MCD-file.
		/// @note It's funky to read from disk just to get the count of records
		/// but at present an McdRecordCollection is retained only by a
		/// currently loaded Tileset .... That's to say there is no general
		/// cache of all available terrains; even a Map's Descriptor retains
		/// only the allocated terrains as tuples in a dictionary-object.
		/// See ResourceInfo - where the *sprites* of a terrain *are* cached.
		/// </summary>
		/// <param name="terrain">the terrain file w/out extension</param>
		/// <param name="dirTerrain">path to the directory of the terrain file</param>
		/// <param name="suppressError">true to suppress any error</param>
		/// <returns>count of MCD-records or 0 on fail</returns>
		internal static int GetRecordCount(
				string terrain,
				string dirTerrain,
				bool suppressError)
		{
			string pfeMcd = Path.Combine(dirTerrain, terrain + GlobalsXC.McdExt);

			if (File.Exists(pfeMcd))
			{
				using (var bs = new BufferedStream(File.OpenRead(pfeMcd)))
					return (int)bs.Length / Length; // TODO: Error if this don't work out right.
			}

			if (!suppressError)
			{
				MessageBox.Show(
							"Can't find file for MCD data."
								+ Environment.NewLine + Environment.NewLine
								+ pfeMcd,
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
			return 0;
		}

		/// <summary>
		/// Gets the dead-tile of a given MCD-record.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="id"></param>
		/// <param name="record"></param>
		/// <param name="tileparts"></param>
		/// <returns></returns>
		private static Tilepart GetDeadPart(
				string file,
				int id,
				McdRecord record,
				Tilepart[] tileparts)
		{
			if (record.DieTile != 0)
			{
				if (record.DieTile < tileparts.Length)
					return tileparts[record.DieTile];

				string warn = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"In the MCD file {0}, the tile entry {1} has an invalid dead tile (id {2} of {3} records).",
										file,
										id,
										record.DieTile,
										tileparts.Length);
				MessageBox.Show(
							warn,
							"Warning",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0);
			}
			return null;
		}

		/// <summary>
		/// Gets the alternate-tile of a given MCD-record.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="id"></param>
		/// <param name="record"></param>
		/// <param name="tileparts"></param>
		/// <returns></returns>
		private static Tilepart GetAlternatePart(
				string file,
				int id,
				McdRecord record,
				Tilepart[] tileparts)
		{
			if (record.Alt_MCD != 0) // || record.HumanDoor || record.UfoDoor
			{
				if (record.Alt_MCD < tileparts.Length)
					return tileparts[record.Alt_MCD];

				string warn = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"In the MCD file {0}, the tile entry {1} has an invalid alternate tile (id {2} of {3} records).",
										file,
										id,
										record.Alt_MCD,
										tileparts.Length);
				MessageBox.Show(
							warn,
							"Warning",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1,
							0);
			}
			return null;
		}
		#endregion
	}
}
