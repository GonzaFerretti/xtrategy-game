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
        Dictionary<Vector3Int,Node> openSet = new Dictionary<Vector3Int, Node>();
        Dictionary<Vector3Int,Node> closedSet = new Dictionary<Vector3Int,Node>();
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
            closedSet.Add(node.coordinates,node);

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
                break;
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

        Debug.Log(safeguard);
        foreach (GameGridCell cell in gridCoordinates.Values)
        {
            cell.Untint();
        }

        foreach (Node possibleNodes in openSet.Values)
        {
            gridCoordinates[possibleNodes.coordinates].TintPath();
        }

        foreach (Node discoveredNode in closedSet.Values)
        {
            gridCoordinates[discoveredNode.coordinates].TintSelected();
        }
        
        foreach (Node finalPathNode in finalPath)
        {
            gridCoordinates[finalPathNode.coordinates].TintFinalPath();
        }

        gridCoordinates[startCoordinates].TintStart();
        gridCoordinates[destinationCoordinates].TintEnd();
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

    public List<Node> GetNeighbourNodes(Node node, Dictionary<Vector3Int, Node> openSet, Dictionary<Vector3Int, Node> closedSet)
    {
        List<Vector3Int> possibleNeighbourCoordinates = new List<Vector3Int>();
        List<Node> neighbourNodes = new List<Node>();

        possibleNeighbourCoordinates.Add(node.coordinates + new Vector3Int(1, 0, 0));
        possibleNeighbourCoordinates.Add(node.coordinates + new Vector3Int(-1, 0, 0));
        possibleNeighbourCoordinates.Add(node.coordinates + new Vector3Int(0, 1, 0));
        possibleNeighbourCoordinates.Add(node.coordinates + new Vector3Int(0, -1, 0));

        foreach (Vector3Int neighbour in possibleNeighbourCoordinates)
        {
            if (IsNeighbourViable(node.coordinates, neighbour))
            {
                Node neighbourNode = (openSet.ContainsKey(neighbour)) ? openSet[neighbour] : ((closedSet.ContainsKey(neighbour)) ? closedSet[neighbour] : new Node(neighbour));
                neighbourNodes.Add(neighbourNode);
            }
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
