using UnityEngine;
using Random = UnityEngine.Random;

public class BackgroundObjectMovement : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    
    [SerializeField]
    private float minSpeed = 5f;

    [SerializeField]
    private float maxSpeed = 8f;
    
    [SerializeField]
    private float minHeight;

    [SerializeField]
    private float maxHeight;

    [SerializeField]
    private float borderDistance;

    private float speed;
    private float spriteWidth;

    private float RandomPosX => Random.Range(-borderDistance, borderDistance);
    private float RandomPosY => Random.Range(minHeight, maxHeight);
    private float RandomSpeed => Random.Range(minSpeed, maxSpeed);

    private void Awake()
    {
        transform.position = new Vector3(RandomPosX, RandomPosY);
        spriteWidth = spriteRenderer.bounds.size.x;
        speed = RandomSpeed;
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        if (transform.position.x >= borderDistance + spriteWidth / 2f) {
            transform.position = new Vector3(-borderDistance - spriteWidth / 2f, RandomPosY);
            speed = RandomSpeed;
        }
    }
}