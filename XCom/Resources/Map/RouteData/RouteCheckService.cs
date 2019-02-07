using System;
using System.Collections.Generic;
using System.Windows.Forms;

using XCom.Resources.Map.RouteData;


namespace XCom.Resources.Map.RouteData
{
	public static class RouteCheckService
	{
		#region Fields
		private static MapFileChild _file;

		private static readonly List<RouteNode> _invalids = new List<RouteNode>();
		private static int _count;
		#endregion


		#region Fields (public)
		/// <summary>
		/// Checks for and if found gives user a choice to delete nodes that are
		/// outside of a Map's x/y/z bounds.
		/// </summary>
		/// <param name="file"></param>
		/// <returns>true if user opted to clear invalid nodes</returns>
		public static bool CheckNodeBounds(MapFileChild file)
		{
			if ((_file = file) != null)
			{
				_invalids.Clear();

				if ((_count = GetInvalidNodes()) != 0)
				{
					return ShowInvalids();
				}
			}
			return false;
		}

		/// <summary>
		/// Checks for and if found gives user a choice to delete nodes that are
		/// outside of a Map's x/y/z bounds.
		/// </summary>
		/// <param name="file"></param>
		/// <returns>true if node(s) are deleted</returns>
		public static bool CheckNodeBoundsMenuitem(MapFileChild file)
		{
			if ((_file = file) != null)
			{
				_invalids.Clear();

				if ((_count = GetInvalidNodes()) != 0)
				{
					return ShowInvalids();
				}

				MessageBox.Show(
							"There are no Out of Bounds nodes detected.",
							"Good stuff, Magister Ludi",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information,
							MessageBoxDefaultButton.Button1,
							0);
			}
			return false;
		}
		#endregion Fields (public)


		#region Fields (private)
		/// <summary>
		/// Fills the list with any invalid nodes.
		/// </summary>
		/// <returns>count of invalid nodes</returns>
		private static int GetInvalidNodes()
		{
			foreach (RouteNode node in _file.Routes)
			{
				if (RouteNodeCollection.IsNodeOutsideMapBounds(
															node,
															_file.MapSize.Cols,
															_file.MapSize.Rows,
															_file.MapSize.Levs))
				{
					_invalids.Add(node);
				}
			}
			return _invalids.Count;
		}

		/// <summary>
		/// Opens a dialog to delete the invalid nodes.
		/// </summary>
		/// <returns>true if user chooses to delete out-of-bounds nodes</returns>
		private static bool ShowInvalids()
		{
			using (var f = new RouteCheckInfobox())
			{
				bool singular = (_count == 1);
				string label = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"There {0} " + _count + " route-node{1} outside"
											+ " the bounds of the Map.{3}{3}Do you want {2} deleted?",
										singular ? "is" : "are",
										singular ? ""   : "s",
										singular ? "it" : "them",
										Environment.NewLine);

				string text = String.Empty;
				foreach (var node in _invalids)
				{
					text += "id " + node.Index + " : "
						  + node.GetLocationString(_file.MapSize.Levs) // TODO: format that
						  + Environment.NewLine;
				}

				f.SetText(label, text);

				if (f.ShowDialog() == DialogResult.Yes)
				{
					foreach (var node in _invalids)
						_file.Routes.DeleteNode(node);

					return true;
				}
			}
			return false;
		}
		#endregion Fields (private)
	}
}
