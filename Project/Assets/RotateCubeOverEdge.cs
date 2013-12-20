using UnityEngine;
using System.Collections;
using Random = System.Random;

public class RotateCubeOverEdge : MonoBehaviour {
    public Vector3 axisLocation;
    public Vector3 axisDirection;
    public float rotateTime;
    public float waitTime;
    private Transform pivot;

    void Start() {
        StartCoroutine(rotate90(rotateTime));
    }
    void Update() {

    }

    private IEnumerator rotate90(float time) {
    	System.Random random = new Random();

        while (true) {
            var initialRotation = transform.rotation;
            Debug.Log("Start loop");
            var rate = 1f / time;
            var totalStep = 0.0f;
            var thisStep = 0.0f;
            var lastStep = thisStep;
            var smoothStep = 0.0f;
            var allSteps = 0.0f;
            var pivot = transform.TransformPoint(axisLocation);
            var rotationAxis = transform.TransformDirection(axisDirection);
            while (totalStep < 1.0f)
            {
                totalStep += Time.deltaTime * rate;
                smoothStep = Mathf.SmoothStep(0f, 1f, totalStep);
                thisStep = smoothStep - lastStep;
                lastStep = smoothStep;
                transform.RotateAround(pivot, rotationAxis, 90 * thisStep);
                allSteps += thisStep;
                yield return null;
            }
            transform.rotation = initialRotation;
            if (random.Next(0, 100) < 50) {
                if (random.Next(0, 2) == 0) {
                    transform.Rotate(Vector3.up, 90);
                }
                else {
                    transform.Rotate(Vector3.up, -90);
                }
            }
            var totalWait = 0.0f;
            while (totalWait < waitTime) {
                totalWait += Time.deltaTime;
                yield return null;
            }
        }
    }
}
