using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class GameGridManager : MonoBehaviour
{
    // I'm using Unity grid class to handle grid to world or viceversa conversions.
    [Header("Cell Info")]
    [SerializeField] private Grid grid;
    [SerializeField] private Dictionary<Vector3Int, GameGridCell> gridCoordinates = new Dictionary<Vector3Int, GameGridCell>();
    [SerializeField] private GameGridCell baseGridCell;
    [SerializeField] private GameObject baseLowCover;

    [Header("Test Parameters")]
    [SerializeField] public Vector2Int gameGridSize;


    public void InitGrid()
    {
        int rows = gameGridSize.x;
        int columns = gameGridSize.y;
        CleanGrid();
        for (int row = 1; row <= rows; row++)
        {
            for (int column = 1; column <= columns; column++)
            {
                Vector3Int cellCoordinates = new Vector3Int(row, column, 0);
                Vector3 currentCellWorldPosition = grid.CellToWorld(cellCoordinates);
                GameGridCell cell = Instantiate(baseGridCell, currentCellWorldPosition, Quaternion.identity, transform);
                cell.name = "cell(" + row + "," + column + ")";
                //cell.transform.localScale = grid.cellSize;
                cell.SetCoordinates(cellCoordinates);
                gridCoordinates.Add(cellCoordinates, cell);
            }
        }
    }

    public void CleanGrid()
    {
        if (gridCoordinates.Count > 0)
        {
            foreach (KeyValuePair<Vector3Int, GameGridCell> cellData in gridCoordinates)
            {
                if (cellData.Value)
                {
                    DestroyImmediate(cellData.Value.gameObject);
                }
            }
            gridCoordinates = new Dictionary<Vector3Int, GameGridCell>();
        }
    }

    public void AddCover(Vector3 position, CellMovement cellMovement)
    {
        GameObject cover = Instantiate(baseLowCover);
        cover.transform.position = position;
        if (!cellMovement.IsCellMovementDirectionInXAxis())
        {
            cover.transform.localEulerAngles = new Vector3(0, 90, 0);
        }
    }
}
