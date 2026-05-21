using UnityEngine;

public class BambooEnd : MonoBehaviour
{
    public bool IsGrabbed { get; private set; }
    public PlayerMovement HeldByPlayer { get; private set; }

    private Bamboo bamboo;

    private void Awake()
    {
        bamboo = GetComponentInParent<Bamboo>();
        if (bamboo == null) Debug.LogError("BambooEnd has no Bamboo in parent!", this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsGrabbed || !collision.CompareTag("Player")) return;

        PlayerMovement player = collision.GetComponent<PlayerMovement>();
        if (player == null) { Debug.LogError("Player-tagged object missing PlayerMovement!", collision.gameObject); return; }

        IsGrabbed = true;
        HeldByPlayer = player;
        bamboo.OnEndGrabbed(player);
    }

    public void ResetEnd()
    {
        IsGrabbed = false;
        HeldByPlayer = null;
    }
}
