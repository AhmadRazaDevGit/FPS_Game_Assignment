
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntegrateWeaponWindow : EditorWindow
{
    private UnityEngine.Object weaponAsset; 
    private GUIContent weaponFieldLabel = new GUIContent("Weapon FBX/Prefab", "Drag the FBX or prefab for the weapon here.");
    private string statusMessage = "";

    private const string scriptsFolder = "Assets/FPS_Game_Assignment/Weapons/Scripts/Guns";
    private const string soFolder = "Assets/FPS_Game_Assignment/Weapons/SO";
    private const string bulletsPath = "Assets/FPS_Game_Assignment/Weapons/Prefabs/Bullets/RifleBullet.prefab";
    private const string sessionKey = "IntegrateWeaponWindow.Pending";

    private static PendingIntegration pending;

    [MenuItem("FPSGame/Integrate Weapon")]
    public static void ShowWindow()
    {
        var w = GetWindow<IntegrateWeaponWindow>("Integrate Weapon");
        w.minSize = new Vector2(420, 140);
    }

    private void OnGUI()
    {
        GUILayout.Label("Integrate Weapon Into Scene", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select an FBX or prefab for a weapon. Clicking Integrate will create a Weapon script, a WeaponData SO, " +
            "instantiate the model as a child of a created root GameObject, create a MuzzleEffect child, add an ObjectPool child and assign the RifleBullet prefab.", MessageType.Info);

        weaponAsset = EditorGUILayout.ObjectField(weaponFieldLabel, weaponAsset, typeof(UnityEngine.Object), false);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(weaponAsset == null))
        {
            if (GUILayout.Button("Integrate"))
            {
                try
                {
                    Integrate();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    statusMessage = "ERROR: " + ex.Message;
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Status:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(statusMessage, MessageType.None);
    }

    private void Integrate()
    {
        if (weaponAsset == null)
        {
            statusMessage = "No FBX/prefab selected.";
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(weaponAsset);
        if (string.IsNullOrEmpty(assetPath))
        {
            statusMessage = "Selected object must be a project asset (FBX or Prefab).";
            return;
        }

        string baseName = Path.GetFileNameWithoutExtension(assetPath);
        if (string.IsNullOrEmpty(baseName))
        {
            statusMessage = "Could not determine name from selected asset.";
            return;
        }

    
        EnsureFolderExists("Assets/FPS_Game_Assignment");
        EnsureFolderExists("Assets/FPS_Game_Assignment/Weapons");
        EnsureFolderExists(scriptsFolder);
        EnsureFolderExists(soFolder);

     
        string scriptPath = $"{scriptsFolder}/{baseName}.cs";
        if (!File.Exists(scriptPath))
        {
            string scriptCode = GenerateWeaponScriptCode(baseName);
            File.WriteAllText(scriptPath, scriptCode);
            Debug.Log($"Created script: {scriptPath}");
        }
        else
        {
            Debug.Log($"Script already exists: {scriptPath} (will reuse)");
        }

       
        string soPath = $"{soFolder}/{baseName}.asset";
        UnityEngine.Object soInstance = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(soPath);
        if (soInstance == null)
        {
            
            UnityEngine.Object newSo = null;
            Type weaponDataType = GetTypeByName("WeaponData");
            if (weaponDataType != null && weaponDataType.IsSubclassOf(typeof(ScriptableObject)))
            {
                newSo = ScriptableObject.CreateInstance(weaponDataType);
                AssetDatabase.CreateAsset(newSo, soPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"Created ScriptableObject asset: {soPath}");
            }
            else
            {
                
                ScriptableObject placeholder = ScriptableObject.CreateInstance<ScriptableObject>();
                AssetDatabase.CreateAsset(placeholder, soPath);
                AssetDatabase.SaveAssets();
                Debug.LogWarning("WeaponData type not found at creation time. Created placeholder asset; the real type will be assigned after compile.");
            }
        }
        else
        {
            Debug.Log($"WeaponData asset already exists: {soPath} (will reuse)");
        }


        string rootName = baseName;
        GameObject rootGO = new GameObject(rootName);
        Undo.RegisterCreatedObjectUndo(rootGO, "Create Weapon Root");

 
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

     
        UnityEngine.Object instantiated = null;
        try
        {
            instantiated = PrefabUtility.InstantiatePrefab(weaponAsset);
            if (instantiated is GameObject child)
            {
                child.transform.SetParent(rootGO.transform, false);
            }
            else
            {
                Debug.LogWarning("Instantiated asset is not a GameObject.");
            }
        }
        catch (Exception)
        {
            
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go != null)
            {
                GameObject instGO = (GameObject)PrefabUtility.InstantiatePrefab(go);
                instGO.transform.SetParent(rootGO.transform, false);
                instantiated = instGO;
            }
            else
            {
                Debug.LogWarning("Unable to instantiate prefab; model will not be parented.");
            }
        }

       
        GameObject muzzle = new GameObject("MuzzleEffect");
        muzzle.transform.SetParent(rootGO.transform, false);

       
        GameObject poolGO = new GameObject("Object Pool");
        poolGO.transform.SetParent(rootGO.transform, false);
        Component poolComp = null;
        Type poolType = GetTypeByName("ObjectPool");
        if (poolType != null)
        {
            poolComp = poolGO.AddComponent(poolType);
        }
        else
        {
            Debug.LogWarning("ObjectPool type not found in project. Object Pool GameObject created; add ObjectPool component later.");
        }

        
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletsPath);
        if (poolComp != null)
        {
            SerializedObject poolSo = new SerializedObject(poolComp);
            var prop = poolSo.FindProperty("prefab") ?? poolSo.FindProperty("_prefab") ?? poolSo.FindProperty("Prefab");
            if (prop != null)
            {
                prop.objectReferenceValue = bulletPrefab;
                poolSo.ApplyModifiedProperties();
                Debug.Log($"Assigned bullet prefab to pool: {bulletsPath}");
            }
            else
            {
                Debug.LogWarning("Could not find a field named 'prefab' on ObjectPool. You may need to set it manually.");
            }
        }

        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        
        var gid = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(rootGO);
        pending = new PendingIntegration
        {
            scriptClassName = baseName,
            rootGlobalId = gid.ToString(),
            soAssetPath = soPath,
            muzzleName = "MuzzleEffect",
            bulletPrefabPath = bulletsPath,
            poolObjectName = "Object Pool"
        };
        
        SessionState.SetString(sessionKey, JsonUtility.ToJson(pending));

        statusMessage = $"Created files & scene objects. Waiting for compile to attach '{baseName}' to root.";
        Debug.Log(statusMessage);
    }

    
    [InitializeOnLoadMethod]
    private static void AutoFinalizeAfterReload()
    {
        TryFinalizePending();
    }

    private static void TryFinalizePending()
    {
        
        var json = SessionState.GetString(sessionKey, string.Empty);
        if (!string.IsNullOrEmpty(json))
        {
            pending = JsonUtility.FromJson<PendingIntegration>(json);
        }
        if (pending == null)
            return;

        try
        {
            
            GameObject root = null;
            if (!string.IsNullOrEmpty(pending.rootGlobalId) && UnityEditor.GlobalObjectId.TryParse(pending.rootGlobalId, out var gid))
            {
                var obj = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(gid);
                root = obj as GameObject;
            }
            if (root == null)
            {
                Debug.LogError("Pending weapon integration failed: could not locate the created root GameObject.");
                pending = null;
                SessionState.EraseString(sessionKey);
                return;
            }

            
            Type scriptType = GetTypeByNameStatic(pending.scriptClassName);
            if (scriptType != null)
            {
                if (root.GetComponent(scriptType) == null)
                {
                    Undo.AddComponent(root, scriptType);
                    Debug.Log($"Added component {pending.scriptClassName} to {root.name}");
                }
            }
            else
            {
                Debug.LogError($"Script type '{pending.scriptClassName}' not found after compilation. Check for compile errors.");
            }

            
            UnityEngine.Object soObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(pending.soAssetPath);
            if (soObj != null && scriptType != null)
            {
                Component comp = root.GetComponent(scriptType);
                if (comp != null)
                {
                    SerializedObject compSo = new SerializedObject(comp);
                    var soProp = compSo.FindProperty("data") ?? compSo.FindProperty("weaponData") ?? compSo.FindProperty("weapon") ?? compSo.FindProperty("weaponDataAsset");
                    if (soProp != null)
                    {
                        soProp.objectReferenceValue = soObj;
                        compSo.ApplyModifiedProperties();
                    }
                  
                    var muzzleObj = FindChildByName(root.transform, pending.muzzleName);
                    if (muzzleObj != null)
                    {
                        var muzzleProp = compSo.FindProperty("muzzleTransform") ?? compSo.FindProperty("muzzle") ?? compSo.FindProperty("muzzlePoint");
                        if (muzzleProp != null)
                        {
                            muzzleProp.objectReferenceValue = muzzleObj.transform;
                            compSo.ApplyModifiedProperties();
                        }
                    }

                   
                    var poolRefGO = FindChildByName(root.transform, pending.poolObjectName);
                    if (poolRefGO != null)
                    {
                        
                        var objectPoolType = GetTypeByNameStatic("ObjectPool");
                        Component poolComp = objectPoolType != null ? poolRefGO.GetComponent(objectPoolType) : null;

                      
                        SerializedProperty poolProp =
                            compSo.FindProperty("projectilePool") ??
                            compSo.FindProperty("pool") ??
                            compSo.FindProperty("bulletPool") ??
                            compSo.FindProperty("objectPoolGO") ??
                            compSo.FindProperty("poolGO") ??
                            compSo.FindProperty("poolTransform") ??
                            compSo.FindProperty("objectPoolTransform");

                        if (poolProp != null)
                        {
                         
                            UnityEngine.Object assignObj = null;
                            if (poolComp != null)
                                assignObj = (UnityEngine.Object)poolComp;
                            else
                                assignObj = (UnityEngine.Object)poolRefGO;

                           
                            if (poolProp.propertyPath.ToLower().Contains("transform"))
                                assignObj = poolRefGO.transform;

                            poolProp.objectReferenceValue = assignObj;
                            compSo.ApplyModifiedProperties();
                        }
                    }
                }
            }

            
            GameObject poolGO = FindChildByName(root.transform, pending.poolObjectName);
            if (poolGO != null)
            {
                Component poolComp = poolGO.GetComponent(GetTypeByNameStatic("ObjectPool"));
                if (poolComp != null)
                {
                    SerializedObject poolSo = new SerializedObject(poolComp);
                    var prop = poolSo.FindProperty("prefab") ?? poolSo.FindProperty("_prefab") ?? poolSo.FindProperty("Prefab");
                    if (prop != null)
                    {
                        var bulletPrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(pending.bulletPrefabPath);
                        prop.objectReferenceValue = bulletPrefab;
                        poolSo.ApplyModifiedProperties();
                    }
                }
            }

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            
            pending = null;
            SessionState.EraseString(sessionKey);
            Debug.Log("Weapon integration finalization complete.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            pending = null;
            SessionState.EraseString(sessionKey);
        }
    }

    private static GameObject FindChildByName(Transform parent, string childName)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName)
                return t.gameObject;
        }
        return null;
    }

    private static void EnsureFolderExists(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
        {
            string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
            string newFolderName = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolderExists(parent);
            AssetDatabase.CreateFolder(parent, newFolderName);
            AssetDatabase.Refresh();
        }
    }

    private string GenerateWeaponScriptCode(string className)
    {

        return
$@"using UnityEngine;

public class {className} : WeaponBase
{{
    // Auto-generated weapon script. Modify as needed.

    protected override void OnFireEffects()
    {{
        base.OnFireEffects();
        // TODO: add custom fx here
    }}
}}
";
    }

    private static Type GetTypeByName(string typeName)
    {
       
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(typeName);
                if (t != null) return t;

                
                var found = asm.GetTypes().FirstOrDefault(x => x.Name == typeName);
                if (found != null) return found;
            }
            catch
            {
               
            }
        }
        return null;
    }

    private static Type GetTypeByNameStatic(string typeName)
    {
        return GetTypeByName(typeName);
    }

    [Serializable]
    private class PendingIntegration
    {
        public string scriptClassName;
        public string rootGlobalId;
        public string soAssetPath;
        public string muzzleName;
        public string bulletPrefabPath;
        public string poolObjectName;
    }
}
