using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [Header("Elemente UI")]
    public TMP_Text scoreTextP1;
    public TMP_Text scoreTextP2;
    public GameObject winPanel;
    public TMP_Text winMessage;

    private void Awake()
    {
        Instance = this;
    }
    
    public void UpdateScoreUI(ulong playerId, int score)
    {
        if (playerId == 0)
        {
            scoreTextP1.text = $"Player 1: {score}";
        }
        else
        {
            scoreTextP2.text = $"Player 2: {score}";
        }
    }

    public void ShowWinScreen(ulong playerId)
    {
        winPanel.SetActive(true); 
        
        string castigator = (playerId == 0) ? "PLAYER 1" : "PLAYER 2";
        winMessage.text = $"{castigator} WON THE GAME!";
        
        Time.timeScale = 0; 
    }
}
