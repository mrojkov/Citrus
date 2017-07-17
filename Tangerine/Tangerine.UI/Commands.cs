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
		public static readonly ICommand DeleteKeyframes = new Command("Delete Selected Keyframes", new Shortcut(Modifiers.Shift, Key.Delete));
		public static readonly ICommand CreateMarkerPlay = new Command("Create Play Marker", new Shortcut(Modifiers.Alt, Key.Number1));
		public static readonly ICommand CreateMarkerStop = new Command("Create Stop Marker", new Shortcut(Modifiers.Alt, Key.Number2));
		public static readonly ICommand CreateMarkerJump = new Command("Create Jump Marker", new Shortcut(Modifiers.Alt, Key.Number3));
		public static readonly ICommand DeleteMarker = new Command("Delete Marker", new Shortcut(Modifiers.Alt, Key.Number4));
		public static readonly ICommand CopyMarkers = new Command("Copy Markers");
		public static readonly ICommand PasteMarkers = new Command("Paste Markers");
		public static readonly ICommand DeleteMarkers = new Command("Delete Markers");
	}

	public static class InspectorCommands
	{
		public static readonly ICommand InspectRootNodeCommand = new Command("Inspect root node") { Icon = IconPool.GetTexture("Tools.Root") };
	}

	public static class GenericCommands
	{
		public static readonly ICommand New = new Command("New", new Shortcut(Modifiers.Command, Key.N));
		public static readonly ICommand Open = new Command("Open", new Shortcut(Modifiers.Command, Key.O));
		public static readonly ICommand Save = new Command("Save", new Shortcut(Modifiers.Command, Key.S));
		public static readonly ICommand SaveAs = new Command("Save As", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.S));
		public static readonly ICommand UpgradeDocumentFormat = new Command("Upgrade Document Format (.tan)");
		public static readonly ICommand OpenProject = new Command("Open Project...", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.O));
		public static readonly ICommand PreferencesDialog = new Command("Preferences...", new Shortcut(Modifiers.Command, Key.P));
		public static readonly ICommand CloseDocument = new Command("Close Document", new Shortcut(Modifiers.Command, Key.W));
		public static readonly ICommand Group = new Command("Group", new Shortcut(Modifiers.Command, Key.G));
		public static readonly ICommand Ungroup = new Command("Ungroup", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.G));
		public static readonly ICommand InsertTimelineColumn = new Command("Insert Timeline Column", new Shortcut(Modifiers.Command, Key.E));
		public static readonly ICommand RemoveTimelineColumn = new Command("Remove Timeline Column", new Shortcut(Modifiers.Command | Modifiers.Shift, Key.E));
		public static readonly ICommand NextDocument = new Command("Next Document", new Shortcut(Modifiers.Control, Key.Tab));
		public static readonly ICommand PreviousDocument = new Command("Previous Document", new Shortcut(Modifiers.Control | Modifiers.Shift, Key.Tab));
#if MAC
		public static readonly ICommand Quit = new Command("Quit", new Shortcut(Modifiers.Command, Key.Q));
#else
		public static readonly ICommand Quit = new Command("Quit", new Shortcut(Modifiers.Alt, Key.F4));
#endif
		public static readonly ICommand DefaultLayout = new Command("Default Layout");
		public static readonly ICommand GroupContentsToMorphableMeshes = new Command("Group Contents To Morphable Meshes", new Shortcut(Modifiers.Command | Modifiers.Control, Key.M));
		public static readonly ICommand ExportScene = new Command("Export Scene...");
		public static readonly ICommand UpsampleAnimationTwice = new Command("Upsample Animation Twice");
		public static readonly ICommand Overlays = new Command("Overlays");
	}

	public static class SceneViewCommands
	{
		public static readonly ICommand PreviewAnimation = new Command(new Shortcut(Key.F5));
		public static readonly ICommand DragRight = new Command(new Shortcut(Key.D));
		public static readonly ICommand DragLeft = new Command(new Shortcut(Key.A));
		public static readonly ICommand DragUp = new Command(new Shortcut(Key.W));
		public static readonly ICommand DragDown = new Command(new Shortcut(Key.S));
		public static readonly ICommand DragRightFast = new Command(new Shortcut(Modifiers.Shift, Key.D));
		public static readonly ICommand DragLeftFast = new Command(new Shortcut(Modifiers.Shift, Key.A));
		public static readonly ICommand DragUpFast = new Command(new Shortcut(Modifiers.Shift, Key.W));
		public static readonly ICommand DragDownFast = new Command(new Shortcut(Modifiers.Shift, Key.S));
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

	public static class OrangeCommands
	{
		public static readonly ICommand Run = new Command("Run", new Shortcut(Key.F9));
	}
}
