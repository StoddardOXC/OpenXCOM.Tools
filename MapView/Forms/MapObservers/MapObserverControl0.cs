using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

//using DSShared.Windows;

using MapView.Forms.MainWindow;

using XCom.Interfaces.Base;


namespace MapView
{
	/// <summary>
	/// Inherited by TopView, TileView, RouteView.
	/// </summary>
	internal class MapObserverControl0
		:
			UserControl,
			IMapObserver
	{
		#region IMapObserver requirements
		private readonly Dictionary<string, IMapObserver> _panels = new Dictionary<string, IMapObserver>();
		public Dictionary<string, IMapObserver> Panels
		{
			get { return _panels; }
		}

		private MapFileBase _mapBase;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual MapFileBase MapBase
		{
			get { return _mapBase; }
			set
			{
				_mapBase = value;
				Refresh();
			}
		}

/*		private RegistryInfo _regInfo;
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public RegistryInfo RegistryInfo
		{
			get { return _regInfo; } // NOTE: not used. Only satisfies IMapObserver requirement.
			set
			{
				_regInfo = value;
//				value.RegistryLoadEvent += (sender, e) => OnExtraRegistrySettingsLoad(e);
//				value.RegistrySaveEvent += (sender, e) => OnExtraRegistrySettingsSave(e);
			}
		} */

		/// <summary>
		/// Satisfies IMapObserver.
		/// </summary>
		/// <param name="args"></param>
		public virtual void OnLocationSelectedObserver(LocationSelectedEventArgs args)
		{
			Refresh();
		}

		/// <summary>
		/// Satisfies IMapObserver.
		/// </summary>
		/// <param name="args"></param>
		public virtual void OnLevelChangedObserver(LevelChangedEventArgs args)
		{
			Refresh();
		}
		#endregion


		#region Properties
		internal Options Options
		{ get; set; }
		#endregion


		#region cTor
		/// <summary>
		/// Invoked by TopView, TileView, RouteView.
		/// </summary>
		public MapObserverControl0()
		{
			Options = new Options();
		}
		#endregion


		#region Methods (virtual)
		internal protected virtual void LoadControl0Options()
		{}

//		/// <summary>
//		/// Currently implemented only to load TopView's visible-quadrants menu.
//		/// </summary>
//		/// <param name="e"></param>
//		protected virtual void OnExtraRegistrySettingsLoad(RegistryEventArgs e)
//		{}
//		/// <summary>
//		/// Currently implemented only to save TopView's visible-quadrants menu.
//		/// </summary>
//		/// <param name="e"></param>
//		protected virtual void OnExtraRegistrySettingsSave(RegistryEventArgs e)
//		{}
		#endregion


		#region Eventcalls (override)
		/// <summary>
		/// Scrolls the z-axis for TopView and RouteView.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if      (e.Delta < 0) _mapBase.LevelUp();
			else if (e.Delta > 0) _mapBase.LevelDown();

			ViewerFormsManager.ToolFactory.ToggleDownButtons(_mapBase.Level != _mapBase.MapSize.Levs - 1);
			ViewerFormsManager.ToolFactory.ToggleUpButtons(  _mapBase.Level != 0);
		}
		#endregion
	}
}
