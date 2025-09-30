// Place this file in an Editor folder, e.g. Assets/Editor/IntegrateEnemyWindow.cs
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations; // <- runtime AnimatorOverrideController lives here

public class IntegrateEnemyWindow : EditorWindow
{
    private UnityEngine.Object fbxObject;
    private Vector2 scroll;

    // Paths used by the tool (change if your project structure differs)
    private const string BaseFolder = "Assets/FPS_Game_Assignment/Enemy";
    private const string AnimatorsFolder = BaseFolder + "/Animators";
    private const string ScriptsFolder = BaseFolder + "/Scripts";
    private const string SOFolder = BaseFolder + "/SO";
    private const string PrefabsFolder = BaseFolder + "/Prefabs";
    private const string EnemyCanvasPrefabPath = PrefabsFolder + "/EnemyCanvas.prefab";

    [MenuItem("FPSGame/Integrate Enemy")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<IntegrateEnemyWindow>("Integrate Enemy");
        wnd.minSize = new Vector2(420, 160);
    }

    private void OnGUI()
    {
        GUILayout.Space(8);
        EditorGUILayout.LabelField("Integrate Enemy", EditorStyles.boldLabel);
        GUILayout.Space(4);

        EditorGUILayout.HelpBox("Select the FBX (model/prefab) from the Project window and press Integrate. The tool will create a scene GameObject and wire components/assets.", MessageType.Info);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("FBX (model/prefab)", "Drag the FBX model/prefab from the Project window here."), GUILayout.Width(160));
        EditorGUI.BeginChangeCheck();
        fbxObject = EditorGUILayout.ObjectField(fbxObject, typeof(UnityEngine.Object), false);
        if (EditorGUI.EndChangeCheck())
        {
            // nothing special
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Integrate", GUILayout.Width(140), GUILayout.Height(28)))
        {
            // run integration
            if (fbxObject == null)
            {
                EditorUtility.DisplayDialog("Integrate Enemy", "Please assign an FBX model/prefab in the field.", "OK");
            }
            else
            {
                try
                {
                    IntegrateEnemy(fbxObject);
                }
                catch (Exception ex)
                {
                    Debug.LogError("IntegrateEnemy failed: " + ex);
                    EditorUtility.DisplayDialog("Integrate Enemy - Error", ex.Message, "OK");
                }
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Notes:", EditorStyles.miniLabel);
        EditorGUILayout.LabelField("- Will create folders if missing: Animators, Scripts, SO.", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("- Detection uses Layer \"Player\". Ensure it exists.", EditorStyles.wordWrappedMiniLabel);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folderName = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static Type FindTypeByName(string shortName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var asm in assemblies)
        {
            try
            {
                var type = asm.GetTypes().FirstOrDefault(t => t.Name == shortName);
                if (type != null) return type;
            }
            catch { }
        }
        return null;
    }

    private static string SanitizeName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(input.Where(c => !invalid.Contains(c)).ToArray());
        cleaned = cleaned.Replace(" ", "_");
        if (string.IsNullOrEmpty(cleaned)) cleaned = "Enemy";
        // C# class names cannot start with digit
        if (char.IsDigit(cleaned[0])) cleaned = "_" + cleaned;
        return cleaned;
    }

    private void IntegrateEnemy(UnityEngine.Object fbxObj)
    {
        // Ensure output folders exist
        EnsureFolder(AnimatorsFolder);
        EnsureFolder(ScriptsFolder);
        EnsureFolder(SOFolder);

        // Determine name
        string rawName = fbxObj.name;
        string enemyName = SanitizeName(rawName);

        // Create root GameObject
        GameObject root = new GameObject(enemyName);
        Undo.RegisterCreatedObjectUndo(root, "Create Enemy Root");

        // Add CapsuleCollider - default values
        var capsule = Undo.AddComponent<CapsuleCollider>(root);
        capsule.center = Vector3.zero;
        capsule.radius = 0.5f;
        capsule.height = 2f;

        // Animator + controller asset (use runtime AnimatorOverrideController)
        var overrideController = new AnimatorOverrideController();
        string animatorAssetPath = $"{AnimatorsFolder}/{enemyName}_Override.controller";
        AssetDatabase.CreateAsset(overrideController, animatorAssetPath);
        AssetDatabase.SaveAssets();

        var animator = Undo.AddComponent<Animator>(root);
        //animator.runtimeAnimatorController = overrideController;

        // Try to assign avatar from the FBX (if present).
        Avatar avatar = null;
        if (AssetDatabase.Contains(fbxObj))
        {
            var assetPath = AssetDatabase.GetAssetPath(fbxObj);
            var objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            avatar = objs.OfType<Avatar>().FirstOrDefault();
        }
        if (avatar != null)
        {
            animator.avatar = avatar;
        }

        // NavMeshAgent
        var agent = Undo.AddComponent<NavMeshAgent>(root);

        // Rigidbody with freeze constraints
        var rb = Undo.AddComponent<Rigidbody>(root);
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = false;

        // Create script file inheriting BaseEnemy
        string className = enemyName;
        string scriptPath = $"{ScriptsFolder}/{className}.cs";
        if (!File.Exists(scriptPath))
        {
            string scriptTemplate = $@"using UnityEngine;

public class {className} : BaseEnemy
{{
    // Auto-generated enemy class. Add behavior overrides here if needed.
}}";
            File.WriteAllText(scriptPath, scriptTemplate);
            AssetDatabase.ImportAsset(scriptPath);
            AssetDatabase.Refresh();
        }

        // Add the generated enemy script as component (if the type exists after import)
        Type enemyType = FindTypeByName(className);
        Component enemyComponent = null;
        if (enemyType != null && enemyType.IsSubclassOf(typeof(MonoBehaviour)))
        {
            enemyComponent = Undo.AddComponent(root, enemyType);
        }
        else
        {
            // fallback: try to add BaseEnemy
            Type baseEnemyType = FindTypeByName("BaseEnemy");
            if (baseEnemyType != null)
                enemyComponent = Undo.AddComponent(root, baseEnemyType);
            else
                Debug.LogWarning("Could not find generated enemy type or BaseEnemy type to attach. You will need to attach manually.");
        }

        // Create EnemyData ScriptableObject asset
        Type enemyDataType = FindTypeByName("EnemyData");
        UnityEngine.Object enemyDataAsset = null;
        if (enemyDataType != null && enemyDataType.IsSubclassOf(typeof(ScriptableObject)))
        {
            var soInstance = ScriptableObject.CreateInstance(enemyDataType);
            string soPath = $"{SOFolder}/{enemyName}_EnemyData.asset";
            AssetDatabase.CreateAsset(soInstance, soPath);
            AssetDatabase.SaveAssets();
            enemyDataAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(soPath);
        }
        else
        {
            Debug.LogWarning("EnemyData ScriptableObject type not found - cannot create asset.");
        }

        // Create Detection child and attach EnemySensor
        GameObject detectionGO = new GameObject("Detection");
        Undo.RegisterCreatedObjectUndo(detectionGO, "Create Detection");
        detectionGO.transform.SetParent(root.transform, false);
        var sphere = Undo.AddComponent<SphereCollider>(detectionGO);
        sphere.isTrigger = true;
        sphere.center = Vector3.zero;
        sphere.radius = 3f; // default radius

        Type sensorType = FindTypeByName("EnemySensor");
        Component sensorComp = null;
        if (sensorType != null)
        {
            sensorComp = Undo.AddComponent(detectionGO, sensorType);

            // set first LayerMask field on sensor to layer "Player" if present
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                var field = sensorType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType == typeof(LayerMask));
                if (field != null)
                {
                    object lm = (LayerMask)(1 << playerLayer);
                    field.SetValue(sensorComp, lm);
                    EditorUtility.SetDirty((UnityEngine.Object)sensorComp);
                }
                else
                {
                    var prop = sensorType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .FirstOrDefault(p => p.PropertyType == typeof(LayerMask) && p.CanWrite);
                    if (prop != null)
                    {
                        object lm = (LayerMask)(1 << playerLayer);
                        prop.SetValue(sensorComp, lm);
                        EditorUtility.SetDirty((UnityEngine.Object)sensorComp);
                    }
                    else
                    {
                        Debug.LogWarning("EnemySensor does not expose a LayerMask field/property we could find. Set detection mask manually.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Layer named 'Player' not found. Detection mask not set.");
            }
        }
        else
        {
            Debug.LogWarning("EnemySensor type not found. Create the Detection child and attach EnemySensor manually.");
        }

        // Attach EnemyHealth to root (object which has NavMeshAgent)
        Type enemyHealthType = FindTypeByName("EnemyHealth");
        Component enemyHealthComp = null;
        if (enemyHealthType != null)
        {
            enemyHealthComp = Undo.AddComponent(root, enemyHealthType);
        }
        else
        {
            Debug.LogWarning("EnemyHealth type not found - please attach it manually.");
        }

        // Wire references on the enemy component: enemyData, sensor, enemyHealth (use SerializedObject)
        if (enemyComponent != null)
        {
            var so = new SerializedObject((UnityEngine.Object)enemyComponent);
            var propEnemyData = so.FindProperty("enemyData");
            if (propEnemyData != null && enemyDataAsset != null)
            {
                propEnemyData.objectReferenceValue = enemyDataAsset;
            }
            var propSensor = so.FindProperty("sensor");
            if (propSensor != null && sensorComp != null)
            {
                propSensor.objectReferenceValue = (UnityEngine.Object)sensorComp;
            }
            var propEnemyHealth = so.FindProperty("enemyHealth");
            if (propEnemyHealth != null && enemyHealthComp != null)
            {
                propEnemyHealth.objectReferenceValue = (UnityEngine.Object)enemyHealthComp;
            }
            so.ApplyModifiedProperties();
        }

        // Instantiate EnemyCanvas prefab as child
        GameObject enemyCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyCanvasPrefabPath);
        if (enemyCanvasPrefab != null)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(enemyCanvasPrefab);
            Undo.RegisterCreatedObjectUndo(inst, "Instantiate EnemyCanvas");
            inst.transform.SetParent(root.transform, false);
        }
        else
        {
            Debug.LogWarning($"EnemyCanvas prefab not found at path '{EnemyCanvasPrefabPath}'.");
        }

        // Put the provided FBX as child of root.
        GameObject fbxSceneInstance = null;
        if (AssetDatabase.Contains(fbxObj))
        {
            var path = AssetDatabase.GetAssetPath(fbxObj);
            var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabRoot != null)
            {
                fbxSceneInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
                Undo.RegisterCreatedObjectUndo(fbxSceneInstance, "Instantiate FBX");
            }
            else
            {
                // fallback: instantiate a generic object
                fbxSceneInstance = (GameObject)Instantiate(fbxObj);
                Undo.RegisterCreatedObjectUndo(fbxSceneInstance, "Instantiate FBX (fallback)");
            }
        }
        else
        {
            // user dragged a scene GameObject - just clone it
            var go = fbxObj as GameObject;
            if (go != null)
            {
                fbxSceneInstance = (GameObject)Instantiate(go);
                Undo.RegisterCreatedObjectUndo(fbxSceneInstance, "Clone FBX Scene Object");
            }
        }

        if (fbxSceneInstance != null)
        {
            fbxSceneInstance.name = rawName;
            fbxSceneInstance.transform.SetParent(root.transform, false);
            fbxSceneInstance.transform.localPosition = Vector3.zero;
            fbxSceneInstance.transform.localRotation = Quaternion.identity;
            fbxSceneInstance.transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning("Failed to instantiate or clone the FBX into the scene. Please add the visual model manually as a child of the created root.");
        }

        // Select the created root in the editor and mark scene dirty
        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log($"Enemy '{enemyName}' integrated successfully.");
    }
}
