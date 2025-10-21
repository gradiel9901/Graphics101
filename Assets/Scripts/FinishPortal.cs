using UnityEngine;

public class FinishPortal : MonoBehaviour
{
    private GameObject winUI;

    public void AssignWinUI(GameObject ui)
    {
        winUI = ui;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (winUI != null)
                winUI.SetActive(true);
            else
                Debug.LogWarning("Win UI not assigned to FinishPortal!");
        }
    }
}
