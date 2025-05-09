using UnityEngine;
using UnityEngine.UI;

public class PercentBar : MonoBehaviour
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    [Header("UI References")]
    public RectTransform targetImageRect;

    [Range(0f, 1f)]
    public float value = 1.0f;

    [Header("Settings")]
    public Orientation orientation = Orientation.Horizontal;
    public bool updateInRuntime = true;

    private Vector2 originalSize;

    void Start()
    {
        if (targetImageRect == null)
        {
            Debug.LogError("PercentBar: No targetImageRect assigned.");
            enabled = false;
            return;
        }

        // Store the original size
        originalSize = targetImageRect.sizeDelta;

        // Initial update
        UpdateBar();
    }

    void Update()
    {
        if (updateInRuntime)
        {
            UpdateBar();
        }
    }

    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        UpdateBar();
    }

    public void SetOrientation(Orientation newOrientation)
    {
        orientation = newOrientation;
        UpdateBar();
    }

    private void UpdateBar()
    {
        Vector2 newSize = originalSize;

        if (orientation == Orientation.Horizontal)
        {
            newSize.x = originalSize.x * value;
        }
        else
        {
            newSize.y = originalSize.y * value;
        }

        targetImageRect.sizeDelta = newSize;
    }
}