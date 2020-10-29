
namespace MT.Packages.LD47
{
	public abstract class DefaultPool_Object : Pool_Object
	{
		public bool updatePosition;
		public SmoothTransformPosition position;

		protected virtual void Awake() {
			position = (transform, .15f);
		}

		public override void OnEnableAll() {
			
		}

		public override void OnDisableAll() {
			
		}

		public override void OnUpdate() {
			if (!info.isMine) {
				position.Update();
			}
		}

		public override void OnNetworkAdd() {
			Utils.Send((DefaultPool_Message_Add) (info, position.current));
		}

		public override void OnNetworkUpdate() {
			if (updatePosition) {
				Utils.Send((DefaultPool_Message_Update) (info, position.current));
			}
		}
	}
}