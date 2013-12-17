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
		_drive = transform.parent.gameObject.GetComponent<Drive>();
		_drive.CollisionPrediction = this;
	}

	void OnTriggerEnter(Collider other) {
		//Debug.Log ("Predict Collision enter: " + other.name + " " + name);
        if (other.gameObject.tag == "wall" || other.gameObject.tag == "cube" || other.gameObject.tag == "gameWall")
        {
			if (_drive != null) {
				_drive.OnPredictedCollisionEnter ();
			    if (other.gameObject.tag == "gameWall") {
			        _drive.OnPredictedGameWallCollisionEnter();
			    }
			}
		}
        if (other.gameObject.tag == "powerUp0")
        {
            other.gameObject.GetComponent<PowerUpDestroyer>().ConsumePowerUp();
            if (_drive != null)
            {
                _drive.ConsumeSpeedPowerup();
            }
        }
        if (other.gameObject.tag == "powerUp1")
        {
            other.gameObject.GetComponent<PowerUpDestroyer>().ConsumePowerUp();
            if (_drive != null)
            {
                _drive.ConsumeIndestructiblePowerup();
            }
        }
	}
	
	void OnTriggerExit(Collider other) {
		//Debug.Log ("Predict Collision exit: " + other.name + " " + name);
        if (other.gameObject.tag == "wall" || other.gameObject.tag == "cube" || other.gameObject.tag == "gameWall")
        {
			_drive.OnPredictedCollisionExit ();
            if (other.gameObject.tag == "gameWall")
            {
                _drive.OnPredictedGameWallCollisionExit();
            }
		}
	}

	public float Length {
		get {
			return _collider.size.z;
		}
		set {
			Vector3 old = _collider.size;
			_collider.size = new Vector3(old.x, old.y, value);
            _collider.center = new Vector3(_collider.center.x, _collider.center.y, value / 2f + 4.7f);
		}
	}
}
