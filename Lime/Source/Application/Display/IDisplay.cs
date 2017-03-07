
namespace Lime
{
	/// <summary>
	/// Defines a display device.
	/// </summary>
	public interface IDisplay
	{
		/// <summary>
		/// Gets the size of display on the desktop in virtual pixels.
		/// </summary>
		Vector2 Position { get; }

		/// <summary>
		/// Gets the size of display in virtual pixels.
		/// </summary>
		Vector2 Size { get; }
	}
}
