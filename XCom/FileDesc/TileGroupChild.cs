using System;
using System.Collections.Generic;

using XCom.Interfaces;


namespace XCom
{
	/// <summary>
	/// Instantiates descriptors for all tilesets in MapTilesets.
	/// </summary>
	internal sealed class TileGroupChild
		:
			TileGroup
	{
		#region cTors
		/// <summary>
		/// cTor[1]. Load from YAML.
		/// </summary>
		internal TileGroupChild(string labelGroup, List<Tileset> tilesets)
			:
				base(labelGroup)
		{
			//LogFile.WriteLine("");
			//LogFile.WriteLine("TileGroupChild cTor label= " + labelGroup);

			var progress = ProgressBarForm.Instance;
			progress.SetInfo("Sorting: " + labelGroup);
			progress.SetTotal(tilesets.Count);
			progress.ResetProgress();
			progress.Show();


			foreach (var tileset in tilesets)
			{
				//LogFile.WriteLine(". tileset.Label= " + tileset.Label);

				if (tileset.Group == labelGroup)
				{
					//LogFile.WriteLine(". . tileset belongs to Group");
					//LogFile.WriteLine(". . tileset.Category= " + tileset.Category);

					if (!Categories.ContainsKey(tileset.Category))
					{
						//LogFile.WriteLine(". . . Create new Category");

						Categories[tileset.Category] = new Dictionary<string, Descriptor>();
					}

					if (String.IsNullOrEmpty(tileset.BasePath)) // assign the Configurator's basepath to the tileset's Descriptor ->
					{
						switch (GroupType)
						{
							case GameType.Ufo:
								tileset.BasePath = SharedSpace.Instance.GetShare(SharedSpace.ResourceDirectoryUfo);
								break;
							case GameType.Tftd:
								tileset.BasePath = SharedSpace.Instance.GetShare(SharedSpace.ResourceDirectoryTftd);
								break;
						}
					}

					var descriptor = new Descriptor(
												tileset.Label,
												tileset.Terrains,
												tileset.BasePath,
												Pal);

					Categories[tileset.Category][tileset.Label] = descriptor;
				}
				//else LogFile.WriteLine(". . tileset not in this Group - bypass.");

				progress.UpdateProgress();
			}
			progress.Hide();
		}

		/// <summary>
		/// cTor[2] for editing the label of the TileGroup.
		/// </summary>
		/// <param name="labelGroup"></param>
		internal TileGroupChild(string labelGroup)
			:
				base(labelGroup)
		{}
		#endregion
	}
}
