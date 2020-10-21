using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameGridManager : MonoBehaviour
{
    // I'm using Unity grid class to handle grid to world or viceversa conversions.
    [Header("Cell Info")]
    [SerializeField] private Grid grid;
    public Transform cellsRootTransform;
    public Transform coversRootTransform;
    public Transform cellIndicatorsRootTransform;


    public GridCoordinates gridCoordinates;
    public CoverInformation covers;

    [SerializeField] private Dictionary<int, AsyncRangeQuery> currentQueries = new Dictionary<int, AsyncRangeQuery>();
    [SerializeField] private Dictionary<Vector3Int, GridIndicator> gridIndicators = new Dictionary<Vector3Int, GridIndicator>();
    public MapDictData savedData;
    public MapElementsDB elementsDatabase;
    public GameManager gameManager;

    [SerializeField] GridIndicator gridIndicatorPrefab;
    [SerializeField] float indicatorHeight;


    public void Start()
    {
        BuildCellsFromData();
        BuildCoversFromData();
        InitializeGridIndicators();
        CopyListOfCellsToUnusedList();
    }

    void CopyListOfCellsToUnusedList()
    {
        foreach (GameGridCell cell in gridCoordinates.Values)
        {
            unusedCells.Add(cell);
        }
    }

    List<GameGridCell> unusedCells = new List<GameGridCell>();

    public GameGridCell GetRandomUnusedCell()
    {
        GameGridCell cell = unusedCells[Random.Range(1, unusedCells.Count)];
        unusedCells.Remove(cell);
        return cell;
    }

    public void InitializeGridIndicators()
    {
        foreach (KeyValuePair<Vector3Int, GameGridCell> cellData in gridCoordinates)
        {
            GridIndicator newGridIndicator = Instantiate(gridIndicatorPrefab);
            newGridIndicator.transform.position = cellData.Value.transform.position + Vector3.up * indicatorHeight;
            newGridIndicator.transform.parent = cellIndicatorsRootTransform;

            gridIndicators.Add(cellData.Key, newGridIndicator);
        }
    }

    public void BuildCellsFromData()
    {
        Dictionary<string, GameGridCell> gridElementCache = new Dictionary<string, GameGridCell>();
        for (int i = 0; i < savedData.cellsCoordinates.Count; i++)
        {
            Vector3Int coordinates = savedData.cellsCoordinates[i];
            string prefabName = savedData.cellsPrefabNames[i];
            Vector3 cellPosition = GetWorldPositionFromCoords(coordinates);
            GameGridCell cellToInstantiate = null;
            if (gridElementCache.ContainsKey(prefabName))
            {
                cellToInstantiate = gridElementCache[prefabName];
            }
            else
            {
                cellToInstantiate = elementsDatabase.GetElementByType<GameGridCell>(prefabName);
                gridElementCache.Add(prefabName, cellToInstantiate);
            }
            GameGridCell cell = Instantiate(cellToInstantiate, cellPosition, Quaternion.identity, cellsRootTransform);
            cell.name = "cell(" + coordinates.x + "," + coordinates.y + ")";
            cell.SetCoordinates(coordinates);
            cell.SetGridManagerReference(this);
            gridCoordinates.Add(coordinates, cell);
        }
    }

    public void BuildCoversFromData()
    {
        Dictionary<string, Cover> coverElementsCache = new Dictionary<string, Cover>();
        for (int i = 0; i < savedData.coversData.Count; i++)
        {
            CoverData coverInfo = savedData.coversData[i];
            string prefabName = savedData.coversPrefabNames[i];
            Vector3 position = (GetWorldPositionFromCoords(coverInfo.side1) + GetWorldPositionFromCoords(coverInfo.side2)) / 2;
            Cover coverToInstantiate = null;
            if (coverElementsCache.ContainsKey(prefabName))
            {
                coverToInstantiate = coverElementsCache[prefabName];
            }
            else
            {
                coverToInstantiate = elementsDatabase.GetElementByType<Cover>(prefabName);
                coverElementsCache.Add(prefabName, coverToInstantiate);
            }
            Cover cover = Instantiate(coverToInstantiate);
            cover.SetGridManagerReference(this);
            cover.transform.position = position;
            cover.transform.parent = coversRootTransform;
            cover.coverData = coverInfo;
            if (!coverInfo.IsCellMovementDirectionInXAxis())
            {
                cover.transform.localEulerAngles = new Vector3(0, 90, 0);
            }
            covers.Add(coverInfo, cover);
        }
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

    public void EnableCellIndicators(IEnumerable<Vector3Int> indicatorsToEnable, GridIndicatorMode gridIndicatorMode)
    {
        foreach (Vector3Int indicator in indicatorsToEnable)
        {
            gridIndicators[indicator].Enable(gridIndicatorMode);
        }
    }

    public void EnableCellIndicator(Vector3Int indicatorToEnable, GridIndicatorMode gridIndicatorMode)
    {
        gridIndicators[indicatorToEnable].Enable(gridIndicatorMode);
    }

    public void DisableCellIndicators(IEnumerable<Vector3Int> indicatorsToDisable)
    {
        foreach (Vector3Int indicator in indicatorsToDisable)
        {
            gridIndicators[indicator].Disable();
        }
    }

    public void DisableCellIndicator(Vector3Int indicatorToDisable)
    {
        gridIndicators[indicatorToDisable].Disable();
    }

    public void DisableAllCellIndicators()
    {
        foreach (KeyValuePair<Vector3Int, GridIndicator> indicatorData in gridIndicators)
        {
            indicatorData.Value.Disable();
        }
    }

    // Disregards diagonal movements as this isn't allowed in the game
    public int GetDistance(Vector3Int cell1, Vector3Int cell2)
    {
        int result = Mathf.Abs(cell1.x - cell2.x) + Mathf.Abs(cell1.y - cell2.y);
        return result;
    }

    public List<Vector3Int> GetViableNeighbourCellsForMovement(Vector3Int currentCellCoords)
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
        List<Vector3Int> possibleNeighbourCoordinates = GetViableNeighbourCellsForMovement(node.coordinates);

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


    public IEnumerator ProcessUnitRangeQuery(int maxSteps, Vector3Int currentCell, int queryId)
    {
        Dictionary<Vector3Int, int> currentBorder = new Dictionary<Vector3Int, int>();
        currentBorder.Add(currentCell, 0);
        int depth = 0;
        DisableCellIndicators(gridCoordinates.Keys);
        while (depth <= maxSteps)
        {
            currentQueries[queryId].cellsInRange.AddRange(currentBorder.Keys);
            Dictionary<Vector3Int, int> nextBorder = new Dictionary<Vector3Int, int>();
            foreach (Vector3Int currentBorderCell in currentBorder.Keys)
            {
                List<Vector3Int> possibleNextBorders = GetViableNeighbourCellsForMovement(currentBorderCell);

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

        List<Vector3Int> cellsInRangeWithoutUnits = new List<Vector3Int>();

        foreach (Vector3Int cell in currentQueries[queryId].cellsInRange)
        {
            cellsInRangeWithoutUnits.Add(cell);
        }

        foreach (Vector3Int cell in currentQueries[queryId].cellsInRange)
        {
            foreach (Unit unit in gameManager.allUnits)
            {
                if (unit.GetCoordinates() == cell)
                {
                    cellsInRangeWithoutUnits.Remove(cell);
                    break;
                }
            }
        }
        currentQueries[queryId].cellsInRange = cellsInRangeWithoutUnits;
    }

    public IEnumerator StartUnitRangeQuery(int maxSteps, Vector3Int startingCell, int queryId)
    {
        yield return StartCoroutine(ProcessUnitRangeQuery(maxSteps, startingCell, queryId));
        currentQueries[queryId].hasFinished = true;
    }

    public AsyncRangeQuery QueryUnitRange(int maxSteps, Vector3Int startingCell)
    {
        foreach (GameGridCell cell in gridCoordinates.Values)
        {
            gridIndicators[cell.GetCoordinates()].Disable();
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

    public IEnumerator ProcessAttackRangeQuery(int minRange, int maxRange, Vector3Int currentCell, int queryId)
    {
        Dictionary<Vector3Int, int> currentBorder = new Dictionary<Vector3Int, int>();
        foreach (Vector3Int cell in GetViableNeighbourCellsForAttack(currentCell))
        {
            currentBorder.Add(cell, 0);
        }
        List<Vector3Int> discardedRange = new List<Vector3Int>();
        int range = 1;
        DisableCellIndicators(gridCoordinates.Keys);
        while (range <= maxRange)
        {
            if (range >= minRange && range <= maxRange) currentQueries[queryId].cellsInRange.AddRange(currentBorder.Keys);
            else discardedRange.AddRange(currentBorder.Keys);
            Dictionary<Vector3Int, int> nextBorder = new Dictionary<Vector3Int, int>();
            foreach (Vector3Int currentBorderCell in currentBorder.Keys)
            {
                List<Vector3Int> possibleNextBorders = GetViableNeighbourCellsForAttack(currentBorderCell);

                foreach (Vector3Int possibleNeighbour in possibleNextBorders)
                {
                    if (!nextBorder.ContainsKey(possibleNeighbour) && !discardedRange.Contains(possibleNeighbour) && !currentQueries[queryId].cellsInRange.Contains(possibleNeighbour))
                    {
                        nextBorder.Add(possibleNeighbour, currentBorder[currentBorderCell] + 1);
                    }
                }
            }
            currentBorder = nextBorder;
            range++;
            yield return null;
        }

        List<Vector3Int> cellsInValidAttackRange = new List<Vector3Int>();

        foreach (Vector3Int cell in currentQueries[queryId].cellsInRange)
        {
            cellsInValidAttackRange.Add(cell);
        }

        foreach (Unit unit in gameManager.allUnits)
        {
            Vector3Int unitCoords = unit.GetCoordinates();
            foreach (Cover cover in unit.currentCovers)
            {
                if (cover is LowCover) continue;
                if (IsCoverInTheWayOfAttack(currentCell, unitCoords, cover))
                {
                    cellsInValidAttackRange.Remove(unitCoords);
                    break;
                }
            }
        }
        currentQueries[queryId].cellsInRange = cellsInValidAttackRange;

    }

    public bool IsCoverInTheWayOfAttack(Vector3Int origin, Vector3Int destination, Cover coverObject)
    {
        CoverData cover = coverObject.coverData;

        bool isOnSide1OfCover = destination == cover.side1;
        bool isOnSide2OfCover = destination == cover.side2;
        if (!isOnSide1OfCover && !isOnSide2OfCover) return false;
        bool coversXAxis = cover.IsCellMovementDirectionInXAxis();
        if (coversXAxis)
        {
            if (cover.side1.y < cover.side2.y && isOnSide1OfCover && origin.y >= cover.side2.y)
            {
                return true;
            }
            else if (cover.side2.y < cover.side1.y && isOnSide2OfCover && origin.y >= cover.side1.y)
            {
                return true;
            }
            else if (cover.side1.y < cover.side2.y && isOnSide2OfCover && origin.y <= cover.side1.y)
            {
                return true;
            }
            else if (cover.side2.y < cover.side1.y && isOnSide1OfCover && origin.y <= cover.side2.y)
            {
                return true;
            }
            return false;
        }
        else
        {


            if (cover.side1.x < cover.side2.x && isOnSide1OfCover && origin.x >= cover.side2.x)
            {
                return true;
            }
            else if (cover.side2.x < cover.side1.x && isOnSide2OfCover && origin.x >= cover.side1.x)
            {
                return true;
            }
            else if (cover.side1.x < cover.side2.x && isOnSide2OfCover && origin.x <= cover.side1.x)
            {
                return true;
            }
            else if (cover.side2.x < cover.side1.x && isOnSide1OfCover && origin.x <= cover.side2.x)
            {
                return true;
            }
            return false;
        }
    }

    public List<Vector3Int> GetViableNeighbourCellsForAttack(Vector3Int cell)
    {
        List<Vector3Int> possibleNeighbourCoordinates = new List<Vector3Int>();

        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(1, 0, 0));
        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(-1, 0, 0));
        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(0, 1, 0));
        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(0, -1, 0));
        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(1, 1, 0));
        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(-1, -1, 0));
        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(-1, 1, 0));
        CheckNeighbourAttackViabilityAndAdd(ref possibleNeighbourCoordinates, cell, new Vector3Int(1, -1, 0));

        return possibleNeighbourCoordinates;
    }

    public void CheckNeighbourAttackViabilityAndAdd(ref List<Vector3Int> coordinatesList, Vector3Int currentNode, Vector3Int direction)
    {
        if (IsNeighbourViableForAttack(currentNode, currentNode + direction))
        {
            coordinatesList.Add(currentNode + direction);
        }
    }

    public bool IsNeighbourViableForAttack(Vector3Int node, Vector3Int neighbour)
    {
        return gridCoordinates.ContainsKey(neighbour);
    }

    public IEnumerator StartUnitAttackRangeQuery(int minRange, int maxRange, Vector3Int startingCell, int queryId)
    {
        yield return StartCoroutine(ProcessAttackRangeQuery(minRange, maxRange, startingCell, queryId));
        currentQueries[queryId].hasFinished = true;
    }

    public AsyncRangeQuery QueryUnitAttackRange(int minRange, int maxRange, Vector3Int startingCell)
    {
        foreach (GameGridCell cell in gridCoordinates.Values)
        {
            gridIndicators[cell.GetCoordinates()].Disable();
        }

        int queryId = Random.Range(0, 100);
        while (currentQueries.ContainsKey(queryId))
        {
            queryId = Random.Range(0, 100);
        }

        AsyncRangeQuery rangeQuery = new AsyncRangeQuery(queryId, this);
        currentQueries.Add(queryId, rangeQuery);
        StartCoroutine(StartUnitAttackRangeQuery(minRange, maxRange, startingCell, queryId));
        return rangeQuery;
    }

    public Vector3 GetWorldPositionFromCoords(Vector3Int cellCoords)
    {
        return grid.CellToWorld(cellCoords);
    }

    public Vector3Int[] GetBestPathToGetToClosestUnit(Unit thisUnit, List<Unit> otherUnits)
    {
        List<Vector3Int[]> possiblePaths = new List<Vector3Int[]>();
        foreach (Unit possibleTargetUnit in otherUnits)
        {
            List<Vector3Int> possibleDirections = GetViableNeighbourCellsForMovement(possibleTargetUnit.GetCoordinates());
            foreach (Vector3Int possibleDirection in possibleDirections)
            {
                Vector3Int[] posssiblePath = CalculateShortestPath(thisUnit.GetCoordinates(), possibleDirection);
                possiblePaths.Add(posssiblePath);
            }
        }

        return GetShortestPathFromPossiblePaths(possiblePaths);
    }

    public Vector3Int[] GetPathToCoverClosestToEnemy(Unit thisUnit, List<Unit> otherUnits)
    {
        Dictionary<Vector3Int[], int> possiblePaths = new Dictionary<Vector3Int[], int>();
        foreach (Unit possibleTargetUnit in otherUnits)
        {
            foreach (CoverData cover in covers.Keys)
            {
                Vector3Int possiblePositionToCover;
                if (isCoverSuitableToDefendAgainstUnit(cover, possibleTargetUnit, out possiblePositionToCover))
                {
                    if (possiblePositionToCover == thisUnit.GetCoordinates()) continue;
                    Vector3Int[] pathToPosition = CalculateShortestPath(thisUnit.GetCoordinates(), possiblePositionToCover);
                    Vector3Int[] pathFromCoverToEnemy = CalculateShortestPath(possiblePositionToCover, possibleTargetUnit.GetCoordinates());
                    if (!possiblePaths.ContainsKey(pathToPosition))  possiblePaths.Add(pathToPosition, pathFromCoverToEnemy.Length);

                }
            }
        }

        int stepsToUnitFromClosestCover = int.MaxValue;
        List<Vector3Int[]> closestToUnitsCover = new List<Vector3Int[]>();

        foreach (KeyValuePair<Vector3Int[], int> existingPath in possiblePaths)
        {
            int stepsToUnit = existingPath.Value;
            if (stepsToUnit <= stepsToUnitFromClosestCover)
            {
                stepsToUnitFromClosestCover = stepsToUnit;
                closestToUnitsCover.Add(existingPath.Key);
            }
        }

        return GetShortestPathFromPossiblePaths(closestToUnitsCover);
    }

    public List<Cover> GetCoversFromCoord(Vector3Int coord)
    {
        List<Cover> result = new List<Cover>();
        foreach (KeyValuePair<CoverData, Cover> possibleCover in covers)
        {
            if (possibleCover.Key.side1 == coord || possibleCover.Key.side2 == coord)
            {
                result.Add(possibleCover.Value);
            }
        }
        return result;
    }

    public Vector3Int[] GetShortestPathFromPossiblePaths(List<Vector3Int[]> possiblePaths)
    {
        int stepsToClosestUnit = int.MaxValue;
        Vector3Int[] closestPath = new Vector3Int[0];

        foreach (Vector3Int[] existingPath in possiblePaths)
        {
            int steps = existingPath.Length;
            if (steps < stepsToClosestUnit)
            {
                stepsToClosestUnit = steps;
                closestPath = existingPath;
            }
        }
        return closestPath;
    }

    public bool isCoverSuitableToDefendAgainstUnit(CoverData cover, Unit unit, out Vector3Int suitableSide)
    {
        suitableSide = new Vector3Int(-1, -1, -1);
        if (unit.currentCovers.Contains(covers[cover])) return false;
        Vector3Int unitCoords = unit.GetCoordinates();

        bool coversXAxis = cover.IsCellMovementDirectionInXAxis();
        if (coversXAxis)
        {
            if (cover.side1.y < cover.side2.y && unitCoords.y >= cover.side2.y)
            {
                suitableSide = cover.side1;
                return true;
            }
            else if (cover.side2.y < cover.side1.y && unitCoords.y >= cover.side1.y)
            {
                suitableSide = cover.side2;
                return true;
            }
            else if (cover.side1.y < cover.side2.y && unitCoords.y <= cover.side1.y)
            {
                suitableSide = cover.side2;
                return true;
            }
            else if (cover.side2.y < cover.side1.y && unitCoords.y <= cover.side2.y)
            {
                suitableSide = cover.side1;
                return true;
            }
            return false;
        }
        else
        {
            if (cover.side1.x < cover.side2.x && unitCoords.x >= cover.side2.x)
            {
                suitableSide = cover.side1;
                return true;
            }
            else if (cover.side2.x < cover.side1.x && unitCoords.x >= cover.side1.x)
            {
                suitableSide = cover.side2;
                return true;
            }
            else if (cover.side1.x < cover.side2.x && unitCoords.x <= cover.side1.x)
            {
                suitableSide = cover.side2;
                return true;
            }
            else if (cover.side2.x < cover.side1.x && unitCoords.x <= cover.side2.x)
            {
                suitableSide = cover.side1;
                return true;
            }
            return false;
        }
    }
}

[System.Serializable]
public class GridCoordinates : SerializableDictionaryBase<Vector3Int, GameGridCell> { }

[System.Serializable]
public class CoverInformation : SerializableDictionaryBase<CoverData, Cover> { }

