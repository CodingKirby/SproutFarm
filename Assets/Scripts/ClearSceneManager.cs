using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearSceneManager : MonoBehaviour
{
    // 게임 오버와 게임 클리어 UI 요소에 대한 참조
    public GameObject gameOverScene;
    public GameObject gameClearScene;

    private void Start()
    {
        // PlayerPrefs에서 게임 결과를 가져옵니다. 설정되어 있지 않으면 기본값으로 0(승리)을 사용합니다.
        bool isVictory = PlayerPrefs.GetInt("IsVictory", 0) == 0;

        // 게임 결과에 따라 적절한 UI를 활성화합니다.
        if (isVictory)
        {
            gameClearScene.SetActive(true); // 게임 클리어 UI 활성화
            gameOverScene.SetActive(false); // 게임 오버 UI 비활성화
        }
        else
        {
            gameClearScene.SetActive(false); // 게임 클리어 UI 비활성화
            gameOverScene.SetActive(true); // 게임 오버 UI 활성화
        }
    }

    void Update()
    {
        // 마우스 왼쪽 버튼 클릭을 감지하거나 터치 입력을 감지합니다.
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭
        {
            RestartGame(); // 게임 재시작
        }
        else if (Input.touchCount > 0) // 터치 입력 감지
        {
            RestartGame(); // 게임 재시작
        }
    }

    // 게임을 재시작하는 메서드
    void RestartGame()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single); // GameScene을 로드하여 게임 재시작
    }
}
