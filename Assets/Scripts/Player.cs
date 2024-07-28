using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // GameManager �� DayNightCycle ����
    public GameManager gameManager;
    public DayNightCycle dayNightCycle;

    // �̵� ���� ������
    private Rigidbody2D rigidBody;
    public float baseSpeed;
    public float boostedSpeed;
    private float currentSpeed;
    public Vector2 inputVector;
    private Vector3 directionVector;

    // ��������Ʈ �� �ִϸ��̼� ���� ������
    private SpriteRenderer spriteRenderer;
    public Animator animator;

    // ��ĵ�� ������Ʈ ����
    private GameObject scanObject;
    // ȭ��ǥ �̹��� ��ü
    public Image arrowSpriteImage;

    // �̵� ȿ���� ���� ������
    private AudioSource movementAudioSource;
    public AudioClip movementSound;

    // ��ƼŬ �ý��� ����
    private ParticleSystem movementParticleSystem;

    // �÷��̾� ���� ����
    private PlayerStatus playerStatus;

    public Tilemap roofTilemap;



    // ������Ʈ �ʱ�ȭ
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        baseSpeed = 3f;
        boostedSpeed = 4f;
        currentSpeed = baseSpeed;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // ȭ��ǥ �̹��� ��ü �ʱ�ȭ
        if (arrowSpriteImage != null)
        {
            arrowSpriteImage.gameObject.SetActive(false);
        }

        // �̵� ȿ������ AudioSource �ʱ�ȭ
        movementAudioSource = gameObject.AddComponent<AudioSource>();
        movementAudioSource.clip = movementSound;
        movementAudioSource.loop = true;
        movementAudioSource.playOnAwake = false;

        // ��ƼŬ �ý��� �ʱ�ȭ
        movementParticleSystem = GetComponentInChildren<ParticleSystem>();
        if (movementParticleSystem != null)
        {
            var emission = movementParticleSystem.emission;
            emission.enabled = false;
        }

        // PlayerStatus ���� �ʱ�ȭ
        playerStatus = FindObjectOfType<PlayerStatus>();
    }

    // �� ������ ȣ��Ǵ� ������Ʈ �޼���
    void Update()
    {
        UpdateDirectionVector(); // �̵� ���� ���� ������Ʈ
        HandleDialogueInput(); // ��ȭ �Է� ó��
        UpdatePlayerSpeed(); // �÷��̾� �ӵ� ������Ʈ
        UpdateArrowDirection(); // ȭ��ǥ ���� ������Ʈ
        HandleMovementSound(); // �̵� ȿ���� ó��
        HandleStaminaRecovery(); // ���¹̳� ȸ�� ó��
    }

    // ���� ������Ʈ �޼���
    private void FixedUpdate()
    {
        if (gameManager == null || dayNightCycle == null)
        {
            Debug.LogError("GameManager �Ǵ� DayNightCycle�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (gameManager.IsInitialDialogue()) // ó�� ���̾�α׸� ��� ���� ���� �������� ����
            return;

        MovePlayer(); // �÷��̾� �̵� ó��
        DetectObject(); // ��ü ����
    }

    // �̵� �Է� ó��
    private void OnMove(InputValue value)
    {
        inputVector = value.Get<Vector2>();
    }

    // �浹 ó��
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Animal"))
        {
            Animal animal = collision.gameObject.GetComponent<Animal>();
            animal?.StartFollowingPlayer();
        }
    }

    // Ʈ���� �浹 ó��
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Animal animal = collision.gameObject.GetComponent<Animal>();
            if (animal != null && animal.isFollowing)
            {
                animal.StopFollowingPlayer();
                // ��Ÿ�� ��ġ�� �����ϰ� ��Ÿ�� ������ �ֱ�
                Vector2 fencePosition = new Vector2(4, 4); // ��Ÿ���� �߽� ��ġ
                animal.CapturedIn(fencePosition);
            }
        }
    }

    // �ִϸ��̼� ������Ʈ
    private void LateUpdate()
    {
        UpdateAnimation();
    }

    // �̵� ���� ���� ������Ʈ
    private void UpdateDirectionVector()
    {
        if (inputVector.y == 1) directionVector = Vector3.up;
        else if (inputVector.y == -1) directionVector = Vector3.down;
        else if (inputVector.x == 1) directionVector = Vector3.right;
        else if (inputVector.x == -1) directionVector = Vector3.left;
    }

    // ��ȭ �Է� ó��
    private void HandleDialogueInput()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (Input.GetButtonDown("Jump") && scanObject != null)
        {
            gameManager.ScanAction(scanObject);
        }
    }

    // �÷��̾� �ӵ� ������Ʈ
    private void UpdatePlayerSpeed()
    {
        if (dayNightCycle == null)
        {
            Debug.LogError("DayNightCycle�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        float currentBaseSpeed = Input.GetKey(KeyCode.LeftShift) ? boostedSpeed : baseSpeed;

        int followingAnimalsCount = GetFollowingAnimalsCount();
        currentSpeed = Mathf.Max(2.5f, currentBaseSpeed - 0.1f * followingAnimalsCount);

        dayNightCycle.UpdatePlayerSpeed(currentSpeed);
    }

    // �÷��̾� �̵� ó��
    private void MovePlayer()
    {
        Vector2 nextVector = inputVector.normalized * currentSpeed * Time.fixedDeltaTime;
        rigidBody.MovePosition(rigidBody.position + nextVector);
    }

    // ��ü ���� ó��
    private void DetectObject()
    {
        Debug.DrawRay(rigidBody.position, directionVector * 0.7f, new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(rigidBody.position, directionVector, 0.7f, LayerMask.GetMask("Object"));

        if (rayHit.collider != null)
        {
            scanObject = rayHit.collider.gameObject;
            ObjectData objectData = scanObject.GetComponent<ObjectData>();

            if (objectData != null && objectData.id >= 0 && gameManager != null)
            {
                if (gameManager.HasFollowingAnimals() && !gameManager.dialoguePanel.activeSelf)
                {
                    gameManager.ShowDialogue(objectData.id);
                }

                if (objectData.id == 1 && Input.GetKeyDown(KeyCode.Space))
                {
                    foreach (GameObject animal in GameObject.FindGameObjectsWithTag("Animal"))
                    {
                        Animal animalScript = animal.GetComponent<Animal>();
                        if (animalScript != null && animalScript.isFollowing)
                        {
                            Vector2 fencePosition = new Vector2(4, 4); // ��Ÿ���� �߽� ��ġ
                            animalScript.CapturedIn(fencePosition);
                        }
                    }
                }
            }
        }
        else
        {
            scanObject = null;
            if (gameManager != null)
            {
                gameManager.CloseScanAction();
            }
        }
    }

    // �ִϸ��̼� ������Ʈ
    private void UpdateAnimation()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        if (gameManager.dialoguePanel.activeSelf) return;

        if (inputVector.magnitude > 0)
        {
            if (inputVector.y > 0) animator.SetTrigger("MoveUp");
            else if (inputVector.y < 0) animator.SetTrigger("MoveDown");
            else if (inputVector.x > 0) animator.SetTrigger("MoveRight");
            else if (inputVector.x < 0) animator.SetTrigger("MoveLeft");
        }
        else
        {
            animator.SetTrigger("Idle");
        }
    }

    // �̵� ȿ���� ó��
    private void HandleMovementSound()
    {
        if (gameManager != null && gameManager.IsInitialDialogue())
        {
            if (movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }

            if (movementParticleSystem != null)
            {
                var emission = movementParticleSystem.emission;
                emission.enabled = false;
            }

            return;
        }

        if (inputVector.magnitude > 0)
        {
            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }

            if (movementParticleSystem != null)
            {
                var emission = movementParticleSystem.emission;
                emission.enabled = true;
            }
        }
        else
        {
            if (movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }

            if (movementParticleSystem != null)
            {
                var emission = movementParticleSystem.emission;
                emission.enabled = false;
            }
        }
    }

    // ���¹̳� ȸ�� ó��
    private void HandleStaminaRecovery()
    {
        if (inputVector.magnitude == 0)
        {
            playerStatus.IncreaseStamina(playerStatus.recoveryRate * Time.deltaTime);
        }
    }

    // ȭ��ǥ ���� ������Ʈ
    private void UpdateArrowDirection()
    {
        if (arrowSpriteImage == null)
        {
            Debug.LogError("Arrow Sprite Image�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        // ���� �±׸� ���� ������Ʈ���� ã��
        GameObject[] animals = GameObject.FindGameObjectsWithTag("Animal");
        if (animals.Length == 0)
        {
            arrowSpriteImage.gameObject.SetActive(false);
            return;
        }

        // ���� ����� ������ ã��
        GameObject closestAnimal = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject animal in animals)
        {
            Animal animalScript = animal.GetComponent<Animal>();
            if (animalScript == null || animalScript.isFollowing || animalScript.isCaptured)
            {
                continue; // isFollowing ���̰ų� isCaptured ������ ������ ����
            }

            float distance = Vector2.Distance(transform.position, animal.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAnimal = animal;
            }
        }

        if (closestAnimal != null)
        {
            arrowSpriteImage.gameObject.SetActive(true);

            // ȭ��ǥ ���� ����
            Vector2 direction = closestAnimal.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            RectTransform arrowRectTransform = arrowSpriteImage.GetComponent<RectTransform>();
            if (arrowRectTransform != null)
            {
                arrowRectTransform.localRotation = Quaternion.Euler(0, 0, angle - 90); // ȭ��ǥ�� ���� ���ϵ��� -90�� ����
            }
        }
        else
        {
            arrowSpriteImage.gameObject.SetActive(true);

            // ��� ������ ������ ��� 0,0�� ����Ŵ
            Vector2 initialPosition = Vector2.zero;
            Vector2 direction = initialPosition - (Vector2)transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            RectTransform arrowRectTransform = arrowSpriteImage.GetComponent<RectTransform>();
            if (arrowRectTransform != null)
            {
                arrowRectTransform.localRotation = Quaternion.Euler(0, 0, angle - 90); // ȭ��ǥ�� ���� ���ϵ��� -90�� ����
            }
        }
    }

    // ������� ������ ���� ��ȯ�ϴ� �޼���
    private int GetFollowingAnimalsCount()
    {
        int count = 0;
        foreach (GameObject animal in GameObject.FindGameObjectsWithTag("Animal"))
        {
            Animal animalScript = animal.GetComponent<Animal>();
            if (animalScript != null && animalScript.isFollowing)
            {
                count++;
            }
        }
        return count;
    }
}
