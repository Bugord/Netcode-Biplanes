using UnityEngine;

public class EdgeLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) {
            var playerPos = col.transform.position;
            var newPosX = playerPos.x * -1 + (playerPos.x > 0 ? 0.1f : -0.1f);
            col.transform.position = new Vector2(newPosX, playerPos.y);
        }
    }
}
