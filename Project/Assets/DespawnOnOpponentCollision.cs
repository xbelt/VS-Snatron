using UnityEngine;
using System.Collections;

public class DespawnOnOpponentCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "tron") {
            if (other.gameObject != Game.Instance.Spawner.LocalPlayer) {
                Destroy(gameObject);
            }
        }
    }
}
