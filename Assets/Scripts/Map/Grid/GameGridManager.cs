using RotaryHeart.Lib.SerializableDictionary;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameGridManager : MonoBehaviour
{
    // I'm using Unity grid class to handle grid to world or viceversa conversions.
    [Header("Cell Info")]
    [SerializeField] private Grid grid;
    public Transform cellsRootTransform;
    public Transform coversRootTransform;
    public Transform cellIndicatorsRootTransform;
    public Transform minesRootTransform;
    public Dictionary<Vector3Int, List<Vector3Int>> neighbourDict = new Dictionary<Vector3Int, List<Vector3Int>>();
    public Dictionary<Vector3Int, List<Vector3Int>> mineNeighbourDict = new Dictionary<Vector3Int, List<Vector3Int>>();

    Dictionary<Vector3Int, Unit> unitPositionDict = new Dictionary<Vector3Int, Unit>();

    public Dictionary<Vector3Int, MagicMine> mineTriggerTiles = new Dictionary<Vector3Int, MagicMine>();
    public Dictionary<Vector3Int, MagicMine> mineDetonationTiles = new Dictionary<Vector3Int, MagicMine>();

    [SerializeField] MagicMine minePrefab;

    [SerializeField] ExplosionEffect explosionPrefab;

    public GridCoordinates gridCoordinates;
    public CoverInformation covers;

    [SerializeField] private Dictionary<int, AsyncQuery> currentQueries = new Dictionary<int, AsyncQuery>();
    [SerializeField] private Dictionary<Vector3Int, GridIndicator> gridIndicators = new Dictionary<Vector3Int, GridIndicator>();
    [SerializeField] private CoverIndicatorInformation coverIndicators = new CoverIndicatorInformation();

    public MapDictData savedData;
    public MapElementsDB elementsDatabase;
    public GameManager gameManager;

    [SerializeField] GridIndicator gridIndicatorPrefab;
    [SerializeField] float indicatorHeight;

    [SerializeField] CoverIndicator LowCoverIndicatorPrefab;
    [SerializeField] CoverIndicator HighCoverIndicatorPrefab;
    // Distance in X here is used for horizontal distance, and Y for vertical distance.
    [SerializeField] Vector2 coverIndicatorDistance;


    public void Init()
    {
        BuildCellsFromData();
        BuildCoversFromData();
        InitializeGridIndicators();
        CopyListOfCellsToUnusedList();
        GenerateAllPossibleNeighbours();
        GenerateAllPossibleMineNeighbours();
    }

    public void InitCoverIndicator(Vector3 coverPosition, Cover cover)
    {
        bool isHighCover = cover is HighCover;
        CoverIndicator coverIndicator = Instantiate((isHighCover) ? HighCoverIndicatorPrefab : LowCoverIndicatorPrefab);

        coverIndicator.transform.position = coverPosition + new Vector3(0, coverIndicatorDistance.y, 0);

        coverIndicator.SetDistance(coverIndicatorDistance.x);

        coverIndicator.transform.parent = cover.transform;

        coverIndicators.Add((cover.coverData.side1, coverIndicator));
        coverIndicators.Add((cover.coverData.side2, coverIndicator));
        coverIndicator.gameObject.SetActive(false);
    }

    public void InitLoadedMine(MineSaveInfo mineSaveInfo)
    {
        MagicMine newMine = InstantiateMine(GameObject.Find(mineSaveInfo.owner).GetComponent<BaseController>(), mineSaveInfo.position);

        foreach (var triggerCoord in mineSaveInfo.triggerTiles)
        {
            newMine.triggerTiles.Add(triggerCoord);
            mineTriggerTiles.Add(triggerCoord, newMine);
        }

        foreach (var detonationCoord in mineSaveInfo.detonationTiles)
        {
            newMine.triggerTiles.Add(detonationCoord);
            mineDetonationTiles.Add(detonationCoord, newMine);
        }
    }

    public MagicMine InstantiateMine(BaseController owner, Vector3Int pos)
    {
        MagicMine newMine = Instantiate(minePrefab, minesRootTransform);
        newMine.owner = owner;
        newMine.coordinates = pos;
        newMine.SetTeamColor();

        newMine.transform.position = GetWorldPositionFromCoords(pos);

        return newMine;
    }

    public int GetAttackRangeDistance(Vector3Int pos1, Vector3Int pos2)
    {
        int distX = Mathf.Abs(pos1.x - pos2.x);
        int distY = Mathf.Abs(pos1.y - pos2.y);

        return Mathf.Max(distX, distY);
    }

    public IEnumerator CreateMine(BaseController owner, Vector3Int position)
    {
        MagicMine newMine = InstantiateMine(owner, position);
        newMine.triggerTiles.Add(position);
        mineTriggerTiles.Add(position, newMine);

        foreach (var minedPosition in mineNeighbourDict[position])
        {
            if (!mineTriggerTiles.ContainsKey(minedPosition))
            {
                mineTriggerTiles.Add(minedPosition, newMine);
                newMine.triggerTiles.Add(minedPosition);
            }
        }

        AsyncRangeQuery attackQuery = QueryUnitAttackRange(0, 3, position);

        while (!attackQuery.hasFinished)
        {
            yield return null;
        }

        foreach (var cell in attackQuery.cellsInRange)
        {
            if (!newMine.triggerTiles.Contains(cell) && !mineDetonationTiles.ContainsKey(cell))
            {
                mineDetonationTiles.Add(cell, newMine);
                newMine.detonationTiles.Add(cell);
            }
        }
    }

    public void SetCoverIndicator(Vector3Int coords, bool state)
    {
        foreach (var indicatorData in coverIndicators)
        {
            if (coords == indicatorData.cellCoordinates) indicatorData.indicator.gameObject.SetActive(state);
        }
    }

    public void SetAllCoverIndicators(bool state)
    {
        foreach (var indicatorData in coverIndicators)
        {
            indicatorData.indicator.gameObject.SetActive(state);
        }
    }

    public void SetCoverIndicators(List<Vector3Int> coords, bool state)
    {
        foreach (Vector3Int coord in coords)
        {
            SetCoverIndicator(coord, state);
        }
    }

    void CopyListOfCellsToUnusedList()
    {
        foreach (GameGridCell cell in gridCoordinates.Values)
        {
            unusedCells.Add(cell);
        }
    }

    readonly List<GameGridCell> unusedCells = new List<GameGridCell>();

    public GameGridCell GetRandomUnusedCell()
    {
        GameGridCell cell = unusedCells[Random.Range(0, unusedCells.Count - 1)];
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
            newGridIndicator.transform.localRotation = Quaternion.identity;
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
            GameGridCell cellToInstantiate;
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
            cell.transform.localRotation = Quaternion.identity;
            cell.name = "cell(" + coordinates.x + "," + coordinates.y + ")";
            cell.SetCoordinates(coordinates);
            cell.SetGridManagerReference(this);
            gridCoordinates.Add(coordinates, cell);
        }
    }

    public void UpdateUnitPositionCache(Unit unit, Vector3Int coordinates)
    {
        if (!unitPositionDict.ContainsKey(coordinates))
        {
            unitPositionDict.Add(coordinates, unit);
        }
        else
        {
            unitPositionDict[coordinates] = unit;
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
            Cover coverToInstantiate;
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

            InitCoverIndicator(position, cover);

            if (!coverInfo.IsCellMovementDirectionInXAxis())
            {
                cover.transform.localEulerAngles = new Vector3(0, 90, 0);
            }
            else
            {
                cover.transform.localEulerAngles = new Vector3(0, 0, 0);
            }
            covers.Add(coverInfo, cover);
        }
    }

    public GameGridCell GetCellAtCoordinate(Vector3Int coord)
    {
        return gridCoordinates[coord];
    }

    public AsyncPathQuery StartShortestPathQuery(Vector3Int startCoordinates, Vector3Int destinationCoordinates)
    {
        int queryId = GetAvailableQueryId();
        AsyncPathQuery query = new AsyncPathQuery(queryId, this);
        currentQueries.Add(queryId, query);
        StartCoroutine(ProcessShortestPathQuery(startCoordinates, destinationCoordinates, queryId));

        return query;
    }

    public IEnumerator ProcessShortestPathQuery(Vector3Int startCoordinates, Vector3Int destinationCoordinates, int queryId)
    {
        AsyncPathQuery currentQuery = currentQueries[queryId] as AsyncPathQuery;
        Dictionary<Vector3Int, Node> openSet = new Dictionary<Vector3Int, Node>();
        Dictionary<Vector3Int, Node> closedSet = new Dictionary<Vector3Int, Node>();
        Node startNode = new Node(startCoordinates);
        Node targetNode = new Node(destinationCoordinates);
        openSet.Add(startCoordinates, startNode);
        float startTime = Time.realtimeSinceStartup;
        while (openSet.Count > 0)
        {
            Node node = openSet.Values.ElementAt(0);

            foreach (var possibleNode in openSet.Values)
            {
                if (possibleNode.GetFCost() < node.GetFCost() || possibleNode.GetFCost() == node.GetFCost())
                {
                    if (possibleNode.hCost < node.hCost)
                        node = possibleNode;
                }
            }

            openSet.Remove(node.coordinates);
            closedSet.Add(node.coordinates, node);

            if (node.coordinates == destinationCoordinates)
            {
                Node currentPathNode = closedSet[destinationCoordinates];
                while (currentPathNode != startNode)
                {
                    currentQuery.finalPath.Add(currentPathNode);
                    currentPathNode = currentPathNode.parent;
                }
                currentQuery.finalPath.Reverse();
                currentQuery.hasFinished = true;
                yield break;
            }

            List<Node> neighbourNodes = GetNeighbourNodes(node, openSet, closedSet);

            for (int i = 0; i < neighbourNodes.Count; i++)
            {
                Node neighbour = neighbourNodes[i];
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
    }

    public bool CheckMineProximity(out int damage, Vector3Int unitPosition, BaseController owner = null)
    {
        damage = 0;

        if (!mineTriggerTiles.ContainsKey(unitPosition)) return false;
        if (owner == null) return false;
        MagicMine currentlySteppedOnMine = mineTriggerTiles[unitPosition];

        if (owner == currentlySteppedOnMine.owner) return false;

        damage = currentlySteppedOnMine.stepDamage;

        DestroyMine(currentlySteppedOnMine);
        return true;
    }

    public void DetonateMine(Vector3Int mineCoords, BaseController owner)
    {
        if (mineTriggerTiles.ContainsKey(mineCoords))
        {
            MagicMine mine = mineTriggerTiles[mineCoords];
            foreach (var triggerTile in mine.triggerTiles)
            {
                ExplosionEffect explosion = Instantiate(explosionPrefab);
                explosion.Setup(true);
                explosion.transform.position = GetWorldPositionFromCoords(triggerTile) + Vector3.up * indicatorHeight * 5;
                explosion.transform.parent = cellIndicatorsRootTransform;
                explosion.transform.localRotation = Quaternion.identity;
                Unit unit = GetUnitAtCoordinates(triggerTile);
                if (unit)
                {
                    if (!unit.unitAttributes.isImmuneToExplosions && unit.owner != owner)
                    {
                        unit.TakeDamage(mine.centerDetonateDamage, mineCoords);
                    }
                }
            }

            foreach (var explosionTile in mine.detonationTiles)
            {
                Unit unit = GetUnitAtCoordinates(explosionTile);
                ExplosionEffect explosion = Instantiate(explosionPrefab);
                explosion.transform.position = GetWorldPositionFromCoords(explosionTile) + Vector3.up * indicatorHeight * 5;
                explosion.Setup(false);
                explosion.transform.parent = cellIndicatorsRootTransform;
                explosion.transform.localRotation = Quaternion.identity;

                if (unit)
                {
                    if (!unit.unitAttributes.isImmuneToExplosions && unit.owner != owner)
                    {
                        unit.TakeDamage(mine.sideDetonateDamage, mineCoords);
                    }
                }
            }

            DestroyMine(mine);
        }
    }

    public void DestroyMine(Vector3Int coords)
    {
        if (mineTriggerTiles.ContainsKey(coords))
        {
            DestroyMine(mineTriggerTiles[coords]);
        }
    }

    public void DestroyMine(MagicMine mine)
    {
        foreach (var coordinates in mine.triggerTiles)
        {
            mineTriggerTiles.Remove(coordinates);
        }

        foreach (var coordinates in mine.detonationTiles)
        {
            mineDetonationTiles.Remove(coordinates);
        }

        Destroy(mine.gameObject);
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

    public List<Vector3Int> GetViableNeighbourCellsForMine(Vector3Int currentCellCoords)
    {
        List<Vector3Int> possibleNeighbourCoordinates = new List<Vector3Int>();

        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(1, 0, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(-1, 0, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(0, 1, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(0, -1, 0));
        // Mine also attacks diagonal tiles
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(1, -1, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(-1, -1, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(-1, 1, 0));
        CheckNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(1, 1, 0));

        return possibleNeighbourCoordinates;
    }

    public void GenerateAllPossibleNeighbours()
    {
        foreach (var node in gridCoordinates)
        {
            neighbourDict.Add(node.Key, GetViableNeighbourCellsForMovement(node.Key));
        }
    }

    public void GenerateAllPossibleMineNeighbours()
    {
        foreach (var node in gridCoordinates)
        {
            mineNeighbourDict.Add(node.Key, GetViableNeighbourCellsForMine(node.Key));
        }
    }

    public List<Node> GetNeighbourNodes(Node node, Dictionary<Vector3Int, Node> openSet, Dictionary<Vector3Int, Node> closedSet)
    {
        List<Vector3Int> possibleNeighbourCoordinates = neighbourDict[node.coordinates];

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
        Dictionary<Vector3Int, int> currentBorder = new Dictionary<Vector3Int, int>
        {
            { currentCell, 0 }
        };
        int depth = 0;
        DisableCellIndicators(gridCoordinates.Keys);
        while (depth <= maxSteps)
        {
            (currentQueries[queryId] as AsyncRangeQuery).cellsInRange.AddRange(currentBorder.Keys);
            Dictionary<Vector3Int, int> nextBorder = new Dictionary<Vector3Int, int>();
            foreach (Vector3Int currentBorderCell in currentBorder.Keys)
            {
                List<Vector3Int> possibleNextBorders = neighbourDict[currentBorderCell];

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

        foreach (Vector3Int cell in (currentQueries[queryId] as AsyncRangeQuery).cellsInRange)
        {
            cellsInRangeWithoutUnits.Add(cell);
        }

        foreach (Vector3Int cell in (currentQueries[queryId] as AsyncRangeQuery).cellsInRange)
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
        (currentQueries[queryId] as AsyncRangeQuery).cellsInRange = cellsInRangeWithoutUnits;
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

        int queryId = GetAvailableQueryId();

        AsyncRangeQuery rangeQuery = new AsyncRangeQuery(queryId, this);
        currentQueries.Add(queryId, rangeQuery);
        StartCoroutine(StartUnitRangeQuery(maxSteps, startingCell, queryId));
        return rangeQuery;
    }

    public int GetAvailableQueryId()
    {
        int queryId = Random.Range(0, 511);
        while (currentQueries.ContainsKey(queryId))
        {
            queryId = Random.Range(0, 511);
        }
        return queryId;
    }

    public void EndQuery(AsyncQuery query)
    {
        currentQueries.Remove(query.id);
    }

    public Unit GetUnitAtCoordinates(Vector3Int coordinates)
    {
        if (unitPositionDict.ContainsKey(coordinates))
        {
            return unitPositionDict[coordinates];
        }
        return null;
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
            if (range >= minRange && range <= maxRange) (currentQueries[queryId] as AsyncRangeQuery).cellsInRange.AddRange(currentBorder.Keys);
            else discardedRange.AddRange(currentBorder.Keys);
            Dictionary<Vector3Int, int> nextBorder = new Dictionary<Vector3Int, int>();
            foreach (Vector3Int currentBorderCell in currentBorder.Keys)
            {
                List<Vector3Int> possibleNextBorders = GetViableNeighbourCellsForAttack(currentBorderCell);

                foreach (Vector3Int possibleNeighbour in possibleNextBorders)
                {
                    if (!nextBorder.ContainsKey(possibleNeighbour) && !discardedRange.Contains(possibleNeighbour) && !(currentQueries[queryId] as AsyncRangeQuery).cellsInRange.Contains(possibleNeighbour))
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

        foreach (Vector3Int cell in (currentQueries[queryId] as AsyncRangeQuery).cellsInRange)
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
        (currentQueries[queryId] as AsyncRangeQuery).cellsInRange = cellsInValidAttackRange;
        currentQueries[queryId].hasFinished = true;
    }

    public bool IsCoverInTheWayOfAttack(Vector3Int origin, Vector3Int destination, Cover coverObject)
    {
        CoverData cover = coverObject.coverData;

        if (origin == destination) return true;
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
        if (IsNeighbourViableForAttack(currentNode + direction))
        {
            coordinatesList.Add(currentNode + direction);
        }
    }

    public bool IsNeighbourViableForAttack(Vector3Int neighbour)
    {
        return gridCoordinates.ContainsKey(neighbour);
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
        StartCoroutine(ProcessAttackRangeQuery(minRange, maxRange, startingCell, queryId));
        return rangeQuery;
    }

    public Vector3 GetWorldPositionFromCoords(Vector3Int cellCoords)
    {
        return grid.CellToWorld(cellCoords);
    }

    public List<MagicMine> GetEnemyMines(BaseController owner)
    {
        List<MagicMine> enemyMines = new List<MagicMine>();

        foreach (var mine in mineTriggerTiles.Values)
        {
            if (mine.owner != owner) enemyMines.Add(mine);
        }

        return enemyMines;
    }

    public MagicMine GetMineWithEnemyNearby(List<Vector3Int> scanArea, BaseController attackingOwner)
    {
        MagicMine mine = null;
        foreach (var coordinates in scanArea)
        {
            if (mineTriggerTiles.ContainsKey(coordinates))
            {
                foreach (Unit unit in gameManager.allUnits)
                {
                    if (unit.owner != attackingOwner)
                    {
                        if (unit.GetCoordinates() == coordinates)
                        {
                            mine = mineTriggerTiles[coordinates];
                            break;
                        }
                    }
                }
                if (mine) break;
            }
            else if (mineDetonationTiles.ContainsKey(coordinates))
            {
                foreach (Unit unit in gameManager.allUnits)
                {
                    if (unit.owner != attackingOwner)
                    {
                        if (unit.GetCoordinates() == coordinates)
                        {
                            mine = mineDetonationTiles[coordinates];
                            break;
                        }
                    }
                }
                if (mine) break;
            }
        }
        return mine;
    }

    public AsyncPathQuery StartBestPathToClosestMineQuery(Unit thisUnit)
    {
        int queryId = GetAvailableQueryId();
        AsyncPathQuery currentQuery = new AsyncPathQuery(queryId, this);
        currentQueries.Add(queryId, currentQuery);
        StartCoroutine(GetBestPathToGetToClosestMine(thisUnit, GetEnemyMines(thisUnit.owner), queryId));
        return currentQuery;
    }

    public IEnumerator GetBestPathToGetToClosestMine(Unit thisUnit, List<MagicMine> otherMines, int queryId)
    {
        List<Vector3Int[]> possiblePaths = new List<Vector3Int[]>();

        float currentStartTime = Time.realtimeSinceStartup;

        foreach (MagicMine possibleMine in otherMines)
        {
            List<Vector3Int> possibleDirections = neighbourDict[possibleMine.coordinates];
            foreach (Vector3Int possibleDirection in possibleDirections)
            {
                int possiblePathQueryId = GetAvailableQueryId();
                AsyncPathQuery possiblePathQuery = new AsyncPathQuery(possiblePathQueryId, this);
                currentQueries.Add(possiblePathQueryId, possiblePathQuery);
                StartCoroutine(ProcessShortestPathQuery(thisUnit.GetCoordinates(), possibleDirection, possiblePathQueryId));
                while (!possiblePathQuery.hasFinished)
                {
                    yield return null;
                }
                possiblePaths.Add(possiblePathQuery.GetPathArray());
                possiblePathQuery.End();
            }
        }

        (currentQueries[queryId] as AsyncPathQuery).finalPathArray = GetShortestPathFromPossiblePaths(possiblePaths);
        (currentQueries[queryId] as AsyncPathQuery).hasFinished = true;
    }

    public AsyncPathQuery StartBestPathToClosestUnitQuery(Unit thisUnit, List<Unit> otherUnits)
    {
        int queryId = GetAvailableQueryId();
        AsyncPathQuery currentQuery = new AsyncPathQuery(queryId, this);
        currentQueries.Add(queryId, currentQuery);
        StartCoroutine(GetBestPathToGetToClosestUnit(thisUnit, otherUnits, queryId));
        return currentQuery;
    }

    public IEnumerator GetBestPathToGetToClosestUnit(Unit thisUnit, List<Unit> otherUnits, int queryId)
    {
        List<Vector3Int[]> possiblePaths = new List<Vector3Int[]>();

        float currentStartTime = Time.realtimeSinceStartup;

        foreach (Unit possibleTargetUnit in otherUnits)
        {
            List<Vector3Int> possibleDirections = neighbourDict[possibleTargetUnit.GetCoordinates()];
            foreach (Vector3Int possibleDirection in possibleDirections)
            {
                int possiblePathQueryId = GetAvailableQueryId();
                AsyncPathQuery possiblePathQuery = new AsyncPathQuery(possiblePathQueryId, this);
                currentQueries.Add(possiblePathQueryId, possiblePathQuery);
                StartCoroutine(ProcessShortestPathQuery(thisUnit.GetCoordinates(), possibleDirection, possiblePathQueryId));
                while (!possiblePathQuery.hasFinished)
                {
                    yield return null;
                }
                possiblePaths.Add(possiblePathQuery.GetPathArray());
                possiblePathQuery.End();
            }
        }

        (currentQueries[queryId] as AsyncPathQuery).finalPathArray = GetShortestPathFromPossiblePaths(possiblePaths);
        (currentQueries[queryId] as AsyncPathQuery).hasFinished = true;
    }

    public AsyncPathQuery StartPathCoverClosestToEnemyQuery(Unit thisUnit, List<Unit> otherUnits)
    {
        int queryId = GetAvailableQueryId();
        AsyncPathQuery currentQuery = new AsyncPathQuery(queryId, this);
        currentQueries.Add(queryId, currentQuery);
        StartCoroutine(ProcessPathCoverClosestToEnemyQuery(thisUnit, otherUnits, queryId));
        return currentQuery;
    }

    public IEnumerator ProcessPathCoverClosestToEnemyQuery(Unit thisUnit, List<Unit> otherUnits, int queryId)
    {
        Dictionary<Vector3Int[], int> possiblePaths = new Dictionary<Vector3Int[], int>();
        AsyncPathQuery query = currentQueries[queryId] as AsyncPathQuery;

        foreach (Unit possibleTargetUnit in otherUnits)
        {
            foreach (CoverData cover in covers.Keys)
            {
                if (IsCoverSuitableToDefendAgainstUnit(cover, possibleTargetUnit, out Vector3Int possiblePositionToCover))
                {
                    if (possiblePositionToCover == thisUnit.GetCoordinates()) continue;

                    AsyncPathQuery pathToPositionQuery = StartShortestPathQuery(thisUnit.GetCoordinates(), possiblePositionToCover);

                    while (!pathToPositionQuery.hasFinished)
                    {
                        yield return null;
                    }

                    Vector3Int[] pathToPosition = pathToPositionQuery.GetPathArray();

                    pathToPositionQuery.End();

                    AsyncPathQuery pathFromCoverToEnemyQuery = StartShortestPathQuery(possiblePositionToCover, possibleTargetUnit.GetCoordinates());

                    while (!pathFromCoverToEnemyQuery.hasFinished)
                    {
                        yield return null;
                    }

                    Vector3Int[] pathFromCoverToEnemy = pathFromCoverToEnemyQuery.GetPathArray();

                    pathFromCoverToEnemyQuery.End();

                    if (!possiblePaths.ContainsKey(pathToPosition)) possiblePaths.Add(pathToPosition, pathFromCoverToEnemy.Length);

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

        query.finalPathArray = GetShortestPathFromPossiblePaths(closestToUnitsCover);
        query.hasFinished = true;
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

    public bool IsCoverSuitableToDefendAgainstUnit(CoverData cover, Unit unit, out Vector3Int suitableSide)
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

public class CoverIndicatorInformation : List<(Vector3Int cellCoordinates, CoverIndicator indicator)> { }


