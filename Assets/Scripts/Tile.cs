using System;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class Tile : MonoBehaviour
{
    [SerializeField] private long modifier = 0;
    [SerializeField] private TextMeshPro text;
    [SerializeField] private MeshRenderer quadRenderer;
    [SerializeField] private Operation op;

    public long Modifier => modifier;

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

    private void UpdateText()
    {
        text.text = FormatValue(modifier);
    }

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

    public void SetColor(Color color)
    {
        if (quadRenderer != null)
            quadRenderer.material.color = color;
    }
}
