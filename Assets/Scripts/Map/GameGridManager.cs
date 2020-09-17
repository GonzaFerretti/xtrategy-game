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
    [SerializeField] public GridCoordinates gridCoordinates;
    [SerializeField] public CoverInformation covers;
    [SerializeField] private GameGridCell baseGridCell;
    [SerializeField] private Cover baseLowCover;
    [SerializeField] private Cover baseHighCover;
    [SerializeField] private Cover indicatorCover;
    [SerializeField] private Transform cellsRootTransform;
    [SerializeField] private Transform coversRootTransform;
    [SerializeField] public bool editCoverMode = false;

    [SerializeField] private Dictionary<int, AsyncRangeQuery> currentQueries = new Dictionary<int, AsyncRangeQuery>();
    [SerializeField] private MapDictData savedData;

    [Header("Test Parameters")]
    [SerializeField] public Vector2Int gameGridSize;
    [SerializeField] private Vector3Int testStartNode;
    [SerializeField] private Vector3Int testEndNode;
    [SerializeField] private Unit baseUnit;


    public void SetMapData(MapDictData data)
    {
        savedData = data;
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
                Vector3 currentCellWorldPosition = grid.CellToWorld(cellCoordinates);
                GameGridCell cell = Instantiate(baseGridCell, currentCellWorldPosition, Quaternion.identity, cellsRootTransform);
                cell.name = "cell(" + row + "," + column + ")";
                //cell.transform.localScale = grid.cellSize;
                cell.SetCoordinates(cellCoordinates);
                cell.SetGridManagerReference(this);
                gridCoordinates.Add(cellCoordinates, cell);
            }
        }

        Vector3Int randomPositionInGrid = new Vector3Int(Random.Range(1, rows), Random.Range(1, columns), 0);
        Unit newUnit = Instantiate(baseUnit);
        newUnit.SetGridManagerReference(this);
        newUnit.Init(GetWorldPositionFromCoords(randomPositionInGrid), gridCoordinates[randomPositionInGrid]);
    }

    public void CleanObstacles()
    {
        while (coversRootTransform.childCount > 0)
        {
            DestroyImmediate(coversRootTransform.GetChild(0).gameObject);
        }

        covers = new CoverInformation();
    }

    public void CleanGrid()
    {
        while (cellsRootTransform.childCount > 0)
        {
            DestroyImmediate(cellsRootTransform.GetChild(0).gameObject);
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

    public void Start()
    {
        gridCoordinates = savedData.gridCoordinates;
        covers = savedData.coverInfo;
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

    public GameGridCell GetCellAtCoordinate(Vector3Int coord)
    {
        return gridCoordinates[coord];
    }

    public Vector3Int[] CalculateShortestPath(Vector3Int startCoordinates, Vector3Int destinationCoordinates)
    {
        Dictionary<Vector3Int, Node> openSet = new Dictionary<Vector3Int, Node>();
        Dictionary<Vector3Int, Node> closedSet = new Dictionary<Vector3Int, Node>();
        Node startNode = new Node(startCoordinates);
        Node targetNode = new Node(destinationCoordinates);
        openSet.Add(startCoordinates, startNode);
        List<Node> finalPath = new List<Node>();
        int safeguard = 0;
        while (openSet.Count > 0)
        {
            Node node = openSet.Values.ElementAt(0);
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet.Values.ElementAt(i).fCost < node.fCost || openSet.Values.ElementAt(i).fCost == node.fCost)
                {
                    if (openSet.Values.ElementAt(i).hCost < node.hCost)
                        node = openSet.Values.ElementAt(i);
                }
            }

            openSet.Remove(node.coordinates);
            closedSet.Add(node.coordinates, node);

            safeguard++;
            if (safeguard > 500)
            {
                Debug.Log("nope");
                break;
            }

            if (node.coordinates == destinationCoordinates)
            {
                Node currentPathNode = closedSet[destinationCoordinates];
                while (currentPathNode != startNode)
                {
                    finalPath.Add(currentPathNode);
                    currentPathNode = currentPathNode.parent;
                }
                finalPath.Reverse();
                return finalPath.Select(n => n.coordinates).ToArray();
            }

            foreach (Node neighbour in GetNeighbourNodes(node, openSet, closedSet))
            {
                if (closedSet.ContainsKey(neighbour.coordinates))
                {
                    continue;
                }

                int newCostToNeighbour = node.gCost + GetDistance(node.coordinates, neighbour.coordinates);
                if (newCostToNeighbour < neighbour.gCost || !openSet.ContainsKey(neighbour.coordinates))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour.coordinates, targetNode.coordinates);
                    neighbour.parent = node;

                    if (!openSet.ContainsKey(neighbour.coordinates))
                        openSet.Add(neighbour.coordinates, neighbour);
                }
            }
        }

        return null;
    }

    public void UntintBulk(IEnumerable<Vector3Int> cellsToUntint)
    {
        foreach (Vector3Int cell in cellsToUntint)
        {
            gridCoordinates[cell].Untint();
        }
    }

    public void TintBulk(IEnumerable<Vector3Int> cellsToTint)
    {
        foreach (Vector3Int cell in cellsToTint)
        {
            gridCoordinates[cell].TintSelected();
        }
    }

    public void TestPathfinding()
    {
        CalculateShortestPath(testStartNode, testEndNode);
    }

    // Disregards diagonal movements as this isn't allowed in the game
    public int GetDistance(Vector3Int cell1, Vector3Int cell2)
    {
        int result = Mathf.Abs(cell1.x - cell2.x) + Mathf.Abs(cell1.y - cell2.y);
        return result;
    }

    public List<Vector3Int> GetViableNeighbourCells(Vector3Int currentCellCoords)
    {
        List<Vector3Int> possibleNeighbourCoordinates = new List<Vector3Int>();

        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(1, 0, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(-1, 0, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(0, 1, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(0, -1, 0));

        return possibleNeighbourCoordinates;
    }

    public List<Node> GetNeighbourNodes(Node node, Dictionary<Vector3Int, Node> openSet, Dictionary<Vector3Int, Node> closedSet)
    {
        List<Vector3Int> possibleNeighbourCoordinates = GetViableNeighbourCells(node.coordinates);

        List<Node> neighbourNodes = new List<Node>();
        foreach (Vector3Int neighbour in possibleNeighbourCoordinates)
        {
            Node neighbourNode = (openSet.ContainsKey(neighbour)) ? openSet[neighbour] : ((closedSet.ContainsKey(neighbour)) ? closedSet[neighbour] : new Node(neighbour));
            neighbourNodes.Add(neighbourNode);
        }

        return neighbourNodes;
    }

    public void CheckNeighbourViabilityAndAdd(ref List<Vector3Int> coordinatesList, Vector3Int currentNode, Vector3Int direction)
    {
        if (IsNeighbourViable(currentNode, currentNode + direction))
        {
            coordinatesList.Add(currentNode + direction);
        }
    }

    public bool IsNeighbourViable(Vector3Int node, Vector3Int neighbour)
    {
        CoverData possibleCover = new CoverData(node, neighbour);
        bool containsCoverData = covers.ContainsKey(possibleCover);
        bool cellExists = gridCoordinates.ContainsKey(neighbour);
        bool containsCoverDataInverted = covers.ContainsKey(possibleCover.GetInverted());
        if (containsCoverData)
        {
            return covers[possibleCover] is LowCover && cellExists;
        }
        else if (containsCoverDataInverted)
        {
            return covers[possibleCover.GetInverted()] is LowCover && cellExists;
        }
        else return cellExists;
    }

    public Cover GetCoverFromCells(Vector3Int coord, Vector3Int nextCoord)
    {
        CoverData coverData = new CoverData(coord, nextCoord);
        Cover possibleCover = null;
        try { possibleCover = covers[coverData]; }
        catch { try { possibleCover = covers[coverData.GetInverted()]; } catch { } }
        return possibleCover;
    }

    public void TintMovementRange()
    {

    }

    public IEnumerator ProcessUnitRangeQuery(int maxSteps, int currentSteps, Vector3Int currentCell, int queryId)
    {
        currentQueries[queryId].cellsInRange.Add(currentCell);
        yield return new WaitForEndOfFrame();
        List<Vector3Int> currentNeighbours = GetViableNeighbourCells(currentCell);

        foreach (Vector3Int neighbour in currentNeighbours)
        {
            if (!currentQueries[queryId].cellsInRange.Contains(neighbour))
            {
                Cover possibleCover = GetCoverFromCells(currentCell, neighbour);
                int nextSteps = (possibleCover && possibleCover is LowCover) ? currentSteps + 2 : currentSteps + 1;
                if (nextSteps <= maxSteps)
                {
                    yield return StartCoroutine(ProcessUnitRangeQuery(maxSteps, nextSteps, neighbour, queryId));
                }
            }
        }

    }

    public IEnumerator ProcessUnitRangeQueryBIS(int maxSteps, Vector3Int currentCell, int queryId)
    {
        Dictionary<Vector3Int, int> currentBorder = new Dictionary<Vector3Int, int>();
        currentBorder.Add(currentCell, 0);
        int depth = 0;
        UntintBulk(gridCoordinates.Keys);
        while (depth <= maxSteps)
        {
            currentQueries[queryId].cellsInRange.AddRange(currentBorder.Keys);

            TintBulk(currentQueries[queryId].cellsInRange);
            Dictionary<Vector3Int, int> nextBorder = new Dictionary<Vector3Int, int>();
            foreach (Vector3Int currentBorderCell in currentBorder.Keys)
            {
                List<Vector3Int> possibleNextBorders = GetViableNeighbourCells(currentBorderCell);

                foreach (Vector3Int possibleNeighbour in possibleNextBorders)
                {
                    Cover possibleCover = GetCoverFromCells(currentBorderCell, possibleNeighbour);
                    int stepsTaken = (possibleCover && possibleCover is LowCover) ? currentBorder[currentBorderCell] + 2 : currentBorder[currentBorderCell] + 1;
                    if (stepsTaken <= maxSteps)
                    {
                        if (nextBorder.ContainsKey(possibleNeighbour))
                        {
                            if (nextBorder[possibleNeighbour] > stepsTaken)
                            {
                                nextBorder[possibleNeighbour] = stepsTaken;
                            }
                        }
                        else
                        {
                            nextBorder.Add(possibleNeighbour, stepsTaken);
                        }
                    }
                }
            }
            currentBorder = nextBorder;
            depth++;
            yield return null;
        }
    }

    public IEnumerator StartUnitRangeQuery(int maxSteps, Vector3Int startingCell, int queryId)
    {
        yield return StartCoroutine(ProcessUnitRangeQueryBIS(maxSteps, startingCell, queryId));
        currentQueries[queryId].hasFinished = true;
    }

    public AsyncRangeQuery QueryUnitRange(int maxSteps, Vector3Int startingCell)
    {
        foreach (GameGridCell cell in gridCoordinates.Values)
        {
            cell.Untint();
        }

        int queryId = Random.Range(0, 100);
        while (currentQueries.ContainsKey(queryId))
        {
            queryId = Random.Range(0, 100);
        }

        AsyncRangeQuery rangeQuery = new AsyncRangeQuery(queryId, this);
        currentQueries.Add(queryId, rangeQuery);
        StartCoroutine(StartUnitRangeQuery(maxSteps, startingCell, queryId));
        return rangeQuery;
    }

    public void EndQuery(AsyncRangeQuery query)
    {
        currentQueries.Remove(query.id);
    }

    public Vector3 GetWorldPositionFromCoords(Vector3Int cellCoords)
    {
        return grid.CellToWorld(cellCoords);
    }
}

[System.Serializable]
public class GridCoordinates : SerializableDictionaryBase<Vector3Int, GameGridCell> { }

[System.Serializable]
public class CoverInformation : SerializableDictionaryBase<CoverData, Cover> { }
