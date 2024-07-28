using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class IntroSceneManager : MonoBehaviour
{
    public float moveDistance = 10f;
    public float duration = 7f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 1f;

    public GameObject player;
    public TMP_Text textToFollow;
    public List<Image> spritesToFollow;
    public Canvas canvas;

    private Vector3 initialTextOffset;
    private List<Vector3> initialSpriteOffsets = new List<Vector3>();
    private bool textAndSpritesStopped = false;

    public Tilemap ground;
    public Tilemap flower;

    private void Start()
    {
        ResetState();

        if (player == null)
        {
            Debug.LogError("Player reference is not set in IntroSceneManager script.");
            return;
        }

        if (textToFollow == null)
        {
            Debug.LogError("TextMeshPro reference is not set in IntroSceneManager script.");
            return;
        }

        if (spritesToFollow == null || spritesToFollow.Count == 0)
        {
            Debug.LogError("Sprite (Image) references are not set in IntroSceneManager script.");
            return;
        }

        initialTextOffset = textToFollow.transform.position - Camera.main.WorldToScreenPoint(player.transform.position);

        foreach (var sprite in spritesToFollow)
        {
            initialSpriteOffsets.Add(sprite.transform.position - textToFollow.transform.position);
        }

        StartCoroutine(MovePlayerAcrossScreen());
        StartCoroutine(MoveUIWithPlayer());
        StartCoroutine(MoveTilemapsUpwards());
    }

    private void ResetState()
    {
        textAndSpritesStopped = false;
        initialSpriteOffsets.Clear();
    }

    private IEnumerator MovePlayerAcrossScreen()
    {
        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = startPosition + new Vector3(moveDistance, 0, 0);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            float newX = Mathf.Lerp(startPosition.x, targetPosition.x, progress);
            float newY = startPosition.y + Mathf.Sin(progress * Mathf.PI * 2 * waveFrequency) * waveAmplitude;
            player.transform.position = new Vector3(newX, newY, startPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPosition;

        SceneManager.LoadScene("GameScene");
    }

    private IEnumerator MoveUIWithPlayer()
    {
        float elapsedTime = 0f;
        float canvasCenterX = canvas.GetComponent<RectTransform>().rect.width / 2;

        while (elapsedTime < 2f)
        {
            Vector3 playerWorldPosition = player.transform.position;
            Vector3 playerScreenPosition = Camera.main.WorldToScreenPoint(playerWorldPosition);

            if (!textAndSpritesStopped)
            {
                textToFollow.transform.position = playerScreenPosition + initialTextOffset;

                if (Mathf.Abs(textToFollow.transform.position.x - canvasCenterX) < 1f)
                {
                    textToFollow.transform.position = new Vector3(canvasCenterX, textToFollow.transform.position.y, textToFollow.transform.position.z);

                    for (int i = 0; i < spritesToFollow.Count; i++)
                    {
                        spritesToFollow[i].transform.position = textToFollow.transform.position + initialSpriteOffsets[i];
                    }

                    textAndSpritesStopped = true;
                }
                else
                {
                    for (int i = 0; i < spritesToFollow.Count; i++)
                    {
                        spritesToFollow[i].transform.position = textToFollow.transform.position + initialSpriteOffsets[i];
                    }
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (!textAndSpritesStopped)
        {
            textToFollow.transform.position = new Vector3(canvasCenterX, textToFollow.transform.position.y, textToFollow.transform.position.z);

            for (int i = 0; i < spritesToFollow.Count; i++)
            {
                spritesToFollow[i].transform.position = textToFollow.transform.position + initialSpriteOffsets[i];
            }
        }
    }

    private IEnumerator MoveTilemapsUpwards()
    {
        if (!textAndSpritesStopped)
            yield return new WaitForSeconds(2.5f);

        Vector3 groundStartPos = ground.transform.position;
        Vector3 flowerStartPos = flower.transform.position;
        Vector3 targetPos = groundStartPos + new Vector3(0, 5f, 0);
        float elapsedTime = 0f;
        float moveDuration = 2f;

        while (elapsedTime < moveDuration)
        {
            float progress = elapsedTime / moveDuration;
            ground.transform.position = Vector3.Lerp(groundStartPos, targetPos, progress);
            flower.transform.position = Vector3.Lerp(flowerStartPos, targetPos, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ground.transform.position = targetPos;
        flower.transform.position = targetPos;
    }
}
