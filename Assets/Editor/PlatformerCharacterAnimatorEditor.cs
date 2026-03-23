using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlatformerCharacterAnimator))]
public class PlatformerCharacterAnimatorEditor : Editor
{
    public override void OnInspectorGUI() 
    {
        DrawDefaultInspector();

        PlatformerCharacterAnimator animator = (PlatformerCharacterAnimator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Setup Animator"))
        {
            SetupAnimator(animator);
        }
    }

    private void SetupAnimator(PlatformerCharacterAnimator animator)
    {
        SpriteRenderer sr = animator.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("No SpriteRenderer found!");
            return;
        }

        // Force reload animations
        var method = typeof(PlatformerCharacterAnimator).GetMethod("LoadAllStates", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(animator, null);

        // Get Idle state (index 0)
        var statesField = typeof(PlatformerCharacterAnimator).GetField("states", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Sprite[][] states = (Sprite[][])statesField.GetValue(animator);

        if (states != null && states.Length > 0 && states[0] != null && states[0].Length > 0)
        {
            sr.sprite = states[0][0];

            // Resize collider
            ResizeCollider(animator, states[0][0]);
        }

        EditorUtility.SetDirty(animator);
    }

    private void ResizeCollider(PlatformerCharacterAnimator animator, Sprite sprite)
    {
        BoxCollider2D box = animator.GetComponent<BoxCollider2D>();

        if (box == null)
        {
            box = animator.gameObject.AddComponent<BoxCollider2D>();
        }

        box.size = sprite.bounds.size;
        box.offset = sprite.bounds.center;
    }
}