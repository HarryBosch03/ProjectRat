using UnityEditor;
using UnityEngine;

public static class CopyCollision
{
    [MenuItem("GameObject/Copy Collision")]
    public static void Execute()
    {
        if (Selection.activeGameObject == null)
            throw new System.NullReferenceException("Copy Collision Requires an active selected GameObject");
        
        
        if (Selection.gameObjects.Length < 2)
            throw new System.NullReferenceException("Copy Collision Requires a minimum of two selected GameObjects");

        var copy = Selection.activeGameObject.GetComponents<Collider>();
        
        for (var i = 0; i < Selection.gameObjects.Length; i++)
        {
            var other = Selection.gameObjects[i];
            if (other == Selection.activeGameObject) continue;
            Undo.RecordObject(other, "Copy Collision");
        }
        
        for (var i = 0; i < Selection.gameObjects.Length; i++)
        {
            var other = Selection.gameObjects[i];
            if (other == Selection.activeGameObject) continue;

            foreach (var old in other.GetComponents<Collider>())
            {
                Object.DestroyImmediate(old);
            }
            
            for (var j = 0; j < copy.Length; j++)
            {
                var from = copy[j];
                var to = other.AddComponent(from.GetType());

                EditorUtility.CopySerialized(from, to);
            }
        }
    }
}
