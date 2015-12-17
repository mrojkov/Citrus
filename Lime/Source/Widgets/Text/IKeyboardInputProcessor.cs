namespace Lime
{
	public interface IKeyboardInputProcessor
	{
		bool Visible { get; }
		string Text { get; }
	}
}