using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



[RequireComponent(typeof(CanvasGroup))]
public class Level : MonoBehaviour
{
    public TextMeshProUGUI levelName;
    public TextMeshProUGUI costToPlayText;
    public TextMeshProUGUI coinsToEarnText;
    public TextMeshProUGUI bonusCoinsText;
    // public int levelNumber { get; set; }
    // public ulong costToPlay { get; set; }
    // public ulong coinsToEarn { get; set; }
    // public ulong bonusCoins { get; set; }
    // public List<EnemyData> enemies { get; set; }
    // public List<SpyData> spies { get; set; }
    // Start is called before the first frame update
    private CanvasGroup canvasGroup;
    private Toggle toggle;
	public GameLevel levelData {
		get;
		private set;
	}

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component missing from the object. Please add a CanvasGroup component to use Level functionalities properly.");
        }
    }

    public void AddTogle()
    {
            toggle = GetComponent<Toggle>();
            toggle.group = transform.parent.GetComponent<ToggleGroup>();
    }

    public void LoadLevelDetails(GameLevel levelData, ulong playerCoins)
    {
        // levelNumber = levelData.level_number;
        // costToPlay = levelData.cost;
        // coinsToEarn = levelData.reward;
        // bonusCoins = levelData.bonus;
        // enemies = new List<EnemyData>(levelData.enemies);
        // spies = new List<SpyData>(levelData.spies);
		this.levelData = levelData;
        levelName.text = $"Level {levelData.level_number}";
        costToPlayText.text = $"{levelData.cost} SUXO";
        coinsToEarnText.text = $"{levelData.reward} SUXO";
        bonusCoinsText.text = $"{levelData.bonus} SUXO";

        if (playerCoins < levelData.cost) Disable(); else Enable();
    }

    public void Disable()
    {
        canvasGroup.alpha = 0.5f;
        toggle.interactable = false;

        //canvasGroup.blocksRaycasts = ;
        //canvasGroup.interactable = ;
    }

    public void Enable()
    {
        canvasGroup.alpha = 1;

        //canvasGroup.blocksRaycasts = ;
        //canvasGroup.interactable = ;
    }
}
