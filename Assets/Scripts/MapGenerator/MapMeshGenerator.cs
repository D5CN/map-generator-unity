using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MapMeshGenerator
{
    /**
     * 地图切割等级
     */ 
    public static int CUT_LEVEL = 3;
    public static int DISTANCE = 10;
    public static List<MapGraph.MapNode> nowNodeList= new List<MapGraph.MapNode>();
    private static float PL_relief = 100.0f;
    private static float PL_maxHeight = 0.14f;
    private static bool makeGrass;
    private static List<Vector3> grass_roots;
    private static HeightMap hmap;
    public static MeshData GenerateMesh(MapGraph mapGraph, HeightMap heightmap, int meshSize)
    {
        var meshData = new MeshData();
        meshData.vertices = new List<Vector3>();
        meshData.indices = new List<int>();
        hmap = heightmap;

        Vector2 p1 = new Vector2();
        Vector2 p2 = MapGenerator.firstLake==null ? new Vector2(0, 0) : new Vector2(MapGenerator.firstLake.centerPoint.x, MapGenerator.firstLake.centerPoint.z);
        Vector3 v0 = new Vector3();
        Vector3 v1 = new Vector3();
        Vector3 v2 = new Vector3();
        int count = 0;

        
        // 存放顶点与索引的临时字典类
        Dictionary<Vector3, int> verticesResultDic = new Dictionary<Vector3, int>();

        nowNodeList.RemoveAll(it=>true);
        var grassMacker = GameObject.Find("GrassMacker");
        if(grassMacker) grassMacker.SendMessage("clearCache");

        foreach (var node in mapGraph.nodesByCenterPosition.Values)
        {
            p1.x = node.centerPoint.x;
            p1.y = node.centerPoint.z;

            if (Vector2.Distance(p1, p2) > DISTANCE) continue;

            makeGrass = node.nodeType == MapGraph.MapNodeType.Grass;

            nowNodeList.Add(node);
            meshData.vertices.Add(node.centerPoint);
            v0 = node.centerPoint;
            var edges = node.GetEdges().ToList();

            count++;
            int count_edges = edges.Count();
            grass_roots = new List<Vector3>();
            for (var i = 0; i < count_edges; i++)
            {
                
                if (i == 0)
                {
                    v1 = edges[i].previous.destination.position;
                    v2 = edges[i].destination.position;
                    cutMesh(meshData, v0, v1, v2, verticesResultDic, CUT_LEVEL);
                }
                else if (i <= edges.Count() -1)
                {
                    v1 = edges[i].destination.position;
                    cutMesh(meshData, v0, v2, v1, verticesResultDic, CUT_LEVEL);
                    v2 = v1;
                }
            }

            if(makeGrass && GameObject.Find("GrassMacker"))
            {
                System.Object[] opt = new System.Object[2] { node, grass_roots.ToArray() };
                GameObject.Find("GrassMacker").SendMessage("addGrass", opt);
            }
        }

        meshData.uvs = new Vector2[meshData.vertices.Count];
        for (int i = 0; i < meshData.uvs.Length; i++)
        {
            meshData.uvs[i] = new Vector2(meshData.vertices[i].x / meshSize, meshData.vertices[i].z / meshSize);
        }

        Debug.Log(string.Format("There are {0:n0} vertices ", meshData.vertices.Count()));
        return meshData;
    }

    public static HeightMap heightMap
    {
        get { return hmap; }
    }

    public static MapGraph.MapNode isIn(Vector2 p)
    {
        MapGraph.MapNode result=null;
        foreach(var node in nowNodeList)
        {
            var r = node.GetBoundingRectangle();
            if(r.Contains(p))
            {
                result = node;
            }
        }

        return result;
    }

    private static void cutMesh(MeshData meshData,Vector3 v0,Vector3 v1,Vector3 v2,Dictionary<Vector3,int> verticesResultDic,int level)
    {
        Vector3 v01 = (v0 + v1) * .5f;
        Vector3 v12 = (v1 + v2) * .5f;
        Vector3 v02 = (v0 + v2) * .5f;

        v01 = plRandY(v01);
        v12 = plRandY(v12);
        v02 = plRandY(v02);

        int i0;
        int i1;
        int i2;

        if(level>0)
        {
            level--;
            cutMesh(meshData, v0, v01, v02, verticesResultDic, level);
            cutMesh(meshData, v01, v1, v12, verticesResultDic, level);
            cutMesh(meshData, v2, v02, v12, verticesResultDic, level);
            cutMesh(meshData, v02, v01, v12, verticesResultDic, level);
        }
        else
        {
            i0 = AddVertices(verticesResultDic, v0, meshData);
            i1 = AddVertices(verticesResultDic, v01, meshData);
            i2 = AddVertices(verticesResultDic, v02, meshData);
            AddTriangle(meshData, i0, i1, i2);


            i0 = AddVertices(verticesResultDic, v01, meshData);
            i1 = AddVertices(verticesResultDic, v1, meshData);
            i2 = AddVertices(verticesResultDic, v12, meshData);
            AddTriangle(meshData, i0, i1, i2);

            i0 = AddVertices(verticesResultDic, v2, meshData);
            i1 = AddVertices(verticesResultDic, v02, meshData);
            i2 = AddVertices(verticesResultDic, v12, meshData);
            AddTriangle(meshData, i0, i1, i2);

            i0 = AddVertices(verticesResultDic, v02, meshData);
            i1 = AddVertices(verticesResultDic, v01, meshData);
            i2 = AddVertices(verticesResultDic, v12, meshData);
            AddTriangle(meshData, i0, i1, i2);
        }
    }

    private static Vector3 plRandY(Vector3 vertice)
    {
        // 利用噪声随机地形
        float y = 0;
        float xSample = (vertice.x) / PL_relief;
        float zSample = (vertice.z) / PL_relief;
        float noise = Mathf.PerlinNoise(xSample, zSample);
        y = PL_maxHeight * noise;
        //Debug.Log(string.Format("Noice {0:n0} vertices ", y));
        vertice.y += y;
        return vertice;
    }

    private static int AddVertices(Dictionary<Vector3, int> verticesResultDic, Vector3 vertice,MeshData meshData)
    {
        if (verticesResultDic.ContainsKey(vertice))
            return verticesResultDic[vertice];


        
        if (makeGrass) grass_roots.Add(vertice);
        meshData.vertices.Add(vertice);
        int index = meshData.vertices.Count - 1;
        verticesResultDic.Add(vertice, index);
        return index;
    }
    

    private static void AddTriangle(MeshData meshData, int v1, int v2, int v3)
    {
        meshData.indices.Add(v1);
        meshData.indices.Add(v2);
        meshData.indices.Add(v3);
    }

    
}