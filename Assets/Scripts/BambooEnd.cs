using UnityEngine;

public class BambooEnd : MonoBehaviour
{
    public bool isGrabbed = false;
    public PlayerMovement HeldByPlayer { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || isGrabbed) return;

        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player == null) { Debug.LogError("PlayerMovement not found on collider!"); return; }

        Bamboo bamboo = transform.parent.GetComponent<Bamboo>();
        if (bamboo == null) { Debug.LogError("Bamboo not found on parent!"); return; }

        isGrabbed = true;
        HeldByPlayer = player;
        bamboo.SetGrabbedEnd(player);
    }

    public void ResetEnd()
    {
        isGrabbed = false;
        HeldByPlayer = null;
    }
}
