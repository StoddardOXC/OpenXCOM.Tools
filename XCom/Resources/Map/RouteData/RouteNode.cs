using System.IO;


namespace XCom
{
	public sealed class RouteNode
	{
		#region Fields (static)
		public const int LinkSlots = 5;
		#endregion


		#region Properties
		public byte Col
		{ get; set; }

		public byte Row
		{ get; set; }

		public int Lev
		{ get; set; }


		private readonly Link[] _links;
		/// <summary>
		/// Gets the link at slot.
		/// </summary>
		public Link this[int slot]
		{
			get { return _links[slot]; }
		}

		public UnitType Type
		{ get; set; }

		public byte Rank
		{ get; set; }

		public PatrolPriority Patrol
		{ get; set; }

		public BaseAttack Attack
		{ get; set; }

		public SpawnWeight Spawn
		{ get; set; }

		/// <summary>
		/// Gets the index of this RouteNode.
		/// </summary>
		public byte Index
		{ get; set; }
		#endregion


		/// <summary>
		/// Catches an out-of-bounds Rank value if it tries to load from the .RMP.
		/// TFTD appears to have ~10 Maps that have OobRanks.
		/// </summary>
		public byte OobRank
		{ get; set; }


		#region cTors
		/// <summary>
		/// cTor[1]. Creates a node from binary data.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="bindata"></param>
		internal RouteNode(byte id, byte[] bindata)
		{
			Index = id;

			Col = bindata[1]; // note that x & y are switched in the RMP-file.
			Row = bindata[0];
			Lev = bindata[2]; // NOTE: auto-converts to int-type.

			// NOTE: 'bindata[3]' is not used.

			_links = new Link[LinkSlots];

			int offset = 4;
			for (int slotId = 0; slotId != LinkSlots; ++slotId)
			{
				_links[slotId] = new Link(
										bindata[offset],
										bindata[offset + 1],
										bindata[offset + 2]);
				offset += 3;
			}

			Type   = (UnitType)bindata[19];
			Rank   = bindata[20];
			Patrol = (PatrolPriority)bindata[21];
			Attack = (BaseAttack)bindata[22];
			Spawn  = (SpawnWeight)bindata[23];

			if (Rank > (byte)8) // NodeRanks are 0..8 (if valid.)
			{
				OobRank = Rank;
				Rank = (byte)9; // invalid case appears in the combobox.
			}
			else
				OobRank = (byte)0;
		}

		/// <summary>
		/// cTor[2]. Creates a node based on row/col/level.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="row"></param>
		/// <param name="col"></param>
		/// <param name="lev"></param>
		internal RouteNode(byte id, byte row, byte col, byte lev)
		{
			Index = id;

			Col =      col;
			Row =      row;
			Lev = (int)lev; // NOTE: auto-converts to int-type. Do it explicitly.

			_links = new Link[LinkSlots];
			for (int slotId = 0; slotId != LinkSlots; ++slotId)
				_links[slotId] = new Link(Link.NotUsed, 0, 0);

			Type   = UnitType.Any;
			Rank   = (byte)0;
			Patrol = PatrolPriority.Zero;
			Attack = BaseAttack.Zero;
			Spawn  = SpawnWeight.None;

			OobRank = (byte)0;
		}
		#endregion


		#region Methods
		/// <summary>
		/// Writes data to the FileStream provided by RouteNodeCollection.SaveNodes().
		/// </summary>
		/// <param name="fs"></param>
		internal void SaveNode(Stream fs)
		{
			fs.WriteByte(Row);
			fs.WriteByte(Col);
			fs.WriteByte((byte)Lev);
			fs.WriteByte((byte)0);

			for (int slot = 0; slot != LinkSlots; ++slot)
			{
				fs.WriteByte(_links[slot].Destination);
				fs.WriteByte(_links[slot].Distance);
				fs.WriteByte((byte)_links[slot].Type);
			}

			fs.WriteByte((byte)Type);

			if (Rank != (byte)9)
			{
				fs.WriteByte(Rank);
				OobRank = (byte)0; // just clear it.
			}
			else
				fs.WriteByte(OobRank); // else retain the bug in user's .RMP file.

			fs.WriteByte((byte)Patrol);
			fs.WriteByte((byte)Attack);
			fs.WriteByte((byte)Spawn);
		}

		/// <summary>
		/// Gets the location of this node as a string. This funct inverts the
		/// z-level for readability (which is the policy in Mapview II).
		/// </summary>
		/// <param name="levels">the quantity of z-levels of the Map</param>
		/// <returns></returns>
		public string GetLocationString(int levels)
		{
			return ("c " + Col + "  r " + Row + "  L " + ((byte)(levels - Lev)));
		}
		#endregion


		#region Methods (override)
		public override bool Equals(object obj)
		{
			var other = obj as RouteNode;
			return (other != null && other.Index == Index);
		}

		public override int GetHashCode()
		{
			return Index; // nice hashcode ...
		}

		public override string ToString()
		{
			return ("c " + Col + "  r " + Row + "  L " + Lev);
		}
		#endregion
	}
}
