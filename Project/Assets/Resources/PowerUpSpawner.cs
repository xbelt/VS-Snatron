using UnityEngine;
using System.Collections;

public class PowerUpSpawner : MonoBehaviour {
    private System.Random random = new System.Random();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (random.Next(0, 1000) < 7 && Game.Instance.HasGameStarted)
		{
			//Debug.Log("Spawn powerUp");
			Game.Instance.Spawner.SpawnPowerUp();
			/* TODO remove these lines

            //TODO: Make dependent of framerate
            var x = random.Next(-Game.Instance.FieldBorderCoordinates, Game.Instance.FieldBorderCoordinates);
            var z = random.Next(-Game.Instance.FieldBorderCoordinates, Game.Instance.FieldBorderCoordinates);
            Network.Instantiate(Resources.Load<Transform>("PowerUpPrefab" + random.Next(0, 2)), new Vector3(x, 0, z),
                    Quaternion.identity, 0);
			*/
        }
	}
}
