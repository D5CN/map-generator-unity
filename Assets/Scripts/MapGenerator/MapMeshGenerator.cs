using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MapMeshGenerator
{

    public static MeshData GenerateMesh(MapGraph mapGraph, HeightMap heightmap, int meshSize)
    {
        var meshData = new MeshData();
        meshData.vertices = new List<Vector3>();
        meshData.indices = new List<int>();

        Vector2 p1 = new Vector2();
        Vector2 p2 = MapGenerator.firstLake==null ? new Vector2(0, 0) : new Vector2(MapGenerator.firstLake.centerPoint.x, MapGenerator.firstLake.centerPoint.z);
        int count = 0;
        foreach(var node in mapGraph.nodesByCenterPosition.Values)
        {
            meshData.vertices.Add(node.centerPoint);
            var centerIndex = meshData.vertices.Count - 1;
            var edges = node.GetEdges().ToList();
            p1.x = node.centerPoint.x;
            p1.y = node.centerPoint.z;

            if (Vector2.Distance(p1, p2) > 16) continue;

            count++;
            int lastIndex = 0;
            int firstIndex = 0;
            for (var i = 0; i < edges.Count(); i++)
            {
                
                if (i == 0)
                {
                    meshData.vertices.Add(edges[i].previous.destination.position);
                    var i2 = meshData.vertices.Count - 1;
                    meshData.vertices.Add(edges[i].destination.position);
                    var i3 = meshData.vertices.Count - 1;
                    AddTriangle(meshData, centerIndex, i2, i3);
                    firstIndex = i2;
                    lastIndex = i3;
                }
                else if (i < edges.Count() -1)
                {
                    meshData.vertices.Add(edges[i].destination.position);
                    var currentIndex = meshData.vertices.Count - 1;
                    AddTriangle(meshData, centerIndex, lastIndex, currentIndex);
                    lastIndex = currentIndex;
                } 
                else
                {
                    AddTriangle(meshData, centerIndex, lastIndex, firstIndex);
                }
            }
        }

        meshData.uvs = new Vector2[meshData.vertices.Count];
        for (int i = 0; i < meshData.uvs.Length; i++)
        {
            meshData.uvs[i] = new Vector2(meshData.vertices[i].x / meshSize, meshData.vertices[i].z / meshSize);
        }

        Debug.Log(string.Format("There are {0:n0} Nodes ", meshData.uvs.Length));
        return meshData;
    }

    private static void AddTriangle(MeshData meshData, int v1, int v2, int v3)
    {
        meshData.indices.Add(v1);
        meshData.indices.Add(v2);
        meshData.indices.Add(v3);
    }

    
}