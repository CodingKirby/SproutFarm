using UnityEngine;
using Pathfinding;
using System.Collections;
using UnityEngine.Tilemaps;
using System.IO;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(AIPath))]
[RequireComponent(typeof(AIDestinationSetter))]
[RequireComponent(typeof(AudioSource))]
public class Animal : MonoBehaviour
{
    public float baseSpeed = 3.0f;
    public float followDistance = 2.0f;
    public float fleeDistance = 3.0f;
    public float randomMoveRadius = 5.0f;
    public float activationRadius = 5.0f; // ����� Ȱ��ȭ �ݰ�
    public float maxVolume = 1.0f; // �ִ� ����
    public float minVolume = 0.1f; // �ּ� ����
    public AudioClip capturedSound; // ������ ������ �� ȿ����
    public AudioClip capturedInSound; // ��Ÿ�� ������ �� �� ȿ����
    public AudioClip escapingSound;
    public AudioClip surprisedSound;

    private AIPath aiPath;
    private Seeker seeker;
    private Rigidbody2D rigid;
    private Collider2D collider2d;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AIDestinationSetter destinationSetter;
    private AudioSource mainAudioSource; // ���� AudioSource
    private AudioSource capturedAudioSource; // capturedSound ����� AudioSource
    private AudioSource capturedInAudioSource; // capturedInSound ����� AudioSource
    private AudioSource escapingSoundAudioSource;
    private AudioSource surprisedSoundAudioSource;

    public bool isCaptured = true;
    public bool isFollowing = false;

    private Vector2 escapeTarget;
    private Vector2 previousPosition;

    private static int animalCount = 0;

    private Transform playerTransform;
    private TilemapCollider2D[] fenceColliders;

    void Awake()
    {
        InitializeComponents();
        SetSortingOrder();
        aiPath.maxSpeed = baseSpeed;
        aiPath.canMove = false;
        destinationSetter.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Animal");
    }

    void Start()
    {
        isFollowing = false;

        FindPlayerTransform();
        FindFenceColliders();
        previousPosition = rigid.position;
        StartCoroutine(RandomMovement());
    }

    void Update()
    {
        HandleMovement();
        HandleAnimation();
        CheckPlayerDistance(); // �÷��̾���� �Ÿ� üũ
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartFollowingPlayer();
        }
        else if (collision.CompareTag("Area"))
        {
            Physics2D.IgnoreCollision(collider2d, collision, true);
        }
    }

    private void InitializeComponents()
    {
        rigid = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        seeker = GetComponent<Seeker>();
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        mainAudioSource = GetComponent<AudioSource>(); // ���� AudioSource �ʱ�ȭ

        // ȿ���� ����� AudioSource �߰� �� �ʱ�ȭ
        capturedAudioSource = gameObject.AddComponent<AudioSource>();
        capturedAudioSource.playOnAwake = false;

        capturedInAudioSource = gameObject.AddComponent<AudioSource>();
        capturedInAudioSource.playOnAwake = false;

        escapingSoundAudioSource = gameObject.AddComponent<AudioSource>();
        escapingSoundAudioSource.playOnAwake = false;

        surprisedSoundAudioSource = gameObject.AddComponent<AudioSource>();
        surprisedSoundAudioSource.playOnAwake = false;
    }

    private void SetSortingOrder()
    {
        animalCount++;
        spriteRenderer.sortingOrder = 1000 + animalCount;

        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            SpriteRenderer objSpriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (objSpriteRenderer != null && obj != gameObject && objSpriteRenderer.sortingOrder >= spriteRenderer.sortingOrder)
            {
                objSpriteRenderer.sortingOrder -= 1;
            }
        }
    }

    private void FindPlayerTransform()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player ��ü�� ���� �����ϴ�. Player �±װ� �ùٸ��� �����Ǿ����� Ȯ���ϼ���.");
        }
    }

    private void FindFenceColliders()
    {
        GameObject[] fenceObjects = GameObject.FindGameObjectsWithTag("Fence");
        fenceColliders = new TilemapCollider2D[fenceObjects.Length];
        for (int i = 0; i < fenceObjects.Length; i++)
        {
            fenceColliders[i] = fenceObjects[i].GetComponent<TilemapCollider2D>();
        }
    }

    private void HandleMovement()
    {
        if (isFollowing)
        {
            aiPath.canMove = true;
            aiPath.maxSpeed = GameManager.instance.player.baseSpeed;
            destinationSetter.target = playerTransform;
        }
        else if (!isCaptured && Vector2.Distance(transform.position, playerTransform.position) <= fleeDistance)
        {
            FleeFromPlayer();
        }
    }

    private void HandleAnimation()
    {
        Vector2 currentPosition = rigid.position;
        if (currentPosition != previousPosition)
        {
            animator.SetBool("isRunning", true);
            spriteRenderer.flipX = currentPosition.x < previousPosition.x;
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
        previousPosition = currentPosition;
    }

    private void CheckPlayerDistance()
    {
        if (playerTransform == null)
            return;

        if (isFollowing || isCaptured)
        {
            if (mainAudioSource.isPlaying)
            {
                mainAudioSource.Stop();
            }
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= activationRadius)
        {
            if (!mainAudioSource.isPlaying)
            {
                mainAudioSource.Play();
            }

            // �Ÿ� ����Ͽ� ���� ����
            float volume = Mathf.Lerp(maxVolume, minVolume, distanceToPlayer / activationRadius);
            mainAudioSource.volume = volume;
        }
        else
        {
            if (mainAudioSource.isPlaying)
            {
                mainAudioSource.Stop();
            }
        }
    }

    public void StartFollowingPlayer()
    {
        if (!isFollowing)
        {
            isFollowing = true;
            aiPath.canMove = true;
            destinationSetter.enabled = true;
            aiPath.maxSpeed = GameManager.instance.player.baseSpeed;
            destinationSetter.target = playerTransform;
            aiPath.endReachedDistance = followDistance;
            StopCoroutine(RandomMovement());

            // ȿ���� ���
            if (capturedSound != null)
            {
                capturedAudioSource.PlayOneShot(capturedSound);
            }
            else
            {
                Debug.LogWarning("capturedSound is not set.");
            }
        }
    }

    public void StopFollowingPlayer()
    {
        isFollowing = false;
        aiPath.canMove = false;
        destinationSetter.enabled = false;
        StartCoroutine(RandomMovement());
    }

    public void Escape()
    {
        escapeTarget = new Vector2(10, 10);

        isCaptured = false;
        isFollowing = false;

        transform.position = escapeTarget;
        aiPath.canMove = true;
        destinationSetter.target = null;

        // Ż�� �� ȿ���� ���
        if (escapingSound != null && surprisedSound != null)
        {
            surprisedSoundAudioSource.PlayOneShot(surprisedSound);
            //escapingSoundAudioSource.PlayOneShot(escapingSound);
        }
        else
        {
            Debug.LogWarning("escapingSound or surprisedSound is not set.");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartFollowingPlayer();
        }
    }

    private void FleeFromPlayer()
    {
        Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
        Vector2 fleePosition = (Vector2)transform.position + fleeDirection * fleeDistance;

        aiPath.destination = fleePosition;
        aiPath.canMove = true;
        aiPath.SearchPath();
    }

    private IEnumerator RandomMovement()
    {
        while (!isFollowing)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 randomPosition = (Vector2)transform.position + randomDirection * randomMoveRadius;

            aiPath.destination = randomPosition;
            aiPath.canMove = true;
            aiPath.SearchPath();

            yield return new WaitForSeconds(Random.Range(3, 7));
        }
    }

    public void CapturedIn(Vector2 position)
    {
        StopFollowingPlayer();
        isCaptured = true;
        transform.position = position; // ��ġ�� ��� ����
        aiPath.canMove = false;
        destinationSetter.target = null;

        // ��Ÿ�� ������ �� �� ȿ���� ���
        if (capturedInSound != null)
        {
            capturedInAudioSource.PlayOneShot(capturedInSound);
        }
        else
        {
            Debug.LogWarning("capturedInSound is not set.");
        }
    }
}
