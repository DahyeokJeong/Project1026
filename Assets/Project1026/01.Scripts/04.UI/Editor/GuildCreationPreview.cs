using UnityEditor;
using UnityEngine;

public sealed class GuildCreationPreview : EditorWindow
{
    [MenuItem("Project1026/Guild Creation Preview")]
    private static void Open()
    {
        var window = GetWindow<GuildCreationPreview>("Guild Creation");
        window.minSize = new Vector2(800, 600);
    }

    public void CreateGUI()
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/Maplestory Light.ttf");
        rootVisualElement.Add(new GuildCreationForm(font, draft =>
            ShowNotification(new GUIContent($"창설 양식 확인 · {draft.Name}"))));
    }
}
