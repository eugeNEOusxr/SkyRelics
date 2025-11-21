using UnityEngine;
using System.Collections.Generic;

namespace SkyRelics.Procedural
{
    /// <summary>
    /// Procedural female character mesh generator.
    /// Creates a low-poly humanoid body with proper proportions.
    /// Vertex count: ~500-800 vertices for game-ready character.
    /// </summary>
    public class ProceduralCharacterBuilder : MonoBehaviour
    {
        [Header("Body Proportions")]
        [Range(1.4f, 1.9f)]
        public float height = 1.65f; // Female average height
        
        [Range(0.8f, 1.2f)]
        public float bodyWidth = 1.0f;
        
        [Range(0.8f, 1.2f)]
        public float shoulderWidth = 0.95f;
        
        [Range(0.8f, 1.2f)]
        public float hipWidth = 1.05f;

        [Header("Materials")]
        public Material skinMaterial;
        public Color skinColor = new Color(0.95f, 0.76f, 0.65f, 1f); // Peachy skin tone

        [Header("Detail Level")]
        [Range(4, 16)]
        public int cylinderSegments = 8; // Higher = smoother limbs

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        void Start()
        {
            GenerateCharacter();
        }

        [ContextMenu("Generate Character")]
        public void GenerateCharacter()
        {
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Mesh characterMesh = BuildFemaleBody();
            
            if (characterMesh != null)
            {
                meshFilter.mesh = characterMesh;
                
                if (skinMaterial == null)
                {
                    skinMaterial = new Material(Shader.Find("Standard"));
                    skinMaterial.color = skinColor;
                    skinMaterial.SetFloat("_Smoothness", 0.4f);
                }
                meshRenderer.material = skinMaterial;
                
                Debug.Log($"✓ Female character generated with {characterMesh.vertexCount} vertices");
            }
        }

        /// <summary>
        /// Build complete female body mesh: head, torso, arms, legs.
        /// Total vertices: ~650-800 depending on segment count.
        /// </summary>
        Mesh BuildFemaleBody()
        {
            List<CombineInstance> bodyParts = new List<CombineInstance>();

            // Scale factor based on height
            float scale = height / 1.65f;

            // HEAD (sphere) - ~66 vertices at 8 segments
            Mesh head = ProceduralMeshGenerator.CreateSphere(
                0.12f * scale,
                cylinderSegments,
                new Vector3(0, 1.52f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(head, skinColor);
            bodyParts.Add(new CombineInstance { mesh = head, transform = Matrix4x4.identity });

            // NECK - ~18 vertices
            Mesh neck = ProceduralMeshGenerator.CreateCylinder(
                0.06f * scale,
                0.12f * scale,
                cylinderSegments,
                new Vector3(0, 1.4f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(neck, skinColor);
            bodyParts.Add(new CombineInstance { mesh = neck, transform = Matrix4x4.identity });

            // UPPER TORSO (chest) - 24 vertices (box shaped)
            Mesh upperTorso = ProceduralMeshGenerator.CreateBox(
                new Vector3(0.18f * bodyWidth * scale, 0.22f * scale, 0.12f * scale),
                new Vector3(0, 1.15f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(upperTorso, skinColor);
            bodyParts.Add(new CombineInstance { mesh = upperTorso, transform = Matrix4x4.identity });

            // MID TORSO (waist - narrower) - 24 vertices
            Mesh midTorso = ProceduralMeshGenerator.CreateBox(
                new Vector3(0.14f * bodyWidth * scale, 0.18f * scale, 0.11f * scale),
                new Vector3(0, 0.88f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(midTorso, skinColor);
            bodyParts.Add(new CombineInstance { mesh = midTorso, transform = Matrix4x4.identity });

            // LOWER TORSO (hips - wider) - 24 vertices
            Mesh lowerTorso = ProceduralMeshGenerator.CreateBox(
                new Vector3(0.17f * hipWidth * scale, 0.16f * scale, 0.14f * scale),
                new Vector3(0, 0.68f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(lowerTorso, skinColor);
            bodyParts.Add(new CombineInstance { mesh = lowerTorso, transform = Matrix4x4.identity });

            // SHOULDERS (Left & Right) - 24 vertices each
            float shoulderOffset = 0.11f * shoulderWidth * scale;
            
            Mesh leftShoulder = ProceduralMeshGenerator.CreateSphere(
                0.055f * scale,
                6,
                new Vector3(-shoulderOffset, 1.28f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftShoulder, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftShoulder, transform = Matrix4x4.identity });

            Mesh rightShoulder = ProceduralMeshGenerator.CreateSphere(
                0.055f * scale,
                6,
                new Vector3(shoulderOffset, 1.28f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightShoulder, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightShoulder, transform = Matrix4x4.identity });

            // UPPER ARMS (Left & Right) - ~18 vertices each
            Mesh leftUpperArm = ProceduralMeshGenerator.CreateCylinder(
                0.04f * scale,
                0.28f * scale,
                cylinderSegments,
                new Vector3(-shoulderOffset, 1.0f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftUpperArm, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftUpperArm, transform = Matrix4x4.identity });

            Mesh rightUpperArm = ProceduralMeshGenerator.CreateCylinder(
                0.04f * scale,
                0.28f * scale,
                cylinderSegments,
                new Vector3(shoulderOffset, 1.0f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightUpperArm, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightUpperArm, transform = Matrix4x4.identity });

            // ELBOWS (Left & Right) - 24 vertices each
            Mesh leftElbow = ProceduralMeshGenerator.CreateSphere(
                0.038f * scale,
                5,
                new Vector3(-shoulderOffset, 0.86f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftElbow, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftElbow, transform = Matrix4x4.identity });

            Mesh rightElbow = ProceduralMeshGenerator.CreateSphere(
                0.038f * scale,
                5,
                new Vector3(shoulderOffset, 0.86f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightElbow, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightElbow, transform = Matrix4x4.identity });

            // FOREARMS (Left & Right) - ~18 vertices each
            Mesh leftForearm = ProceduralMeshGenerator.CreateCylinder(
                0.035f * scale,
                0.24f * scale,
                cylinderSegments,
                new Vector3(-shoulderOffset, 0.62f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftForearm, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftForearm, transform = Matrix4x4.identity });

            Mesh rightForearm = ProceduralMeshGenerator.CreateCylinder(
                0.035f * scale,
                0.24f * scale,
                cylinderSegments,
                new Vector3(shoulderOffset, 0.62f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightForearm, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightForearm, transform = Matrix4x4.identity });

            // HANDS (Left & Right) - 24 vertices each (simplified box hands)
            Mesh leftHand = ProceduralMeshGenerator.CreateBox(
                new Vector3(0.035f * scale, 0.08f * scale, 0.02f * scale),
                new Vector3(-shoulderOffset, 0.46f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftHand, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftHand, transform = Matrix4x4.identity });

            Mesh rightHand = ProceduralMeshGenerator.CreateBox(
                new Vector3(0.035f * scale, 0.08f * scale, 0.02f * scale),
                new Vector3(shoulderOffset, 0.46f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightHand, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightHand, transform = Matrix4x4.identity });

            // HIPS (Left & Right connection) - 24 vertices each
            float hipOffset = 0.08f * hipWidth * scale;

            Mesh leftHip = ProceduralMeshGenerator.CreateSphere(
                0.06f * scale,
                6,
                new Vector3(-hipOffset, 0.6f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftHip, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftHip, transform = Matrix4x4.identity });

            Mesh rightHip = ProceduralMeshGenerator.CreateSphere(
                0.06f * scale,
                6,
                new Vector3(hipOffset, 0.6f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightHip, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightHip, transform = Matrix4x4.identity });

            // UPPER LEGS (thighs) - ~18 vertices each
            Mesh leftThigh = ProceduralMeshGenerator.CreateCylinder(
                0.055f * scale,
                0.38f * scale,
                cylinderSegments,
                new Vector3(-hipOffset, 0.34f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftThigh, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftThigh, transform = Matrix4x4.identity });

            Mesh rightThigh = ProceduralMeshGenerator.CreateCylinder(
                0.055f * scale,
                0.38f * scale,
                cylinderSegments,
                new Vector3(hipOffset, 0.34f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightThigh, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightThigh, transform = Matrix4x4.identity });

            // KNEES (Left & Right) - 24 vertices each
            Mesh leftKnee = ProceduralMeshGenerator.CreateSphere(
                0.045f * scale,
                5,
                new Vector3(-hipOffset, 0.15f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftKnee, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftKnee, transform = Matrix4x4.identity });

            Mesh rightKnee = ProceduralMeshGenerator.CreateSphere(
                0.045f * scale,
                5,
                new Vector3(hipOffset, 0.15f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightKnee, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightKnee, transform = Matrix4x4.identity });

            // LOWER LEGS (calves) - ~18 vertices each
            Mesh leftCalf = ProceduralMeshGenerator.CreateCylinder(
                0.042f * scale,
                0.36f * scale,
                cylinderSegments,
                new Vector3(-hipOffset, -0.08f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftCalf, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftCalf, transform = Matrix4x4.identity });

            Mesh rightCalf = ProceduralMeshGenerator.CreateCylinder(
                0.042f * scale,
                0.36f * scale,
                cylinderSegments,
                new Vector3(hipOffset, -0.08f * scale, 0)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightCalf, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightCalf, transform = Matrix4x4.identity });

            // FEET (Left & Right) - 24 vertices each (elongated boxes)
            Mesh leftFoot = ProceduralMeshGenerator.CreateBox(
                new Vector3(0.04f * scale, 0.035f * scale, 0.1f * scale),
                new Vector3(-hipOffset, -0.28f * scale, 0.03f * scale)
            );
            ProceduralMeshGenerator.ApplyVertexColors(leftFoot, skinColor);
            bodyParts.Add(new CombineInstance { mesh = leftFoot, transform = Matrix4x4.identity });

            Mesh rightFoot = ProceduralMeshGenerator.CreateBox(
                new Vector3(0.04f * scale, 0.035f * scale, 0.1f * scale),
                new Vector3(hipOffset, -0.28f * scale, 0.03f * scale)
            );
            ProceduralMeshGenerator.ApplyVertexColors(rightFoot, skinColor);
            bodyParts.Add(new CombineInstance { mesh = rightFoot, transform = Matrix4x4.identity });

            // COMBINE ALL PARTS
            Mesh finalMesh = ProceduralMeshGenerator.CombineMeshes(bodyParts);
            finalMesh.name = $"ProceduralFemaleBody_{finalMesh.vertexCount}v";
            
            return finalMesh;
        }
    }
}
