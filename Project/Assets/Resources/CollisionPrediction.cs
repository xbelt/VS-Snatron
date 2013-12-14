using UnityEngine;
using System.Collections;

public class CollisionPrediction : MonoBehaviour {

	private Drive _drive;
	private BoxCollider _collider;

	// Use this for initialization
	void Start ()
	{
		// Only the local player shall have this behavior
		if (!transform.parent.GetComponent<NetworkView> ().isMine) {
			Destroy(gameObject);
			return;
		}

		_collider = GetComponent<BoxCollider> ();
		this._drive = transform.parent.gameObject.GetComponent<Drive>();
		_drive.CollisionPrediction = this;
	}
	
	void OnTriggerEnter(Collider other) {
		Debug.Log ("Predict Collision enter: " + other.name + " " + name);
		if (other.gameObject.tag == "wall") {
			_drive.OnPredictedCollisionEnter ();
		}
	}
	
	void OnTriggerExit(Collider other) {
		Debug.Log ("Predict Collision exit: " + other.name + " " + name);
		if (other.gameObject.tag == "wall") {
			_drive.OnPredictedCollisionExit ();
		}
	}

	public float Length {
		get {
			return _collider.size.z;
		}
		set {
			Vector3 old = _collider.size;
			_collider.size = new Vector3(old.x, old.y, value);
			Vector3 pos = _collider.transform.position;
			_collider.transform.position.Set(pos.x, pos.y, transform.localScale.z*5.1f + value/2);
		}
	}
}
