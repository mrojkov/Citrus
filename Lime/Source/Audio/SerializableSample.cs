using Yuzu;

namespace Lime
{
	/// <summary>
	/// Вспомогательный класс, использующийся для сериализации звуков через ProtoBuf
	/// </summary>
	public class SerializableSample
	{
		public string Path;

		public SerializableSample() {}

		public SerializableSample(string path)
		{
			SerializationPath = path;
		}

		[YuzuMember]
		public string SerializationPath
		{
			get { return Serialization.ShrinkPath(Path); }
			set { Path = Serialization.ExpandPath(value); }
		}

		/// <summary>
		/// Создает звук с заданными параметрами
		/// </summary>
		/// <param name="group">Группа звуковых эффектов для этого звука</param>
		/// <param name="looping">Зацикленное проигрывание</param>
		/// <param name="priority">Если не будет хватать числа свободных аудиоканалов, то каналы с более низким приоритетом будут уступать ресурсы каналу этого звука</param>
		/// <param name="fadeinTime">Время плавного нарастания громкости в момент начала воспроизведения</param>
		/// <param name="paused">Изначально звук будет на паузе</param>
		/// <param name="volume">Громкость. От 0 до 1</param>
		/// <param name="pan">Смещение. 0 - слева, 1 - справа, 0.5 - посередине</param>
		/// <param name="pitch">Высота звука</param>
		public Sound Play(AudioChannelGroup group, bool paused, float fadeinTime = 0, bool looping = false, float priority = 0.5f, float volume = 1, float pan = 0, float pitch = 1)
		{
			return AudioSystem.Play(Path, group, looping, priority, fadeinTime, paused, volume, pan, pitch);
		}
	}
}