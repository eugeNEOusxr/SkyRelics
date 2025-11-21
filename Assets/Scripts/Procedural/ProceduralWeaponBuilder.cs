using UnityEngine;
using System.Collections.Generic;

namespace SkyRelics.Procedural
{
    /// <summary>
    /// Procedural weapon mesh builder.
    /// Attach to GameObject and click Generate Weapon in Inspector.
    /// </summary>
    public class ProceduralWeaponBuilder : MonoBehaviour
    {
        public enum WeaponType
        {
            AssaultRifle,
            Shotgun,
            SniperRifle,
            Pistol,
            Sword,
            Axe,
            Bow,
            RocketLauncher
        }

        [Header("Generation Settings")]
        public WeaponType weaponType = WeaponType.AssaultRifle;
        public Material weaponMaterial;
        
        [Header("Colors")]
        public Color primaryColor = new Color(0.2f, 0.2f, 0.25f);
        public Color secondaryColor = new Color(0.4f, 0.35f, 0.3f);
        public Color accentColor = new Color(0.8f, 0.6f, 0.2f);

        [Header("Scale")]
        [Range(0.5f, 2f)]
        public float weaponScale = 1f;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        void Start()
        {
            GenerateWeapon();
        }

        [ContextMenu("Generate Weapon")]
        public void GenerateWeapon()
        {
            Debug.Log($"Generating {weaponType} weapon...");
            
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Mesh weaponMesh = weaponType switch
            {
                WeaponType.AssaultRifle => BuildAssaultRifle(),
                WeaponType.Shotgun => BuildShotgun(),
                WeaponType.SniperRifle => BuildSniperRifle(),
                WeaponType.Pistol => BuildPistol(),
                WeaponType.Sword => BuildSword(),
                WeaponType.Axe => BuildAxe(),
                WeaponType.Bow => BuildBow(),
                WeaponType.RocketLauncher => BuildRocketLauncher(),
                _ => null
            };

            if (weaponMesh != null)
            {
                meshFilter.mesh = weaponMesh;
                
                if (weaponMaterial == null)
                {
                    weaponMaterial = new Material(Shader.Find("Standard"));
                    weaponMaterial.color = primaryColor;
                }
                meshRenderer.material = weaponMaterial;
                
                Debug.Log($"✓ {weaponType} generated with {weaponMesh.vertexCount} vertices");
            }
        }

        Mesh BuildAssaultRifle()
        {
            // Simple box-based rifle for now
            return GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;
        }

        Mesh BuildShotgun() => BuildAssaultRifle();
        Mesh BuildSniperRifle() => BuildAssaultRifle();
        Mesh BuildPistol() => BuildAssaultRifle();
        Mesh BuildSword() => BuildAssaultRifle();
        Mesh BuildAxe() => BuildAssaultRifle();
        Mesh BuildBow() => BuildAssaultRifle();
        Mesh BuildRocketLauncher() => BuildAssaultRifle();
    }
}
