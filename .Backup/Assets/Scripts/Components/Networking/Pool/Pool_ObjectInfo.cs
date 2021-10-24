
namespace MT.Packages.LD47
{
	public struct Pool_ObjectInfo
	{
		public string poolID;
		public uint ownerID;
		public byte ownerRingIndex;
		public byte ownerFraction;
		public uint objectID;

		// public Pool_ObjectInfo(string poolID, uint ownerID, byte ownerRingIndex, byte ownerFraction, uint objectID) {
		// 	this.poolID = poolID;
		// 	this.ownerID = ownerID;
		// 	this.ownerRingIndex = ownerRingIndex;
		// 	this.ownerFraction = ownerFraction;
		// 	this.objectID = objectID;
		// }

		public bool isInUse { get { return objectID != 0; } }

		public bool isMine {
			get {
				return isInUse && Utility.IsMine(ownerID);
			}
		}

		public Pool GetPool() {
			return Core.Unique.Get(poolID) as Pool;
		}

		public T GetPool<T>() where T : Pool {
			return Core.Unique.Get<T>(poolID);
		}

		public void UsePool(System.Action<Pool, Pool_ObjectInfo> callback) {
			var pool = GetPool();
			if (pool) {
				callback(pool, this);
			}
		}

		public void UsePool<T>(System.Action<T, Pool_ObjectInfo> callback) where T : Pool {
			var pool = GetPool<T>();
			if (pool) {
				callback(pool, this);
			}
		}

		public override string ToString() {
			return $"" +
				$"pooldID: {poolID}, " +
				$"ownerID: {ownerID}, " +
				$"ownerRingIndex: {ownerRingIndex}, " +
				$"ownerFraction: {ownerFraction}, " +
				$"objectID: {objectID}";
		}
	}
}