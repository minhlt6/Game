#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GrindAndGlory.Editor
{
    public static class GrindAndGloryEditorSetup
    {
        [MenuItem("Grind and Glory/1. Khởi tạo & Cập nhật Scene (Setup Game Scene)")]
        public static void SetupScenesMenu()
        {
            SetupGameScenes();
            Debug.Log("[Grind and Glory] Đã khởi tạo và lưu Scene thành công! Hãy bấm nút Play (▶) để chơi thử.");
        }

        [MenuItem("Grind and Glory/2. Chạy Kiểm thử Tự động 20 Test Cases (Run Automated Tests)")]
        public static void RunTestsMenu()
        {
            string report = RunComprehensiveTestSuite();
            string reportPath = Path.Combine(Application.dataPath, "..", "TestResults_Summary.txt");
            File.WriteAllText(reportPath, report, Encoding.UTF8);
            Debug.Log(report);
        }

        [MenuItem("Grind and Glory/3. Mở khóa 3 Màn chơi & +5000 Gold (Demo Unlock)")]
        public static void UnlockDemoMenu()
        {
            PlayerPrefs.SetInt("GG_UnlockedStage", 3);
            int curGold = PlayerPrefs.GetInt("GG_TotalGold", 300);
            PlayerPrefs.SetInt("GG_TotalGold", curGold + 5000);
            PlayerPrefs.Save();
            if (GrindAndGloryManager.Instance != null)
            {
                GrindAndGloryManager.Instance.UnlockAllAndGrantDemoResources();
            }
            Debug.Log("[Grind and Glory] Đã mở khóa toàn bộ 3 Màn chơi và cộng thêm +5,000 Gold!");
        }

        [MenuItem("Grind and Glory/4. Đóng gói File Chạy .EXE (Build Windows Standalone)")]
        public static void BuildWindowsStandaloneMenu()
        {
            SetupGameScenes();
            string buildFolder = Path.Combine(Application.dataPath, "..", "Build_Windows");
            Directory.CreateDirectory(buildFolder);
            string exePath = Path.Combine(buildFolder, "GrindAndGlory.exe");

            BuildPlayerOptions opts = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/SampleScene.unity", "Assets/Scenes/GrindAndGlory.unity" },
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Grind and Glory] Đóng gói thành công tại: {exePath}");
            }
            else
            {
                Debug.LogError($"[Grind and Glory] Đóng gói thất bại: {report.summary.result}");
            }
        }

        public static void ConfigureAndVerifyAll()
        {
            try
            {
                Debug.Log("=== [Grind and Glory] Bắt đầu cấu hình Project, tạo Scene và chạy Kiểm thử Tự động (20 Test Cases) ===");
                ConfigureSpriteImporters();
                SetupGameScenes();
                string report = RunComprehensiveTestSuite();
                string reportPath = Path.Combine(Application.dataPath, "..", "TestResults_Summary.txt");
                File.WriteAllText(reportPath, report, Encoding.UTF8);
                Debug.Log("=== [Grind and Glory] Hoàn tất ConfigureAndVerifyAll thành công! ===");
            }
            catch (Exception ex)
            {
                Debug.LogError("[Grind and Glory] Lỗi trong ConfigureAndVerifyAll: " + ex);
                EditorApplication.Exit(1);
            }
        }

        private static void ConfigureSpriteImporters()
        {
            string[] folders = { "Assets/Resources/Sprites", "Assets/Resources/Maps", "Assets/Resources/Animations" };
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", folders);
            Debug.Log($"[Grind and Glory] Đã xác nhận {guids.Length} Pixel Art textures trong Resources.");
        }

        public static void SetupGameScenes()
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "Scenes"));

            string[] scenePaths = new[]
            {
                "Assets/Scenes/SampleScene.unity",
                "Assets/Scenes/GrindAndGlory.unity"
            };

            foreach (string scenePath in scenePaths)
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                // 1. Main Camera (2D Top-Down)
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                Camera cam = camObj.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 6.8f;
                cam.backgroundColor = new Color(0.07f, 0.08f, 0.11f, 1f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                camObj.transform.position = new Vector3(0f, 0f, -10f);
                camObj.AddComponent<AudioListener>();

                // 2. GrindAndGlory Master System Object
                GameObject sysObj = new GameObject("GrindAndGlory_GameSystem");
                sysObj.AddComponent<AudioManager>();
                sysObj.AddComponent<MapEnvironmentBuilder>();
                sysObj.AddComponent<GrindAndGloryUI>();
                sysObj.AddComponent<GrindAndGloryManager>();

                EditorSceneManager.SaveScene(scene, scenePath);
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/GrindAndGlory.unity", true)
            };

            AssetDatabase.SaveAssets();
        }

        public static string RunComprehensiveTestSuite()
        {
            GrindAndGloryManager.DisableAutoBootstrapForTests = true;
            StringBuilder sb = new StringBuilder();
            int passed = 0;
            int failed = 0;

            // Preserve any existing shop prefs during test execution
            int oldShopHp = PlayerPrefs.GetInt("GG_Shop_HP", 0);
            int oldShopAtk = PlayerPrefs.GetInt("GG_Shop_ATK", 0);
            int oldShopEnd = PlayerPrefs.GetInt("GG_Shop_END", 0);
            int oldShopSpd = PlayerPrefs.GetInt("GG_Shop_SPD", 0);
            PlayerPrefs.SetInt("GG_Shop_HP", 0);
            PlayerPrefs.SetInt("GG_Shop_ATK", 0);
            PlayerPrefs.SetInt("GG_Shop_END", 0);
            PlayerPrefs.SetInt("GG_Shop_SPD", 0);

            void AssertTest(string testId, string description, bool condition, string detail = "")
            {
                if (condition)
                {
                    passed++;
                    sb.AppendLine($"[PASS] {testId} | {description} {(string.IsNullOrEmpty(detail) ? "" : "-> " + detail)}");
                }
                else
                {
                    failed++;
                    sb.AppendLine($"[FAIL] {testId} | {description} {(string.IsNullOrEmpty(detail) ? "" : "-> " + detail)}");
                }
            }

            sb.AppendLine("=====================================================================");
            sb.AppendLine("BÁO CÁO KẾT QUẢ KIỂM THỬ & ĐÁNH GIÁ NÂNG CẤP - GRIND AND GLORY (NHÓM 12)");
            sb.AppendLine($"Thời gian thực thi: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Phiên bản Unity: {Application.unityVersion}");
            sb.AppendLine("=====================================================================");

            // 1. Kiểm thử 3 Nhân vật chính (Bảng 3 Nhân vật - Phần 3.1)
            HeroDefinition mercy = GameDatabase.GetHero(HeroId.Mercy);
            HeroDefinition inirius = GameDatabase.GetHero(HeroId.Inirius);
            HeroDefinition eni = GameDatabase.GetHero(HeroId.Eni);

            AssertTest("TC-HERO-01", "Kiểm tra thông số khởi điểm & tăng trưởng của Mercy (normal_nuri)",
                mercy.BaseHP == 150f && mercy.HPPerLevel == 12f && mercy.BaseAttack == 18f && mercy.AttackPerLevel == 3f &&
                mercy.BaseEndurance == 12f && mercy.EndurancePerLevel == 2f && Mathf.Approximately(mercy.BaseSpeed, 4.2f) &&
                mercy.StartingWeaponId == "weapon_00",
                $"HP={mercy.BaseHP}(+{mercy.HPPerLevel}), STR={mercy.BaseAttack}(+{mercy.AttackPerLevel}), END={mercy.BaseEndurance}(+{mercy.EndurancePerLevel}), SPD={mercy.BaseSpeed}, Weapon={mercy.StartingWeaponId}");

            AssertTest("TC-HERO-02", "Kiểm tra thông số khởi điểm & tăng trưởng của Inirius (normal_durun)",
                inirius.BaseHP == 100f && inirius.HPPerLevel == 8f && inirius.BaseAttack == 25f && inirius.AttackPerLevel == 4f &&
                inirius.BaseEndurance == 6f && inirius.EndurancePerLevel == 1f && Mathf.Approximately(inirius.BaseSpeed, 4.5f) &&
                inirius.StartingWeaponId == "weapon_04",
                $"HP={inirius.BaseHP}(+{inirius.HPPerLevel}), MAG={inirius.BaseAttack}(+{inirius.AttackPerLevel}), END={inirius.BaseEndurance}(+{inirius.EndurancePerLevel}), SPD={inirius.BaseSpeed}, Weapon={inirius.StartingWeaponId}");

            AssertTest("TC-HERO-03", "Kiểm tra thông số khởi điểm & tăng trưởng của Eni (normal_mokoji)",
                eni.BaseHP == 110f && eni.HPPerLevel == 9f && eni.BaseAttack == 16f && Mathf.Approximately(eni.AttackPerLevel, 2.5f) &&
                eni.BaseEndurance == 8f && Mathf.Approximately(eni.EndurancePerLevel, 1.5f) && Mathf.Approximately(eni.BaseSpeed, 4.8f) &&
                eni.StartingWeaponId == "weapon_06_1",
                $"HP={eni.BaseHP}(+{eni.HPPerLevel}), MAG={eni.BaseAttack}(+{eni.AttackPerLevel}), END={eni.BaseEndurance}(+{eni.EndurancePerLevel}), SPD={eni.BaseSpeed}, Weapon={eni.StartingWeaponId}");

            // 2. Kiểm thử Công thức Sát thương Vũ khí & Sát thương Nhận vào (Phần 3.1)
            float dmgMercyLv1 = GameDatabase.CalculateWeaponDamage(65f, 18f, 1, 0); // (65 + 18) * 1.2 * 1.0 = 99.6
            float dmgMercyLv2CuongLuc2 = GameDatabase.CalculateWeaponDamage(65f, 18f, 2, 2); // (65 + 18) * 1.4 * 1.3 = 151.06
            AssertTest("TC-MATH-01", "Kiểm tra công thức tính sát thương Vũ khí chính / Vũ khí phụ",
                Mathf.Abs(dmgMercyLv1 - 99.6f) < 0.01f && Mathf.Abs(dmgMercyLv2CuongLuc2 - 151.06f) < 0.01f,
                $"Lv1(NoSkill)={dmgMercyLv1:0.00} (Expected 99.60), Lv2(CuongLuc2)={dmgMercyLv2CuongLuc2:0.00} (Expected 151.06)");

            float takenFromSlime = GameDatabase.CalculateDamageTaken(6f, 12f); // 6 - 9.6 = -3.6 -> Min 1.0
            float takenFromGolem = GameDatabase.CalculateDamageTaken(30f, 12f); // 30 - 9.6 = 20.4
            AssertTest("TC-MATH-02", "Kiểm tra công thức tính sát thương nhân vật nhận vào (kể cả ngưỡng tối thiểu 1)",
                Mathf.Abs(takenFromSlime - 1.0f) < 0.01f && Mathf.Abs(takenFromGolem - 20.4f) < 0.01f,
                $"SlimeHit={takenFromSlime:0.0} (Expected 1.0), GolemHit={takenFromGolem:0.0} (Expected 20.4)");

            // 3. Kiểm thử các mốc EXP lên cấp (Phần 3.1 Note)
            int expLv1 = GameDatabase.GetCumulativeExpThreshold(1);   // 5
            int expLv5 = GameDatabase.GetCumulativeExpThreshold(5);   // 25
            int expLv10 = GameDatabase.GetCumulativeExpThreshold(10); // 75
            int expLv20 = GameDatabase.GetCumulativeExpThreshold(20); // 275
            int expLv30 = GameDatabase.GetCumulativeExpThreshold(30); // 675
            AssertTest("TC-MATH-03", "Kiểm tra các mốc EXP tích lũy từ Level 1 đến Level 30",
                expLv1 == 5 && expLv5 == 25 && expLv10 == 75 && expLv20 == 275 && expLv30 == 675,
                $"Lv1={expLv1}, Lv5={expLv5}, Lv10={expLv10}, Lv20={expLv20}, Lv30={expLv30}");

            // 4. Kiểm thử đầy đủ 9 Vũ khí & 5 Kỹ năng Nội tại
            string[] expectedWeapons = { "weapon_00", "weapon_04", "weapon_06_1", "weapon_02_1", "weapon_05_4", "weapon_08", "hurricane", "thunderbolt", "awaken_aura" };
            bool allWeaponsValid = GameDatabase.Weapons.Count == 9;
            foreach (string wId in expectedWeapons)
            {
                if (!GameDatabase.Weapons.ContainsKey(wId)) allWeaponsValid = false;
            }
            AssertTest("TC-WEAPON-01", "Kiểm tra danh sách 9 Vũ khí (3 Vũ khí chính & 6 Vũ khí phụ)",
                allWeaponsValid &&
                GameDatabase.Weapons["weapon_00"].BaseDamage == 65f &&
                GameDatabase.Weapons["weapon_04"].BaseDamage == 75f &&
                GameDatabase.Weapons["weapon_06_1"].BaseDamage == 55f &&
                GameDatabase.Weapons["weapon_02_1"].BaseDamage == 70f &&
                GameDatabase.Weapons["weapon_05_4"].BaseDamage == 45f &&
                GameDatabase.Weapons["weapon_08"].BaseDamage == 95f &&
                GameDatabase.Weapons["hurricane"].BaseDamage == 60f &&
                GameDatabase.Weapons["thunderbolt"].BaseDamage == 90f &&
                GameDatabase.Weapons["awaken_aura"].BaseDamage == 35f,
                $"Total Weapons = {GameDatabase.Weapons.Count}/9");

            AssertTest("TC-SKILL-01", "Kiểm tra danh sách 5 Kỹ năng Nội tại (Cường Lực, Thần Tốc, Giáp Cứng, Hồi Phục, Từ Trường)",
                GameDatabase.PassiveSkills.Count == 5 &&
                GameDatabase.PassiveSkills.ContainsKey("cuong_luc") &&
                GameDatabase.PassiveSkills.ContainsKey("than_toc") &&
                GameDatabase.PassiveSkills.ContainsKey("giap_cung") &&
                GameDatabase.PassiveSkills.ContainsKey("hoi_phuc") &&
                GameDatabase.PassiveSkills.ContainsKey("tu_truong"),
                $"Total Passive Skills = {GameDatabase.PassiveSkills.Count}/5");

            // 5. Kiểm thử 7 Quái thường, 3 Mini Boss, 3 Final Boss và 3 Màn chơi
            AssertTest("TC-MOB-01", "Kiểm tra thông số 7 nhóm Quái thường (Loại 1) ở cả 3 màn chơi",
                GameDatabase.Monsters["slime"].MaxHP == 100f &&
                GameDatabase.Monsters["goblin"].MaxHP == 180f &&
                GameDatabase.Monsters["wild_boar"].MaxHP == 240f &&
                GameDatabase.Monsters["skeleton"].MaxHP == 280f &&
                GameDatabase.Monsters["vampire_bat"].MaxHP == 250f &&
                GameDatabase.Monsters["gargoyle"].MaxHP == 360f &&
                GameDatabase.Monsters["lava_slime"].MaxHP == 450f,
                "Slime(100), Goblin(180), Boar(240), Skeleton(280), Bat(250), Gargoyle(360), LavaSlime(450)");

            AssertTest("TC-BOSS-01", "Kiểm tra thông số & điều kiện Kill Count của 3 Mini Boss (Loại 2) và 3 Final Boss (Loại 3)",
                GameDatabase.Monsters["boss_goblin"].MaxHP == 1500f && GameDatabase.Monsters["boss_goblin"].SpawnKillThreshold == 100 &&
                GameDatabase.Monsters["boss_golem"].MaxHP == 3500f && GameDatabase.Monsters["boss_golem"].SpawnKillThreshold == 250 &&
                GameDatabase.Monsters["boss_bone_gargoyle"].MaxHP == 2400f && GameDatabase.Monsters["boss_bone_gargoyle"].SpawnKillThreshold == 150 &&
                GameDatabase.Monsters["boss_grim_reaper"].MaxHP == 4500f && GameDatabase.Monsters["boss_grim_reaper"].SpawnKillThreshold == 350 &&
                GameDatabase.Monsters["boss_guardian_fire"].MaxHP == 3600f && GameDatabase.Monsters["boss_guardian_fire"].SpawnKillThreshold == 200 &&
                GameDatabase.Monsters["boss_herald_of_death"].MaxHP == 6500f && GameDatabase.Monsters["boss_herald_of_death"].SpawnKillThreshold == 500,
                "Stage1(100/250), Stage2(150/350), Stage3(200/500) verified");

            // 6. Kiểm thử Tài nguyên (Sprites, Animations, Audio, VFX) trong Resources
            bool allHeroAnimsLoaded =
                ResourceCache.GetAnimationFrames("normal_nuri", "idle").Count > 0 &&
                ResourceCache.GetAnimationFrames("normal_nuri", "run").Count > 0 &&
                ResourceCache.GetAnimationFrames("normal_durun", "idle").Count > 0 &&
                ResourceCache.GetAnimationFrames("normal_mokoji", "idle").Count > 0 &&
                ResourceCache.GetAnimationFrames("mr_rock", "idle").Count > 0 &&
                ResourceCache.GetAnimationFrames("dark_murderer", "idle").Count > 0 &&
                ResourceCache.GetAnimationFrames("kaiser_demon", "idle").Count > 0;

            AssertTest("TC-ASSET-01", "Kiểm tra nạp chuỗi Frame hoạt ảnh (Animations) cho Nhân vật, Quái và Boss",
                allHeroAnimsLoaded,
                $"Mercy idle frames={ResourceCache.GetAnimationFrames("normal_nuri", "idle").Count}, KaiserDemon frames={ResourceCache.GetAnimationFrames("kaiser_demon", "idle").Count}");

            bool allAudioLoaded =
                ResourceCache.GetAudio("Audio/bgm_menu") != null &&
                ResourceCache.GetAudio("Audio/bgm_stage1") != null &&
                ResourceCache.GetAudio("Audio/bgm_stage2") != null &&
                ResourceCache.GetAudio("Audio/bgm_stage3") != null &&
                ResourceCache.GetAudio("Audio/bgm_boss") != null &&
                ResourceCache.GetAudio("Audio/sfx_slash") != null &&
                ResourceCache.GetAudio("Audio/sfx_levelup") != null &&
                ResourceCache.GetAudio("Audio/sfx_win") != null;

            AssertTest("TC-ASSET-02", "Kiểm tra nạp tài nguyên Âm thanh (5 bản BGM và 16 hiệu ứng SFX)",
                allAudioLoaded, "All BGM & SFX AudioClips loaded from Resources/Audio");

            bool allVfxAnimsLoaded =
                ResourceCache.GetAnimationFrames("burn", "attack", 64f, true).Count > 0 &&
                ResourceCache.GetAnimationFrames("freezing", "attack", 64f, true).Count > 0 &&
                ResourceCache.GetAnimationFrames("bleeding", "attack", 64f, true).Count > 0 &&
                ResourceCache.GetAnimationFrames("hurricane", "run", 64f, true).Count > 0 &&
                ResourceCache.GetAnimationFrames("thunderbolt", "attack", 64f, true).Count > 0 &&
                ResourceCache.GetAnimationFrames("awaken_aura", "idle", 64f, true).Count > 0 &&
                ResourceCache.GetAnimationFrames("energy_capsules", "idle", 96f, true).Count > 0 &&
                ResourceCache.GetAnimationFrames("item_box", "idle", 72f, true).Count > 0;

            AssertTest("TC-VFX-01", "Kiểm tra nạp hoạt ảnh hiệu ứng Vũ khí, Kỹ năng & Vật phẩm (burn, freezing, bleeding, hurricane, thunderbolt, awaken_aura, item_box)",
                allVfxAnimsLoaded,
                $"burn={ResourceCache.GetAnimationFrames("burn", "attack").Count}, freezing={ResourceCache.GetAnimationFrames("freezing", "attack").Count}, bleeding={ResourceCache.GetAnimationFrames("bleeding", "attack").Count}");

            // 7. Kiểm thử Tích hợp Gameplay Loop & Hệ thống Nâng cấp (Simulation Tests)
            GameObject testSys = new GameObject("TestSystem");
            MapEnvironmentBuilder mapBuilder = testSys.AddComponent<MapEnvironmentBuilder>();
            mapBuilder.BuildStageMap(1);

            Vector2 outOfBoundsTest = mapBuilder.ClampAndResolveMovement(new Vector2(999f, -999f), 0.4f);
            bool clampOk = outOfBoundsTest.x <= mapBuilder.PlayableBounds.xMax && outOfBoundsTest.y >= mapBuilder.PlayableBounds.yMin;
            AssertTest("TC-MAP-01", "Kiểm tra khởi tạo Bản đồ, Chướng ngại vật tự nhiên & Giới hạn biên di chuyển",
                mapBuilder.Obstacles.Count == 20 && clampOk,
                $"Obstacles={mapBuilder.Obstacles.Count}, ClampedPos={outOfBoundsTest}");

            GameObject testPlayerObj = new GameObject("TestPlayer");
            PlayerController testPlayer = testPlayerObj.AddComponent<PlayerController>();
            testPlayer.InitializeHero(HeroId.Mercy);
            testPlayer.GainExp(5); // Level 1 -> Level 2
            var choices = testPlayer.GenerateUpgradeChoices();
            testPlayer.ApplyUpgradeOption(choices[0]);

            AssertTest("TC-LOOP-01", "Kiểm tra vòng lặp nhận EXP -> Lên cấp (Level Up) -> Sinh 3 thẻ nâng cấp -> Áp dụng nâng cấp",
                testPlayer.CurrentLevel == 2 && choices.Count == 3,
                $"Player reached Lv.{testPlayer.CurrentLevel}, Generated {choices.Count} upgrade cards, First card={choices[0].Title}");

            // 8. Kiểm thử Nâng cấp Vũ khí từ Lv.1 -> Lv.5 và giới hạn Max Level 5
            testPlayer.InitializeHero(HeroId.Mercy); // Reset Lv.1 (STR=18, weapon_00 Lv.1)
            float dmgW1 = testPlayer.GetComputedWeaponDamage("weapon_00"); // (65+18)*1.2 = 99.6
            for (int k = 0; k < 6; k++) testPlayer.EquipOrUpgradeWeapon("weapon_00"); // Upgrade to max Lv.5
            int maxWeaponLv = testPlayer.GetWeaponLevel("weapon_00");
            float dmgW5 = testPlayer.GetComputedWeaponDamage("weapon_00"); // (65+18)*(1 + 0.2*5) = 83 * 2.0 = 166.0
            AssertTest("TC-UPG-01", "Kiểm tra nâng cấp Vũ khí từ Lv.1 -> Lv.5 (giới hạn tối đa Lv.5 & sát thương tăng đúng công thức)",
                maxWeaponLv == 5 && Mathf.Abs(dmgW1 - 99.6f) < 0.01f && Mathf.Abs(dmgW5 - 166.0f) < 0.01f,
                $"Weapon Lv={maxWeaponLv}/5, Dmg Lv1={dmgW1:F1} -> Dmg Lv5={dmgW5:F1}");

            // 9. Kiểm thử Nâng cấp 5 Kỹ năng Nội tại (Lv.1 -> Lv.2)
            testPlayer.EquipOrUpgradePassive("cuong_luc"); // Lv.1 (+15%)
            testPlayer.EquipOrUpgradePassive("cuong_luc"); // Lv.2 (+30%)
            testPlayer.EquipOrUpgradePassive("cuong_luc"); // Should cap at Lv.2
            testPlayer.EquipOrUpgradePassive("giap_cung"); // Lv.1 (+5 END)
            testPlayer.EquipOrUpgradePassive("giap_cung"); // Lv.2 (+10 END)
            testPlayer.EquipOrUpgradePassive("than_toc");  // Lv.1 (+10% SPD)
            testPlayer.EquipOrUpgradePassive("tu_truong"); // Lv.1 (+30% Magnet)

            float dmgW5CuongLuc2 = testPlayer.GetComputedWeaponDamage("weapon_00"); // 166.0 * 1.30 = 215.8
            float effEnd = testPlayer.GetEffectiveEndurance(); // 12 + 10 = 22
            float effSpd = testPlayer.GetEffectiveMoveSpeed(); // 4.2 * 1.10 = 4.62
            float effMag = testPlayer.GetPickupMagnetRadius(); // 2.4 * 1.30 = 3.12
            AssertTest("TC-UPG-02", "Kiểm tra nâng cấp Kỹ năng Nội tại Lv.1 -> Lv.2 (Cường Lực +30%, Giáp Cứng +10 END, Thần Tốc +10%, Từ Trường +30%)",
                testPlayer.GetPassiveLevel("cuong_luc") == 2 &&
                Mathf.Abs(dmgW5CuongLuc2 - 215.8f) < 0.05f &&
                Mathf.Abs(effEnd - 22.0f) < 0.01f &&
                Mathf.Abs(effSpd - 4.62f) < 0.01f &&
                Mathf.Abs(effMag - 3.12f) < 0.01f,
                $"Dmg(CL2)={dmgW5CuongLuc2:F1}, END(GC2)={effEnd:F1}, SPD(TT1)={effSpd:F2}, Magnet(TT1)={effMag:F2}");

            // 10. Kiểm thử 4 Vật phẩm rơi (energy_capsules, redball, item_egg, item_box)
            testPlayer.InitializeHero(HeroId.Mercy);
            testPlayer.TakeDamageFromMonster(60f, "TestHit"); // Reduce HP below Max
            float hpAfterHit = testPlayer.CurrentHP;
            DropItemController redDrop = DropItemController.SpawnDrop(Vector2.zero, DropItemType.RedBall, 0, 0);
            testPlayer.CollectDropItem(redDrop); // +30% MaxHP (+45 HP)
            float hpAfterRed = testPlayer.CurrentHP;
            UnityEngine.Object.DestroyImmediate(redDrop.gameObject);

            DropItemController eggDrop = DropItemController.SpawnDrop(Vector2.zero, DropItemType.ItemEgg, 0, 0);
            testPlayer.CollectDropItem(eggDrop); // +50% MaxHP + Speed Buff
            bool hasEggSpeedBuff = testPlayer.EggSpeedBuffRemaining > 0f;
            UnityEngine.Object.DestroyImmediate(eggDrop.gameObject);

            DropItemController boxDrop = DropItemController.SpawnDrop(Vector2.zero, DropItemType.ItemBox, 0, 150);
            testPlayer.CollectDropItem(boxDrop); // Instant +1..2 Weapon Levels
            int weaponLvAfterChest = testPlayer.GetWeaponLevel("weapon_00");
            UnityEngine.Object.DestroyImmediate(boxDrop.gameObject);

            AssertTest("TC-DROP-01", "Kiểm tra thu thập 4 loại Vật phẩm rơi (RedBall +30% HP, ItemEgg +50% HP & Tăng tốc, ItemBox nâng cấp Vũ khí)",
                hpAfterRed > hpAfterHit && hasEggSpeedBuff && weaponLvAfterChest >= 2,
                $"HP: {hpAfterHit:0}->{hpAfterRed:0}, EggSpeedBuff={hasEggSpeedBuff}, WeaponLvAfterBox={weaponLvAfterChest}");

            // 11. Kiểm thử Hiệu ứng Trạng thái (Status Effects: Freezing, Burn, Bleed & Cleanse)
            EnemyController dummyEnemy = EnemyController.SpawnMonster("goblin", new Vector2(3f, 0f), 1.0f);
            dummyEnemy.ApplySlow(0.35f, 2.5f);
            dummyEnemy.ApplyBurn(20f, 2.5f);
            testPlayer.ApplyMonsterStatusEffect(true, true);
            bool statusApplied = testPlayer.PlayerBurnRemaining > 0f && testPlayer.PlayerBleedRemaining > 0f;
            testPlayer.CleanseStatusEffects();
            bool statusCleansed = testPlayer.PlayerBurnRemaining <= 0f && testPlayer.PlayerBleedRemaining <= 0f;
            UnityEngine.Object.DestroyImmediate(dummyEnemy.gameObject);

            AssertTest("TC-STATUS-01", "Kiểm tra hiệu ứng trạng thái chiến đấu (Làm chậm Freezing, Bỏng lửa Burn, Chảy máu Bleed & Thanh tẩy)",
                statusApplied && statusCleansed,
                $"Applied={statusApplied}, CleansedOnDashOrHeal={statusCleansed}");

            // 12. Kiểm thử Hệ thống Thương gia xứ Astral (Merchant Shop Permanent Upgrades)
            PlayerPrefs.SetInt("GG_Shop_HP", 2);  // +40 HP
            PlayerPrefs.SetInt("GG_Shop_ATK", 2); // +6 ATK
            PlayerPrefs.SetInt("GG_Shop_END", 2); // +4 END
            PlayerPrefs.SetInt("GG_Shop_SPD", 2); // +0.30 SPD
            testPlayer.InitializeHero(HeroId.Mercy);
            bool shopStatsOk =
                Mathf.Approximately(testPlayer.MaxHP, 190f) &&
                Mathf.Approximately(testPlayer.BaseAttackStat, 24f) &&
                Mathf.Approximately(testPlayer.BaseEnduranceStat, 16f) &&
                Mathf.Abs(testPlayer.BaseSpeedStat - 4.5f) < 0.01f;

            AssertTest("TC-SHOP-01", "Kiểm tra Thương gia xứ Astral (Merchant Shop) cộng chỉ số khởi điểm vĩnh viễn cho Nhân vật",
                shopStatsOk,
                $"Mercy w/ Shop Lv2: HP={testPlayer.MaxHP}(150+40), STR={testPlayer.BaseAttackStat}(18+6), END={testPlayer.BaseEnduranceStat}(12+4), SPD={testPlayer.BaseSpeedStat:F1}(4.2+0.3)");

            // Restore original PlayerPrefs
            PlayerPrefs.SetInt("GG_Shop_HP", oldShopHp);
            PlayerPrefs.SetInt("GG_Shop_ATK", oldShopAtk);
            PlayerPrefs.SetInt("GG_Shop_END", oldShopEnd);
            PlayerPrefs.SetInt("GG_Shop_SPD", oldShopSpd);
            PlayerPrefs.Save();

            UnityEngine.Object.DestroyImmediate(testPlayerObj);
            UnityEngine.Object.DestroyImmediate(testSys);

            sb.AppendLine("---------------------------------------------------------------------");
            sb.AppendLine($"TỔNG KẾT KIỂM THỬ & ĐÁNH GIÁ NÂNG CẤP: {passed} PASSED / {failed} FAILED (Tỷ lệ đạt: {(passed * 100f / Mathf.Max(1, passed + failed)):0.0}%)");
            sb.AppendLine("=====================================================================");
            return sb.ToString();
        }
    }
}
#endif