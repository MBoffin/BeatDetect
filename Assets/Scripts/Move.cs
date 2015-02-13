using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

	public float speed = 1f;
	void Update() {
		transform.Translate(Vector3.back * speed * Time.deltaTime);
        transform.Translate(Vector3.down * (speed / 10) * Time.deltaTime);
	}
}
