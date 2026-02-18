using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private long value = 0;
    [SerializeField] private TextMeshPro text;

    public long Value => value;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshPro>();

        UpdateVisual();
    }

    public void SetValue(long newValue)
    {
        value = newValue;
        UpdateVisual();
    }

    public void Add(long amount)
    {
        value += amount;
        UpdateVisual();
    }

    public void Multiply(long factor)
    {
        value *= factor;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        text.text = FormatValue(value);
    }

    private string FormatValue(long v)
    {
        // Replace hyphen with proper minus sign (\u2212)
        return v.ToString().Replace("-", "\u2212");
    }
}
