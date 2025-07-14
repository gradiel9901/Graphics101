using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform;
    public Light playerLight;
    public float blinkSpeed = 5f;
    public float detectionRange = 10f;
    public TMP_Text alertText;
    public AudioClip dangerClip;

    [Header("Death UI")]
    public GameObject deathPanel; // Assign this in Inspector

    private CharacterController controller;
    private bool canMove = true;
    private AudioSource audioSource;
    private bool isEnemyNear = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        alertText.gameObject.SetActive(false);

        if (playerLight != null)
            playerLight.intensity = 1f;

        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    void Update()
    {
        MovePlayer();
        CheckEnemyProximity();
    }

    void MovePlayer()
    {
        if (!canMove) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = camForward * v + camRight * h;
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);

            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10f * Time.deltaTime);
        }
    }

    void CheckEnemyProximity()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
                closestDistance = dist;
        }

        if (closestDistance < detectionRange)
        {
            if (!isEnemyNear)
            {
                isEnemyNear = true;
                alertText.gameObject.SetActive(true);
                alertText.text = "They are near!";

                if (dangerClip != null && !audioSource.isPlaying)
                {
                    audioSource.clip = dangerClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }

            if (playerLight != null)
                playerLight.intensity = Mathf.PingPong(Time.time * blinkSpeed, 1f);
        }
        else
        {
            if (isEnemyNear)
            {
                isEnemyNear = false;
                alertText.gameObject.SetActive(false);

                if (audioSource.isPlaying)
                    audioSource.Stop();

                if (playerLight != null)
                    playerLight.intensity = 1f;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Die();
        }
    }

    void Die()
    {
        Time.timeScale = 0f;

        if (playerLight != null)
            playerLight.enabled = false;

        if (alertText != null)
        {
            alertText.gameObject.SetActive(true);
            alertText.text = "You Died!";
        }

        if (deathPanel != null)
            deathPanel.SetActive(true);

        SetMovementEnabled(false);
    }

    public void SetMovementEnabled(bool isEnabled)
    {
        canMove = isEnabled;
    }
}
