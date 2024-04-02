using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleScreen;

public class LandingScreen : MonoBehaviour
{
	public Button playButton;
	public Button buyLifeButton;
	public GameObject LevelContainer;
    public GameObject LevelPrefab;
    public GameView gameView;

	GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
		playButton.onClick.AddListener(OnPlay);
		buyLifeButton.onClick.AddListener(OnBuyLife);
		gameManager.OnPlayerUpdated += OnPlayerUpdate;
		gameManager.OnPlayerReady += OnPlayerUpdate;
		// RenderLevels();
    }

	void OnDestroy()
	{
		playButton.onClick.RemoveListener(OnPlay);
		buyLifeButton.onClick.RemoveListener(OnBuyLife);
		gameManager.OnPlayerUpdated -= OnPlayerUpdate;
		gameManager.OnPlayerReady -= OnPlayerUpdate;
	}

	void OnPlayerUpdate(Player player){
		buyLifeButton.interactable = player.coins >= GameManager.LIFE_PRICE ? true : false;
		RenderLevels();
	}

	private void OnBuyLife()
	{
		buyLifeButton.interactable = false;
		StartCoroutine(gameManager.BuyLife(() => {
			buyLifeButton.interactable = gameManager.player.coins >= GameManager.LIFE_PRICE ? true : false;
		}));
	}

	private void OnPlay()
	{
		int selectedLevel = 0;
        ToggleGroup levelToggleGroup = LevelContainer.GetComponent<ToggleGroup>();
        var activeToggle = levelToggleGroup.ActiveToggles().First();
        if (activeToggle != null)
        {
            Level levelScript = activeToggle.GetComponent<Level>();
            if (levelScript != null) selectedLevel = levelScript.levelData.level_number;
			StartCoroutine(gameManager.PlayLevel(selectedLevel, SimpleScreenManager.instance.Hide));
		}
		else
		{
			Debug.Log("No level selected");
        }
	}

	void RenderLevels()
    {
        float height = 0;

        foreach (Transform child in LevelContainer.transform)
            Destroy(child.gameObject);

        foreach (var levelData in gameManager.GameData.levels)
        {
            GameObject levelPrefab = Instantiate(LevelPrefab); // Assuming levelPrefab is defined elsewhere
            Level levelScript = levelPrefab.GetComponent<Level>();
            levelPrefab.transform.SetParent(LevelContainer.transform, false);
            levelScript.AddTogle();
            levelScript.LoadLevelDetails(levelData.fields, gameManager.player.coins);
            Toggle toggleComponent = levelScript.GetComponent<Toggle>();
            toggleComponent.isOn = false;

            RectTransform levelPrefabRect = levelPrefab.GetComponent<RectTransform>();

            height += levelPrefabRect.rect.height;

            if (gameManager.player.coins < levelData.fields.cost)
                break;
        }

        int childCount = LevelContainer.transform.childCount;
        if (childCount > 1)
        {
            Transform secondLastToggle = LevelContainer.transform.GetChild(childCount - 2);
            Toggle toggleComponent = secondLastToggle.GetComponent<Toggle>();
            if (toggleComponent != null)
            {
                toggleComponent.isOn = true;
            }
        }

        RectTransform levelContainerRect = LevelContainer.GetComponent<RectTransform>();
        RectTransform viewportRect = LevelContainer.transform.parent.GetComponent<RectTransform>();
        RectTransform scrollRectRect = LevelContainer.transform.parent.parent.GetComponent<RectTransform>();

        //LayoutRebuilder.ForceRebuildLayoutImmediate(levelContainerRect);

        // Calculate the height difference between the viewport and the level container
        float viewportHeight = viewportRect.rect.height;
        float levelContainerHeight = height;//levelContainerRect.rect.height;
        float heightDifference = levelContainerHeight - viewportHeight;
        //Debug.Log($"Viewport Height: {viewportHeight}, Level Container Height: {levelContainerHeight}, Height Difference: {heightDifference}");
    
        // If the level container is shorter than the viewport, align it to the bottom
        if (heightDifference > 0)
        {
            // Set the bottom anchor to 0 to align it to the bottom
            levelContainerRect.anchorMin = new Vector2(levelContainerRect.anchorMin.x, 0);
            levelContainerRect.anchorMax = new Vector2(levelContainerRect.anchorMax.x, 0);

            // Adjust the pivot to ensure the content aligns to the bottom
            levelContainerRect.pivot = new Vector2(levelContainerRect.pivot.x, 0);

            // Reset the position to ensure it aligns properly after changing the anchors and pivot
            levelContainerRect.anchoredPosition = new Vector2(levelContainerRect.anchoredPosition.x, 0);
        } else {
            // Align the level container to the top if it's taller than the viewport
            levelContainerRect.anchorMin = new Vector2(levelContainerRect.anchorMin.x, 1);
            levelContainerRect.anchorMax = new Vector2(levelContainerRect.anchorMax.x, 1);

            // Adjust the pivot to ensure the content aligns to the top
            levelContainerRect.pivot = new Vector2(levelContainerRect.pivot.x, 1);

            // Reset the position to ensure it aligns properly after changing the anchors and pivot
            levelContainerRect.anchoredPosition = new Vector2(levelContainerRect.anchoredPosition.x, 0);
        }
    }
}
