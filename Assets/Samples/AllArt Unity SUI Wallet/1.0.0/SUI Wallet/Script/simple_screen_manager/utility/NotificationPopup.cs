using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NotificationPopup : MonoBehaviour
{
    public TextMeshProUGUI titleText;
	public float duration = 4f;

    internal void SetInfo(Color color, string message, Sprite sprite = null)
    {
        titleText.text = message;
        if(sprite != null)
        {
            GetComponentInChildren<Image>().sprite = sprite;
        }
        else{
            GetComponentInChildren<Image>().color = color;
        }
        SetDestroyTimer();
    }

    void SetDestroyTimer() { 
        StartCoroutine(DestroyAfter());
    }

    IEnumerator DestroyAfter() {
        yield return new WaitForSeconds(duration);
        InfoPopupManager.instance.RemoveNotif(this);
        Destroy(gameObject);
    }
}
