using TMPro;
using UnityEngine;

[ExecuteAlways]
public class Block : MonoBehaviour
{
    [SerializeField] private long value = 0;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private MeshRenderer quadRenderer;

    public long Value => value;

    private void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshPro>();

        UpdateText();
    }

    private void OnValidate()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshPro>();

        UpdateText();
    }

    public void SetValue(long newValue)
    {
        value = newValue;
        UpdateText();
    }

    public void Add(long amount)
    {
        value += amount;
        UpdateText();
    }

    public void Multiply(long factor)
    {
        value *= factor;
        UpdateText();
    }

    private void UpdateText()
    {
        text.text = FormatValue(value);
    }

    private string FormatValue(long v)
    {
        // Replace hyphen with proper minus sign (\u2212)
        return v.ToString().Replace("-", "\u2212");
    }

    public void SetColor(Color color)
    {
        if (quadRenderer != null)
            quadRenderer.material.color = color;
    }
}
