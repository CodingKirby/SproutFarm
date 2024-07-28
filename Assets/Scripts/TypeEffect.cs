using System.Collections;
using UnityEngine;
using TMPro;
using KoreanTyper;

public class TypeEffect : MonoBehaviour
{
    // 초당 출력할 문자 수
    public int CharPerSeconds;
    // 커서 게임 오브젝트
    public GameObject Cursor;

    // 대기 시간
    WaitForSeconds waitTime;
    // 원본 텍스트
    string originText;
    // TMP_Text 컴포넌트 참조
    TMP_Text text;
    // AudioSource 컴포넌트 참조
    AudioSource audioSource;
    // 타이핑 중 여부를 나타내는 속성
    public bool isTyping { get; private set; }

    // MonoBehaviour의 Awake() 메서드. 오브젝트가 활성화될 때 호출됩니다.
    private void Awake()
    {
        // TMP_Text 컴포넌트를 가져옴
        text = GetComponent<TMP_Text>();
        // AudioSource 컴포넌트를 가져옴
        audioSource = GetComponent<AudioSource>();
        // 오디오를 루프로 설정
        audioSource.loop = true;
        // 초당 출력할 문자 수에 따른 대기 시간 설정
        waitTime = new WaitForSeconds(1f / CharPerSeconds);

        isTyping = true;
    }

    // 메시지를 설정하는 메서드
    public void SetMsg(string msg)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("TypeEffect의 게임 오브젝트가 비활성화 상태입니다.");
            return;
        }

        // 원본 텍스트 설정
        originText = msg;
        // 텍스트 초기화
        text.text = "";
        // 커서 비활성화
        Cursor.SetActive(false);
        // 타이핑 중으로 설정
        isTyping = true;
        // 모든 코루틴 중지
        StopAllCoroutines();
        // 타이핑 코루틴 시작
        StartCoroutine(TypingRoutine());
    }

    IEnumerator TypingRoutine()
    {
        int typingLength = originText.GetTypingLength();

        // 오디오 재생
        audioSource.Play();

        // 텍스트를 한 글자씩 출력
        for (int index = 0; index < typingLength; index++)
        {
            text.text = originText.Typing(index);
            // 대기 시간만큼 대기
            yield return waitTime;
        }

        // 전체 텍스트 출력 후 효과 종료
        text.text = originText;
        EffectEnd();
    }

    void EffectEnd()
    {
        isTyping = false;
        // 오디오가 재생 중이면 중지
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // 커서가 존재하면 활성화
        if (Cursor != null)
        {
            Cursor.SetActive(true);
        }
    }

    public void CompleteTyping()
    {
        if (isTyping)
        {
            // 모든 코루틴 중지
            StopAllCoroutines();

            // 전체 텍스트 출력
            text.text = originText;
            // 효과 종료
            EffectEnd();
        }
    }

}
