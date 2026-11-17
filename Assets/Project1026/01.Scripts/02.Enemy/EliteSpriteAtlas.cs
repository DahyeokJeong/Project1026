using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EliteSpriteAtlas : ScriptableObject
{
    [Serializable]
    private struct Frame
    {
        public Sprite source;
        public Rect rect;
        public Vector2 sourceOffset;
    }

    [SerializeField] private Texture2D texture;
    [SerializeField] private Frame[] frames;

    private Dictionary<Sprite, Frame> frameLookup;
    private readonly Dictionary<Sprite, Sprite> outlinedSprites = new();

    public Sprite GetOutlined(Sprite source)
    {
        if (source == null)
            return null;

        if (outlinedSprites.TryGetValue(source, out Sprite outlined))
            return outlined;

        if (frameLookup == null)
        {
            frameLookup = new Dictionary<Sprite, Frame>(frames.Length);
            foreach (Frame entry in frames)
                frameLookup.Add(entry.source, entry);
        }

        if (!frameLookup.TryGetValue(source, out Frame frame))
            return source;

        Vector2 pivot = source.pivot - frame.sourceOffset;
        pivot = new Vector2(pivot.x / frame.rect.width, pivot.y / frame.rect.height);
        outlined = Sprite.Create(texture, frame.rect, pivot, source.pixelsPerUnit,
            0, SpriteMeshType.FullRect, Vector4.zero, false);
        outlined.name = source.name + " (Elite)";
        outlinedSprites.Add(source, outlined);
        return outlined;
    }

    private void OnDisable()
    {
        foreach (Sprite sprite in outlinedSprites.Values)
        {
            if (Application.isPlaying)
                Destroy(sprite);
            else
                DestroyImmediate(sprite);
        }

        outlinedSprites.Clear();
    }
}
