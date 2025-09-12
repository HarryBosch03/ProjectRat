using System;
using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;

public class NoiseGenerator : EditorWindow
{
    public int width = 1024;
    public int height = 1024;

    [MenuItem("Tools/Noise Generator")]
    public static void Open()
    {
        CreateWindow<NoiseGenerator>("Noise Generator");
    }

    private void OnGUI()
    {
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);

        if (width < 2) width = 2;
        if (height < 2) height = 2;

        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        var rng = new System.Random();
        
        var texture = new Texture2D(width, height);
        for (var y = 0; y < texture.height; y++)
        for (var x = 0; x < texture.width; x++)
        {
            texture.SetPixel(x, y, new Color((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble(), 1f));
        }
        
        var png = texture.EncodeToPNG();
        var saveLocation = EditorUtility.SaveFilePanel("Save Noise File", Application.dataPath, "noise", "png");

        File.WriteAllBytes(saveLocation, png);
        AssetDatabase.Refresh();

        DestroyImmediate(texture);
    }
}
