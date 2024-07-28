using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour
{
    public float stamina = 100f;
    public TMP_Text staminaText; // ���׹̳��� ǥ���� UI �ؽ�Ʈ
    public Image staminaIcon; // ���׹̳� �������� ǥ���� �̹���
    public Sprite[] staminaIcons; // �� ���׹̳� �ܰ迡 �ش��ϴ� ������ �迭

    public float maxStamina = 100f;
    public float recoveryRate = 1f; // ���׹̳� ȸ�� �ӵ� (�ʴ� ȸ����)

    public AudioClip warningSound;
    private AudioSource warningSoundAudioSource;

    private float staminaChange = 0f; // ü�� ��ȭ���� �����ϴ� ����

    private void Awake()
    {
        warningSoundAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        ApplyStaminaChange();
        UpdateStaminaDisplay();
        if (stamina == 0f)
        {
            ShowGameOver();
        }
    }

    public void DecreaseStamina(float amount)
    {
        staminaChange -= amount;
    }

    public void IncreaseStamina(float amount)
    {
        staminaChange += amount;
    }

    public void ApplyStaminaChange()
    {
        stamina = Mathf.Clamp(stamina + staminaChange, 0f, maxStamina);
        staminaChange = 0f;
    }

    private void UpdateStaminaDisplay()
    {
        staminaText.text = $"{stamina:F0}%";
        staminaIcon.sprite = staminaIcons[GetStaminaIconIndex()];
    }

    private int GetStaminaIconIndex()
    {
        if (stamina > 80f) return 0;
        if (stamina > 60f) return 1;
        if (stamina > 40f) return 2;
        if (stamina > 20f) return 3;
        if (stamina > 0f)
        {
            if (!warningSoundAudioSource.isPlaying)
            {
                warningSoundAudioSource.PlayOneShot(warningSound);
            }
            return 4;
        }

        return 5;
    }

    private void ShowGameOver()
    {
        PlayerPrefs.SetInt("IsVictory", 1); // �й�� ����
        SceneManager.LoadScene("ClearScene", LoadSceneMode.Single);
    }
}
