using UnityEngine;

public class SelfRotate : MonoBehaviour
{
    public float rotationSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        if (!GameStateManager.Instance.isPaused())
            transform.Rotate(new Vector3(0,0,1), rotationSpeed * Time.deltaTime);
    }
}
