
namespace MT.Packages.LD47
{
	public interface IHostile
	{
		bool IsDead();
		void ReceiveDamage(string senderTag, float damage);
	}
}