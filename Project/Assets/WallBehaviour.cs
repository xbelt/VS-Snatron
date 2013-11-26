﻿using UnityEngine;
using System.Collections;

public class WallBehaviour : MonoBehaviour {
	public Vector3 start;
	public Vector3 end;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public void updateWall (Vector3 newEnd) {
		end = newEnd;
		float angle = (start.x == end.x) ? 0 : 90;
		transform.position = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 0.5f;
		transform.eulerAngles = new Vector3(0, angle, 0);
		transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Vector3.Distance(start, end));
	}
}