using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    public GameGridManager gameGridManager;
    [SerializeField] public GridCoordinates gridCoordinates;
    [SerializeField] public CoverInformation covers;
    [SerializeField] private GameGridCell baseGridCell;
    [SerializeField] private Cover baseLowCover;
    [SerializeField] private Cover baseHighCover;
    [SerializeField] private Cover indicatorCover;
    [SerializeField] public bool editCoverMode = false;


    [Header("Test Parameters")]
    [SerializeField] public Vector2Int gameGridSize;
    [SerializeField] private Vector3Int testStartNode;
    [SerializeField] private Vector3Int testEndNode;
    [SerializeField] private Unit baseUnit;

    public void TestPathfinding()
    {
        gameGridManager.CalculateShortestPath(testStartNode, testEndNode);
    }
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
                Vector3 currentCellWorldPosition = gameGridManager.GetWorldPositionFromCoords(cellCoordinates);
                GameGridCell cell = Instantiate(baseGridCell, currentCellWorldPosition, Quaternion.identity, gameGridManager.cellsRootTransform);
                cell.name = "cell(" + row + "," + column + ")";
                //cell.transform.localScale = grid.cellSize;
                cell.basePrefabName = baseGridCell.name;
                cell.SetCoordinates(cellCoordinates);
                cell.SetGridManagerReference(gameGridManager);
                gridCoordinates.Add(cellCoordinates, cell);
            }
        }

        Vector3Int randomPositionInGrid = new Vector3Int(Random.Range(1, rows), Random.Range(1, columns), 0);
    }

    public void CleanObstacles()
    {
        while (gameGridManager.coversRootTransform.childCount > 0)
        {
            DestroyImmediate(gameGridManager.coversRootTransform.GetChild(0).gameObject);
        }

        covers = new CoverInformation();
    }

    public void CleanGrid()
    {
        while (gameGridManager.cellsRootTransform.childCount > 0)
        {
            DestroyImmediate(gameGridManager.cellsRootTransform.GetChild(0).gameObject);
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
        cover.transform.parent = gameGridManager.coversRootTransform;
        cover.basePrefabName = CoverTypeSample.name;
        if (!cellMovement.IsCellMovementDirectionInXAxis())
        {
            cover.transform.localEulerAngles = new Vector3(0, 90, 0);
        }
        covers.Add(cellMovement, cover);
        cover.SetGridManagerReference(gameGridManager);
        return cover;
    }

    public GameGridCell GetAdjacentCellRelativeToMousePosition(Vector3 currentMousePosition, Vector3Int currentlyHoveredCell)
    {
        if (gridCoordinates.ContainsKey(currentlyHoveredCell))
        {
            if (this == null) return null;
            Vector3 gridCellPosition = gridCoordinates[currentlyHoveredCell].transform.position;
            Vector3 scaledDirection = currentMousePosition - gridCellPosition;
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
