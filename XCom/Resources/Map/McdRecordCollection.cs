using System;
using System.Collections.ObjectModel;


namespace XCom
{
	public class McdRecordCollection
		:
			ReadOnlyCollection<Tilepart>
	{
		#region cTor
		/// <summary>
		/// Instantiates a read-only collection of MCD records.
		/// </summary>
		/// <param name="tileparts"></param>
		internal McdRecordCollection(Tilepart[] tileparts)
			:
				base(tileparts)
		{}
		#endregion
	}
}
