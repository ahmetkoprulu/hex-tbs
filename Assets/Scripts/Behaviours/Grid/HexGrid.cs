using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    [field: SerializeField] public HexOrientation Orientation { get; set; }
    [field: SerializeField] public int Width { get; set; }
    [field: SerializeField] public int Height { get; set; }
    [field: SerializeField] public float HexSize { get; set; }

    public HexCell[,] OffsetGrid { get; private set; }
    public HexCell[,,] AxialGrid { get; private set; }

    public void SetOffsetGrid(HexCell[,] grid) => OffsetGrid = grid;

    public void SetAxialGrid(HexCell[,,] grid) => AxialGrid = grid;

    private void OnDrawGizmos()
    {
        for (int z = 0; z < Height; z++)
        {
            for (var x = 0; x < Width; x++)
            {
                var center = HexHelpers.GetCenter(HexSize, x, z, Orientation) + transform.position;
                var corners = HexHelpers.GetCorners(HexSize, Orientation);

                for (var c = 0; c < corners.Length; c++)
                {
                    var startPoint = center + corners[c];
                    var endPoint = center + corners[(c + 1) % 6];
                    Gizmos.DrawLine(startPoint, endPoint);
                }
            }
        }
    }
}

public enum HexOrientation : short
{
    FlatTop,
    PointyTop
}