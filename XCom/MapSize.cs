using System;


namespace XCom
{
	public struct MapSize
	{
		#region Properties
		private readonly int _rows;
		public int Rows
		{
			get { return _rows; }
		}

		private readonly int _cols;
		public int Cols
		{
			get { return _cols; }
		}

		private readonly int _levs;
		public int Levs
		{
			get { return _levs; }
		}
		#endregion


		#region cTor
		public MapSize(int rows, int cols, int levs)
		{
			_rows = rows;
			_cols = cols;
			_levs = levs;
		}
		#endregion


		#region Methods
		public bool Equals(MapSize other)
		{
			return (other._rows == _rows
				 && other._cols == _cols
				 && other._levs == _levs);
		}
		#endregion


		#region Methods (override)
		/// <summary>
		/// Gets the Map's dimensions as a string to print in MainView's
		/// statusbar.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format(
							System.Globalization.CultureInfo.CurrentCulture,
							"{0}, {1}, {2}",
							_cols, _rows, _levs);
		}
		#endregion


		#region Satisfy FxCop CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes
		// http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
		// http://stackoverflow.com/questions/371328/why-is-it-important-to-override-gethashcode-when-equals-method-is-overridden
		// https://blogs.msdn.microsoft.com/ericlippert/2011/02/28/guidelines-and-rules-for-gethashcode/
		// http://stackoverflow.com/questions/873654/overriding-gethashcode-for-mutable-objects
		// and if you really want evil about it:
		// http://eternallyconfuzzled.com/tuts/algorithms/jsw_tut_hashing.aspx
		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				int hash = 17;
				// Suitable nullity checks etc, of course :)
				hash = hash * 29 + _rows.GetHashCode();
				hash = hash * 29 + _cols.GetHashCode();
				hash = hash * 29 + _levs.GetHashCode();
				return hash;
			}
		}

		// Guidelines for Overriding Equals() and Operator ==
		// https://msdn.microsoft.com/en-us/library/ms173147(VS.90).aspx
		public override bool Equals(object obj)
		{
			return (obj is MapSize && Equals((MapSize)obj));
		}

		public static bool operator ==(MapSize size1, MapSize size2)
		{
			return size1.Equals(size2);
		}

		public static bool operator !=(MapSize size1, MapSize size2)
		{
			return !size1.Equals(size2);
		}
		#endregion
	}
}
