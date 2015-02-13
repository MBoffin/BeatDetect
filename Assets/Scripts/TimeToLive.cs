using UnityEngine;
using System.Collections;

public class TimeToLive : MonoBehaviour {

	public float ttl = 1.5f;

	void OnEnable() {
		Invoke("Die", ttl);
	}
	
	void Die() {
		gameObject.SetActive(false);
	}

    void OnDisable() {
        CancelInvoke(); // just in case
    }
}
