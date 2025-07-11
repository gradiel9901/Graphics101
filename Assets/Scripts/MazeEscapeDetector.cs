using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MazeEscapeDetector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;       // The panel to show on escape
    public TMP_Text winText;          // Text that says "Congratulations..."
    public Button playAgainButton;    // Button to restart the game

    private bool hasEscaped = false;

    void Start()
    {
        // Ensure win panel is hidden at start
        if (winPanel != null)
            winPanel.SetActive(false);

        // Hook up the restart function to the button
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
