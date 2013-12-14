using UnityEngine;
using System.Collections;
using System;

public class CubeBehaviour : MonoBehaviour {
	public float width;
	public Vector3 speed;
	public Vector3 initialPosition;

	// Use this for initialization
	void Start () {
		// TODO set position randomly?
		initialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (Math.Abs(transform.position.x - initialPosition.x) < width) {
			Debug.Log(speed);
				transform.Translate (speed * Time.deltaTime);
		} else {
				initialPosition = transform.position;
				speed = -speed;
		}
	}
}
