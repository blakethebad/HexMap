using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
    public HexCell[] hexes;
    public HexMesh terrain;


    private void Awake()
    {
        

        hexes = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
    }

    public void AddCell(int index, HexCell cell)
    {
        hexes[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
    }


    public void Refresh()
    {
        enabled = true;
    }

    private void LateUpdate()
    {
        TriangulateHexGroup();   

        enabled = false;
    }

    public void TriangulateHexGroup()
    {
        terrain.Clear();

        for (int i = 0; i < hexes.Length; i++)
        {
            for (HexDirection d = HexDirection.NorthEast; d <= HexDirection.NorthWest; d++)
            {
                TriangulateHex(d, hexes[i]);

            }

        }
        terrain.Apply();
    }

    void TriangulateHex(HexDirection direction, HexCell hex)
    {
        Vector3 center = hex.thisHex.center;
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v1 = hex.thisHex.GetFirstSolidCorner(direction) + hex.thisHex.center;
        Vector3 v2 = hex.thisHex.GetSecondSolidCorner(direction) + hex.thisHex.center;
        Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());

        HexCell neighbor = hex.GetNeighbor(direction);
        HexCell nextNeighbor = hex.GetNeighbor(direction.Next());

        if (neighbor != null)
        {
            bridge.y = neighbor.thisHex.center.y - hex.thisHex.center.y;
        }

        if (nextNeighbor != null)
        {

            v5.y = nextNeighbor.thisHex.center.y;
        }

        EdgeVertices e = new EdgeVertices(v1, v2);

        EdgeVertices e2 = new EdgeVertices(e.v1 + bridge, e.v5 + bridge);

        if (hex.HasRiver)
        {
            if (hex.HasRiverThroughEdge(direction))
            {
                e.v3.y = hex.StreamBedY;
                if (hex.HasRiverBeginOrEnd)
                {
                    TriangulateWithRiverBeginOrEnd(direction, hex, center, e);
                }
                else
                {
                    TriangulateWithRiver(direction, hex, center, e);
                }
            }
            else
            {
                TriangulateAdjacentToRiver(direction, hex, center, e);
            }
        }
        else
        {
            TriangulateEdgeFan(center, e, hex.Color);
        }






        if (direction <= HexDirection.SouthEast)
        {
            TriangulateConnection(direction, hex, neighbor, nextNeighbor, e, e2, v5);
        }

    }

    void TriangulateAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(center, e.v1, 0.5f),
            Vector3.Lerp(center, e.v5, 0.5f)
        );

        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
        TriangulateEdgeFan(center, m, cell.Color);
    }

    void TriangulateWithRiverBeginOrEnd(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        if (cell.HasRiverThroughEdge(direction.Next()))
        {
            if (cell.HasRiverThroughEdge(direction.Previous()))
            {
                center += HexMetrics.GetSolidEdgeMiddle(direction) *
                    (HexMetrics.innerToOuter * 0.5f);
            }
            else if (
                cell.HasRiverThroughEdge(direction.Previous2())
            )
            {
                center += cell.thisHex.GetFirstSolidCorner(direction) * 0.25f;
            }
        }
        else if (
            cell.HasRiverThroughEdge(direction.Previous()) &&
            cell.HasRiverThroughEdge(direction.Next2())
        )
        {
            center += cell.thisHex.GetSecondSolidCorner(direction) * 0.25f;
        }

        EdgeVertices m = new EdgeVertices(
            Vector3.Lerp(center, e.v1, 0.5f),
            Vector3.Lerp(center, e.v5, 0.5f)
        );

        m.v3.y = e.v3.y;

        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
        TriangulateEdgeFan(center, m, cell.Color);
    }

    void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e)
    {
        Vector3 centerL = center +
            cell.thisHex.GetFirstSolidCorner(direction.Previous()) * 0.25f;
        Vector3 centerR = center +
            cell.thisHex.GetSecondSolidCorner(direction.Next()) * 0.25f;

        if (cell.HasRiverThroughEdge(direction.Opposite()))
        {
            centerL = center + cell.thisHex.GetFirstSolidCorner(direction.Previous()) * 0.25f;

            centerR = center + cell.thisHex.GetSecondSolidCorner(direction.Next()) * 0.25f;
        }


        else if (cell.HasRiverThroughEdge(direction.Next()))
        {
            centerL = center;
            centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
        }
        else if (cell.HasRiverThroughEdge(direction.Previous()))
        {
            centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
            centerR = center;
        }
        else if (cell.HasRiverThroughEdge(direction.Next2()))
        {
            centerL = center;
            centerR = center +
                HexMetrics.GetSolidEdgeMiddle(direction.Next()) * (0.5f * HexMetrics.innerToOuter);
        }

        else
        {
            centerL = center +
                 HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * (0.5f * HexMetrics.innerToOuter);
            centerR = center;
        }

        center = Vector3.Lerp(centerL, centerR, 0.5f); // Dikkat

        EdgeVertices m = new EdgeVertices(Vector3.Lerp(centerL, e.v1, 0.5f), Vector3.Lerp(centerR, e.v5, 0.5f), 1f / 6f);
        m.v3.y = center.y = e.v3.y;
        TriangulateEdgeStrip(m, cell.Color, e, cell.Color);

        terrain.AddTriangle(centerL, m.v1, m.v2);
        terrain.AddTriangleColor(cell.Color);
        terrain.AddQuad(centerL, center, m.v2, m.v3);
        terrain.AddQuadColor(cell.Color);
        terrain.AddQuad(center, centerR, m.v3, m.v4);
        terrain.AddQuadColor(cell.Color);
        terrain.AddTriangle(centerR, m.v4, m.v5);
        terrain.AddTriangleColor(cell.Color);
    }

    void TriangulateConnection(HexDirection direction, HexCell hex, HexCell neighbor, HexCell nextNeighbor, EdgeVertices e1, EdgeVertices e2, Vector3 v5)
    {
        if (neighbor != null)
        {
            if (hex.HasRiverThroughEdge(direction))
            {
                e2.v3.y = neighbor.StreamBedY; // DIkkat
            }

            TriangulateEdges(direction, hex, neighbor, e1, e2);
            TriangulateCorners(direction, hex, neighbor, nextNeighbor, e1, e2, v5);
        }

    }


    void TriangulateEdges(HexDirection direction, HexCell hex, HexCell neighbor, EdgeVertices e1, EdgeVertices e2)
    {
        if (hex.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            SlopeEdgeConnection(direction, hex, neighbor, e1, e2);
        }

        else if (hex.GetEdgeType(direction) == HexEdgeType.Cliff)
        {
            TriangulateEdgeStrip(e1, hex.Color, e2, neighbor.Color);
        }

        else
        {
            TriangulateEdgeStrip(e1, hex.Color, e2, neighbor.Color);
        }
    }

    void TriangulateCorners(HexDirection direction, HexCell hex, HexCell neighbor, HexCell nextNeighbor, EdgeVertices e1, EdgeVertices e2, Vector3 v5)
    {

        if (nextNeighbor != null)
        {

            if (hex.Elevation <= neighbor.Elevation)
            {
                if (hex.Elevation <= nextNeighbor.Elevation)
                {

                    ElevatedCornerConnection(hex, e1.v5, neighbor, e2.v5, nextNeighbor, v5);
                }

                else
                {

                    ElevatedCornerConnection(nextNeighbor, v5, hex, e1.v5, neighbor, e2.v5);
                }
            }

            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {


                ElevatedCornerConnection(neighbor, e2.v5, nextNeighbor, v5, hex, e1.v5);
            }

            else
            {


                ElevatedCornerConnection(nextNeighbor, v5, hex, e1.v5, neighbor, e2.v5);
            }

        }
    }

    void ElevatedCornerConnection(HexCell bottomHex, Vector3 bottom, HexCell leftHex, Vector3 left, HexCell rightHex, Vector3 right)
    {
        HexEdgeType leftEdgeType = bottomHex.GetEdgeType(leftHex);
        HexEdgeType rightEdgeType = bottomHex.GetEdgeType(rightHex);

        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TerraceCornerConnection(bottom, bottomHex, left, leftHex, right, rightHex);
                return;
            }

            if (rightEdgeType == HexEdgeType.Flat)
            {
                TerraceCornerConnection(left, leftHex, right, rightHex, bottom, bottomHex);
                return;
            }

            SlopeToCliffConnection(bottom, bottomHex, left, leftHex, right, rightHex);
            return;

        }
        if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {
                TerraceCornerConnection(right, rightHex, bottom, bottomHex, left, leftHex);
                return;
            }

            CliffToSlopeConnection(bottom, bottomHex, left, leftHex, right, rightHex);
            return;
        }

        if (leftHex.GetEdgeType(rightHex) == HexEdgeType.Slope)
        {
            if (leftHex.Elevation < rightHex.Elevation)
            {
                CliffToSlopeConnection(right, rightHex, bottom, bottomHex, left, leftHex);
            }
            else
            {
                CliffToSlopeConnection(left, leftHex, right, rightHex, bottom, bottomHex);
            }
            return;
        }
        else
        {
            terrain.AddTriangle(bottom, left, right);
            terrain.AddTriangleColor(bottomHex.Color, leftHex.Color, rightHex.Color);
        }
    }

    void TerraceCornerConnection(Vector3 begin, HexCell beginHex, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightHex)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceColorLerp(beginHex.Color, leftCell.Color, 1);
        Color c4 = HexMetrics.TerraceColorLerp(beginHex.Color, rightHex.Color, 1);

        terrain.AddTriangle(begin, v3, v4);
        terrain.AddTriangleColor(beginHex.Color, c3, c4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceColorLerp(beginHex.Color, leftCell.Color, i);
            c4 = HexMetrics.TerraceColorLerp(beginHex.Color, rightHex.Color, i);
            terrain.AddQuad(v1, v2, v3, v4);
            terrain.AddQuadColor(c1, c2, c3, c4);
        }

        terrain.AddQuad(v3, v4, left, right);
        terrain.AddQuadColor(c3, c4, leftCell.Color, rightHex.Color);
    }

    void SlopeToCliffConnection(Vector3 begin, HexCell beginHex, Vector3 left, HexCell leftHex, Vector3 right, HexCell rightHex)
    {
        float b = 1f / (rightHex.Elevation - beginHex.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(right), b);
        Color boundaryColor = Color.Lerp(beginHex.Color, rightHex.Color, b);

        BoundaryTriangle(begin, beginHex, left, leftHex, boundary, boundaryColor);

        if (leftHex.GetEdgeType(rightHex) == HexEdgeType.Slope)
        {
            BoundaryTriangle(left, leftHex, right, rightHex, boundary, boundaryColor);
        }
        else
        {
            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleColor(leftHex.Color, rightHex.Color, boundaryColor);
        }

    }

    void CliffToSlopeConnection(Vector3 begin, HexCell beginHex, Vector3 left, HexCell leftHex, Vector3 right, HexCell rightHex)
    {
        float b = 1f / (leftHex.Elevation - beginHex.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(HexMetrics.Perturb(begin), HexMetrics.Perturb(left), b);
        Color boundaryColor = Color.Lerp(beginHex.Color, leftHex.Color, b);

        BoundaryTriangle(right, rightHex, begin, beginHex, boundary, boundaryColor);

        if (leftHex.GetEdgeType(rightHex) == HexEdgeType.Slope)
        {
            BoundaryTriangle(left, leftHex, right, rightHex, boundary, boundaryColor);
        }
        else
        {
            terrain.AddTriangleUnperturbed(HexMetrics.Perturb(left), HexMetrics.Perturb(right), boundary);
            terrain.AddTriangleColor(leftHex.Color, rightHex.Color, boundaryColor);

        }

    }

    void BoundaryTriangle(Vector3 begin, HexCell beginHex, Vector3 left, HexCell leftHex, Vector3 boundary, Color boundaryColor)
    {
        Vector3 v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, 1));

        Color c2 = HexMetrics.TerraceColorLerp(beginHex.Color, leftHex.Color, 1);

        terrain.AddTriangleUnperturbed(HexMetrics.Perturb(begin), v2, boundary);
        terrain.AddTriangleColor(beginHex.Color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceColorLerp(beginHex.Color, leftHex.Color, i);
            terrain.AddTriangleUnperturbed(v1, v2, boundary);
            terrain.AddTriangleColor(c1, c2, boundaryColor);
        }

        terrain.AddTriangleUnperturbed(v2, HexMetrics.Perturb(left), boundary);
        terrain.AddTriangleColor(c2, leftHex.Color, boundaryColor);
    }


    void SlopeEdgeConnection(HexDirection direction, HexCell beginHex, HexCell endHex, EdgeVertices begin, EdgeVertices end)
    {
        EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color c2 = HexMetrics.TerraceColorLerp(beginHex.Color, endHex.Color, 1);


        TriangulateEdgeStrip(begin, beginHex.Color, e2, c2);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            EdgeVertices e1 = e2;
            Color c1 = c2;

            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceColorLerp(beginHex.Color, endHex.Color, i);

            TriangulateEdgeStrip(e1, c1, e2, c2);
        }


        TriangulateEdgeStrip(e2, c2, end, endHex.Color);
    }

    void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        terrain.AddTriangle(center, edge.v1, edge.v2);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v2, edge.v3);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v3, edge.v4);
        terrain.AddTriangleColor(color);
        terrain.AddTriangle(center, edge.v4, edge.v5);
        terrain.AddTriangleColor(color);
    }

    void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
    {
        terrain.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        terrain.AddQuadColor(c1, c2);
        terrain.AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
        terrain.AddQuadColor(c1, c2);
    }

}
