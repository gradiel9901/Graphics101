using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MazeEscapeDetector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;
    public TMP_Text winText;
    public Button playAgainButton;

    private bool hasEscaped = false;

    void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);
    }

    void OnTriggerExit(Collider other)
    {
        if (hasEscaped) return;

        if (other.CompareTag("Player"))
        {
            hasEscaped = true;
            Debug.Log("Player escaped the maze!");
            ShowWinPanel();
        }
    }

    void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (winText != null)
            winText.text = "🎉 Congratulations! You escaped the maze!";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
