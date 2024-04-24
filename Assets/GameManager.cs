using Core;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance => instance;

    [SerializeField]
    private GameSession gameSession;

    private void Awake()
    {
        instance = this;
    }

    public void StartSession()
    {
        gameSession.StartSession();
    }
}