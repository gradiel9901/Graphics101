using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BombUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text equationText;
    public TMP_InputField inputField;
    public TMP_Text timerText;
    public Button submitButton;

    public TMP_Text feedbackText;
    public GameObject timerTextGO;
    public GameObject inputFieldGO;
    public GameObject submitButtonGO;
    public GameObject restartButtonGO;

    public float feedbackFadeTime = 1.5f;

    private int correctAnswer;
    private float timeLeft = 60f;
    private bool timerActive = false;
    private Transform currentBomb;

    private int difficultyLevel = 1;
    private int correctStreak = 0;

    private PlayerController playerMovement;

    void Start()
    {
        panel.SetActive(false);
        feedbackText.text = "";
        SetTextAlpha(feedbackText, 0f);
        restartButtonGO.SetActive(false);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerController>();
        }
    }

    public void ActivateBomb(Transform bomb)
    {
        currentBomb = bomb;
        GenerateEquation();

        inputField.text = "";
        timeLeft = 60f;

        panel.SetActive(true);
        timerActive = true;

        timerTextGO.SetActive(true);
        inputFieldGO.SetActive(true);
        submitButtonGO.SetActive(true);
        restartButtonGO.SetActive(false);

        inputField.Select();
        inputField.ActivateInputField();

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false); // Use the new movement toggle method
    }

    void Update()
    {
        if (!timerActive) return;

        timeLeft -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0)
        {
            Explode();
        }

        // 🧠 ENTER key to submit
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CheckAnswer();
        }
    }

    public void CheckAnswer()
    {
        if (int.TryParse(inputField.text, out int result))
        {
            if (result == correctAnswer)
            {
                Defuse();
            }
            else
            {
                Explode();
            }
        }
    }

    IEnumerator DelayedCloseUI()
    {
        yield return new WaitForSeconds(feedbackFadeTime);
        CloseUI();
    }

    void Defuse()
    {
        ShowFeedback("Correct!");
        Destroy(currentBomb.gameObject);

        correctStreak++;
        if (correctStreak % 3 == 0)
        {
            difficultyLevel++;
            Debug.Log("Difficulty increased to: " + difficultyLevel);
        }

        StartCoroutine(DelayedCloseUI());
    }

    void Explode()
    {
        Destroy(currentBomb.gameObject);
        timerActive = false;

        equationText.text = "Game Over!";
        timerTextGO.SetActive(false);
        inputFieldGO.SetActive(false);
        submitButtonGO.SetActive(false);
        restartButtonGO.SetActive(true);

        panel.SetActive(true);
    }

    public void RestartGame()
    {
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void CloseUI()
    {
        panel.SetActive(false);
        timerActive = false;

        restartButtonGO.SetActive(false);
        submitButtonGO.SetActive(true);
        inputFieldGO.SetActive(true);
        timerTextGO.SetActive(true);
        equationText.text = "";

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);
    }

    void ShowFeedback(string message)
    {
        feedbackText.text = message;
        SetTextAlpha(feedbackText, 1f);
        StartCoroutine(FadeOutFeedback());
    }

    IEnumerator FadeOutFeedback()
    {
        float t = 0f;
        Color original = feedbackText.color;

        while (t < feedbackFadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / feedbackFadeTime);
            SetTextAlpha(feedbackText, alpha);
            yield return null;
        }

        feedbackText.text = "";
    }

    void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null) return;
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }

    void GenerateEquation()
    {
        int a, b, c;

        switch (difficultyLevel)
        {
            case 1:
                a = Random.Range(1, 10);
                b = Random.Range(1, 10);
                equationText.text = $"{a} + {b} = ";
                correctAnswer = a + b;
                break;

            case 2:
                a = Random.Range(10, 20);
                b = Random.Range(1, 10);
                equationText.text = $"{a} - {b} = ";
                correctAnswer = a - b;
                break;

            case 3:
                a = Random.Range(2, 10);
                b = Random.Range(2, 10);
                equationText.text = $"{a} × {b} = ";
                correctAnswer = a * b;
                break;

            case 4:
                b = Random.Range(2, 10);
                correctAnswer = Random.Range(2, 10);
                a = correctAnswer * b;
                equationText.text = $"{a} ÷ {b} = ";
                break;

            case 5:
                a = Random.Range(1, 10);
                b = Random.Range(1, 10);
                c = Random.Range(1, 10);
                equationText.text = $"{a} + {b} × {c} = ";
                correctAnswer = a + b * c;
                break;

            case 6:
                a = Random.Range(1, 5);
                b = Random.Range(1, 5);
                c = Random.Range(1, 5);
                equationText.text = $"({a} + {b}) × {c} = ";
                correctAnswer = (a + b) * c;
                break;

            default:
                a = Random.Range(2, 5 + difficultyLevel);
                b = Random.Range(1, 10);
                equationText.text = $"{a}² + {b} = ";
                correctAnswer = a * a + b;
                break;
        }

        Debug.Log($"Generated equation: {equationText.text} Answer: {correctAnswer}");
    }
}
