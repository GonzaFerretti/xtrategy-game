using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
public class GameGridManager : MonoBehaviour
{
    // I'm using Unity grid class to handle grid to world or viceversa conversions.
    [Header("Cell Info")]
    [SerializeField] private Grid grid;
    [SerializeField] private GridCoordinates gridCoordinates = new GridCoordinates();
    [SerializeField] private CoverInformation covers = new CoverInformation();
    [SerializeField] private GameGridCell baseGridCell;
    [SerializeField] private Cover baseLowCover;
    [SerializeField] private Cover baseHighCover;
    [SerializeField] private Cover indicatorCover;
    [SerializeField] private Transform cellsRootTransform;
    [SerializeField] private Transform coversRootTransform;

    [Header("Test Parameters")]
    [SerializeField] public Vector2Int gameGridSize;


    public void InitGrid()
    {
        int rows = gameGridSize.x;
        int columns = gameGridSize.y;
        CleanGrid();
        CleanObstacles();
        for (int row = 1; row <= rows; row++)
        {
            for (int column = 1; column <= columns; column++)
            {
                Vector3Int cellCoordinates = new Vector3Int(row, column, 0);
                Vector3 currentCellWorldPosition = grid.CellToWorld(cellCoordinates);
                GameGridCell cell = Instantiate(baseGridCell, currentCellWorldPosition, Quaternion.identity, cellsRootTransform);
                cell.name = "cell(" + row + "," + column + ")";
                //cell.transform.localScale = grid.cellSize;
                cell.SetCoordinates(cellCoordinates);
                cell.SetGridManagerReference(this);
                gridCoordinates.Add(cellCoordinates, cell);
            }
        }
    }

    public void CleanObstacles()
    {
        foreach (KeyValuePair<CoverData, Cover> coverData in covers)
        {
            if (coverData.Value)
            {
                DestroyImmediate(coverData.Value.gameObject);
            }
        }
        covers = new CoverInformation();
    }

    public void CleanGrid()
    {
        foreach (KeyValuePair<Vector3Int, GameGridCell> cellData in gridCoordinates)
        {
            if (cellData.Value)
            {
                DestroyImmediate(cellData.Value.gameObject);
            }
        }
        gridCoordinates = new GridCoordinates();
    }

    public void AddCover(Vector3 position, CoverData cellMovement)
    {
        CoverData invertedCellMovement = cellMovement.GetInverted();
        bool containsInCurrentDirection = covers.ContainsKey(cellMovement);
        bool containsInInvertedDirection = covers.ContainsKey(invertedCellMovement);
        if (!containsInCurrentDirection && !containsInInvertedDirection)
        {
            CreateCover(position, cellMovement, baseLowCover);
        }
        else
        {
            CoverData indexedDirection = (containsInCurrentDirection) ? cellMovement : invertedCellMovement;
            Cover cover = covers[indexedDirection];
            bool isCurrentCoverLow = cover is LowCover;
            covers.Remove(indexedDirection);
            DestroyImmediate(cover.gameObject);

            if (isCurrentCoverLow)
            {
                CreateCover(position, indexedDirection, baseHighCover);
            }
        }
    }

    public Cover CreateCover(Vector3 position, CoverData cellMovement, Cover CoverTypeSample)
    {
        Cover cover = Instantiate(CoverTypeSample);
        cover.transform.position = position;
        cover.transform.parent = coversRootTransform;
        if (!cellMovement.IsCellMovementDirectionInXAxis())
        {
            cover.transform.localEulerAngles = new Vector3(0, 90, 0);
        }
        covers.Add(cellMovement, cover);
        cover.SetGridManagerReference(this);
        return cover;
    }

    public GameGridCell GetAdjacentCellRelativeToMousePosition(Vector3 currentMousePosition, Vector3Int currentlyHoveredCell)
    {
        if (gridCoordinates.ContainsKey(currentlyHoveredCell))
        {
            Vector3 scaledDirection = currentMousePosition - gridCoordinates[currentlyHoveredCell].transform.position;
            float AbsX = Mathf.Abs(scaledDirection.x);
            float AbsY = Mathf.Abs(scaledDirection.z);
            Vector3Int vectorToCheck = (AbsX > AbsY) ? Vector3Int.right * (int)Mathf.Sign(scaledDirection.x) : Vector3Int.up * (int)Mathf.Sign(scaledDirection.z);
            if (gridCoordinates.ContainsKey(currentlyHoveredCell + vectorToCheck))
            {
                return gridCoordinates[currentlyHoveredCell + vectorToCheck];
            }
            else
            {
                return null;
            }
        }
        else return null;
    }
}

[System.Serializable]
public class GridCoordinates : SerializableDictionaryBase<Vector3Int, GameGridCell> { }

[System.Serializable]
public class CoverInformation : SerializableDictionaryBase<CoverData, Cover> { }
