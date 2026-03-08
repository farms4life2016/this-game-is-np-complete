using TMPro;
using UnityEngine;

/// <summary>
/// View component for displaying a block's numeric value in the scene.
/// This is a visual wrapper and is separate from the model-layer <c>Block</c> class.
/// </summary>
[ExecuteAlways]
public class BlockView : MonoBehaviour
{
    [SerializeField] private long value = 0;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private MeshRenderer quadRenderer;

    /// <summary>
    /// Gets the value currently shown by this visual block.
    /// </summary>
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

    /// <summary>
    /// Sets the displayed block value.
    /// </summary>
    public void SetValue(long newValue)
    {
        value = newValue;
        UpdateText();
    }

    /// <summary>
    /// Adds to the displayed block value.
    /// </summary>
    public void Add(long amount)
    {
        value += amount;
        UpdateText();
    }

    /// <summary>
    /// Multiplies the displayed block value.
    /// </summary>
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

    /// <summary>
    /// Sets the quad material color for this visual block.
    /// </summary>
    public void SetColor(Color color)
    {
        if (quadRenderer != null)
            quadRenderer.material.color = color;
    }
}
