using System;
using TMPro;
using UnityEngine;

/// <summary>
/// View component for displaying a board tile's modifier and color in the scene.
/// This is a visual wrapper and is separate from the model-layer <c>Tile</c> class.
/// </summary>
[ExecuteAlways]
public class TileView : MonoBehaviour
{
    [SerializeField] private long modifier = 0;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private MeshRenderer quadRenderer;
    [SerializeField] private Operation op;

    /// <summary>
    /// Gets the tile modifier currently shown by this visual tile.
    /// </summary>
    public long Modifier => modifier;

    /// <summary>
    /// Supported display operations for rendering the tile label.
    /// </summary>
    public enum Operation
    {
        Add,
        Multiply,
        Subtract
    }

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
    /// Applies the formatted modifier text to the tile label.
    /// </summary>
    private void UpdateText()
    {
        text.text = FormatValue(modifier);
    }

    /// <summary>
    /// Formats a value with its operation symbol (e.g. +3, x4).
    /// </summary>
    private string FormatValue(long v)
    {
        string numberStr = Math.Abs(v).ToString();
        char opChar = op switch
        {
            Operation.Add => '+',
            Operation.Multiply => '\u00D7', // multiply cross
            Operation.Subtract => '\u2212', // math minus sign
            _ => throw new NotImplementedException()
        };

        // TODO: format string when multiplying by a negative number
        return opChar + numberStr;
    }

    /// <summary>
    /// Sets the quad material color for this visual tile.
    /// </summary>
    public void SetColor(Color color)
    {
        if (quadRenderer != null)
            quadRenderer.material.color = color;
    }
}
