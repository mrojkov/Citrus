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
