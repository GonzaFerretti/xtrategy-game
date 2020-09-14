using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.IO;
using UnityEditor.UIElements;

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
    [SerializeField] public bool editCoverMode = false;



    [Header("Test Parameters")]
    [SerializeField] public Vector2Int gameGridSize;
    [SerializeField] private Vector3Int testStartNode;
    [SerializeField] private Vector3Int testEndNode;


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

    public GameGridCell GetCellAtCoordinate(Vector3Int coord)
    {
        return gridCoordinates[coord];
    }

    public void CalculateShortestPath(Vector3Int startCoordinates, Vector3Int destinationCoordinates)
    {
        //Vector3Int startCoordinates = origin.GetCoordinates();
        //Vector3Int destinationCoordinates = destination.GetCoordinates();

        Dictionary<Vector3Int, Vector3Int> discoveredPathNodes = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> possiblePathNodes = new Dictionary<Vector3Int, Vector3Int>();
        discoveredPathNodes.Add(startCoordinates, startCoordinates);

        Vector3Int current = startCoordinates;
        while (true)
        {
            current = GetLowestFCostInList(discoveredPathNodes.Keys.ToList(), startCoordinates, destinationCoordinates);

            possiblePathNodes.Add(current, discoveredPathNodes[current]);
            discoveredPathNodes.Remove(current);

            if (current == destinationCoordinates) break;

            foreach (Vector3Int neighbour in GetNeighbourNodes(current))
            {
                if (possiblePathNodes.ContainsKey(neighbour)) continue;

                if (!discoveredPathNodes.ContainsKey(neighbour))
                {
                    discoveredPathNodes.Add(neighbour, current);
                }
            }
        }
        List<Vector3Int> finalPath = new List<Vector3Int>();
        finalPath.Add(destinationCoordinates);
        Vector3Int currentPathNode = possiblePathNodes[destinationCoordinates];
        while (currentPathNode != startCoordinates)
        {
            finalPath.Add(currentPathNode);
            currentPathNode = possiblePathNodes[currentPathNode];
        }
        finalPath.Reverse();

        foreach (GameGridCell cell in gridCoordinates.Values)
        {
            cell.Untint();
        }

        foreach (KeyValuePair<Vector3Int,Vector3Int> possibleNodes in possiblePathNodes)
        {
            gridCoordinates[possibleNodes.Key].TintPath();
        }

        foreach (KeyValuePair<Vector3Int, Vector3Int> discoveredNode in discoveredPathNodes)
        {
            gridCoordinates[discoveredNode.Key].TintSelected();
        }
        foreach (Vector3Int finalPathNode in finalPath)
        {
            gridCoordinates[finalPathNode].TintFinalPath();
        }

        gridCoordinates[startCoordinates].TintStart();
        gridCoordinates[destinationCoordinates].TintEnd();
    }

    public void TestPathfinding()
    {
        CalculateShortestPath(testStartNode, testEndNode);
    }

    public void RetracePath()
    {

    }

    // Disregards diagonal movements as this isn't allowed in the game
    public int GetDistanceBetweenCells(Vector3Int cell1, Vector3Int cell2)
    {
        int result = Mathf.Abs(cell1.x - cell2.x) + Mathf.Abs(cell1.y - cell2.y);
        return result;
    }

    // G cost in A* pathfinding means distance from starting node
    public int GetGCost(Vector3Int node, Vector3Int start)
    {
        return GetDistanceBetweenCells(node, start);
    }
    // H cost in A* pathfindingg means distance from destination node
    public int GetHCost(Vector3Int node, Vector3Int end)
    {
        return GetDistanceBetweenCells(node, end);
    }

    // F cost is defined as G cost + H cost
    public int GetFCost(Vector3Int node, Vector3Int start, Vector3Int end)
    {
        return GetHCost(node, end) + GetGCost(node, start);
    }

    public Vector3Int GetLowestFCostInList(List<Vector3Int> discoveredPathNodes, Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> orderedByFCost = discoveredPathNodes.OrderBy(n => GetFCost(n, start, end)).ToList();
        List<int> fcostList = orderedByFCost.Select(n => GetFCost(n, start, end)).ToList();
        return orderedByFCost[0];
    }

    public List<Vector3Int> GetNeighbourNodes(Vector3Int node)
    {
        List<Vector3Int> possibleNeighbourNodes = new List<Vector3Int>();
        List<Vector3Int> neighbourNodes = new List<Vector3Int>();

        possibleNeighbourNodes.Add(node + new Vector3Int(1, 0, 0));
        possibleNeighbourNodes.Add(node + new Vector3Int(-1, 0, 0));
        possibleNeighbourNodes.Add(node + new Vector3Int(0, 1, 0));
        possibleNeighbourNodes.Add(node + new Vector3Int(0, -1, 0));

        foreach (Vector3Int neighbour in possibleNeighbourNodes)
        {
            if (IsNeighbourViable(node, neighbour)) neighbourNodes.Add(neighbour);
        }

        return neighbourNodes;
    }

    public bool IsNeighbourViable(Vector3Int node, Vector3Int neighbour)
    {
        CoverData possibleCover = new CoverData(node, neighbour);
        bool hasCoverInBetween = covers.ContainsKey(possibleCover) || covers.ContainsKey(possibleCover.GetInverted());
        return gridCoordinates.ContainsKey(neighbour) && !hasCoverInBetween;
    }
}

[System.Serializable]
public class GridCoordinates : SerializableDictionaryBase<Vector3Int, GameGridCell> { }

[System.Serializable]
public class CoverInformation : SerializableDictionaryBase<CoverData, Cover> { }
