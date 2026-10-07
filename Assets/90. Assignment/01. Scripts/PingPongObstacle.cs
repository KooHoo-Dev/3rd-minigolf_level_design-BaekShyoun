using UnityEngine;

namespace Study3D.MiniGolf
{
    public class PingPongObstacle : MonoBehaviour
    {
        [SerializeField] private float moveDistance = 2f;
        [SerializeField] private float moveSpeed = 0.5f;

        private Vector3 leftPosition;
        private Vector3 rightPosition;
        private float elapsed;

        private void Awake()
        {
            Vector3 startPosition = transform.localPosition;

            leftPosition = startPosition + Vector3.left * moveDistance;
            rightPosition = startPosition + Vector3.right * moveDistance;
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime * moveSpeed;

            // 가운데에서 오른쪽으로 출발
            float t = Mathf.PingPong(elapsed + 0.5f, 1f);
            transform.localPosition =
                Vector3.Lerp(leftPosition, rightPosition, t);
        }
    }
}