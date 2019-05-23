using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GpuGrass : MonoBehaviour
{
    public uint grass_layer = 5;
    public float grass_width = .125f;
    public float grass_scale = .4f;
    private uint count = 1023;
    private Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        float per = 1f / grass_layer;
        int v0;
        int v1;
        int v2;
        int v3;
        Dictionary<Vector3, int> verticesResultDic = new Dictionary<Vector3, int>();
        if (grass_layer < 2) grass_layer = 2;
        
        for (var i=0;i<grass_layer;i++)
        {

            v0 = AddVertices(verticesResultDic, newVertices, new Vector3(0, i * per, 0));
            v1 = AddVertices(verticesResultDic, newVertices, new Vector3(grass_width, i * per, 0));
            v2 = AddVertices(verticesResultDic, newVertices, new Vector3(0, (i + 1) * per, 0));
            v3 = AddVertices(verticesResultDic, newVertices, new Vector3(grass_width, (i + 1) * per, 0));

            newTriangles.Add(v0);
            newTriangles.Add(v1);
            newTriangles.Add(v2);

            newTriangles.Add(v1);
            newTriangles.Add(v3);
            newTriangles.Add(v2);

        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
    }

    private int AddVertices(Dictionary<Vector3, int> verticesResultDic, List<Vector3> meshData, Vector3 vertice)
    {
        if (verticesResultDic.ContainsKey(vertice))
            return verticesResultDic[vertice];
        
        meshData.Add(vertice);
        int index = meshData.Count - 1;
        verticesResultDic.Add(vertice, index);
        return index;
    }

    private void clearCache()
    {
        if(nodeList!=null) nodeList.Clear();
    }

    private Dictionary<MapGraph.MapNode,Matrix4x4[]> nodeList = new Dictionary<MapGraph.MapNode, Matrix4x4[]>();
    private void addGrass(System.Object[] data)
    {
        MapGraph.MapNode node = (MapGraph.MapNode)data[0];
        Vector3[] roots = (Vector3[])data[1];

        if (node==null || roots.Length==0 || nodeList.ContainsKey(node)) return;

        int len = roots.Length;
        var node_matrics = new Matrix4x4[len];
        var rec = node.GetBoundingRectangle();
        


        for (int i = 0; i < roots.Length; i++)
        {
            var center = roots[i];
            madeGrass(node_matrics,center.x,center.y,center.z,i);
        }
        
        nodeList.Add(node,node_matrics);
    }

    private void madeGrass(Matrix4x4[] list,float x,float y,float z,int index)
    {
        float k = MapGeneratorPreview.SCALE_K;
        //var rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        var rotation = Quaternion.Euler(0,0,0);
        var matrix = Matrix4x4.TRS(new Vector3(x * k, y * k, z * k), rotation, Vector3.one*grass_scale);
        list[index] = matrix;
    }

    // Update is called once per frame
    void Update()
    {
        if (nodeList == null) return;
        var meshRenderer = GetComponent<MeshRenderer>();
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;

        foreach(var matrics in nodeList.Values)
        {
            Graphics.DrawMeshInstanced(mesh, 0, meshRenderer.sharedMaterial, matrics);
        }
        

        
    }
}
