using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CongratsScreen : MonoBehaviour
{
    public Button continueButton;
	public TextMeshProUGUI rewardText;
	public TextMeshProUGUI bonusText;
	public GameObject StarsContainer;
	public GameView gameView;

	GameManager gameManager;

    void Start()
    {
		continueButton.onClick.AddListener(OnContinue);
		gameManager = FindObjectOfType<GameManager>();
		// gameManager.OnPlayerReady += OnPlayerUpdate;
		// gameManager.OnPlayerUpdated += OnPlayerUpdate;
    }

	void OnDestroy()
	{
		continueButton.onClick.RemoveListener(OnContinue);
	}

	public void Render(ulong reward, ulong maxbonus, ulong bonus)
    {
        rewardText.text = "Reward: " + reward.ToString() + " $SUXO";
        bonusText.text = "Bonus: " + bonus.ToString() + " $SUXO";

        float starPercentage = (float)bonus / maxbonus;
        int starsToActivate = Mathf.CeilToInt(starPercentage * 5); // Calculate how many stars to activate based on the bonus percentage

        for (int i = 0; i < StarsContainer.transform.childCount; i++)
        {
            var star = StarsContainer.transform.GetChild(i).GetComponent<Image>();
            if (i < starsToActivate)
            {
                star.color = new Color(star.color.r, star.color.g, star.color.b, 1f); // Turn on star by setting alpha to 1
            }
            else
            {
                star.color = new Color(star.color.r, star.color.g, star.color.b, 0.2f); // Turn off star by setting alpha to 0.2
            }
        }
    }

	void OnContinue()
	{
		gameView.ShowLevels();
	}
}
