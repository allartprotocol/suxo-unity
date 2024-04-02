using UnityEngine;
using System; // For Action

public class SwipeDetector : MonoBehaviour
{
    public static event Action OnSwipeLeft;
    public static event Action OnSwipeRight;
    public static event Action OnSwipeUp;
    public static event Action OnSwipeDown;

    private Vector2 touchStart;
    private Vector2 touchEnd;
    private bool isSwipeDetected = false;

    public float minSwipeDistance = 50f;

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    isSwipeDetected = false;
                    break;

                case TouchPhase.Ended:
                    touchEnd = touch.position;
                    DetectSwipe();
                    break;
            }
        }
    }

    void DetectSwipe()
    {
        if (Vector2.Distance(touchStart, touchEnd) >= minSwipeDistance && !isSwipeDetected)
        {
            Vector2 direction = touchEnd - touchStart;
            Vector2 swipeType = Vector2.zero;

            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                swipeType = Vector2.right * Mathf.Sign(direction.x);
            }
            else
            {
                swipeType = Vector2.up * Mathf.Sign(direction.y);
            }

            if (swipeType.x != 0.0f)
            {
                if (swipeType.x > 0.0f)
                {
                    OnSwipeRight?.Invoke();
                }
                else
                {
                    OnSwipeLeft?.Invoke();
                }
            }

            if (swipeType.y != 0.0f)
            {
                if (swipeType.y > 0.0f)
                {
                    OnSwipeUp?.Invoke();
                }
                else
                {
                    OnSwipeDown?.Invoke();
                }
            }

            isSwipeDetected = true;
        }
    }
}