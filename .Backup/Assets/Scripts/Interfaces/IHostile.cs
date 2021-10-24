
namespace MT.Packages.LD47
{
	public interface IHostile
	{
		byte GetFraction();
		void ReceiveDamage(uint senderID, float damage);
	}
}