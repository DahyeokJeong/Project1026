using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>Deterministic integration checks against the imported prefab and real damage registry.</summary>
public static class WhiteTigerVerification
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int assertions;

    [MenuItem("Project1026/Boss/Verify White Tiger")]
    public static void Verify()
    {
        if (EditorApplication.isPlaying) throw new Exception("Run verification outside Play mode.");
        var managerObject = new GameObject("White tiger verification managers");
        var manager = managerObject.AddComponent<Managers>();
        var instance = typeof(Managers).GetField("s_instance", BindingFlags.Static | BindingFlags.NonPublic);
        object previous = instance.GetValue(null);
        instance.SetValue(null, manager);
        var targetObject = new GameObject("Test player");
        var player = targetObject.AddComponent<PlayerModel>();
        GwaegangWhiteTiger boss = null;
        GameObject battleObject = null;
        float damage = 0f;
        int hits = 0;
        try
        {
            assertions = 0;
            var stage = AssetDatabase.LoadAssetAtPath<StageDataSO>("Assets/Project1026/06.Datas/SO/Stage Data/StageData01.asset");
            Check(stage != null && stage.BossPrefab != null, "Stage 1 has a boss prefab");
            boss = UnityEngine.Object.Instantiate(stage.BossPrefab);
            Managers.Damage.Register(player.transform, value => { damage += value; hits++; });
            void Reset(float distance)
            {
                Managers.BattleState.Reset();
                boss.transform.position = Vector3.zero;
                player.transform.position = Vector3.right * distance;
                boss.Initialize(player, null, null);
                damage = 0f;
                hits = 0;
            }
            void Tick(float dt) => typeof(GwaegangWhiteTiger).GetMethod("Tick", Private).Invoke(boss, new object[] { dt });
            void Set(string name, float value) => typeof(GwaegangWhiteTiger).GetField(name, Private).SetValue(boss, value);

            Reset(1f);
            Tick(1.99f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.Idle, "Initial claw cooldown");
            Tick(0.01f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.Claw, "Claw starts at 2 seconds");
            Tick(0.4f); Tick(0.1f);
            Check(hits == 1 && damage == 75f, "Claw hits once for 75");

            Reset(1f); Tick(2f);
            player.transform.position = Vector3.left;
            Tick(0.4f);
            Check(hits == 0, "Claw direction remains locked");

            Reset(4f); Set("clock", 9.99f); Tick(0.01f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.Stomp, "Stomp priority at inclusive 4-unit boundary");
            Tick(0.99f); Check(hits == 0, "Stomp warns for one second");
            Tick(0.011f); Tick(0.1f);
            Check(hits == 1 && damage == 113f, "Stomp hits once and rounds 112.5 up");
            Check(boss.State == GwaegangWhiteTiger.ActionState.Stomp, "Stomp recovery locks actions");
            Check(Mathf.Approximately((float)typeof(GwaegangWhiteTiger).GetField("nextStomp", Private).GetValue(boss), 20f), "Stomp cooldown begins at cast start");

            Reset(8f); Set("clock", 5.99f); Tick(0.01f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.JumpReady, "Jump starts at 6 seconds and 8-unit boundary");
            player.transform.position = new Vector3(20f, 0f);
            Tick(1f); Tick(1f);
            Check(Vector2.Distance(boss.transform.position, Vector2.right * 8f) < 0.001f && hits == 0, "Jump landing point remains fixed and can be dodged");
            Check(boss.State == GwaegangWhiteTiger.ActionState.Recovery, "Jump has recovery");

            Reset(6f); Set("clock", 6f); Tick(0f); Tick(1f); Tick(1f);
            Check(hits == 1 && damage == 150f, "Jump impact hits once for 150");
            Reset(8.01f); Set("clock", 10f); Tick(0f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.Chase, "Beyond 8 units only chases");

            Reset(1f); Managers.BattleState.SetState(BattleState.AugmentSelecting); Tick(20f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.Idle && hits == 0, "Augment selection freezes AI");
            Managers.BattleState.Reset(); Tick(1f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.Idle, "Pause freezes cooldown clock");
            Managers.Damage.ApplyDamage(boss.transform, 95f);
            Check(boss.CurrentHp == 725f, "Existing player damage registry reaches boss and applies defense 20");
            int deaths = 0;
            boss.Defeated += () => deaths++;
            boss.TakeDamage(10000f);
            Check(boss.State == GwaegangWhiteTiger.ActionState.Dead && deaths == 0, "Death does not clear before animation");
            Check(!Managers.Damage.IsRegistered(boss.transform) && !boss.GetComponent<Collider2D>().enabled, "Dead boss cannot be targeted");
            Tick(0.79f); Check(deaths == 0, "Death animation completes before event");
            Tick(0.02f); Tick(2f);
            Check(deaths == 1, "Death event emitted exactly once");

            battleObject = new GameObject("Test battle");
            var spawner = battleObject.AddComponent<EnemyGenerate>();
            var controller = battleObject.AddComponent<BattleSceneController>();
            var ui = battleObject.AddComponent<BossBattleUI>();
            ui.Bind(boss);
            void Configure(string name, object value) => typeof(BattleSceneController).GetField(name, Private).SetValue(controller, value);
            Configure("currentStageData", stage);
            Configure("enemyGenerate", spawner);
            Configure("battleRunning", true);
            Configure("boss", boss);
            Configure("bossUI", ui);
            int beforeReward = Managers.GameState.SoulEssence;
            typeof(BattleSceneController).GetMethod("EndBattle", Private).Invoke(controller, new object[] { true });
            Check(Managers.BattleState.Current == BattleState.Cleared && Managers.GameState.IsStageCleared(1), "Boss clear records stage progression");
            Check(Managers.GameState.SoulEssence == beforeReward + 10, "Stage 1 reward is awarded");
            Check(battleObject.GetComponentInChildren<UnityEngine.UI.Button>() != null, "Reward UI has a return button");
            typeof(BattleSceneController).GetMethod("EndBattle", Private).Invoke(controller, new object[] { true });
            Check(Managers.GameState.SoulEssence == beforeReward + 10, "Duplicate clear does not duplicate reward");
            Debug.Log($"WHITE_TIGER_VERIFIED: {assertions} assertions passed.");
        }
        finally
        {
            if (battleObject != null) UnityEngine.Object.DestroyImmediate(battleObject);
            if (boss != null) UnityEngine.Object.DestroyImmediate(boss.gameObject);
            UnityEngine.Object.DestroyImmediate(targetObject);
            UnityEngine.Object.DestroyImmediate(managerObject);
            instance.SetValue(null, previous);
        }
    }

    private static void Check(bool passed, string message)
    {
        if (!passed) throw new Exception("White tiger verification failed: " + message);
        assertions++;
    }
}
