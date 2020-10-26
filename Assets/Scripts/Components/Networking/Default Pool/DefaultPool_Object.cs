
namespace MT.Packages.LD47
{
	public abstract class DefaultPool_Object : Pool_Object
	{
		
		public override void OnEnableAll() {
			
		}

		public override void OnDisableAll() {
			
		}

		public override void OnUpdate() {

		}

		public override void OnNetworkAdd() {
			new DefaultPool_Message_Add {
				info = info,
				position = transform.position
			}.SendToAll();
		}

		public override void OnNetworkUpdate() {

		}
	}
}