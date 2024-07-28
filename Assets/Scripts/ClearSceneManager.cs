using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class ClearSceneManager : MonoBehaviour
{
    // 게임 오버와 게임 클리어 UI 요소에 대한 참조
    public GameObject gameOverScene;
    public GameObject gameClearScene;

    // 게임 오버 및 게임 클리어 타일맵에 대한 참조
    public Grid gameOverGrid;
    public Grid gameClearGrid;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // PlayerPrefs에서 게임 결과를 가져옵니다. 설정되어 있지 않으면 기본값으로 0(승리)을 사용합니다.
        bool isVictory = PlayerPrefs.GetInt("IsVictory", 0) == 0;

        // 게임 결과에 따라 적절한 UI와 타일맵을 활성화합니다.
        if (isVictory)
        {
            gameClearScene.SetActive(true); // 게임 클리어 UI 활성화
            gameOverScene.SetActive(false); // 게임 오버 UI 비활성화

            // 게임 클리어 그리드 활성화
            gameClearGrid.gameObject.SetActive(true);
            gameOverGrid.gameObject.SetActive(false);

            AdjustGridToScreen(gameClearGrid);
        }
        else
        {
            gameClearScene.SetActive(false); // 게임 클리어 UI 비활성화
            gameOverScene.SetActive(true); // 게임 오버 UI 활성화

            // 게임 오버 그리드 활성화
            gameClearGrid.gameObject.SetActive(false);
            gameOverGrid.gameObject.SetActive(true);

            AdjustGridToScreen(gameOverGrid);
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

    // 그리드를 화면 중앙에 맞추고 창 크기에 맞게 조절하는 메서드
    void AdjustGridToScreen(Grid grid)
    {
        // 그리드의 모든 타일맵의 경계를 가져옵니다.
        Bounds combinedBounds = new Bounds();
        Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();

        foreach (var tilemap in tilemaps)
        {
            if (combinedBounds.size == Vector3.zero)
            {
                combinedBounds = tilemap.localBounds;
            }
            else
            {
                combinedBounds.Encapsulate(tilemap.localBounds);
            }
        }

        // 그리드의 중심을 카메라의 위치로 이동시킵니다.
        Vector3 gridCenter = combinedBounds.center;
        Vector3 cameraPosition = mainCamera.transform.position;
        grid.transform.position = new Vector3(cameraPosition.x - gridCenter.x, cameraPosition.y - gridCenter.y, grid.transform.position.z);

        // 카메라의 orthographic size를 조정하여 그리드가 화면에 꽉 차도록 합니다.
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float tilemapAspect = combinedBounds.size.x / combinedBounds.size.y;

        if (screenAspect >= tilemapAspect)
        {
            mainCamera.orthographicSize = combinedBounds.size.y / 2;
        }
        else
        {
            mainCamera.orthographicSize = combinedBounds.size.x / 2 / screenAspect;
        }
    }
}
