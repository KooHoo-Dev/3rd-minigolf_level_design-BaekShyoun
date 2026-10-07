using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfGoalTrigger : MonoBehaviour
    {
        [SerializeField] private GolfGameManager golfGameManager;

        private void OnTriggerEnter(Collider other)
        {
            GolfBall ball = other.GetComponent<GolfBall>();

            golfGameManager.Goal(ball);
        }
    }
}