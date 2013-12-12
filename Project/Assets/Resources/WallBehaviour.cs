using System;
using UnityEngine;

public class WallBehaviour : MonoBehaviour {
	public Vector3 start;
	public Vector3 end;
    public Color DefaultColor = new Color(45/255f, 203/255f, 1, 0.8f);
	// Use this for initialization
	void Start ()
	{
	    
	}

    void Update()
    {
        gameObject.renderer.material.color = DefaultColor;
    }

    public void setDefaultColor(Color c)
    {
        DefaultColor = c;
    }
	
	// Update is called once per frame
	public void updateWall (Vector3 newEnd) {
		end = newEnd;
		float angle = (Math.Abs(start.x - end.x) < 0.001) ? 0 : 90;
		transform.position = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 1f;
		transform.eulerAngles = new Vector3(0, angle, 0);
		transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, Vector3.Distance(start, end));
	}
}
