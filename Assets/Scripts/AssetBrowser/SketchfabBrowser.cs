using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SkyRelics.AssetBrowser
{
    /// <summary>
    /// Sketchfab API integration for browsing and importing free 3D models.
    /// Displays models in a 3D grid gallery with categories.
    /// </summary>
    public class SketchfabBrowser : MonoBehaviour
    {
        [Header("API Settings")]
        public string sketchfabApiKey = ""; // Get free key at sketchfab.com/developers
        private const string API_BASE = "https://api.sketchfab.com/v3";
        
        [Header("Search Settings")]
        public string searchQuery = "weapon";
        public string category = ""; // weapons, characters, vehicles, etc.
        public bool freeDownloadOnly = true;
        public int maxResults = 50;

        [Header("3D Gallery Layout")]
        public float gridSpacing = 5f;
        public int itemsPerRow = 5;
        public GameObject previewPrefab; // Cube with preview image
        
        [Header("Categories")]
        public string[] categoryTags = new string[] 
        { 
            "weapons", "characters", "vehicles", "buildings", 
            "props", "nature", "sci-fi", "fantasy" 
        };

        private List<SketchfabModel> loadedModels = new List<SketchfabModel>();
        private Dictionary<string, List<GameObject>> categoryGroups = new Dictionary<string, List<GameObject>>();

        [System.Serializable]
        public class SketchfabModel
        {
            public string uid;
            public string name;
            public string description;
            public int vertexCount;
            public int faceCount;
            public string downloadUrl;
            public string thumbnailUrl;
            public string[] tags;
            public bool isDownloadable;
            public string license;
        }

        [System.Serializable]
        public class SketchfabSearchResponse
        {
            public List<SketchfabModelData> results;
            public int count;
        }

        [System.Serializable]
        public class SketchfabModelData
        {
            public string uid;
            public string name;
            public string description;
            public ThumbnailsData thumbnails;
            public DownloadData download;
            public LicenseData license;
            public List<string> tags;
            public int vertexCount;
            public int faceCount;
        }

        [System.Serializable]
        public class ThumbnailsData
        {
            public List<ImageData> images;
        }

        [System.Serializable]
        public class ImageData
        {
            public string url;
            public int width;
            public int height;
        }

        [System.Serializable]
        public class DownloadData
        {
            public string gltf;
        }

        [System.Serializable]
        public class LicenseData
        {
            public string label;
        }

        void Start()
        {
            if (string.IsNullOrEmpty(sketchfabApiKey))
            {
                Debug.LogWarning("⚠️ Sketchfab API key not set! Get one at: https://sketchfab.com/settings/password");
            }
        }

        /// <summary>
        /// Search Sketchfab for models by category.
        /// </summary>
        [ContextMenu("Search Models")]
        public void SearchModels()
        {
            StartCoroutine(SearchSketchfabModels(searchQuery, category));
        }

        /// <summary>
        /// Create categorized 3D gallery.
        /// </summary>
        [ContextMenu("Create 3D Gallery")]
        public void CreateCategorizedGallery()
        {
            StartCoroutine(BuildCategorizedGallery());
        }

        IEnumerator SearchSketchfabModels(string query, string cat)
        {
            string url = $"{API_BASE}/models?";
            
            if (!string.IsNullOrEmpty(query))
                url += $"q={UnityWebRequest.EscapeURL(query)}&";
            
            if (!string.IsNullOrEmpty(cat))
                url += $"categories={UnityWebRequest.EscapeURL(cat)}&";
            
            if (freeDownloadOnly)
                url += "downloadable=true&";
            
            url += $"count={maxResults}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                if (!string.IsNullOrEmpty(sketchfabApiKey))
                {
                    request.SetRequestHeader("Authorization", $"Token {sketchfabApiKey}");
                }

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    SketchfabSearchResponse response = JsonConvert.DeserializeObject<SketchfabSearchResponse>(json);

                    Debug.Log($"✓ Found {response.results.Count} models");

                    foreach (var modelData in response.results)
                    {
                        SketchfabModel model = new SketchfabModel
                        {
                            uid = modelData.uid,
                            name = modelData.name,
                            description = modelData.description,
                            vertexCount = modelData.vertexCount,
                            faceCount = modelData.faceCount,
                            thumbnailUrl = modelData.thumbnails?.images[0]?.url,
                            tags = modelData.tags?.ToArray(),
                            isDownloadable = modelData.download?.gltf != null,
                            license = modelData.license?.label
                        };

                        loadedModels.Add(model);
                    }

                    DisplayModelsInGallery();
                }
                else
                {
                    Debug.LogError($"❌ Sketchfab API error: {request.error}");
                }
            }
        }

        /// <summary>
        /// Display models in a 3D grid layout.
        /// </summary>
        void DisplayModelsInGallery()
        {
            for (int i = 0; i < loadedModels.Count; i++)
            {
                SketchfabModel model = loadedModels[i];

                int row = i / itemsPerRow;
                int col = i % itemsPerRow;

                Vector3 position = new Vector3(
                    col * gridSpacing,
                    0,
                    row * gridSpacing
                );

                // Create preview cube
                GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
                preview.transform.position = position;
                preview.transform.localScale = Vector3.one * 2f;
                preview.name = model.name;

                // Add model info component
                ModelPreview previewComponent = preview.AddComponent<ModelPreview>();
                previewComponent.modelData = model;
                previewComponent.browser = this;

                // Load thumbnail texture
                if (!string.IsNullOrEmpty(model.thumbnailUrl))
                {
                    StartCoroutine(LoadThumbnail(model.thumbnailUrl, preview));
                }

                // Add text label
                CreateLabel(model.name, position + Vector3.up * 1.5f);
            }

            Debug.Log($"✓ Displayed {loadedModels.Count} models in gallery");
        }

        /// <summary>
        /// Build categorized gallery with zones.
        /// </summary>
        IEnumerator BuildCategorizedGallery()
        {
            foreach (string category in categoryTags)
            {
                yield return SearchSketchfabModels(category, "");
                
                // Wait between requests to avoid rate limiting
                yield return new WaitForSeconds(1f);
            }

            OrganizeByCategories();
        }

        void OrganizeByCategories()
        {
            categoryGroups.Clear();

            // Group models by tags
            foreach (var model in loadedModels)
            {
                string primaryCategory = "uncategorized";
                
                if (model.tags != null && model.tags.Length > 0)
                {
                    foreach (string tag in model.tags)
                    {
                        if (System.Array.Exists(categoryTags, cat => cat.ToLower() == tag.ToLower()))
                        {
                            primaryCategory = tag.ToLower();
                            break;
                        }
                    }
                }

                if (!categoryGroups.ContainsKey(primaryCategory))
                {
                    categoryGroups[primaryCategory] = new List<GameObject>();
                }
            }

            // Layout categories in different zones
            int zoneIndex = 0;
            foreach (var category in categoryGroups)
            {
                Vector3 zoneOffset = new Vector3(zoneIndex * gridSpacing * (itemsPerRow + 2), 0, 0);
                CreateCategoryZone(category.Key, zoneOffset);
                zoneIndex++;
            }
        }

        void CreateCategoryZone(string categoryName, Vector3 offset)
        {
            // Create zone label
            CreateLabel($"=== {categoryName.ToUpper()} ===", offset + Vector3.up * 3f);

            // Create ground plane for zone
            GameObject zonePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            zonePlane.transform.position = offset;
            zonePlane.transform.localScale = new Vector3(gridSpacing * itemsPerRow / 10f, 1, gridSpacing * 3f);
            zonePlane.name = $"Zone_{categoryName}";

            // Random color per category
            Color zoneColor = Random.ColorHSV(0f, 1f, 0.5f, 0.5f, 0.8f, 0.8f);
            zonePlane.GetComponent<Renderer>().material.color = zoneColor;
        }

        IEnumerator LoadThumbnail(string url, GameObject preview)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    preview.GetComponent<Renderer>().material.mainTexture = texture;
                }
            }
        }

        void CreateLabel(string text, Vector3 position)
        {
            GameObject label = new GameObject($"Label_{text}");
            label.transform.position = position;
            
            // In a real implementation, use TextMeshPro
            // For now, just create a marker
            Debug.Log($"📝 Label: {text} at {position}");
        }

        /// <summary>
        /// Download and import a model (requires GLTF importer).
        /// </summary>
        public IEnumerator DownloadModel(SketchfabModel model)
        {
            if (!model.isDownloadable)
            {
                Debug.LogWarning($"❌ Model '{model.name}' is not downloadable");
                yield break;
            }

            string downloadUrl = $"{API_BASE}/models/{model.uid}/download";

            using (UnityWebRequest request = UnityWebRequest.Get(downloadUrl))
            {
                request.SetRequestHeader("Authorization", $"Token {sketchfabApiKey}");
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✓ Downloaded model: {model.name}");
                    Debug.Log($"Save GLTF data and import using GLTFUtility or similar package");
                    
                    // TODO: Save file and import with GLTF importer
                    // This requires additional packages like GLTFUtility or UnityGLTF
                }
                else
                {
                    Debug.LogError($"❌ Download failed: {request.error}");
                }
            }
        }
    }

    /// <summary>
    /// Component attached to each preview cube for interaction.
    /// </summary>
    public class ModelPreview : MonoBehaviour
    {
        public SketchfabBrowser.SketchfabModel modelData;
        public SketchfabBrowser browser;

        void OnMouseDown()
        {
            Debug.Log($"🖱️ Clicked: {modelData.name}");
            Debug.Log($"   Vertices: {modelData.vertexCount:N0}");
            Debug.Log($"   License: {modelData.license}");
            Debug.Log($"   Tags: {string.Join(", ", modelData.tags ?? new string[0])}");
            
            // Start download
            if (Input.GetKey(KeyCode.LeftShift))
            {
                browser.StartCoroutine(browser.DownloadModel(modelData));
            }
        }

        void OnMouseEnter()
        {
            transform.localScale = Vector3.one * 2.2f; // Highlight
        }

        void OnMouseExit()
        {
            transform.localScale = Vector3.one * 2f;
        }
    }
}
