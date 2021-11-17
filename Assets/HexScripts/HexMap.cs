using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class HexMap : MonoBehaviour
{
    public HexCell hexPrefab;
    public HexGridChunk chunkPrefab;
    public HexCell[] hexes;
    public HexGridChunk[] chunks;

    public Color defaultColor = Color.blue;
    public Color touchedColor = Color.magenta;
    public Texture2D noiseSource;

    public int chunkCountX = 2;
    public int chunkCountZ = 2;

    int cellCountX;
    int cellCountZ;

    void Awake()
    {
        
        HexMetrics.noiseSource = noiseSource;

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        hexes = new HexCell[cellCountX * cellCountZ];

        GenerateMap();
    }

    private void GenerateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int i = 0, z = 0; z < chunkCountX * chunkCountZ; z++)
        {
            HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
            chunk.transform.SetParent(transform);
        }
    }

    private void GenerateMap()
    {
        GenerateChunks();

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                GenerateHex(x, z, i++);
            }
        }

    }

    


    void GenerateHex(int x, int z, int i)
    {
        Vector3 center;

        center.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        center.y = 0f;
        center.z = z * (HexMetrics.radius * 1.5f);
        int q = x - z / 2;
        int r = z;
        Hex hex = new Hex(q, r, -q-r);
        hex.center = center;
        hex.SetCorners();
        hex.offsetCordX = x;
        hex.offsetCordZ = z;

        HexCell hexCell = hexes[i] = Instantiate<HexCell>(hexPrefab);
        SetHexNeighbors(x, z, i, hexCell);

        var tmp = hexCell.GetComponentInChildren<TextMeshPro>();

        tmp.text = (hex.q + "," + hex.r + "," + hex.s);

        center.y = 0.1f;
        tmp.transform.position = center;

        hexCell.thisHex = hex;
        hexCell.Elevation = 0;
        hexCell.Color = defaultColor;
        var offsetPoints = HexMetrics.CubeToOffsetCoordinates(hex);

        AddCellToChunk(offsetPoints,hexCell);

        


    }

    void SetHexNeighbors(int x, int z, int i, HexCell cell)
    {
        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.West, hexes[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SouthEast, hexes[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SouthWest, hexes[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SouthWest, hexes[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SouthEast, hexes[i - cellCountX + 1]);
                }
            }
        }
    }

    void AddCellToChunk(Point point, HexCell hex)
    {
        int chunkX = point.x / HexMetrics.chunkSizeX;
        int chunkZ = point.y / HexMetrics.chunkSizeZ;

        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = point.x - chunkX * HexMetrics.chunkSizeX;
        int localZ = point.y - chunkZ * HexMetrics.chunkSizeZ;

        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, hex);
        
    }


    public HexCell GetSelectedHex(Vector3 hitPosition)
    {
        hitPosition = transform.InverseTransformPoint(hitPosition);

        Hex h = HexMetrics.WorldToHex(hitPosition);

        int index = h.q + h.r * cellCountX + h.r / 2;

        HexCell cell = hexes[index];

        Debug.Log(hitPosition);
        Debug.Log(h.q + "," + h.r);


        return cell;
    }

    public void OnEnable()
    {
        HexMetrics.noiseSource = noiseSource;
    }

    public HexCell GetCell(Hex hex)
    {
        int z = hex.r;
        if (z < 0 || z >= cellCountZ)
        {
            return null;
        }

        
        int x = hex.q + z / 2;
        if (x < 0 || x >= cellCountX)
        {
            return null;
        }


        return hexes[x + z * cellCountX];
    }
}
