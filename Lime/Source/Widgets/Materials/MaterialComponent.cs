namespace Lime
{
	[MutuallyExclusiveDerivedComponents]
	[AllowedComponentOwnerTypes(typeof(Image))]
	public class MaterialComponent : NodeComponent
	{

	}

	public class MaterialComponent<T> : MaterialComponent where T : IMaterial, new()
	{
		protected T CustomMaterial { get; private set; }
		private IMaterial savedCustomMaterial;

		public MaterialComponent()
		{
			CustomMaterial = new T();
		}

		public override NodeComponent Clone()
		{
			var clone = (MaterialComponent<T>)base.Clone();
			clone.CustomMaterial = (T)CustomMaterial.Clone();
			return clone;
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (oldOwner != null) {
				((Image)oldOwner).CustomMaterial = savedCustomMaterial;
			}
			if (Owner != null) {
				savedCustomMaterial = ((Image)Owner).CustomMaterial;
				((Image)Owner).CustomMaterial = CustomMaterial;
			}
		}
	}
}
