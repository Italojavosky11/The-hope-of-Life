#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class JohnKentBossSetup
{
    private const string ControllerPath = "Assets/Animaao/JohnKent/John Kent.controller";
    private const string AnimationPath = "Assets/Animaao/JohnKent/";
    private const string ScenePath = "Assets/Scenes/Level 3.unity";

    [MenuItem("Tools/Boss/Configure John Kent Boss")]
    public static void Configure()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) { EditorUtility.DisplayDialog("John Kent", "Animator Controller not found.", "OK"); return; }
        ConfigureController(controller);
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject boss = GameObject.Find("John Kent");
        if (boss == null) { EditorUtility.DisplayDialog("John Kent", "John Kent was not found in Level 3.", "OK"); return; }
        ConfigureBoss(boss, controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureController(AnimatorController controller)
    {
        controller.parameters = new[] { new AnimatorControllerParameter { name = "Animacao", type = AnimatorControllerParameterType.Int } };
        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in sm.states) sm.RemoveState(child.state);
        foreach (AnimatorStateTransition transition in sm.anyStateTransitions) sm.RemoveAnyStateTransition(transition);
        string[] clips = { "walkNormal", "jkAtirandoParado", "jkFrenteAtirandoParado", "jkCostaAtirandoParado", "jkAtirandoAndando", "jkFrenteAtirandoAndando", "jkCostaAtirandoAndando", "walkFrente", "walkCosta", "Idle", "Idle frente", "idle costa" };
        AnimatorState[] states = new AnimatorState[clips.Length];
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimationPath + clips[i] + ".anim");
            if (clip == null) { EditorUtility.DisplayDialog("John Kent", "Missing animation: " + clips[i], "OK"); return; }
            states[i] = sm.AddState(clips[i], new Vector3(260, 40 + i * 70));
            states[i].motion = clip;
            states[i].writeDefaultValues = true;
        }
        sm.defaultState = states[9];
        for (int i = 0; i < states.Length; i++)
        {
            AnimatorStateTransition transition = sm.AddAnyStateTransition(states[i]);
            transition.hasExitTime = false;
            transition.duration = 0.08f;
            transition.offset = 0f;
            transition.canTransitionToSelf = false;
            transition.AddCondition(AnimatorConditionMode.Equals, i, "Animacao");
        }
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureBoss(GameObject boss, AnimatorController controller)
    {
        Animator animator = boss.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        Rigidbody2D body = boss.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        JohnKentBoss ai = boss.GetComponent<JohnKentBoss>();
        if (ai == null) ai = Undo.AddComponent<JohnKentBoss>(boss);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) ai.player = player.transform;
        GameObject bullet = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bala.prefab");
        if (bullet != null)
        {
            ai.projectilePrefab = bullet;
            SpriteRenderer renderer = bullet.GetComponent<SpriteRenderer>();
            if (renderer != null) ai.projectileSprite = renderer.sprite;
        }
        ai.rightWeapon.firePoint = GetOrCreateFirePoint(boss.transform, "RightFirePoint", new Vector3(0.22f, 0.16f, 0f));
        ai.leftWeapon.firePoint = GetOrCreateFirePoint(boss.transform, "LeftFirePoint", new Vector3(0.22f, -0.08f, 0f));
        EditorUtility.SetDirty(ai);
        EditorUtility.SetDirty(animator);
    }

    private static Transform GetOrCreateFirePoint(Transform parent, string name, Vector3 localPosition)
    {
        Transform point = parent.Find(name);
        if (point == null)
        {
            GameObject child = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(child, "Create John Kent muzzle point");
            point = child.transform;
            point.SetParent(parent, false);
        }
        point.localPosition = localPosition;
        return point;
    }
}
#endif



