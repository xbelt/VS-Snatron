using UnityEngine;
using System.Collections;

public class PowerUpDestroyer : MonoBehaviour {
    public void ConsumePowerUp()
    {
        Destroy(gameObject);
    }

    public void DestroyTimed(int time) {
        Destroy(gameObject, time);
    }
}
