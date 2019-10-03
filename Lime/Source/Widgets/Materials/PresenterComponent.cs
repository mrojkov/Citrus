namespace Lime
{
	[MutuallyExclusiveDerivedComponents]
	[AllowedComponentOwnerTypes(typeof(Image))]
	public class PresenterComponent : NodeComponent { }

	public class PresenterComponent<T> : PresenterComponent where T : IPresenter, new()
	{
		protected T CustomPresenter { get; private set; }
		private IPresenter savedPresenter;

		public PresenterComponent()
		{
			CustomPresenter = new T();
		}

		protected override void OnOwnerChanged(Node oldOwner)
		{
			base.OnOwnerChanged(oldOwner);
			if (oldOwner != null) {
				((Image)oldOwner).Presenter = savedPresenter;
			}
			if (Owner != null) {
				var image = (Image)Owner;
				savedPresenter = image.Presenter;
				image.Presenter = CustomPresenter;
			}
		}
	}
}
