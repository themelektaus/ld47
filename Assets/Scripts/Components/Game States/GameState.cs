
namespace MT.Packages.LD47
{
	public class GameState : Core.State<NetworkManager>
	{
		protected override void StateChanged(Core.State<NetworkManager> state) {
			_ = state as GameState;
		}
	}
}