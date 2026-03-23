using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;

[CustomEditor(typeof(PlatformerPlayerController))]
public class PlatformerPlayerControllerEditor : Editor
{
    private const float defaultUndergroundOffset = -0.05f;

    private void OnEnable()
    {
        PlatformerPlayerController controller = (PlatformerPlayerController)target;
        if (controller.transform.Find("groundDetector") != null) return;
        GameObject detector = new GameObject("groundDetector");
        Undo.RegisterCreatedObjectUndo(detector, "Create groundDetector");
        detector.transform.SetParent(controller.transform, false);
        detector.transform.localPosition = GetBottomOffset(controller) + new Vector3(0, defaultUndergroundOffset, 0);
        EditorGUIUtility.PingObject(detector);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlatformerPlayerController controller = (PlatformerPlayerController)target;

        GUILayout.Space(8);
        bool cameraExists = FindBoundCamera(controller) != null;
        using (new EditorGUI.DisabledScope(cameraExists))
        {
            if (GUILayout.Button(cameraExists ? "Cinemachine Camera Already Bound" : "Setup Cinemachine Camera"))
                SpawnCinemachineCamera(controller);
        }
    }
    private void SpawnCinemachineCamera(PlatformerPlayerController controller)
    {
        GameObject go = new GameObject("CM_PlayerCamera");
        Undo.RegisterCreatedObjectUndo(go, "Create Cinemachine Camera");

        // Start behind the scene
        go.transform.position = new Vector3(0f, 0f, -10f);

        CinemachineCamera vcam = go.AddComponent<CinemachineCamera>();

        // Cinemachine 3 uses TrackingTarget
        vcam.Target.TrackingTarget = controller.transform;

        // PositionComposer is correct for 2D orthographic — no rotation needed
        var composer = go.AddComponent<CinemachinePositionComposer>();
        composer.CameraDistance = 4f;

        // Zero dead zone so no yellow square is drawn
        composer.Composition.DeadZone.Size = Vector2.zero;
        composer.Composition.DeadZone.Enabled = false;

        // Slight vertical offset — player sees more above than below
        composer.Composition.ScreenPosition = new Vector2(0f, -0.1f);

        // Responsive damping for platformer
        composer.Damping = new Vector3(0.1f, 0.15f, 0f);

        vcam.Lens.OrthographicSize = 5f;

        SerializedObject so = new SerializedObject(controller);
        so.FindProperty("cinemachineCamera").objectReferenceValue = vcam;
        so.ApplyModifiedProperties();

        Camera mainCam = Camera.main;
        if (mainCam != null && mainCam.GetComponent<CinemachineBrain>() == null)
        {
            Undo.AddComponent<CinemachineBrain>(mainCam.gameObject);
            Debug.Log("CinemachineBrain added to Main Camera.");
        }

        EditorGUIUtility.PingObject(go);
    }

    private CinemachineCamera FindBoundCamera(PlatformerPlayerController controller)
    {
        var prop = new SerializedObject(controller).FindProperty("cinemachineCamera");
        if (prop.objectReferenceValue != null)
            return prop.objectReferenceValue as CinemachineCamera;

        foreach (CinemachineCamera vcam in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
            if (vcam.Follow == controller.transform) return vcam;

        return null;
    }

    private Vector3 GetBottomOffset(PlatformerPlayerController controller)
    {
        BoxCollider2D col = controller.GetComponent<BoxCollider2D>();
        if (col != null)
            return new Vector3(col.offset.x, col.offset.y - col.size.y / 2f, 0f);
        return new Vector3(0f, -0.5f, 0f);
    }
}