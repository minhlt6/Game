using System.Collections.Generic;
using UnityEngine;

namespace GrindAndGlory
{
    public class FloatingTextPopup : MonoBehaviour
    {
        private TextMesh _textMesh;
        private Vector3 _velocity;
        private float _lifetime = 0.75f;
        private float _timer;
        private Color _baseColor;

        public static void Spawn(Vector3 worldPos, string text, Color color, float fontSize = 0.14f)
        {
            GameObject go = new GameObject("PopupText");
            go.transform.position = worldPos + new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), 0.6f, -1f);
            go.transform.localScale = Vector3.one * fontSize;

            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 36;
            tm.characterSize = 0.25f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontStyle = FontStyle.Bold;
            tm.color = color;

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 5000;

            FloatingTextPopup pop = go.AddComponent<FloatingTextPopup>();
            pop._textMesh = tm;
            pop._baseColor = color;
            pop._velocity = new Vector3(UnityEngine.Random.Range(-0.4f, 0.4f), 1.6f, 0f);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += _velocity * Time.deltaTime;
            _velocity.y *= 0.96f;

            if (_textMesh != null)
            {
                float alpha = Mathf.Clamp01(1f - (_timer / _lifetime));
                _textMesh.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
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
            sr.sortingOrder = Mathf.RoundToInt(-spawnPos.y * 100f) - 5;

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

        // Dash/Charge state
        private float _chargeDurationTimer;
        private Vector2 _chargeDir;

        public static EnemyController SpawnMonster(string monsterId, Vector2 position, float difficultyScale = 1.0f)
        {
            if (!GameDatabase.Monsters.TryGetValue(monsterId, out MonsterDefinition def))
            {
                def = GameDatabase.Monsters["slime"];
            }

            GameObject go = new GameObject($"Enemy_{def.Id}");
            go.transform.position = new Vector3(position.x, position.y, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            SpriteAnimator anim = go.AddComponent<SpriteAnimator>();

            bool useSecondary = !string.IsNullOrEmpty(def.SecondaryAnimFolder) && UnityEngine.Random.value > 0.5f;
            string animFolder = useSecondary ? def.SecondaryAnimFolder : def.PrimaryAnimFolder;

            float ppu = def.Category == MonsterCategory.FinalBoss ? 46f :
                        (def.Category == MonsterCategory.MiniBoss ? 54f : 64f);
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

            // Movement + Knockback + Charge
            float speedMult = Mathf.Clamp(1.0f - _slowRatio, 0.25f, 1.0f);
            float moveSpeed = Definition.Speed * 0.85f * speedMult;

            if (_chargeDurationTimer > 0f)
            {
                _chargeDurationTimer -= dt;
                dir = _chargeDir;
                moveSpeed *= 2.2f;
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
                _spriteRenderer.sortingOrder = Mathf.RoundToInt(-desiredPos.y * 100f);
            }

            if (_animator != null && _chargeDurationTimer <= 0f)
            {
                _animator.Play("run", true, false);
            }

            // Contact Attack against Player
            float hitRange = Definition.Category == MonsterCategory.Regular ? 0.72f : 1.15f;
            if (dist <= hitRange && _contactCooldown <= 0f)
            {
                _contactCooldown = 0.75f;
                if (_animator != null) _animator.PlayOneShot("attack", "run");
                player.TakeDamageFromMonster(Definition.Strength, Definition.DisplayName);
                if (Definition.AppliesBurn || Definition.AppliesBleed)
                {
                    player.ApplyMonsterStatusEffect(Definition.AppliesBurn, Definition.AppliesBleed);
                }
            }

            // Special / Ranged / Boss Skills
            _specialAttackTimer -= dt;
            if (_specialAttackTimer <= 0f && dist <= 11f)
            {
                ExecuteSpecialPattern(player, dir, dist);
            }
        }

        private void ExecuteSpecialPattern(PlayerController player, Vector2 dirToPlayer, float dist)
        {
            _specialAttackTimer = Definition.Category == MonsterCategory.FinalBoss ? 2.6f :
                                  (Definition.Category == MonsterCategory.MiniBoss ? 3.2f : 4.2f);

            if (Definition.HasChargeAttack && dist > 1.8f && dist < 8.5f)
            {
                _chargeDir = dirToPlayer;
                _chargeDurationTimer = 0.45f;
                if (_animator != null) _animator.PlayOneShot("dash", "run");
                if (Definition.Category != MonsterCategory.Regular)
                {
                    FloatingTextPopup.Spawn(transform.position, "XUNG KÍCH!", new Color(1f, 0.5f, 0.2f), 0.14f);
                }
            }

            if (Definition.HasRangedAttack && dist < 9.5f)
            {
                if (_animator != null) _animator.PlayOneShot("attack", "run");
                int count = Definition.Category == MonsterCategory.FinalBoss ? 3 :
                            (Definition.Category == MonsterCategory.MiniBoss ? 2 : 1);
                float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                for (int i = 0; i < count; i++)
                {
                    float offsetAngle = (i - (count - 1) * 0.5f) * 18f;
                    float rad = (baseAngle + offsetAngle) * Mathf.Deg2Rad;
                    Vector2 shootDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                    CombatProjectile.SpawnEnemyProjectile(transform.position, shootDir, Definition.Strength, Definition.AppliesBurn, Definition.AppliesBleed);
                }
            }

            if (Definition.HasAoEAttack && dist < 4.5f)
            {
                if (_animator != null) _animator.PlayOneShot("skill", "run");
                CombatAoEEffect.SpawnBossShockwave(transform.position, Definition.Category == MonsterCategory.FinalBoss ? 3.2f : 2.3f, Definition.Strength * 1.15f, Definition.AppliesBurn, Definition.AppliesBleed);
            }

            if (Definition.SummonsMinions && ActiveEnemies.Count < 35)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * 2.2f;
                    SpawnMonster("lava_slime", (Vector2)transform.position + offset, 0.8f);
                }
                FloatingTextPopup.Spawn(transform.position, "TRIỆU HỒI ÁC QUỶ!", new Color(1f, 0.35f, 0.2f), 0.16f);
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
                _freezeVfxObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                _freezeVfxObj.transform.localScale = Vector3.one * 0.75f;
                SpriteRenderer sr = _freezeVfxObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 2200;
                SpriteAnimator anim = _freezeVfxObj.AddComponent<SpriteAnimator>();
                anim.Initialize("freezing", 64f, 12f, false, true, "attack");
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

            float finalDamage = Mathf.Max(1f, rawWeaponDamage);
            CurrentHP -= finalDamage;

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