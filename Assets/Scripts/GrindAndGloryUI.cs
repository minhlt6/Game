using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace GrindAndGlory
{
    public class GrindAndGloryUI : MonoBehaviour
    {
        private GrindAndGloryManager _manager;
        private Canvas _canvas;
        private Font _font;

        // Top-level panels
        private GameObject _mainMenuPanel;
        private GameObject _setupPanel;
        private GameObject _merchantPanel;
        private GameObject _hudPanel;
        private GameObject _levelUpPanel;
        private GameObject _pausePanel;
        private GameObject _victoryPanel;
        private GameObject _defeatPanel;

        // Pre-game setup dynamic elements
        private Image[] _heroCardBorders = new Image[3];
        private Image[] _stageCardBorders = new Image[3];
        private Text _setupSummaryText;

        // Merchant shop dynamic elements
        private Text _merchantGoldText;
        private Text[] _shopLevelTexts = new Text[4];

        // HUD dynamic elements
        private Image _hudHeroPortrait;
        private Text _hudLevelText;
        private Image _hudHpFill;
        private Text _hudHpText;
        private Image _hudExpFill;
        private Text _hudExpText;
        private Text _hudStatsText;
        private Text _hudDashAndBuffText;
        private Text _hudKillCounterText;
        private Text _hudBossProgressText;
        private Text _hudRadarTrackerText;
        private Text _hudGoldAndTimeText;

        // Boss HUD
        private GameObject _hudBossBarObj;
        private Text _hudBossNameText;
        private Image _hudBossHpFill;
        private Text _hudBossHpValueText;

        // Banner notification
        private GameObject _bannerObj;
        private Text _bannerText;
        private float _bannerTimer;

        // Weapon & Passive Slots on HUD
        private Transform _weaponSlotsContainer;
        private Transform _passiveSlotsContainer;
        private int _lastEquipmentHash = -1;

        // Level Up Cards
        private Text _levelUpHeaderText;
        private readonly Button[] _upgradeCardButtons = new Button[3];
        private readonly Image[] _upgradeCardHeaderBars = new Image[3];
        private readonly Image[] _upgradeCardIcons = new Image[3];
        private readonly Text[] _upgradeCardTitles = new Text[3];
        private readonly Text[] _upgradeCardSubtitles = new Text[3];
        private readonly Text[] _upgradeCardDescs = new Text[3];

        // Pause panel elements
        private Text _pauseGodModeBtnText;

        // Victory / Defeat elements
        private Text _victoryTitleText;
        private Text _victoryDetailText;
        private GameObject _victoryNextStageBtnObj;
        private Text _defeatDetailText;

        public void InitializeUI(GrindAndGloryManager manager)
        {
            _manager = manager;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            EnsureEventSystem();
            CreateMasterCanvas();

            BuildMainMenuPanel();
            BuildPreGameSetupPanel();
            BuildMerchantShopPanel();
            BuildGameplayHudPanel();
            BuildLevelUpPanel();
            BuildPauseSettingsPanel();
            BuildVictoryPanel();
            BuildDefeatPanel();

            RefreshStateUI();
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esObj.AddComponent<InputSystemUIInputModule>();
#else
            esObj.AddComponent<StandaloneInputModule>();
#endif
        }

        private void CreateMasterCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas_GrindAndGloryUI");
            canvasObj.transform.SetParent(transform, false);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // ==================== 1. MAIN MENU PANEL ====================
        private void BuildMainMenuPanel()
        {
            _mainMenuPanel = CreateFullScreenPanel("MainMenuPanel", new Color(0.06f, 0.07f, 0.12f, 0.94f));

            // Decorative dark frame card
            GameObject centerCard = CreatePanel(_mainMenuPanel.transform, "CenterCard", new Vector2(0.5f, 0.5f), new Vector2(1140f, 880f), new Color(0.11f, 0.13f, 0.20f, 0.97f));
            AddOutline(centerCard, new Color(0.95f, 0.75f, 0.25f, 0.9f), 3f);

            CreateText(centerCard.transform, "UniversityTag", "TRƯỜNG ĐẠI HỌC THỦY LỢI - KHOA CNTT | ĐỒ ÁN THIẾT KẾ & PHÁT TRIỂN TRÒ CHƠI (NHÓM 12 - PIXEL GAMING)",
                new Vector2(0f, 385f), new Vector2(1060f, 36f), 19, new Color(0.75f, 0.85f, 1f), FontStyle.Bold);

            CreateText(centerCard.transform, "GameTitle", "GRIND AND GLORY",
                new Vector2(0f, 315f), new Vector2(1000f, 84f), 66, new Color(1f, 0.84f, 0.25f), FontStyle.Bold);

            CreateText(centerCard.transform, "GameSubtitle", "2D Top-Down Action RPG Survival • Giải Cứu Vùng Đất Astral",
                new Vector2(0f, 252f), new Vector2(960f, 36f), 23, new Color(0.9f, 0.92f, 0.96f), FontStyle.Italic);

            // Heroes Banner from Docx (Hình 1.1 / 3.1)
            GameObject bannerImgObj = CreateImage(centerCard.transform, "HeroesBanner", "Sprites/banner_heroes", new Vector2(0f, 115f), new Vector2(620f, 196f));
            AddOutline(bannerImgObj, new Color(0.3f, 0.35f, 0.5f, 0.8f), 2f);

            CreateText(centerCard.transform, "HeroNamesCaption", "Mercy (Chiến binh)   •   Inirius (Pháp sư)   •   Eni (Phù thủy)",
                new Vector2(0f, -2f), new Vector2(820f, 32f), 20, new Color(0.85f, 0.9f, 1f), FontStyle.Bold);

            // Menu Buttons
            CreateMenuButton(centerCard.transform, "BtnNewGame", "BẮT ĐẦU HÀNH TRÌNH (CHỌN NHÂN VẬT & MÀN CHƠI)", new Vector2(0f, -78f), new Vector2(640f, 64f),
                new Color(0.88f, 0.48f, 0.12f), () => _manager.OpenPreGameSetup());

            CreateMenuButton(centerCard.transform, "BtnContinue", "TIẾP TỤC TRẬN ĐẤU GẦN NHẤT (CONTINUE)", new Vector2(0f, -155f), new Vector2(640f, 58f),
                new Color(0.18f, 0.55f, 0.32f), () => _manager.ContinueSavedGame());

            CreateMenuButton(centerCard.transform, "BtnMerchant", "THƯƠNG GIA XỨ ASTRAL (NÂNG CẤP TRANG BỊ BẰNG VÀNG)", new Vector2(0f, -228f), new Vector2(640f, 58f),
                new Color(0.24f, 0.42f, 0.72f), () => _manager.OpenMerchantShop());

            CreateMenuButton(centerCard.transform, "BtnExit", "THOÁT TRÒ CHƠI (EXIT)", new Vector2(0f, -300f), new Vector2(640f, 54f),
                new Color(0.55f, 0.20f, 0.24f), () =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });

            CreateText(centerCard.transform, "ControlFooter", "Điều khiển: [W/A/S/D hoặc Mũi Tên] Di chuyển & Đánh  |  [Space/Shift] Lướt né  |  [T] Tự động ngắm  |  [1/2/3] Chọn thẻ  |  [F1] +5000 Gold  |  [R/Esc] Menu",
                new Vector2(0f, -385f), new Vector2(1080f, 44f), 16, new Color(0.7f, 0.75f, 0.85f), FontStyle.Normal);
        }

        // ==================== 2. PRE-GAME SETUP PANEL ====================
        private void BuildPreGameSetupPanel()
        {
            _setupPanel = CreateFullScreenPanel("PreGameSetupPanel", new Color(0.06f, 0.08f, 0.13f, 0.96f));

            CreateText(_setupPanel.transform, "HeaderTitle", "THIẾT LẬP TRƯỚC TRẬN ĐẤU (CHỌN NHÂN VẬT & MÀN CHƠI)",
                new Vector2(0f, 475f), new Vector2(1400f, 54f), 38, new Color(1f, 0.85f, 0.25f), FontStyle.Bold);

            // Section 1: 3 Heroes
            CreateText(_setupPanel.transform, "HeroSecLabel", "1. CHỌN 1 TRONG 3 NHÀ MẠO HIỂM GIA (Chỉ 1 nhân vật xuất hiện chiến đấu trong màn chơi):",
                new Vector2(0f, 415f), new Vector2(1600f, 36f), 22, new Color(0.8f, 0.92f, 1f), FontStyle.Bold);

            for (int i = 0; i < 3; i++)
            {
                HeroDefinition h = GameDatabase.Heroes[i];
                float xPos = (i - 1) * 560f;
                GameObject card = CreatePanel(_setupPanel.transform, $"HeroCard_{h.CodeName}", new Vector2(0.5f, 0.5f), new Vector2(525f, 325f), new Color(0.13f, 0.16f, 0.24f, 0.98f));
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 215f);
                _heroCardBorders[i] = card.GetComponent<Image>();
                AddOutline(card, new Color(0.4f, 0.5f, 0.7f, 0.8f), 2f);

                // Animated Hero Preview
                GameObject iconBg = CreatePanel(card.transform, "PortraitBG", new Vector2(0.5f, 0.5f), new Vector2(115f, 125f), new Color(0.08f, 0.1f, 0.15f, 1f));
                iconBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(-180f, 75f);
                GameObject portraitObj = CreateImage(iconBg.transform, "Portrait", h.PortraitSprite, Vector2.zero, new Vector2(96f, 112f));
                SpriteAnimator anim = portraitObj.AddComponent<SpriteAnimator>();
                anim.Initialize(h.AssetFolder, 64f, 8f, true);

                // Weapon icon
                WeaponDefinition mainW = GameDatabase.Weapons[h.StartingWeaponId];
                CreateImage(card.transform, "WeaponIcon", mainW.IconSprite, new Vector2(-180f, -45f), new Vector2(62f, 62f));
                CreateText(card.transform, "WeaponLbl", mainW.DisplayName, new Vector2(-180f, -100f), new Vector2(140f, 30f), 15, new Color(1f, 0.85f, 0.35f), FontStyle.Bold);

                // Stats & Info
                CreateText(card.transform, "HeroName", $"{h.DisplayName}\n<size=18><color=#7FD8FF>{h.RoleTitle}</color></size>",
                    new Vector2(55f, 112f), new Vector2(320f, 56f), 24, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);

                string statsStr = $"• HP: <b>{h.BaseHP}</b> (+{h.HPPerLevel}/lvl)\n" +
                                  $"• {h.AttackStatLabel}: <b>{h.BaseAttack}</b> (+{h.AttackPerLevel}/lvl)\n" +
                                  $"• Endurance: <b>{h.BaseEndurance}</b> (+{h.EndurancePerLevel}/lvl) | Speed: <b>{h.BaseSpeed}</b>";
                CreateText(card.transform, "HeroStats", statsStr,
                    new Vector2(55f, 28f), new Vector2(325f, 85f), 17, new Color(0.9f, 0.95f, 0.9f), FontStyle.Normal, TextAnchor.MiddleLeft);

                CreateText(card.transform, "HeroProg", h.ProgressionSummary,
                    new Vector2(45f, -72f), new Vector2(340f, 92f), 14, new Color(0.78f, 0.84f, 0.95f), FontStyle.Normal, TextAnchor.UpperLeft);

                HeroId capturedHero = h.Id;
                Button btn = card.AddComponent<Button>();
                btn.targetGraphic = _heroCardBorders[i];
                btn.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_button");
                    _manager.SelectedHero = capturedHero;
                    RefreshSetupHighlight();
                });
            }

            // Section 2: 3 Stages
            CreateText(_setupPanel.transform, "StageSecLabel", "2. CHỌN BẢN ĐỒ MÀN CHƠI (Tích lũy Kill Count để gọi Mini Boss và Final Boss):",
                new Vector2(0f, -2f), new Vector2(1600f, 36f), 22, new Color(0.8f, 0.92f, 1f), FontStyle.Bold);

            for (int i = 0; i < 3; i++)
            {
                StageDefinition s = GameDatabase.Stages[i];
                float xPos = (i - 1) * 560f;
                GameObject sCard = CreatePanel(_setupPanel.transform, $"StageCard_{s.StageNumber}", new Vector2(0.5f, 0.5f), new Vector2(525f, 290f), new Color(0.13f, 0.15f, 0.22f, 0.98f));
                sCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, -175f);
                _stageCardBorders[i] = sCard.GetComponent<Image>();
                AddOutline(sCard, new Color(0.4f, 0.5f, 0.7f, 0.8f), 2f);

                CreateImage(sCard.transform, "StagePreview", s.PreviewSprite, new Vector2(-175f, 32f), new Vector2(130f, 130f));

                MonsterDefinition miniDef = GameDatabase.Monsters[s.MiniBossId];
                MonsterDefinition finalDef = GameDatabase.Monsters[s.FinalBossId];
                CreateImage(sCard.transform, "FinalBossIcon", finalDef.PortraitSprite, new Vector2(-175f, -82f), new Vector2(78f, 70f));

                CreateText(sCard.transform, "StageTitle", $"{s.Title} ({s.ChapterCode})",
                    new Vector2(55f, 102f), new Vector2(325f, 38f), 22, new Color(1f, 0.86f, 0.3f), FontStyle.Bold, TextAnchor.MiddleLeft);

                string stageInfo = $"{s.LoreDescription}\n\n" +
                                   $"<color=#7FFFD4>• Mini Boss ({s.MiniBossKillsRequired} Kills):</color> {miniDef.DisplayName}\n" +
                                   $"<color=#FF8080>• Final Boss ({s.FinalBossKillsRequired} Kills):</color> {finalDef.DisplayName} (HP: {finalDef.MaxHP})";
                CreateText(sCard.transform, "StageInfo", stageInfo,
                    new Vector2(55f, -20f), new Vector2(325f, 195f), 16, new Color(0.9f, 0.92f, 0.96f), FontStyle.Normal, TextAnchor.UpperLeft);

                int capturedStage = s.StageNumber;
                Button sBtn = sCard.AddComponent<Button>();
                sBtn.targetGraphic = _stageCardBorders[i];
                sBtn.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_button");
                    _manager.SelectedStageNumber = capturedStage;
                    RefreshSetupHighlight();
                });
            }

            _setupSummaryText = CreateText(_setupPanel.transform, "SetupSummary", "",
                new Vector2(0f, -365f), new Vector2(1400f, 36f), 21, new Color(0.6f, 1f, 0.7f), FontStyle.Bold);

            CreateMenuButton(_setupPanel.transform, "BtnBackMenu", "QUAY LẠI MENU CHÍNH", new Vector2(-480f, -445f), new Vector2(340f, 62f),
                new Color(0.32f, 0.36f, 0.48f), () => _manager.OpenMainMenu());

            CreateMenuButton(_setupPanel.transform, "BtnQuickDemoUnlock", "[F1] MỞ KHÓA 3 MÀN & +5000 GOLD", new Vector2(-60f, -445f), new Vector2(420f, 62f),
                new Color(0.22f, 0.52f, 0.68f), () => _manager.UnlockAllAndGrantDemoResources());

            CreateMenuButton(_setupPanel.transform, "BtnStartBattle", "XUẤT TRẬN CHIẾN ĐẤU (START GAME)", new Vector2(410f, -445f), new Vector2(440f, 68f),
                new Color(0.92f, 0.45f, 0.12f), () => _manager.StartStageBattle(_manager.SelectedHero, _manager.SelectedStageNumber));
        }

        private void RefreshSetupHighlight()
        {
            for (int i = 0; i < 3; i++)
            {
                bool heroSel = ((int)_manager.SelectedHero == i);
                if (_heroCardBorders[i] != null)
                {
                    _heroCardBorders[i].color = heroSel
                        ? new Color(0.22f, 0.36f, 0.56f, 1f)
                        : new Color(0.12f, 0.14f, 0.20f, 0.95f);
                }

                bool stageSel = (_manager.SelectedStageNumber == i + 1);
                if (_stageCardBorders[i] != null)
                {
                    _stageCardBorders[i].color = stageSel
                        ? new Color(0.38f, 0.26f, 0.52f, 1f)
                        : new Color(0.12f, 0.14f, 0.20f, 0.95f);
                }
            }

            HeroDefinition h = GameDatabase.GetHero(_manager.SelectedHero);
            StageDefinition s = GameDatabase.GetStage(_manager.SelectedStageNumber);
            WeaponDefinition w = GameDatabase.Weapons[h.StartingWeaponId];
            if (_setupSummaryText != null)
            {
                _setupSummaryText.text = $"Đang chọn: Nhân vật [{h.DisplayName}] - Vũ khí chính [{w.DisplayName}]  |  Bản đồ [{s.Title}] (Mốc thắng: {s.FinalBossKillsRequired} Kills + Hạ Final Boss)";
            }
        }

        // ==================== 3. MERCHANT SHOP PANEL (Phần 4 Cốt truyện) ====================
        private void BuildMerchantShopPanel()
        {
            _merchantPanel = CreateFullScreenPanel("MerchantShopPanel", new Color(0.06f, 0.07f, 0.12f, 0.96f));
            GameObject box = CreatePanel(_merchantPanel.transform, "ShopBox", new Vector2(0.5f, 0.5f), new Vector2(1180f, 760f), new Color(0.12f, 0.14f, 0.22f, 0.98f));
            AddOutline(box, new Color(0.95f, 0.78f, 0.25f, 0.9f), 3f);

            CreateText(box.transform, "ShopTitle", "THƯƠNG GIA XỨ ASTRAL (NPC MERCHANT)",
                new Vector2(0f, 320f), new Vector2(960f, 50f), 36, new Color(1f, 0.85f, 0.28f), FontStyle.Bold);

            // Animated Merchant NPC from Spine asset
            GameObject npcCard = CreatePanel(box.transform, "MerchantAvatarBox", new Vector2(0.5f, 0.5f), new Vector2(180f, 180f), new Color(0.08f, 0.1f, 0.15f, 1f));
            npcCard.GetComponent<RectTransform>().anchoredPosition = new Vector2(-420f, 145f);
            GameObject npcImg = CreateImage(npcCard.transform, "MerchantSprite", "Sprites/npc_merchant", Vector2.zero, new Vector2(140f, 140f));
            SpriteAnimator mAnim = npcImg.AddComponent<SpriteAnimator>();
            mAnim.Initialize("merchant", 64f, 8f, true);

            CreateText(box.transform, "ShopLore",
                "\"Ta từng là một cư dân của vùng đất Astral tươi đẹp... Hãy dùng số Vàng (Gold) thu được từ việc tiêu diệt lũ quái vật để cường hóa trang bị khởi đầu cho nhóm mạo hiểm gia Mercy, Inirius và Eni!\"",
                new Vector2(85f, 165f), new Vector2(780f, 95f), 20, new Color(0.88f, 0.93f, 1f), FontStyle.Italic, TextAnchor.MiddleLeft);

            _merchantGoldText = CreateText(box.transform, "GoldBalance", "Vàng hiện có: 300 Gold",
                new Vector2(85f, 85f), new Vector2(520f, 42f), 26, new Color(1f, 0.88f, 0.2f), FontStyle.Bold, TextAnchor.MiddleLeft);

            string[] keys = { "GG_Shop_HP", "GG_Shop_ATK", "GG_Shop_END", "GG_Shop_SPD" };
            string[] titles = { "Giáp Sinh Mệnh (+20 HP Khởi điểm)", "Đá Cường Hóa (+3 Sức Tấn Công)", "Khiên Astral (+2 Endurance / Giáp)", "Giày Gió (+0.15 Tốc độ di chuyển)" };
            string[] icons = { "Sprites/skill_hoi_phuc", "Sprites/skill_cuong_luc", "Sprites/skill_giap_cung", "Sprites/skill_than_toc" };

            for (int i = 0; i < 4; i++)
            {
                float yPos = -10f - i * 68f;
                GameObject row = CreatePanel(box.transform, $"ShopRow_{i}", new Vector2(0.5f, 0.5f), new Vector2(1020f, 58f), new Color(0.16f, 0.19f, 0.29f, 1f));
                row.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, yPos);

                CreateImage(row.transform, "Icon", icons[i], new Vector2(-460f, 0f), new Vector2(46f, 46f));
                CreateText(row.transform, "Name", titles[i], new Vector2(-130f, 0f), new Vector2(560f, 40f), 20, Color.white, FontStyle.Bold, TextAnchor.MiddleLeft);
                _shopLevelTexts[i] = CreateText(row.transform, "Lv", "Cấp: 0", new Vector2(180f, 0f), new Vector2(160f, 40f), 20, new Color(0.55f, 0.95f, 1f), FontStyle.Bold);

                string prefKey = keys[i];
                CreateMenuButton(row.transform, "BuyBtn", "NÂNG CẤP (100 Gold)", new Vector2(380f, 0f), new Vector2(220f, 46f),
                    new Color(0.22f, 0.58f, 0.30f), () =>
                    {
                        if (_manager.TrySpendGold(100))
                        {
                            int cur = PlayerPrefs.GetInt(prefKey, 0);
                            PlayerPrefs.SetInt(prefKey, cur + 1);
                            PlayerPrefs.Save();
                            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_levelup", 0.75f);
                            RefreshMerchantShopUI();
                        }
                        else
                        {
                            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_hit", 0.5f);
                        }
                    });
            }

            CreateMenuButton(box.transform, "BtnShopGrantGold", "[F1] Nhận +5,000 Gold Thử Nghiệm", new Vector2(220f, -310f), new Vector2(380f, 58f),
                new Color(0.52f, 0.36f, 0.16f), () => _manager.UnlockAllAndGrantDemoResources());

            CreateMenuButton(box.transform, "BtnCloseShop", "QUAY LẠI MENU CHÍNH", new Vector2(-220f, -310f), new Vector2(380f, 58f),
                new Color(0.35f, 0.40f, 0.52f), () => _manager.OpenMainMenu());
        }

        private void RefreshMerchantShopUI()
        {
            if (_merchantGoldText != null)
            {
                _merchantGoldText.text = $"Vàng tích lũy hiện có: {_manager.TotalSavedGold} Gold";
            }
            string[] keys = { "GG_Shop_HP", "GG_Shop_ATK", "GG_Shop_END", "GG_Shop_SPD" };
            for (int i = 0; i < 4; i++)
            {
                if (_shopLevelTexts[i] != null)
                {
                    _shopLevelTexts[i].text = $"Cấp: +{PlayerPrefs.GetInt(keys[i], 0)}";
                }
            }
        }

        // ==================== 4. GAMEPLAY HUD PANEL (Phần 3.5) ====================
        private void BuildGameplayHudPanel()
        {
            _hudPanel = CreateFullScreenPanel("GameplayHUDPanel", new Color(0f, 0f, 0f, 0f));
            _hudPanel.GetComponent<Image>().raycastTarget = false;

            // --- TOP-LEFT: HP Bar, EXP Bar, Level & Character Portrait ---
            GameObject topLeftBox = CreatePanel(_hudPanel.transform, "TopLeftStatusBox", new Vector2(0f, 1f), new Vector2(560f, 178f), new Color(0.08f, 0.10f, 0.16f, 0.88f));
            RectTransform tlRt = topLeftBox.GetComponent<RectTransform>();
            tlRt.pivot = new Vector2(0f, 1f);
            tlRt.anchoredPosition = new Vector2(20f, -20f);
            AddOutline(topLeftBox, new Color(0.4f, 0.5f, 0.7f, 0.8f), 2f);

            GameObject portFrame = CreatePanel(topLeftBox.transform, "PortraitFrame", new Vector2(0f, 0.5f), new Vector2(96f, 124f), new Color(0.14f, 0.17f, 0.26f, 1f));
            portFrame.GetComponent<RectTransform>().anchoredPosition = new Vector2(62f, 10f);
            _hudHeroPortrait = CreateImage(portFrame.transform, "HeroPortrait", "Sprites/hero_mercy", Vector2.zero, new Vector2(84f, 108f)).GetComponent<Image>();

            _hudLevelText = CreateText(topLeftBox.transform, "LevelText", "LV. 1 - Mercy",
                new Vector2(315f, 64f), new Vector2(395f, 30f), 21, new Color(1f, 0.88f, 0.25f), FontStyle.Bold, TextAnchor.MiddleLeft);

            // HP Bar
            GameObject hpBg = CreatePanel(topLeftBox.transform, "HPBarBG", new Vector2(0.5f, 0.5f), new Vector2(400f, 28f), new Color(0.22f, 0.08f, 0.10f, 1f));
            hpBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(52f, 28f);
            GameObject hpFillObj = CreatePanel(hpBg.transform, "HPBarFill", new Vector2(0f, 0.5f), new Vector2(400f, 28f), new Color(0.88f, 0.20f, 0.24f, 1f));
            RectTransform hpFillRt = hpFillObj.GetComponent<RectTransform>();
            hpFillRt.pivot = new Vector2(0f, 0.5f);
            hpFillRt.anchoredPosition = Vector2.zero;
            _hudHpFill = hpFillObj.GetComponent<Image>();
            _hudHpText = CreateText(hpBg.transform, "HPText", "HP: 150 / 150", Vector2.zero, new Vector2(390f, 26f), 16, Color.white, FontStyle.Bold);

            // EXP Bar
            GameObject expBg = CreatePanel(topLeftBox.transform, "EXPBarBG", new Vector2(0.5f, 0.5f), new Vector2(400f, 22f), new Color(0.08f, 0.16f, 0.28f, 1f));
            expBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(52f, -3f);
            GameObject expFillObj = CreatePanel(expBg.transform, "EXPBarFill", new Vector2(0f, 0.5f), new Vector2(0f, 22f), new Color(0.20f, 0.72f, 1f, 1f));
            RectTransform expFillRt = expFillObj.GetComponent<RectTransform>();
            expFillRt.pivot = new Vector2(0f, 0.5f);
            expFillRt.anchoredPosition = Vector2.zero;
            _hudExpFill = expFillObj.GetComponent<Image>();
            _hudExpText = CreateText(expBg.transform, "EXPText", "EXP: 0 / 5", Vector2.zero, new Vector2(390f, 22f), 14, Color.white, FontStyle.Bold);

            _hudStatsText = CreateText(topLeftBox.transform, "StatsLine", "ATK: 18 | END: 12 | SPD: 4.2",
                new Vector2(52f, -34f), new Vector2(405f, 24f), 15, new Color(0.78f, 0.88f, 1f), FontStyle.Normal, TextAnchor.MiddleLeft);

            _hudDashAndBuffText = CreateText(topLeftBox.transform, "DashAndBuffLine", "[Space] Lướt: SẴN SÀNG | [T] Ngắm: TỰ ĐỘNG",
                new Vector2(0f, -66f), new Vector2(530f, 24f), 14, new Color(0.55f, 1f, 0.75f), FontStyle.Bold, TextAnchor.MiddleLeft);

            // --- TOP-RIGHT: Kill Counter, Radar & Progress ---
            GameObject topRightBox = CreatePanel(_hudPanel.transform, "TopRightKillBox", new Vector2(1f, 1f), new Vector2(485f, 168f), new Color(0.08f, 0.10f, 0.16f, 0.88f));
            RectTransform trRt = topRightBox.GetComponent<RectTransform>();
            trRt.pivot = new Vector2(1f, 1f);
            trRt.anchoredPosition = new Vector2(-20f, -20f);
            AddOutline(topRightBox, new Color(0.85f, 0.45f, 0.25f, 0.85f), 2f);

            _hudKillCounterText = CreateText(topRightBox.transform, "KillCountText", "QUÁI TIÊU DIỆT (KILLS): 0 / 250",
                new Vector2(0f, 54f), new Vector2(460f, 32f), 22, new Color(1f, 0.55f, 0.35f), FontStyle.Bold);

            _hudBossProgressText = CreateText(topRightBox.transform, "BossThresholdText", "Mini Boss: 100 Kills | Final Boss: 250 Kills",
                new Vector2(0f, 22f), new Vector2(460f, 26f), 15, new Color(0.95f, 0.90f, 0.65f), FontStyle.Normal);

            _hudRadarTrackerText = CreateText(topRightBox.transform, "RadarTrackerText", "Radar mục tiêu: Đang quét khu vực...",
                new Vector2(0f, -6f), new Vector2(460f, 24f), 14, new Color(0.55f, 0.92f, 1f), FontStyle.Bold);

            _hudGoldAndTimeText = CreateText(topRightBox.transform, "GoldAndTimeText", "Vàng: 0 Gold | Thời gian: 00:00",
                new Vector2(-70f, -48f), new Vector2(310f, 28f), 16, new Color(0.8f, 0.95f, 0.85f), FontStyle.Bold);

            CreateMenuButton(topRightBox.transform, "BtnHudPause", "[R / Esc] Menu", new Vector2(160f, -48f), new Vector2(135f, 38f),
                new Color(0.28f, 0.34f, 0.48f), () => _manager.TogglePauseMenu(true));

            // --- TOP-CENTER: Boss Health Bar + Event Banner ---
            _hudBossBarObj = CreatePanel(_hudPanel.transform, "BossHPBarPanel", new Vector2(0.5f, 1f), new Vector2(720f, 78f), new Color(0.16f, 0.06f, 0.08f, 0.92f));
            RectTransform bossRt = _hudBossBarObj.GetComponent<RectTransform>();
            bossRt.pivot = new Vector2(0.5f, 1f);
            bossRt.anchoredPosition = new Vector2(0f, -20f);
            AddOutline(_hudBossBarObj, new Color(1f, 0.25f, 0.25f, 0.9f), 2f);

            _hudBossNameText = CreateText(_hudBossBarObj.transform, "BossName", "FINAL BOSS: GOLEM",
                new Vector2(0f, 18f), new Vector2(690f, 28f), 20, new Color(1f, 0.82f, 0.28f), FontStyle.Bold);

            GameObject bHpBg = CreatePanel(_hudBossBarObj.transform, "BossHPBG", new Vector2(0.5f, 0.5f), new Vector2(680f, 24f), new Color(0.08f, 0.04f, 0.05f, 1f));
            bHpBg.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -14f);
            GameObject bHpFillObj = CreatePanel(bHpBg.transform, "BossHPFill", new Vector2(0f, 0.5f), new Vector2(680f, 24f), new Color(0.95f, 0.18f, 0.22f, 1f));
            RectTransform bFillRt = bHpFillObj.GetComponent<RectTransform>();
            bFillRt.pivot = new Vector2(0f, 0.5f);
            bFillRt.anchoredPosition = Vector2.zero;
            _hudBossHpFill = bHpFillObj.GetComponent<Image>();
            _hudBossHpValueText = CreateText(bHpBg.transform, "BossHPVal", "3500 / 3500", Vector2.zero, new Vector2(660f, 24f), 15, Color.white, FontStyle.Bold);

            _bannerObj = CreatePanel(_hudPanel.transform, "EventBanner", new Vector2(0.5f, 1f), new Vector2(760f, 74f), new Color(0.10f, 0.12f, 0.20f, 0.92f));
            _bannerObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -145f);
            AddOutline(_bannerObj, new Color(1f, 0.82f, 0.25f, 0.9f), 2f);
            _bannerText = CreateText(_bannerObj.transform, "BannerText", "", Vector2.zero, new Vector2(730f, 66f), 19, new Color(1f, 0.92f, 0.45f), FontStyle.Bold);
            _bannerObj.SetActive(false);

            // --- BOTTOM: Weapon Slots & Passive Skill Slots (Ô Trang Bị) ---
            GameObject bottomEquipBar = CreatePanel(_hudPanel.transform, "BottomEquipmentBar", new Vector2(0.5f, 0f), new Vector2(1140f, 114f), new Color(0.08f, 0.10f, 0.16f, 0.88f));
            RectTransform botRt = bottomEquipBar.GetComponent<RectTransform>();
            botRt.pivot = new Vector2(0.5f, 0f);
            botRt.anchoredPosition = new Vector2(0f, 16f);
            AddOutline(bottomEquipBar, new Color(0.45f, 0.55f, 0.75f, 0.85f), 2f);

            CreateText(bottomEquipBar.transform, "WeaponSlotsTitle", "Ô TRANG BỊ VŨ KHÍ (CÔNG THỨC SÁT THƯƠNG GDD):",
                new Vector2(-265f, 40f), new Vector2(540f, 24f), 15, new Color(1f, 0.86f, 0.35f), FontStyle.Bold, TextAnchor.MiddleLeft);

            CreateText(bottomEquipBar.transform, "PassiveSlotsTitle", "KỸ NĂNG NỘI TẠI (PASSIVE SKILLS):",
                new Vector2(290f, 40f), new Vector2(460f, 24f), 15, new Color(0.55f, 0.92f, 1f), FontStyle.Bold, TextAnchor.MiddleLeft);

            GameObject wContainer = new GameObject("WeaponSlotsContainer", typeof(RectTransform));
            wContainer.transform.SetParent(bottomEquipBar.transform, false);
            wContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(-275f, -10f);
            _weaponSlotsContainer = wContainer.transform;

            GameObject pContainer = new GameObject("PassiveSlotsContainer", typeof(RectTransform));
            pContainer.transform.SetParent(bottomEquipBar.transform, false);
            pContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(285f, -10f);
            _passiveSlotsContainer = pContainer.transform;
        }

        // ==================== 5. LEVEL UP CARD SELECTION PANEL (Hình 3.3) ====================
        private void BuildLevelUpPanel()
        {
            _levelUpPanel = CreateFullScreenPanel("LevelUpSelectionPanel", new Color(0.03f, 0.04f, 0.08f, 0.84f));

            _levelUpHeaderText = CreateText(_levelUpPanel.transform, "LevelUpTitle", "THĂNG CẤP! CHỌN 1 THẺ NÂNG CẤP VŨ KHÍ / KỸ NĂNG",
                new Vector2(0f, 340f), new Vector2(1400f, 58f), 40, new Color(1f, 0.88f, 0.25f), FontStyle.Bold);

            CreateText(_levelUpPanel.transform, "LevelUpHint", "Nhấn Chuột Trái vào thẻ bài hoặc bấm phím số [1], [2], [3] trên bàn phím",
                new Vector2(0f, 285f), new Vector2(1100f, 34f), 20, new Color(0.8f, 0.9f, 1f), FontStyle.Italic);

            for (int i = 0; i < 3; i++)
            {
                float xPos = (i - 1) * 430f;
                // Styled directly after Hình 3.3 (image36.png)
                GameObject card = CreatePanel(_levelUpPanel.transform, $"UpgradeCard_{i + 1}", new Vector2(0.5f, 0.5f), new Vector2(390f, 520f), new Color(0.24f, 0.27f, 0.36f, 0.99f));
                card.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, -25f);
                AddOutline(card, new Color(0.85f, 0.9f, 1f, 0.95f), 3f);

                // Top shield badge [1], [2], [3]
                GameObject badge = CreatePanel(card.transform, "KeyBadge", new Vector2(0.5f, 0.5f), new Vector2(68f, 44f), new Color(0.12f, 0.14f, 0.20f, 1f));
                badge.GetComponent<RectTransform>().anchoredPosition = new Vector2(-140f, 222f);
                CreateText(badge.transform, "KeyTxt", $"[{i + 1}]", Vector2.zero, new Vector2(64f, 40f), 22, new Color(1f, 0.88f, 0.3f), FontStyle.Bold);

                // Black square icon box
                GameObject iconBox = CreatePanel(card.transform, "IconBlackBox", new Vector2(0.5f, 0.5f), new Vector2(145f, 145f), new Color(0.05f, 0.06f, 0.08f, 1f));
                iconBox.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 140f);
                AddOutline(iconBox, new Color(0.5f, 0.55f, 0.7f, 0.8f), 2f);

                _upgradeCardIcons[i] = CreateImage(iconBox.transform, "UpgradeIcon", "Sprites/weapon_00", Vector2.zero, new Vector2(110f, 110f)).GetComponent<Image>();

                // Colored ribbon banner (Pink / Blue / Silver matching Hình 3.3)
                GameObject ribbon = CreatePanel(card.transform, "RibbonBar", new Vector2(0.5f, 0.5f), new Vector2(360f, 58f), new Color(1f, 0.22f, 0.45f, 1f));
                ribbon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 18f);
                _upgradeCardHeaderBars[i] = ribbon.GetComponent<Image>();

                _upgradeCardTitles[i] = CreateText(ribbon.transform, "CardTitle", "Kiếm Ánh Sáng (Lv.2)",
                    Vector2.zero, new Vector2(340f, 52f), 23, Color.white, FontStyle.Bold);

                _upgradeCardSubtitles[i] = CreateText(card.transform, "CardSubtitle", "Nâng cấp Vũ khí Chính",
                    new Vector2(0f, -32f), new Vector2(340f, 30f), 17, new Color(0.95f, 0.88f, 0.55f), FontStyle.Bold);

                _upgradeCardDescs[i] = CreateText(card.transform, "CardDesc", "",
                    new Vector2(0f, -135f), new Vector2(340f, 165f), 18, new Color(0.92f, 0.95f, 1f), FontStyle.Normal);

                int cardIndex = i;
                Button btn = card.AddComponent<Button>();
                btn.targetGraphic = card.GetComponent<Image>();
                btn.onClick.AddListener(() => _manager.SelectUpgradeCard(cardIndex));
                _upgradeCardButtons[i] = btn;
            }
        }

        public void ShowLevelUpCards(List<UpgradeOption> options)
        {
            RefreshStateUI();
            if (PlayerController.Instance != null && _levelUpHeaderText != null)
            {
                _levelUpHeaderText.text = $"LÊN CẤP LEVEL {PlayerController.Instance.CurrentLevel}! CHỌN 1 NÂNG CẤP VŨ KHÍ / KỸ NĂNG";
            }

            Color[] ribbonColors = new Color[]
            {
                new Color(0.98f, 0.22f, 0.45f, 1f), // Pink (Hình 3.3 left card)
                new Color(0.05f, 0.68f, 0.98f, 1f), // Cyan/Blue (Hình 3.3 middle card)
                new Color(0.38f, 0.44f, 0.62f, 1f)  // Silver/Slate (Hình 3.3 right card)
            };

            for (int i = 0; i < 3; i++)
            {
                if (i >= options.Count) continue;
                UpgradeOption opt = options[i];
                _upgradeCardIcons[i].sprite = ResourceCache.GetSprite(opt.IconPath, 64f);
                _upgradeCardHeaderBars[i].color = ribbonColors[Mathf.Clamp(opt.CardStyleIndex, 0, 2)];
                _upgradeCardTitles[i].text = opt.Title;
                _upgradeCardSubtitles[i].text = opt.Subtitle;
                _upgradeCardDescs[i].text = opt.Description;
            }
        }

        // ==================== 6. PAUSE & SETTINGS PANEL (Phím R / Esc) ====================
        private void BuildPauseSettingsPanel()
        {
            _pausePanel = CreateFullScreenPanel("PauseSettingsPanel", new Color(0.03f, 0.04f, 0.08f, 0.86f));
            GameObject box = CreatePanel(_pausePanel.transform, "PauseBox", new Vector2(0.5f, 0.5f), new Vector2(920f, 760f), new Color(0.12f, 0.15f, 0.23f, 0.98f));
            AddOutline(box, new Color(0.6f, 0.75f, 1f, 0.9f), 3f);

            CreateText(box.transform, "PauseTitle", "TẠM DỪNG & CÀI ĐẶT (PAUSE / SETTINGS)",
                new Vector2(0f, 320f), new Vector2(820f, 48f), 34, new Color(1f, 0.86f, 0.28f), FontStyle.Bold);

            // Audio volume controls
            CreateText(box.transform, "AudioLbl", "ĐIỀU CHỈNH ÂM THANH (NHẠC NỀN BGM & HIỆU ỨNG SFX):",
                new Vector2(0f, 255f), new Vector2(780f, 32f), 20, new Color(0.75f, 0.92f, 1f), FontStyle.Bold);

            CreateMenuButton(box.transform, "BgmToggle", "Nhạc nền (BGM): Bật / Chuyển mức âm lượng", new Vector2(-210f, 200f), new Vector2(390f, 48f),
                new Color(0.22f, 0.42f, 0.65f), () =>
                {
                    if (AudioManager.Instance != null)
                    {
                        float next = AudioManager.Instance.BgmVolume > 0.1f ? 0f : 0.55f;
                        AudioManager.Instance.SetBgmVolume(next);
                        ShowBanner($"Âm lượng Nhạc nền (BGM): {(next > 0f ? "BẬT" : "TẮT")}");
                    }
                });

            CreateMenuButton(box.transform, "SfxToggle", "Hiệu ứng (SFX): Bật / Chuyển mức âm lượng", new Vector2(210f, 200f), new Vector2(390f, 48f),
                new Color(0.22f, 0.42f, 0.65f), () =>
                {
                    if (AudioManager.Instance != null)
                    {
                        float next = AudioManager.Instance.SfxVolume > 0.1f ? 0f : 0.75f;
                        AudioManager.Instance.SetSfxVolume(next);
                        AudioManager.Instance.PlaySfx("sfx_button");
                    }
                });

            // Quick Demo / Grading Tools
            CreateText(box.transform, "DemoTitle", "CÔNG CỤ KIỂM THỬ & BÁO CÁO NHANH (DEMO / TEST TOOLS):",
                new Vector2(0f, 128f), new Vector2(820f, 32f), 19, new Color(0.6f, 1f, 0.72f), FontStyle.Bold);

            CreateMenuButton(box.transform, "BtnTestLvUp", "+1 Lên Cấp Ngay (Mở Thẻ Bài Nâng Cấp)", new Vector2(-210f, 72f), new Vector2(390f, 48f),
                new Color(0.52f, 0.32f, 0.68f), () =>
                {
                    _manager.TogglePauseMenu(false);
                    if (PlayerController.Instance != null)
                    {
                        int needed = GameDatabase.GetCumulativeExpThreshold(PlayerController.Instance.CurrentLevel) - PlayerController.Instance.TotalExp;
                        PlayerController.Instance.GainExp(Mathf.Max(1, needed));
                    }
                });

            CreateMenuButton(box.transform, "BtnTestAddKills", "+50 Quái Tiêu Diệt (+50 Kill Count)", new Vector2(210f, 72f), new Vector2(390f, 48f),
                new Color(0.52f, 0.32f, 0.68f), () =>
                {
                    _manager.TogglePauseMenu(false);
                    _manager.DebugAddKills(50);
                });

            CreateMenuButton(box.transform, "BtnTestMiniBoss", "Triệu Hồi Mini Boss Của Màn Ngay", new Vector2(-210f, 12f), new Vector2(390f, 48f),
                new Color(0.68f, 0.38f, 0.18f), () =>
                {
                    _manager.TogglePauseMenu(false);
                    _manager.SpawnStageMiniBoss();
                });

            CreateMenuButton(box.transform, "BtnTestFinalBoss", "Triệu Hồi Final Boss Của Màn Ngay", new Vector2(210f, 12f), new Vector2(390f, 48f),
                new Color(0.75f, 0.22f, 0.25f), () =>
                {
                    _manager.TogglePauseMenu(false);
                    _manager.SpawnStageFinalBoss();
                });

            Button godBtn = CreateMenuButton(box.transform, "BtnTestGodMode", "Hồi Đầy HP & Bật/Tắt Bất Tử (God Mode: TẮT)", new Vector2(0f, -48f), new Vector2(810f, 46f),
                new Color(0.20f, 0.55f, 0.45f), () =>
                {
                    if (PlayerController.Instance != null)
                    {
                        PlayerController.Instance.Heal(9999f, false);
                        PlayerController.Instance.CleanseStatusEffects();
                        PlayerController.Instance.GodMode = !PlayerController.Instance.GodMode;
                        if (_pauseGodModeBtnText != null)
                        {
                            _pauseGodModeBtnText.text = $"Hồi Đầy HP & Bật/Tắt Bất Tử (God Mode: {(PlayerController.Instance.GodMode ? "ĐANG BẬT" : "TẮT")})";
                        }
                    }
                });
            _pauseGodModeBtnText = godBtn.GetComponentInChildren<Text>();

            // Standard Pause Actions
            CreateMenuButton(box.transform, "BtnResume", "TIẾP TỤC CHIẾN ĐẤU (RESUME - PHÍM R / ESC)", new Vector2(0f, -145f), new Vector2(560f, 60f),
                new Color(0.20f, 0.65f, 0.32f), () => _manager.TogglePauseMenu(false));

            CreateMenuButton(box.transform, "BtnRetryStage", "CHƠI LẠI MÀN NÀY (RETRY)", new Vector2(0f, -218f), new Vector2(560f, 54f),
                new Color(0.82f, 0.48f, 0.15f), () => _manager.StartStageBattle(_manager.SelectedHero, _manager.SelectedStageNumber));

            CreateMenuButton(box.transform, "BtnPauseToMenu", "THOÁT VỀ MENU CHÍNH (MAIN MENU)", new Vector2(0f, -288f), new Vector2(560f, 54f),
                new Color(0.55f, 0.22f, 0.26f), () => _manager.OpenMainMenu());
        }

        // ==================== 7. VICTORY & DEFEAT PANELS (Hình 3.7 bg_win & Hình 3.8 bg_defeat) ====================
        private void BuildVictoryPanel()
        {
            _victoryPanel = CreateFullScreenPanel("VictoryPanel", new Color(0f, 0f, 0f, 0.92f));
            CreateImage(_victoryPanel.transform, "BgWin", "Sprites/bg_win", Vector2.zero, new Vector2(1920f, 1080f));

            GameObject overlay = CreatePanel(_victoryPanel.transform, "WinOverlayBox", new Vector2(0.5f, 0.5f), new Vector2(1020f, 640f), new Color(0.07f, 0.12f, 0.18f, 0.88f));
            AddOutline(overlay, new Color(1f, 0.84f, 0.25f, 0.95f), 3f);

            _victoryTitleText = CreateText(overlay.transform, "WinTitle", "CHIẾN THẮNG MÀN CHƠI (STAGE CLEAR!)",
                new Vector2(0f, 235f), new Vector2(960f, 64f), 46, new Color(1f, 0.88f, 0.25f), FontStyle.Bold);

            _victoryDetailText = CreateText(overlay.transform, "WinDetail", "",
                new Vector2(0f, 35f), new Vector2(920f, 270f), 22, Color.white, FontStyle.Normal);

            Button nextBtn = CreateMenuButton(overlay.transform, "BtnNextStage", "TIẾN VÀO MÀN TIẾP THEO (NEXT STAGE)", new Vector2(210f, -195f), new Vector2(420f, 64f),
                new Color(0.18f, 0.68f, 0.34f), () =>
                {
                    int nextStage = Mathf.Clamp(_manager.SelectedStageNumber + 1, 1, 3);
                    _manager.StartStageBattle(_manager.SelectedHero, nextStage);
                });
            _victoryNextStageBtnObj = nextBtn.gameObject;

            CreateMenuButton(overlay.transform, "BtnWinMainMenu", "VỀ MENU CHÍNH (MAIN MENU)", new Vector2(-210f, -195f), new Vector2(360f, 64f),
                new Color(0.28f, 0.42f, 0.65f), () => _manager.OpenMainMenu());
        }

        private void BuildDefeatPanel()
        {
            _defeatPanel = CreateFullScreenPanel("DefeatPanel", new Color(0f, 0f, 0f, 0.92f));
            CreateImage(_defeatPanel.transform, "BgDefeat", "Sprites/bg_defeat", Vector2.zero, new Vector2(1920f, 1080f));

            GameObject overlay = CreatePanel(_defeatPanel.transform, "DefeatOverlayBox", new Vector2(0.5f, 0.5f), new Vector2(940f, 560f), new Color(0.14f, 0.06f, 0.08f, 0.90f));
            AddOutline(overlay, new Color(0.95f, 0.25f, 0.25f, 0.95f), 3f);

            CreateText(overlay.transform, "DefeatTitle", "THẤT BẠI (GAME OVER)",
                new Vector2(0f, 195f), new Vector2(860f, 64f), 48, new Color(1f, 0.30f, 0.30f), FontStyle.Bold);

            _defeatDetailText = CreateText(overlay.transform, "DefeatDetail", "",
                new Vector2(0f, 25f), new Vector2(860f, 210f), 22, new Color(0.95f, 0.9f, 0.9f), FontStyle.Normal);

            CreateMenuButton(overlay.transform, "BtnRetry", "CHƠI LẠI MÀN NÀY (RETRY)", new Vector2(195f, -175f), new Vector2(360f, 64f),
                new Color(0.88f, 0.45f, 0.15f), () => _manager.StartStageBattle(_manager.SelectedHero, _manager.SelectedStageNumber));

            CreateMenuButton(overlay.transform, "BtnDefeatMenu", "VỀ MENU CHÍNH (MAIN MENU)", new Vector2(-195f, -175f), new Vector2(360f, 64f),
                new Color(0.36f, 0.38f, 0.48f), () => _manager.OpenMainMenu());
        }

        // ==================== UI UPDATE & REFRESH ====================
        public void RefreshStateUI()
        {
            if (_manager == null) return;
            GameFlowState st = _manager.State;

            _mainMenuPanel.SetActive(st == GameFlowState.MainMenu);
            _setupPanel.SetActive(st == GameFlowState.PreGameSetup);
            _merchantPanel.SetActive(st == GameFlowState.MerchantShop);

            bool showHud = (st == GameFlowState.Playing || st == GameFlowState.LevelUpSelection || st == GameFlowState.Paused);
            _hudPanel.SetActive(showHud);
            _levelUpPanel.SetActive(st == GameFlowState.LevelUpSelection);
            _pausePanel.SetActive(st == GameFlowState.Paused);
            _victoryPanel.SetActive(st == GameFlowState.Victory);
            _defeatPanel.SetActive(st == GameFlowState.Defeat);

            if (st == GameFlowState.PreGameSetup)
            {
                RefreshSetupHighlight();
            }
            else if (st == GameFlowState.MerchantShop)
            {
                RefreshMerchantShopUI();
            }
            else if (st == GameFlowState.Victory)
            {
                PopulateVictoryScreen();
            }
            else if (st == GameFlowState.Defeat)
            {
                PopulateDefeatScreen();
            }
            _lastEquipmentHash = -1;
        }

        private void PopulateVictoryScreen()
        {
            StageDefinition stg = _manager.CurrentStage ?? GameDatabase.GetStage(1);
            PlayerController p = PlayerController.Instance;
            int lv = p != null ? p.CurrentLevel : 1;

            if (_manager.SelectedStageNumber >= 3)
            {
                _victoryTitleText.text = "CHIẾN THẮNG TOÀN BỘ GAME GRIND AND GLORY!";
                _victoryDetailText.text =
                    "Chúc mừng nhà mạo hiểm gia! Bạn đã đánh bại Ác quỷ Địa ngục <b>Herald of Death</b> và đóng cánh cổng dẫn tới Địa ngục!\n" +
                    "Không còn nguồn năng lượng hắc ám, đám ma quỷ tan biến và vùng đất <b>Astral</b> đã lấy lại sự bình yên vốn có.\n\n" +
                    $"<color=#FFE066>• Nhân vật vinh danh:</color> {GameDatabase.GetHero(_manager.SelectedHero).DisplayName} (Đạt Level {lv})\n" +
                    $"<color=#7FFFD4>• Tổng số quái tiêu diệt:</color> {_manager.CurrentKills} Kills  |  <color=#FFD700>Vàng thưởng:</color> +{_manager.SessionGold} Gold";
                if (_victoryNextStageBtnObj != null) _victoryNextStageBtnObj.SetActive(false);
            }
            else
            {
                _victoryTitleText.text = $"CHIẾN THẮNG {stg.Title.ToUpper()}!";
                _victoryDetailText.text =
                    $"Bạn đã vượt qua thử thách tại <b>{stg.Title}</b> và tiêu diệt thành công Final Boss <b>{GameDatabase.Monsters[stg.FinalBossId].DisplayName}</b>!\n\n" +
                    $"<color=#FFE066>• Nhân vật:</color> {GameDatabase.GetHero(_manager.SelectedHero).DisplayName} (Level {lv})\n" +
                    $"<color=#7FFFD4>• Quái tiêu diệt:</color> {_manager.CurrentKills} Kills\n" +
                    $"<color=#FFD700>• Vàng thu thập trong màn:</color> +{_manager.SessionGold} Gold";
                if (_victoryNextStageBtnObj != null) _victoryNextStageBtnObj.SetActive(true);
            }
        }

        private void PopulateDefeatScreen()
        {
            StageDefinition stg = _manager.CurrentStage ?? GameDatabase.GetStage(1);
            PlayerController p = PlayerController.Instance;
            int lv = p != null ? p.CurrentLevel : 1;

            _defeatDetailText.text =
                $"Nhà mạo hiểm gia đã kiệt sức tại <b>{stg.Title}</b> khi lượng máu (HP) giảm về 0.\n" +
                $"Hãy ghé thăm Thương gia xứ Astral hoặc thử lại với chiến thuật nâng cấp Vũ khí / Kỹ năng tối ưu hơn!\n\n" +
                $"• Cấp độ đạt được: Level {lv}  |  Số quái tiêu diệt: {_manager.CurrentKills} / {stg.FinalBossKillsRequired} Kills\n" +
                $"• Vàng thu được đã lưu: +{_manager.SessionGold} Gold";
        }

        public void ShowBanner(string message)
        {
            if (_bannerObj == null || _bannerText == null) return;
            _bannerText.text = message;
            _bannerObj.SetActive(true);
            _bannerTimer = 4.2f;
        }

        private void Update()
        {
            if (_bannerTimer > 0f)
            {
                _bannerTimer -= Time.unscaledDeltaTime;
                if (_bannerTimer <= 0f && _bannerObj != null)
                {
                    _bannerObj.SetActive(false);
                }
            }

            if (_manager == null || !_hudPanel.activeSelf) return;
            UpdateLiveGameplayHud();
        }

        private void UpdateLiveGameplayHud()
        {
            PlayerController p = PlayerController.Instance;
            StageDefinition stg = _manager.CurrentStage;
            if (p == null || stg == null) return;

            _hudHeroPortrait.sprite = ResourceCache.GetSprite(p.HeroDef.PortraitSprite, 64f);
            _hudLevelText.text = $"LV. {p.CurrentLevel} — {p.HeroDef.CodeName} ({p.HeroDef.RoleTitle})";

            float hpRatio = Mathf.Clamp01(p.CurrentHP / Mathf.Max(1f, p.MaxHP));
            _hudHpFill.rectTransform.sizeDelta = new Vector2(400f * hpRatio, 28f);
            _hudHpText.text = $"HP: {Mathf.CeilToInt(p.CurrentHP)} / {Mathf.CeilToInt(p.MaxHP)}";

            int prevExp = GameDatabase.GetCumulativeExpThreshold(p.CurrentLevel - 1);
            int nextExp = GameDatabase.GetCumulativeExpThreshold(p.CurrentLevel);
            int curInLv = Mathf.Max(0, p.TotalExp - prevExp);
            int reqInLv = Mathf.Max(1, nextExp - prevExp);
            float expRatio = Mathf.Clamp01((float)curInLv / reqInLv);
            _hudExpFill.rectTransform.sizeDelta = new Vector2(400f * expRatio, 22f);
            _hudExpText.text = $"EXP: {p.TotalExp} / {nextExp} (+{reqInLv - curInLv} lên Lv.{p.CurrentLevel + 1})";

            _hudStatsText.text = $"{p.HeroDef.AttackStatLabel}: {p.BaseAttackStat:0.#} | END: {p.GetEffectiveEndurance():0.#} | SPD: {p.GetEffectiveMoveSpeed():F1}";

            string dashStatus = p.DashCooldownRemaining <= 0.01f
                ? "<color=#66FF99>[Space] Lướt: SẴN SÀNG</color>"
                : $"<color=#FFD166>[Space] Lướt: {p.DashCooldownRemaining:F1}s</color>";
            string aimStatus = p.AutoAimEnabled
                ? "<color=#7FD8FF>[T] Ngắm: TỰ ĐỘNG</color>"
                : "<color=#CCCCCC>[T] Ngắm: HƯỚNG ĐI</color>";
            string buffs = "";
            if (p.GodMode) buffs += " <color=#FFE066>[BẤT TỬ]</color>";
            if (p.EggSpeedBuffRemaining > 0f) buffs += $" <color=#66FFFF>[TĂNG TỐC {p.EggSpeedBuffRemaining:F1}s]</color>";
            if (p.PlayerBurnRemaining > 0f) buffs += " <color=#FF8844>[BỎNG]</color>";
            if (p.PlayerBleedRemaining > 0f) buffs += " <color=#FF4466>[CHẢY MÁU]</color>";
            _hudDashAndBuffText.text = $"{dashStatus}  |  {aimStatus}{buffs}";

            _hudKillCounterText.text = $"QUÁI TIÊU DIỆT: {_manager.CurrentKills} / {stg.FinalBossKillsRequired}";
            string miniStatus = _manager.MiniBossDefeated ? "<color=#66FF88>Đã hạ Mini Boss</color>" : $"Mini Boss: {_manager.CurrentKills}/{stg.MiniBossKillsRequired}";
            string finalStatus = _manager.FinalBossSpawned ? "<color=#FF6666>ĐANG ĐẤU FINAL BOSS!</color>" : $"Final Boss: {_manager.CurrentKills}/{stg.FinalBossKillsRequired}";
            _hudBossProgressText.text = $"{miniStatus}  •  {finalStatus}";

            _hudRadarTrackerText.text = BuildRadarStatusString(p);

            int mins = Mathf.FloorToInt(_manager.StageElapsedTime / 60f);
            int secs = Mathf.FloorToInt(_manager.StageElapsedTime % 60f);
            _hudGoldAndTimeText.text = $"Vàng: {_manager.TotalSavedGold}G  |  {mins:00}:{secs:00}";

            // Boss Bar
            if (_manager.ActiveBoss != null && !_manager.ActiveBoss.IsDead)
            {
                _hudBossBarObj.SetActive(true);
                EnemyController boss = _manager.ActiveBoss;
                string tag = boss.Definition.Category == MonsterCategory.FinalBoss ? "FINAL BOSS" : "MINI BOSS";
                _hudBossNameText.text = $"{tag}: {boss.Definition.DisplayName.ToUpper()}";
                float bRatio = Mathf.Clamp01(boss.CurrentHP / Mathf.Max(1f, boss.MaxHP));
                _hudBossHpFill.rectTransform.sizeDelta = new Vector2(680f * bRatio, 24f);
                _hudBossHpValueText.text = $"{Mathf.CeilToInt(boss.CurrentHP)} / {Mathf.CeilToInt(boss.MaxHP)}";
            }
            else
            {
                _hudBossBarObj.SetActive(false);
            }

            // Refresh Weapon & Passive Slots when equipment or level changes
            int hash = ComputeEquipmentHash(p);
            if (hash != _lastEquipmentHash)
            {
                _lastEquipmentHash = hash;
                RebuildHudEquipmentSlots(p);
            }
        }

        private string BuildRadarStatusString(PlayerController p)
        {
            Vector2 pPos = p.transform.position;

            // 1. Check for uncollected Weapon Chest (item_box)
            for (int i = 0; i < DropItemController.ActiveDrops.Count; i++)
            {
                DropItemController d = DropItemController.ActiveDrops[i];
                if (d != null && d.ItemType == DropItemType.ItemBox)
                {
                    Vector2 diff = (Vector2)d.transform.position - pPos;
                    return $"<color=#FFE066>★ RƯƠNG VŨ KHÍ ({GetCompassArrow(diff)} {diff.magnitude:0.0}m)</color>";
                }
            }

            // 2. Check for Active Boss
            if (_manager.ActiveBoss != null && !_manager.ActiveBoss.IsDead)
            {
                Vector2 diff = (Vector2)_manager.ActiveBoss.transform.position - pPos;
                return $"<color=#FF7777>⚠ MỤC TIÊU BOSS: {_manager.ActiveBoss.Definition.DisplayName} ({GetCompassArrow(diff)} {diff.magnitude:0.0}m)</color>";
            }

            // 3. Nearest enemy count
            return $"Quái quanh khu vực: {EnemyController.ActiveEnemies.Count} mục tiêu";
        }

        private string GetCompassArrow(Vector2 diff)
        {
            if (diff.sqrMagnitude < 0.25f) return "●";
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            int sector = Mathf.RoundToInt(angle / 45f) % 8;
            string[] arrows = { "→ Đông", "↗ ĐB", "↑ Bắc", "↖ TB", "← Tây", "↙ TN", "↓ Nam", "↘ ĐN" };
            return arrows[sector];
        }

        private int ComputeEquipmentHash(PlayerController p)
        {
            int h = 17 + p.CurrentLevel * 13 + Mathf.RoundToInt(p.BaseAttackStat);
            foreach (var kv in p.EquippedWeapons) h = h * 31 + kv.Key.GetHashCode() + kv.Value;
            foreach (var kv in p.EquippedPassives) h = h * 31 + kv.Key.GetHashCode() + kv.Value;
            return h;
        }

        private void RebuildHudEquipmentSlots(PlayerController p)
        {
            foreach (Transform child in _weaponSlotsContainer) Destroy(child.gameObject);
            foreach (Transform child in _passiveSlotsContainer) Destroy(child.gameObject);

            // Up to 6 weapon slots
            for (int i = 0; i < 6; i++)
            {
                float x = (i - 2.5f) * 84f;
                GameObject slot = CreatePanel(_weaponSlotsContainer, $"WSlot_{i}", new Vector2(0.5f, 0.5f), new Vector2(76f, 74f), new Color(0.14f, 0.17f, 0.26f, 1f));
                slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0f);
                AddOutline(slot, new Color(0.95f, 0.75f, 0.25f, 0.8f), 1.5f);

                if (i < p.WeaponOrder.Count)
                {
                    string wId = p.WeaponOrder[i];
                    int lv = p.GetWeaponLevel(wId);
                    float dmg = p.GetComputedWeaponDamage(wId);
                    WeaponDefinition wDef = GameDatabase.Weapons[wId];
                    CreateImage(slot.transform, "Icon", wDef.IconSprite, new Vector2(0f, 10f), new Vector2(44f, 44f));
                    CreateText(slot.transform, "Lv", $"Lv.{lv} ({dmg:0})", new Vector2(0f, -24f), new Vector2(74f, 20f), 12, new Color(1f, 0.9f, 0.3f), FontStyle.Bold);
                }
            }

            // 5 passive skill slots
            string[] pIds = { "cuong_luc", "than_toc", "giap_cung", "hoi_phuc", "tu_truong" };
            for (int i = 0; i < pIds.Length; i++)
            {
                float x = (i - 2f) * 78f;
                string pid = pIds[i];
                int lv = p.GetPassiveLevel(pid);
                PassiveSkillDefinition pDef = GameDatabase.PassiveSkills[pid];

                Color bgCol = lv > 0 ? new Color(0.16f, 0.26f, 0.38f, 1f) : new Color(0.11f, 0.13f, 0.18f, 0.65f);
                GameObject slot = CreatePanel(_passiveSlotsContainer, $"PSlot_{pid}", new Vector2(0.5f, 0.5f), new Vector2(70f, 74f), bgCol);
                slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, 0f);
                AddOutline(slot, lv > 0 ? new Color(0.4f, 0.85f, 1f, 0.9f) : new Color(0.3f, 0.35f, 0.45f, 0.5f), 1.5f);

                GameObject iconObj = CreateImage(slot.transform, "Icon", pDef.IconSprite, new Vector2(0f, 10f), new Vector2(44f, 44f));
                iconObj.GetComponent<Image>().color = lv > 0 ? Color.white : new Color(1f, 1f, 1f, 0.28f);
                CreateText(slot.transform, "Lv", lv > 0 ? $"Lv.{lv}" : "-", new Vector2(0f, -24f), new Vector2(66f, 20f), 13,
                    lv > 0 ? new Color(0.5f, 0.95f, 1f) : new Color(0.6f, 0.6f, 0.6f), FontStyle.Bold);
            }
        }

        // ==================== UI BUILDER HELPERS ====================
        private GameObject CreateFullScreenPanel(string name, Color bgColor)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_canvas.transform, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = go.GetComponent<Image>();
            img.sprite = ResourceCache.GetWhiteSprite();
            img.color = bgColor;
            return go;
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            Image img = go.GetComponent<Image>();
            img.sprite = ResourceCache.GetWhiteSprite();
            img.color = color;
            return go;
        }

        private GameObject CreateImage(Transform parent, string name, string resourcePath, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = go.GetComponent<Image>();
            img.sprite = ResourceCache.GetSprite(resourcePath, 64f);
            img.preserveAspect = true;
            return go;
        }

        private Text CreateText(Transform parent, string name, string content, Vector2 pos, Vector2 size, int fontSize, Color color, FontStyle style = FontStyle.Normal, TextAnchor align = TextAnchor.MiddleCenter)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Text txt = go.GetComponent<Text>();
            txt.font = _font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.color = color;
            txt.fontStyle = style;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.supportRichText = true;

            Shadow sh = go.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.85f);
            sh.effectDistance = new Vector2(1.5f, -1.5f);
            return txt;
        }

        private Button CreateMenuButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color bgColor, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = CreatePanel(parent, name, new Vector2(0.5f, 0.5f), size, bgColor);
            go.GetComponent<RectTransform>().anchoredPosition = pos;
            AddOutline(go, new Color(1f, 0.9f, 0.6f, 0.8f), 2f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(onClick);

            CreateText(go.transform, "Label", label, Vector2.zero, size - new Vector2(16f, 6f), 20, Color.white, FontStyle.Bold);
            return btn;
        }

        private void AddOutline(GameObject go, Color color, float dist)
        {
            Outline ol = go.AddComponent<Outline>();
            ol.effectColor = color;
            ol.effectDistance = new Vector2(dist, -dist);
        }
    }
}