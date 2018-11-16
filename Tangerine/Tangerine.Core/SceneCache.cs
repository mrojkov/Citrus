using System;
using System.Collections.Generic;
using System.Threading;
using Lime;
using Yuzu = Lime.Yuzu;

namespace Tangerine.Core
{
	public class SceneCache
	{
		private class CacheEntry
		{
			public Node Node;
			public DateTime LastModificationTime;
			public bool IsLocked;
			public HashSet<string> Dependencies = new HashSet<string>();
			public bool IsNeedReloadExternalScenes { get; set; }
			public Func<Node> NodeProvider { get; set; }

			public void RefreshDependencies()
			{
				Dependencies.Clear();
				foreach (var descendant in Node.Descendants) {
					string contentsPath = descendant.ContentsPath;
					if (!contentsPath.IsNullOrWhiteSpace() && Node.ResolveScenePath(contentsPath) != null) {
						Dependencies.Add(contentsPath);
					}
				}
			}
		}

		public SceneCache()
		{
			Node.SceneLoading = new ThreadLocal<Node.SceneLoadingDelegate>(() => SceneCache_SceneLoading);
			Node.SceneLoaded = new ThreadLocal<Node.SceneLoadedDelegate>(() => SceneCache_SceneLoaded);
		}

		private readonly Dictionary<string, CacheEntry> contentPathToNodeAndDateModified = new Dictionary<string, CacheEntry>();
		private readonly Dictionary<string, object> contentPathToLock = new Dictionary<string, object>();

		private bool SceneCache_SceneLoading(string path, ref Node instance, bool external)
		{
			Console.WriteLine($"Loading: {path}, external: {external}");
			if (!external) {
				return false;
			}
			lock (GetLockObject(path)) {
				if (!contentPathToNodeAndDateModified.TryGetValue(path, out var t)) {
					contentPathToNodeAndDateModified.Add(path, new CacheEntry {
						LastModificationTime = DateTime.Now,
						IsLocked = true,
					});
					return false;
				} else if (t.Node == null) {
					return false;
				}

				if (t.NodeProvider != null) {
					t.RefreshDependencies();
					instance = t.NodeProvider().Clone();
					return true;
				}
				if (t.IsNeedReloadExternalScenes) {
					t.IsNeedReloadExternalScenes = false;
					t.Node.LoadExternalScenes(TangerineYuzu.Instance.Value);
				}
				instance = t.Node.Clone();
				return true;
			}
		}

		private void SceneCache_SceneLoaded(string path, Node instance, bool external)
		{
			Console.WriteLine($"Loaded: {path}, external: {external}");
			if (!external) {
				return;
			}
			lock (GetLockObject(path)) {
				if (!contentPathToNodeAndDateModified.TryGetValue(path, out var t)) {
					throw new InvalidOperationException();
				}

				if (t.Node == null) {
					t.Node = instance.Clone();
					t.RefreshDependencies();
				} else {
					return;
				}
			}
		}

		public void InvalidateEntry(string path, Func<Node> nodeProviderFunc)
		{
			lock (GetLockObject(path)) {
				var t = contentPathToNodeAndDateModified[path];
				if (nodeProviderFunc == null) {
					t.Node = null;
					t.NodeProvider = null;
				}
				else {
					t.NodeProvider = nodeProviderFunc;
				}
				t.Dependencies.Clear();
			}
			foreach (var kv in contentPathToNodeAndDateModified) {
				if (kv.Value.Dependencies.Contains(path)) {
					// InvalidateEntry(kv.Key);
					// kv.Value.Node.LoadExternalScenes(TangerineYuzu.Instance.Value); // file access problems use lazy access
					kv.Value.IsNeedReloadExternalScenes = true;
				}
			}
		}

		private object GetLockObject(string path)
		{
			lock (contentPathToLock) {
				if (!contentPathToLock.TryGetValue(path, out var lockObject)) {
					lockObject = new object();
					contentPathToLock.Add(path, lockObject);
				}
				return lockObject;
			}
		}

		public void RemoveNodeProvider(string path)
		{
			lock (GetLockObject(path)) {
				var t = contentPathToNodeAndDateModified[path];
				t.NodeProvider = null;
				t.RefreshDependencies();
			}
			foreach (var kv in contentPathToNodeAndDateModified) {
				if (kv.Value.Dependencies.Contains(path)) {
					// InvalidateEntry(kv.Key);
					// kv.Value.Node.LoadExternalScenes(TangerineYuzu.Instance.Value); // file access problems use lazy
					kv.Value.IsNeedReloadExternalScenes = true;
				}
			}
		}
	}
}
