using System.Collections.Generic;
using UnityEngine;

namespace GrindAndGlory
{
    public enum GameFlowState
    {
        MainMenu = 0,
        PreGameSetup = 1,
        MerchantShop = 2,
        Playing = 3,
        LevelUpSelection = 4,
        Paused = 5,
        Victory = 6,
        Defeat = 7
    }

    public class GrindAndGloryManager : MonoBehaviour
    {
        public static GrindAndGloryManager Instance { get; private set; }
        public static bool DisableAutoBootstrapForTests = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (DisableAutoBootstrapForTests) return;
            if (FindAnyObjectByType<GrindAndGloryManager>() == null)
            {
                GameObject root = new GameObject("GrindAndGlory_GameSystem");
                root.AddComponent<GrindAndGloryManager>();
            }
        }

        public GameFlowState State { get; private set; } = GameFlowState.MainMenu;
        public bool IsGameplayActive => State == GameFlowState.Playing;

        public HeroId SelectedHero { get; set; } = HeroId.Mercy;
        public int SelectedStageNumber { get; set; } = 1;
        public StageDefinition CurrentStage { get; private set; }

        public int CurrentKills { get; private set; }
        public int SessionGold { get; private set; }
        public int TotalSavedGold { get; private set; }
        public float StageElapsedTime { get; private set; }

        public bool MiniBossSpawned { get; private set; }
        public bool MiniBossDefeated { get; private set; }
        public bool FinalBossSpawned { get; private set; }
        public bool FinalBossDefeated { get; private set; }
        public EnemyController ActiveBoss { get; private set; }

        private float _spawnTimer;
        private int _pendingLevelUps;
        private List<UpgradeOption> _currentUpgradeChoices = new List<UpgradeOption>();
        private Camera _mainCamera;
        private GrindAndGloryUI _ui;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            TotalSavedGold = PlayerPrefs.GetInt("GG_TotalGold", 300);
            SelectedHero = (HeroId)PlayerPrefs.GetInt("GG_LastHero", 0);
            SelectedStageNumber = Mathf.Clamp(PlayerPrefs.GetInt("GG_LastStage", 1), 1, 3);

            EnsureCoreComponents();
        }

        private void Start()
        {
            OpenMainMenu();
        }

        private void EnsureCoreComponents()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                _mainCamera = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }
            _mainCamera.orthographic = true;
            _mainCamera.orthographicSize = 8.2f;
            _mainCamera.backgroundColor = new Color(0.07f, 0.08f, 0.11f, 1f);
            _mainCamera.transform.position = new Vector3(0f, 0f, -10f);

            if (GetComponent<AudioManager>() == null)
            {
                gameObject.AddComponent<AudioManager>();
            }
            if (GetComponent<MapEnvironmentBuilder>() == null)
            {
                gameObject.AddComponent<MapEnvironmentBuilder>();
            }
            _ui = GetComponent<GrindAndGloryUI>();
            if (_ui == null)
            {
                _ui = gameObject.AddComponent<GrindAndGloryUI>();
            }
            _ui.InitializeUI(this);
        }

        public void OpenMainMenu()
        {
            Time.timeScale = 1f;
            State = GameFlowState.MainMenu;
            CleanupActiveEntities();

            if (MapEnvironmentBuilder.Instance != null)
            {
                MapEnvironmentBuilder.Instance.BuildStageMap(SelectedStageNumber);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBgm("Audio/bgm_menu");
            }
            if (_ui != null)
            {
                _ui.RefreshStateUI();
            }
        }

        public void OpenPreGameSetup()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_button");
            State = GameFlowState.PreGameSetup;
            if (_ui != null) _ui.RefreshStateUI();
        }

        public void OpenMerchantShop()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_button");
            State = GameFlowState.MerchantShop;
            if (_ui != null) _ui.RefreshStateUI();
        }

        public void ContinueSavedGame()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_button");
            SelectedHero = (HeroId)PlayerPrefs.GetInt("GG_LastHero", 0);
            SelectedStageNumber = Mathf.Clamp(PlayerPrefs.GetInt("GG_LastStage", 1), 1, 3);
            StartStageBattle(SelectedHero, SelectedStageNumber);
        }

        public void StartStageBattle(HeroId heroId, int stageNumber)
        {
            Time.timeScale = 1f;
            SelectedHero = heroId;
            SelectedStageNumber = Mathf.Clamp(stageNumber, 1, 3);

            PlayerPrefs.SetInt("GG_LastHero", (int)SelectedHero);
            PlayerPrefs.SetInt("GG_LastStage", SelectedStageNumber);
            PlayerPrefs.Save();

            CurrentStage = GameDatabase.GetStage(SelectedStageNumber);
            CurrentKills = 0;
            SessionGold = 0;
            StageElapsedTime = 0f;
            MiniBossSpawned = false;
            MiniBossDefeated = false;
            FinalBossSpawned = false;
            FinalBossDefeated = false;
            ActiveBoss = null;
            _pendingLevelUps = 0;
            _spawnTimer = 0.5f;

            CleanupActiveEntities();

            // 1. Build Stage Map
            if (MapEnvironmentBuilder.Instance != null)
            {
                MapEnvironmentBuilder.Instance.BuildStageMap(SelectedStageNumber);
            }

            // 2. Spawn Chosen Single Hero at (0, 0)
            GameObject playerObj = new GameObject($"Player_{SelectedHero}");
            playerObj.transform.position = Vector3.zero;
            PlayerController player = playerObj.AddComponent<PlayerController>();
            player.InitializeHero(SelectedHero);

            // 3. Spawn initial wave around player
            for (int i = 0; i < 6; i++)
            {
                SpawnRegularMonsterAroundPlayer();
            }

            // 4. Play Stage BGM & Show HUD
            State = GameFlowState.Playing;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBgm(CurrentStage.BgmTrack);
            }
            if (_ui != null)
            {
                _ui.RefreshStateUI();
                _ui.ShowBanner($"{CurrentStage.Title}\nTiêu diệt {CurrentStage.MiniBossKillsRequired} quái gọi Mini Boss | {CurrentStage.FinalBossKillsRequired} quái gọi Final Boss!");
            }
        }

        private void CleanupActiveEntities()
        {
            if (PlayerController.Instance != null)
            {
                Destroy(PlayerController.Instance.gameObject);
            }

            for (int i = EnemyController.ActiveEnemies.Count - 1; i >= 0; i--)
            {
                if (EnemyController.ActiveEnemies[i] != null)
                {
                    Destroy(EnemyController.ActiveEnemies[i].gameObject);
                }
            }
            EnemyController.ActiveEnemies.Clear();

            for (int i = DropItemController.ActiveDrops.Count - 1; i >= 0; i--)
            {
                if (DropItemController.ActiveDrops[i] != null)
                {
                    Destroy(DropItemController.ActiveDrops[i].gameObject);
                }
            }
            DropItemController.ActiveDrops.Clear();

            foreach (var proj in FindObjectsByType<CombatProjectile>(FindObjectsInactive.Exclude))
            {
                if (proj != null) Destroy(proj.gameObject);
            }
        }

        private void Update()
        {
            // Check F1 Quick Demo Unlock & +5000 Gold Shortcut
            if (InputHelper.IsQuickUnlockPressedThisFrame())
            {
                UnlockAllAndGrantDemoResources();
            }

            // Check Pause Key (R or Esc)
            if (InputHelper.IsPausePressedThisFrame())
            {
                if (State == GameFlowState.Playing)
                {
                    TogglePauseMenu(true);
                    return;
                }
                else if (State == GameFlowState.Paused)
                {
                    TogglePauseMenu(false);
                    return;
                }
            }

            // Check Level Up Keyboard Shortcuts (1, 2, 3)
            if (State == GameFlowState.LevelUpSelection)
            {
                int keyIdx = InputHelper.GetUpgradeCardKeyThisFrame();
                if (keyIdx >= 0 && keyIdx < _currentUpgradeChoices.Count)
                {
                    SelectUpgradeCard(keyIdx);
                    return;
                }
            }

            if (State != GameFlowState.Playing) return;

            float dt = Time.deltaTime;
            StageElapsedTime += dt;

            // Camera Follow Player within Map Bounds
            UpdateCameraFollow(dt);

            // Continuous Monster Wave Spawning
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                int maxRegular = 26 + SelectedStageNumber * 5;
                if (EnemyController.ActiveEnemies.Count < maxRegular)
                {
                    int batch = CurrentKills >= CurrentStage.MiniBossKillsRequired ? 3 : 2;
                    for (int i = 0; i < batch; i++)
                    {
                        SpawnRegularMonsterAroundPlayer();
                    }
                }
                _spawnTimer = Mathf.Max(0.55f, 1.35f - (StageElapsedTime / 180f) * 0.45f);
            }

            // Check Boss Spawn Thresholds
            CheckBossMilestones();
        }

        private void UpdateCameraFollow(float dt)
        {
            if (_mainCamera == null || PlayerController.Instance == null) return;

            // Cho phép lăn chuột (Scroll Wheel) để thu phóng góc nhìn toàn cảnh tùy ý (từ 5.5 đến 11.5, mặc định 8.2)
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _mainCamera.orthographicSize = Mathf.Clamp(_mainCamera.orthographicSize - scroll * 0.6f, 5.5f, 11.5f);
            }

            Vector3 target = PlayerController.Instance.transform.position;
            target.z = -10f;

            if (MapEnvironmentBuilder.Instance != null)
            {
                Rect b = MapEnvironmentBuilder.Instance.PlayableBounds;
                target.x = Mathf.Clamp(target.x, b.xMin + 6f, b.xMax - 6f);
                target.y = Mathf.Clamp(target.y, b.yMin + 4f, b.yMax - 4f);
            }

            _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, target, dt * 8f);
        }

        private void SpawnRegularMonsterAroundPlayer()
        {
            if (PlayerController.Instance == null || CurrentStage == null) return;

            string[] pool = CurrentStage.RegularMonsterIds;
            // Weight spawn progression by kill count
            int maxIdx = pool.Length - 1;
            if (CurrentKills < CurrentStage.MiniBossKillsRequired * 0.35f) maxIdx = Mathf.Min(1, pool.Length - 1);
            string monsterId = pool[UnityEngine.Random.Range(0, maxIdx + 1)];

            Vector2 playerPos = PlayerController.Instance.transform.position;
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float dist = UnityEngine.Random.Range(9.0f, 12.8f);
            Vector2 spawnPos = playerPos + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            if (MapEnvironmentBuilder.Instance != null)
            {
                spawnPos = MapEnvironmentBuilder.Instance.ClampAndResolveMovement(spawnPos, 0.45f);
            }

            float scale = 1.0f + Mathf.Floor(StageElapsedTime / 60f) * 0.08f;
            EnemyController.SpawnMonster(monsterId, spawnPos, scale);
        }

        private void CheckBossMilestones()
        {
            if (CurrentStage == null || PlayerController.Instance == null) return;

            if (!MiniBossSpawned && CurrentKills >= CurrentStage.MiniBossKillsRequired)
            {
                SpawnStageMiniBoss();
            }

            if (!FinalBossSpawned && CurrentKills >= CurrentStage.FinalBossKillsRequired)
            {
                SpawnStageFinalBoss();
            }
        }

        public void SpawnStageMiniBoss()
        {
            if (CurrentStage == null || PlayerController.Instance == null) return;
            MiniBossSpawned = true;

            Vector2 pPos = PlayerController.Instance.transform.position;
            Vector2 dir = PlayerController.Instance.LastMoveDirection.sqrMagnitude > 0.01f
                ? PlayerController.Instance.LastMoveDirection.normalized
                : Vector2.right;
            Vector2 spawnPos = pPos + dir * 6.0f;
            if (MapEnvironmentBuilder.Instance != null)
            {
                spawnPos = MapEnvironmentBuilder.Instance.ClampAndResolveMovement(spawnPos, 0.7f);
            }

            float lvlBonus = 1.0f + Mathf.Max(0, PlayerController.Instance.CurrentLevel - 1) * 0.14f;
            ActiveBoss = EnemyController.SpawnMonster(CurrentStage.MiniBossId, spawnPos, 2.6f * lvlBonus);
            // Triệu hồi thêm đội cận vệ hộ tống Mini Boss
            string guardId = CurrentStage.RegularMonsterIds[CurrentStage.RegularMonsterIds.Length - 1];
            for (int i = 0; i < 3; i++)
            {
                Vector2 gOffset = UnityEngine.Random.insideUnitCircle.normalized * 2.4f;
                EnemyController.SpawnMonster(guardId, spawnPos + gOffset, 1.5f);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("sfx_boss_spawn", 0.9f);
                AudioManager.Instance.PlayBgm("Audio/bgm_boss");
            }
            ShowBannerMessage($"CẢNH BÁO MINI BOSS XUẤT HIỆN!\n{ActiveBoss.Definition.DisplayName} cùng Đội Cận Vệ đã chặn đường!");
        }

        public void SpawnStageFinalBoss()
        {
            if (CurrentStage == null || PlayerController.Instance == null) return;
            FinalBossSpawned = true;

            Vector2 pPos = PlayerController.Instance.transform.position;
            Vector2 dir = PlayerController.Instance.LastMoveDirection.sqrMagnitude > 0.01f
                ? PlayerController.Instance.LastMoveDirection.normalized
                : Vector2.right;
            Vector2 spawnPos = pPos + dir * 6.5f;
            if (MapEnvironmentBuilder.Instance != null)
            {
                spawnPos = MapEnvironmentBuilder.Instance.ClampAndResolveMovement(spawnPos, 0.85f);
            }

            float lvlBonus = 1.0f + Mathf.Max(0, PlayerController.Instance.CurrentLevel - 1) * 0.16f;
            ActiveBoss = EnemyController.SpawnMonster(CurrentStage.FinalBossId, spawnPos, 3.2f * lvlBonus);
            // Triệu hồi đội tinh nhuệ hộ tống Trùm Cuối
            string guardId = CurrentStage.RegularMonsterIds[CurrentStage.RegularMonsterIds.Length - 1];
            for (int i = 0; i < 4; i++)
            {
                Vector2 gOffset = UnityEngine.Random.insideUnitCircle.normalized * 2.8f;
                EnemyController.SpawnMonster(guardId, spawnPos + gOffset, 1.8f);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("sfx_boss_spawn", 1.0f);
                AudioManager.Instance.PlayBgm("Audio/bgm_boss");
            }
            ShowBannerMessage($"TRÙM CUỐI MÀN XUẤT HIỆN!\n{ActiveBoss.Definition.DisplayName} đã giáng lâm với sức mạnh hủy diệt!");
        }

        public void OnMonsterKilled(EnemyController enemy)
        {
            if (enemy == null) return;
            CurrentKills++;

            if (enemy.Definition.Category == MonsterCategory.MiniBoss)
            {
                MiniBossDefeated = true;
                if (ActiveBoss == enemy) ActiveBoss = null;
                if (AudioManager.Instance != null && CurrentStage != null && !FinalBossSpawned)
                {
                    AudioManager.Instance.PlayBgm(CurrentStage.BgmTrack);
                }
                ShowBannerMessage($"ĐÃ HẠ GỤC MINI BOSS {enemy.Definition.DisplayName}!\nRương Vũ Khí (item_box) đã rơi ra trên bản đồ!");
            }
            else if (enemy.Definition.Category == MonsterCategory.FinalBoss)
            {
                FinalBossDefeated = true;
                if (ActiveBoss == enemy) ActiveBoss = null;

                // Win condition: Player HP > 0 and Final Boss HP <= 0
                if (PlayerController.Instance != null && !PlayerController.Instance.IsDead && PlayerController.Instance.CurrentHP > 0f)
                {
                    OnStageVictory();
                }
            }
        }

        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            SessionGold += amount;
            TotalSavedGold += amount;
            PlayerPrefs.SetInt("GG_TotalGold", TotalSavedGold);
        }

        public bool TrySpendGold(int cost)
        {
            if (TotalSavedGold < cost) return false;
            TotalSavedGold -= cost;
            PlayerPrefs.SetInt("GG_TotalGold", TotalSavedGold);
            PlayerPrefs.Save();
            return true;
        }

        public void UnlockAllAndGrantDemoResources()
        {
            PlayerPrefs.SetInt("GG_UnlockedStage", 3);
            AddGold(5000);
            PlayerPrefs.Save();

            if (PlayerController.Instance != null && !PlayerController.Instance.IsDead)
            {
                PlayerController.Instance.Heal(9999f, true);
                PlayerController.Instance.CleanseStatusEffects();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("sfx_levelup", 0.85f);
            }
            if (_ui != null)
            {
                _ui.RefreshStateUI();
                _ui.ShowBanner("[CHẾ ĐỘ DEMO / F1] Đã mở khóa toàn bộ 3 Màn chơi & nhận +5,000 Gold!");
            }
        }

        public void TriggerLevelUpSelection()
        {
            if (PlayerController.Instance == null || PlayerController.Instance.IsDead) return;

            if (State == GameFlowState.LevelUpSelection)
            {
                _pendingLevelUps++;
                return;
            }

            _currentUpgradeChoices = PlayerController.Instance.GenerateUpgradeChoices();
            State = GameFlowState.LevelUpSelection;
            Time.timeScale = 0f;
            if (_ui != null)
            {
                _ui.ShowLevelUpCards(_currentUpgradeChoices);
            }
        }

        public void SelectUpgradeCard(int index)
        {
            if (State != GameFlowState.LevelUpSelection) return;
            if (index < 0 || index >= _currentUpgradeChoices.Count) return;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_button", 0.85f);
            UpgradeOption chosen = _currentUpgradeChoices[index];
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.ApplyUpgradeOption(chosen);
            }

            if (_pendingLevelUps > 0)
            {
                _pendingLevelUps--;
                _currentUpgradeChoices = PlayerController.Instance.GenerateUpgradeChoices();
                if (_ui != null) _ui.ShowLevelUpCards(_currentUpgradeChoices);
                return;
            }

            Time.timeScale = 1f;
            State = GameFlowState.Playing;
            if (_ui != null)
            {
                _ui.RefreshStateUI();
            }
        }

        public void TogglePauseMenu(bool pause)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_button");
            if (pause)
            {
                State = GameFlowState.Paused;
                Time.timeScale = 0f;
            }
            else
            {
                State = GameFlowState.Playing;
                Time.timeScale = 1f;
            }
            if (_ui != null) _ui.RefreshStateUI();
        }

        private void OnStageVictory()
        {
            // Unlock next stage
            int unlocked = PlayerPrefs.GetInt("GG_UnlockedStage", 1);
            if (SelectedStageNumber >= unlocked && SelectedStageNumber < 3)
            {
                PlayerPrefs.SetInt("GG_UnlockedStage", SelectedStageNumber + 1);
            }
            AddGold(CurrentStage != null ? CurrentStage.StageNumber * 200 : 200);
            PlayerPrefs.Save();

            State = GameFlowState.Victory;
            Time.timeScale = 0f;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("sfx_win", 1.0f);
            }
            if (_ui != null)
            {
                _ui.RefreshStateUI();
            }
        }

        public void OnPlayerDefeated()
        {
            PlayerPrefs.Save();
            State = GameFlowState.Defeat;
            Time.timeScale = 0f;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("sfx_defeat", 1.0f);
            }
            if (_ui != null)
            {
                _ui.RefreshStateUI();
            }
        }

        public void ShowBannerMessage(string msg)
        {
            if (_ui != null) _ui.ShowBanner(msg);
        }

        // Quick Debug/Test Helpers for University Grading & Demo
        public void DebugAddKills(int count)
        {
            CurrentKills += count;
            CheckBossMilestones();
            if (_ui != null) _ui.RefreshStateUI();
        }
    }
}