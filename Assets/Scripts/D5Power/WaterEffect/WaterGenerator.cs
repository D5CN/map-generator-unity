// 2016.5.22 luoyinan 自动生成水面
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace d5power
{
    public enum MeshType
    {
        Gizmos_FullMesh,
        Gizmos_WaterMesh,
        WaterMesh,
    }

    public class WaterGenerator : MonoBehaviour
    {
        [Range(1, 200)]
        public int halfWidth = 20;
        [Range(1, 200)]
        public int halfHeight = 20;
        public float gridSize = 2.0f;
        public float maxWaterDepth = 4.0f;
        public Material material;

        string m_ShaderName = "D5Power/WaterEffect/WaterSurface";
        Mesh m_GizmosMesh;
        Vector3 m_LocalPos;
        int m_GridNumX;
        int m_GridNumZ;

        bool[] m_VerticesFlag;
        float[] m_VerticesAlpha;
        Vector3[] m_Vertices;
        int[] m_Triangles;
        List<Vector3> m_VerticesList = new List<Vector3>();
        List<Color> m_ColorsList = new List<Color>();
        List<Vector2> m_UvList = new List<Vector2>();
        List<int> m_TrianglesList = new List<int>();

        void OnDrawGizmosSelected()
        {
            // 水面指示器
            Gizmos.matrix = transform.localToWorldMatrix;
            if (m_GizmosMesh != null)
                UnityEngine.Object.DestroyImmediate(m_GizmosMesh);
            m_GizmosMesh = CreateMesh(MeshType.Gizmos_WaterMesh);
            Gizmos.DrawWireMesh(m_GizmosMesh);
        }

        public void GenerateWater()
        {
            Mesh mesh = CreateMesh(MeshType.WaterMesh);

            // 渲染水面
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
            string name = string.Format("{0}_{1}_{2}", SceneManager.GetActiveScene().name, transform.name, transform.position);
            mesh.name = name;
            GameObject go = new GameObject(name);
            go.transform.parent = transform;
            go.transform.localPosition = m_LocalPos;
            go.layer = LayerMask.NameToLayer("Water");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            if (material == null)
                material = new Material(Shader.Find(m_ShaderName));
            mr.sharedMaterial = material;
            mf.sharedMesh = mesh;

            // obj模型不支持顶点颜色,所以暂时不导出了.
            // 导出obj模型
            //MeshToFile(mf, "Assets/" + name + ".obj");
        }

        Mesh CreateMesh(MeshType type)
        {
            Mesh mesh = new Mesh();
            mesh.MarkDynamic();
            m_GridNumX = halfWidth * 2;
            m_GridNumZ = halfHeight * 2;
            Vector3 centerOffset = new Vector3(-halfWidth, 0, -halfHeight);

            if (type == MeshType.Gizmos_FullMesh)
            {
                int vectices_num = m_GridNumX * m_GridNumZ * 4;
                int triangles_num = m_GridNumX * m_GridNumZ * 6;
                m_Vertices = new Vector3[vectices_num];
                m_Triangles = new int[triangles_num];

                // 从左下角开始创建,三角形索引顺时针是正面.
                // 2 3
                // 0 1
                for (int z = 0; z < m_GridNumZ; ++z)
                {
                    for (int x = 0; x < m_GridNumX; ++x)
                    {
                        int index = x + z * m_GridNumX;
                        int i = index * 4;
                        int j = index * 6;

                        m_Vertices[i] = GetVertexLocalPos(x, z, centerOffset);
                        m_Vertices[i + 1] = GetVertexLocalPos(x + 1, z, centerOffset);
                        m_Vertices[i + 2] = GetVertexLocalPos(x, z + 1, centerOffset);
                        m_Vertices[i + 3] = GetVertexLocalPos(x + 1, z + 1, centerOffset);

                        m_Triangles[j] = i;
                        m_Triangles[j + 1] = i + 2;
                        m_Triangles[j + 2] = i + 3;
                        m_Triangles[j + 3] = i + 3;
                        m_Triangles[j + 4] = i + 1;
                        m_Triangles[j + 5] = i;
                    }
                }

                mesh.vertices = m_Vertices;
                mesh.triangles = m_Triangles;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
            else if (type == MeshType.Gizmos_WaterMesh)
            {
                CalcWaterMesh();

                m_VerticesList.Clear();
                m_ColorsList.Clear();
                m_TrianglesList.Clear();
                int counter = 0;

                // 从左下角开始创建,三角形索引顺时针是正面.
                // 2 3
                // 0 1
                for (int z = 0; z < m_GridNumZ; ++z)
                {
                    for (int x = 0; x < m_GridNumX; ++x)
                    {
                        if (!IsValidGrid(x, z))
                            continue;

                        int i = counter * 4;

                        m_VerticesList.Add(GetVertexLocalPos(x, z, centerOffset));
                        m_VerticesList.Add(GetVertexLocalPos(x + 1, z, centerOffset));
                        m_VerticesList.Add(GetVertexLocalPos(x, z + 1, centerOffset));
                        m_VerticesList.Add(GetVertexLocalPos(x + 1, z + 1, centerOffset));

                        m_TrianglesList.Add(i);
                        m_TrianglesList.Add(i + 2);
                        m_TrianglesList.Add(i + 3);
                        m_TrianglesList.Add(i + 3);
                        m_TrianglesList.Add(i + 1);
                        m_TrianglesList.Add(i);

                        ++counter;
                    }
                }

                // 忘记添加collider了?
                if (m_VerticesList.Count == 0)
                    return CreateMesh(MeshType.Gizmos_FullMesh);

                mesh.vertices = m_VerticesList.ToArray();
                mesh.triangles = m_TrianglesList.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
            else if (type == MeshType.WaterMesh)
            {
                CalcWaterMesh();

                m_VerticesList.Clear();
                m_ColorsList.Clear();
                m_UvList.Clear();
                m_TrianglesList.Clear();
                int counter = 0;

                // 先循环一次,找出最小最大的格子,让UV计算更精确.
                int minX = m_GridNumX - 1;
                int minZ = m_GridNumZ - 1;
                int maxX = 0;
                int maxZ = 0;
                for (int z = 0; z < m_GridNumZ; ++z)
                {
                    for (int x = 0; x < m_GridNumX; ++x)
                    {
                        if (!IsValidGrid(x, z))
                            continue;

                        minX = (x < minX) ? x : minX;
                        minZ = (z < minZ) ? z : minZ;
                        maxX = (x > maxX) ? x : maxX;
                        maxZ = (z > maxZ) ? z : maxZ;
                    }
                }
                int newGridNumX = maxX - minX + 1;
                int newGridNumZ = maxZ - minZ + 1;

                // 创建的水面模型应该做原点矫正,以自己形状的中心为原点,这样好支持水波纹的计算.详见WaterRippleGenerateTool
                float halfGridNumX = (float)newGridNumX * 0.5f;
                float halfGridNumZ = (float)newGridNumZ * 0.5f;
                Vector3 offsetAdjust = new Vector3(-(float)minX - halfGridNumX, 0, -(float)minZ - halfGridNumZ);
                m_LocalPos = (centerOffset - offsetAdjust) * gridSize;
                centerOffset = offsetAdjust;

                // TODO: 水面中心某些alpha为1的顶点其实可以去掉,需要一个自动减面算法.

                // 从左下角开始创建,三角形索引顺时针是正面.
                // 2 3
                // 0 1
                for (int z = 0; z < m_GridNumZ; ++z)
                {
                    for (int x = 0; x < m_GridNumX; ++x)
                    {
                        if (!IsValidGrid(x, z))
                            continue;

                        int i = counter * 4;
                        int newX = x - minX;
                        int newZ = z - minZ;

                        m_VerticesList.Add(GetVertexLocalPos(x, z, centerOffset));
                        m_VerticesList.Add(GetVertexLocalPos(x + 1, z, centerOffset));
                        m_VerticesList.Add(GetVertexLocalPos(x, z + 1, centerOffset));
                        m_VerticesList.Add(GetVertexLocalPos(x + 1, z + 1, centerOffset));

                        m_ColorsList.Add(new Color(1.0f, 1.0f, 1.0f, GetVertexAlpha(x, z)));
                        m_ColorsList.Add(new Color(1.0f, 1.0f, 1.0f, GetVertexAlpha(x + 1, z)));
                        m_ColorsList.Add(new Color(1.0f, 1.0f, 1.0f, GetVertexAlpha(x, z + 1)));
                        m_ColorsList.Add(new Color(1.0f, 1.0f, 1.0f, GetVertexAlpha(x + 1, z + 1)));

                        m_UvList.Add(new Vector2((float)(newX) / (float)newGridNumX, (float)newZ / (float)newGridNumZ));
                        m_UvList.Add(new Vector2((float)(newX + 1) / (float)newGridNumX, (float)newZ / (float)newGridNumZ));
                        m_UvList.Add(new Vector2((float)newX / (float)newGridNumX, (float)(newZ + 1) / (float)newGridNumZ));
                        m_UvList.Add(new Vector2((float)(newX + 1) / (float)newGridNumX, (float)(newZ + 1) / (float)newGridNumZ));

                        m_TrianglesList.Add(i);
                        m_TrianglesList.Add(i + 2);
                        m_TrianglesList.Add(i + 3);
                        m_TrianglesList.Add(i + 3);
                        m_TrianglesList.Add(i + 1);
                        m_TrianglesList.Add(i);

                        ++counter;
                    }
                }

                mesh.vertices = m_VerticesList.ToArray();
                mesh.colors = m_ColorsList.ToArray();
                mesh.uv = m_UvList.ToArray();
                mesh.triangles = m_TrianglesList.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }

            return mesh;
        }

        void CalcWaterMesh()
        {
            int VerticesNum = (m_GridNumX + 1) * (m_GridNumZ + 1);
            m_VerticesFlag = new bool[VerticesNum];
            m_VerticesAlpha = new float[VerticesNum];

            WaterBoundaryFill8(halfWidth, halfHeight, transform.position.y);
        }

        // 八方向的边界填充算法
        void WaterBoundaryFill8(int x, int z, float boundaryHeight)
        {
            int index = x + z * (m_GridNumX + 1);
            if (m_VerticesFlag[index])
                return;

            float height = GetHeight(x, z);
            if (height <= boundaryHeight)
            {
                m_VerticesFlag[index] = true;
                float difference = Mathf.Clamp(boundaryHeight - height, 0, maxWaterDepth);
                m_VerticesAlpha[index] = Mathf.Clamp01(difference / maxWaterDepth);

                if (x + 1 < m_GridNumX + 1 && x - 1 >= 0 && z + 1 < m_GridNumZ + 1 && z - 1 >= 0)
                {
                    WaterBoundaryFill8(x + 1, z, boundaryHeight);
                    WaterBoundaryFill8(x - 1, z, boundaryHeight);
                    WaterBoundaryFill8(x, z + 1, boundaryHeight);
                    WaterBoundaryFill8(x, z - 1, boundaryHeight);

                    WaterBoundaryFill8(x - 1, z - 1, boundaryHeight);
                    WaterBoundaryFill8(x + 1, z - 1, boundaryHeight);
                    WaterBoundaryFill8(x - 1, z + 1, boundaryHeight);
                    WaterBoundaryFill8(x + 1, z + 1, boundaryHeight);
                }
            }
        }

        float GetHeight(int x, int z)
        {
            float height = float.MinValue;
            Vector3 centerOffset = new Vector3(-m_GridNumX * 0.5f, 0, -m_GridNumZ * 0.5f);
            Vector3 worldPos = GetVertexLocalPos(x, z, centerOffset) + transform.position;
            worldPos.y += 100.0f;
            RaycastHit hit;
            if (Physics.Raycast(worldPos, -Vector3.up, out hit, 200.0f))
            {
                height = hit.point.y;
            }
            else
            {
                //LogSystem.DebugLog("Physics.Raycast失败,请检查是否有Collider. x:{0} z:{0}", x, z);
            }

            return height;
        }

        bool IsValidGrid(int x, int z)
        {
            // 4个顶点只要有一个合法,就算合法.
            if (isValidVertex(x, z))
                return true;
            if (isValidVertex(x + 1, z))
                return true;
            if (isValidVertex(x, z + 1))
                return true;
            if (isValidVertex(x + 1, z + 1))
                return true;

            return false;
        }

        bool isValidVertex(int x, int z)
        {
            int index = x + z * (m_GridNumX + 1);
            return m_VerticesFlag[index];
        }

        float GetVertexAlpha(int x, int z)
        {
            int index = x + z * (m_GridNumX + 1);
            return m_VerticesAlpha[index];
        }
        Vector3 GetVertexLocalPos(int x, int z, Vector3 centerOffset)
        {
            return new Vector3((x + centerOffset.x) * gridSize, 0, (z + centerOffset.z) * gridSize);
        }
        // 暂时没用到
        bool IsNearbyBoundary(int x, int z, float boundaryHeight)
        {
            float height = GetHeight(x + 1, z);
            if (height > boundaryHeight)
                return true;
            height = GetHeight(x - 1, z);
            if (height > boundaryHeight)
                return true;
            height = GetHeight(x, z + 1);
            if (height > boundaryHeight)
                return true;
            height = GetHeight(x, z - 1);
            if (height > boundaryHeight)
                return true;

            return false;
        }

        public void MeshToFile(MeshFilter mf, string filename)
        {
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.Write(MeshToString(mf));
            }
        }

        public string MeshToString(MeshFilter mf)
        {
            Mesh m = mf.sharedMesh;
            Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

            StringBuilder sb = new StringBuilder();

            sb.Append("g ").Append(mf.name).Append("\n");
            foreach (Vector3 v in m.vertices)
            {
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.normals)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            for (int material = 0; material < m.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append(mats[material].name).Append("\n");
                sb.Append("usemap ").Append(mats[material].name).Append("\n");

                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                        triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                }
            }
            return sb.ToString();
        }
    }
}