using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridCell : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Hexagon _hexagonPrefab;

    [Header("Settings")]
    [SerializeField] private Color[] _hexagonColors;

    public HexStack Stack { get; private set; }

    public bool IsOccupied 
    {
        get => Stack != null;
        private set { }
    }

    private Renderer _cellRenderer;
    private Color _originalColor;

    private void Awake()
    {
        _cellRenderer = GetComponentInChildren<Renderer>();
        if (_cellRenderer != null)
            _originalColor = _cellRenderer.material.color;
    }

    public void SetHighlight(bool isHighlighted)
    {
        if (_cellRenderer != null)
        {
            _cellRenderer.material.color = isHighlighted ? Color.white : _originalColor;
        }
    }

    private void Start()
    {
        if(_hexagonColors.Length > 0)
            GenerateInitialHexagons();
    }

    public void AssignStack(HexStack stack)
    {
        Stack = stack;
    }


    private void GenerateInitialHexagons()
    {
        while (transform.childCount > 1)
        {
            Transform t = transform.GetChild(1);
            t.SetParent(null);
            Destroy(t.gameObject);
        }
        
        Stack = new GameObject("Initial Stack").AddComponent<HexStack>();
        Stack.transform.SetParent(transform);
        Stack.transform.localPosition = Vector3.up * .2f;

        for (int i = 0; i < _hexagonColors.Length; i++)
        {
            Vector3 spawnPosition = Stack.transform.TransformPoint(Vector3.up * i * .2f);
            
            Hexagon hexagonInstance = Instantiate(_hexagonPrefab, spawnPosition, Quaternion.identity);
            hexagonInstance.Color = _hexagonColors[i];

            Stack.Add(hexagonInstance);
        }
    }
}
