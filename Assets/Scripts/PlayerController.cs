using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrindAndGlory
{
    [Serializable]
    public class UpgradeOption
    {
        public bool IsWeapon;
        public string ItemId;
        public string Title;
        public string Subtitle;
        public string Description;
        public string IconPath;
        public int TargetLevel;
        public int CardStyleIndex; // 0 = Pink (Main/Weapon), 1 = Blue (Sub Weapon), 2 = Silver (Passive Skill)
    }

    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        public HeroDefinition HeroDef { get; private set; }
        public int CurrentLevel { get; private set; } = 1;
        public int TotalExp { get; private set; } = 0;

        public float MaxHP { get; private set; }
        public float CurrentHP { get; private set; }
        public float BaseAttackStat { get; private set; }
        public float BaseEnduranceStat { get; private set; }
        public float BaseSpeedStat { get; private set; }

        public Vector2 LastMoveDirection { get; private set; } = Vector2.right;
        public bool IsDead { get; private set; }
        public bool GodMode { get; set; }
        public bool AutoAimEnabled { get; set; } = true;

        // Equipped Weapons (Id -> Level 1..5) and Passive Skills (Id -> Level 1..2)
        public readonly Dictionary<string, int> EquippedWeapons = new Dictionary<string, int>();
        public readonly List<string> WeaponOrder = new List<string>();
        public readonly Dictionary<string, int> EquippedPassives = new Dictionary<string, int>();

        private readonly Dictionary<string, float> _weaponCooldownTimers = new Dictionary<string, float>();

        private SpriteRenderer _spriteRenderer;
        private SpriteAnimator _animator;
        private GameObject _auraVisualObj;
        private GameObject _statusVfxObj;
        private SpriteAnimator _statusVfxAnim;
        private Transform _worldHpFill;

        private float _dashActiveTimer;
        private float _dashCooldownTimer;
        private const float DashCooldownMax = 1.15f;
        private Vector2 _dashDirection;
        private float _eggSpeedBuffTimer;
        private float _regenTimer;
        private float _hurtFlashTimer;

        // Monster Burn / Bleed status timers on Player
        private float _playerBurnTimer;
        private float _playerBleedTimer;
        private float _statusDotTickTimer;

        public float DashCooldownRemaining => Mathf.Max(0f, _dashCooldownTimer);
        public float DashCooldownDuration => DashCooldownMax;
        public float EggSpeedBuffRemaining => Mathf.Max(0f, _eggSpeedBuffTimer);
        public float PlayerBurnRemaining => Mathf.Max(0f, _playerBurnTimer);
        public float PlayerBleedRemaining => Mathf.Max(0f, _playerBleedTimer);

        private void Awake()
        {
            Instance = this;
        }

        public void InitializeHero(HeroId heroId)
        {
            HeroDef = GameDatabase.GetHero(heroId);
            CurrentLevel = 1;
            TotalExp = 0;
            IsDead = false;
            LastMoveDirection = Vector2.right;
            _playerBurnTimer = 0f;
            _playerBleedTimer = 0f;
            _eggSpeedBuffTimer = 0f;
            _dashCooldownTimer = 0f;
            _dashActiveTimer = 0f;

            EquippedWeapons.Clear();
            WeaponOrder.Clear();
            EquippedPassives.Clear();
            _weaponCooldownTimers.Clear();

            // Merchant permanent blessings from PlayerPrefs
            int bonusHpLv = PlayerPrefs.GetInt("GG_Shop_HP", 0);
            int bonusAtkLv = PlayerPrefs.GetInt("GG_Shop_ATK", 0);
            int bonusEndLv = PlayerPrefs.GetInt("GG_Shop_END", 0);
            int bonusSpdLv = PlayerPrefs.GetInt("GG_Shop_SPD", 0);

            RecalculateStatsForCurrentLevel(bonusHpLv, bonusAtkLv, bonusEndLv, bonusSpdLv);
            CurrentHP = MaxHP;

            // Equip starting main weapon at Level 1
            EquipOrUpgradeWeapon(HeroDef.StartingWeaponId);

            // Visual setup
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            _animator = GetComponent<SpriteAnimator>();
            if (_animator == null) _animator = gameObject.AddComponent<SpriteAnimator>();
            _animator.Initialize(HeroDef.AssetFolder, 56f, 10f, false);

            CreateWorldPlayerHud();
            UpdateAuraVisual();
        }

        private void CreateWorldPlayerHud()
        {
            Transform oldHud = transform.Find("PlayerWorldHP");
            if (oldHud != null) Destroy(oldHud.gameObject);

            GameObject root = new GameObject("PlayerWorldHP");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0f, -0.25f, 0f);

            Sprite white = ResourceCache.GetWhiteSprite();

            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(root.transform, false);
            bg.transform.localScale = new Vector3(1.1f, 0.13f, 1f);
            SpriteRenderer bgsr = bg.AddComponent<SpriteRenderer>();
            bgsr.sprite = white;
            bgsr.color = new Color(0.08f, 0.08f, 0.1f, 0.9f);
            bgsr.sortingOrder = 2500;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(root.transform, false);
            fill.transform.localScale = new Vector3(1.06f, 0.09f, 1f);
            SpriteRenderer fsr = fill.AddComponent<SpriteRenderer>();
            fsr.sprite = white;
            fsr.color = new Color(0.2f, 0.95f, 0.35f, 1f);
            fsr.sortingOrder = 2501;
            _worldHpFill = fill.transform;
        }

        private void RecalculateStatsForCurrentLevel(int shopHp = -1, int shopAtk = -1, int shopEnd = -1, int shopSpd = -1)
        {
            if (shopHp < 0) shopHp = PlayerPrefs.GetInt("GG_Shop_HP", 0);
            if (shopAtk < 0) shopAtk = PlayerPrefs.GetInt("GG_Shop_ATK", 0);
            if (shopEnd < 0) shopEnd = PlayerPrefs.GetInt("GG_Shop_END", 0);
            if (shopSpd < 0) shopSpd = PlayerPrefs.GetInt("GG_Shop_SPD", 0);

            int lvlOffset = Mathf.Max(0, CurrentLevel - 1);
            float prevMax = MaxHP;

            MaxHP = HeroDef.BaseHP + HeroDef.HPPerLevel * lvlOffset + shopHp * 20f;
            BaseAttackStat = HeroDef.BaseAttack + HeroDef.AttackPerLevel * lvlOffset + shopAtk * 3f;
            BaseEnduranceStat = HeroDef.BaseEndurance + HeroDef.EndurancePerLevel * lvlOffset + shopEnd * 2f;
            BaseSpeedStat = HeroDef.BaseSpeed + HeroDef.SpeedPerLevel * lvlOffset + shopSpd * 0.15f;

            if (prevMax > 0f && CurrentHP > 0f)
            {
                CurrentHP = Mathf.Min(MaxHP, CurrentHP + (MaxHP - prevMax));
            }
            UpdateWorldHpBar();
        }

        public float GetEffectiveEndurance()
        {
            int giapLv = GetPassiveLevel("giap_cung");
            float bonus = giapLv == 1 ? 5f : (giapLv >= 2 ? 10f : 0f);
            return BaseEnduranceStat + bonus;
        }

        public float GetEffectiveMoveSpeed()
        {
            int thanTocLv = GetPassiveLevel("than_toc");
            float mult = thanTocLv == 1 ? 1.10f : (thanTocLv >= 2 ? 1.20f : 1.0f);
            if (_eggSpeedBuffTimer > 0f) mult *= 1.25f;
            if (_dashActiveTimer > 0f) mult *= 2.35f;
            return BaseSpeedStat * mult;
        }

        public float GetAttackSpeedMultiplier()
        {
            int thanTocLv = GetPassiveLevel("than_toc");
            float mult = thanTocLv == 1 ? 1.10f : (thanTocLv >= 2 ? 1.20f : 1.0f);
            float spdBonus = 1.0f + Mathf.Max(0f, BaseSpeedStat - 4.0f) * 0.08f;
            return mult * spdBonus;
        }

        public float GetPickupMagnetRadius()
        {
            int magnetLv = GetPassiveLevel("tu_truong");
            float mult = magnetLv == 1 ? 1.30f : (magnetLv >= 2 ? 1.60f : 1.0f);
            return 2.4f * mult;
        }

        public int GetPassiveLevel(string skillId)
        {
            return EquippedPassives.TryGetValue(skillId, out int lv) ? lv : 0;
        }

        public int GetWeaponLevel(string weaponId)
        {
            return EquippedWeapons.TryGetValue(weaponId, out int lv) ? lv : 0;
        }

        public float GetComputedWeaponDamage(string weaponId)
        {
            if (!GameDatabase.Weapons.TryGetValue(weaponId, out WeaponDefinition wDef)) return 0f;
            int wLv = Mathf.Max(1, GetWeaponLevel(weaponId));
            int cuongLucLv = GetPassiveLevel("cuong_luc");
            return GameDatabase.CalculateWeaponDamage(wDef.BaseDamage, BaseAttackStat, wLv, cuongLucLv);
        }

        public float GetComputedWeaponCooldown(string weaponId)
        {
            if (!GameDatabase.Weapons.TryGetValue(weaponId, out WeaponDefinition wDef)) return 1f;
            int wLv = Mathf.Max(1, GetWeaponLevel(weaponId));
            float baseCd = Mathf.Max(0.25f, wDef.BaseCooldown - (wLv - 1) * wDef.CooldownReductionPerLevel);
            return baseCd / Mathf.Max(0.5f, GetAttackSpeedMultiplier());
        }

        public void EquipOrUpgradeWeapon(string weaponId)
        {
            if (!GameDatabase.Weapons.ContainsKey(weaponId)) return;
            if (EquippedWeapons.TryGetValue(weaponId, out int curLv))
            {
                EquippedWeapons[weaponId] = Mathf.Min(5, curLv + 1);
            }
            else
            {
                EquippedWeapons[weaponId] = 1;
                WeaponOrder.Add(weaponId);
                _weaponCooldownTimers[weaponId] = 0.15f;
            }
            UpdateAuraVisual();
        }

        public void EquipOrUpgradePassive(string passiveId)
        {
            if (!GameDatabase.PassiveSkills.ContainsKey(passiveId)) return;
            if (EquippedPassives.TryGetValue(passiveId, out int curLv))
            {
                EquippedPassives[passiveId] = Mathf.Min(2, curLv + 1);
            }
            else
            {
                EquippedPassives[passiveId] = 1;
            }
        }

        private void UpdateAuraVisual()
        {
            int auraLv = GetWeaponLevel("awaken_aura");
            if (auraLv <= 0)
            {
                if (_auraVisualObj != null) _auraVisualObj.SetActive(false);
                return;
            }

            if (_auraVisualObj == null)
            {
                _auraVisualObj = new GameObject("AwakenAuraRing");
                _auraVisualObj.transform.SetParent(transform, false);
                _auraVisualObj.transform.localPosition = Vector3.zero;
                SpriteRenderer sr = _auraVisualObj.AddComponent<SpriteRenderer>();
                sr.sprite = ResourceCache.GetSprite("Sprites/fx_aura_ring", 64f);
                sr.color = new Color(1f, 0.92f, 0.55f, 0.85f);
                sr.sortingOrder = -5;

                SpriteAnimator auraAnim = _auraVisualObj.AddComponent<SpriteAnimator>();
                auraAnim.Initialize("awaken_aura", 64f, 10f, false, true, "idle");
            }
            _auraVisualObj.SetActive(true);
            float radiusMult = 1.0f + (auraLv - 1) * 0.12f;
            _auraVisualObj.transform.localScale = Vector3.one * (1.85f * radiusMult);
        }

        public void ApplyMonsterStatusEffect(bool burn, bool bleed)
        {
            if (IsDead || GodMode || _dashActiveTimer > 0f) return;

            if (burn)
            {
                _playerBurnTimer = Mathf.Max(_playerBurnTimer, 3.0f);
                FloatingTextPopup.Spawn(transform.position + Vector3.up * 0.35f, "BỎNG LỬA!", new Color(1f, 0.55f, 0.15f), 0.13f);
            }
            if (bleed)
            {
                _playerBleedTimer = Mathf.Max(_playerBleedTimer, 3.0f);
                FloatingTextPopup.Spawn(transform.position + Vector3.up * 0.35f, "CHẢY MÁU!", new Color(0.95f, 0.2f, 0.3f), 0.13f);
            }
            if (_statusDotTickTimer <= 0f) _statusDotTickTimer = 1.0f;
            RefreshPlayerStatusVfx();
        }

        public void CleanseStatusEffects()
        {
            _playerBurnTimer = 0f;
            _playerBleedTimer = 0f;
            RefreshPlayerStatusVfx();
        }

        private void RefreshPlayerStatusVfx()
        {
            bool active = _playerBurnTimer > 0f || _playerBleedTimer > 0f;
            if (!active)
            {
                if (_statusVfxObj != null) _statusVfxObj.SetActive(false);
                return;
            }

            if (_statusVfxObj == null)
            {
                _statusVfxObj = new GameObject("PlayerStatusVFX");
                _statusVfxObj.transform.SetParent(transform, false);
                _statusVfxObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                _statusVfxObj.transform.localScale = Vector3.one * 0.75f;
                SpriteRenderer sr = _statusVfxObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 2600;
                _statusVfxAnim = _statusVfxObj.AddComponent<SpriteAnimator>();
            }

            _statusVfxObj.SetActive(true);
            string folder = _playerBurnTimer >= _playerBleedTimer ? "burn" : "bleeding";
            _statusVfxAnim.Initialize(folder, 64f, 12f, false, true, "attack");
        }

        private void Update()
        {
            if (IsDead) return;
            if (GrindAndGloryManager.Instance == null || !GrindAndGloryManager.Instance.IsGameplayActive) return;

            float dt = Time.deltaTime;

            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= dt;
            if (_dashActiveTimer > 0f) _dashActiveTimer -= dt;
            if (_eggSpeedBuffTimer > 0f) _eggSpeedBuffTimer -= dt;
            if (_hurtFlashTimer > 0f)
            {
                _hurtFlashTimer -= dt;
                if (_hurtFlashTimer <= 0f && _spriteRenderer != null)
                {
                    _spriteRenderer.color = Color.white;
                }
            }

            // Status Effect DoT on Player (Burn / Bleed)
            if (_playerBurnTimer > 0f || _playerBleedTimer > 0f)
            {
                if (_playerBurnTimer > 0f) _playerBurnTimer -= dt;
                if (_playerBleedTimer > 0f) _playerBleedTimer -= dt;
                _statusDotTickTimer -= dt;
                if (_statusDotTickTimer <= 0f)
                {
                    _statusDotTickTimer = 1.0f;
                    if (!GodMode && CurrentHP > 5f)
                    {
                        float dotDmg = (_playerBurnTimer > 0f ? 3f : 0f) + (_playerBleedTimer > 0f ? 3f : 0f);
                        CurrentHP = Mathf.Max(1f, CurrentHP - dotDmg);
                        UpdateWorldHpBar();
                        FloatingTextPopup.Spawn(transform.position, $"-{Mathf.RoundToInt(dotDmg)}", new Color(1f, 0.5f, 0.2f), 0.12f);
                    }
                }
                if (_playerBurnTimer <= 0f && _playerBleedTimer <= 0f)
                {
                    RefreshPlayerStatusVfx();
                }
            }

            // Toggle Auto-Aim mode with [T]
            if (InputHelper.IsAutoAimTogglePressedThisFrame())
            {
                AutoAimEnabled = !AutoAimEnabled;
                string modeTxt = AutoAimEnabled ? "TỰ ĐỘNG NGẮM QUÁI GẦN NHẤT" : "ĐÁNH THEO HƯỚNG DI CHUYỂN";
                FloatingTextPopup.Spawn(transform.position + Vector3.up * 0.5f, modeTxt, new Color(0.6f, 1f, 0.7f), 0.13f);
            }

            // 1. Movement & Dash Input (W/A/S/D or Arrows, LeftShift/Space to Dash)
            Vector2 moveInput = InputHelper.GetMoveVector();
            if (moveInput.sqrMagnitude > 0.01f)
            {
                LastMoveDirection = moveInput.normalized;
            }

            if (InputHelper.IsDashPressedThisFrame() && _dashCooldownTimer <= 0f)
            {
                _dashActiveTimer = 0.25f;
                _dashCooldownTimer = DashCooldownMax;
                _dashDirection = LastMoveDirection;
                CleanseStatusEffects();
                if (_animator != null) _animator.PlayOneShot("dash", "run");
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_dash", 0.65f);
                FloatingTextPopup.Spawn(transform.position, "LƯỚT NÉ!", new Color(0.55f, 0.95f, 1f), 0.12f);
            }

            Vector2 activeMoveDir = _dashActiveTimer > 0f ? _dashDirection : moveInput;
            if (activeMoveDir.sqrMagnitude > 0.001f)
            {
                float moveSpeed = GetEffectiveMoveSpeed();
                Vector2 desiredPos = (Vector2)transform.position + activeMoveDir * (moveSpeed * dt);
                if (MapEnvironmentBuilder.Instance != null)
                {
                    desiredPos = MapEnvironmentBuilder.Instance.ClampAndResolveMovement(desiredPos, 0.40f);
                }
                transform.position = new Vector3(desiredPos.x, desiredPos.y, 0f);

                if (_spriteRenderer != null)
                {
                    if (Mathf.Abs(LastMoveDirection.x) > 0.05f)
                    {
                        _spriteRenderer.flipX = LastMoveDirection.x > 0f;
                    }
                    _spriteRenderer.sortingOrder = Mathf.RoundToInt(-desiredPos.y * 100f);
                }

                if (_animator != null && _dashActiveTimer <= 0f)
                {
                    _animator.Play("run", true, false);
                }
            }
            else
            {
                if (_animator != null && _dashActiveTimer <= 0f)
                {
                    _animator.Play("idle", true, false);
                }
            }

            // 2. Passive Skill: Hồi Phục 1 / 2 (2% / 4% MaxHP every 3 seconds)
            int hoiPhucLv = GetPassiveLevel("hoi_phuc");
            if (hoiPhucLv > 0 && CurrentHP < MaxHP)
            {
                _regenTimer += dt;
                if (_regenTimer >= 3.0f)
                {
                    _regenTimer = 0f;
                    float pct = hoiPhucLv == 1 ? 0.02f : 0.04f;
                    Heal(MaxHP * pct, false);
                }
            }

            // 3. Rotate Aura visual
            if (_auraVisualObj != null && _auraVisualObj.activeSelf)
            {
                _auraVisualObj.transform.Rotate(0f, 0f, 90f * dt);
            }

            // 4. Auto-Attack Cycle for all Equipped Weapons
            UpdateAutoAttackWeapons(dt);
        }

        private void UpdateAutoAttackWeapons(float dt)
        {
            float atkSpeedMult = GetAttackSpeedMultiplier();
            int cuongLucLv = GetPassiveLevel("cuong_luc");

            for (int i = 0; i < WeaponOrder.Count; i++)
            {
                string wId = WeaponOrder[i];
                if (!EquippedWeapons.TryGetValue(wId, out int wLv) || wLv <= 0) continue;
                if (!GameDatabase.Weapons.TryGetValue(wId, out WeaponDefinition wDef)) continue;

                if (!_weaponCooldownTimers.TryGetValue(wId, out float timer)) timer = 0f;
                timer -= dt * atkSpeedMult;

                if (timer <= 0f)
                {
                    float cd = Mathf.Max(0.25f, wDef.BaseCooldown - (wLv - 1) * wDef.CooldownReductionPerLevel);
                    _weaponCooldownTimers[wId] = cd;
                    FireWeapon(wDef, wLv, cuongLucLv);
                }
                else
                {
                    _weaponCooldownTimers[wId] = timer;
                }
            }
        }

        private Vector2 GetSmartTargetDirection(float maxRange, bool isMeleeCleave)
        {
            Vector2 moveDir = LastMoveDirection.sqrMagnitude > 0.01f ? LastMoveDirection.normalized : Vector2.right;
            if (!AutoAimEnabled) return moveDir;

            Vector2 origin = transform.position;
            EnemyController bestTarget = null;
            float bestScore = float.MaxValue;

            var enemies = EnemyController.ActiveEnemies;
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyController e = enemies[i];
                if (e == null || e.IsDead) continue;

                Vector2 toE = (Vector2)e.transform.position - origin;
                float dist = toE.magnitude;
                if (dist > maxRange || dist < 0.001f) continue;

                // If melee cleave and there are enemies roughly in front of moveDir, prefer moveDir unless nothing is in front
                float score = dist;
                if (e.Definition.Category != MonsterCategory.Regular && dist <= maxRange * 0.85f)
                {
                    score *= 0.7f; // Prioritize Bosses in range
                }
                if (isMeleeCleave && Vector2.Dot(toE / dist, moveDir) > 0.25f)
                {
                    score *= 0.65f; // Favor enemies in front of movement for melee
                }

                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = e;
                }
            }

            if (bestTarget != null)
            {
                Vector2 diff = (Vector2)bestTarget.transform.position - origin;
                if (diff.sqrMagnitude > 0.001f) return diff.normalized;
            }
            return moveDir;
        }

        private void FireWeapon(WeaponDefinition wDef, int wLv, int cuongLucLv)
        {
            // Formula from Section 3.1:
            // Sát thương = (Chỉ số cơ bản của Vũ khí + Chỉ số Strength/Magic của nhân vật) x (1 + 0,2 x Cấp độ Vũ khí) x Hệ số kỹ năng Cường Lực
            float totalDmg = GameDatabase.CalculateWeaponDamage(wDef.BaseDamage, BaseAttackStat, wLv, cuongLucLv);

            Vector2 origin = transform.position;
            Vector2 moveDir = LastMoveDirection.sqrMagnitude > 0.01f ? LastMoveDirection.normalized : Vector2.right;

            switch (wDef.Id)
            {
                case "weapon_00": // Kiếm Ánh Sáng (Mercy)
                {
                    float range = 2.35f * (1f + (wLv - 1) * wDef.RangeOrRadiusBonusPerLevel);
                    Vector2 slashDir = GetSmartTargetDirection(range + 0.8f, true);
                    CombatAoEEffect.SpawnSlashArc(origin, slashDir, range, totalDmg, false, 1.8f);
                    if (_animator != null) _animator.PlayOneShot("attack", "idle");
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_slash", 0.55f);
                    break;
                }
                case "weapon_04": // Trượng Nguyên Tố (Inirius - Fire/Burn)
                {
                    int count = 1 + (wLv - 1) * wDef.ExtraProjectilesPerLevel;
                    Vector2 aimDir = GetSmartTargetDirection(9.5f, false);
                    float baseAng = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                    for (int k = 0; k < count; k++)
                    {
                        float offset = (k - (count - 1) * 0.5f) * 12f;
                        float rad = (baseAng + offset) * Mathf.Deg2Rad;
                        Vector2 fireDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                        CombatProjectile.SpawnPlayerLinearProjectile(origin + fireDir * 0.45f, fireDir, 9.2f, totalDmg, "Sprites/fx_proj_fire", 2, 0f, 0f, 1.05f, "fire", true);
                    }
                    if (_animator != null) _animator.PlayOneShot("attack", "idle");
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_fireball", 0.55f);
                    break;
                }
                case "weapon_06_1": // Sách Phép Cổ Đại (Eni - Freezing)
                {
                    float slowPct = wDef.SlowPercentBase + (wLv - 1) * wDef.SlowPercentPerLevel;
                    int rays = wLv >= 3 ? 2 : 1;
                    Vector2 aimDir = GetSmartTargetDirection(9.5f, false);
                    float baseAng = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                    for (int k = 0; k < rays; k++)
                    {
                        float offset = (k - (rays - 1) * 0.5f) * 14f;
                        float rad = (baseAng + offset) * Mathf.Deg2Rad;
                        Vector2 iceDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                        CombatProjectile.SpawnPlayerLinearProjectile(origin + iceDir * 0.45f, iceDir, 10.0f, totalDmg, "Sprites/fx_proj_ice", 3, slowPct, 240f, 1.0f);
                    }
                    if (_animator != null) _animator.PlayOneShot("attack", "idle");
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_ice", 0.55f);
                    break;
                }
                case "weapon_02_1": // Rìu Chiến Xoay
                {
                    int axeCount = 1 + (wLv - 1) * wDef.ExtraProjectilesPerLevel;
                    Vector2 aimDir = GetSmartTargetDirection(7.5f, false);
                    for (int k = 0; k < axeCount; k++)
                    {
                        CombatProjectile.SpawnSpinningAxe(origin, aimDir, k, axeCount, totalDmg);
                    }
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_slash", 0.45f);
                    break;
                }
                case "weapon_05_4": // Phi Tiêu Bóng Đêm
                {
                    int shurikenCount = 1 + (wLv - 1) * wDef.ExtraProjectilesPerLevel;
                    Vector2 aimDir = GetSmartTargetDirection(10.0f, false);
                    float baseAng = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
                    for (int k = 0; k < shurikenCount; k++)
                    {
                        float offset = (k - (shurikenCount - 1) * 0.5f) * 9f;
                        float rad = (baseAng + offset) * Mathf.Deg2Rad;
                        Vector2 sDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                        CombatProjectile.SpawnPlayerLinearProjectile(origin + sDir * 0.4f, sDir, 13.5f, totalDmg, "Sprites/weapon_05_4", 2, 0f, 720f, 0.85f);
                    }
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_slash", 0.35f);
                    break;
                }
                case "weapon_08": // Đại Đao Phán Quyết
                {
                    float range = 3.0f * (1f + (wLv - 1) * 0.08f);
                    Vector2 cleaveDir = GetSmartTargetDirection(range + 0.8f, true);
                    CombatAoEEffect.SpawnSlashArc(origin, cleaveDir, range, totalDmg, true, 5.2f);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_slash", 0.75f);
                    break;
                }
                case "hurricane": // Cuồng Phong
                {
                    float radScale = 1.0f + (wLv - 1) * wDef.RangeOrRadiusBonusPerLevel;
                    Vector2 vortexDir = GetSmartTargetDirection(8.5f, false);
                    CombatProjectile.SpawnHurricaneVortex(origin + vortexDir * 0.6f, vortexDir, totalDmg, radScale);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_wind", 0.55f);
                    break;
                }
                case "thunderbolt": // Thiên Lôi
                {
                    int strikes = 1 + (wLv - 1) * wDef.ExtraProjectilesPerLevel;
                    var enemies = EnemyController.ActiveEnemies;
                    for (int k = 0; k < strikes; k++)
                    {
                        Vector2 strikePos;
                        if (enemies.Count > 0)
                        {
                            EnemyController target = enemies[UnityEngine.Random.Range(0, enemies.Count)];
                            strikePos = target != null ? (Vector2)target.transform.position : origin + UnityEngine.Random.insideUnitCircle * 4f;
                        }
                        else
                        {
                            strikePos = origin + moveDir * 3f + UnityEngine.Random.insideUnitCircle * 2f;
                        }
                        CombatAoEEffect.SpawnThunderboltStrike(strikePos, totalDmg);
                    }
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_thunder", 0.60f);
                    break;
                }
                case "awaken_aura": // Hào Quang Thức Tỉnh (35/s continuous, tick every 0.5s)
                {
                    float tickDmg = totalDmg * 0.5f;
                    float auraRadius = 2.15f * (1f + (wLv - 1) * wDef.RangeOrRadiusBonusPerLevel);
                    var enemies = EnemyController.ActiveEnemies;
                    for (int k = enemies.Count - 1; k >= 0; k--)
                    {
                        if (k >= enemies.Count) continue;
                        EnemyController e = enemies[k];
                        if (e == null || e.IsDead) continue;
                        if (Vector2.Distance(origin, e.transform.position) <= auraRadius)
                        {
                            e.TakeWeaponDamage(tickDmg, false);
                        }
                    }
                    break;
                }
            }
        }

        public void TakeDamageFromMonster(float monsterStrength, string attackerName = "")
        {
            if (IsDead || GodMode || _dashActiveTimer > 0f) return;

            float dmgTaken = GameDatabase.CalculateDamageTaken(monsterStrength, GetEffectiveEndurance());
            CurrentHP = Mathf.Max(0f, CurrentHP - dmgTaken);
            UpdateWorldHpBar();

            FloatingTextPopup.Spawn(transform.position, $"-{Mathf.RoundToInt(dmgTaken)}", new Color(1f, 0.25f, 0.25f), 0.15f);
            _hurtFlashTimer = 0.15f;
            if (_spriteRenderer != null) _spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 1f);
            if (_animator != null) _animator.PlayOneShot("hurt", "idle");
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_hit", 0.7f);

            if (CurrentHP <= 0f)
            {
                IsDead = true;
                CleanseStatusEffects();
                if (_animator != null) _animator.Play("die", false, true);
                if (GrindAndGloryManager.Instance != null)
                {
                    GrindAndGloryManager.Instance.OnPlayerDefeated();
                }
            }
        }

        public void Heal(float amount, bool showPopup = true)
        {
            if (IsDead || amount <= 0f) return;
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
            UpdateWorldHpBar();
            if (showPopup)
            {
                FloatingTextPopup.Spawn(transform.position, $"+{Mathf.RoundToInt(amount)} HP", new Color(0.25f, 1f, 0.45f), 0.14f);
            }
        }

        private void UpdateWorldHpBar()
        {
            if (_worldHpFill == null) return;
            float ratio = Mathf.Clamp01(CurrentHP / Mathf.Max(1f, MaxHP));
            _worldHpFill.localScale = new Vector3(1.06f * ratio, 0.09f, 1f);
            _worldHpFill.localPosition = new Vector3(-1.06f * (1f - ratio) * 0.5f, 0f, 0f);
        }

        public void CollectDropItem(DropItemController drop)
        {
            if (drop == null || IsDead) return;

            switch (drop.ItemType)
            {
                case DropItemType.EnergyCapsule:
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_exp", 0.45f);
                    if (GrindAndGloryManager.Instance != null && drop.GoldValue > 0)
                    {
                        GrindAndGloryManager.Instance.AddGold(drop.GoldValue);
                    }
                    GainExp(drop.ExpValue);
                    break;

                case DropItemType.RedBall:
                    // Cầu Sinh Lực 1: Hồi ngay lập tức 30% lượng máu tối đa
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_heal", 0.75f);
                    CleanseStatusEffects();
                    Heal(MaxHP * 0.30f, true);
                    break;

                case DropItemType.ItemEgg:
                    // Cầu Sinh Lực 2: Hồi 50% lượng máu và tăng nhẹ tốc độ di chuyển tạm thời
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_heal", 0.85f);
                    CleanseStatusEffects();
                    Heal(MaxHP * 0.50f, true);
                    _eggSpeedBuffTimer = 6.0f;
                    FloatingTextPopup.Spawn(transform.position + Vector3.up * 0.4f, "TĂNG TỐC!", new Color(0.4f, 0.95f, 1f), 0.14f);
                    break;

                case DropItemType.ItemBox:
                    // Rương Vũ Khí: Nâng cấp tức thì 1-2 cấp độ cho vũ khí đang có trong Ô trang bị và thưởng Vàng
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_chest", 0.9f);
                    int levelsToGrant = UnityEngine.Random.Range(1, 3);
                    for (int i = 0; i < levelsToGrant; i++)
                    {
                        if (WeaponOrder.Count > 0)
                        {
                            string wId = WeaponOrder[UnityEngine.Random.Range(0, WeaponOrder.Count)];
                            EquipOrUpgradeWeapon(wId);
                        }
                    }
                    if (GrindAndGloryManager.Instance != null)
                    {
                        GrindAndGloryManager.Instance.AddGold(Mathf.Max(150, drop.GoldValue));
                        GrindAndGloryManager.Instance.ShowBannerMessage($"MỞ RƯƠNG VŨ KHÍ! Nâng cấp tức thì +{levelsToGrant} cấp Vũ khí & +{Mathf.Max(150, drop.GoldValue)} Gold!");
                    }
                    FloatingTextPopup.Spawn(transform.position, $"RƯƠNG VŨ KHÍ +{levelsToGrant} CẤP!", new Color(1f, 0.85f, 0.2f), 0.16f);
                    break;
            }
        }

        public void GainExp(int expAmount)
        {
            if (expAmount <= 0 || IsDead) return;
            TotalExp += expAmount;

            while (TotalExp >= GameDatabase.GetCumulativeExpThreshold(CurrentLevel))
            {
                CurrentLevel++;
                RecalculateStatsForCurrentLevel();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx("sfx_levelup", 0.85f);
                FloatingTextPopup.Spawn(transform.position, $"LEVEL UP! (Lv.{CurrentLevel})", new Color(1f, 0.92f, 0.25f), 0.18f);

                if (GrindAndGloryManager.Instance != null)
                {
                    GrindAndGloryManager.Instance.TriggerLevelUpSelection();
                }
            }
        }

        public List<UpgradeOption> GenerateUpgradeChoices()
        {
            List<UpgradeOption> pool = new List<UpgradeOption>();

            // Build candidate weapon upgrades
            foreach (var kv in GameDatabase.Weapons)
            {
                WeaponDefinition w = kv.Value;
                // Only offer character's own main weapon or sub-weapons
                bool isOtherHeroMain = (w.Category == WeaponType.MainPhysical || w.Category == WeaponType.MainMagic) && w.Id != HeroDef.StartingWeaponId;
                if (isOtherHeroMain) continue;

                int curLv = GetWeaponLevel(w.Id);
                if (curLv >= 5) continue;

                int nextLv = curLv + 1;
                float nextDmg = GameDatabase.CalculateWeaponDamage(w.BaseDamage, BaseAttackStat, nextLv, GetPassiveLevel("cuong_luc"));
                pool.Add(new UpgradeOption
                {
                    IsWeapon = true,
                    ItemId = w.Id,
                    TargetLevel = nextLv,
                    Title = $"{w.DisplayName} (Lv.{nextLv})",
                    Subtitle = curLv == 0 ? "Vũ khí Phụ Mới" : (w.Id == HeroDef.StartingWeaponId ? "Nâng cấp Vũ khí Chính" : "Nâng cấp Vũ khí Phụ"),
                    Description = $"{w.Description}\n<color=#FFE066>{w.LevelUpNote}</color>\n<color=#7FFFD4>Sát thương cấp {nextLv}: {nextDmg:0.#}</color>",
                    IconPath = w.IconSprite,
                    CardStyleIndex = w.Id == HeroDef.StartingWeaponId ? 0 : 1
                });
            }

            // Build candidate passive skill upgrades
            foreach (var kv in GameDatabase.PassiveSkills)
            {
                PassiveSkillDefinition p = kv.Value;
                int curLv = GetPassiveLevel(p.Id);
                if (curLv >= p.MaxLevel) continue;

                int nextLv = curLv + 1;
                pool.Add(new UpgradeOption
                {
                    IsWeapon = false,
                    ItemId = p.Id,
                    TargetLevel = nextLv,
                    Title = $"{p.DisplayName} {nextLv}",
                    Subtitle = "Kỹ năng Nội tại (Passive Skill)",
                    Description = nextLv == 1 ? p.DescriptionLv1 : p.DescriptionLv2,
                    IconPath = p.IconSprite,
                    CardStyleIndex = 2
                });
            }

            // Prioritize signature milestone upgrades from Bảng 3 Nhân vật (Lv3, Lv5, Lv10, Lv15)
            List<string> preferredIds = GetPreferredMilestoneIds(HeroDef.Id, CurrentLevel);
            List<UpgradeOption> result = new List<UpgradeOption>();

            foreach (string prefId in preferredIds)
            {
                UpgradeOption match = pool.Find(o => o.ItemId == prefId);
                if (match != null && !result.Contains(match) && result.Count < 3)
                {
                    result.Add(match);
                }
            }

            // Fill remaining slots randomly
            while (result.Count < 3 && pool.Count > result.Count)
            {
                UpgradeOption candidate = pool[UnityEngine.Random.Range(0, pool.Count)];
                if (!result.Contains(candidate))
                {
                    result.Add(candidate);
                }
            }

            // Fallback if all weapons & passives are maxed
            while (result.Count < 3)
            {
                result.Add(new UpgradeOption
                {
                    IsWeapon = false,
                    ItemId = "__heal_gold__",
                    TargetLevel = CurrentLevel,
                    Title = "Phước Lành Astral",
                    Subtitle = "Hồi phục & Thưởng Vàng",
                    Description = "Hồi ngay 50% HP tối đa và nhận +100 Gold.",
                    IconPath = "Sprites/item_egg",
                    CardStyleIndex = 2
                });
            }

            return result;
        }

        private List<string> GetPreferredMilestoneIds(HeroId id, int level)
        {
            List<string> list = new List<string>();
            if (id == HeroId.Mercy)
            {
                if (level <= 3) { list.Add("weapon_02_1"); list.Add("cuong_luc"); }
                else if (level <= 5) { list.Add("weapon_00"); list.Add("giap_cung"); }
                else if (level <= 10) { list.Add("awaken_aura"); list.Add("weapon_08"); }
                else { list.Add("hoi_phuc"); list.Add("weapon_00"); }
            }
            else if (id == HeroId.Inirius)
            {
                if (level <= 3) { list.Add("thunderbolt"); list.Add("cuong_luc"); }
                else if (level <= 5) { list.Add("weapon_04"); }
                else if (level <= 10) { list.Add("hurricane"); list.Add("than_toc"); }
                else { list.Add("awaken_aura"); list.Add("weapon_04"); }
            }
            else // Eni
            {
                if (level <= 3) { list.Add("weapon_05_4"); list.Add("tu_truong"); }
                else if (level <= 5) { list.Add("weapon_06_1"); list.Add("hoi_phuc"); }
                else if (level <= 10) { list.Add("hurricane"); list.Add("thunderbolt"); }
                else { list.Add("than_toc"); list.Add("weapon_06_1"); }
            }
            return list;
        }

        public void ApplyUpgradeOption(UpgradeOption opt)
        {
            if (opt == null) return;
            if (opt.ItemId == "__heal_gold__")
            {
                Heal(MaxHP * 0.5f, true);
                if (GrindAndGloryManager.Instance != null) GrindAndGloryManager.Instance.AddGold(100);
                return;
            }

            if (opt.IsWeapon)
            {
                EquipOrUpgradeWeapon(opt.ItemId);
            }
            else
            {
                EquipOrUpgradePassive(opt.ItemId);
            }
        }
    }
}