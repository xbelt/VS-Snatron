using UnityEngine;
using Random = System.Random;

namespace Assets.Resources
{
    class KIControler : Drive
    {
        private System.Random random = new Random();
        private bool lastFrameTurned = false;
        void Start() {
            transform.FindChild("CollisionPredictor").GetComponent<CollisionPrediction>()._drive = this;
            if (GetComponent<NetworkView>().isMine)
            {
                NewWall();
            }
        }

        void Update() {
            if (GetComponent<NetworkView>().isMine)
            {
                // Note: For the collision detection to work well,
                // it is essential that the tron rests at the same place
                // when it is turning. Otherwise the engine has no time to
                // predict the collision. Therefore we return if there was some user input
                bool applied = ApplyUserCommands();
                if (applied)
                {
                    lastFrameTurned = true;
                    return;
                }
            }

            // The tron would keep moving straight
            // But are there any obstacles in front?
            if (_predictedCollisions > 0 && !isIndestructible || _predictedWallCollisions > 0)
            {
                if (!lastFrameTurned) {
                    if (random.Next(0, 100) < 50) {
                        TurnLeft();
                    }
                    else {
                        TurnRight();
                    }
                }
                else {
                    DeadlyCollide();
                }
                lastFrameTurned = true;
                return;
            }
            lastFrameTurned = false;
            // move forward
            transform.Translate(Vector3.forward * _speed * Time.deltaTime);
            if (_latestWallGameObject != null)
            {
                _latestWallGameObject.GetComponent<WallBehaviour>().updateWall(CurrentWallEnd);
            }

            // Adjust the speed last. Implies resizing collider for collision prediction.
            // want to resize the collider before control is returned to unity for collision detection etc.
            // Adjusting the collider within this method, earlier, would not immediately trigger new collisions etc.
            AdjustSpeed();
        }

        new bool ApplyUserCommands() {
            //Handling touch input
            if (random.Next(0, 1000) < 5) {
                TurnLeft();
                return true;
            }
            if (random.Next(0, 1000) > 994) {
                TurnRight();
                return true;
            }
            return false;
        }
    }
}
