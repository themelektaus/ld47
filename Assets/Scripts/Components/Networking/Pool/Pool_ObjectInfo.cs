
namespace MT.Packages.LD47
{
	public struct Pool_ObjectInfo
	{
		public string poolID;
		public uint ownerID;
		public uint objectID;

		public Pool_ObjectInfo(string poolID, uint ownerID, uint objectID) {
			this.poolID = poolID;
			this.ownerID = ownerID;
			this.objectID = objectID;
		}

		public bool isInUse { get { return objectID != 0; } }

		public bool isMine {
			get {
				return isInUse && Utils.IsMine(ownerID);
			}
		}

		public Pool GetPool() {
			return Unique.Get(poolID) as Pool;
		}

		public T GetPool<T>() where T : Pool {
			return Unique.Get<T>(poolID);
		}

		public override string ToString() {
			return $"pooldID: {poolID}, ownerID: {ownerID}, objectID: {objectID}";
		}
	}
}