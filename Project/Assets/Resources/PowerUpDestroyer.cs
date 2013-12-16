using UnityEngine;
using System.Collections;

public class PowerUpDestroyer : MonoBehaviour {
    private System.Random random = new System.Random();
	// Use this for initialization
	void Start ()
	{
	    var ttl = random.Next(5, 15);
        Destroy(gameObject, ttl);
	}

    public void ConsumePowerUp()
    {
        Destroy(gameObject);
    }
}
