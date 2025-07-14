using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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

    private float timeLeft = 60f;
    private bool timerActive = false;
    private Transform currentBomb;

    private int correctAnswer;
    private string correctTextAnswer;
    private bool isMathQuestion;

    private int difficultyLevel = 1;
    private int correctStreak = 0;

    private PlayerController playerMovement;

    private List<(string question, string answer)> historyQuestions = new List<(string, string)>
    {
        ("Who was the first President of the Philippines?", "Emilio Aguinaldo"),
        ("When did the Philippines gain independence from Spain?", "1898"),
        ("What was the name of the Filipino revolutionary group led by Andres Bonifacio?", "Kataas-taasang, Kagalang-galangang Katipunan"),
        ("Where was Jose Rizal executed?", "Luneta"),
        ("What year was the EDSA People Power Revolution?", "1986"),
        ("Who was the Filipino hero known for the phrase 'What is death to me now'?", "Jose Rizal"),
        ("What is the longest-running revolt in Philippine history?", "Dagohoy Rebellion"),
        ("Who was the first female president of the Philippines?", "Corazon Aquino")
    };

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
        GenerateQuestion();

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

        Time.timeScale = 0f;
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);
    }

    void Update()
    {
        if (!timerActive) return;

        timeLeft -= Time.unscaledDeltaTime;
        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0)
        {
            Explode();
        }

        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            CheckAnswer();
        }
    }

    public void CheckAnswer()
    {
        string userInput = inputField.text.Trim();

        if (isMathQuestion)
        {
            if (int.TryParse(userInput, out int result) && result == correctAnswer)
                Defuse();
            else
                Explode();
        }
        else
        {
            if (userInput.ToLower() == correctTextAnswer.ToLower())
                Defuse();
            else
                Explode();
        }
    }

    IEnumerator DelayedCloseUI()
    {
        yield return new WaitForSecondsRealtime(feedbackFadeTime);
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

        equationText.text = "You Died!";
        timerTextGO.SetActive(false);
        inputFieldGO.SetActive(false);
        submitButtonGO.SetActive(false);
        restartButtonGO.SetActive(true);

        panel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

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

        Time.timeScale = 1f;

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
            t += Time.unscaledDeltaTime;
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

    void GenerateQuestion()
    {
        if (Random.value <= 0.7f)
        {
            isMathQuestion = true;
            GenerateMath();
        }
        else
        {
            isMathQuestion = false;
            GenerateHistory();
        }
    }

    void GenerateMath()
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
    }

    void GenerateHistory()
    {
        int index = Random.Range(0, historyQuestions.Count);
        equationText.text = historyQuestions[index].question;
        correctTextAnswer = historyQuestions[index].answer;
    }
}
