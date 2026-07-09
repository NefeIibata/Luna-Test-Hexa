using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackController : MonoBehaviour
{
    [Header(" Settings ")]
    [SerializeField] private LayerMask _hexagonLayerMask;
    [SerializeField] private LayerMask _gridHexagonLayerMask;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _dragHeightOffset = 2.5f; 
    private HexStack _currentStack;
    private Vector3 _currentStackInitialPos;

    [Header(" Data ")]
    private GridCell _targetCell;
    private GridCell _currentHighlightCell;


    [Header(" Actions ")]
    public static Action<GridCell> OnStackPlaced;

    private void Update()
    {
        ManageControl();
    }

    private void ManageControl()
    {
        if (Input.GetMouseButtonDown(0))
            ManageMouseDown();
        else if (Input.GetMouseButton(0) && _currentStack != null)
            ManageMouseDrag();
        else if (Input.GetMouseButtonUp(0) && _currentStack != null)
            ManageMouseUp();
    }


    private void ManageMouseDown()
    {

        RaycastHit hit;
        Physics.Raycast(GetClickedRay(), out hit, 500, _hexagonLayerMask);

        if (hit.collider == null)
        {
            Debug.Log("We have not detected any hexagon");
            return;
        }

        Hexagon clickedHexagon = hit.collider.GetComponent<Hexagon>();
        
        if (clickedHexagon == null || clickedHexagon.HexStack == null)
        {
            // Либо кликнули не по гексу, либо по стартовому гексу на поле (у которого отключено взятие)
            return;
        }

        _currentStack = clickedHexagon.HexStack;
        _currentStackInitialPos = _currentStack.transform.position;
    }


    private void ManageMouseDrag()
    {
        Ray ray = GetClickedRay();
        
        if (Physics.Raycast(ray, out RaycastHit hit, 500, _gridHexagonLayerMask))
        {
            GridCell gridCell = hit.collider.GetComponent<GridCell>();

            if (!gridCell.IsOccupied)
            {
                _targetCell = gridCell;
                UpdateHighlight(gridCell);
            }
            else
            {
                _targetCell = null;
                UpdateHighlight(null);
            }
        }
        else
        {
            _targetCell = null;
            UpdateHighlight(null);
        }

        Plane dragPlane = new Plane(Vector3.up, Vector3.up * (_currentStackInitialPos.y + _dragHeightOffset));
        if (dragPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPos = ray.GetPoint(distance);
            _currentStack.transform.position = Vector3.MoveTowards(
                _currentStack.transform.position,
                targetPos,
                Time.deltaTime * 30);
        }
    }

    private void UpdateHighlight(GridCell newTarget)
    {
        if (_currentHighlightCell != newTarget)
        {
            if (_currentHighlightCell != null)
                _currentHighlightCell.SetHighlight(false);

            _currentHighlightCell = newTarget;

            if (_currentHighlightCell != null)
                _currentHighlightCell.SetHighlight(true);
        }
    }

    private void ManageMouseUp()
    {
        UpdateHighlight(null);

        if(_targetCell == null)
        {
            _currentStack.transform.position = _currentStackInitialPos;
            _currentStack = null;
            return;
        }

        _currentStack.transform.position = _targetCell.transform.position + Vector3.up * 0.2f;
        _currentStack.transform.SetParent(_targetCell.transform);
        _currentStack.Place();

        _targetCell.AssignStack(_currentStack);

        OnStackPlaced?.Invoke(_targetCell);

        _targetCell = null;
        _currentStack = null;
    }


    private Ray GetClickedRay() => Camera.main.ScreenPointToRay(Input.mousePosition);
}
