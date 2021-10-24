// Generated file

using Mirror;

namespace MT.Packages.LD47
{
	public partial class NetworkManager : Mirror.NetworkManager
	{
		public void RegisterServerMessageHandlers() {
			NetworkServer.RegisterHandler<DefaultPool_Message_Add>(DefaultPool_Message_Add.OnServerReceive);
			NetworkServer.RegisterHandler<DefaultPool_Message_Update>(DefaultPool_Message_Update.OnServerReceive);
			NetworkServer.RegisterHandler<SpawnResourceMessage>(SpawnResourceMessage.OnServerReceive);
			NetworkServer.RegisterHandler<Pool_Message_Disable>(Pool_Message_Disable.OnServerReceive);
			NetworkServer.RegisterHandler<Pool_Message_Remove>(Pool_Message_Remove.OnServerReceive);
			NetworkServer.RegisterHandler<ProjectilePool_Message_Add>(ProjectilePool_Message_Add.OnServerReceive);
			NetworkServer.RegisterHandler<ProjectilePool_Message_AddWithScale>(ProjectilePool_Message_AddWithScale.OnServerReceive);
			NetworkServer.RegisterHandler<ProjectilePool_Message_Update>(ProjectilePool_Message_Update.OnServerReceive);
		}
		
		public void RegisterClientMessageHandlers() {
			NetworkClient.RegisterHandler<DefaultPool_Message_Add>(DefaultPool_Message_Add.OnClientReceive);
			NetworkClient.RegisterHandler<DefaultPool_Message_Update>(DefaultPool_Message_Update.OnClientReceive);
			NetworkClient.RegisterHandler<Pool_Message_Disable>(Pool_Message_Disable.OnClientReceive);
			NetworkClient.RegisterHandler<Pool_Message_DisableAll>(Pool_Message_DisableAll.OnClientReceive);
			NetworkClient.RegisterHandler<Pool_Message_Remove>(Pool_Message_Remove.OnClientReceive);
			NetworkClient.RegisterHandler<ProjectilePool_Message_Add>(ProjectilePool_Message_Add.OnClientReceive);
			NetworkClient.RegisterHandler<ProjectilePool_Message_AddWithScale>(ProjectilePool_Message_AddWithScale.OnClientReceive);
			NetworkClient.RegisterHandler<ProjectilePool_Message_Update>(ProjectilePool_Message_Update.OnClientReceive);
		}
	}
}