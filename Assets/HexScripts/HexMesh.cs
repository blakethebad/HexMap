using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    

    Mesh hexMesh;
    MeshCollider meshCollider;

    [NonSerialized] List<Vector3> vertices;
    [NonSerialized] List<Color> colors;
    [NonSerialized] List<int> triangles;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";

        meshCollider = gameObject.AddComponent<MeshCollider>();
    }


    public void Clear()
    {
        hexMesh.Clear();
        vertices = ListPool<Vector3>.Get();
        colors = ListPool<Color>.Get();
        triangles = ListPool<int>.Get();
    }

    public void Apply()
    {
        hexMesh.SetVertices(vertices);
        ListPool<Vector3>.Add(vertices);
        hexMesh.SetColors(colors);
        ListPool<Color>.Add(colors);
        hexMesh.SetTriangles(triangles, 0);
        ListPool<int>.Add(triangles);
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }



    public void AddTriangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics.Perturb(vertex1));
        vertices.Add(HexMetrics.Perturb(vertex2));
        vertices.Add(HexMetrics.Perturb(vertex3));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }



    public void AddQuad(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Vector3 vertex4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(HexMetrics.Perturb(vertex1));
        vertices.Add(HexMetrics.Perturb(vertex2));
        vertices.Add(HexMetrics.Perturb(vertex3));
        vertices.Add(HexMetrics.Perturb(vertex4));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }


    public void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

    }

    public void AddTriangleColor( Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    public void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    public void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    public void AddQuadColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

  


}


public static class ListPool<T>
{
    static Stack<List<T>> stack = new Stack<List<T>>();

    public static List<T> Get()
    {
        if (stack.Count > 0)
        {
            return stack.Pop();
        }
        return new List<T>();
    }

    public static void Add(List<T> list)
    {
        list.Clear();
        stack.Push(list);
    }
}