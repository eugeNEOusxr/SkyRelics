using UnityEngine;
using System.IO;
using System.Collections;

namespace SkyRelics.AssetBrowser
{
    /// <summary>
    /// Import and spawn downloaded Sketchfab models into the game world.
    /// Supports GLTF/GLB, OBJ, and FBX formats.
    /// </summary>
    public class ModelImporter : MonoBehaviour
    {
        [Header("Import Settings")]
        public string importFolder = "Assets/ImportedModels";
        public float defaultScale = 1f;
        public Vector3 spawnPosition = Vector3.zero;
        
        [Header("Spawn Layout")]
        public float spacing = 5f;
        public int modelsPerRow = 5;
        
        [Header("Auto-Import on Start")]
        public bool autoImportOnStart = true;
        public string[] modelPaths = new string[]
        {
            "model1.gltf",
            "model2.gltf"
        };

        private int spawnedCount = 0;

        void Start()
        {
            if (autoImportOnStart)
            {
                ImportAllModels();
            }
        }

        /// <summary>
        /// Import all models in the specified folder.
        /// </summary>
        [ContextMenu("Import All Models")]
        public void ImportAllModels()
        {
            if (!Directory.Exists(importFolder))
            {
                Debug.LogWarning($"Import folder doesn't exist: {importFolder}");
                return;
            }

            // Find all supported model files
            string[] extensions = new string[] { "*.gltf", "*.glb", "*.obj", "*.fbx" };
            
            foreach (string ext in extensions)
            {
                string[] files = Directory.GetFiles(importFolder, ext, SearchOption.AllDirectories);
                
                foreach (string file in files)
                {
                    ImportModel(file);
                }
            }

            Debug.Log($"✓ Imported {spawnedCount} models");
        }

        /// <summary>
        /// Import a specific model file path.
        /// </summary>
        public void ImportModel(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"❌ File not found: {filePath}");
                return;
            }

            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".gltf":
                case ".glb":
                    ImportGLTF(filePath);
                    break;
                case ".obj":
                    ImportOBJ(filePath);
                    break;
                case ".fbx":
                    ImportFBX(filePath);
                    break;
                default:
                    Debug.LogWarning($"Unsupported format: {extension}");
                    break;
            }
        }

        /// <summary>
        /// Import GLTF/GLB model (requires GLTFUtility or similar package).
        /// </summary>
        void ImportGLTF(string filePath)
        {
            Debug.Log($"📦 Importing GLTF: {filePath}");

            // Using GLTFUtility package (install from: https://github.com/Siccity/GLTFUtility)
            // Uncomment this if you have GLTFUtility installed:
            /*
            Importer.ImportGLTFAsync(filePath, new ImportSettings(), (result, animations) => {
                result.transform.position = GetNextSpawnPosition();
                result.transform.localScale = Vector3.one * defaultScale;
                result.name = Path.GetFileNameWithoutExtension(filePath);
                spawnedCount++;
                Debug.Log($"✓ Spawned: {result.name}");
            });
            */

            // Temporary: Create placeholder for now
            CreatePlaceholder(filePath);
        }

        /// <summary>
        /// Import OBJ model using Unity's built-in runtime OBJ loader.
        /// </summary>
        void ImportOBJ(string filePath)
        {
            Debug.Log($"📦 Importing OBJ: {filePath}");

            // OBJ parsing (basic implementation)
            Mesh mesh = LoadOBJMesh(filePath);
            
            if (mesh != null)
            {
                GameObject obj = new GameObject(Path.GetFileNameWithoutExtension(filePath));
                obj.AddComponent<MeshFilter>().mesh = mesh;
                obj.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
                obj.transform.position = GetNextSpawnPosition();
                obj.transform.localScale = Vector3.one * defaultScale;
                
                spawnedCount++;
                Debug.Log($"✓ Spawned OBJ: {obj.name} ({mesh.vertexCount} vertices)");
            }
        }

        /// <summary>
        /// Import FBX model (Unity native format).
        /// </summary>
        void ImportFBX(string filePath)
        {
            Debug.Log($"📦 Importing FBX: {filePath}");
            
            // FBX files should be placed in Assets folder to use Unity's importer
            // At runtime, we can only load from Resources or AssetBundles
            CreatePlaceholder(filePath);
        }

        /// <summary>
        /// Basic OBJ file parser (vertices and faces only).
        /// </summary>
        Mesh LoadOBJMesh(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                
                System.Collections.Generic.List<Vector3> vertices = new System.Collections.Generic.List<Vector3>();
                System.Collections.Generic.List<Vector2> uvs = new System.Collections.Generic.List<Vector2>();
                System.Collections.Generic.List<Vector3> normals = new System.Collections.Generic.List<Vector3>();
                System.Collections.Generic.List<int> triangles = new System.Collections.Generic.List<int>();

                foreach (string line in lines)
                {
                    string[] parts = line.Split(' ');
                    
                    if (parts.Length == 0) continue;

                    switch (parts[0])
                    {
                        case "v": // Vertex
                            if (parts.Length >= 4)
                            {
                                vertices.Add(new Vector3(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2]),
                                    float.Parse(parts[3])
                                ));
                            }
                            break;

                        case "vt": // UV
                            if (parts.Length >= 3)
                            {
                                uvs.Add(new Vector2(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2])
                                ));
                            }
                            break;

                        case "vn": // Normal
                            if (parts.Length >= 4)
                            {
                                normals.Add(new Vector3(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2]),
                                    float.Parse(parts[3])
                                ));
                            }
                            break;

                        case "f": // Face (triangle)
                            if (parts.Length >= 4)
                            {
                                // Simple triangle parsing (v/vt/vn format)
                                for (int i = 1; i <= 3; i++)
                                {
                                    string[] indices = parts[i].Split('/');
                                    int vertexIndex = int.Parse(indices[0]) - 1; // OBJ is 1-indexed
                                    triangles.Add(vertexIndex);
                                }
                            }
                            break;
                    }
                }

                Mesh mesh = new Mesh();
                mesh.name = Path.GetFileNameWithoutExtension(filePath);
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                
                if (uvs.Count > 0)
                    mesh.uv = uvs.ToArray();
                
                if (normals.Count > 0)
                    mesh.normals = normals.ToArray();
                else
                    mesh.RecalculateNormals();

                mesh.RecalculateBounds();
                
                return mesh;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Failed to load OBJ: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a placeholder cube for unsupported formats.
        /// </summary>
        void CreatePlaceholder(string filePath)
        {
            GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = Path.GetFileNameWithoutExtension(filePath) + " (Placeholder)";
            placeholder.transform.position = GetNextSpawnPosition();
            placeholder.transform.localScale = Vector3.one * 2f;
            
            // Add label
            TextMesh label = new GameObject("Label").AddComponent<TextMesh>();
            label.text = placeholder.name;
            label.transform.SetParent(placeholder.transform);
            label.transform.localPosition = Vector3.up * 1.5f;
            label.fontSize = 20;
            label.color = Color.white;
            label.anchor = TextAnchor.MiddleCenter;
            
            spawnedCount++;
            Debug.Log($"⚠️ Created placeholder for: {Path.GetFileName(filePath)}");
        }

        /// <summary>
        /// Calculate next spawn position in grid layout.
        /// </summary>
        Vector3 GetNextSpawnPosition()
        {
            int row = spawnedCount / modelsPerRow;
            int col = spawnedCount % modelsPerRow;
            
            return spawnPosition + new Vector3(
                col * spacing,
                0,
                row * spacing
            );
        }

        /// <summary>
        /// Import from Downloads folder (common location for Sketchfab downloads).
        /// </summary>
        [ContextMenu("Import From Downloads Folder")]
        public void ImportFromDownloads()
        {
            string downloadsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile) + "/Downloads";
            
            if (Directory.Exists(downloadsPath))
            {
                importFolder = downloadsPath;
                ImportAllModels();
            }
            else
            {
                Debug.LogError($"❌ Downloads folder not found: {downloadsPath}");
            }
        }
    }
}
