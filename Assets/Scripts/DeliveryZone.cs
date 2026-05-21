using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bamboo bamboo = collision.GetComponent<Bamboo>();
        if (bamboo == null) return;

        GameManager.Instance?.OnBambooDelivered(bamboo.PointValue);
        bamboo.UnlinkPlayers();
        Destroy(bamboo.gameObject);
    }
}
