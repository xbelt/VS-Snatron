using UnityEngine;
using System.Collections;

public class DespawnOnLeaveWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (transform.position.magnitude > Game.Instance.FieldBorderCoordinates + 100) {
	        Destroy(gameObject);
	    }	
    }
}
