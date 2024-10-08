using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexCellGenerator : MonoBehaviour
{
    [field: SerializeField] public HexGrid Grid { get; set; }
    [field: SerializeField] public MapGenerator MapGenerator { get; set; }
    [field: SerializeField] public int BatchSize { get; set; } = 2;

    private List<HexCell> cells = new();

    private void Start()
    {
        Generate();
    }

    private void Awake()
    {
        if (Grid == null) Grid = GetComponent<HexGrid>();
        if (MapGenerator == null) MapGenerator = GetComponent<MapGenerator>();
    }

    public void Generate()
    {
        if (MapGenerator.TerrainMap == null) MapGenerator.Generate();
        SetHexCellTerrainTypes(MapGenerator.TerrainMap);
    }

    private void SetHexCellTerrainTypes(TerrainType[,] terrainMap)
    {
        Debug.Log("Setting Hex Cell Terrain Types");
        ClearHexCells();

        cells = GenerateHexCells(Grid, terrainMap);
        Grid.SetOffsetGrid(TransformTo2DArray(cells, Grid.Width, Grid.Height, Grid.Orientation));
        StartCoroutine(InstantiateCells(cells));
    }

    public List<HexCell> GenerateHexCells(HexGrid grid, TerrainType[,] terrainMap) =>
        Enumerable.Range(0, grid.Height)
            .Select(y =>
                Enumerable.Range(0, grid.Width)
                    .Select(x =>
                    {
                        int flippedX = grid.Width - x - 1;
                        int flippedY = grid.Height - y - 1;
                        var center = HexHelpers.GetCenter(grid.HexSize, x, y, grid.Orientation);
                        //Vector3 centrePosition = HexMetrics.Center(HexSize, x, -y, Orientation) + gridOrigin;
                        return HexCell.Create(grid, grid.HexSize)
                            .SetCoordinates(new Vector2(x, y), grid.Orientation)
                            .SetTerrainType(terrainMap[flippedY, flippedX]);
                    })
            )
            .SelectMany(x => x)
            .ToList();

    public HexCell[,] TransformTo2DArray(List<HexCell> hexCells, int width, int height, HexOrientation orientation)
    {
        HexCell[,] hexCellArray = new HexCell[width, height];
        for (int i = 0; i < hexCells.Count; i++)
        {
            int x = (int)hexCells[i].OffsetCoordinates.x;
            int y = (int)hexCells[i].OffsetCoordinates.y;
            hexCellArray[x, y] = hexCells[i];
        }

        for (int i = 0; i < hexCells.Count; i++)
        {
            int x = (int)hexCells[i].OffsetCoordinates.x;
            int y = (int)hexCells[i].OffsetCoordinates.y;
            var nCoordinates = HexHelpers.GetNeighboursCoordinates(x, y, orientation)
                .Where(x => !HexHelpers.IsExceedingGrid((int)x.x, (int)x.y, width, height))
                .Select(x => hexCellArray[(int)x.x, (int)x.y])
                .ToList();

            hexCellArray[x, y].SetNeighbours(nCoordinates);
        }

        return hexCellArray;
    }

    public void ClearHexCells()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].ClearTerrain();
        }
        cells.Clear();
    }

    private IEnumerator InstantiateCells(List<HexCell> hexCells)
    {
        Debug.Log("Instantiating Hex Cells");
        int batchCount = 0;
        int totalBatches = Mathf.CeilToInt(hexCells.Count / BatchSize);
        for (int i = 0; i < cells.Count; i++)
        {
            cells[i].CreateTerrain();
            // Yield every batchSize hex cells
            if (i % BatchSize == 0 && i != 0)
            {
                batchCount++;
                // OnCellBatchGenerated?.Invoke((float)batchCount / totalBatches);
                yield return null;
            }
        }

        // OnCellInstancesGenerated?.Invoke();
    }
}