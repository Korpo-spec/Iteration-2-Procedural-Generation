using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MeshBuilder
{
    public readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector3> normals  = new List<Vector3>();
    private readonly List<Vector2> uv       = new List<Vector2>();
    private readonly List<int> triangles = new List<int>();
    public Matrix4x4 VertexMatrix = Matrix4x4.identity;
    public Matrix4x4 TextureMatrix = Matrix4x4.identity;
    public Mesh mesh;
    public Color[] pixels;
    public GameObject g;

    public MeshBuilder(Mesh meshToBuild)
    {
        mesh = meshToBuild;
    }
    public void Build(Mesh mesh)
    {
        
        
        
        
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uv);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        
        mesh.MarkModified();
        mesh.RecalculateTangents();
        vertices.Clear();
        normals.Clear();
        uv.Clear();
        triangles.Clear();
    }
    public void Build()
    {
        
        
        //Debug.Log(triangles.Count, gameObject);
        
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uv);
        mesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        
        mesh.MarkModified();
        mesh.RecalculateTangents();
        mesh.RecalculateNormals();
        
    }
    public int AddVertex(Vector3 position, Vector3 normal, Vector2 uv) {
        int index = vertices.Count;
        vertices.Add(VertexMatrix.MultiplyPoint(position));
        normals.Add(VertexMatrix.MultiplyVector(normal));
        this.uv.Add(TextureMatrix.MultiplyPoint(uv));
        return index;
    }

    public void RemoveLastVertex(int amount)
    {
        triangles.Clear();
        vertices.Clear();
        normals.Clear();
        this.uv.Clear();
        for (int i = 0; i < amount; i++)
        {
            if (vertices.Count > 0&& normals.Count >0 && uv.Count>0)
            {
                vertices.RemoveAt(vertices.Count - 1);
                normals.RemoveAt(normals.Count - 1);
                this.uv.RemoveAt(uv.Count-1);
            }
            else
            {
                break;
            }
            
        }
        
    }

    public void AddTri(int bottomLeft, int topLeft, int topRight)
    {
        triangles.Add(bottomLeft);
        triangles.Add(topLeft);
        triangles.Add(topRight);
    }

    public void AddTri(int[] indexes)
    {
        foreach (var index in indexes)
        {
            triangles.Add(index);
        }
    }
    public void AddQuad(int bottomLeft, int topLeft, 
        int topRight, int bottomRight) {
        // First triangle
        triangles.Add(bottomLeft);
        triangles.Add(topLeft);
        triangles.Add(topRight);
        // Second triangle
        triangles.Add(bottomLeft);
        triangles.Add(topRight);
        triangles.Add(bottomRight);
    }

    public int ReplaceVector(int index, Vector3 position, Vector3 normal, Vector2 uv)
    {
        vertices[index] = VertexMatrix.MultiplyPoint(position);
        normals[index] = VertexMatrix.MultiplyVector(normal);
        this.uv[index] = TextureMatrix.MultiplyPoint(uv);
        return index;
    }
    public void Add4xTriQuad(int[] vertexes)
    {
        
        for (int i = 0; i < 12; i++)
        {
            triangles.Add(vertexes[i]);
        }
    }
}


