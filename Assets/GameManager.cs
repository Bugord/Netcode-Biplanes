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

        Screen.SetResolution(512, 512, FullScreenMode.Windowed);
    }

    public void StartSession()
    {
        gameSession.StartSession();
    }
}