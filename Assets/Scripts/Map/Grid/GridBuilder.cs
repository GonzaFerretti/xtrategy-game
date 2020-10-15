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

        Vector3Int randomPositionInGrid = new Vector3Int(Random.Range(1, rows), Random.Range(1, columns), 0);
    }

    public void CreateCell(Vector3Int cellCoordinates, GameGridCell prefab)
    {
        Vector3 currentCellWorldPosition = gameGridManager.GetWorldPositionFromCoords(cellCoordinates);
        GameGridCell cell = Instantiate(prefab, currentCellWorldPosition, Quaternion.identity, gameGridManager.cellsRootTransform);
        cell.name = "cell(" + cellCoordinates.x + "," + cellCoordinates.y + ")";
        //cell.transform.localScale = grid.cellSize;
        cell.basePrefabName = prefab.name;
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

    void BuildCellsFromData(MapDictData savedData)
    {
        Dictionary<string, GameGridCell> gridElementCache = new Dictionary<string, GameGridCell>();
        for (int i = 0; i < savedData.cellsCoordinates.Count; i++)
        {
            Vector3Int coordinates = savedData.cellsCoordinates[i];
            string prefabName = savedData.cellsPrefabNames[i];
            Vector3 cellPosition = gameGridManager.GetWorldPositionFromCoords(coordinates);
            GameGridCell cellToInstantiate = null;
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
            Cover coverToInstantiate = null;
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
            covers.Add(coverInfo, cover);
        }
    }

    public void LoadFromFile(MapDictData mapData)
    {
        BuildCellsFromData(mapData);
        BuildCoversFromData(mapData);
    }

}
