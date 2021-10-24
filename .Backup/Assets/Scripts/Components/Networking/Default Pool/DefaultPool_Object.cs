
namespace MT.Packages.LD47
{
	public abstract class DefaultPool_Object : Pool_Object
	{
		public bool updatePosition;
		public Core.SmoothTransformPosition position;

		protected virtual void Awake() {
			position = (transform, .15f);
		}

		public override void OnEnableObject() {
			
		}

		public override void OnDisableObject() {
			
		}

		public override void OnUpdate() {
			if (!info.isMine) {
				position.Update();
			}
		}

		public override void OnNetworkAdd() {
			Utility.Send((DefaultPool_Message_Add) (info, position.current));
		}

		public override void OnNetworkUpdate() {
			if (updatePosition) {
				Utility.Send((DefaultPool_Message_Update) (info, position.current));
			}
		}
	}
}