using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
    NorthEast, East, SouthEast, SouthWest, West, NorthWest
}

public enum HexEdgeType
{
    Flat, Slope, Cliff
}

public class HexMetrics 
{
    public const float outerToInner = 0.866025404f;
    public const float innerToOuter = 1f / outerToInner;

    public static Texture2D noiseSource;
    public const float cellPerturbStrength = 0f; //1f;
    public const float noiseScale = 0.01f;
    public const float elevationPerturbStrenght = 0.00001f;

    public const float streamBedElevationoffset = -1f;


    public const int chunkSizeX = 20;
    public const int chunkSizeZ = 20;

    public const float solidFactor = 0.75f;
    public const float blendFactor = 1f - solidFactor;

    public const float elevationStep = 0.5f;

    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    public const float radius = 1f;
    public const float innerRadius = radius * outerToInner;

    public static Vector3[] hexCorners =
    {
            new Vector3(0f, 0f, HexMetrics.radius),
            new Vector3(HexMetrics.innerRadius, 0f, 0.5f * HexMetrics.radius),
            new Vector3(HexMetrics.innerRadius, 0f, -0.5f * HexMetrics.radius),
            new Vector3(0f, 0f, -HexMetrics.radius),
            new Vector3(-HexMetrics.innerRadius, 0f, -0.5f * HexMetrics.radius),
            new Vector3(-HexMetrics.innerRadius, 0f, 0.5f * HexMetrics.radius),
            new Vector3(0f,0f, HexMetrics.radius)
    };

    public static List<Hex> direction = new List<Hex>
    {
        new Hex(0,1,-1), new Hex(1,0,-1), new Hex(1,-1,0), new Hex(0,-1,1), new Hex(-1,0,1), new Hex(-1,1,0)
    };

    public static Hex WorldToHex(Vector3 worldPosition) //TODO: these numbers are hardcoded thinking that radius will be 1f however we need to adjust these values with the radius
    {
        float x = worldPosition.x / (HexMetrics.innerRadius * 2f);
        float y = -x;


        float offset = worldPosition.z / (HexMetrics.radius * 3f);

        x -= offset;
        y -= offset;

        int iX = (int)Mathf.Round(x);
        int iY = (int)Mathf.Round(y);
        int iZ = (int)Mathf.Round(-x-y);


        if (iX + iY + iZ != 0)
        {
            float dx = Mathf.Abs(x - iX);
            float dy = Mathf.Abs(y - iY);
            float dz = Mathf.Abs(-x - y - iZ);


            if (dx > dy && dx > dz)
            {
                iX = -iY - iZ;
            }
            else if (dz > dy)
            {
                iZ = -iX - iY;
            }
            else
            {
                iY = -iZ - iX;
            }


           
        }

        Debug.Log(iX + "," + iZ + ","  + iY);

        return new Hex(iX, -iX-iY,  iY);
    }


    public static Vector3 GetBridge(HexDirection direction)
    {
        return (HexMetrics.hexCorners[(int)direction] + hexCorners[(int)direction + 1]) * blendFactor;
        
    }


    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        float v = ((step + 1) / 2 * HexMetrics.verticalTerraceStepSize);
        a.y += (b.y - a.y) * v;

        return a;
    }

    public static Color TerraceColorLerp(Color a, Color b, int step)
    {
        float h = step * HexMetrics.horizontalTerraceStepSize;

        return Color.Lerp(a, b, h);
    }

    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }
        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1)
        {
            return HexEdgeType.Slope;
        }
        return HexEdgeType.Cliff;
    }


    public static Vector4 SampleNoise(Vector3 position)
    {
        return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
        
    }



    public static Point CubeToOffsetCoordinates(Hex hex)
    {
        int col = hex.q + (int)((hex.r - 1 * (hex.r & 1)) / 2);
        int row = hex.r;

        return new Point(col, row);
    }
    public static Vector3 GetSolidEdgeMiddle(HexDirection direction)
    {
        return
            (hexCorners[(int)direction] + hexCorners[(int)direction + 1]) *
            (0.5f * solidFactor);
    }


    public static Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = SampleNoise(position);
        position.x += (sample.x * 2 - 1) * cellPerturbStrength;
        //position.y += (sample.y * 2 - 1) * HexMetrics.cellPerturbStrength;
        position.z += (sample.z * 2 - 1) * cellPerturbStrength;
        return position;
    }

}


public struct Point
{
    public readonly int x;
    public readonly int y;


    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NorthEast ? HexDirection.NorthWest : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NorthWest ? HexDirection.NorthEast : (direction + 1);
    }

    public static HexDirection Previous2(this HexDirection direction)
    {
        direction -= 2;
        return direction >= HexDirection.NorthEast ? direction : (direction + 6);
    }

    public static HexDirection Next2(this HexDirection direction)
    {
        direction += 2;
        return direction <= HexDirection.NorthWest ? direction : (direction - 6);
    }

}