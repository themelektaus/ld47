using System.Collections;
using UnityEngine;

namespace MT.Packages.LD47
{
	public abstract class Pool_Object : MonoBehaviour
	{
		public Pool_ObjectInfo info;

		public abstract void OnEnableObject();

		public abstract void OnDisableObject();

		public abstract void OnUpdate();

		public abstract void OnNetworkAdd();

		public abstract void OnNetworkUpdate();

		readonly Core.Timer updateTimer = .1f;

		protected bool disabled;

		void OnEnable() {
			if (!info.GetPool()) {
				gameObject.SetActive(false);
				return;
			}
			Enable();
			Update();
			if (!info.isMine) {
				return;
			}
			StartCoroutine(DisableRoutine());
			OnNetworkAdd();
			updateTimer.Reset();
		}

		public void Enable() {
			disabled = false;
			OnEnableObject();
		}

		IEnumerator DisableRoutine() {
			yield return new WaitForSeconds(info.GetPool().objectMaxLifetime);
			UnSpawn();
		}

		public void UnSpawn() {
			Utility.Send((Pool_Message_Disable) info);
			Disable();
		}

		public void Disable() {
			disabled = true;
			OnDisableObject();
			StartCoroutine(SetInactiveRoutine());
		}

		IEnumerator SetInactiveRoutine() {
			yield return new WaitForSeconds(1);
			gameObject.SetActive(false);
		}

		void OnDisable() {
			if (info.isMine) {
				Utility.Send((Pool_Message_Remove) info);
			}
			StopAllCoroutines();
			info.ownerID = 0;
			info.objectID = 0;
			disabled = true;
		}

		protected virtual void Update() {
			if (disabled) {
				return;
			}
			OnUpdate();
			if (info.isMine) {
				if (updateTimer.Update()) {
					OnNetworkUpdate();
				}
			}
		}

		bool CheckTrigger(Collider2D collision) {
			return info.isMine && !disabled && !collision.isTrigger;
		}

		void OnTriggerEnter2D(Collider2D collision) {
			if (CheckTrigger(collision)) {
				OnTrigger(collision);
			}
		}

		void OnTriggerStay2D(Collider2D collision) {
			if (CheckTrigger(collision)) {
				OnTrigger(collision);
			}
		}

		protected virtual void OnTrigger(Collider2D collision) {

		}

		public override string ToString() {
			return $"{GetType().Name} <Info: {info}>";
		}
	}
}