using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearSceneManager : MonoBehaviour
{
    // ���� ������ ���� Ŭ���� UI ��ҿ� ���� ����
    public GameObject gameOverScene;
    public GameObject gameClearScene;

    private void Start()
    {
        // PlayerPrefs���� ���� ����� �����ɴϴ�. �����Ǿ� ���� ������ �⺻������ 0(�¸�)�� ����մϴ�.
        bool isVictory = PlayerPrefs.GetInt("IsVictory", 0) == 0;

        // ���� ����� ���� ������ UI�� Ȱ��ȭ�մϴ�.
        if (isVictory)
        {
            gameClearScene.SetActive(true); // ���� Ŭ���� UI Ȱ��ȭ
            gameOverScene.SetActive(false); // ���� ���� UI ��Ȱ��ȭ
        }
        else
        {
            gameClearScene.SetActive(false); // ���� Ŭ���� UI ��Ȱ��ȭ
            gameOverScene.SetActive(true); // ���� ���� UI Ȱ��ȭ
        }
    }

    void Update()
    {
        // ���콺 ���� ��ư Ŭ���� �����ϰų� ��ġ �Է��� �����մϴ�.
        if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư Ŭ��
        {
            RestartGame(); // ���� �����
        }
        else if (Input.touchCount > 0) // ��ġ �Է� ����
        {
            RestartGame(); // ���� �����
        }
    }

    // ������ ������ϴ� �޼���
    void RestartGame()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single); // GameScene�� �ε��Ͽ� ���� �����
    }
}
