
namespace MT.Packages.LD47
{
	public interface IHostile
	{
		bool IsDead();
		void ReceiveDamage(int senderID, string senderTag, float damage);
	}
}