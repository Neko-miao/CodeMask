// ================================================
// Editor 工具 - Mesh 生成器
// ================================================

using UnityEngine;
using UnityEditor;
using System.IO;

namespace EditorTools
{
    public static class MeshGenerator
    {
        /// <summary>
        /// 生成大型 Quad Mesh
        /// </summary>
        [MenuItem("Tools/Generate Large Quad Mesh")]
        public static void GenerateLargeQuadMesh()
        {
            // 创建目录
            string folderPath = "Assets/Resources/Scenes/Models";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "Scenes");
                }
                AssetDatabase.CreateFolder("Assets/Resources/Scenes", "Models");
            }

            // 生成 100x100 大小的 Quad Mesh
            Mesh quadMesh = CreateQuadMesh(100f, 100f, "LargeQuadMesh");
            SaveMeshAsset(quadMesh, folderPath + "/LargeQuadMesh.asset");

            // 生成 500x500 大小的 Quad Mesh
            Mesh hugeQuadMesh = CreateQuadMesh(500f, 500f, "HugeQuadMesh");
            SaveMeshAsset(hugeQuadMesh, folderPath + "/HugeQuadMesh.asset");

            // 生成 1000x1000 大小的 Quad Mesh
            Mesh giantQuadMesh = CreateQuadMesh(1000f, 1000f, "GiantQuadMesh");
            SaveMeshAsset(giantQuadMesh, folderPath + "/GiantQuadMesh.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MeshGenerator] 已生成以下 Quad Mesh:\n" +
                      "- LargeQuadMesh (100x100)\n" +
                      "- HugeQuadMesh (500x500)\n" +
                      "- GiantQuadMesh (1000x1000)\n" +
                      "位置: Resources/Scenes/Models/");
        }

        /// <summary>
        /// 创建 Quad Mesh
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="name">Mesh 名称</param>
        /// <returns>生成的 Mesh</returns>
        private static Mesh CreateQuadMesh(float width, float height, string name)
        {
            Mesh mesh = new Mesh();
            mesh.name = name;

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            // 顶点 (4个顶点组成一个面片)
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-halfWidth, 0, -halfHeight),  // 左下
                new Vector3(halfWidth, 0, -halfHeight),   // 右下
                new Vector3(-halfWidth, 0, halfHeight),   // 左上
                new Vector3(halfWidth, 0, halfHeight)     // 右上
            };

            // UV 坐标
            Vector2[] uv = new Vector2[]
            {
                new Vector2(0, 0),  // 左下
                new Vector2(1, 0),  // 右下
                new Vector2(0, 1),  // 左上
                new Vector2(1, 1)   // 右上
            };

            // 法线 (朝上)
            Vector3[] normals = new Vector3[]
            {
                Vector3.up,
                Vector3.up,
                Vector3.up,
                Vector3.up
            };

            // 切线
            Vector4[] tangents = new Vector4[]
            {
                new Vector4(1, 0, 0, -1),
                new Vector4(1, 0, 0, -1),
                new Vector4(1, 0, 0, -1),
                new Vector4(1, 0, 0, -1)
            };

            // 三角形索引 (两个三角形组成一个面)
            int[] triangles = new int[]
            {
                0, 2, 1,  // 第一个三角形
                2, 3, 1   // 第二个三角形
            };

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// 保存 Mesh 为资产文件
        /// </summary>
        private static void SaveMeshAsset(Mesh mesh, string path)
        {
            // 检查是否已存在，如果存在则更新
            Mesh existingMesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existingMesh != null)
            {
                EditorUtility.CopySerialized(mesh, existingMesh);
                EditorUtility.SetDirty(existingMesh);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, path);
            }
        }

        /// <summary>
        /// 生成细分的大型 Quad Mesh (用于需要更多顶点的情况)
        /// </summary>
        [MenuItem("Tools/Generate Subdivided Quad Mesh")]
        public static void GenerateSubdividedQuadMesh()
        {
            // 创建目录
            string folderPath = "Assets/Resources/Scenes/Models";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "Scenes");
                }
                AssetDatabase.CreateFolder("Assets/Resources/Scenes", "Models");
            }

            // 生成 100x100 大小、10x10 细分的 Quad Mesh
            Mesh subdivQuad = CreateSubdividedQuadMesh(100f, 100f, 10, 10, "SubdividedQuadMesh");
            SaveMeshAsset(subdivQuad, folderPath + "/SubdividedQuadMesh.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[MeshGenerator] 已生成细分 Quad Mesh:\n" +
                      "- SubdividedQuadMesh (100x100, 10x10 细分)\n" +
                      "位置: Resources/Scenes/Models/");
        }

        /// <summary>
        /// 创建细分的 Quad Mesh
        /// </summary>
        private static Mesh CreateSubdividedQuadMesh(float width, float height, int subdivisionsX, int subdivisionsY, string name)
        {
            Mesh mesh = new Mesh();
            mesh.name = name;

            int vertCountX = subdivisionsX + 1;
            int vertCountY = subdivisionsY + 1;
            int vertexCount = vertCountX * vertCountY;
            int triangleCount = subdivisionsX * subdivisionsY * 6;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector4[] tangents = new Vector4[vertexCount];
            int[] triangles = new int[triangleCount];

            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            // 生成顶点
            for (int y = 0; y < vertCountY; y++)
            {
                for (int x = 0; x < vertCountX; x++)
                {
                    int index = y * vertCountX + x;
                    float xPos = -halfWidth + (width * x / subdivisionsX);
                    float zPos = -halfHeight + (height * y / subdivisionsY);

                    vertices[index] = new Vector3(xPos, 0, zPos);
                    uv[index] = new Vector2((float)x / subdivisionsX, (float)y / subdivisionsY);
                    normals[index] = Vector3.up;
                    tangents[index] = new Vector4(1, 0, 0, -1);
                }
            }

            // 生成三角形
            int triIndex = 0;
            for (int y = 0; y < subdivisionsY; y++)
            {
                for (int x = 0; x < subdivisionsX; x++)
                {
                    int bottomLeft = y * vertCountX + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + vertCountX;
                    int topRight = topLeft + 1;

                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = bottomRight;

                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = topRight;
                    triangles[triIndex++] = bottomRight;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.triangles = triangles;

            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
