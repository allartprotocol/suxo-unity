using System.Collections;
using UnityEngine;

public class UITweener : MonoBehaviour
{
    private enum TweenState { Open, Close }
    private TweenState state;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private Vector3 startScale = Vector3.one;
    private Vector3 endScale = Vector3.one;
    private float startAlpha = 1f;
    private float endAlpha = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetupTween(Vector2 start, Vector2 end, Vector3? scaleStart = null, Vector3? scaleEnd = null, float? alphaStart = null, float? alphaEnd = null)
    {
        startPosition = start;
        endPosition = end;
        if (scaleStart.HasValue) startScale = scaleStart.Value;
        if (scaleEnd.HasValue) endScale = scaleEnd.Value;
        if (alphaStart.HasValue) startAlpha = alphaStart.Value;
        if (alphaEnd.HasValue) endAlpha = alphaEnd.Value;
    }
    public void Open()
    {
        StopAllCoroutines(); // Stop previous tweens before starting a new one
        state = TweenState.Open;
        StartCoroutine(Tween(true));
    }

    public void Close()
    {
        StopAllCoroutines(); // Stop previous tweens before starting a new one
        state = TweenState.Close;
        StartCoroutine(Tween(false));
    }

    private IEnumerator Tween(bool opening)
    {
        float time = 0f;
        float duration = 1f; // Duration in seconds
        Vector2 position = opening ? startPosition : endPosition;
        Vector3 scale = opening ? startScale : endScale;
        float alpha = opening ? startAlpha : endAlpha;

        while (time < duration)
        {
            transform.localPosition = Vector2.Lerp(startPosition, endPosition, time / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            if (canvasGroup) canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = position;
        transform.localScale = scale;
        if (canvasGroup) canvasGroup.alpha = alpha;
    }

    public void OpenClose()
    {
        if (state == TweenState.Open)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void TweenToLeft()
    {
        float width = GetComponent<RectTransform>().rect.width;
        SetupTween(new Vector2(width, 0), Vector2.zero);
        Open();
    }

    public void TweenToRight()
    {
        float width = GetComponent<RectTransform>().rect.width;
        SetupTween(Vector2.zero, new Vector2(-width, 0));
        Open();
    }

    public void DefineState(bool isOpen)
    {
        state = isOpen ? TweenState.Open : TweenState.Close;
    }
}