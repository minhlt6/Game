using System.Collections.Generic;
using UnityEngine;

namespace GrindAndGlory
{
    public class FloatingTextPopup : MonoBehaviour
    {
        private TextMesh _textMesh;
        private TextMesh _shadowMesh;
        private Vector3 _velocity;
        private float _lifetime = 0.78f;
        private float _timer;
        private Color _baseColor;

        public static void Spawn(Vector3 worldPos, string text, Color color, float fontSize = 0.14f)
        {
            GameObject go = new GameObject("PopupText");
            go.transform.position = worldPos + new Vector3(UnityEngine.Random.Range(-0.22f, 0.22f), 0.65f, -1f);
            go.transform.localScale = Vector3.one * (fontSize * 1.15f);

            // High-DPI FontSize = 96 with smaller characterSize for razor-sharp text rendering
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 96;
            tm.characterSize = 0.09f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 5005;

            // Crisp dark outline/drop-shadow backing
            GameObject shObj = new GameObject("Shadow");
            shObj.transform.SetParent(go.transform, false);
            shObj.transform.localPosition = new Vector3(0.06f, -0.06f, 0.01f);
            TextMesh stm = shObj.AddComponent<TextMesh>();
            stm.text = text;
            stm.fontSize = 96;
            stm.characterSize = 0.09f;
            stm.anchor = TextAnchor.MiddleCenter;
            stm.alignment = TextAlignment.Center;
            stm.fontStyle = FontStyle.Bold;
            stm.color = new Color(0f, 0f, 0f, 0.92f);
            MeshRenderer smr = shObj.GetComponent<MeshRenderer>();
            if (smr != null) smr.sortingOrder = 5004;

            FloatingTextPopup pop = go.AddComponent<FloatingTextPopup>();
            pop._textMesh = tm;
            pop._shadowMesh = stm;
            pop._baseColor = color;
            pop._velocity = new Vector3(UnityEngine.Random.Range(-0.35f, 0.35f), 1.65f, 0f);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += _velocity * Time.deltaTime;
            _velocity.y *= 0.96f;

            float alpha = Mathf.Clamp01(1f - (_timer / _lifetime));
            if (_textMesh != null)
            {
                _textMesh.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
            }
            if (_shadowMesh != null)
            {
                _shadowMesh.color = new Color(0f, 0f, 0f, alpha * 0.92f);
            }

            if (_timer >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    public enum DropItemType
    {
        EnergyCapsule = 0, // Hạt Kinh Nghiệm (energy_capsules)
        RedBall = 1,       // Cầu Sinh Lực 1 (redball) - 30% HP
        ItemEgg = 2,       // Cầu Sinh Lực 2 (item_egg) - 50% HP + Speed buff
        ItemBox = 3        // Rương Vũ Khí (item_box / golden_mimic) - Nâng cấp vũ khí 1-2 cấp + Gold
    }

    public class DropItemController : MonoBehaviour
    {
        public static readonly List<DropItemController> ActiveDrops = new List<DropItemController>();

        public DropItemType ItemType;
        public int ExpValue = 2;
        public int GoldValue = 10;

        private float _bobPhase;
        private Vector3 _basePos;
        private bool _isMagnetized;

        public static DropItemController SpawnDrop(Vector2 pos, DropItemType type, int expVal = 2, int goldVal = 10)
        {
            GameObject go = new GameObject($"Drop_{type}");
            Vector2 offset = UnityEngine.Random.insideUnitCircle * 0.35f;
            Vector3 spawnPos = new Vector3(pos.x + offset.x, pos.y + offset.y, 0f);
            go.transform.position = spawnPos;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = MapEnvironmentBuilder.GetEntitySortingOrder(spawnPos.y, -5);

            DropItemController ctrl = go.AddComponent<DropItemController>();
            ctrl.ItemType = type;
            ctrl.ExpValue = expVal;
            ctrl.GoldValue = goldVal;
            ctrl._basePos = spawnPos;
            ctrl._bobPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

            switch (type)
            {
                case DropItemType.EnergyCapsule:
                {
                    sr.sprite = ResourceCache.GetSprite("Sprites/item_energy_capsules", 96f);
                    SpriteAnimator anim = go.AddComponent<SpriteAnimator>();
                    anim.Initialize("energy_capsules", 96f, 8f, false, true, "idle");
                    go.transform.localScale = Vector3.one * 0.72f;
                    break;
                }
                case DropItemType.RedBall:
                    sr.sprite = ResourceCache.GetSprite("Sprites/item_redball", 32f);
                    go.transform.localScale = Vector3.one * 0.9f;
                    break;
                case DropItemType.ItemEgg:
                    sr.sprite = ResourceCache.GetSprite("Sprites/item_egg", 84f);
                    go.transform.localScale = Vector3.one * 0.85f;
                    break;
                case DropItemType.ItemBox:
                {
                    sr.sprite = ResourceCache.GetSprite("Sprites/item_box", 72f);
                    SpriteAnimator anim = go.AddComponent<SpriteAnimator>();
                    anim.Initialize("item_box", 72f, 8f, false, true, "idle");
                    go.transform.localScale = Vector3.one * 1.05f;
                    break;
                }
            }

            ActiveDrops.Add(ctrl);
            return ctrl;
        }

        private void OnDestroy()
        {
            ActiveDrops.Remove(this);
        }

        private void Update()
        {
            if (GrindAndGloryManager.Instance == null || !GrindAndGloryManager.Instance.IsGameplayActive) return;
            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead) return;

            Vector2 pPos = player.transform.position;
            Vector2 myPos = transform.position;
            float dist = Vector2.Distance(pPos, myPos);

            float magnetRadius = player.GetPickupMagnetRadius();
            if (ItemType == DropItemType.ItemBox) magnetRadius *= 0.65f;

            if (dist <= magnetRadius)
            {
                _isMagnetized = true;
            }

            if (_isMagnetized)
            {
                Vector2 dir = (pPos - myPos).normalized;
                float pullSpeed = Mathf.Max(8.5f, player.GetEffectiveMoveSpeed() * 2.2f);
                transform.position += (Vector3)(dir * pullSpeed * Time.deltaTime);
                if (dist <= 0.55f)
                {
                    player.CollectDropItem(this);
                    Destroy(gameObject);
                }
            }
            else
            {
                _bobPhase += Time.deltaTime * 4f;
                transform.position = _basePos + new Vector3(0f, Mathf.Sin(_bobPhase) * 0.08f, 0f);
            }
        }
    }

    public class EnemyController : MonoBehaviour
    {
        public static readonly List<EnemyController> ActiveEnemies = new List<EnemyController>();

        public MonsterDefinition Definition;
        public float CurrentHP;
        public float MaxHP;
        public bool IsDead;

        private SpriteRenderer _spriteRenderer;
        private SpriteAnimator _animator;
        private Transform _hpBarFill;
        private GameObject _hpBarRoot;
        private GameObject _burnVfxObj;
        private GameObject _freezeVfxObj;

        private float _contactCooldown;
        private float _specialAttackTimer;
        private float _slowTimer;
        private float _slowRatio; // 0..0.75
        private float _burnTimer;
        private float _burnTickTimer;
        private float _burnDps;
        private float _flashTimer;
        private Vector2 _knockbackVel;

        // Dash/Charge & Boss Enrage state
        private float _chargeDurationTimer;
        private Vector2 _chargeDir;
        private bool _isEnraged;

        public static EnemyController SpawnMonster(string monsterId, Vector2 position, float difficultyScale = 1.0f)
        {
            if (!GameDatabase.Monsters.TryGetValue(monsterId, out MonsterDefinition def))
            {
                def = GameDatabase.Monsters["slime"];
            }

            GameObject go = new GameObject($"Enemy_{def.Id}");
            go.transform.position = new Vector3(position.x, position.y, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = MapEnvironmentBuilder.GetEntitySortingOrder(position.y);
            SpriteAnimator anim = go.AddComponent<SpriteAnimator>();

            bool useSecondary = !string.IsNullOrEmpty(def.SecondaryAnimFolder) && UnityEngine.Random.value > 0.5f;
            string animFolder = useSecondary ? def.SecondaryAnimFolder : def.PrimaryAnimFolder;

            float ppu = def.Category == MonsterCategory.FinalBoss ? 48f :
                        (def.Category == MonsterCategory.MiniBoss ? 56f : 64f);
            anim.Initialize(animFolder, ppu, 9f, false);

            EnemyController enemy = go.AddComponent<EnemyController>();
            enemy.Definition = def;
            enemy._spriteRenderer = sr;
            enemy._animator = anim;
            enemy.MaxHP = def.MaxHP * difficultyScale;
            enemy.CurrentHP = enemy.MaxHP;
            enemy._specialAttackTimer = UnityEngine.Random.Range(1.8f, 3.5f);

            enemy.CreateWorldHealthBar(def.Category != MonsterCategory.Regular);
            ActiveEnemies.Add(enemy);
            return enemy;
        }

        private void CreateWorldHealthBar(bool isBoss)
        {
            _hpBarRoot = new GameObject("HPBar");
            _hpBarRoot.transform.SetParent(transform, false);
            float yOffset = isBoss ? -0.35f : -0.22f;
            float width = isBoss ? 1.8f : 0.85f;
            float height = isBoss ? 0.16f : 0.10f;
            _hpBarRoot.transform.localPosition = new Vector3(0f, yOffset, 0f);

            Sprite white = ResourceCache.GetWhiteSprite();

            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(_hpBarRoot.transform, false);
            bg.transform.localScale = new Vector3(width, height, 1f);
            SpriteRenderer bgsr = bg.AddComponent<SpriteRenderer>();
            bgsr.sprite = white;
            bgsr.color = new Color(0.1f, 0.1f, 0.12f, 0.85f);
            bgsr.sortingOrder = 2000;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(_hpBarRoot.transform, false);
            fill.transform.localPosition = Vector3.zero;
            fill.transform.localScale = new Vector3(width, height * 0.8f, 1f);
            SpriteRenderer fsr = fill.AddComponent<SpriteRenderer>();
            fsr.sprite = white;
            fsr.color = isBoss ? new Color(0.95f, 0.2f, 0.25f, 1f) : new Color(0.9f, 0.3f, 0.2f, 0.95f);
            fsr.sortingOrder = 2001;

            _hpBarFill = fill.transform;
        }

        private void OnDestroy()
        {
            ActiveEnemies.Remove(this);
        }

        private void Update()
        {
            if (IsDead) return;
            if (GrindAndGloryManager.Instance == null || !GrindAndGloryManager.Instance.IsGameplayActive) return;

            PlayerController player = PlayerController.Instance;
            if (player == null || player.IsDead) return;

            float dt = Time.deltaTime;

            if (_contactCooldown > 0f) _contactCooldown -= dt;
            if (_slowTimer > 0f)
            {
                _slowTimer -= dt;
                if (_slowTimer <= 0f)
                {
                    _slowRatio = 0f;
                    if (_freezeVfxObj != null) _freezeVfxObj.SetActive(false);
                    UpdateTintColor();
                }
            }
            if (_burnTimer > 0f)
            {
                _burnTimer -= dt;
                _burnTickTimer -= dt;
                if (_burnTickTimer <= 0f)
                {
                    _burnTickTimer = 0.5f;
                    TakeWeaponDamage(Mathf.Max(2f, _burnDps * 0.5f), false);
                    if (IsDead) return;
                }
                if (_burnTimer <= 0f)
                {
                    if (_burnVfxObj != null) _burnVfxObj.SetActive(false);
                    UpdateTintColor();
                }
            }
            if (_flashTimer > 0f)
            {
                _flashTimer -= dt;
                if (_flashTimer <= 0f)
                {
                    UpdateTintColor();
                }
            }

            Vector2 myPos = transform.position;
            Vector2 pPos = player.transform.position;
            Vector2 toPlayer = pPos - myPos;
            float dist = toPlayer.magnitude;
            Vector2 dir = dist > 0.001f ? (toPlayer / dist) : Vector2.right;

            // Movement + Knockback + Charge + Boss Enrage Speed Boost
            float speedMult = Mathf.Clamp(1.0f - _slowRatio, 0.25f, 1.0f);
            float baseSpeedFactor = Definition.Category == MonsterCategory.Regular ? 0.85f : (_isEnraged ? 1.32f : 1.05f);
            float moveSpeed = Definition.Speed * baseSpeedFactor * speedMult;

            if (_chargeDurationTimer > 0f)
            {
                _chargeDurationTimer -= dt;
                dir = _chargeDir;
                moveSpeed *= Definition.Category == MonsterCategory.Regular ? 2.2f : 2.8f;
            }

            Vector2 desiredPos = myPos + dir * (moveSpeed * dt) + _knockbackVel * dt;
            _knockbackVel = Vector2.Lerp(_knockbackVel, Vector2.zero, dt * 10f);

            if (MapEnvironmentBuilder.Instance != null)
            {
                float radius = Definition.Category == MonsterCategory.Regular ? 0.35f : 0.55f;
                desiredPos = MapEnvironmentBuilder.Instance.ClampAndResolveMovement(desiredPos, radius);
            }

            transform.position = new Vector3(desiredPos.x, desiredPos.y, 0f);

            if (_spriteRenderer != null)
            {
                if (Mathf.Abs(dir.x) > 0.05f)
                {
                    _spriteRenderer.flipX = dir.x > 0f;
                }
                _spriteRenderer.sortingOrder = MapEnvironmentBuilder.GetEntitySortingOrder(desiredPos.y);
            }

            if (_animator != null && _chargeDurationTimer <= 0f)
            {
                _animator.Play("run", true, false);
            }

            // Contact Attack against Player
            float hitRange = Definition.Category == MonsterCategory.Regular ? 0.72f : 1.25f;
            if (dist <= hitRange && _contactCooldown <= 0f)
            {
                _contactCooldown = Definition.Category == MonsterCategory.Regular ? 0.75f : 0.55f;
                if (_animator != null) _animator.PlayOneShot("attack", "run");
                float contactStr = Definition.Category == MonsterCategory.Regular ? Definition.Strength : Definition.Strength * (_isEnraged ? 1.55f : 1.30f);
                player.TakeDamageFromMonster(contactStr, Definition.DisplayName);
                if (Definition.AppliesBurn || Definition.AppliesBleed)
                {
                    player.ApplyMonsterStatusEffect(Definition.AppliesBurn, Definition.AppliesBleed);
                }
            }

            // Special / Ranged / Boss Skills
            _specialAttackTimer -= dt;
            if (_specialAttackTimer <= 0f && dist <= 13.5f)
            {
                ExecuteSpecialPattern(player, dir, dist);
            }
        }

        private void ExecuteSpecialPattern(PlayerController player, Vector2 dirToPlayer, float dist)
        {
            float baseCd = Definition.Category == MonsterCategory.FinalBoss ? 1.55f :
                           (Definition.Category == MonsterCategory.MiniBoss ? 2.0f : 4.2f);
            if (_isEnraged) baseCd *= 0.68f;
            _specialAttackTimer = baseCd;

            bool isBoss = Definition.Category != MonsterCategory.Regular;
            float bossDmg = Definition.Strength * (_isEnraged ? 1.45f : 1.25f);

            // 1. Charge Attack (Mọi Boss đều có thể xung kích tốc độ cao khi ở xa)
            if ((Definition.HasChargeAttack || isBoss) && dist > 1.8f && dist < 11.5f)
            {
                _chargeDir = dirToPlayer;
                _chargeDurationTimer = isBoss ? 0.55f : 0.45f;
                if (_animator != null) _animator.PlayOneShot("dash", "run");
                if (isBoss)
                {
                    FloatingTextPopup.Spawn(transform.position, _isEnraged ? "BẠO KÍCH CUỒNG NỘ!" : "XUNG KÍCH!", new Color(1f, 0.45f, 0.15f), 0.15f);
                }
            }

            // 2. Ranged Barrage (Boss bắn loạt đạn tỏa 5-8 hướng ép góc di chuyển của người chơi)
            if ((Definition.HasRangedAttack || isBoss) && dist < 12.5f)
            {
                if (_animator != null) _animator.PlayOneShot("attack", "run");
                int count = Definition.Category == MonsterCategory.FinalBoss ? (_isEnraged ? 8 : 5) :
                            (Definition.Category == MonsterCategory.MiniBoss ? (_isEnraged ? 5 : 3) : 1);
                float spreadStep = Definition.Category == MonsterCategory.FinalBoss && _isEnraged ? 45f : 18f;
                float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                for (int i = 0; i < count; i++)
                {
                    float offsetAngle = (i - (count - 1) * 0.5f) * spreadStep;
                    float rad = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                    Vector2 shootDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    CombatProjectile.SpawnEnemyProjectile(transform.position, shootDir, isBoss ? bossDmg : Definition.Strength, Definition.AppliesBurn, Definition.AppliesBleed);
                }
            }

            // 3. Boss Ground Shockwave AoE
            if ((Definition.HasAoEAttack || isBoss) && dist < 5.5f)
            {
                if (_animator != null) _animator.PlayOneShot("skill", "run");
                float shockRadius = Definition.Category == MonsterCategory.FinalBoss ? (_isEnraged ? 4.0f : 3.4f) : 2.6f;
                CombatAoEEffect.SpawnBossShockwave(transform.position, shockRadius, bossDmg * 1.2f, Definition.AppliesBurn, Definition.AppliesBleed);
            }

            // 4. Minion Summon
            if ((Definition.SummonsMinions || (Definition.Category == MonsterCategory.FinalBoss && _isEnraged)) && ActiveEnemies.Count < 35)
            {
                string minionId = Definition.StagesAppearing != null && Definition.StagesAppearing.Length > 0 && Definition.StagesAppearing[0] == 1 ? "goblin" :
                                  (Definition.StagesAppearing != null && Definition.StagesAppearing.Length > 0 && Definition.StagesAppearing[0] == 2 ? "vampire_bat" : "lava_slime");
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * 2.4f;
                    SpawnMonster(minionId, (Vector2)transform.position + offset, 1.1f);
                }
                FloatingTextPopup.Spawn(transform.position, "TRIỆU HỒI HỘ VỆ!", new Color(1f, 0.35f, 0.2f), 0.16f);
            }
        }

        public void ApplySlow(float slowPercent, float duration)
        {
            if (IsDead) return;
            _slowRatio = Mathf.Clamp(Mathf.Max(_slowRatio, slowPercent), 0f, 0.75f);
            _slowTimer = Mathf.Max(_slowTimer, duration);

            if (_freezeVfxObj == null)
            {
                _freezeVfxObj = new GameObject("FreezingVFX");
                _freezeVfxObj.transform.SetParent(transform, false);
                _freezeVfxObj.transform.localPosition = new Vector3(0f, 0.38f, 0f);
                _freezeVfxObj.transform.localScale = Vector3.one * 0.85f;
                SpriteRenderer sr = _freezeVfxObj.AddComponent<SpriteRenderer>();
                sr.sprite = ResourceCache.GetSprite("Sprites/fx_ice_burst", 48f);
                sr.color = new Color(0.65f, 0.95f, 1f, 0.9f);
                sr.sortingOrder = 2200;
            }
            _freezeVfxObj.SetActive(true);
            UpdateTintColor();
        }

        public void ApplyBurn(float dps, float duration)
        {
            if (IsDead) return;
            _burnDps = Mathf.Max(_burnDps, dps);
            _burnTimer = Mathf.Max(_burnTimer, duration);
            if (_burnTickTimer <= 0f) _burnTickTimer = 0.5f;

            if (_burnVfxObj == null)
            {
                _burnVfxObj = new GameObject("BurnVFX");
                _burnVfxObj.transform.SetParent(transform, false);
                _burnVfxObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                _burnVfxObj.transform.localScale = Vector3.one * 0.75f;
                SpriteRenderer sr = _burnVfxObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 2205;
                SpriteAnimator anim = _burnVfxObj.AddComponent<SpriteAnimator>();
                anim.Initialize("burn", 64f, 12f, false, true, "attack");
            }
            _burnVfxObj.SetActive(true);
            UpdateTintColor();
        }

        public void ApplyKnockback(Vector2 impulse)
        {
            float resist = Definition.Category == MonsterCategory.FinalBoss ? 0.25f :
                           (Definition.Category == MonsterCategory.MiniBoss ? 0.5f : 1.0f);
            _knockbackVel += impulse * resist;
        }

        public void TakeWeaponDamage(float rawWeaponDamage, bool isCritOrHeavy = false)
        {
            if (IsDead) return;

            // Boss Armor (Giáp Boss giảm sát thương nhận vào để trận đánh Boss kịch tính hơn)
            float armorMult = Definition.Category == MonsterCategory.FinalBoss ? 0.58f :
                              (Definition.Category == MonsterCategory.MiniBoss ? 0.68f : 1.0f);
            float finalDamage = Mathf.Max(1f, rawWeaponDamage * armorMult);
            CurrentHP -= finalDamage;

            // Kích hoạt trạng thái Cuồng Nộ (Enrage) khi Boss còn dưới 50% HP
            if (!_isEnraged && Definition.Category != MonsterCategory.Regular && CurrentHP > 0f && CurrentHP <= MaxHP * 0.5f)
            {
                _isEnraged = true;
                _specialAttackTimer = 0.2f;
                FloatingTextPopup.Spawn(transform.position + Vector3.up * 0.4f, "BOSS CUỒNG NỘ!", new Color(1f, 0.25f, 0.25f), 0.18f);
                if (GrindAndGloryManager.Instance != null)
                {
                    GrindAndGloryManager.Instance.ShowBannerMessage($"CẢNH BÁO: {Definition.DisplayName} ĐÃ VÀO TRẠNG THÁI CUỒNG NỘ (<50% HP)!\nTốc độ di chuyển, sát thương và tần suất tung chiêu tăng mạnh!");
                }
            }

            Color popCol = isCritOrHeavy ? new Color(1f, 0.85f, 0.2f) : Color.white;
            FloatingTextPopup.Spawn(transform.position, Mathf.RoundToInt(finalDamage).ToString(), popCol, isCritOrHeavy ? 0.16f : 0.13f);

            if (_hpBarFill != null)
            {
                float ratio = Mathf.Clamp01(CurrentHP / Mathf.Max(1f, MaxHP));
                float baseW = Definition.Category == MonsterCategory.Regular ? 0.85f : 1.8f;
                _hpBarFill.localScale = new Vector3(baseW * ratio, _hpBarFill.localScale.y, 1f);
                _hpBarFill.localPosition = new Vector3(-baseW * (1f - ratio) * 0.5f, 0f, 0f);
            }

            _flashTimer = 0.09f;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = new Color(1f, 0.45f, 0.45f, 1f);
            }

            if (CurrentHP <= 0f)
            {
                Die();
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("sfx_hit", 0.45f);
            }
        }

        private void UpdateTintColor()
        {
            if (_spriteRenderer == null) return;
            if (_slowTimer > 0f)
            {
                _spriteRenderer.color = new Color(0.55f, 0.85f, 1f, 1f);
            }
            else if (_burnTimer > 0f)
            {
                _spriteRenderer.color = new Color(1f, 0.72f, 0.45f, 1f);
            }
            else
            {
                _spriteRenderer.color = Color.white;
            }
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;
            ActiveEnemies.Remove(this);

            if (_hpBarRoot != null) Destroy(_hpBarRoot);
            if (_burnVfxObj != null) Destroy(_burnVfxObj);
            if (_freezeVfxObj != null) Destroy(_freezeVfxObj);
            if (_animator != null) _animator.Play("die", false, true);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySfx("sfx_kill", 0.65f);
            }

            // Spawn Drops according to Item Table in Section 3.1
            Vector2 pos = transform.position;
            if (Definition.Category == MonsterCategory.Regular)
            {
                // 100% drop energy_capsules
                DropItemController.SpawnDrop(pos, DropItemType.EnergyCapsule, Definition.ExpReward, Definition.GoldReward);

                // Random chance to drop redball (Cầu Sinh Lực 1 - 30% HP) or item_egg (Cầu Sinh Lực 2 - 50% HP + speed)
                float roll = UnityEngine.Random.value;
                if (roll < 0.08f)
                {
                    DropItemController.SpawnDrop(pos, DropItemType.RedBall, 0, 0);
                }
                else if (roll < 0.11f)
                {
                    DropItemController.SpawnDrop(pos, DropItemType.ItemEgg, 0, 0);
                }
            }
            else if (Definition.Category == MonsterCategory.MiniBoss)
            {
                // 100% drop energy_capsules + item_egg + 100% Rương Vũ Khí (item_box)
                DropItemController.SpawnDrop(pos, DropItemType.EnergyCapsule, Definition.ExpReward, Definition.GoldReward);
                DropItemController.SpawnDrop(pos + Vector2.right * 0.5f, DropItemType.ItemEgg, 0, 0);
                DropItemController.SpawnDrop(pos + Vector2.up * 0.4f, DropItemType.ItemBox, 0, Definition.GoldReward);
            }
            else if (Definition.Category == MonsterCategory.FinalBoss)
            {
                DropItemController.SpawnDrop(pos, DropItemType.EnergyCapsule, Definition.ExpReward, Definition.GoldReward);
            }

            if (GrindAndGloryManager.Instance != null)
            {
                GrindAndGloryManager.Instance.OnMonsterKilled(this);
            }

            Destroy(gameObject, 0.38f);
        }
    }
}