using UnityEngine;

public class RotateSpinner : MonoBehaviour
{
    private RectTransform _rectTransform;
    private const float Speed = 250F;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        _rectTransform.Rotate(0, 0, -(Speed * Time.deltaTime));
    }
}
