using System;
using Lime;

namespace Tangerine.UI
{
	public static class TimelineCommands
	{
		public static readonly ICommand ScrollLeft = new Command(new Shortcut(Key.Left));
		public static readonly ICommand ScrollRight = new Command(new Shortcut(Key.Right));
		public static readonly ICommand FastScrollLeft = new Command(new Shortcut(Modifiers.Alt, Key.Left));
		public static readonly ICommand FastScrollRight = new Command(new Shortcut(Modifiers.Alt, Key.Right));
		public static readonly ICommand ScrollUp = new Command(new Shortcut(Key.Up));
		public static readonly ICommand ScrollDown = new Command(new Shortcut(Key.Down));
		public static readonly ICommand SelectUp = new Command(new Shortcut(Modifiers.Shift, Key.Up));
		public static readonly ICommand SelectDown = new Command(new Shortcut(Modifiers.Shift, Key.Down));
		public static readonly ICommand EnterNode = new Command(new Shortcut(Key.Enter));
		public static readonly ICommand ExitNode = new Command(new Shortcut(Key.BackSpace));
		public static readonly ICommand RenameRow = new Command(new Shortcut(Key.F2));
		public static readonly ICommand DeleteKeyframes = new Command("Delete Selected Keyframes", new Shortcut(Modifiers.Shift, Key.Delete));
		public static readonly ICommand CreateMarkerPlay = new Command("Create Play Marker", new Shortcut(Modifiers.Alt, Key.Number1));
		public static readonly ICommand CreateMarkerStop = new Command("Create Stop Marker", new Shortcut(Modifiers.Alt, Key.Number2));
		public static readonly ICommand CreateMarkerJump = new Command("Create Jump Marker", new Shortcut(Modifiers.Alt, Key.Number3));
		public static readonly ICommand DeleteMarker = new Command("Delete Marker", new Shortcut(Modifiers.Alt, Key.Number4));
		public static readonly ICommand CopyMarkers = new Command("Copy All Markers");
		public static readonly ICommand PasteMarkers = new Command("Paste All Markers");
		public static readonly ICommand DeleteMarkers = new Command("Delete All Markers");
		public static readonly ICommand CutKeyframes = new Command("Cut keyframes", new Shortcut(Modifiers.Alt | Modifiers.Control, Key.X));
		public static readonly ICommand CopyKeyframes = new Command("Copy keyframes", new Shortcut(Modifiers.Alt | Modifiers.Control, Key.C));
		public static readonly ICommand PasteKeyframes = new Command("Paste keyframes", new Shortcut(Modifiers.Alt | Modifiers.Control, Key.V));
		public static readonly ICommand MoveDown = new Command("Move down", new Shortcut(Modifiers.Control, Key.LBracket));
		public static readonly ICommand MoveUp = new Command("Move up", new Shortcut(Modifiers.Control, Key.RBracket));
	}

	public static class InspectorCommands
	{
		public static readonly ICommand InspectRootNodeCommand = new Command("Inspect root node");
	}

	public static class GenericCommands
	{
		public static readonly ICommand NewProject = new Command("New Project", new Shortcut(Modifiers.Command, Key.J));
		public static readonly ICommand NewScene = new Command("New Scene", new Shortcut(Modifiers.Command, Key.N));
		public static readonly ICommand NewTan = new Command("New Tan");
		public static readonly ICommand NewTanWithCustomRoot = new Command("New Tan with Custom Root", new Menu());
		public static readonly ICommand Open = new Command("Open", new Shortcut(Modifiers.Command, Key.O));
		public static readonly ICommand RecentDocuments = new Command("Recent Documents", new Menu());
		public static readonly ICommand Save = new Command("Save", new Shortcut(Modifiers.Command, Key.S));
		public static readonly ICommand SaveAs = new Command("Save As", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.S));
		public static readonly ICommand SaveAll = new Command("Save All");
		public static readonly ICommand Revert = new Command("Revert", new Shortcut(Modifiers.Command, Key.R));
		public static readonly ICommand UpgradeDocumentFormat = new Command("Upgrade Document Format (.tan)");
		public static readonly ICommand OpenProject = new Command("Open Project...", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.O));
		public static readonly ICommand RecentProjects = new Command("Recent Projects", new Menu());
		public static readonly ICommand PreferencesDialog = new Command("Preferences...", new Shortcut(Modifiers.Command, Key.P));
		public static readonly ICommand Close = new Command("Close");
		public static readonly ICommand CloseAll = new Command("Close All");
		public static readonly ICommand CloseAllButCurrent = new Command("Close All But Current");
		public static readonly ICommand Group = new Command("Group", new Shortcut(Modifiers.Command, Key.G));
		public static readonly ICommand Ungroup = new Command("Ungroup", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.G));
		public static readonly ICommand InsertTimelineColumn = new Command("Insert Timeline Column", new Shortcut(Modifiers.Command, Key.Q));
		public static readonly ICommand RemoveTimelineColumn = new Command("Remove Timeline Column", new Shortcut(Modifiers.Command, Key.W));
		public static readonly ICommand NextDocument = new Command("Next Document", new Shortcut(Modifiers.Control, Key.Tab));
		public static readonly ICommand PreviousDocument = new Command("Previous Document", new Shortcut(Modifiers.Control | Modifiers.Shift, Key.Tab));
		public static readonly ICommand CloseProject = new Command("Close Project", new Shortcut(Modifiers.Alt, Key.Q));
#if MAC
		public static readonly ICommand Quit = new Command("Quit", new Shortcut(Modifiers.Command, Key.Q));
#else
		public static readonly ICommand Quit = new Command("Quit", new Shortcut(Modifiers.Alt, Key.F4));
#endif
		public static readonly ICommand DefaultLayout = new Command("Default Layout");
		public static readonly ICommand GroupContentsToMorphableMeshes = new Command("Group Contents To Morphable Meshes", new Shortcut(Modifiers.Command | Modifiers.Control, Key.M));
		public static readonly ICommand ExportScene = new Command("Export Scene...");
		public static readonly ICommand UpsampleAnimationTwice = new Command("Upsample Animation Twice");
		public static readonly ICommand Overlays;
		public static readonly ICommand ViewHelp = new Command("View Help", new Shortcut(Modifiers.Control, Key.F1));
		public static readonly ICommand HelpMode = new Command("Help Mode", Key.F1);
		public static readonly ICommand SaveLayout = new Command("Save layout");
		public static readonly ICommand LoadLayout = new Command("Load layout");
	}

	public static class SceneViewCommands
	{
		public static readonly ICommand PreviewAnimation = new Command(new Shortcut(Key.F5));
		public static readonly ICommand PreviewAnimationWithTriggeringOfMarkers = new Command(new Shortcut(Modifiers.Control, Key.F5));
		public static readonly ICommand ResolutionChanger = new Command("Preview Next Resolution", new Shortcut(Key.F11));
		public static readonly ICommand ResolutionReverceChanger = new Command("Preview Previous Resolution", new Shortcut(Modifiers.Shift, Key.F11));
		public static readonly ICommand ResolutionOrientation = new Command("Preview Next Orientation", new Shortcut(Key.F12));
		public static readonly ICommand DragRight = new Command(new Shortcut(Key.D));
		public static readonly ICommand DragLeft = new Command(new Shortcut(Key.A));
		public static readonly ICommand DragUp = new Command(new Shortcut(Key.W));
		public static readonly ICommand DragDown = new Command(new Shortcut(Key.S));
		public static readonly ICommand DragRightFast = new Command(new Shortcut(Modifiers.Shift, Key.D));
		public static readonly ICommand DragLeftFast = new Command(new Shortcut(Modifiers.Shift, Key.A));
		public static readonly ICommand DragUpFast = new Command(new Shortcut(Modifiers.Shift, Key.W));
		public static readonly ICommand DragDownFast = new Command(new Shortcut(Modifiers.Shift, Key.S));
		public static readonly ICommand DisplayBones = new Command("Display Bones (3D)", new Shortcut(Modifiers.Command, Key.B));
		public static readonly ICommand DisplayPivotsForAllWidgets = new Command("Display Pivots For All Widgets", new Shortcut(Modifiers.Command, Key.M));
		public static readonly ICommand DisplayPivotsForInvisibleWidgets = new Command("Display Pivots For Invisible Widgets", new Shortcut(Modifiers.Command | Modifiers.Alt, Key.M));
		public static readonly ICommand TieWidgetsWithBones = new Command("Tie Widgets With Bones", new Shortcut(Modifiers.Command, Key.T));
		public static readonly ICommand UntieWidgetsFromBones = new Command("Untie Widgets From Bones", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.T));
		public static readonly ICommand Duplicate = new Command("Duplicate", new Shortcut(Modifiers.Command, Key.D));
		public static readonly ICommand ToggleDisplayRuler = new Command("Toggle Dispaly", new Shortcut(Modifiers.Command | Modifiers.Alt, Key.R));
		public static readonly ICommand SaveCurrentRuler = new Command("Save Ruler");
		public static readonly ICommand ClearActiveRuler = new Command("Clear Active Ruler");
		public static readonly ICommand SnapWidgetPivotToRuler = new Command("Snap Widget Pivot To Ruler");
		public static readonly ICommand SnapWidgetBorderToRuler = new Command("Snap Widget To Ruler");
		public static readonly ICommand SnapRulerLinesToWidgets = new Command("Snap Ruler Lines To Widgets");
		public static readonly ICommand ManageRulers = new Command("Manage Rulers");
	}

	public static class Tools
	{
		public static readonly ICommand AlignLeft = new Command("Align Left") { Icon = IconPool.GetTexture("Tools.AlignLeft") };
		public static readonly ICommand AlignRight = new Command("Align Right") { Icon = IconPool.GetTexture("Tools.AlignRight") };
		public static readonly ICommand AlignTop = new Command("Align Top") { Icon = IconPool.GetTexture("Tools.AlignTop") };
		public static readonly ICommand AlignBottom = new Command("Align Bottom") { Icon = IconPool.GetTexture("Tools.AlignBottom") };
		public static readonly ICommand CenterHorizontally = new Command("Center Horizontally") { Icon = IconPool.GetTexture("Tools.CenterH") };
		public static readonly ICommand CenterVertically = new Command("Center Vertically") { Icon = IconPool.GetTexture("Tools.CenterV") };
		public static readonly ICommand AlignCentersHorizontally = new Command("Align Centers Horizontally") { Icon = IconPool.GetTexture("Tools.AlignCentersHorizontally") };
		public static readonly ICommand AlignCentersVertically = new Command("Align Centers Vertically") { Icon = IconPool.GetTexture("Tools.AlignCentersVertically") };
		public static readonly ICommand RestoreOriginalSize = new Command("Restore Original Size") { Icon = IconPool.GetTexture("Tools.RestoreOriginalSize") };
		public static readonly ICommand ResetScale = new Command("Reset Scale") { Icon = IconPool.GetTexture("Tools.SetUnitScale") };
		public static readonly ICommand ResetRotation = new Command("Reset Rotation") { Icon = IconPool.GetTexture("Tools.SetZeroRotation") };
		public static readonly ICommand FlipX = new Command("Flip Horizontally") { Icon = IconPool.GetTexture("Tools.FlipH") };
		public static readonly ICommand FlipY = new Command("Flip Vertically") { Icon = IconPool.GetTexture("Tools.FlipV") };
		public static readonly ICommand FitToContainer = new Command("Fit To Container") { Icon = IconPool.GetTexture("Tools.FitToContainer") };
		public static readonly ICommand FitToContent = new Command("Fit To Content") { Icon = IconPool.GetTexture("Tools.FitToContent") };
	}

	public static class FilesystemCommands
	{
		public static readonly ICommand Left = new Command(Key.Left);
		public static readonly ICommand Right = new Command(Key.Right);
		public static readonly ICommand Up = new Command(Key.Up);
		public static readonly ICommand Down = new Command(Key.Down);
		public static readonly ICommand PageUp = new Command(Key.PageUp);
		public static readonly ICommand PageDown = new Command(Key.PageDown);
		public static readonly ICommand Home = new Command(Key.Home);
		public static readonly ICommand End = new Command(Key.End);
		public static readonly ICommand SelectLeft = new Command(Modifiers.Shift, Key.Left);
		public static readonly ICommand SelectRight = new Command(Modifiers.Shift, Key.Right);
		public static readonly ICommand SelectUp = new Command(Modifiers.Shift, Key.Up);
		public static readonly ICommand SelectDown = new Command(Modifiers.Shift, Key.Down);
		public static readonly ICommand SelectPageUp = new Command(Modifiers.Shift, Key.PageUp);
		public static readonly ICommand SelectPageDown = new Command(Modifiers.Shift, Key.PageDown);
		public static readonly ICommand SelectHome = new Command(Modifiers.Shift, Key.Home);
		public static readonly ICommand SelectEnd = new Command(Modifiers.Shift, Key.End);
		public static readonly ICommand ToggleLeft = new Command(Modifiers.Command, Key.Left);
		public static readonly ICommand ToggleRight = new Command(Modifiers.Command, Key.Right);
		public static readonly ICommand ToggleUp = new Command(Modifiers.Command, Key.Up);
		public static readonly ICommand ToggleDown = new Command(Modifiers.Command, Key.Down);
		public static readonly ICommand TogglePageUp = new Command(Modifiers.Command, Key.PageUp);
		public static readonly ICommand TogglePageDown = new Command(Modifiers.Command, Key.PageDown);
		public static readonly ICommand ToggleHome = new Command(Modifiers.Command, Key.Home);
		public static readonly ICommand ToggleEnd = new Command(Modifiers.Command, Key.End);
		public static readonly ICommand Cancel = new Command(Key.Escape);
		public static readonly ICommand Enter = new Command(Key.Enter);
		public static readonly ICommand EnterSpecial = new Command(Modifiers.Command, Key.Enter);
		public static readonly ICommand GoUp = new Command(Key.BackSpace);
		// Also go up on Alt + Up
		public static readonly ICommand GoUpAlso = new Command(Modifiers.Alt, Key.Up);
		public static readonly ICommand GoBack = new Command(Modifiers.Alt, Key.Left);
		public static readonly ICommand GoForward = new Command(Modifiers.Alt, Key.Right);
		public static readonly ICommand ToggleSelection = new Command(Modifiers.Command, Key.Space);
		public static readonly ICommand NavigateTo = new Command("Navigate to");
#if WIN
		private const string OpenInSystemFileManagerDescription = "Show in Explorer";
#elif MAC
		private const string OpenInSystemFileManagerDescription = "Show in Finder";
#endif
		public static readonly ICommand OpenInSystemFileManager = new Command(OpenInSystemFileManagerDescription);
	}

	public static class OrangeCommands
	{
		public static readonly ICommand Run = new Command("Run", new Shortcut(Key.F9));
		public static readonly ICommand OptionsDialog = new Command("Build options", new Shortcut(Modifiers.Control, Key.F9));
		public static readonly ICommand CookGameAssets = new Command("Cook Game Assets", new Shortcut(Modifiers.Alt, Key.F9));
	}
}
