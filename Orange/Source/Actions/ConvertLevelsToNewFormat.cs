using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Lime;
using System.Collections;
using System.Globalization;

namespace Orange.Source.Actions
{
	using LevelsConverting;

	static partial class Actions
	{
		[Export(nameof(OrangePlugin.MenuItems))]
		[ExportMetadata("Label", "Convert Levels to the New Format")]
		public static void ConvertLevelsToNewFormat()
		{
			var s = The.Workspace.AssetsDirectory;

			foreach (City city in Enum.GetValues(typeof(City))) {
				try {
					var levels = new LevelsGroup(city);
					levels.OldLoad();
					foreach (var level in levels.Levels) {
						level.IsKBLevel = false;
						level[0].isKBLevel = false;
					}
					if (city == City.Tutorial) {
						File.Delete(Path.Combine(LevelsGroup.GetEditableConfigDir(), levels.baseName) + ".txt");
					} else {
						int numKBs = 0;
						while (File.Exists(Path.Combine(LevelsGroup.GetEditableConfigDir(), LevelsGroup.GetLevelsFile(levels.baseName, 3 + numKBs)))) {
							numKBs++;
						}
						for (int i = 0; i < 3 + numKBs; i++) {
							var filename = Path.Combine(LevelsGroup.GetEditableConfigDir(), LevelsGroup.GetLevelsFile(levels.baseName, i));
							File.Delete(filename);
						}
					}
					levels.Save(GetBranches(city));
					Console.WriteLine($"City {city} succesfully converted to new format");
				} catch (System.Exception ex) {
					Console.WriteLine($"Cant convert {city} City: {ex.Message}");
				}
			}
		}


		public static Dictionary<int, int> GetBranches(City city)
		{
			#region Branches
			var branches = new Dictionary<int, int>();
			switch (city) {
				case City.Tutorial:
					break;
				case City.Italy:
					branches[73 - 1] = 1;
					branches[76 - 1] = 1;
					branches[79 - 1] = 1;
					branches[83 - 1] = 1;
					branches[86 - 1] = 1;
					branches[89 - 1] = 2;
					branches[93 - 1] = 1;
					branches[96 - 1] = 2;
					branches[99 - 1] = 2;
					branches[103 - 1] = 1;
					branches[106 - 1] = 2;
					branches[118 - 1] = 1;
					break;
				case City.France:
					branches[73 - 1] = 1;
					branches[76 - 1] = 1;
					branches[79 - 1] = 1;
					branches[83 - 1] = 1;
					branches[86 - 1] = 1;
					branches[89 - 1] = 2;
					branches[93 - 1] = 1;
					branches[96 - 1] = 2;
					branches[99 - 1] = 2;
					branches[103 - 1] = 1;
					branches[106 - 1] = 2;
					break;
				default:
					throw new NotImplementedException(city.ToString());
			}
			#endregion
			return branches;
		}


	}
}

namespace Orange.Source.Actions.LevelsConverting
{
	public enum City
	{
		Tutorial = 0,
		Italy = 1,
		France = 2,
		Thailand = 3,
		USA = 4,
		Mexico = 5,
		Italy2 = 6,
	}

	public enum MysteryBagSpawn
	{
		// Blockers
		Chain,
		DoubleChain,
		ObstacleLv1,
		ObstacleLv2,
		ObstacleLv3,
		EvilStickerLv2,
		EvilStickerLv3,
		IceCube,
		Desert,
		GalantineLv1,
		GalantineLv2,
		GalantineLv3,
		Trash1,
		Trash2,
		Trash3,

		// Bonuses
		VerLineBonus,
		HorLineBonus,
		AreaBomb,
		Kite,
		Lightning,
	}

	public class MysteryBagSpawnSet
	{
		public float BlockerSpawnWeight;
		public float BonusSpawnWeight;
		public List<Tuple<MysteryBagSpawn, float>> Spawns = new List<Tuple<MysteryBagSpawn, float>>();

		public bool Equals(MysteryBagSpawnSet other)
		{
			return
				BlockerSpawnWeight == other.BlockerSpawnWeight &&
				BonusSpawnWeight == other.BonusSpawnWeight &&
				HasSameSpawns(other) &&
				other.HasSameSpawns(this);
		}

		private bool HasSameSpawns(MysteryBagSpawnSet other)
		{
			foreach (var s1 in Spawns) {
				if (s1.Item2 > 0) {
					var s2 = other.Spawns.Find(s => s.Item1 == s1.Item1);
					if (s2 == null || s2.Item2 != s1.Item2) {
						return false;
					}
				}
			}
			return true;
		}

		public string GetHashString()
		{
			var r = new StringBuilder();
			r.Append(BlockerSpawnWeight);
			r.Append(BonusSpawnWeight);
			foreach (var s in Spawns) {
				r.Append(s.Item1);
				r.Append(s.Item2);
			}
			return r.ToString();
		}
	}

	public static class MysteryBagHelpers
	{
		public static bool IsBonus(this MysteryBagSpawn spawn)
		{
			switch (spawn) {
				case MysteryBagSpawn.VerLineBonus:
				case MysteryBagSpawn.HorLineBonus:
				case MysteryBagSpawn.AreaBomb:
				case MysteryBagSpawn.Kite:
				case MysteryBagSpawn.Lightning:
					return true;
				default:
					return false;
			}
		}

		public static UnlockInfo GetUnlock(this MysteryBagSpawn spawn)
		{
			switch (spawn) {
				case MysteryBagSpawn.Chain:
				case MysteryBagSpawn.DoubleChain:
				case MysteryBagSpawn.ObstacleLv1:
				case MysteryBagSpawn.ObstacleLv2:
				case MysteryBagSpawn.ObstacleLv3:
				case MysteryBagSpawn.EvilStickerLv2:
				case MysteryBagSpawn.EvilStickerLv3:
				case MysteryBagSpawn.IceCube:
				case MysteryBagSpawn.Desert:
				case MysteryBagSpawn.GalantineLv1:
				case MysteryBagSpawn.GalantineLv2:
				case MysteryBagSpawn.GalantineLv3:
				case MysteryBagSpawn.Trash1:
				case MysteryBagSpawn.Trash2:
				case MysteryBagSpawn.Trash3:
				case MysteryBagSpawn.VerLineBonus:
				case MysteryBagSpawn.HorLineBonus:
				case MysteryBagSpawn.AreaBomb:
				case MysteryBagSpawn.Kite:
				case MysteryBagSpawn.Lightning:
					return UnlockInfo.Always;
			}
			throw new Lime.Exception("GetSpawnLevel() is undefined for {0}", spawn);
		}

		public static float MysteryBagBlockerSpawnWeight = 1;
		public static float MysteryBagBonusSpawnWeight = 3;

		public static float MysteryBagChainSpawnWeight = 1;
		public static float MysteryBagDoubleChainSpawnWeight = 1;
		public static float MysteryBagObstacleLv1SpawnWeight = 1;
		public static float MysteryBagObstacleLv2SpawnWeight = 1;
		public static float MysteryBagObstacleLv3SpawnWeight = 1;
		public static float MysteryBagEvilStickerLv2SpawnWeight = 0;   // unused by default
		public static float MysteryBagEvilStickerLv3SpawnWeight = 0;   // unused by default
		public static float MysteryBagIceCubeSpawnWeight = 1;
		public static float MysteryBagDesertSpawnWeight = 0;           // unused by default
		public static float MysteryBagGalantineLv1SpawnWeight = 1;
		public static float MysteryBagGalantineLv2SpawnWeight = 1;
		public static float MysteryBagGalantineLv3SpawnWeight = 1;
		public static float MysteryBagTrash1SpawnWeight = 1;
		public static float MysteryBagTrash2SpawnWeight = 1;
		public static float MysteryBagTrash3SpawnWeight = 1;

		public static float MysteryBagVerLineBonusSpawnWeight = 2;
		public static float MysteryBagHorLineBonusSpawnWeight = 2;
		public static float MysteryBagAreaBombSpawnWeight = 2;
		public static float MysteryBagKiteSpawnWeight = 2;
		public static float MysteryBagLightningSpawnWeight = 2;

		public static float GetDefaultWeight(this MysteryBagSpawn spawn)
		{
			switch (spawn) {
				case MysteryBagSpawn.Chain:
					return MysteryBagChainSpawnWeight;
				case MysteryBagSpawn.DoubleChain:
					return MysteryBagDoubleChainSpawnWeight;
				case MysteryBagSpawn.ObstacleLv1:
					return MysteryBagObstacleLv1SpawnWeight;
				case MysteryBagSpawn.ObstacleLv2:
					return MysteryBagObstacleLv2SpawnWeight;
				case MysteryBagSpawn.ObstacleLv3:
					return MysteryBagObstacleLv3SpawnWeight;
				case MysteryBagSpawn.EvilStickerLv2:
					return MysteryBagEvilStickerLv2SpawnWeight;
				case MysteryBagSpawn.EvilStickerLv3:
					return MysteryBagEvilStickerLv3SpawnWeight;
				case MysteryBagSpawn.IceCube:
					return MysteryBagIceCubeSpawnWeight;
				case MysteryBagSpawn.Desert:
					return MysteryBagDesertSpawnWeight;
				case MysteryBagSpawn.GalantineLv1:
					return MysteryBagGalantineLv1SpawnWeight;
				case MysteryBagSpawn.GalantineLv2:
					return MysteryBagGalantineLv2SpawnWeight;
				case MysteryBagSpawn.GalantineLv3:
					return MysteryBagGalantineLv3SpawnWeight;
				case MysteryBagSpawn.Trash1:
					return MysteryBagTrash1SpawnWeight;
				case MysteryBagSpawn.Trash2:
					return MysteryBagTrash2SpawnWeight;
				case MysteryBagSpawn.Trash3:
					return MysteryBagTrash3SpawnWeight;
				case MysteryBagSpawn.VerLineBonus:
					return MysteryBagVerLineBonusSpawnWeight;
				case MysteryBagSpawn.HorLineBonus:
					return MysteryBagHorLineBonusSpawnWeight;
				case MysteryBagSpawn.AreaBomb:
					return MysteryBagAreaBombSpawnWeight;
				case MysteryBagSpawn.Kite:
					return MysteryBagKiteSpawnWeight;
				case MysteryBagSpawn.Lightning:
					return MysteryBagLightningSpawnWeight;
			}
			throw new Lime.Exception("GetDefaultWeight() is undefined for {0}", spawn);
		}
	}

	public class UnlockInfo
	{
		public static UnlockInfo Always = new UnlockInfo(City.Italy, 0);

		public UnlockInfo(City city, int level)
		{
			City = city;
			Level = level;
		}

		public City City { get; }

		public int Level { get; }

		public bool UnlockedInLevel(City city, int levelIdx /*zero-based*/)
		{
			if (Level == 0 || city > City) {
				return true;
			} else if (city < City) {
				return false;
			} else {
				return levelIdx >= Level;
			}
		}

		public static bool TryParse(string s, out UnlockInfo result)
		{
			result = null;
			var split = s.Split(',');
			if (split.Length != 2) {
				return false;
			}
			City city;
			if (!Enum.TryParse(split[0], true, out city)) {
				return false;
			}
			string levelStr = split[1].Trim();
			if (!levelStr.StartsWith("level ")) {
				return false;
			}
			int level;
			if (!int.TryParse(levelStr.Substring(6), out level)) {
				return false;
			}
			result = new UnlockInfo(city, level);
			return true;
		}
	}

	public class LevelInfo
	{
		/// <summary>
		/// По этой структуре генерится хеш уровня, который используется как уникальный идентификатор,
		/// по которому можно сравнивать уровни
		/// </summary>
		private struct HashStruct
		{
			private string board;
			private string items;
			private int scienceGoal;
			private int engineeringGoal;
			private int aestheticsGoal;
			private int scoreGoal;
			private int turnLimit;
			private int timeLimit;
			private int trashPeriod;
			private int mysteryBagPeriod;
			private int tornados;
			private int stones;
			private int stoneInterval;
			private int blockerBombs;
			private int blockerBombInterval;
			private int blockerBombCapacity;
			private int difficulty;
			private readonly int obstacleGoal;
			private readonly int waffleGoal;
			private readonly int trashGoal;
			private readonly int lineBonusGoal;
			private readonly int bombBonusGoal;
			private readonly int kiteBonusGoal;
			private readonly int lightningBonusGoal;
			private readonly int movesLimit;
			private readonly int extraBoards;
			private readonly int marbleKindGoal;
			private readonly int marbleCountGoal;
			private readonly int directorStrategy;
			private readonly string mysteryBagSpawns;

			public HashStruct(LevelInfo levelInfo)
			{
				// через Reflection тут нельзя

				board = levelInfo.board;
				items = levelInfo.items;
				scienceGoal = levelInfo.scienceGoal;
				engineeringGoal = levelInfo.engineeringGoal;
				aestheticsGoal = levelInfo.aestheticsGoal;
				scoreGoal = levelInfo.scoreGoal;
				turnLimit = levelInfo.turnLimit;
				timeLimit = levelInfo.timeLimit;
				trashPeriod = levelInfo.trashPeriod;
				mysteryBagPeriod = levelInfo.mysteryBagPeriod;
				tornados = levelInfo.tornados;
				stones = levelInfo.stones;
				stoneInterval = levelInfo.stones > 0 ? levelInfo.stoneInterval : 20;
				blockerBombs = levelInfo.blockerBombs;
				blockerBombInterval = levelInfo.blockerBombInterval;
				blockerBombCapacity = levelInfo.blockerBombCapacity;
				difficulty = levelInfo.difficulty;
				obstacleGoal = levelInfo.ObstacleGoal;
				waffleGoal = levelInfo.WaffleGoal;
				trashGoal = levelInfo.TrashGoal;
				lineBonusGoal = levelInfo.LineBonusGoal;
				bombBonusGoal = levelInfo.BombBonusGoal;
				kiteBonusGoal = levelInfo.KiteBonusGoal;
				lightningBonusGoal = levelInfo.LightningBonusGoal;
				movesLimit = levelInfo.MovesLimit;
				extraBoards = levelInfo.extraBoards;
				marbleKindGoal = levelInfo.MarbleKindGoal;
				marbleCountGoal = levelInfo.MarbleCountGoal;
				directorStrategy = (int)levelInfo.directorStrategy;
				mysteryBagSpawns = levelInfo.mysteryBagSpawns?.GetHashString();
			}

		}

		public string board;
		public string items = string.Empty;
		public int scienceGoal;
		public int engineeringGoal;
		public int aestheticsGoal;
		public int scoreGoal;
		public int turnLimit;
		public int timeLimit;
		public int trashPeriod;
		public int mysteryBagPeriod;
		public int tornados;
		public int stones;
		public int stoneInterval;
		public int blockerBombs;
		public int blockerBombInterval;
		public int blockerBombCapacity;
		public int difficulty = -1;
		public string extraAnimation;
		public string goalTextOverride;
		public string tutorials1; // Параметр "Tutorials" не поддерживал обратную совместимость конфигов, т.к. валил игру при указании отсутствующего в коде туториала.
		public string tutorials2; // Этот недостаток был исправлен, но отныне новые туториалы надо задавать параметром "Tutorials (safe)", который игнорируется старыми версиями игры, не имеющими исправления.
		public string successMessage;
		public bool isKeyTier; // после этого tier-а мы получаем человечка/линзу/и так далее (раньше всегда было только после третьего)
		public bool cellBreakAnimationOnLevelGoals;
		public bool noMusic;
		public int winCount;
		public int failCount;
		public int ObstacleGoal { get; set; }
		public int WaffleGoal { get; set; }
		public int TrashGoal { get; set; }
		public int LineBonusGoal { get; set; }
		public int BombBonusGoal { get; set; }
		public int KiteBonusGoal { get; set; }
		public int LightningBonusGoal { get; set; }
		public int MarbleKindGoal { get; set; }
		public int MarbleCountGoal { get; set; }
		public string Cornerstone { get; set; }
		public int MovesLimit;
		public int extraBoards; // количество дополнительных досок внутри уровня (KB levels)
		public int directorStrategy;
		public MysteryBagSpawnSet mysteryBagSpawns;

		/// <summary>
		/// Этот ключ задается при создании уровня в редакторе и больше не меняется
		/// (даже если уровень был отредактирван). Используется только для сбора статистики
		/// </summary>
		public virtual Guid StatisticsKey { get; set; }

		// не сохраняется
		public int scoreMultiplier = 1;

		public List<LevelInfo> branch = null; // разворачивается функцией ExpandBranches
		public bool isKBLevel = false;

		// не сохраняется
		int? hash = null;

		public LevelInfo()
		{
			StatisticsKey = Guid.Empty;
		}

		public LevelInfo(Guid statisticsKey)
		{
			StatisticsKey = statisticsKey;
		}

		public bool HasTechResource { get { return items.IndexOf('1') >= 0; } }
		public bool HasArtResource { get { return items.IndexOf('2') >= 0; } }
		public bool HasScienceResource { get { return items.IndexOf('3') >= 0; } }

		public int NumResourceMarbles
		{
			get
			{
				int result = 0;
				if (HasTechResource)
					result++;
				if (HasArtResource)
					result++;
				if (HasScienceResource)
					result++;
				return result;
			}
		}

		public int NumNonResourceMarbles
		{
			get
			{
				var marbleTypes = new HashSet<char>();
				foreach (var r in items) {
					if (r != '1' && r != '2' && r != '3') {
						marbleTypes.Add(r);
					}
				}
				return marbleTypes.Count;
			}
		}

		/// <summary>
		/// Хеш, по которому можно сравнить два уровня на уникальность.
		/// Если значения полей изменились, это свойство нужно обновить, вызвав UpdateHash()
		/// </summary>
		public int Hash
		{
			get
			{
				if (hash == null) {
					UpdateHash();
				}

				return hash.Value;
			}
			private set { hash = value; }
		}

		/// <summary>
		/// Файл, из которого загружен уровень. Если null, то уровень загружен не из файла,
		/// а из игры (т.е. этот уровень уже в игре)
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Обновить Id. Этот метод нужно вызывать, когда значения полей изменились
		/// </summary>
		public void UpdateHash()
		{
			Hash = new HashStruct(this).GetHashCode();
		}

		public virtual void OnSave() { }

		public virtual void OnLoad() { }

		public static bool ReadIntAttribute(string line, string name, ref int value)
		{
			if (line.Length < (name.Length + 1))
				return false;
			if (line.Substring(0, name.Length) != name)
				return false;
			return int.TryParse(line.Substring(name.Length).Trim(), out value);
		}

		public static bool ReadIntAttribute(string line, string name, Action<int> propertySetter)
		{
			int tmp = 0;
			if (ReadIntAttribute(line, name, ref tmp)) {
				propertySetter(tmp);
				return true;
			}
			return false;
		}

		public static bool ReadStringAttribute(string line, string name, ref string value)
		{
			if (line.Length < (name.Length + 1))
				return false;
			if (line.Substring(0, name.Length) != name)
				return false;
			value = line.Substring(name.Length).Trim();
			return true;
		}

		public static bool ReadStringAttribute(string line, string name, Action<string> propertySetter)
		{
			string tmp = null;
			if (ReadStringAttribute(line, name, ref tmp)) {
				propertySetter(tmp);
				return true;
			}
			return false;
		}

		static bool ReadStatisticsKeyAttribute(string line, ref Guid value)
		{
#if !UNITY
			// GUID - старое название для Statistics key
			var names = new string[2] { "Statistics key:", "GUID:" };
			foreach (var name in names) {
				if (line.Contains(name)) {
					return Guid.TryParse(line.Replace(name, "").Trim(), out value);
				}
			}
#endif
			return false;
		}

		// добавлено для совместимости с Android и iPhone, где нет редактора
		static public IEnumerable<IntVector2> EnumerateLevelsInTree(Dictionary<int, int> branches)
		{
			for (int i = 0; true; i++) {
				yield return new IntVector2(i, -1);
				int branchLength;
				if (branches.TryGetValue(i, out branchLength)) {
					for (int j = 0; j < branchLength; j++) {
						yield return new IntVector2(i, j);
					}
				}
			}
		}

		public static bool ReadTimeAttribute(string line, string name, ref int valueSeconds)
		{
			if (line.Length < (name.Length + 1))
				return false;
			if (line.Substring(0, name.Length) != name)
				return false;
			return TryParseTime(line.Substring(name.Length).Trim(), out valueSeconds);
		}

		public static bool TryParseTime(string s, out int result)
		{
			bool success = false;
			result = 0;
			string[] pars = s.Split(' ');
			foreach (string par in pars) {
				string p = par.Trim();
				if (p.Length >= 2) {
					char ch = p[p.Length - 1];
					if (ch == 'd') {
						int pVal;
						if (!int.TryParse(p.Substring(0, p.Length - 1), out pVal)) {
							return false;
						}
						result += pVal * 24 * 60 * 60;
						success = true;
					} else if (ch == 'h') {
						int pVal;
						if (!int.TryParse(p.Substring(0, p.Length - 1), out pVal)) {
							return false;
						}
						result += pVal * 60 * 60;
						success = true;
					} else if (ch == 'm') {
						int pVal;
						if (!int.TryParse(p.Substring(0, p.Length - 1), out pVal)) {
							return false;
						}
						result += pVal * 60;
						success = true;
					} else if (ch == 's') {
						int pVal;
						if (!int.TryParse(p.Substring(0, p.Length - 1), out pVal)) {
							return false;
						}
						result += pVal;
						success = true;
					}
				}
			}
			return success;
		}

		public static bool ReadEnumAttribute<T>(string line, string name, ref T value) where T : struct
		{
			if (line.Length < (name.Length + 1))
				return false;
			if (line.Substring(0, name.Length) != name)
				return false;
			return Enum.TryParse<T>(line.Substring(name.Length).Trim(), out value);
		}

		public static bool ReadFloatAttribute(string line, string name, ref float value)
		{
			if (line.Length < (name.Length + 1))
				return false;
			if (line.Substring(0, name.Length) != name)
				return false;
			return float.TryParse(line.Substring(name.Length).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}

		public static bool ReadBoolAttribute(string line, string name, ref bool value)
		{
			if (line == name) {
				value = true;
				return true;
			}
			return false;
		}

		public static LevelInfo[] OldLoadLevels(string str)
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream)) {
				writer.Write(str);
				writer.Flush();

				stream.Position = 0;
				return LevelInfo.OldLoadLevels(stream).ToArray();
			}
		}

		public static IEnumerable<LevelInfo> OldLoadLevels(Stream stream)
		{
			//List<LevelInfo> levels = new List<LevelInfo>();
			using (var r = new StreamReader(stream)) {
				LevelInfo info = null;
				Guid statisticsKey = Guid.Empty;
				while (true) {
					string line = r.ReadLine();
					if (line == null) {
						break;
					} else if (line.Length > 5 && line.Substring(0, 5) == "Level") {
						if (info != null) {
							//levels.Add(info);
							yield return info;
						}
#if UNITY || !ALLOW_XML
						info = new LevelInfo();
#else
						info = new MetadataLevelInfo();
#endif
					} else if (info != null) {
						if (line == "Board:") {
							var sb = new StringBuilder();
							using (var w = new StringWriter(sb)) {
								line = r.ReadLine();
								while (!string.IsNullOrEmpty(line)) {
									w.WriteLine(line);
									line = r.ReadLine();
								}
							}
							info.board = sb.ToString();
						} else if (line == "Mystery Bag spawns: [") {
							info.mysteryBagSpawns = new MysteryBagSpawnSet();
							line = r.ReadLine();
							while (line != null && line != "]") {
								line = line.Trim();
								if (ReadFloatAttribute(line, "Blocker:", ref info.mysteryBagSpawns.BlockerSpawnWeight)) {
								} else if (ReadFloatAttribute(line, "Bonus:", ref info.mysteryBagSpawns.BonusSpawnWeight)) {
								} else {
									var split = line.Split(':');
									if (split.Length == 2) {
										MysteryBagSpawn key;
										float value;
										if (Enum.TryParse(split[0], out key) && float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
											info.mysteryBagSpawns.Spawns.Add(Tuple.Create(key, value));
										}
									}
								}
								line = r.ReadLine();
							}
						} else if (ReadStringAttribute(line, "Items:", ref info.items)) {
						} else if (ReadIntAttribute(line, "Science goal:", ref info.scienceGoal)) {
						} else if (ReadIntAttribute(line, "Engineering goal:", ref info.engineeringGoal)) {
						} else if (ReadIntAttribute(line, "Aesthetics goal:", ref info.aestheticsGoal)) {
						} else if (ReadIntAttribute(line, "Score goal:", ref info.scoreGoal)) {
						} else if (ReadIntAttribute(line, "Turn limit:", ref info.turnLimit)) {
						} else if (ReadIntAttribute(line, "Time limit:", ref info.timeLimit)) {
						} else if (ReadIntAttribute(line, "Barrel period:", ref info.trashPeriod)) {
						} else if (ReadIntAttribute(line, "Mystery Bag period:", ref info.mysteryBagPeriod)) {
						} else if (ReadIntAttribute(line, "Tornados:", ref info.tornados)) {
						} else if (ReadIntAttribute(line, "Stones:", ref info.stones)) {
						} else if (ReadIntAttribute(line, "Stone interval:", ref info.stoneInterval)) {
						} else if (ReadIntAttribute(line, "Blocker bombs:", ref info.blockerBombs)) {
						} else if (ReadIntAttribute(line, "Blocker bomb interval:", ref info.blockerBombInterval)) {
						} else if (ReadIntAttribute(line, "Blocker bomb capacity:", ref info.blockerBombCapacity)) {
						} else if (ReadIntAttribute(line, "Difficulty:", ref info.difficulty)) {
						} else if (ReadStringAttribute(line, "Tutorials:", ref info.tutorials1)) {
						} else if (ReadStringAttribute(line, "Tutorials (safe):", ref info.tutorials2)) {
						} else if (ReadStringAttribute(line, "Success message:", ref info.successMessage)) {
						} else if (ReadStringAttribute(line, "Extra animation:", ref info.extraAnimation)) {
						} else if (ReadStringAttribute(line, "Goal text:", ref info.goalTextOverride)) {
						} else if (ReadStringAttribute(line, Labels.Cornerstone, (v) => info.Cornerstone = v)) {
						} else if (ReadStatisticsKeyAttribute(line, ref statisticsKey)) {
							info.StatisticsKey = statisticsKey;
						} else if (line == "Key Tier") {
							info.isKeyTier = true;
						} else if (line == "Cell break tutorial") {
							info.cellBreakAnimationOnLevelGoals = true;
						} else if (line == "No music") {
							info.noMusic = true;
						} else if (ReadIntAttribute(line, "WinCount:", ref info.winCount)) {
						} else if (ReadIntAttribute(line, "FailCount:", ref info.failCount)) {
						} else if (ReadIntAttribute(line, Labels.ObstacleGoal, (v) => info.ObstacleGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.WaffleGoal, (v) => info.WaffleGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.TrashGoal, (v) => info.TrashGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.LineBonusGoal, (v) => info.LineBonusGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.BombBonusGoal, (v) => info.BombBonusGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.KiteBonusGoal, (v) => info.KiteBonusGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.LightningBonusGoal, (v) => info.LightningBonusGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.MovesLimit, (v) => info.MovesLimit = v)) {
						} else if (ReadIntAttribute(line, Labels.MarbleKindGoal, (v) => info.MarbleKindGoal = v)) {
						} else if (ReadIntAttribute(line, Labels.MarbleCountGoal, (v) => info.MarbleCountGoal = v)) {
						} else if (ReadIntAttribute(line, "Extra boards:", ref info.extraBoards)) {
						} else if (ReadIntAttribute(line, "Director strategy:", ref info.directorStrategy)) {
						}
					}
				}
				if (info != null) {
					//levels.Add(info);
					info.OnLoad();
					yield return info;
				}
			}
			//return levels;
		}

		public static LevelInfo LoadLevel(string str)
		{
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream)) {
				writer.Write(str);
				writer.Flush();

				stream.Position = 0;
				return LoadLevel(stream);
			}
		}

		public static LevelInfo LoadLevel(Stream stream)
		{
			using (var reader = new StreamReader(stream)) {
				var level = new LevelInfo();
				level.LoadLevel(reader);
				return level;
			}
		}

		public void LoadLevel(StreamReader r)
		{
			Guid statisticsKey = Guid.Empty;
			while (true) {
				string line = r.ReadLine();
				if (line == "Board:") {
					var sb = new StringBuilder();
					using (var w = new StringWriter(sb)) {
						line = r.ReadLine();
						while (!string.IsNullOrEmpty(line)) {
							w.WriteLine(line);
							line = r.ReadLine();
						}
					}
					board = sb.ToString();
					return;
				} else if (line == "Mystery Bag spawns: [") {
					mysteryBagSpawns = new MysteryBagSpawnSet();
					line = r.ReadLine();
					while (line != null && line != "]") {
						line = line.Trim();
						if (ReadFloatAttribute(line, "Blocker:", ref mysteryBagSpawns.BlockerSpawnWeight)) {
						} else if (ReadFloatAttribute(line, "Bonus:", ref mysteryBagSpawns.BonusSpawnWeight)) {
						} else {
							var split = line.Split(':');
							if (split.Length == 2) {
								MysteryBagSpawn key;
								float value;
								if (Enum.TryParse(split[0], out key) && float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
									mysteryBagSpawns.Spawns.Add(Tuple.Create(key, value));
								}
							}
						}
						line = r.ReadLine();
					}
				} else if (ReadStringAttribute(line, "Items:", ref items)) {
				} else if (ReadIntAttribute(line, "Science goal:", ref scienceGoal)) {
				} else if (ReadIntAttribute(line, "Engineering goal:", ref engineeringGoal)) {
				} else if (ReadIntAttribute(line, "Aesthetics goal:", ref aestheticsGoal)) {
				} else if (ReadIntAttribute(line, "Score goal:", ref scoreGoal)) {
				} else if (ReadIntAttribute(line, "Turn limit:", ref turnLimit)) {
				} else if (ReadIntAttribute(line, "Time limit:", ref timeLimit)) {
				} else if (ReadIntAttribute(line, "Barrel period:", ref trashPeriod)) {
				} else if (ReadIntAttribute(line, "Mystery Bag period:", ref mysteryBagPeriod)) {
				} else if (ReadIntAttribute(line, "Tornados:", ref tornados)) {
				} else if (ReadIntAttribute(line, "Stones:", ref stones)) {
				} else if (ReadIntAttribute(line, "Stone interval:", ref stoneInterval)) {
				} else if (ReadIntAttribute(line, "Blocker bombs:", ref blockerBombs)) {
				} else if (ReadIntAttribute(line, "Blocker bomb interval:", ref blockerBombInterval)) {
				} else if (ReadIntAttribute(line, "Blocker bomb capacity:", ref blockerBombCapacity)) {
				} else if (ReadIntAttribute(line, "Difficulty:", ref difficulty)) {
				} else if (ReadStringAttribute(line, "Tutorials:", ref tutorials1)) {
				} else if (ReadStringAttribute(line, "Tutorials (safe):", ref tutorials2)) {
				} else if (ReadStringAttribute(line, "Success message:", ref successMessage)) {
				} else if (ReadStringAttribute(line, "Extra animation:", ref extraAnimation)) {
				} else if (ReadStringAttribute(line, "Goal text:", ref goalTextOverride)) {
				} else if (ReadStringAttribute(line, Labels.Cornerstone, (v) => Cornerstone = v)) {
				} else if (ReadStatisticsKeyAttribute(line, ref statisticsKey)) {
					StatisticsKey = statisticsKey;
				} else if (ReadBoolAttribute(line, "Key Tier", ref isKeyTier)) {
				} else if (ReadBoolAttribute(line, "Cell break tutorial", ref cellBreakAnimationOnLevelGoals)) {
				} else if (ReadBoolAttribute(line, "No music", ref noMusic)) {
				} else if (ReadIntAttribute(line, "WinCount:", ref winCount)) {
				} else if (ReadIntAttribute(line, "FailCount:", ref failCount)) {
				} else if (ReadIntAttribute(line, Labels.ObstacleGoal, (v) => ObstacleGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.WaffleGoal, (v) => WaffleGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.TrashGoal, (v) => TrashGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.LineBonusGoal, (v) => LineBonusGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.BombBonusGoal, (v) => BombBonusGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.KiteBonusGoal, (v) => KiteBonusGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.LightningBonusGoal, (v) => LightningBonusGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.MovesLimit, (v) => MovesLimit = v)) {
				} else if (ReadIntAttribute(line, Labels.MarbleKindGoal, (v) => MarbleKindGoal = v)) {
				} else if (ReadIntAttribute(line, Labels.MarbleCountGoal, (v) => MarbleCountGoal = v)) {
				} else if (ReadIntAttribute(line, "Extra boards:", ref extraBoards)) {
				} else if (ReadIntAttribute(line, "Director strategy:", ref directorStrategy)) {
				}
			}
		}


		// Есть два SaveLevels. Отличия в них в том, что первый (который принимает StreamWriter)
		// не закрывает поток, а второй (который принимает Stream) закрывает
		// satana: по совету Кости заменил StreamWriter на TextWriter, чтобы можно
		// было передать StringWriter

		private void WriteProperty(TextWriter w, string propertyName, int propertyValue, int threshold = 0)
		{
			if (propertyValue > threshold) {
				w.WriteLine($"{propertyName} {propertyValue}");
			}
		}

		private void WriteProperty(TextWriter w, string propertyName, string propertyValue)
		{
			if (!string.IsNullOrEmpty(propertyValue)) {
				w.WriteLine($"{propertyName} {propertyValue}");
			}
		}

		private void WriteProperty(TextWriter w, string propertyName, bool propertyValue)
		{
			if (propertyValue) {
				w.WriteLine(propertyName);
			}
		}

		/// <summary>
		/// Сохраняет уровень, игнорируя бранчи
		/// </summary>
		public void SaveLevel(TextWriter w)
		{
			WriteProperty(w, "WinCount:", winCount);
			WriteProperty(w, "FailCount:", failCount);

			w.WriteLine("Statistics key: " + StatisticsKey.ToString());

			WriteProperty(w, "Items:", items);
			WriteProperty(w, "Science goal:", scienceGoal);
			WriteProperty(w, "Engineering goal:", engineeringGoal);
			WriteProperty(w, "Aesthetics goal:", aestheticsGoal);
			WriteProperty(w, "Score goal:", scoreGoal);
			WriteProperty(w, Labels.ObstacleGoal, ObstacleGoal);
			WriteProperty(w, Labels.WaffleGoal, WaffleGoal);
			WriteProperty(w, Labels.TrashGoal, TrashGoal);
			WriteProperty(w, Labels.LineBonusGoal, LineBonusGoal);
			WriteProperty(w, Labels.BombBonusGoal, BombBonusGoal);
			WriteProperty(w, Labels.KiteBonusGoal, KiteBonusGoal);
			WriteProperty(w, Labels.LightningBonusGoal, LightningBonusGoal);
			WriteProperty(w, Labels.MovesLimit, MovesLimit);
			WriteProperty(w, Labels.MarbleKindGoal, MarbleKindGoal);
			WriteProperty(w, Labels.MarbleCountGoal, MarbleCountGoal);
			WriteProperty(w, Labels.Cornerstone, Cornerstone);
			WriteProperty(w, "Turn limit:", turnLimit);
			WriteProperty(w, "Time limit:", timeLimit);
			WriteProperty(w, "Barrel period:", trashPeriod);
			WriteProperty(w, "Mystery Bag period:", mysteryBagPeriod);
			WriteProperty(w, "Tornados:", tornados);
			WriteProperty(w, "Stones:", stones);
			WriteProperty(w, "Stone interval:", stoneInterval);
			WriteProperty(w, "Blocker bombs:", blockerBombs);
			WriteProperty(w, "Blocker bomb interval:", blockerBombInterval);
			WriteProperty(w, "Blocker bomb capacity:", blockerBombCapacity);
			WriteProperty(w, "Tutorials:", tutorials1);
			WriteProperty(w, "Tutorials (safe):", tutorials2);
			WriteProperty(w, "Success message:", successMessage);
			WriteProperty(w, "Extra animation:", extraAnimation);
			WriteProperty(w, "Goal text:", goalTextOverride);
			WriteProperty(w, "Difficulty:", difficulty, -1);
			WriteProperty(w, "Key Tier", isKeyTier);
			WriteProperty(w, "Cell break tutorial", cellBreakAnimationOnLevelGoals);
			WriteProperty(w, "No music", noMusic);
			WriteProperty(w, "Extra boards:", extraBoards);
			WriteProperty(w, "Director strategy:", directorStrategy);

			if (mysteryBagSpawns != null) {
				w.WriteLine("Mystery Bag spawns: [");
				w.WriteLine("\tBlocker: " + mysteryBagSpawns.BlockerSpawnWeight);
				w.WriteLine("\tBonus: " + mysteryBagSpawns.BonusSpawnWeight);
				foreach (var s in mysteryBagSpawns.Spawns) {
					w.WriteLine(string.Format("\t{0}: {1}", s.Item1, s.Item2));
				}
				w.WriteLine("]");
			}
			if (!string.IsNullOrEmpty(board)) {
				w.WriteLine("Board:");
				w.Write(board);
				w.WriteLine();
			}

			OnSave();

		}

		public void SaveLevel(Stream stream)
		{
			using (var w = new StreamWriter(stream)) {
				SaveLevel(w);
			}
		}

		private class Labels
		{
			internal const string ObstacleGoal = "Obstacle Goal:";
			internal const string WaffleGoal = "Waffle Goal:";
			internal const string TrashGoal = "Trash Goal:";
			internal const string LineBonusGoal = "Line Bonus Goal:";
			internal const string BombBonusGoal = "Bomb Bonus Goal:";
			internal const string KiteBonusGoal = "Kite Bonus Goal:";
			internal const string LightningBonusGoal = "Lightning Bonus Goal:";
			internal const string MovesLimit = "Moves Limit:";
			internal const string MarbleKindGoal = "Marble Kind Goal:";
			internal const string MarbleCountGoal = "Marble Count Goal:";
			internal const string Cornerstone = "Cornerstone:";
		}
	}

	public class LevelInfoTierPack : IEnumerable<LevelInfo>
	{
		private readonly LevelInfo[] tiers;

		public List<LevelInfoTierPack> Branch;
		public int Count { get { return tiers.Length; } }
		public bool IsKBLevel { get; set; }
		public int LevelIndex { get; set; } = -1;
		public int BranchIndex { get; set; } = -1;

		public LevelInfoTierPack(IEnumerable<LevelInfo> tiers)
		{
			this.tiers = tiers.ToArray();
			SetKB();
		}

		public LevelInfoTierPack(IEnumerable<LevelInfo> tiers, int levelIndex, int branchIndex, bool isKB)
		{
			this.tiers = tiers.ToArray();
			LevelIndex = levelIndex;
			BranchIndex = branchIndex;
			IsKBLevel = isKB;
		}

		public LevelInfoTierPack(params LevelInfo[] tiers)
		{
			this.tiers = tiers;
			SetKB();
		}

		private void SetKB()
		{
			if (tiers.Length == 0) {
				return;
			}
			bool isKB = false;
			bool isNoKB = false;
			foreach (var tier in tiers) {
				isKB = isKB || tier.isKBLevel;
				isNoKB = isNoKB || !tier.isKBLevel;
			}
			if (isKB == isNoKB)
				throw new ArgumentException("all or no one tiers must be marked as KB");
			IsKBLevel = isKB;
		}

		public LevelInfo this[int i]
		{
			get
			{
				return tiers[i];
			}
			set
			{
				tiers[i] = value;
			}
		}

		public static LevelInfoTierPack LoadTiers(Stream stream, int levelIndex = -1, int branchIndex = -1, bool isKB = false)
		{
			return new LevelInfoTierPack(LoadTiersProcess(stream), levelIndex, branchIndex, isKB);
		}

		private static IEnumerable<LevelInfo> LoadTiersProcess(Stream stream)
		{
			using (var reader = new StreamReader(stream)) {
				LevelInfo info = null;
				Guid statisticsKey = Guid.Empty;
				while (true) {
					string line = reader.ReadLine();
					if (line == null) {
						break;
					} else if (line.Length > 4 && line.Substring(0, 4) == "Tier") {
						if (info != null) {
							yield return info;
						}
#if UNITY || !ALLOW_XML
						info = new LevelInfo();
#else
						info = new MetadataLevelInfo();
#endif
						info.LoadLevel(reader);
					}
				}
				if (info != null) {
					info.OnLoad();
					yield return info;
				}
			}
		}

		public void SaveTiers(TextWriter w)
		{
			for (int i = 0; i < tiers.Length; i++) {
				w.WriteLine($"Tier {i + 1}");
				tiers[i].SaveLevel(w);
			}
		}

		public void SaveTiers(Stream stream)
		{
			using (var w = new StreamWriter(stream)) {
				SaveTiers(w);
			}
		}

		public override string ToString()
		{
			return $"{(IsKBLevel ? "[KB]" : "")}[{LevelIndex}][{BranchIndex}]";
		}

		public IEnumerator<LevelInfo> GetEnumerator()
		{
			return ((IEnumerable<LevelInfo>)tiers).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<LevelInfo>)tiers).GetEnumerator();
		}
	}

	public class LevelsGroup
	{
		// 3 normal tiers (must have same level count) + KB levels (1 tier for each KB)
		public List<LevelInfo>[] tier;

		public List<LevelInfoTierPack> Levels;
		public List<LevelInfoTierPack> KeyBuildings;

		public string baseName;
		private readonly City city;

		public LevelsGroup(City city)
		{
			this.city = city;
			baseName = GetConfigBase(city);
			NormalTierCount = city == City.Tutorial ? 1 : 3;
		}

		public string GetConfigBase(City city)
		{
			var number = (int)city;
			var id = city.ToString();
			return $"{number:00}{id}/{id}";
		}

		static string[] LevelTierSuffix_NewFormat = { "1.txt", "2.txt", "3.txt" };
		public static string GetLevelsFile(string baseName, int tier)
		{
			if (tier > 2) {
				return baseName + string.Format("KB{0:00}.txt", tier - 2);
			} else {
				return baseName + LevelTierSuffix_NewFormat[tier];
			}
		}

		public static string GetLevelsFile(string baseName, LevelInfoTierPack level)
		{
			return GetLevelsFile(baseName, level.LevelIndex, level.BranchIndex, level.IsKBLevel);
		}

		public static string GetLevelsFile(string baseName, int levelIndex, int branchIndex, bool isKBlevel)
		{
			if (isKBlevel) {
				return $"{baseName}KB{levelIndex + 1:00}.txt";
			} else {
				return $"{baseName}{levelIndex + 1:000}{(branchIndex >= 0 ? $"-{branchIndex + 1}" : string.Empty)}.txt";
			}
		}

		public int NormalTierCount { get; private set; }

		public void OldLoad(bool onlyLocal = false)
		{
			if (city == City.Tutorial) {
				tier = new List<LevelInfo>[1];
				var filename = Path.Combine(GetEditableConfigDir(), baseName) + ".txt";
				using (var stream = OpenConfigFile(filename, onlyLocal)) {
					tier[0] = LevelInfo.OldLoadLevels(stream).ToList();
				}
			} else {
				int numKBs = 0;
				while (File.Exists(Path.Combine(GetEditableConfigDir(), GetLevelsFile(baseName, 3 + numKBs)))) {
					numKBs++;
				}
				tier = new List<LevelInfo>[3 + numKBs];
				for (int i = 0; i < tier.Length; i++) {
					var filename = Path.Combine(GetEditableConfigDir(), GetLevelsFile(baseName, i));
					using (var stream = OpenConfigFile(filename, onlyLocal)) {
						tier[i] = LevelInfo.OldLoadLevels(stream).ToList();
					}
				}
				for (int i = 3; i < tier.Length; i++) {
					foreach (var info in tier[i]) {
						info.isKBLevel = true;
					}
				}
			}
			Levels = new List<LevelInfoTierPack>();
			KeyBuildings = new List<LevelInfoTierPack>();
			if (city == City.Tutorial) {
				for (int i = 0; i < tier[0].Count; i++) {
					Levels.Add(new LevelInfoTierPack(tier[0][i]));
				}
			} else {
				for (int i = 0; i < tier[0].Count; i++) {
					Levels.Add(new LevelInfoTierPack(tier[0][i], tier[1][i], tier[2][i]));
				}
				for (int i = 3; i < tier.Length; i++) {
					KeyBuildings.Add(new LevelInfoTierPack(tier[i]));
				}
			}
				Verify();
		}

		public void Load(bool onlyLocal = false)
		{
			Levels = new List<LevelInfoTierPack>();
			string filename;
			int levelIndex = 0;
			while (true) {
				int branchLength = -1;
				while (true) {
					filename = Path.Combine(GetEditableConfigDir(), GetLevelsFile(baseName, levelIndex, branchLength, false));
					if (File.Exists(filename)) {
						using (var stream = OpenConfigFile(filename, onlyLocal)) {
							Levels.Add(LevelInfoTierPack.LoadTiers(stream, levelIndex, branchLength));
						}
						branchLength++;
					} else
						break;
				}
				if (branchLength == -1)
					break;
				levelIndex++;
			}

			KeyBuildings = new List<LevelInfoTierPack>();
			int kbIndex = 0;
			while (true) {
				filename = GetLevelsFile(baseName, kbIndex, -1, true);
				if (File.Exists(filename)) {
					using (var stream = OpenConfigFile(filename, onlyLocal)) {
						KeyBuildings.Add(LevelInfoTierPack.LoadTiers(stream, kbIndex, isKB: true));
					}
					kbIndex++;
				} else
					break;
			}
			Verify();
		}

		public bool TryLoad(bool onlyLocal = false)
		{
			try {
				Load(onlyLocal);
			} catch (Lime.Exception) {
				// We're expecting a "Can't open asset" error if there are no downloaded cfg files and the city bundle is not available.
				return false;
			}
			return true;
		}

		void Verify()
		{
			if (Levels.Count != 0) {
				var tiers = Levels[0].Count;
				foreach (var level in Levels)
					if (level.Count != tiers) {
						throw new Lime.Exception(
							$"Tiers count in all levels must be equal (L{level.LevelIndex}{(level.BranchIndex > -1 ? ("-" + level.BranchIndex) : "")} - {level.Count} tiers instead {tiers})");
					}
			} else if (KeyBuildings.Count != 0) {
				var tiers = KeyBuildings[0].Count;
				foreach (var level in KeyBuildings)
					if (level.Count != tiers) {
						throw new Lime.Exception(
							$"Tiers count in all levels must be equal (L{level.LevelIndex}{(level.BranchIndex > -1 ? ("-" + level.BranchIndex) : "")} - {level.Count} tiers instead {tiers})");
					}
			} else {
				throw new Lime.Exception("No levels loaded");
			}
		}

#if WIN

		public bool Save(Dictionary<int, int> branches)
		{
			try {
				int index = 0;
				int branchIndex = -1;
				int branchCount = 0;
				foreach (var level in Levels) {
					level.LevelIndex = index;
					if (branches.ContainsKey(index)) {
						branchCount = branches[index];
					} else if (branchCount > 0) {
						branchIndex++;
					} else {
						index++;
					}

					if (branchCount > 0) {
						level.BranchIndex = branchIndex;
						branchIndex++;
						if (branchIndex == branchCount) {
							branchIndex = -1;
							branchCount = 0;
							index++;
						}
					}
				}
				index = 0;
				foreach (var level in KeyBuildings) {
					level.LevelIndex = index;
					index++;
				}

				foreach (var level in Levels) {
					using (var stream = OpenEditableConfigForWriting(Path.Combine(GetEditableConfigDir(), GetLevelsFile(baseName, level)))) {
						level.SaveTiers(stream);
					}
				}

				foreach (var level in KeyBuildings) {
					using (var stream = OpenEditableConfigForWriting(Path.Combine(GetEditableConfigDir(), GetLevelsFile(baseName, level)))) {
						level.SaveTiers(stream);
					}
				}

				return true;
			} catch (IOException) {
				// файл может быть занят кем-то еще
				return false;
			}
		}

		public static Stream OpenConfigFile(string fileName, bool onlyLocal = true)
		{
			if (File.Exists(fileName)) {
				return new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
			return null;
		}

		public static Stream OpenEditableConfigForWriting(string fileName)
		{
			return new FileStream(Path.Combine(GetEditableConfigDir(), fileName), FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
		}

		public static string GetEditableConfigDir() { return Path.Combine(The.Workspace.AssetsDirectory, "Levels"); }

		public void SetLevels(IEnumerable<LevelInfoTierPack> levels)
		{
			Levels = new List<LevelInfoTierPack>();
			KeyBuildings = new List<LevelInfoTierPack>();

			foreach (var level in levels) {
				if (level.IsKBLevel)
					KeyBuildings.Add(level);
				else
					Levels.Add(level);
			}
		}

#endif

		public void ExpandBranches(Dictionary<int, int> branches)
		{
			if (Levels == null) {
				return;
			}
			var result = new List<LevelInfoTierPack>();
			for (int i = 0, src = 0; src < Levels.Count; i++, src++) {
				result.Add(Levels[src]);
				int branchLength;
				if (branches.TryGetValue(i, out branchLength)) {
					var level = Levels[src];
					level.Branch = Levels.GetRange(src + 1, branchLength);
					for (int j = 0; j < level.Count; j++) {
						level[j].branch = level.Branch.Select(b => b[j]).ToList();
					}
					src += branchLength;
				}
			}
			Levels = result;
		}

		public List<LevelInfoTierPack> this[bool isKB]
		{
			get { return isKB ? KeyBuildings : Levels; }
		}

		public int Count { get { return Levels.Count; } }
		public int KeyBuildingsCount { get { return KeyBuildings.Count; } }
		public int TotalCount { get { return Levels.Count + KeyBuildings.Count; } }

		public int GetKeyTier(int level, bool isKB = false)
		{
			var levels = isKB ? KeyBuildings : Levels;
			for (int i = 0; i < levels[level].Count; i++) {
				if (levels[level][i].isKeyTier) {
					return i;
				}
			}
			return -1;
		}

		public void SetKeyTier(int level, int value, bool isKB)
		{
			var levels = isKB ? KeyBuildings : Levels;
			for (int i = 0; i < levels[level].Count; i++) {
				levels[level][i].isKeyTier = (i == value);
			}
		}

		public bool HasKeyTier(int level, bool isKB = false)
		{
			return GetKeyTier(level, isKB) != -1;
		}
	}
}
