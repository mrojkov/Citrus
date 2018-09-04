namespace Tangerine
{
	public class AlignPreferences
	{

		public AlignTo AlignTo { get; set; } = AlignTo.Selection;
		public int Spacing { get; set; } = 5;

		public static AlignPreferences Instance = new AlignPreferences();

	}
}
