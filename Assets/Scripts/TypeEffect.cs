using System.Collections;
using UnityEngine;
using TMPro;
using KoreanTyper;

public class TypeEffect : MonoBehaviour
{
    // �ʴ� ����� ���� ��
    public int CharPerSeconds;
    // Ŀ�� ���� ������Ʈ
    public GameObject Cursor;

    // ��� �ð�
    WaitForSeconds waitTime;
    // ���� �ؽ�Ʈ
    string originText;
    // TMP_Text ������Ʈ ����
    TMP_Text text;
    // AudioSource ������Ʈ ����
    AudioSource audioSource;
    // Ÿ���� �� ���θ� ��Ÿ���� �Ӽ�
    public bool isTyping { get; private set; }

    // MonoBehaviour�� Awake() �޼���. ������Ʈ�� Ȱ��ȭ�� �� ȣ��˴ϴ�.
    private void Awake()
    {
        // TMP_Text ������Ʈ�� ������
        text = GetComponent<TMP_Text>();
        // AudioSource ������Ʈ�� ������
        audioSource = GetComponent<AudioSource>();
        // ������� ������ ����
        audioSource.loop = true;
        // �ʴ� ����� ���� ���� ���� ��� �ð� ����
        waitTime = new WaitForSeconds(1f / CharPerSeconds);

        isTyping = true;
    }

    // �޽����� �����ϴ� �޼���
    public void SetMsg(string msg)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TypeEffect�� ���� ������Ʈ�� ��Ȱ��ȭ �����Դϴ�.");
            return;
        }

        // ���� �ؽ�Ʈ ����
        originText = msg;
        // �ؽ�Ʈ �ʱ�ȭ
        text.text = "";
        // Ŀ�� ��Ȱ��ȭ
        Cursor.SetActive(false);
        // Ÿ���� ������ ����
        isTyping = true;
        // ��� �ڷ�ƾ ����
        StopAllCoroutines();
        // Ÿ���� �ڷ�ƾ ����
        StartCoroutine(TypingRoutine());
    }

    IEnumerator TypingRoutine()
    {
        int typingLength = originText.GetTypingLength();

        // ����� ���
        audioSource.Play();

        // �ؽ�Ʈ�� �� ���ھ� ���
        for (int index = 0; index < typingLength; index++)
        {
            text.text = originText.Typing(index);
            // ��� �ð���ŭ ���
            yield return waitTime;
        }

        // ��ü �ؽ�Ʈ ��� �� ȿ�� ����
        text.text = originText;
        EffectEnd();
    }

    void EffectEnd()
    {
        isTyping = false;
        // ������� ��� ���̸� ����
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Ŀ���� �����ϸ� Ȱ��ȭ
        if (Cursor != null)
        {
            Cursor.SetActive(true);
        }
    }

    public void CompleteTyping()
    {
        if (isTyping)
        {
            // ��� �ڷ�ƾ ����
            StopAllCoroutines();

            // ��ü �ؽ�Ʈ ���
            text.text = originText;
            // ȿ�� ����
            EffectEnd();
        }
    }

}
