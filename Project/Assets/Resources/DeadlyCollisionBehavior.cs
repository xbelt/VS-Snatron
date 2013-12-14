using UnityEngine;
using System.Collections;

public class DeadlyCollisionBehavior : MonoBehaviour {

	private Drive _drive;

	// Use this for initialization
	void Start () {
		// Only the local player shall have this behavior
		if (!transform.parent.GetComponent<NetworkView> ().isMine) {
			Destroy(gameObject);
			return;
		}
		Debug.Log ("is mine: deadly collision");

		_drive = transform.parent.GetComponent<Drive> ();
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "wall") {
			Debug.Log ("Deadly Wall entered: " + other.name + " " + name);
			_drive.kill();
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "wall") {
			Debug.Log ("Deadly Wall exited: " + other.name + " " + name);
			_drive.kill ();
		}
	}
}
