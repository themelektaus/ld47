
namespace MT.Packages.LD47
{
	public class GameState_Start : GameState
	{
		protected override void Start() {
			if (StartAsServer.exists) {
				Context.SetTrigger("Start Level");
				return;
			}
			Context.SetTrigger("Goto Titlescreen");
		}
	}
}