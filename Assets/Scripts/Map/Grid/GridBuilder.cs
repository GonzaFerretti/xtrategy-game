using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBuilder : MonoBehaviour
{
    public GameGridManager gameGridManager;
    public GridCoordinates gridCoordinates;
    public CoverInformation covers;
    [SerializeField] private GameGridCell baseGridCell;
    [SerializeField] private Cover baseLowCover;
    [SerializeField] private Cover baseHighCover;
    [SerializeField] private Cover indicatorCover;


    [Header("Test Parameters")]
    public Vector2Int gameGridSize;
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
                CreateCell(cellCoordinates, baseGridCell);
            }
        }
    }

    public void CreateCell(Vector3Int cellCoordinates, GameGridCell prefab)
    {
        Vector3 currentCellWorldPosition = gameGridManager.GetWorldPositionFromCoords(cellCoordinates);
        GameGridCell cell = Instantiate(prefab, currentCellWorldPosition, Quaternion.identity, gameGridManager.cellsRootTransform);
        cell.name = "cell(" + cellCoordinates.x + "," + cellCoordinates.y + ")";
        //cell.transform.localScale = grid.cellSize;
        cell.basePrefabName = prefab.name;
        cell.transform.localEulerAngles = new Vector3(0, 0, 0);
        cell.SetCoordinates(cellCoordinates);
        cell.SetGridManagerReference(gameGridManager);
        gridCoordinates.Add(cellCoordinates, cell);
    }

    public void ReplaceCell(Vector3Int coords, GameGridCell prefabToReplaceItWith)
    {
        GameGridCell oldCell = gridCoordinates[coords];
        gridCoordinates.Remove(coords);
        DestroyImmediate(oldCell.gameObject);
        CreateCell(coords, prefabToReplaceItWith.GetComponent<GameGridCell>());
    }

    public void ReplaceCover(CoverData coverData, Cover prefabToReplaceWith)
    {
        Cover oldCover = covers[coverData];
        Vector3 position = oldCover.transform.position;
        covers.Remove(coverData);
        DestroyImmediate(oldCover.gameObject);

        CreateCover(position, coverData, prefabToReplaceWith);
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
        else
        {
            cover.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        covers.Add(cellMovement, cover);
        cover.SetGridManagerReference(gameGridManager);
        cover.coverData = cellMovement;
        return cover;
    }

    public GameGridCell GetAdjacentCellRelativeToMousePosition(Vector3 currentMousePosition, Vector3Int currentlyHoveredCell)
    {
        if (gridCoordinates.ContainsKey(currentlyHoveredCell))
        {
            if (this == null) return null;

            List<Vector3Int> adjacentCells = GetNeighbourCells(currentlyHoveredCell);

            GameGridCell nearestCell = GetNearestCellToPosition(adjacentCells, currentMousePosition);
            return nearestCell;
        }
        else return null;
    }

    public List<Vector3Int> GetNeighbourCells(Vector3Int currentCellCoords)
    {
        List<Vector3Int> possibleNeighbourCoordinates = new List<Vector3Int>();

        CheckBuildNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(1, 0, 0));
        CheckBuildNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(-1, 0, 0));
        CheckBuildNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(0, 1, 0));
        CheckBuildNeighbourViabilityAndAdd(ref possibleNeighbourCoordinates, currentCellCoords, new Vector3Int(0, -1, 0));

        return possibleNeighbourCoordinates;
    }

    public void CheckBuildNeighbourViabilityAndAdd(ref List<Vector3Int> coordinatesList, Vector3Int currentNode, Vector3Int direction)
    {
        if (gridCoordinates.ContainsKey(currentNode + direction))
        {
            coordinatesList.Add(currentNode + direction);
        }
    }

    public GameGridCell GetNearestCellToPosition(List<Vector3Int> cellsToCheck, Vector3 currentPosition) 
    {
        GameGridCell nearestCell = null;
        float nearestDistance = float.MaxValue;

        foreach (Vector3Int cellCoordToCheck in cellsToCheck)
        {
            GameGridCell currentlyCheckingCell = gridCoordinates[cellCoordToCheck];
            Vector3 cellToCheckPos = currentlyCheckingCell.transform.position;
            float currentDistance = Vector3.Distance(currentPosition, cellToCheckPos);
            if (currentDistance < nearestDistance)
            {
                nearestDistance = currentDistance;
                nearestCell = currentlyCheckingCell;
            }
        }

        return nearestCell;
    }

    void BuildCellsFromData(MapDictData savedData)
    {
        Dictionary<string, GameGridCell> gridElementCache = new Dictionary<string, GameGridCell>();
        for (int i = 0; i < savedData.cellsCoordinates.Count; i++)
        {
            Vector3Int coordinates = savedData.cellsCoordinates[i];
            string prefabName = savedData.cellsPrefabNames[i];
            Vector3 cellPosition = gameGridManager.GetWorldPositionFromCoords(coordinates);
            GameGridCell cellToInstantiate;
            if (gridElementCache.ContainsKey(prefabName))
            {
                cellToInstantiate = gridElementCache[prefabName];
            }
            else
            {
                cellToInstantiate = gameGridManager.elementsDatabase.GetElementByType<GameGridCell>(prefabName);
                gridElementCache.Add(prefabName, cellToInstantiate);
            }
            GameGridCell cell = Instantiate(cellToInstantiate, cellPosition, Quaternion.identity, gameGridManager.cellsRootTransform);
            cell.name = "cell(" + coordinates.x + "," + coordinates.y + ")";
            cell.SetCoordinates(coordinates);
            cell.basePrefabName = prefabName;
            cell.SetGridManagerReference(gameGridManager);
            cell.transform.localEulerAngles = new Vector3(0, 0, 0);
            gridCoordinates.Add(coordinates, cell);
        }
    }

    void BuildCoversFromData(MapDictData savedData)
    {
        Dictionary<string, Cover> coverElementsCache = new Dictionary<string, Cover>();
        for (int i = 0; i < savedData.coversData.Count; i++)
        {
            CoverData coverInfo = savedData.coversData[i];
            string prefabName = savedData.coversPrefabNames[i];
            Vector3 position = (gameGridManager.GetWorldPositionFromCoords(coverInfo.side1) + gameGridManager.GetWorldPositionFromCoords(coverInfo.side2)) / 2;
            Cover coverToInstantiate;
            if (coverElementsCache.ContainsKey(prefabName))
            {
                coverToInstantiate = coverElementsCache[prefabName];
            }
            else
            {
                coverToInstantiate = gameGridManager.elementsDatabase.GetElementByType<Cover>(prefabName);
                coverElementsCache.Add(prefabName, coverToInstantiate);
            }
            Cover cover = Instantiate(coverToInstantiate);
            cover.basePrefabName = prefabName;
            cover.SetGridManagerReference(gameGridManager);
            cover.coverData = coverInfo;
            cover.transform.position = position;
            cover.transform.parent = gameGridManager.coversRootTransform;
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

    public void LoadFromFile(MapDictData mapData)
    {
        BuildCellsFromData(mapData);
        BuildCoversFromData(mapData);
    }

}
