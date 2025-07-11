using UnityEngine;

public class Bomb : MonoBehaviour
{
    public bool isActive = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered: " + other.name); // Debug log

        if (other.CompareTag("Player") && !isActive)
        {
            isActive = true;
            Debug.Log("Player entered bomb range"); // Confirm trigger match
            FindFirstObjectByType<BombUI>().ActivateBomb(this.transform);
        }
    }
}
