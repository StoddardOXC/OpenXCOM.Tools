using System;
using System.Windows.Forms;

using XCom;


namespace MapView
{
	internal sealed partial class MapTreeInputBox
		:
			Form
	{
		/// <summary>
		/// The possible box-types.
		/// </summary>
		internal enum BoxType
		{
			AddGroup,
			AddCategory,
			EditGroup,
			EditCategory
		}


		#region Fields (static)
		private const string NewGroup    = "Add Group";
		private const string NewCategory = "Add Category";
		private const string RenGroup    = "Relabel Group";
		private const string RenCategory = "Relabel Category";
		#endregion


		#region Properties
		private BoxType InputBoxType
		{ get; set; }

		private string GroupLabel
		{ get; set; }

		/// <summary>
		/// Gets/Sets the text in the textbox.
		/// </summary>
		internal string Label
		{
			get { return tbInput.Text; }
			set { tbInput.Text = value; }
		}
		#endregion


		#region cTor
		/// <summary>
		/// cTor. Constructs an inputbox for adding/editing the group- or
		/// category-labels of MainView's map-tree.
		/// </summary>
		/// <param name="infoAbove">info about the add/edit</param>
		/// <param name="infoBelow">more info about the add/edit</param>
		/// <param name="boxType">type of box</param>
		/// <param name="labelGroup">label of the group if applicable</param>
		internal MapTreeInputBox(
				string infoAbove,
				string infoBelow,
				BoxType boxType,
				string labelGroup)
		{
			InitializeComponent();

			lblInfoTop.Text    = infoAbove;
			lblInfoBottom.Text = infoBelow;

			switch (InputBoxType = boxType)
			{
				case BoxType.AddGroup:
					Text = NewGroup;
					lblParent.Text = "@root";
					break;
				case BoxType.EditGroup:
					Text = RenGroup;
					lblParent.Text = "@root";
					break;
				case BoxType.AddCategory:
					Text = NewCategory;
					lblParent.Text = "@" + labelGroup;
					break;
				case BoxType.EditCategory:
					Text = RenCategory;
					lblParent.Text = "@" + labelGroup;
					break;
			}

			GroupLabel = labelGroup;

			tbInput.Select();
		}
		#endregion


		#region Eventcalls
		private void OnAcceptClick(object sender, EventArgs e)
		{
			Label = Label.Trim();

			switch (InputBoxType)
			{
				case BoxType.AddGroup:
				case BoxType.EditGroup:
					if (String.IsNullOrEmpty(Label))
					{
						ShowErrorDialog("A group label has not been specified.");
						tbInput.Select();
					}
					else if (!Label.StartsWith("ufo",  StringComparison.OrdinalIgnoreCase)
						&&   !Label.StartsWith("tftd", StringComparison.OrdinalIgnoreCase))
					{
						ShowErrorDialog("The group label needs to start with UFO or TFTD (case insensitive).");
						tbInput.Select();
					}
					else if (Label.StartsWith("ufo", StringComparison.OrdinalIgnoreCase)
						&& String.IsNullOrEmpty(SharedSpace.Instance.GetShare(SharedSpace.ResourceDirectoryUfo)))
					{
						ShowErrorDialog("UFO has not been configured. If you have UFO resources"
							+ " and want to set the configuration for UFO, run the Configurator"
							+ " from MainView's Edit menu.");
						tbInput.Select();
					}
					else if (Label.StartsWith("tftd", StringComparison.OrdinalIgnoreCase)
						&& String.IsNullOrEmpty(SharedSpace.Instance.GetShare(SharedSpace.ResourceDirectoryTftd)))
					{
						ShowErrorDialog("TFTD has not been configured. If you have TFTD resources"
							+ " and want to set the configuration for TFTD, run the Configurator"
							+ " from MainView's Edit menu.");
						tbInput.Select();
					}
					else
					{
						bool bork = false;

						var groups = ResourceInfo.TileGroupManager.TileGroups;
						foreach (var labelGroup in groups.Keys)
						{
							if (String.Equals(labelGroup, Label, StringComparison.OrdinalIgnoreCase))
							{
								bork = true;
								break;
							}
						}

						if (bork)
						{
							ShowErrorDialog("The group label already exists.");
							tbInput.Select();
						}
						else
							DialogResult = DialogResult.OK;
					}
					break;

				case BoxType.AddCategory:
				case BoxType.EditCategory:
					if (String.IsNullOrEmpty(Label))
					{
						ShowErrorDialog("A category label has not been specified.");
						tbInput.Select();
					}
					else
					{
						bool bork = false;

						var tilegroup = ResourceInfo.TileGroupManager.TileGroups[GroupLabel];
						foreach (var labelCategory in tilegroup.Categories.Keys)
						{
							if (String.Equals(labelCategory, Label, StringComparison.OrdinalIgnoreCase))
							{
								bork = true;
								break;
							}
						}

						if (bork)
						{
							ShowErrorDialog("The category label already exists.");
							tbInput.Select();
						}
						else
							DialogResult = DialogResult.OK;
					}
					break;
			}
		}
		#endregion


		#region Methods
		/// <summary>
		/// Wrapper for MessageBox.Show().
		/// </summary>
		/// <param name="error">the error string to show</param>
		private void ShowErrorDialog(string error)
		{
			MessageBox.Show(
						this,
						error,
						"Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error,
						MessageBoxDefaultButton.Button1,
						0);
		}
		#endregion
	}
}
