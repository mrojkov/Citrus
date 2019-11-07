using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public class BehaviorSystem
	{
		private Dictionary<Type, BehaviorUpdateStage> updateStages = new Dictionary<Type, BehaviorUpdateStage>();
		private Dictionary<Type, BehaviorUpdateFamily> updateFamilies = new Dictionary<Type, BehaviorUpdateFamily>();
		private LinkedList<BehaviorComponent> behaviorsToStart = new LinkedList<BehaviorComponent>();

		public BehaviorUpdateStage GetUpdateStage(Type stageType)
		{
			if (!updateStages.TryGetValue(stageType, out var stage)) {
				stage = new BehaviorUpdateStage(stageType);
				updateStages.Add(stageType, stage);
			}
			return stage;
		}

		public void StartPendingBehaviors()
		{
			while (behaviorsToStart.Count > 0) {
				var b = behaviorsToStart.First.Value;
				behaviorsToStart.RemoveFirst();
				b.StartQueueNode = null;
				b.UpdateFamily = GetUpdateFamily(b.GetType());
				b.UpdateFamily?.Filter(b);
				b.Start();
			}
		}

		internal void Add(BehaviorComponent behavior)
		{
			behavior.StartQueueNode = behaviorsToStart.AddLast(behavior);
		}

		internal void Remove(BehaviorComponent behavior, Node owner)
		{
			if (behavior.StartQueueNode != null) {
				behaviorsToStart.Remove(behavior.StartQueueNode);
				behavior.StartQueueNode = null;
			} else {
				behavior.UpdateFamily?.Remove(behavior);
				behavior.UpdateFamily = null;
				behavior.Stop(owner);
			}
		}

		internal void OnOwnerFrozenChanged(BehaviorComponent behavior)
		{
			if (behavior.StartQueueNode == null) {
				behavior.UpdateFamily?.Filter(behavior);
				behavior.OnOwnerFrozenChanged();
			}
		}

		private BehaviorUpdateFamily GetUpdateFamily(Type behaviorType)
		{
			if (updateFamilies.TryGetValue(behaviorType, out var updateFamily)) {
				return updateFamily;
			}
			var updateStageAttr = behaviorType.GetCustomAttribute<UpdateStageAttribute>(inherit: true);
			if (updateStageAttr == null) {
				return null;
			}
			var updateStage = GetUpdateStage(updateStageAttr.StageType);
			var updateFrozen = behaviorType.IsDefined(typeof(UpdateFrozenAttribute), inherit: true);
			updateFamily = updateStage.CreateFamily(behaviorType, updateFrozen);
			updateFamilies.Add(behaviorType, updateFamily);
			foreach (var i in behaviorType.GetCustomAttributes<UpdateAfterBehaviorAttribute>(inherit: true)) {
				updateStage.AddDependency(GetUpdateFamily(i.BehaviorType), updateFamily);
			}
			foreach (var i in behaviorType.GetCustomAttributes<UpdateBeforeBehaviorAttribute>(inherit: true)) {
				updateStage.AddDependency(updateFamily, GetUpdateFamily(i.BehaviorType));
			}
			return updateFamily;
		}
	}

	public class BehaviorUpdateStage
	{
		private Dictionary<BehaviorUpdateFamily, int> familyIndexMap = new Dictionary<BehaviorUpdateFamily, int>();
		private List<BehaviorUpdateFamily> families = new List<BehaviorUpdateFamily>();
		private List<List<int>> afterFamilyIndices = new List<List<int>>();
		private List<BehaviorUpdateFamily> sortedFamilies = new List<BehaviorUpdateFamily>();
		private bool shouldSortFamilies;

		public readonly Type StageType;

		internal BehaviorUpdateStage(Type stageType)
		{
			StageType = stageType;
		}

		internal BehaviorUpdateFamily CreateFamily(Type behaviorType, bool updateFrozen)
		{
			var family = new BehaviorUpdateFamily(behaviorType, updateFrozen);
			familyIndexMap.Add(family, families.Count);
			families.Add(family);
			afterFamilyIndices.Add(new List<int>());
			shouldSortFamilies = true;
			return family;
		}

		internal void AddDependency(BehaviorUpdateFamily before, BehaviorUpdateFamily after)
		{
			if (!familyIndexMap.TryGetValue(before, out var beforeIndex) ||
				!familyIndexMap.TryGetValue(after, out var afterIndex)
			) {
				throw new InvalidOperationException();
			}
			afterFamilyIndices[beforeIndex].Add(afterIndex);
			shouldSortFamilies = true;
		}

		public void Update(float delta)
		{
			if (shouldSortFamilies) {
				SortFamilies();
				shouldSortFamilies = false;
			}
			foreach (var f in sortedFamilies) {
				f.Update(delta);
			}
		}

		private void SortFamilies()
		{
			sortedFamilies.Clear();
			var beforeCounter = new int[families.Count];
			for (var i = 0; i < families.Count; i++) {
				foreach (var j in afterFamilyIndices[i]) {
					beforeCounter[j]++;
				}
			}
			var queue = new Queue<int>();
			for (var i = 0; i < families.Count; i++) {
				if (beforeCounter[i] == 0) {
					queue.Enqueue(i);
				}
			}
			while (queue.Count > 0) {
				var i = queue.Dequeue();
				sortedFamilies.Add(families[i]);
				foreach (var j in afterFamilyIndices[i]) {
					if (--beforeCounter[j] == 0) {
						queue.Enqueue(j);
					}
				}
			}
			if (sortedFamilies.Count != families.Count) {
				throw new InvalidOperationException();
			}
		}
	}

	internal class BehaviorUpdateFamily
	{
		private List<BehaviorComponent> behaviors = new List<BehaviorComponent>();

		public readonly Type BehaviorType;
		public readonly bool UpdateFrozen;

		public BehaviorUpdateFamily(Type behaviorType, bool updateFrozen)
		{
			BehaviorType = behaviorType;
			UpdateFrozen = updateFrozen;
		}

		public void Filter(BehaviorComponent b)
		{
			if ((b.Owner.GloballyFrozen && !UpdateFrozen) || b.Suspended) {
				Remove(b);
			} else {
				Add(b);
			}
		}

		public void Add(BehaviorComponent b)
		{
			if (b.IndexInUpdateFamily < 0) {
				b.IndexInUpdateFamily = behaviors.Count;
				behaviors.Add(b);
			}
		}

		public void Remove(BehaviorComponent b)
		{
			if (b.IndexInUpdateFamily >= 0) {
				behaviors[b.IndexInUpdateFamily] = null;
				b.IndexInUpdateFamily = -1;
			}
		}

		public void Update(float delta)
		{
			for (var i = behaviors.Count - 1; i >= 0; i--) {
				var b = behaviors[i];
				if (b != null) {
					b.Update(delta * b.Owner.EffectiveAnimationSpeed);
				} else {
					b = behaviors[behaviors.Count - 1];
					if (b != null) {
						b.IndexInUpdateFamily = i;
					}
					behaviors[i] = b;
					behaviors.RemoveAt(behaviors.Count - 1);
				}
			}
		}
	}
}
