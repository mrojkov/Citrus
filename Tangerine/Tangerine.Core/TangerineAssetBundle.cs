using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Lime;
using Orange.FbxImporter;
using Yuzu;

namespace Tangerine.Core
{
	public class TangerineAssetBundle : UnpackedAssetBundle
	{
		private const string VersionFile = "__CACHE_VERSION__";

		public class CacheMeta
		{
			private const string CurrentVersion = "1.6";

			[YuzuRequired]
			public string Version { get; set; } = CurrentVersion;

			public bool IsActual => Version == CurrentVersion;
		}

		public TangerineAssetBundle(string baseDirectory) : base(baseDirectory) { }

		public bool IsActual()
		{
			using (var cacheBundle = OpenCacheBundle(AssetBundleFlags.Writable)) {
				if (!cacheBundle.FileExists(VersionFile)) {
					return false;
				}
				try {
					using (var stream = cacheBundle.OpenFile(VersionFile)) {
						var cacheMeta = TangerineYuzu.Instance.Value.ReadObject<CacheMeta>(VersionFile, stream);
						if (!cacheMeta.IsActual) {
							return false;
						}
					}
				} catch {
					return false;
				}
			}
			return true;
		}

		public void CleanupBundle()
		{
			using (var cacheBundle = OpenCacheBundle(AssetBundleFlags.Writable)) {
				foreach (var path in cacheBundle.EnumerateFiles().ToList()) {
					cacheBundle.DeleteFile(path);
				}
				TangerineYuzu.Instance.Value.WriteObjectToBundle(cacheBundle, VersionFile, new CacheMeta(), Serialization.Format.Binary, string.Empty, AssetAttributes.None, new byte[0]);
			}
		}

		public override Stream OpenFile(string path)
		{
			var ext = Path.GetExtension(path);
			if (ext == ".t3d") {
				var exists3DScene = base.FileExists(path);
				var fbxPath = Path.ChangeExtension(path, "fbx");
				var existsFbx = base.FileExists(fbxPath);
				if (existsFbx && exists3DScene) {
					throw new Exception($"Ambiguity between: {path} and {fbxPath}");
				}
				return exists3DScene ? base.OpenFile(path) : OpenFbx(path);
			}
			if (ext == ".ant") {
				var fbxPath = GetFbxPathFromAnimationPath(path);
				if (fbxPath != null) {
					CheckFbx(fbxPath);
					using (var cacheBundle = OpenCacheBundle()) {
						return cacheBundle.OpenFile(path);
					}
				}
			}
			return base.OpenFile(path);
		}

		private Stream OpenFbx(string path)
		{
			CheckFbx(path);
			using (var cacheBundle = OpenCacheBundle()) {
				return cacheBundle.OpenFile(path);
			}
		}

		private AssetBundle OpenCacheBundle(AssetBundleFlags flags = AssetBundleFlags.None)
		{
			return new PackedAssetBundle(Orange.The.Workspace.TangerineCacheBundle, flags);
		}

		private void CheckFbx(string path)
		{
			using (var cacheBundle = OpenCacheBundle(AssetBundleFlags.Writable)) {
				Model3DAttachment attachment = null;
				Model3D model = null;
				var fbxPath = Path.ChangeExtension(path, "fbx");
				var fbxExists = base.FileExists(fbxPath);
				var fbxCached = cacheBundle.FileExists(path);
				var fbxUpToDate = fbxCached == fbxExists &&
					(!fbxExists || cacheBundle.GetFileLastWriteTime(path) >= base.GetFileLastWriteTime(fbxPath));

				var fbxFullPath = Path.Combine(Orange.The.Workspace.AssetsDirectory, fbxPath);
				var attachmentPath = Path.ChangeExtension(path, Model3DAttachment.FileExtension);
				var attachmentExists = base.FileExists(attachmentPath);
				var attachmentCached = cacheBundle.FileExists(attachmentPath);
				var attachmentUpToDate = attachmentCached == attachmentExists &&
					(!attachmentExists || cacheBundle.GetFileLastWriteTime(attachmentPath) >= base.GetFileLastWriteTime(attachmentPath));
				var fbxImportOptions = new FbxImportOptions {
					Path = fbxFullPath,
					Target = Orange.The.Workspace.ActiveTarget,
					ApplyAttachment = false
				};

				var attachmentMetaPath = Path.ChangeExtension(path, Model3DAttachmentMeta.FileExtension);
				var attachmentMetaCached = cacheBundle.FileExists(attachmentMetaPath);
				var attachmentMetaUpToDate = attachmentMetaCached &&
					cacheBundle.GetFileLastWriteTime(attachmentMetaPath) >= base.GetFileLastWriteTime(fbxPath);
				if (!attachmentMetaUpToDate && fbxExists) {
					using (var fbxImporter = new FbxModelImporter(fbxImportOptions)) {
						model = fbxImporter.LoadModel();
						var meta = new Model3DAttachmentMeta();
						foreach (var animation in model.Animations) {
							meta.SourceAnimationIds.Add(animation.Id);
						}

						foreach (var mesh in model.Descendants.OfType<Mesh3D>()) {
							meta.MeshIds.Add(mesh.Id);
							foreach (var submesh3D in mesh.Submeshes) {
								if (meta.SourceMaterials.All(m => m.Id != submesh3D.Material.Id)) {
									meta.SourceMaterials.Add(submesh3D.Material);
								}
							}
						}
						TangerineYuzu.Instance.Value.WriteObjectToBundle(
							cacheBundle,
							attachmentMetaPath,
							meta, Serialization.Format.Binary, Model3DAttachmentMeta.FileExtension,
							AssetAttributes.None, new byte[0]);
					}
				}

				if (fbxUpToDate && attachmentUpToDate) {
					return;
				}

				var animationPathPrefix = Orange.Toolbox.ToUnixSlashes(Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "@");
				foreach (var assetPath in cacheBundle.EnumerateFiles().ToList()) {
					if (assetPath.EndsWith(".ant") && assetPath.StartsWith(animationPathPrefix)) {
						cacheBundle.DeleteFile(assetPath);
					}
				}

				if (fbxExists) {
					if (model == null) {
						using (var fbxImporter = new FbxModelImporter(fbxImportOptions)) {
							model = fbxImporter.LoadModel();
						}
					}
					if (attachmentExists) {
						attachment = Model3DAttachmentParser.GetModel3DAttachment(fbxFullPath);
						if (attachment.Animations != null) {
							foreach (var animation in attachment.Animations) {
								if (animation.SourceAnimationId == null) {
									animation.SourceAnimationId = model.FirstAnimation?.Id;
								}
							}
						}
						attachment.Apply(model);
					}

					foreach (var animation in model.Animations) {
						if (animation.IsLegacy) {
							continue;
						}
						var animationPathWithoutExt = animationPathPrefix + animation.Id;
						animationPathWithoutExt = Animation.FixAntPath(animationPathWithoutExt);
						var animationPath = animationPathWithoutExt + ".ant";
						animation.ContentsPath = animationPathWithoutExt;
						Serialization.WriteObjectToBundle(cacheBundle, animationPath, animation.GetData(), Serialization.Format.Binary, ".ant", AssetAttributes.None, new byte[0]);
						var animators = new List<IAnimator>();
						animation.FindAnimators(animators);
						foreach (var animator in animators) {
							animator.Owner.Animators.Remove(animator);
						}
					}
					TangerineYuzu.Instance.Value.WriteObjectToBundle(cacheBundle, path, model, Serialization.Format.Binary, ".t3d", AssetAttributes.None, new byte[0]);

				} else if (fbxCached) {
					cacheBundle.DeleteFile(path);
					cacheBundle.DeleteFile(attachmentMetaPath);
				}

				if (attachmentExists) {
					TangerineYuzu.Instance.Value.WriteObjectToBundle(
						cacheBundle,
						attachmentPath,
						Model3DAttachmentParser.ConvertToModelAttachmentFormat(attachment), Serialization.Format.Binary, ".txt",
						AssetAttributes.None, new byte[0]);
				} else if (attachmentCached) {
					cacheBundle.DeleteFile(attachmentPath);
				}
			}
		}

		public override bool FileExists(string path)
		{
			var ext = Path.GetExtension(path);
			if (ext == ".t3d") {
				var exists3DScene = base.FileExists(path);
				var fbxPath = Path.ChangeExtension(path, "fbx");
				var existsFbx = base.FileExists(fbxPath);
				if (existsFbx && exists3DScene) {
					throw new Exception($"Ambiguity between: {path} and {fbxPath}");
				}
				return exists3DScene || existsFbx;
			}
			if (ext == ".ant") {
				var fbxPath = GetFbxPathFromAnimationPath(path);
				if (fbxPath != null) {
					CheckFbx(fbxPath);
					using (var cacheBundle = OpenCacheBundle()) {
						return cacheBundle.FileExists(path);
					}
				}
			}
			return base.FileExists(path);
		}

		private string GetFbxPathFromAnimationPath(string animationPath)
		{
			var separatorIndex = animationPath.LastIndexOf("@");
			if (separatorIndex >= 0) {
				return animationPath.Remove(separatorIndex) + ".fbx";
			}
			return null;
		}
	}

	public class Model3DAttachmentMeta
	{
		public const string FileExtension = ".AttachmentMeta";

		[YuzuMember]
		public ObservableCollection<IMaterial> SourceMaterials = new ObservableCollection<IMaterial>();

		[YuzuMember]
		public ObservableCollection<string> SourceAnimationIds = new ObservableCollection<string>();

		[YuzuMember]
		public ObservableCollection<string> MeshIds = new ObservableCollection<string>();
	}
}
