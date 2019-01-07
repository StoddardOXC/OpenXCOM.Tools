using System;
using System.Windows.Forms;
using System.IO;


namespace XCom.Resources.Map
{
	public static class XCTileFactory
	{
		#region Fields (static)
		private const string McdExt = ".MCD";

		private const int Length = 62; // there are 62 bytes in each MCD record.
		#endregion


		#region Methods (static)
		/// <summary>
		/// Creates an array of tileparts from a given terrain and spriteset.
		/// </summary>
		/// <param name="terrain"></param>
		/// <param name="dirTerrain"></param>
		/// <param name="spriteset"></param>
		/// <returns></returns>
		internal static Tilepart[] CreateTileparts(
				string terrain,
				string dirTerrain,
				SpriteCollection spriteset)
		{
			if (spriteset != null)
			{
				string pfeMcd = Path.Combine(dirTerrain, terrain + McdExt);

				if (!File.Exists(pfeMcd))
				{
					MessageBox.Show(
								"Can't find file for terrain data"
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
						var parts = new Tilepart[(int)bs.Length / Length]; // TODO: Error if this don't work out right.

						for (int id = 0; id != parts.Length; ++id)
						{
							var bindata = new byte[Length];
							bs.Read(bindata, 0, Length);
							var record = McdRecordFactory.CreateRecord(bindata);

							var part = new Tilepart(id, spriteset, record);

							parts[id] = part;
						}

						for (int id = 0; id != parts.Length; ++id)
						{
							parts[id].Dead = GetDeadPart(terrain, id, parts[id].Record, parts);
							parts[id].Alternate = GetAlternatePart(terrain, id, parts[id].Record, parts);
						}

						return parts;
					}
				}
			}
			return new Tilepart[0];
		}

		/// <summary>
		/// Gets the dead-tile of a given MCD-record.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="id"></param>
		/// <param name="record"></param>
		/// <param name="parts"></param>
		/// <returns></returns>
		private static Tilepart GetDeadPart(
				string file,
				int id,
				McdRecord record,
				Tilepart[] parts)
		{
			if (record.DieTile != 0)
			{
				if (record.DieTile < parts.Length)
					return parts[record.DieTile];

				string warn = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"In the MCD file {0}, the tile entry {1} has an invalid dead tile (id {2} of {3} records).",
										file,
										id,
										record.Alt_MCD,
										parts.Length);
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
		/// <param name="parts"></param>
		/// <returns></returns>
		private static Tilepart GetAlternatePart(
				string file,
				int id,
				McdRecord record,
				Tilepart[] parts)
		{
			if (record.Alt_MCD != 0) // || record.HumanDoor || record.UfoDoor
			{
				if (record.Alt_MCD < parts.Length)
					return parts[record.Alt_MCD];

				string warn = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"In the MCD file {0}, the tile entry {1} has an invalid alternate tile (id {2} of {3} records).",
										file,
										id,
										record.Alt_MCD,
										parts.Length);
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
