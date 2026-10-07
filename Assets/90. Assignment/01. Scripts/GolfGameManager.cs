using Study.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Study3D.MiniGolf
{
    public class GolfGameManager : MonoBehaviour
    {
        [Header("Golf Settings")]
        [SerializeField] private GolfBall ball;
        [SerializeField] private GolfPlayer player;

        [Header("Falling Settings")]
        [SerializeField] private float falledY = -5f;
        [SerializeField] private FallingPadManager fallingPads;

        [Header("UI Settings")]
        [SerializeField] private int startHearts = 3;
        [SerializeField] private GameObject[] hearts;
        [SerializeField] private TextMeshProUGUI textUI;

        private int currentHearts;
        private bool isGameOver;
        private bool isClear;
        private bool isStarted;
        private Vector3 startPosition;

        private void Start()
        {
            // startPosition에 저장
            startPosition = ball.transform.position;

            InitAll();
            textUI.gameObject.SetActive(true);
            SetText(true, "Press 'Enter' to start");
            UpdateHearts();
        }
        
        private void Update()
        {
            if (isGameOver)
            {
                if (SimpleInput.GetKeyDown(Key.Enter))
                {
                    InitAll();
                    StartGame();
                }
                return;
            }
            if (isClear)
            {
                ClearGame();
            }
            StartGame();
        }

        # region Initialize

        private void InitAll()
        {
            InitGameState();
            InitPlayer();
        }
        
        private void InitGameState()
        {
            fallingPads.ResetPads();
            // 공 위치 초기화
            ball.PlaceAt(startPosition);
            // 하트 개수 초기화
            currentHearts = startHearts;
            
            // 게임 진행 상태 초기화
            isClear = false;
            isGameOver = false;
            isStarted = false;

            // 하트 켜기
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].SetActive(true);
            }
        }

        private void InitPlayer()
        {
            player.enabled = false;
        }
        
        # endregion
        
        
        
        # region GameState
        
        private void BallFall()
        {
            currentHearts--;
            UpdateHearts();

            fallingPads.RespawnBall();
            
            if (currentHearts <= 0)
            {
                GameOver();
            }
        }

        private void StartGame()
        {
            if (isStarted == false && isClear != true)
            {
                if (SimpleInput.GetKeyDown(Key.Enter))
                {
                    player.enabled = true;
                    isStarted = true;
                    SetText(false);
                }
            }
            
            if (ball.transform.position.y < falledY)
            {
                BallFall();
            }
        }
        
        private void GameOver()
        {
            isGameOver = true;
            InitPlayer();
            Debug.Log("GameOver");
            SetText(true, "Game Over\nPress 'Enter' To Retry");
        }
        
        public void Goal(GolfBall ball)
        {
            isClear = true;
            Debug.Log("Goal");
            InitPlayer();
        }

        private void ClearGame()
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            if (currentScene == 0)
            {
                SetText(true, "Goal!\nPress 'Enter' To Start Next Stage");
                if (SimpleInput.GetKeyDown(Key.Enter)) SceneManager.LoadScene(1);
            }
            else
            {
                SetText(true, "Goal!\nPress 'Enter' To Quit Game");
                if (SimpleInput.GetKeyDown(Key.Enter)) Application.Quit();
            }
        }
        # endregion

        
        
        # region UGUI
        
        private void UpdateHearts()
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].SetActive(i < currentHearts);
            }
        }

        private void SetText(bool isActive, string text = "")
        {
            textUI.SetText(text);
            textUI.gameObject.SetActive(isActive);
        }

        # endregion
    }

}
