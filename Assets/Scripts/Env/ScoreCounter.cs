using UnityEngine;
using TMPro;

public class ScoreCounter : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private string prefix = "Score: ";
    #endregion
 
    #region Properties
    public static ScoreCounter Instance { get; private set; }
    public int Score { get; private set; }
    #endregion
 
    #region Unity Methods
    private void Awake()
    {
        Instance = this;
        UpdateText();
    }
 
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion
 
    #region Score
    public void AddPoint()
    {
        Score++;
        UpdateText();
    }
 
    private void UpdateText()
    {
        scoreText.text = prefix + Score;
    }
    #endregion

    #region Public Methods
    public void SpendPoints(int amount)
    {
        Score -= amount;
        UpdateText();
    }
    #endregion
}
