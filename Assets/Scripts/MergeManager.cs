using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MergeManager : MonoBehaviour
{
    [Header("Elements")]
    private List<GridCell> _updatedCells = new List<GridCell>();

    public static float AnimationDuration = 0.35f;
    public static float AnimationDelay = 0.05f;

    private void Awake()
    {
        StackController.OnStackPlaced += StackPlacedCallback;
    }

    private void OnDestroy()
    {
        StackController.OnStackPlaced -= StackPlacedCallback;
    }

    private void StackPlacedCallback(GridCell gridCell)
    {
        AnimationDuration = 0.35f;
        AnimationDelay = 0.05f;

        StartCoroutine(StackPlacedCoroutine(gridCell));
    }

    private IEnumerator StackPlacedCoroutine(GridCell gridCell)
    {
        yield return null;

        _updatedCells.Add(gridCell);

        while (_updatedCells.Count > 0)
        {
            GridCell nextCell = _updatedCells[0];
            _updatedCells.RemoveAt(0);
            
            yield return StartCoroutine(CheckForMerge(nextCell));
            
            AnimationDuration = Mathf.Max(AnimationDuration * 0.7f, 0.02f);
            AnimationDelay = Mathf.Max(AnimationDelay * 0.7f, 0.001f);
        }
        
        AreMovesAvailable();
    }

    private IEnumerator CheckForMerge(GridCell gridCell)
    {
        if (!gridCell.IsOccupied)
            yield break;

        Color topColor = gridCell.Stack.GetTopHexagonColor();
        List<GridCell> cluster = GetContiguousSimilarCells(gridCell, topColor);

        if (cluster.Count <= 1)
        {
            yield return StartCoroutine(CheckForCompleteStack(gridCell, topColor));
            yield break;
        }

        GridCell largestCell = cluster[0];
        int maxHexes = GetTopColorCount(largestCell, topColor);

        foreach (GridCell cell in cluster)
        {
            int count = GetTopColorCount(cell, topColor);
            if (count > maxHexes)
            {
                maxHexes = count;
                largestCell = cell;
            }
        }

        List<Hexagon> hexagonsToMove = new List<Hexagon>();

        foreach (GridCell cell in cluster)
        {
            if (cell != largestCell)
            {
                List<Hexagon> extracted = ExtractTopHexagons(cell, topColor);
                hexagonsToMove.AddRange(extracted);

                if (cell.IsOccupied && !_updatedCells.Contains(cell))
                {
                    _updatedCells.Add(cell);
                }
            }
        }

        MoveHexagons(largestCell, hexagonsToMove);

        yield return new WaitForSeconds(AnimationDuration + (hexagonsToMove.Count + 1) * AnimationDelay);

        yield return StartCoroutine(CheckForCompleteStack(largestCell, topColor));
    }

    private List<GridCell> GetContiguousSimilarCells(GridCell startCell, Color targetColor)
    {
        List<GridCell> cluster = new List<GridCell>();
        Queue<GridCell> queue = new Queue<GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();

        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();
            cluster.Add(current);

            List<GridCell> neighbors = GetNeighborGridCells(current);
            foreach (GridCell neighbor in neighbors)
            {
                if (!visited.Contains(neighbor) && neighbor.IsOccupied)
                {
                    if (neighbor.Stack.GetTopHexagonColor() == targetColor)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
        return cluster;
    }

    private int GetTopColorCount(GridCell cell, Color topColor)
    {
        int count = 0;
        for (int i = cell.Stack.Hexagons.Count - 1; i >= 0; i--)
        {
            if (cell.Stack.Hexagons[i].Color == topColor)
                count++;
            else
                break;
        }
        return count;
    }

    private List<Hexagon> ExtractTopHexagons(GridCell cell, Color topColor)
    {
        List<Hexagon> extracted = new List<Hexagon>();
        for (int i = cell.Stack.Hexagons.Count - 1; i >= 0; i--)
        {
            Hexagon hex = cell.Stack.Hexagons[i];
            if (hex.Color == topColor)
            {
                extracted.Add(hex);
                hex.SetParent(null);
            }
            else
            {
                break;
            }
        }
        
        foreach (Hexagon hex in extracted)
        {
            cell.Stack.Remove(hex);
        }
        
        extracted.Reverse();
        
        return extracted;
    }

    private List<GridCell> GetNeighborGridCells(GridCell gridCell)
    {
        LayerMask gridCellMask = 1 << gridCell.gameObject.layer;
        List<GridCell> neighborGridCells = new List<GridCell>();

        Collider[] colliders = Physics.OverlapSphere(gridCell.transform.position, 1.25f, gridCellMask);

        foreach (Collider col in colliders)
        {
            GridCell neighbor = col.GetComponent<GridCell>();
            if (neighbor != null && neighbor.IsOccupied && neighbor != gridCell)
            {
                neighborGridCells.Add(neighbor);
            }
        }

        return neighborGridCells;
    }

    private void MoveHexagons(GridCell gridCell, List<Hexagon> hexagonsToAdd)
    {
        float initialY = gridCell.Stack.Hexagons.Count * 0.2f;

        for (int i = 0; i < hexagonsToAdd.Count; i++)
        {
            Hexagon hexagon = hexagonsToAdd[i];

            float targetY = initialY + i * 0.2f;
            Vector3 targetLocalPosition = Vector3.up * targetY;

            gridCell.Stack.Add(hexagon);
            hexagon.MoveToLocal(targetLocalPosition, i * AnimationDelay);
        }
    }

    private IEnumerator CheckForCompleteStack(GridCell gridCell, Color topColor)
    {
        if (gridCell.Stack == null || gridCell.Stack.Hexagons.Count < 10)
            yield break;

        List<Hexagon> similarHexagons = new List<Hexagon>();

        for (int i = gridCell.Stack.Hexagons.Count - 1; i >= 0; i--)
        {
            Hexagon hexagon = gridCell.Stack.Hexagons[i];
            if (hexagon.Color != topColor)
                break;
            similarHexagons.Add(hexagon);
        }

        int similarHexagonCount = similarHexagons.Count;

        if (similarHexagons.Count < 10)
            yield break;

        float delay = 0;

        while (similarHexagons.Count > 0)
        {
            similarHexagons[0].SetParent(null);
            similarHexagons[0].Vanish(delay);

            delay += AnimationDelay;

            gridCell.Stack.Remove(similarHexagons[0]);
            similarHexagons.RemoveAt(0);
        }

        if (gridCell.IsOccupied && !_updatedCells.Contains(gridCell))
        {
            _updatedCells.Add(gridCell);
        }

        yield return new WaitForSeconds(AnimationDuration + (similarHexagonCount + 1) * AnimationDelay);
    }

    private bool AreMovesAvailable()
    {
        GridCell[] allGridCells = GameObject.FindObjectsOfType<GridCell>();

        foreach (GridCell cell in allGridCells)
        {
            if (!cell.IsOccupied)
                return true;
        }

        foreach (GridCell cell in allGridCells)
        {
            if (cell.IsOccupied)
            {
                Color topColor = cell.Stack.GetTopHexagonColor();
                List<GridCell> neighbors = GetNeighborGridCells(cell);
                foreach(var neighbor in neighbors) 
                {
                    if (neighbor.Stack.GetTopHexagonColor() == topColor) 
                        return true;
                }
            }
        }

        Debug.Log("No more moves! Game Over");
        if (PackshotManager.Instance != null)
            PackshotManager.Instance.ShowPackshot();
            
        return false;
    }
}
