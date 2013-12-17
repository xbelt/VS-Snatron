using UnityEngine;
using System.Collections;

public class PowerUpDestroyer : MonoBehaviour {
    public int timeToLive = 10000;

    private void Start() {
        Destroy(gameObject, timeToLive);
    }

    public void ConsumePowerUp() {
        Destroy(gameObject);
    }
}