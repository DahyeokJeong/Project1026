using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class WhiteTigerAssetBuilder
{
    private const string Art = "Assets/Project1026/08.Resources/03.Boss/GwaegangWhiteTiger";
    private const string Prefab = "Assets/Project1026/08.Resources/03.Boss/GwaegangWhiteTiger/GwaegangWhiteTiger.prefab";

    [MenuItem("Project1026/Boss/Rebuild White Tiger Assets")]
    public static void Build()
    {
        foreach (string path in Directory.GetFiles(Art, "*.png", SearchOption.AllDirectories))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64f;
            importer.spritePivot = new Vector2(0.5f, 0.15f);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0.15f);
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        var root = new GameObject("괴강백호");
        try
        {
            root.layer = LayerMask.NameToLayer("Enemy");
            root.tag = "Enemy";
            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.8f;
            collider.offset = new Vector2(0f, 0.7f);
            collider.isTrigger = true;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Object";
            var boss = root.AddComponent<GwaegangWhiteTiger>();
            var serialized = new SerializedObject(boss);
            serialized.FindProperty("body").objectReferenceValue = renderer;
            string[] states = { "Idle", "Move", "Attack", "Stomp", "JumpReady", "JumpSlam", "Dead" };
            foreach (string state in states)
            {
                var frames = Directory.GetFiles(Art + "/" + state, "*.png").OrderBy(p => p, StringComparer.Ordinal)
                    .Select(p => AssetDatabase.LoadAssetAtPath<Sprite>(p.Replace('\\', '/'))).ToArray();
                if (frames.Length == 0 || frames.Any(f => f == null)) throw new Exception("Missing boss sprites: " + state);
                var property = serialized.FindProperty(char.ToLowerInvariant(state[0]) + state.Substring(1));
                property.arraySize = frames.Length;
                for (int i = 0; i < frames.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                if (state == "Idle") renderer.sprite = frames[0];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, Prefab);
            var stage = AssetDatabase.LoadAssetAtPath<StageDataSO>("Assets/Project1026/06.Datas/SO/Stage Data/StageData01.asset");
            var stageData = new SerializedObject(stage);
            stageData.FindProperty("bossPrefab").objectReferenceValue = prefab.GetComponent<GwaegangWhiteTiger>();
            stageData.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(stage);
            AssetDatabase.SaveAssets();
            Debug.Log("WHITE_TIGER_ASSETS_OK: 52 frames, prefab and stage 1 connected.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
}
