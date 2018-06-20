using System.Collections.Generic;
using System.Collections.ObjectModel;
using Yuzu;

namespace Tangerine.Core
{
	public class ProjectUserPreferences
	{
		private readonly List<Ruler> defaultRulers = new List<Ruler>();
		public IReadOnlyList<Ruler> DefaultRulers => defaultRulers;

		[YuzuMember]
		public readonly List<string> Documents = new List<string>();

		[YuzuMember]
		public string CurrentDocument;

		[YuzuRequired]
		public bool RulerVisible { get; set; }

		[YuzuRequired]
		public List<string> DisplayedRulers { get; set; } = new List<string>();

		[YuzuRequired]
		public List<string> DisplayedOverlays { get; set; } = new List<string>();

		[YuzuRequired]
		public Ruler ActiveRuler { get; set; } = new Ruler();

		[YuzuRequired]
		public ObservableCollection<Ruler> Rulers { get; } = new ObservableCollection<Ruler>();
		
		[YuzuRequired]
		public readonly List<string> RecentDocuments = new List<string>();

		public const int MaxRecentDocuments = 5;

		public static ProjectUserPreferences Instance => Project.Current.UserPreferences;

		public ProjectUserPreferences()
		{
			RulerVisible = true;
			InitializeDefaultRulers();
		}

		private void InitializeDefaultRulers()
		{
			var lineA = new RulerLine(-384);
			var lineB = new RulerLine(384);
			var ruler = new Ruler { Name = "1152x768 (3:2)", AnchorToRoot = true };
			ruler.Lines.Add(new RulerLine(-576, RulerOrientation.Vertical));
			ruler.Lines.Add(new RulerLine(576, RulerOrientation.Vertical));
			ruler.Lines.Add(lineA);
			ruler.Lines.Add(lineB);
			defaultRulers.Add(ruler);
			ruler = new Ruler { Name = "1024x768 (3:4)", AnchorToRoot = true };
			ruler.Lines.Add(new RulerLine(-512, RulerOrientation.Vertical));
			ruler.Lines.Add(new RulerLine(512, RulerOrientation.Vertical));
			ruler.Lines.Add(lineA);
			ruler.Lines.Add(lineB);
			defaultRulers.Add(ruler);
			ruler = new Ruler { Name = "1366x768 (16:9)", AnchorToRoot = true };
			ruler.Lines.Add(new RulerLine(-683, RulerOrientation.Vertical));
			ruler.Lines.Add(new RulerLine(683, RulerOrientation.Vertical));
			ruler.Lines.Add(lineA);
			ruler.Lines.Add(lineB);
			defaultRulers.Add(ruler);
			ruler = new Ruler { Name = "1579x768", AnchorToRoot = true };
			ruler.Lines.Add(new RulerLine(-790, RulerOrientation.Vertical));
			ruler.Lines.Add(new RulerLine(790, RulerOrientation.Vertical));
			ruler.Lines.Add(lineA);
			ruler.Lines.Add(lineB);
			defaultRulers.Add(ruler);
		}

		public void SaveActiveRuler()
		{
			Rulers.Add(ActiveRuler);
			ActiveRuler = new Ruler();
			UserPreferences.Instance.Save();
		}

	}
}
