using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex
{
    public readonly int q;
    public readonly int r;
    public readonly int s;

    public int offsetCordX;
    public int offsetCordZ;

    public int elevation = 0;

    public Vector3 center;
    public Vector3[] corners = new Vector3[7];

    public Hex(int q, int r,int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;

    }



    public void SetCorners()
    {
        for (int i = 0; i < HexMetrics.hexCorners.Length; i++)
        {
            Vector3 corner = HexMetrics.hexCorners[i] + center;
            corners[i] = corner;
        }
    }


    public Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    public Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return (HexMetrics.hexCorners[(int)direction] * HexMetrics.solidFactor);
    }

    public Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return (HexMetrics.hexCorners[(int)direction + 1] * HexMetrics.solidFactor);
    }


    public Hex AddHex(Hex hex)
    {
        int newQ = hex.q + q;
        int newR = hex.r + r;
        int newS = hex.s + s;

        return new Hex(newQ, newR, newS);

    }

}