using Lime;

namespace EmptyProject.Dialogs
{
	public class GameScreen : Dialog<Scenes.GameScreen>
	{
		Widget thumb;
		DragGesture dragGesture;
		PinchGesture pinchGesture;

		public GameScreen()
		{
			//new KineticDragGesture(new DeceleratingKineticMotionStrategy(0.97f, 1.002f)),
			//new PinchGesture(exclusive: true)
			SoundManager.PlayMusic("Ingame");
			Scene._BtnExit.It.Clicked = ReturnToMenu;
			thumb = Root["Image"];
			thumb.HitTestTarget = true;
			thumb.Gestures.Add(new ClickGesture(() => {
				thumb.Parent.RunAnimation("Click");
			}));
			thumb.Gestures.Add(new LongTapGesture(1.0f, () => {
				thumb.Parent.RunAnimation("LongTap");
			}));
			thumb.Gestures.Add(new DoubleClickGesture(() => {
				thumb.Parent.RunAnimation("Tap");
			}));
			dragGesture = new DragGesture(
				new DragGesture.DampingMotionStrategy(0.97f, 0.998f)
			//new SpecialDragGesture.FixedTimeMotionStrategy(0.5f)
			);
			pinchGesture = new PinchGesture();
			thumb.Gestures.Add(dragGesture);
			thumb.Gestures.Add(pinchGesture);
			dragGesture.Changed += OnDragged;
			pinchGesture.Changed += OnPinched;
		}

		private void OnDragged()
		{
			if (dragGesture.IsActive) {
				thumb.Position += dragGesture.LastDragDistance / thumb.Scale;
			}
		}

		private void OnPinched()
		{
			if (pinchGesture.IsActive) {
				thumb.Position -= pinchGesture.LastDragDistance / thumb.Scale;
				var zoom = Mathf.Clamp(thumb.Scale * pinchGesture.LastPinchScale, Vector2.One * 0.2f, Vector2.One * 3.0f);
				thumb.Scale = zoom;
			}
		}

		protected override void Update(float delta)
		{
			if (Input.WasKeyPressed(Key.Escape)) {
				ReturnToMenu();
			}

			thumb.X = Mathf.Clamp(thumb.X, 0, The.World.Width);
			thumb.Y = Mathf.Clamp(thumb.Y, 0, The.World.Height);
		}

		protected override bool HandleAndroidBackButton()
		{
			ReturnToMenu();
			return true;
		}

		private void ReturnToMenu()
		{
			var confirmation = new Confirmation("Are you sure?");
			confirmation.OkClicked += CrossfadeInto<MainMenu>;
			Open(confirmation);
		}
	}
}
