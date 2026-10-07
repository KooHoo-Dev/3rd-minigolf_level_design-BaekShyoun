using UnityEngine;
using UnityEngine.Rendering;

namespace Study3D.MiniGolf
{
    // 패드들만 모아 둔 부모 오브젝트에 붙인다.
    public class FallingPadManager : MonoBehaviour
    {
        [Header("Golf Settings")]
        [SerializeField] private GolfBall ball;
        [SerializeField] private GolfPlayer player;

        [Header("Falling Settings")]
        [SerializeField] private float fallDelay = 10f;
        [SerializeField] private float hideY = -5f;
        [SerializeField] private float groundTolerance = 0.03f;

        [Header("Warning Line")]
        [SerializeField] private Material warningMaterial;
        [SerializeField] private float lineWidth = 0.05f;

        private class PadData
        {
            public BoxCollider collider;
            public Rigidbody body;
            public LineRenderer line;
            public Vector3 startPosition;
            public Quaternion startRotation;
            public float elapsed;
            public bool isCounting;
            public bool isFalling;
        }

        private PadData[] pads;
        private SphereCollider ballCollider;
        
        private void Awake()
        {
            ballCollider = ball.GetComponent<SphereCollider>();
            pads = new PadData[transform.childCount];

            for (int i = 0; i < pads.Length; i++)
            {
                Transform padRoot = transform.GetChild(i);
                
                BoxCollider padCollider = padRoot.GetComponent<BoxCollider>();
                Rigidbody body = padRoot.GetComponent<Rigidbody>();

                if (body == null)
                    body = padRoot.gameObject.AddComponent<Rigidbody>();

                body.constraints = RigidbodyConstraints.FreezePositionX
                                   | RigidbodyConstraints.FreezePositionZ
                                   | RigidbodyConstraints.FreezeRotation;

                body.interpolation = RigidbodyInterpolation.None;

                pads[i] = new PadData
                {
                    collider = padCollider,
                    body = body,
                    line = CreateWarningLine(padCollider),
                    startPosition = padRoot.position,
                    startRotation = padRoot.rotation
                };
            }

            ResetPads();
        }

        private void FixedUpdate()
        {
            // 시작 전에는 GolfPlayer가 꺼져 있으므로 타이머도 진행하지 않는다.
            bool isPlaying = player.isActiveAndEnabled;
            if (isPlaying)
                CheckPadBelowBall();

            foreach (PadData pad in pads)
            {
                if (pad.body.gameObject.activeInHierarchy == false)
                    continue;

                if (pad.isFalling)
                {
                    if (pad.body.position.y <= hideY)
                    {
                        RestorePad(pad, false);
                    }
                    continue;
                }

                if (isPlaying == false || pad.isCounting == false)
                    continue;

                pad.elapsed += Time.fixedDeltaTime;

                // 10초 중 마지막 3초 동안 경고선을 켠다.
                if (pad.elapsed >= fallDelay - 3f)
                    pad.line.enabled = true;

                if (pad.elapsed >= fallDelay)
                {
                    pad.isCounting = false;
                    pad.isFalling = true;
                    pad.body.isKinematic = false;
                    pad.body.useGravity = true;
                    pad.body.WakeUp();
                }
            }
        }

        private void CheckPadBelowBall()
        {
            Bounds bounds = ballCollider.bounds;
            if (Physics.Raycast(bounds.center, Vector3.down, out RaycastHit hit,
                    bounds.extents.y + groundTolerance,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) == false)
                return;

            // 윗면을 맞혔을 때만 타이머를 시작한다.
            if (hit.normal.y < 0.9f)
                return;

            foreach (PadData pad in pads)
            {
                if (hit.collider.attachedRigidbody != pad.body)
                    continue;

                if (pad.isCounting == false && pad.isFalling == false)
                {
                    pad.isCounting = true;
                    pad.elapsed = 0f;
                }

                // 한번 시작한 타이머는 공이 떠나도 계속 진행한다.
                return;
            }
        }

        private LineRenderer CreateWarningLine(BoxCollider padCollider)
        {
            LineRenderer line = padCollider.GetComponent<LineRenderer>();
            if (line == null)
                line = padCollider.gameObject.AddComponent<LineRenderer>();

            line.sharedMaterial = warningMaterial;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 4;
            line.widthMultiplier = 1f;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.startColor = Color.red;
            line.endColor = Color.red;
            line.alignment = LineAlignment.View;
            line.numCornerVertices = 2;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;

            Vector3 center = padCollider.center;
            Vector3 halfSize = padCollider.size * 0.5f;
            float topY = center.y + halfSize.y + 0.02f;

            line.SetPosition(0, new Vector3(center.x - halfSize.x, topY, center.z - halfSize.z));
            line.SetPosition(1, new Vector3(center.x - halfSize.x, topY, center.z + halfSize.z));
            line.SetPosition(2, new Vector3(center.x + halfSize.x, topY, center.z + halfSize.z));
            line.SetPosition(3, new Vector3(center.x + halfSize.x, topY, center.z - halfSize.z));
            return line;
        }

        private void RestorePad(PadData pad, bool isActive)
        {
            if (pad.body.isKinematic == false)
            {
                pad.body.linearVelocity = Vector3.zero;
                pad.body.angularVelocity = Vector3.zero;
            }

            pad.body.isKinematic = true;
            pad.body.useGravity = false;

            pad.body.position = pad.startPosition;
            pad.body.rotation = pad.startRotation;

            pad.elapsed = 0f;
            pad.isCounting = false;
            pad.isFalling = false;
            pad.line.enabled = false;

            pad.body.gameObject.SetActive(isActive);
        }

        // 공이 떨어지면 모든 패드를 복구하고 마지막 샷 위치로 돌아간다.
        public void RespawnBall()
        {
            ResetPads();
            ball.ReturnToLasShotPosition();
        }

        // 하트 소모와 전체 게임 재시작 모두 이 함수를 사용한다.
        public void ResetPads()
        {
            foreach (PadData pad in pads)
                RestorePad(pad, true);
        }
    }
}
