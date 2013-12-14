using UnityEngine;
using System.Collections;

public class SpeedUpBehavior : MonoBehaviour {

	private Drive _drive;

	// Use this for initialization
	void Start () {
		// Only the local player shall have this behavior
		if (!transform.parent.GetComponent<NetworkView> ().isMine) {
			Destroy(gameObject);
			return;
		}
		Debug.Log ("is mine: speed up behavior");
		_drive = transform.parent.GetComponent<Drive> ();
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag == "wall") {
			Debug.Log ("Wall near enter: " + other.name + " " + name);
			_drive.OnSpeedUpTriggerEnter();
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.gameObject.tag == "wall") {
			Debug.Log ("Wall near exit: " + other.name + " " + name);
			_drive.OnSpeedUpTriggerExit();
		}
	}
}
