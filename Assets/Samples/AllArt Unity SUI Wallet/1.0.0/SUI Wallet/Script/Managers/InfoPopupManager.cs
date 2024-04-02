using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPopupManager : MonoBehaviour
{

    public enum InfoType
    {
        Info,
        Warning,
        Error
    }

    public Transform contentHolder;
    public GameObject notifPrefab;

    public static InfoPopupManager instance;

    public Color warningColor;
    public Color errorColor;
    public Color infoColor;

    public Sprite warningSprite;
    public Sprite errorSprite;
    public Sprite infoSprite;

    public List<NotificationPopup> notifQueue = new List<NotificationPopup>();
    public Transform underlay;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddNotif(InfoType type, string message, float duration = 4f)
    {
        if (notifQueue.Count > 0 && notifQueue[notifQueue.Count - 1].titleText.text == message) return;

        GameObject go = Instantiate(notifPrefab, contentHolder);
		NotificationPopup popup = go.GetComponent<NotificationPopup>();
		popup.duration = duration;
        switch (type)
        {
            case InfoType.Info:
                popup.SetInfo(infoColor, message, infoSprite);
                break;
            case InfoType.Warning:
                popup.SetInfo(warningColor, message, warningSprite);
                break;
            case InfoType.Error:
                popup.SetInfo(errorColor, message, errorSprite);
                break;
            default:
                break;
        }
        notifQueue.Add(popup);

        if(notifQueue.Count > 3)
        {
            Destroy(notifQueue[0].gameObject);
            notifQueue.Remove(notifQueue[0]);
        }
        underlay.gameObject.SetActive(true);
    }

    public void ClearNotif()
    {
        foreach (var notif in notifQueue)
        {
            Destroy(notif.gameObject);
        }
        notifQueue.Clear();
    }

    public void RemoveNotif(NotificationPopup notif)
    {
        notifQueue.Remove(notif);
        if(notifQueue.Count == 0)
        {
            underlay.gameObject.SetActive(false);
        }
    }
}
