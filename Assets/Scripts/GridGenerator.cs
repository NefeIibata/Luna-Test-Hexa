using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;

public class GridGenerator : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject hexagon;

    [Header(" Settings ")]
    
    [SerializeField] private int GridSize;


    [ContextMenu("Generate Grid")]
    private void GenerateGrid()
    {
        transform.Clear();

        for (int x = -GridSize; x <= GridSize; x++)
        {
            for (int y = -GridSize; y <= GridSize; y++)
            {
                Vector3 spawnPos = grid.CellToWorld(new Vector3Int(x, y, 0));

                if (spawnPos.magnitude > grid.CellToWorld(new Vector3Int(1, 0, 0)).magnitude * GridSize)
                    continue;

                GameObject gridHexInstance = (GameObject)PrefabUtility.InstantiatePrefab(hexagon);
                gridHexInstance.transform.position = spawnPos;
                gridHexInstance.transform.rotation = Quaternion.identity;
                gridHexInstance.transform.SetParent(transform);

            }
        }
    }
}
#endif
