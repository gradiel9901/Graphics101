using UnityEngine;

public class Bomb : MonoBehaviour
{
    public bool isActive = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered: " + other.name);

        if (other.CompareTag("Player") && !isActive)
        {
            isActive = true;
            Debug.Log("Player entered bomb range");
            FindFirstObjectByType<BombUI>().ActivateBomb(this.transform);
        }
    }
}
