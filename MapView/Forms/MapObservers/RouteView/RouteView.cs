using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using MapView.Forms.MainWindow;

using XCom;
using XCom.Interfaces.Base;
using XCom.Resources.Map.RouteData;


namespace MapView.Forms.MapObservers.RouteViews
{
	/// <summary>
	/// Does all the heavy-lifting/node-manipulations in RouteView and
	/// TopRouteView(Route).
	/// </summary>
	internal sealed partial class RouteView
		:
			MapObserverControl0
	{
		#region Enums
		private enum ConnectNodeType
		{
			ConnectNone,
			ConnectOneWay,
			ConnectTwoWays
		}
		#endregion


		#region Fields (static)
		private const string DontConnect   = " DontConnect";	// NOTE: the space is 'cause ComboBox entries
		private const string OneWayConnect = " OneWayConnect";	// don't get a Padding property.
		private const string TwoWayConnect = " TwoWayConnect";

		private const string NodeCopyPrefix  = "MVNode"; // TODO: use a struct to copy/paste the info.
		private const char NodeCopySeparator = '|';

		private const string Go = "go";

		private static RouteNode _nodeMoved;
		#endregion


		#region Fields
		private Panel _pnlRoutes; // NOTE: needs to be here for MapObserver0 stuff.

		private readonly List<object> _linksList = new List<object>();

		private int _col; // these are used only to print the clicked location info.
		private int _row;
		private int _lev;

		private bool _loadingInfo;

		/// <summary>
		/// Prevents two error-dialogs from showing if a key-cut is underway.
		/// </summary>
		private bool _asterisk;

		/// <summary>
		/// Used by UpdateNodeInformation().
		/// </summary>
		private readonly object[] _linkTypes =
		{
			LinkType.ExitNorth,
			LinkType.ExitEast,
			LinkType.ExitSouth,
			LinkType.ExitWest,
			LinkType.NotUsed
		};
		#endregion


		#region Properties (override)
		/// <summary>
		/// Inherited from IMapObserver through MapObserverControl0.
		/// </summary>
		public override MapFileBase MapBase
		{
			set
			{
				base.MapBase = value;
				MapFile      = value as MapFileChild;

				DeselectNode();

				if ((RoutePanel.MapFile = MapFile) != null)
				{
					cbRank.Items.Clear();

					if (MapFile.Descriptor.Pal == Palette.TftdBattle) // NOTE: Check TFTD else default to UFO.
						cbRank.Items.AddRange(RouteNodeCollection.NodeRankTftd);
					else
						cbRank.Items.AddRange(RouteNodeCollection.NodeRankUfo);

					UpdateNodeInformation();
				}
			}
		}
		#endregion


		#region Properties (static)
		private static RouteNode _nodeSelected;
		private static RouteNode NodeSelected
		{
			get { return _nodeSelected; }
			set
			{
				_nodeSelected           =
				RoutePanel.NodeSelected = value;
			}
		}

		/// <summary>
		/// Stores the node-id from which a "Go" button is clicked. Used to
		/// re-select the original node - which might not be equivalent to
		/// "Back" (if there were a Back button).
		/// </summary>
		private static int OgnodeId
		{ get; set; }
		#endregion


		#region Properties
		internal RoutePanel RoutePanel
		{ get; set; }

		private MapFileChild MapFile
		{ get; set; }

		/// <summary>
		/// Sets the RoutesChanged flag. This is only an intermediary that shows
		/// "routes changed" in RouteView; the real RoutesChanged flag is stored
		/// in XCom..MapFileBase. reasons.
		/// </summary>
		internal bool RoutesChanged
		{
			set { label_RoutesChanged.Visible = (MapFile.RoutesChanged = value); }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor. Instantiates the RouteView viewer and its components/controls.
		/// RouteViewForm and TopRouteViewForm will each invoke and maintain
		/// their own instantiations.
		/// </summary>
		public RouteView()
		{
			InitializeComponent();

			RoutePanel = new RoutePanel();
			RoutePanel.Dock = DockStyle.Fill;
			RoutePanel.RoutePanelMouseDownEvent += OnRoutePanelMouseDown;
			RoutePanel.RoutePanelMouseUpEvent   += OnRoutePanelMouseUp;
			RoutePanel.MouseMove                += OnRoutePanelMouseMove;
			RoutePanel.MouseLeave               += OnRoutePanelMouseLeave;
			RoutePanel.KeyDown                  += OnKeyDown;
			_pnlRoutes.Controls.Add(RoutePanel);

			// setup the connect-type dropdown entries
			tscbConnectType.Items.AddRange(new object[]
			{
				DontConnect,
				OneWayConnect,
				TwoWayConnect
			});

			// node data ->
			var unitTypes = new object[]
			{
				UnitType.Any,
				UnitType.Small,
				UnitType.Large,
				UnitType.FlyingSmall,
				UnitType.FlyingLarge
			};
			cbType.Items.AddRange(unitTypes);

			cbSpawn.Items.AddRange(RouteNodeCollection.SpawnWeight);

			foreach (var value in Enum.GetValues(typeof(PatrolPriority)))
				cbPatrol.Items.Add(value);

			foreach (var value in Enum.GetValues(typeof(BaseAttack)))
				cbAttack.Items.Add(value);

			// link data ->
			cbLink1UnitType.Items.AddRange(unitTypes);
			cbLink2UnitType.Items.AddRange(unitTypes);
			cbLink3UnitType.Items.AddRange(unitTypes);
			cbLink4UnitType.Items.AddRange(unitTypes);
			cbLink5UnitType.Items.AddRange(unitTypes);

			// TODO: change the distance textboxes to labels.

			DeselectNode();
		}
		#endregion


		#region Eventcalls (override) inherited from IMapObserver/MapObserverControl0
		/// <summary>
		/// Inherited from IMapObserver through MapObserverControl0.
		/// </summary>
		/// <param name="args"></param>
		public override void OnLocationSelectedObserver(LocationSelectedEventArgs args)
		{
			_col = args.Location.Col;
			_row = args.Location.Row;
			_lev = args.Location.Lev;

			PrintSelectedInfo();
		}

		/// <summary>
		/// Inherited from IMapObserver through MapObserverControl0.
		/// </summary>
		/// <param name="args"></param>
		public override void OnLevelChangedObserver(LevelChangedEventArgs args)
		{
			_lev = args.Level;

			var loc = RoutePanel.GetTileLocation(
											RoutePanel.CursorPosition.X,
											RoutePanel.CursorPosition.Y);
			int over = -1;
			if (loc.X != -1)
			{
				var node = ((XCMapTile)MapBase[loc.Y, loc.X, _lev]).Node;
				if (node != null)
					over = node.Index;
			}

			ViewerFormsManager.RouteView   .Control     .PrintOverInfo(over, loc);
			ViewerFormsManager.TopRouteView.ControlRoute.PrintOverInfo(over, loc);

			ViewerFormsManager.RouteView   .Control     .Refresh();
			ViewerFormsManager.TopRouteView.ControlRoute.Refresh();
		}
		#endregion


		#region Methods (print TileData)
		/// <summary>
		/// Clears the selected tile-info text when another Map loads.
		/// </summary>
		internal void ClearSelectedInfo()
		{
			lblSelected.Text = String.Empty;
		}

		/// <summary>
		/// Prints the currently selected tile-info to the TileData groupbox.
		/// NOTE: The displayed level is inverted here.
		/// </summary>
		private void PrintSelectedInfo()
		{
			if (MainViewUnderlay.Instance.MainViewOverlay.FirstClick)
			{
				string selected;
				int level;

				if (NodeSelected != null)
				{
					selected = "Selected " + NodeSelected.Index;
					level = NodeSelected.Lev;
				}
				else
				{
					selected = String.Empty;
					level = _lev;
				}

				selected += Environment.NewLine;

				selected += String.Format(
										System.Globalization.CultureInfo.InvariantCulture,
										"c {0}  r {1}  L {2}",
										_col + 1, _row + 1, MapFile.MapSize.Levs - level); // 1-based count.

				lblSelected.Text = selected;
			}
		}

		/// <summary>
		/// Prints the currently mouse-overed tile-info to the TileData groupbox.
		/// </summary>
		/// <param name="overId"></param>
		/// <param name="loc"></param>
		private void PrintOverInfo(int overId, Point loc)
		{
			string over;

			if (overId != -1)
				over = "Over " + overId;
			else
				over = String.Empty;

			over += Environment.NewLine;

			if (loc.X != -1)
			{
				over += String.Format(
									System.Globalization.CultureInfo.InvariantCulture,
									"c {0}  r {1}  L {2}",
									loc.X + 1, loc.Y + 1, MapFile.MapSize.Levs - _lev); // 1-based count.
			}

			lblOver.Text = over;
		}
		#endregion


		#region Eventcalls (mouse-events for RoutePanel)
		private void OnRoutePanelMouseMove(object sender, MouseEventArgs args)
		{
			int over;

			int x = args.X;
			int y = args.Y;

			var tile = RoutePanel.GetTile(ref x, ref y); // x/y -> tile-location
			if (tile != null && tile.Node != null)
				over = tile.Node.Index;
			else
				over = -1;

			var loc = new Point(x, y);

			ViewerFormsManager.RouteView   .Control     .PrintOverInfo(over, loc);
			ViewerFormsManager.TopRouteView.ControlRoute.PrintOverInfo(over, loc);

			RoutePanel.CursorPosition = new Point(args.X, args.Y);

			ViewerFormsManager.RouteView   .Control     .RoutePanel.Refresh();
			ViewerFormsManager.TopRouteView.ControlRoute.RoutePanel.Refresh();
		}						

		/// <summary>
		/// Hides the info-overlay when the mouse leaves this control.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRoutePanelMouseLeave(object sender, EventArgs e)
		{
			RoutePanel.CursorPosition = new Point(-1, -1);

			ViewerFormsManager.RouteView   .Control     .RoutePanel.Refresh();
			ViewerFormsManager.TopRouteView.ControlRoute.RoutePanel.Refresh();
		}							

		private void OnRoutePanelMouseUp(object sender, RoutePanelEventArgs args)
		{
			if (_nodeMoved != null)
			{
				if (((XCMapTile)args.Tile).Node == null)
				{
					RoutesChanged = true;

					((XCMapTile)MapFile[_nodeMoved.Row, // clear the node from the previous tile
										_nodeMoved.Col,
										_nodeMoved.Lev]).Node = null;

					_nodeMoved.Col = (byte)args.Location.Col; // reassign the node's x/y/z values
					_nodeMoved.Row = (byte)args.Location.Row; // these get saved w/ Routes.
					_nodeMoved.Lev = args.Location.Lev;

					((XCMapTile)args.Tile).Node = _nodeMoved; // assign the node to the tile at the mouse-up location.

					// Select the new location so the links draw and the selected node highlights
					// properly but don't re-path the selected-lozenge. Let user see where the
					// node-drag started until a click calls RoutePanelParent.PathSelectedLozenge().
					RoutePanel.SelectedPosition = new Point(_nodeMoved.Col, _nodeMoved.Row);

					ViewerFormsManager.RouteView   .Control     .UpdateLinkDistances();
					ViewerFormsManager.TopRouteView.ControlRoute.UpdateLinkDistances();
				}
				else if (args.Location.Col != _nodeMoved.Col
					||   args.Location.Row != _nodeMoved.Row
					||   args.Location.Lev != _nodeMoved.Lev)
				{
					MessageBox.Show(
								this,
								"Cannot move node onto another node.",
								"Err..",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error,
								MessageBoxDefaultButton.Button1,
								0);
				}
				_nodeMoved = null;
			}
		}

		/// <summary>
		/// Updates distances to and from the currently selected node.
		/// NOTE: 'NodeSelected' must be valid before call.
		/// </summary>
		private void UpdateLinkDistances()
		{
			for (int slot = 0; slot != RouteNode.LinkSlots; ++slot) // update distances to selected node's linked nodes ->
			{
				string distance;

				var link = NodeSelected[slot];
				switch (link.Destination)
				{
					case Link.NotUsed: // NOTE: Should not change; is here to help keep distances consistent.
						link.Distance = 0;
						distance = String.Empty;
						break;

					case Link.ExitWest: // NOTE: Should not change; is here to help keep distances consistent.
					case Link.ExitNorth:
					case Link.ExitEast:
					case Link.ExitSouth:
						link.Distance = 0;
						distance = "0";
						break;

					default:
						link.Distance = CalculateLinkDistance(
															NodeSelected,
															MapFile.Routes[link.Destination]);
						distance = link.Distance.ToString(System.Globalization.CultureInfo.InvariantCulture)
								 + GetDistanceArrow(slot);
						break;
				}
				UpdateLinkText(slot, distance);
			}

			for (var id = 0; id != MapFile.Routes.Length; ++id) // update distances of any links to the selected node ->
			{
				if (id != NodeSelected.Index) // NOTE: a node shall not link to itself.
				{
					var node = MapFile.Routes[id];

					for (int slot = 0; slot != RouteNode.LinkSlots; ++slot)
					{
						var link = node[slot];
						if (link.Destination == NodeSelected.Index)
							link.Distance = CalculateLinkDistance(
																node,
																NodeSelected);
					}
				}
			}
		}

		private void UpdateLinkText(int slot, string distance)
		{
			switch (slot)
			{
				case 0: tbLink1Dist.Text = distance; break;
				case 1: tbLink2Dist.Text = distance; break;
				case 2: tbLink3Dist.Text = distance; break;
				case 3: tbLink4Dist.Text = distance; break;
				case 4: tbLink5Dist.Text = distance; break;
			}
		}

		/// <summary>
		/// Selects a node on LMB, creates and/or connects nodes on RMB.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void OnRoutePanelMouseDown(object sender, RoutePanelEventArgs args)
		{
			//LogFile.WriteLine("OnRoutePanelMouseDown()");

			bool update = false;

			var node = ((XCMapTile)args.Tile).Node;

			if (NodeSelected == null)
			{
				if ((NodeSelected = node) == null
					&& args.MouseButton == MouseButtons.Right)
				{
					RoutesChanged = true;
					NodeSelected = MapFile.AddRouteNode(args.Location);
				}
				update = (NodeSelected != null);
			}
			else // if a node is already selected ...
			{
				if (node == null)
				{
					if (args.MouseButton == MouseButtons.Right)
					{
						RoutesChanged = true;
						node = MapFile.AddRouteNode(args.Location);
						ConnectNode(node);
					}
//					RoutePanel.Refresh(); don't work.

					NodeSelected = node;
					update = true;
				}
				else if (node != NodeSelected)
				{
					if (args.MouseButton == MouseButtons.Right)
						ConnectNode(node);

//					RoutePanel.Refresh(); don't work.

					NodeSelected = node;
					update = true;
				}
				// else the selected node is the node clicked.
			}

			if (update)
			{
				ViewerFormsManager.RouteView   .Control     .UpdateNodeInformation();
				ViewerFormsManager.TopRouteView.ControlRoute.UpdateNodeInformation();
			}

			_nodeMoved = NodeSelected;

			bool valid = (NodeSelected != null);

			ViewerFormsManager.RouteView   .Control     .btnCut           .Enabled =
			ViewerFormsManager.TopRouteView.ControlRoute.btnCut           .Enabled =

			ViewerFormsManager.RouteView   .Control     .btnCopy          .Enabled =
			ViewerFormsManager.TopRouteView.ControlRoute.btnCopy          .Enabled =

			ViewerFormsManager.RouteView   .Control     .btnDelete        .Enabled =
			ViewerFormsManager.TopRouteView.ControlRoute.btnDelete        .Enabled =

			ViewerFormsManager.RouteView   .Control     .tsmiClearLinkData.Enabled =
			ViewerFormsManager.TopRouteView.ControlRoute.tsmiClearLinkData.Enabled = valid;

			ViewerFormsManager.RouteView   .Control     .btnPaste         .Enabled =
			ViewerFormsManager.TopRouteView.ControlRoute.btnPaste         .Enabled = valid
																				  && Clipboard.GetText().Split(NodeCopySeparator)[0] == NodeCopyPrefix;
		}

		/// <summary>
		/// Checks connector and connects nodes if applicable.
		/// </summary>
		/// <param name="node">the node to try to link the currently selected
		/// node to</param>
		private void ConnectNode(RouteNode node)
		{
			var type = GetConnectorType();
			if (type != ConnectNodeType.ConnectNone)
			{
				int slot = GetOpenLinkSlot(NodeSelected, node.Index);
				if (slot > -1)
				{
					RoutesChanged = true;
					NodeSelected[slot].Destination = node.Index;
					NodeSelected[slot].Distance = CalculateLinkDistance(NodeSelected, node);
				}
				else if (slot == -3)
				{
					MessageBox.Show(
								this,
								"Source node could not be linked to the destination node."
									+ " Its link-slots are full.",
								"Warning",
								MessageBoxButtons.OK,
								MessageBoxIcon.Exclamation,
								MessageBoxDefaultButton.Button1,
								0);
					// TODO: the message leaves the RoutePanel drawn in an awkward state
					// but discovering where to call Refresh() is not trivial.
//					RoutePanel.Refresh(); // in case of a warning this needs to happen ...
					// Fortunately a simple mouseover straightens things out for now.
				}

				if (type == ConnectNodeType.ConnectTwoWays)
				{
					slot = GetOpenLinkSlot(node, NodeSelected.Index);
					if (slot > -1)
					{
						RoutesChanged = true;
						node[slot].Destination = NodeSelected.Index;
						node[slot].Distance = CalculateLinkDistance(node, NodeSelected);
					}
					else if (slot == -3)
					{
						MessageBox.Show(
									this,
									"Destination node could not be linked to the source node."
										+ " Its link-slots are full.",
									"Warning",
									MessageBoxButtons.OK,
									MessageBoxIcon.Exclamation,
									MessageBoxDefaultButton.Button1,
									0);
						// TODO: the message leaves the RoutePanel drawn in an awkward state
						// but discovering where to call Refresh() is not trivial.
//						RoutePanel.Refresh(); // in case of a warning this needs to happen ...
						// Fortunately a simple mouseover straightens things out for now.
					}
				}
			}
		}

		/// <summary>
		/// Gets the user-selected Connector type.
		/// </summary>
		/// <returns></returns>
		private ConnectNodeType GetConnectorType()
		{
			if (tscbConnectType.Text == OneWayConnect)
				return ConnectNodeType.ConnectOneWay;

			if (tscbConnectType.Text == TwoWayConnect)
				return ConnectNodeType.ConnectTwoWays;

			return ConnectNodeType.ConnectNone;
		}

		/// <summary>
		/// Gets the first available link-slot for a given node.
		/// </summary>
		/// <param name="node">the node to check the link-slots of</param>
		/// <param name="dest">the id of the destination node</param>
		/// <returns>id of an available link-slot, or
		/// -1 if the source-node is null (not sure if this ever happens)
		/// -2 if the link already exists
		/// -3 if there are no free slots</returns>
		private static int GetOpenLinkSlot(RouteNode node, int dest)
		{
			if (node != null)
			{
				for (int slot = 0; slot != RouteNode.LinkSlots; ++slot) // first check if destination-id already exists
				{
					if (dest != -1 && node[slot].Destination == dest)
						return -2;
				}

				for (int slot = 0; slot != RouteNode.LinkSlots; ++slot) // then check for an open slot
				{
					if (node[slot].Destination == (byte)LinkType.NotUsed)
						return slot;
				}
				return -3;
			}
			return -1;
		}
		#endregion


		/// <summary>
		/// Updates node-info fields below the panel itself.
		/// </summary>
		private void UpdateNodeInformation()
		{
			SuspendLayout();

			PrintSelectedInfo();

			_loadingInfo = true;

			if (NodeSelected == null)
			{
				btnCut      .Enabled =
				btnCopy     .Enabled =
				btnPaste    .Enabled =
				btnDelete   .Enabled =

				gbTileData  .Enabled =
				gbNodeData  .Enabled =
				gbLinkData  .Enabled =
				gbNodeEditor.Enabled =

				btnGoLink1  .Enabled =
				btnGoLink2  .Enabled =
				btnGoLink3  .Enabled =
				btnGoLink4  .Enabled =
				btnGoLink5  .Enabled = false;

				btnGoLink1.Text =
				btnGoLink2.Text =
				btnGoLink3.Text =
				btnGoLink4.Text =
				btnGoLink5.Text = String.Empty;


				cbType  .SelectedItem = UnitType.Any;
				cbPatrol.SelectedItem = PatrolPriority.Zero;
				cbAttack.SelectedItem = BaseAttack.Zero;

				if (MapFile.Descriptor.Pal == Palette.TftdBattle)
					cbRank.SelectedItem = RouteNodeCollection.NodeRankTftd[(byte)NodeRankTftd.CivScout];
				else
					cbRank.SelectedItem = RouteNodeCollection.NodeRankUfo[(byte)NodeRankUfo.CivScout];

				cbSpawn.SelectedItem = RouteNodeCollection.SpawnWeight[(byte)SpawnWeight.None];

				cbLink1Dest.SelectedItem = // TODO: figure out why these show blank and not "NotUsed"
				cbLink2Dest.SelectedItem = // when the app loads its very first Map.
				cbLink3Dest.SelectedItem =
				cbLink4Dest.SelectedItem =
				cbLink5Dest.SelectedItem = LinkType.NotUsed;

				cbLink1UnitType.SelectedItem =
				cbLink2UnitType.SelectedItem =
				cbLink3UnitType.SelectedItem =
				cbLink4UnitType.SelectedItem =
				cbLink5UnitType.SelectedItem = UnitType.Any;

				tbLink1Dist.Text =
				tbLink2Dist.Text =
				tbLink3Dist.Text =
				tbLink4Dist.Text =
				tbLink5Dist.Text = String.Empty;
			}
			else // selected node is valid ->
			{
				gbTileData  .Enabled =
				gbNodeData  .Enabled =
				gbLinkData  .Enabled =
				gbNodeEditor.Enabled = true;

				cbType  .SelectedItem = NodeSelected.Type;
				cbPatrol.SelectedItem = NodeSelected.Patrol;
				cbAttack.SelectedItem = NodeSelected.Attack;

				if (MapFile.Descriptor.Pal == Palette.TftdBattle)
					cbRank.SelectedItem = RouteNodeCollection.NodeRankTftd[NodeSelected.Rank];
				else
					cbRank.SelectedItem = RouteNodeCollection.NodeRankUfo[NodeSelected.Rank];

				cbSpawn.SelectedItem = RouteNodeCollection.SpawnWeight[(byte)NodeSelected.Spawn];

				cbLink1Dest.Items.Clear();
				cbLink2Dest.Items.Clear();
				cbLink3Dest.Items.Clear();
				cbLink4Dest.Items.Clear();
				cbLink5Dest.Items.Clear();

				_linksList.Clear();

				for (byte id = 0; id != MapFile.Routes.Length; ++id)
				{
					if (id != NodeSelected.Index)
						_linksList.Add(id);			// <- add all linkable (ie. other) nodes
				}
				_linksList.AddRange(_linkTypes);	// <- add the four compass-points + link-not-used.

				object[] linkListArray = _linksList.ToArray();

				cbLink1Dest.Items.AddRange(linkListArray);
				cbLink2Dest.Items.AddRange(linkListArray);
				cbLink3Dest.Items.AddRange(linkListArray);
				cbLink4Dest.Items.AddRange(linkListArray);
				cbLink5Dest.Items.AddRange(linkListArray);


				ComboBox cbTypL, cbDest;
				TextBox tbDist;
				Button btnGo;

				Link link;
				byte dest;

				for (int slot = 0; slot != RouteNode.LinkSlots; ++slot)
				{
					switch (slot)
					{
						case 0:
							cbTypL = cbLink1UnitType;
							cbDest = cbLink1Dest;
							tbDist = tbLink1Dist;
							btnGo  = btnGoLink1;
							break;

						case 1:
							cbTypL = cbLink2UnitType;
							cbDest = cbLink2Dest;
							tbDist = tbLink2Dist;
							btnGo  = btnGoLink2;
							break;

						case 2:
							cbTypL = cbLink3UnitType;
							cbDest = cbLink3Dest;
							tbDist = tbLink3Dist;
							btnGo  = btnGoLink3;
							break;

						case 3:
							cbTypL = cbLink4UnitType;
							cbDest = cbLink4Dest;
							tbDist = tbLink4Dist;
							btnGo  = btnGoLink4;
							break;

						default: // case 4:
							cbTypL = cbLink5UnitType;
							cbDest = cbLink5Dest;
							tbDist = tbLink5Dist;
							btnGo  = btnGoLink5;
							break;
					}

					link = NodeSelected[slot];

					cbTypL.SelectedItem = link.Type;
					btnGo.Enabled = link.StandardNode();

					dest = link.Destination;
					if (link.Used())
					{
						btnGo.Text = Go;
						tbDist.Text = Convert.ToString(
													link.Distance,
													System.Globalization.CultureInfo.InvariantCulture)
									+ GetDistanceArrow(slot);

						if (link.StandardNode())
							cbDest.SelectedItem = dest;
						else
							cbDest.SelectedItem = (LinkType)dest;
					}
					else
					{
						btnGo .Text =
						tbDist.Text = String.Empty;
						cbDest.SelectedItem = (LinkType)dest;
					}
				}
			}

			_loadingInfo = false;

			ResumeLayout();
		}

		/// <summary>
		/// Gets an up/down suffix for the linked distance from the currently
		/// selected node, given the link-slot to the destination node. If the
		/// destination is on the same level as the selected node, a blank
		/// string is returned.
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		private string GetDistanceArrow(int slot)
		{
			var link = NodeSelected[slot];
			if (link.StandardNode())
			{
				var dest = MapFile.Routes[link.Destination];
				if (dest != null) // safety.
				{
					if (NodeSelected.Lev > dest.Lev)
						return " \u2191"; // up arrow
	
					if (NodeSelected.Lev < dest.Lev)
						return " \u2193"; // down arrow
				}
			}
			return String.Empty;
		}


		#region Eventcalls (NodeData)
		private void OnUnitTypeSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loadingInfo)
			{
				RoutesChanged = true;
				NodeSelected.Type = (UnitType)cbType.SelectedItem;

				if (Tag as String == "ROUTE")
					ViewerFormsManager.TopRouteView.ControlRoute.cbType.SelectedIndex = cbType.SelectedIndex;
				else //if (Tag == "TOPROUTE")
					ViewerFormsManager.RouteView.Control.cbType.SelectedIndex = cbType.SelectedIndex;
			}
		}

		private bool _bypassRankChanged;
		private void OnNodeRankSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loadingInfo && !_bypassRankChanged)
			{
				if (cbRank.SelectedIndex == 9)
				{
					_bypassRankChanged = true;	// because this funct is going to fire again immediately
					cbRank.SelectedIndex = (int)NodeSelected.Rank;
					_bypassRankChanged = false;	// and I don't want the RoutesChanged flagged.
				}
				else
				{
					RoutesChanged = true;
					NodeSelected.Rank = (byte)cbRank.SelectedIndex;
//					NodeSelected.Rank = (byte)((Pterodactyl)cbRank.SelectedItem).Case; // <- MapView1-type code.

					NodeSelected.OobRank = (byte)0;

					if (Tag as String == "ROUTE")
						ViewerFormsManager.TopRouteView.ControlRoute.cbRank.SelectedIndex = cbRank.SelectedIndex;
					else //if (Tag == "TOPROUTE")
						ViewerFormsManager.RouteView.Control.cbRank.SelectedIndex = cbRank.SelectedIndex;
				}
			}
		}

		private void OnPatrolPrioritySelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loadingInfo)
			{
				RoutesChanged = true;
				NodeSelected.Patrol = (PatrolPriority)cbPatrol.SelectedItem;

				if (Tag as String == "ROUTE")
					ViewerFormsManager.TopRouteView.ControlRoute.cbPatrol.SelectedIndex = cbPatrol.SelectedIndex;
				else //if (Tag == "TOPROUTE")
					ViewerFormsManager.RouteView.Control.cbPatrol.SelectedIndex = cbPatrol.SelectedIndex;

				ViewerFormsManager.RouteView   .Control     .Refresh(); // update the importance bar
				ViewerFormsManager.TopRouteView.ControlRoute.Refresh();
			}
		}

		private void OnSpawnWeightSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loadingInfo)
			{
				RoutesChanged = true;
				NodeSelected.Spawn = (SpawnWeight)((Pterodactyl)cbSpawn.SelectedItem).Case;

				if (Tag as String == "ROUTE")
					ViewerFormsManager.TopRouteView.ControlRoute.cbSpawn.SelectedIndex = cbSpawn.SelectedIndex;
				else //if (Tag == "TOPROUTE")
					ViewerFormsManager.RouteView.Control.cbSpawn.SelectedIndex = cbSpawn.SelectedIndex;

				ViewerFormsManager.RouteView   .Control     .Refresh(); // update the importance bar
				ViewerFormsManager.TopRouteView.ControlRoute.Refresh();
			}
		}

		private void OnBaseAttackSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loadingInfo)
			{
				RoutesChanged = true;
				NodeSelected.Attack = (BaseAttack)cbAttack.SelectedItem;

				if (Tag as String == "ROUTE")
					ViewerFormsManager.TopRouteView.ControlRoute.cbAttack.SelectedIndex = cbAttack.SelectedIndex;
				else //if (Tag == "TOPROUTE")
					ViewerFormsManager.RouteView.Control.cbAttack.SelectedIndex = cbAttack.SelectedIndex;
			}
		}
		#endregion


		#region Eventcalls (LinkData)
		/// <summary>
		/// Changes a link's destination.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLinkDestSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loadingInfo)
			{
				RoutesChanged = true;

				int slot;
				TextBox tb;
				Button btnGo;

				var cb = sender as ComboBox;
				if (cb == cbLink1Dest)
				{
					slot  = 0;
					tb    = tbLink1Dist;
					btnGo = btnGoLink1;
				}
				else if (cb == cbLink2Dest)
				{
					slot  = 1;
					tb    = tbLink2Dist;
					btnGo = btnGoLink2;
				}
				else if (cb == cbLink3Dest)
				{
					slot  = 2;
					tb    = tbLink3Dist;
					btnGo = btnGoLink3;
				}
				else if (cb == cbLink4Dest)
				{
					slot  = 3;
					tb    = tbLink4Dist;
					btnGo = btnGoLink4;
				}
				else //if (cb == cbLink5Dest)
				{
					slot  = 4;
					tb    = tbLink5Dist;
					btnGo = btnGoLink5;
				}

				var dest = cb.SelectedItem as byte?; // check for id or compass pt/not used.
				if (!dest.HasValue)
					dest = (byte?)(cb.SelectedItem as LinkType?);

				bool enable, text;

				var link = NodeSelected[slot];
				switch (link.Destination = dest.Value)
				{
					case Link.NotUsed:
						link.Type = UnitType.Any;

						tb.Text = String.Empty;
						link.Distance = 0;

						enable =
						text   = false;
						break;

					case Link.ExitWest:
					case Link.ExitNorth:
					case Link.ExitEast:
					case Link.ExitSouth:
						tb.Text = "0";
						link.Distance = 0;

						enable = false;
						text   = true;
						break;

					default:
						link.Distance = CalculateLinkDistance(
															NodeSelected,
															MapFile.Routes[link.Destination],
															tb,
															slot);
						enable =
						text   = true;
						break;
				}

				btnGo.Enabled = enable;
				btnGo.Text = text ? Go : String.Empty;

				RoutePanel.SpotPosition = new Point(-1, -1);

				if (Tag as String == "ROUTE")
				{
					ViewerFormsManager.TopRouteView.ControlRoute.TransferDestination(
																				slot,
																				cb.SelectedIndex,
																				tb.Text,
																				enable,
																				btnGo.Text);
				}
				else //if (Tag == "TOPROUTE")
				{
					ViewerFormsManager.RouteView.Control.TransferDestination(
																		slot,
																		cb.SelectedIndex,
																		tb.Text,
																		enable,
																		btnGo.Text);
				}

				ViewerFormsManager.RouteView   .Control     .Refresh();
				ViewerFormsManager.TopRouteView.ControlRoute.Refresh();
			}
		}

		/// <summary>
		/// Transfers link-destination values from RouteView to
		/// TopRouteView(Route) or vice versa. The fields are actually in the
		/// other viewer.
		/// </summary>
		/// <param name="slot">the link-slot's id</param>
		/// <param name="dest">the selected-index of the destination</param>
		/// <param name="dist">the distance text</param>
		/// <param name="enable">true to enable the Go button</param>
		/// <param name="text">the Go button text</param>
		private void TransferDestination(int slot, int dest, string dist, bool enable, string text)
		{
			ComboBox cbDest;
			TextBox tbDist;
			Button btnGo;

			switch (slot)
			{
				case 0:
					cbDest = cbLink1Dest;
					tbDist = tbLink1Dist;
					btnGo  = btnGoLink1;
					break;

				case 1:
					cbDest = cbLink2Dest;
					tbDist = tbLink2Dist;
					btnGo  = btnGoLink2;
					break;

				case 2:
					cbDest = cbLink3Dest;
					tbDist = tbLink3Dist;
					btnGo  = btnGoLink3;
					break;

				case 3:
					cbDest = cbLink4Dest;
					tbDist = tbLink4Dist;
					btnGo  = btnGoLink4;
					break;

				default: //case 4:
					cbDest = cbLink5Dest;
					tbDist = tbLink5Dist;
					btnGo  = btnGoLink5;
					break;
			}

			cbDest.SelectedIndex = dest;
			tbDist.Text          = dist;
			btnGo.Enabled        = enable;
			btnGo.Text           = text;
		}

		/// <summary>
		/// Calculates the distance between two nodes by Pythagoras.
		/// </summary>
		/// <param name="nodeA">a RouteNode</param>
		/// <param name="nodeB">another RouteNode</param>
		/// <param name="textBox">the textbox that shows the distance (default null)</param>
		/// <param name="slot">the slot of the textbox - not used unless 'textBox'
		/// is specified (default 0)</param>
		/// <returns>the distance as a byte-value</returns>
		private byte CalculateLinkDistance(
				RouteNode nodeA,
				RouteNode nodeB,
				Control textBox = null,
				int slot = 0)
		{
			int dist = (int)Math.Sqrt(
									Math.Pow(nodeA.Col - nodeB.Col, 2) +
									Math.Pow(nodeA.Row - nodeB.Row, 2) +
									Math.Pow(nodeA.Lev - nodeB.Lev, 2));
			if (textBox != null)
				textBox.Text = dist.ToString(System.Globalization.CultureInfo.InvariantCulture)
							 + GetDistanceArrow(slot);

			return (byte)dist;
		}


		/// <summary>
		/// Changes a link's UnitType.
		/// TODO: Since a link's UnitType is not used just give it the value
		/// of the link's destination UnitType.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLinkUnitTypeSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!_loadingInfo)
			{
				RoutesChanged = true;

				int slot;

				var cb = sender as ComboBox;
				if (cb == cbLink1UnitType)
					slot = 0;
				else if (cb == cbLink2UnitType)
					slot = 1;
				else if (cb == cbLink3UnitType)
					slot = 2;
				else if (cb == cbLink4UnitType)
					slot = 3;
				else //if (cb == cbLink5UnitType)
					slot = 4;

				NodeSelected[slot].Type = (UnitType)cb.SelectedItem;

				if (Tag as String == "ROUTE")
					ViewerFormsManager.TopRouteView.ControlRoute.TransferUnitType(slot, cb.SelectedIndex);
				else //if (Tag == "TOPROUTE")
					ViewerFormsManager.RouteView.Control.TransferUnitType(slot, cb.SelectedIndex);
			}
		}

		/// <summary>
		/// Transfers link-unittype values from RouteView to TopRouteView(Route)
		/// or vice versa. The field is actually in the other viewer.
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="type"></param>
		private void TransferUnitType(int slot, int type)
		{
			ComboBox cbUnitType;
			switch (slot)
			{
				case 0:  cbUnitType = cbLink1UnitType; break;
				case 1:  cbUnitType = cbLink2UnitType; break;
				case 2:  cbUnitType = cbLink3UnitType; break;
				case 3:  cbUnitType = cbLink4UnitType; break;
				default: cbUnitType = cbLink5UnitType; break; //case 4
			}
			cbUnitType.SelectedIndex = type;
		}

		/// <summary>
		/// Selects the node at the destination of a link when a Go-button is
		/// clicked.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLinkGoClick(object sender, EventArgs e)
		{
			int slot;

			var btn = sender as Button;
			if      (btn == btnGoLink1) slot = 0;
			else if (btn == btnGoLink2) slot = 1;
			else if (btn == btnGoLink3) slot = 2;
			else if (btn == btnGoLink4) slot = 3;
			else                        slot = 4; //if (btn == btnGoLink5)

			var link   = NodeSelected[slot];
			byte dest  = link.Destination;
			var node   = MapFile.Routes[dest];
			int levels = MapFile.MapSize.Levs;

			if (!RouteNodeCollection.IsNodeOutsideMapBounds(
														node,
														MapFile.MapSize.Cols,
														MapFile.MapSize.Rows,
														levels))
			{
				OgnodeId = NodeSelected.Index; // store the current nodeId for the og-button.

				ViewerFormsManager.RouteView   .Control     .btnOg.Enabled =
				ViewerFormsManager.TopRouteView.ControlRoute.btnOg.Enabled = true;

				SelectNode(link.Destination);

				SpotGoDestination(slot); // highlight back to the startnode.
			}
			else
			{
				string info = String.Format(
										System.Globalization.CultureInfo.CurrentCulture,
										"Destination node is outside the Map's boundaries.{0}{0}"
											+ "id {1} : {2}",
										Environment.NewLine,
										dest,
										node.GetLocationString(levels));
				MessageBox.Show(
							info,
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1,
							0);
			}
		}

		/// <summary>
		/// Deals with the ramifications of a Go or Og click.
		/// </summary>
		/// <param name="id"></param>
		private void SelectNode(int id)
		{
			var node = MapFile.Routes[id];

			if (node.Lev != MapFile.Level)
				MapFile.Level = node.Lev;			// fire LevelChangedEvent.

			MapFile.Location = new MapLocation(		// fire LocationSelectedEvent.
											node.Row,
											node.Col,
											MapFile.Level);

			var start = new Point(node.Col, node.Row);

			MainViewUnderlay.Instance.MainViewOverlay.ProcessTileSelection(start, start);

			var args = new RoutePanelEventArgs();
			args.MouseButton = MouseButtons.Left;
			args.Tile        = MapFile[node.Row, node.Col];
			args.Location    = MapFile.Location;

			OnRoutePanelMouseDown(null, args);


			RoutePanel.SelectedPosition = start;

			ViewerFormsManager.RouteView   .Control     .Refresh();
			ViewerFormsManager.TopRouteView.ControlRoute.Refresh();
		}

		/// <summary>
		/// Highlights a link-line and destination-tile when the mousecursor
		/// enters or hovers over a link-slot's control object.
		/// NOTE.NET anomaly: After the selected index changes of a combobox
		/// the next mouse-enter event won't fire; but doing a mouse-leave then
		/// another mouse-enter catches.
		/// WORKAROUND: Use the mouse-hover event for the comboboxes instead of
		/// the mouse-enter event. For this the cursor has to be kept stationary
		/// and there is also a slight lag.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLinkMouseEnter(object sender, EventArgs e)
		{
			int slot;

			string tag = (sender as Control).Tag as String;
			if (tag == "L1")
				slot = 0;
			else if (tag == "L2")
				slot = 1;
			else if (tag == "L3")
				slot = 2;
			else if (tag == "L4")
				slot = 3;
			else //if (tag == "L5")
				slot = 4;

			SpotGoDestination(slot);
		}

		/// <summary>
		/// Sets the highlighted destination link-line and node if applicable.
		/// </summary>
		/// <param name="slot">the link-slot whose destination should get
		/// highlighted</param>
		private void SpotGoDestination(int slot)
		{
			if (NodeSelected != null && NodeSelected[slot] != null) // safety: Go should not be enabled unless a node is selected.
			{
				byte dest = NodeSelected[slot].Destination;
				if (dest != Link.NotUsed)
				{
					int c, r;
					switch (dest)
					{
						case Link.ExitNorth: c = r = -2; break;
						case Link.ExitEast:  c = r = -3; break;
						case Link.ExitSouth: c = r = -4; break;
						case Link.ExitWest:  c = r = -5; break;
	
						default:
							var node = MapFile.Routes[dest];
							c = (int)node.Col;
							r = (int)node.Row;
							break;
					}
	
					RoutePanel.SpotPosition = new Point(c, r);
					Refresh();
				}
			}
		}

		private void OnLinkMouseLeave(object sender, EventArgs e)
		{
			RoutePanel.SpotPosition = new Point(-1, -1);
			Refresh();
		}

		private void OnOgClick(object sender, EventArgs e)
		{
			if (OgnodeId < MapFile.Routes.Length) // in case nodes were deleted.
			{
				if (NodeSelected == null || OgnodeId != NodeSelected.Index)
					SelectNode(OgnodeId);
			}
			else
			{
				ViewerFormsManager.RouteView   .Control     .btnOg.Enabled =
				ViewerFormsManager.TopRouteView.ControlRoute.btnOg.Enabled = false;
			}
		}

		private void OnOgMouseEnter(object sender, EventArgs e)
		{
			if (OgnodeId < MapFile.Routes.Length) // in case nodes were deleted.
			{
				var node = MapFile.Routes[OgnodeId];
				RoutePanel.SpotPosition = new Point(node.Col, node.Row);
				Refresh();
			}
		}

		/// <summary>
		/// Disables the og-button when a Map gets loaded.
		/// </summary>
		internal void DisableOg()
		{
			ViewerFormsManager.RouteView   .Control     .btnOg.Enabled =
			ViewerFormsManager.TopRouteView.ControlRoute.btnOg.Enabled = false;
		}
		#endregion


		#region Eventcalls (Edit handlers)
		private void OnCutClick(object sender, EventArgs e)
		{
			OnCopyClick(null, null);
			OnDeleteClick(null, null);
		}

		private void OnCopyClick(object sender, EventArgs e)
		{
			if (NodeSelected != null)
			{
				ViewerFormsManager.RouteView   .Control     .btnPaste.Enabled =
				ViewerFormsManager.TopRouteView.ControlRoute.btnPaste.Enabled = true;

				var nodeText = string.Format(
										System.Globalization.CultureInfo.InvariantCulture,
										"{0}{6}{1}{6}{2}{6}{3}{6}{4}{6}{5}",
										NodeCopyPrefix,
										cbType.SelectedIndex,
										cbPatrol.SelectedIndex,
										cbAttack.SelectedIndex,
										cbRank.SelectedIndex,
										cbSpawn.SelectedIndex,
										NodeCopySeparator);

				// TODO: include Link info ... perhaps.
				// But re-assigning the link node-ids would be difficult, since
				// those nodes could have be deleted, etc.
				Clipboard.SetText(nodeText);
			}
			else
				ShowDialogAsterisk("A node must be selected.");
		}

		private void OnPasteClick(object sender, EventArgs e)
		{
			if (NodeSelected != null)
			{
				var nodeData = Clipboard.GetText().Split(NodeCopySeparator);
				if (nodeData[0] == NodeCopyPrefix)
				{
					RoutesChanged = true;

					var invariant = System.Globalization.CultureInfo.InvariantCulture;

					cbType  .SelectedIndex = Int32.Parse(nodeData[1], invariant);
					cbPatrol.SelectedIndex = Int32.Parse(nodeData[2], invariant);
					cbAttack.SelectedIndex = Int32.Parse(nodeData[3], invariant);
					cbRank  .SelectedIndex = Int32.Parse(nodeData[4], invariant);
					cbSpawn .SelectedIndex = Int32.Parse(nodeData[5], invariant);

					// TODO: include Link info ... perhaps.
					// But re-assigning the link node-ids would be difficult, since
					// those nodes could have be deleted, etc.
				}
				else // non-node data is on the clipboard.
				{
					ViewerFormsManager.RouteView   .Control     .btnPaste.Enabled =
					ViewerFormsManager.TopRouteView.ControlRoute.btnPaste.Enabled = false;

					ShowDialogAsterisk("The data on the clipboard is not a node.");
				}
			}
			else
				ShowDialogAsterisk("A node must be selected.");
		}

		private void OnDeleteClick(object sender, EventArgs e)
		{
			if (NodeSelected != null)
			{
				RoutesChanged = true;

				((XCMapTile)MapFile[NodeSelected.Row,
									NodeSelected.Col,
									NodeSelected.Lev]).Node = null;
				MapFile.Routes.DeleteNode(NodeSelected);

				ViewerFormsManager.RouteView   .Control     .DeselectNode();
				ViewerFormsManager.TopRouteView.ControlRoute.DeselectNode();

				ViewerFormsManager.RouteView   .Control     .UpdateNodeInformation();
				ViewerFormsManager.TopRouteView.ControlRoute.UpdateNodeInformation();

				gbTileData.Enabled =
				gbNodeData.Enabled =
				gbLinkData.Enabled = false;

				// TODO: check if the Og-button should be disabled when a node gets deleted or cut.

				ViewerFormsManager.RouteView   .Control     .Refresh();
				ViewerFormsManager.TopRouteView.ControlRoute.Refresh();
			}			
			else if (!_asterisk)
				ShowDialogAsterisk("A node must be selected.");
		}

		private void ShowDialogAsterisk(string asterisk)
		{
			MessageBox.Show(
						this,
						asterisk,
						"Err..",
						MessageBoxButtons.OK,
						MessageBoxIcon.Asterisk,
						MessageBoxDefaultButton.Button1,
						0);
		}
		#endregion


		/// <summary>
		/// Deselects any currently selected node.
		/// </summary>
		private void DeselectNode()
		{
			NodeSelected = null;
			RoutePanel.SelectedPosition = new Point(-1, -1);

			tsmiClearLinkData.Enabled = false;
		}

		/// <summary>
		/// Handles keyboard input.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control)
			{
				switch (e.KeyCode)
				{
					case Keys.S:
						XCMainWindow.Instance.OnSaveRoutesClick(null, EventArgs.Empty);
						break;

					case Keys.X:
						_asterisk = true;
						OnCopyClick(null, null);
						OnDeleteClick(null, null);
						_asterisk = false;
						break;

					case Keys.C:
						OnCopyClick(null, null);
						break;

					case Keys.V:
						OnPasteClick(null, null);
						break;
				}
			}
			else
			{
				switch (e.KeyCode)
				{
					case Keys.Delete:
						OnDeleteClick(null, null);
						break;
				}
			}
		}


		#region Eventcalls (menubar)
		/// <summary>
		/// Handler for closing the ConnectType combobox.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnConnectTypeClosed(object sender, EventArgs e)
		{
			if (Tag as String == "ROUTE")
				ViewerFormsManager.TopRouteView.ControlRoute.tscbConnectType.SelectedIndex = tscbConnectType.SelectedIndex;
			else
				ViewerFormsManager.RouteView.Control.tscbConnectType.SelectedIndex = tscbConnectType.SelectedIndex;

			_pnlRoutes.Select();	// take focus off the stupid combobox. Tks. NOTE: It tends to
		}							// stay "highlighted" but at least it's no longer "selected".


		private void OnExportClick(object sender, EventArgs e)
		{
			if (MapFile != null)
			{
				using (var sfd = new SaveFileDialog())
				{
					sfd.Title = "Save Route file as ...";
					sfd.DefaultExt = GlobalsXC.RouteExt;
					sfd.FileName = MapFile.Descriptor.Label + GlobalsXC.RouteExt;
					sfd.Filter = "Route files (*.RMP)|*.RMP|All files (*.*)|*.*";
					sfd.InitialDirectory = Path.Combine(MapFile.Descriptor.Basepath, GlobalsXC.RoutesDir);

					if (sfd.ShowDialog() == DialogResult.OK)
					{
						MapFile.Routes.SaveRoutesExport(sfd.FileName);
					}
				}
			}
		}

		private void OnImportClick(object sender, EventArgs e)
		{
			if (MapFile != null)
			{
				using (var ofd = new OpenFileDialog())
				{
					ofd.Title = "Open a Route file ...";
					ofd.DefaultExt = GlobalsXC.RouteExt;
					ofd.FileName = MapFile.Descriptor.Label + GlobalsXC.RouteExt;
					ofd.Filter = "Route files (*.RMP)|*.RMP|All files (*.*)|*.*";
					ofd.InitialDirectory = Path.Combine(MapFile.Descriptor.Basepath, GlobalsXC.RoutesDir);

					if (ofd.ShowDialog() == DialogResult.OK)
					{
						RoutesChanged = true;

						DeselectNode();

						MapFile.ClearRouteNodes();
						MapFile.Routes = new RouteNodeCollection(ofd.FileName);
						MapFile.SetupRouteNodes();

						UpdateNodeInformation(); // not sure is necessary ...
						_pnlRoutes.Refresh();

						RouteCheckService.CheckNodeBounds(MapFile);
					}
				}
			}
		}


		private void OnEditOpening(object sender, EventArgs e)
		{
			tsmi_LowerNode.Enabled = (NodeSelected != null && NodeSelected.Lev != MapFile.MapSize.Levs - 1);
			tsmi_RaiseNode.Enabled = (NodeSelected != null && NodeSelected.Lev != 0);
		}

		private void OnRaiseNode(object sender, EventArgs e)
		{
			_nodeMoved = NodeSelected;

			var args = new RoutePanelEventArgs();
			args.MouseButton = MouseButtons.None;
			args.Tile        = MapFile        [_nodeMoved.Row, _nodeMoved.Col, _nodeMoved.Lev - 1];
			args.Location    = new MapLocation(_nodeMoved.Row, _nodeMoved.Col, _nodeMoved.Lev - 1);

			OnRoutePanelMouseUp(null, args);

			SelectNode(NodeSelected.Index);
		}

		private void OnLowerNode(object sender, EventArgs e)
		{
			_nodeMoved = NodeSelected;

			var args = new RoutePanelEventArgs();
			args.MouseButton = MouseButtons.None;
			args.Tile        = MapFile        [_nodeMoved.Row, _nodeMoved.Col, _nodeMoved.Lev + 1];
			args.Location    = new MapLocation(_nodeMoved.Row, _nodeMoved.Col, _nodeMoved.Lev + 1);

			OnRoutePanelMouseUp(null, args);

			SelectNode(NodeSelected.Index);
		}

		/// <summary>
		/// Handler for menuitem that sets all NodeRanks to Civ/Scout.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnAllNodeRank0Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(
							this,
							"Are you sure you want to make all nodes spawn Rank"
								+ " 0 Civ/Scout?",
							"Warning",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Exclamation,
							MessageBoxDefaultButton.Button2,
							0) == DialogResult.Yes)
			{
				int changed = 0;
				foreach (RouteNode node in MapFile.Routes)
					if (node.Rank != 0)
					{
						++changed;
						node.Rank = 0;
					}

				if (changed != 0)
				{
					RoutesChanged = true;

					ViewerFormsManager.RouteView   .Control     .UpdateNodeInformation();
					ViewerFormsManager.TopRouteView.ControlRoute.UpdateNodeInformation();

					MessageBox.Show(
								changed + " nodes were changed.",
								"All nodes spawn Rank 0",
								MessageBoxButtons.OK,
								MessageBoxIcon.Information,
								MessageBoxDefaultButton.Button1,
								0);
				}
				else
					MessageBox.Show(
								"All nodes are already rank 0.",
								"All nodes spawn Rank 0",
								MessageBoxButtons.OK,
								MessageBoxIcon.Asterisk,
								MessageBoxDefaultButton.Button1,
								0);
			}
		}

		/// <summary>
		/// Handler for menuitem that clears all link-data of the currently
		/// selected node.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnClearLinkDataClick(object sender, EventArgs e)
		{
			if (NodeSelected != null)
			{
				if (MessageBox.Show(
								this,
								"Are you sure you want to clear the selected node's Link data?",
								"Warning",
								MessageBoxButtons.YesNo,
								MessageBoxIcon.Exclamation,
								MessageBoxDefaultButton.Button2,
								0) == DialogResult.Yes)
				{
					RoutesChanged = true;

					for (int slot = 0; slot != RouteNode.LinkSlots; ++slot)
					{
						NodeSelected[slot].Destination = Link.NotUsed;
						NodeSelected[slot].Distance = 0;

						NodeSelected[slot].Type = UnitType.Any;
					}

					ViewerFormsManager.RouteView   .Control     .UpdateNodeInformation();
					ViewerFormsManager.TopRouteView.ControlRoute.UpdateNodeInformation();

					ViewerFormsManager.RouteView   .Control     .Refresh();
					ViewerFormsManager.TopRouteView.ControlRoute.Refresh();
				}
			}
		}

		/// <summary>
		/// Handler for menuitem that updates all link distances in the RMP.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnUpdateAllLinkDistances(object sender, EventArgs e)
		{
			RouteNode node;
			Link link;
			byte dist;
			int changed = 0;

			for (var id = 0; id != MapFile.Routes.Length; ++id)
			{
				node = MapFile.Routes[id];

				for (int slot = 0; slot != RouteNode.LinkSlots; ++slot)
				{
					link = node[slot];
					switch (link.Destination)
					{
						case Link.NotUsed:
						case Link.ExitWest:
						case Link.ExitNorth:
						case Link.ExitEast:
						case Link.ExitSouth:
							if (link.Distance != 0)
							{
								link.Distance = 0;
								++changed;
							}
							break;

						default:
							dist = CalculateLinkDistance(
													node,
													MapFile.Routes[link.Destination]);
							if (link.Distance != dist)
							{
								link.Distance = dist;
								++changed;
							}
							break;
					}
				}
			}

			string info;
			if (changed != 0)
			{
				RoutesChanged = true;
				info = String.Format(
								System.Globalization.CultureInfo.CurrentCulture,
								"{0} link{1} updated.",
								changed,
								(changed == 1) ? " has been" : "s have been");

				ViewerFormsManager.RouteView   .Control     .UpdateNodeInformation();
				ViewerFormsManager.TopRouteView.ControlRoute.UpdateNodeInformation();
			}
			else
			{
				info = String.Format(
								System.Globalization.CultureInfo.CurrentCulture,
								"All link distances are already correct.");
			}

			MessageBox.Show(
						info,
						"Link distances updated",
						MessageBoxButtons.OK,
						MessageBoxIcon.Information,
						MessageBoxDefaultButton.Button1,
						0);
		}

		/// <summary>
		/// Handler for menuitem that checks if any node's rank is beyond the
		/// array of the combobox. See also RouteNodeCollection.cTor
		/// TODO: Consolidate these checks to RouteCheckService.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCheckNodeRanksClick(object sender, EventArgs e)
		{
			var invalids = new List<byte>();
			foreach (RouteNode node in MapFile.Routes)
			{
				if (node.OobRank != (byte)0)
					invalids.Add(node.Index);
			}

			string info, title;
			MessageBoxIcon icon;

			if (invalids.Count != 0)
			{
				icon  = MessageBoxIcon.Warning;
				title = "Warning";
				info  = String.Format(
									System.Globalization.CultureInfo.CurrentCulture,
									"The following route-{0} an invalid NodeRank.{1}",
									(invalids.Count == 1) ? "node has"
														  : "nodes have",
									Environment.NewLine);

				foreach (byte id in invalids)
					info += Environment.NewLine + id;
			}
			else
			{
				icon  = MessageBoxIcon.Information;
				title = "Good stuff, Magister Ludi";
				info  = "There are no invalid NodeRanks detected.";
			}

			MessageBox.Show(
						info,
						title,
						MessageBoxButtons.OK,
						icon,
						MessageBoxDefaultButton.Button1,
						0);
		}

		/// <summary>
		/// Handler for menuitem that checks if any node's location is outside
		/// the dimensions of the Map.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnCheckOobNodesClick(object sender, EventArgs e)
		{
			if (RouteCheckService.CheckNodeBoundsMenuitem(MapFile))
			{
				RoutesChanged = true;

				ViewerFormsManager.RouteView   .Control     .UpdateNodeInformation();
				ViewerFormsManager.TopRouteView.ControlRoute.UpdateNodeInformation();
			}
		}
		#endregion


		#region Options
		private static Form _foptions; // static to be used by both RouteViewOptions
		private static bool _closing;  // and TopRouteView(Route)Options

		private void OnOptionsClick(object sender, EventArgs e)
		{
			var it = sender as ToolStripMenuItem;
			if (!it.Checked)
			{
				ViewerFormsManager.RouteView   .Control     .tsmiOptions.Checked =
				ViewerFormsManager.TopRouteView.ControlRoute.tsmiOptions.Checked = true;

				_foptions = new OptionsForm("RouteViewOptions", Options);
				_foptions.Text = "RouteView Options";

				_foptions.Show();

				_foptions.FormClosing += (sender1, e1) => // a note describing why this is here could be helpful ...
										{
											if (!_closing)
												OnOptionsClick(sender, e);

											_closing = false;
										};
			}
			else
			{
				ViewerFormsManager.RouteView   .Control     .tsmiOptions.Checked =
				ViewerFormsManager.TopRouteView.ControlRoute.tsmiOptions.Checked = false;

				_closing = true;
				_foptions.Close();
			}
		}

		// headers
		private const string Links = "Links";
		private const string View  = "View";
		private const string Nodes = "Nodes";

		// options
		internal const string UnselectedLinkColor = "UnselectedLinkColor";
		private  const string UnselectedLinkWidth = "UnselectedLinkWidth";
		internal const string SelectedLinkColor   = "SelectedLinkColor";
		private  const string SelectedLinkWidth   = "SelectedLinkWidth";

		internal const string WallColor           = "WallColor";
		private  const string WallWidth           = "WallWidth";
		internal const string ContentColor        = "ContentColor";

		internal const string GridLineColor       = "GridLineColor";
		private  const string GridLineWidth       = "GridLineWidth";
		internal const string Grid10LineColor     = "Grid10LineColor";
		private  const string Grid10LineWidth     = "Grid10LineWidth";

		internal const string UnselectedNodeColor = "UnselectedNodeColor";
		internal const string SelectedNodeColor   = "SelectedNodeColor";
		internal const string SpawnNodeColor      = "SpawnNodeColor";
		private  const string NodeOpacity         = "NodeOpacity";

		private  const string ShowOverlay         = "ShowOverlay";
		private  const string ShowPriorityBars    = "ShowPriorityBars";


		/// <summary>
		/// Loads default options for RouteView in TopRouteView screens.
		/// </summary>
		protected internal override void LoadControl0Options()
		{
			tscbConnectType.SelectedIndex = 0;

			var pens    = RoutePanel.RoutePens;
			var brushes = RoutePanel.RouteBrushes;

			var bc = new OptionChangedEventHandler(OnBrushColorChanged);
			var pc = new OptionChangedEventHandler(OnPenColorChanged);
			var pw = new OptionChangedEventHandler(OnPenWidthChanged);
			var oc = new OptionChangedEventHandler(OnNodeOpacityChanged);
			var sp = new OptionChangedEventHandler(OnShowPriorityChanged);
			var so = new OptionChangedEventHandler(OnShowOverlayChanged);

			var pen = new Pen(Color.OrangeRed, 2);
			pens[UnselectedLinkColor] = pen;
			pens[UnselectedLinkWidth] = pen;
			Options.AddOption(
							UnselectedLinkColor,
							pen.Color,
							"Color of unselected link lines",
							Links,
							pc);
			Options.AddOption(
							UnselectedLinkWidth,
							2,
							"Width of unselected link lines",
							Links,
							pw);

			pen = new Pen(Color.RoyalBlue, 2);
			pens[SelectedLinkColor] = pen;
			pens[SelectedLinkWidth] = pen;
			Options.AddOption(
							SelectedLinkColor,
							pen.Color,
							"Color of selected link lines",
							Links,
							pc);
			Options.AddOption(
							SelectedLinkWidth,
							2,
							"Width of selected link lines",
							Links,
							pw);

			pen = new Pen(Color.BurlyWood, 3);
			pens[WallColor] = pen;
			pens[WallWidth] = pen;
			Options.AddOption(
							WallColor,
							pen.Color,
							"Color of wall indicators",
							View,
							pc);
			Options.AddOption(
							WallWidth,
							3,
							"Width of wall indicators",
							View,
							pw);

			var brush = new SolidBrush(Color.DarkGoldenrod);
			brushes[ContentColor] = brush;
			Options.AddOption(
							ContentColor,
							brush.Color,
							"Color of content indicators",
							View,
							bc);

			pen = new Pen(Color.Black, 1);
			pens[GridLineColor] = pen;
			pens[GridLineWidth] = pen;
			Options.AddOption(
							GridLineColor,
							pen.Color,
							"Color of grid lines",
							View,
							pc);
			Options.AddOption(
							GridLineWidth,
							1,
							"Width of grid lines",
							View,
							pw);

			pen = new Pen(Color.Black, 2);
			pens[Grid10LineColor] = pen;
			pens[Grid10LineWidth] = pen;
			Options.AddOption(
							Grid10LineColor,
							pen.Color,
							"Color of every tenth grid line",
							View,
							pc);
			Options.AddOption(
							Grid10LineWidth,
							2,
							"Width of every tenth grid line",
							View,
							pw);

			brush = new SolidBrush(Color.MediumSeaGreen);
			brushes[UnselectedNodeColor] = brush;
			Options.AddOption(
							UnselectedNodeColor,
							brush.Color,
							"Color of unselected nodes",
							Nodes,
							bc);

			brush = new SolidBrush(Color.RoyalBlue);
			brushes[SelectedNodeColor] = brush;
			Options.AddOption(
							SelectedNodeColor,
							brush.Color,
							"Color of selected nodes",
							Nodes,
							bc);

			brush = new SolidBrush(Color.GreenYellow);
			brushes[SpawnNodeColor] = brush;
			Options.AddOption(
							SpawnNodeColor,
							brush.Color,
							"Color of spawn nodes",
							Nodes,
							bc);

			Options.AddOption(
							NodeOpacity,
							255,
							"Opacity of node colors (0..255)",
							Nodes,
							oc);

			Options.AddOption(
							ShowPriorityBars,
							true,
							"True to show patrol-priority and spawn-weight bars",
							Nodes,
							sp);

			Options.AddOption(
							ShowOverlay,
							true,
							"True to show mouse-over information",
							View,
							so);
		}

		private void OnBrushColorChanged(string key, object val)
		{
			var color = (Color)val;
			RoutePanel.RouteBrushes[key].Color = color;

			switch (key)
			{
				case SelectedNodeColor:
					lblSelected.ForeColor = color;
					break;
				case UnselectedNodeColor:
					lblOver.ForeColor = color;
					break;
			}
			Refresh();
		}

		private void OnPenColorChanged(string key, object val)
		{
			RoutePanel.RoutePens[key].Color = (Color)val;
			Refresh();
		}

		private void OnPenWidthChanged(string key, object val)
		{
			RoutePanel.RoutePens[key].Width = (int)val;
			Refresh();
		}

		private void OnNodeOpacityChanged(string key, object val)
		{
			RoutePanel.Opacity = (int)val;
			Refresh();
		}

		private void OnShowPriorityChanged(string key, object val)
		{
			RoutePanel.ShowPriorityBars = (bool)val;
			Refresh();
		}

		private void OnShowOverlayChanged(string key, object val)
		{
			RoutePanel.ShowOverlay = (bool)val;
			Refresh();
		}
		#endregion


		#region Methods (for Help colors)
		/// <summary>
		/// Gets the wall-color for use by the Help screen.
		/// </summary>
		/// <returns></returns>
		internal Dictionary<string, Pen> GetWallPens()
		{
			return RoutePanel.RoutePens;
		}

		/// <summary>
		/// Gets the content-color for use by the Help screen.
		/// </summary>
		/// <returns></returns>
		internal Dictionary<string, SolidBrush> GetContentBrushes()
		{
			return RoutePanel.RouteBrushes;
		}
		#endregion
	}
}
