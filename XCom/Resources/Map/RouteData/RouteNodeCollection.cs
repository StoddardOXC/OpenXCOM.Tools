using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;


namespace XCom
{
	#region Enums
	// NOTE: Only 'NodeRankUfo' and 'NodeRankTftd' need to be enumerated as
	// byte-type. Otherwise the Pterodactyl class goes snakey when
	// RouteView.OnNodeRankSelectedIndexChanged() fires. For reasons, it cannot
	// handle the cast automatically like the other enumerated types here appear
	// to. But I left the others as bytes also for safety.

	public enum UnitType
		:
			byte
	{
		Any         = 0,
		FlyingSmall = 1,
		Small       = 2,
		FlyingLarge = 3,
		Large       = 4
	};

	public enum NodeRankUfo
		:
			byte
	{
		CivScout        = 0,
		XCOM            = 1,
		Soldier         = 2,
		Navigator       = 3,
		LeaderCommander = 4,
		Engineer        = 5,
		Misc1           = 6,
		Medic           = 7,
		Misc2           = 8,
		invalid         = 9 // WORKAROUND.
	};

	public enum NodeRankTftd
		:
			byte
	{
		CivScout        = 0,
		XCOM            = 1,
		Soldier         = 2,
		SquadLeader     = 3,
		LeaderCommander = 4,
		Medic           = 5,
		Misc1           = 6,
		Technician      = 7,
		Misc2           = 8,
		invalid         = 9 // WORKAROUND.
	};

	public enum SpawnWeight
		:
			byte
	{
		None    = 0,
		Spawn1  = 1,
		Spawn2  = 2,
		Spawn3  = 3,
		Spawn4  = 4,
		Spawn5  = 5,
		Spawn6  = 6,
		Spawn7  = 7,
		Spawn8  = 8,
		Spawn9  = 9,
		Spawn10 = 10
	};

	public enum PatrolPriority
		:
			byte
	{
		Zero  = 0,
		One   = 1,
		Two   = 2,
		Three = 3,
		Four  = 4,
		Five  = 5,
		Six   = 6,
		Seven = 7,
		Eight = 8,
		Nine  = 9,
		Ten   = 10
	};

	public enum BaseAttack
		:
			byte
	{
		Zero  = 0,
		One   = 1,
		Two   = 2,
		Three = 3,
		Four  = 4,
		Five  = 5,
		Six   = 6,
		Seven = 7,
		Eight = 8,
		Nine  = 9,
		Ten   = 10
	};

	public enum LinkType
		:
			byte
	{
		None      = 0x00, // pacify FxCop CA1008 BUT DO NOT USE IT.
		NotUsed   = 0xFF, // since valid route-nodes can and will have a value of 0.
		ExitNorth = 0xFE,
		ExitEast  = 0xFD,
		ExitSouth = 0xFC,
		ExitWest  = 0xFB
	};
	#endregion


	/// <summary>
	/// This class reads, saves, and generally manages all the information in a
	/// .RMP file. It's like the parent of RouteNode.
	/// </summary>
	public sealed class RouteNodeCollection
		:
			IEnumerable<RouteNode>
	{
		#region Fields
		private readonly List<RouteNode> _nodes;
		#endregion


		#region Fields (static)
		private const string RankInvalid = "INVALID";

		public static readonly object[] NodeRankUfo =
		{
			new Pterodactyl("0 : Civ/Scout",        XCom.NodeRankUfo.CivScout),
			new Pterodactyl("1 : XCOM",             XCom.NodeRankUfo.XCOM),
			new Pterodactyl("2 : Soldier",          XCom.NodeRankUfo.Soldier),
			new Pterodactyl("3 : Navigator",        XCom.NodeRankUfo.Navigator),
			new Pterodactyl("4 : Leader/Commander", XCom.NodeRankUfo.LeaderCommander),
			new Pterodactyl("5 : Engineer",         XCom.NodeRankUfo.Engineer),
			new Pterodactyl("6 : Terrorist1",       XCom.NodeRankUfo.Misc1),
			new Pterodactyl("7 : Medic",            XCom.NodeRankUfo.Medic),
			new Pterodactyl("8 : Terrorist2",       XCom.NodeRankUfo.Misc2),
			new Pterodactyl(RankInvalid,            XCom.NodeRankUfo.invalid) // WORKAROUND.
		};

		public static readonly object[] NodeRankTftd =
		{
			new Pterodactyl("0 : Civ/Scout",        XCom.NodeRankTftd.CivScout),
			new Pterodactyl("1 : XCOM",             XCom.NodeRankTftd.XCOM),
			new Pterodactyl("2 : Soldier",          XCom.NodeRankTftd.Soldier),
			new Pterodactyl("3 : Squad Leader",     XCom.NodeRankTftd.SquadLeader),
			new Pterodactyl("4 : Leader/Commander", XCom.NodeRankTftd.LeaderCommander),
			new Pterodactyl("5 : Medic",            XCom.NodeRankTftd.Medic),
			new Pterodactyl("6 : Terrorist1",       XCom.NodeRankTftd.Misc1),
			new Pterodactyl("7 : Technician",       XCom.NodeRankTftd.Technician),
			new Pterodactyl("8 : Terrorist2",       XCom.NodeRankTftd.Misc2),
			new Pterodactyl(RankInvalid,            XCom.NodeRankTftd.invalid) // WORKAROUND.
		};

		public static readonly object[] SpawnWeight =
		{
			new Pterodactyl("0 : None", XCom.SpawnWeight.None),
			new Pterodactyl("1 : Lo",   XCom.SpawnWeight.Spawn1),
			new Pterodactyl("2 : Lo",   XCom.SpawnWeight.Spawn2),
			new Pterodactyl("3 : Lo",   XCom.SpawnWeight.Spawn3),
			new Pterodactyl("4 : Med",  XCom.SpawnWeight.Spawn4),
			new Pterodactyl("5 : Med",  XCom.SpawnWeight.Spawn5),
			new Pterodactyl("6 : Med",  XCom.SpawnWeight.Spawn6),
			new Pterodactyl("7 : Med",  XCom.SpawnWeight.Spawn7),
			new Pterodactyl("8 : Hi",   XCom.SpawnWeight.Spawn8),
			new Pterodactyl("9 : Hi",   XCom.SpawnWeight.Spawn9),
			new Pterodactyl("10 : Hi",  XCom.SpawnWeight.Spawn10)
		};
		#endregion


		#region Properties
		private string FullPath
		{ get; set; }

		/// <summary>
		/// Returns the node at id.
		/// </summary>
		public RouteNode this[int id]
		{
			get
			{
				if (id > -1 && id < _nodes.Count)
					return _nodes[id];

				return null;
			}
		}

		/// <summary>
		/// Returns the length of the collection.
		/// </summary>
		public int Length
		{
			get { return _nodes.Count; }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor. Reads the .RMP file and adds its data as RouteNodes to a List.
		/// </summary>
		/// <param name="routes"></param>
		/// <param name="basepath"></param>
		internal RouteNodeCollection(string routes, string basepath)
		{
			_nodes = new List<RouteNode>();

			FullPath = Path.Combine(basepath, GlobalsXC.RoutesDir);
			FullPath = Path.Combine(FullPath, routes + GlobalsXC.RouteExt);

			if (File.Exists(FullPath))
			{
				using (var bs = new BufferedStream(File.OpenRead(FullPath)))
				{
					for (byte id = 0; id < bs.Length / 24; ++id)
					{
						var bindata = new byte[24];
						bs.Read(bindata, 0, 24);

						_nodes.Add(new RouteNode(id, bindata));
					}
				}

				var invalids = new List<byte>();	// check for invalid Ranks ->
				foreach (RouteNode node in _nodes)	// See also RouteView.OnCheckNodeRanksClick()
				{
					if (node.OobRank != (byte)0)
						invalids.Add(node.Index);
				}

				if (invalids.Count != 0)
				{
					string info = String.Format(
											System.Globalization.CultureInfo.CurrentCulture,
											"The following route-{0} an invalid NodeRank.{1}",
											(invalids.Count == 1) ? "node has"
																  : "nodes have",
											Environment.NewLine);

					foreach (byte id in invalids)
						info += Environment.NewLine + id;

					MessageBox.Show(
								info,
								"Warning",
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning,
								MessageBoxDefaultButton.Button1,
								0);
				}
			}
		}
		#endregion


		#region Interface requirements
		/// <summary>
		/// Gets an enumerator for the node-list.
		/// </summary>
		/// <returns></returns>
		IEnumerator<RouteNode> IEnumerable<RouteNode>.GetEnumerator()
		{
			return _nodes.GetEnumerator();
		}

		/// <summary>
		/// Gets another enumerator for the node-list.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return _nodes.GetEnumerator();
		}
		#endregion


		#region Methods
		/// <summary>
		/// Saves the .RMP file.
		/// </summary>
		internal void SaveRoutes()
		{
			SaveNodes(FullPath);
		}

		/// <summary>
		/// Saves the .RMP file as a different file.
		/// </summary>
		/// <param name="pf">the path+file to save as</param>
		internal void SaveRoutes(string pf)
		{
			string pfe = pf + GlobalsXC.RouteExt;
			Directory.CreateDirectory(Path.GetDirectoryName(pfe));
			SaveNodes(pfe);
		}

		/// <summary>
		/// Saves the route-nodes to a .RMP file.
		/// </summary>
		/// <param name="pfe">path+file+extension</param>
		private void SaveNodes(string pfe)
		{
			using (var fs = File.Create(pfe))
			{
				for (int id = 0; id != _nodes.Count; ++id)
					_nodes[id].SaveNode(fs); // -> RouteNode.SaveNode() writes each node-data
			}
		}

		/// <summary>
		/// Adds a node to the node-list.
		/// </summary>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="lev"></param>
		/// <returns></returns>
		internal RouteNode AddNode(byte row, byte col, byte lev)
		{
			var node = new RouteNode((byte)_nodes.Count, row, col, lev);
			_nodes.Add(node);

			return node;
		}

		/// <summary>
		/// Deletes a node from the node-list.
		/// </summary>
		/// <param name="node">the node to delete</param>
		public void DeleteNode(RouteNode node)
		{
			int id = node.Index;

			_nodes.Remove(node);

			foreach (var node0 in _nodes)
			{
				if (node0.Index > id) // shuffle all higher-indexed nodes down 1
					--node0.Index;

				for (int slot = 0; slot != RouteNode.LinkSlots; ++slot)
				{
					var link = node0[slot];
					if (link.StandardNode())
					{
						if (link.Destination == id)
							link.Destination = Link.NotUsed;
						else if (link.Destination > id)
							--link.Destination;
					}
				}
			}
		}

		/// <summary>
		/// Checks if a given node is outside the Map boundaries.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="cols"></param>
		/// <param name="rows"></param>
		/// <param name="levs"></param>
		/// <returns></returns>
		public static bool IsNodeOutsideMapBounds(
				RouteNode node,
				int cols,
				int rows,
				int levs)
		{
			return node.Col < 0 || node.Col >= cols
				|| node.Row < 0 || node.Row >= rows
				|| node.Lev < 0 || node.Lev >= levs;
		}
		#endregion
	}
}
