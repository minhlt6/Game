using System.Collections.Generic;
using UnityEngine;

namespace GrindAndGlory
{
    public class CombatProjectile : MonoBehaviour
    {
        public bool IsEnemyProjectile;
        public Vector2 Velocity;
        public float GravityY;
        public float SpinSpeed;
        public float Damage;
        public float HitRadius = 0.55f;
        public float Lifetime = 2.5f;
        public int PierceCount = 1;
        public float SlowPercent;
        public float SlowDuration;
        public bool AppliesBurnEffect;
        public bool AppliesBleedEffect;
        public bool IsTornadoVortex;
        public float VortexTickTimer;

        private float _age;
        private readonly HashSet<EnemyController> _hitEnemies = new HashSet<EnemyController>();

        public static void SpawnPlayerLinearProjectile(Vector2 startPos, Vector2 dir, float speed, float damage, string spritePath, int pierce = 2, float slowPct = 0f, float spinSpeed = 0f, float scale = 0.9f, string animFolder = "", bool appliesBurn = false)
        {
            GameObject go = new GameObject("PlayerProjectile");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * scale;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite(spritePath, 64f);
            sr.sortingOrder = 1500;

            if (!string.IsNullOrEmpty(animFolder))
            {
                SpriteAnimator anim = go.AddComponent<SpriteAnimator>();
                anim.Initialize(animFolder, 64f, 12f, false, true, "run");
            }

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = false;
            proj.Velocity = dir.normalized * speed;
            proj.Damage = damage;
            proj.PierceCount = pierce;
            proj.SlowPercent = slowPct;
            proj.SlowDuration = slowPct > 0f ? 2.5f : 0f;
            proj.AppliesBurnEffect = appliesBurn;
            proj.SpinSpeed = spinSpeed;
            proj.Lifetime = 2.2f;
        }

        public static void SpawnSpinningAxe(Vector2 startPos, Vector2 generalDir, int index, int totalCount, float damage)
        {
            GameObject go = new GameObject("SpinningAxe");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * 0.95f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/weapon_02_1", 64f);
            sr.sortingOrder = 1550;

            float spread = (index - (totalCount - 1) * 0.5f) * 2.2f;
            Vector2 initVel = new Vector2(generalDir.x * 4.5f + spread, 7.8f + Mathf.Abs(generalDir.y) * 1.5f);
            if (Mathf.Abs(initVel.x) < 1.2f) initVel.x = (index % 2 == 0 ? 1f : -1f) * 2.5f;

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = false;
            proj.Velocity = initVel;
            proj.GravityY = -12.5f;
            proj.SpinSpeed = 620f;
            proj.Damage = damage;
            proj.HitRadius = 0.72f;
            proj.PierceCount = 999; // Đa mục tiêu xuyên đám đông
            proj.Lifetime = 2.0f;
        }

        public static void SpawnHurricaneVortex(Vector2 startPos, Vector2 moveDir, float damage, float radiusScale)
        {
            GameObject go = new GameObject("HurricaneVortex");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * (0.95f * radiusScale);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/weapon_hurricane", 64f);
            sr.color = new Color(0.85f, 1f, 0.98f, 0.94f);
            sr.sortingOrder = 1480;

            SpriteAnimator anim = go.AddComponent<SpriteAnimator>();
            anim.Initialize("hurricane", 64f, 12f, false, true, "run");

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = false;
            proj.Velocity = moveDir.normalized * 3.8f;
            proj.SpinSpeed = 180f;
            proj.Damage = damage;
            proj.HitRadius = 1.45f * radiusScale;
            proj.IsTornadoVortex = true;
            proj.PierceCount = 999;
            proj.Lifetime = 2.6f;
        }

        public static void SpawnEnemyProjectile(Vector2 startPos, Vector2 dir, float monsterStrength, bool appliesBurn = false, bool appliesBleed = false)
        {
            GameObject go = new GameObject("EnemyProjectile");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * 0.8f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_proj_enemy", 64f);
            sr.sortingOrder = 1520;

            if (appliesBurn)
            {
                SpriteAnimator anim = go.AddComponent<SpriteAnimator>();
                anim.Initialize("flame", 64f, 12f, false, true, "attack");
            }

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = true;
            proj.Velocity = dir.normalized * 5.6f;
            proj.Damage = monsterStrength;
            proj.AppliesBurnEffect = appliesBurn;
            proj.AppliesBleedEffect = appliesBleed;
            proj.HitRadius = 0.42f;
            proj.Lifetime = 2.8f;
        }

        private void Update()
        {
            if (GrindAndGloryManager.Instance != null && !GrindAndGloryManager.Instance.IsGameplayActive) return;

            float dt = Time.deltaTime;
            _age += dt;
            if (_age >= Lifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (Mathf.Abs(GravityY) > 0.01f)
            {
                Velocity.y += GravityY * dt;
            }

            transform.position += (Vector3)(Velocity * dt);

            if (Mathf.Abs(SpinSpeed) > 0.01f)
            {
                transform.Rotate(0f, 0f, SpinSpeed * dt);
            }

            Vector2 pos = transform.position;

            if (IsEnemyProjectile)
            {
                PlayerController player = PlayerController.Instance;
                if (player != null && !player.IsDead)
                {
                    if (Vector2.Distance(pos, player.transform.position) <= HitRadius)
                    {
                        player.TakeDamageFromMonster(Damage, "Đạn Quái Vật");
                        if (AppliesBurnEffect || AppliesBleedEffect)
                        {
                            player.ApplyMonsterStatusEffect(AppliesBurnEffect, AppliesBleedEffect);
                        }
                        Destroy(gameObject);
                    }
                }
                return;
            }

            if (IsTornadoVortex)
            {
                VortexTickTimer -= dt;
                bool doDamageTick = VortexTickTimer <= 0f;
                if (doDamageTick) VortexTickTimer = 0.35f;

                var enemies = EnemyController.ActiveEnemies;
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    if (i >= enemies.Count) continue;
                    EnemyController e = enemies[i];
                    if (e == null || e.IsDead) continue;

                    Vector2 ePos = e.transform.position;
                    float dist = Vector2.Distance(pos, ePos);
                    if (dist <= HitRadius)
                    {
                        // Pull enemy gently toward vortex center
                        Vector2 pull = (pos - ePos).normalized * 2.2f;
                        e.ApplyKnockback(pull * dt * 8f);
                        if (doDamageTick)
                        {
                            e.TakeWeaponDamage(Damage * 0.45f, false);
                        }
                    }
                }
                return;
            }

            // Standard Player Projectile Collision
            var activeList = EnemyController.ActiveEnemies;
            for (int i = activeList.Count - 1; i >= 0; i--)
            {
                if (i >= activeList.Count) continue;
                EnemyController e = activeList[i];
                if (e == null || e.IsDead || _hitEnemies.Contains(e)) continue;

                float dist = Vector2.Distance(pos, e.transform.position);
                if (dist <= HitRadius + 0.35f)
                {
                    _hitEnemies.Add(e);
                    if (SlowPercent > 0f)
                    {
                        e.ApplySlow(SlowPercent, SlowDuration);
                    }
                    if (AppliesBurnEffect)
                    {
                        e.ApplyBurn(Damage * 0.18f, 2.5f);
                    }
                    e.TakeWeaponDamage(Damage, false);
                    PierceCount--;
                    if (PierceCount <= 0)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }
    }

    public class CombatAoEEffect : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private float _duration = 0.28f;
        private float _timer;
        private Color _startColor;
        private Vector3 _startScale;
        private Vector3 _endScale;

        public static void SpawnSlashArc(Vector2 origin, Vector2 facingDir, float range, float damage, bool isHeavyCleave, float knockbackStrength)
        {
            GameObject go = new GameObject(isHeavyCleave ? "HeavyCleaveSlash" : "LightSwordSlash");
            Vector2 dir = facingDir.sqrMagnitude > 0.001f ? facingDir.normalized : Vector2.right;
            go.transform.position = new Vector3(origin.x + dir.x * (range * 0.35f), origin.y + dir.y * (range * 0.35f), 0f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite(isHeavyCleave ? "Sprites/fx_slash_heavy" : "Sprites/fx_slash_light", 64f);
            sr.sortingOrder = 1600;

            CombatAoEEffect fx = go.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = isHeavyCleave ? 0.32f : 0.22f;
            fx._startColor = Color.white;
            float baseScale = range * (isHeavyCleave ? 1.15f : 0.95f);
            fx._startScale = Vector3.one * (baseScale * 0.75f);
            fx._endScale = Vector3.one * (baseScale * 1.15f);
            go.transform.localScale = fx._startScale;

            // Hit all enemies in front arc
            var enemies = EnemyController.ActiveEnemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i >= enemies.Count) continue;
                EnemyController e = enemies[i];
                if (e == null || e.IsDead) continue;

                Vector2 toEnemy = (Vector2)e.transform.position - origin;
                float dist = toEnemy.magnitude;
                if (dist <= range + 0.5f)
                {
                    float dot = dist > 0.01f ? Vector2.Dot(toEnemy / dist, dir) : 1f;
                    if (dot >= -0.25f || dist <= 1.1f)
                    {
                        if (knockbackStrength > 0f && dist > 0.01f)
                        {
                            e.ApplyKnockback((toEnemy / dist) * knockbackStrength);
                        }
                        e.TakeWeaponDamage(damage, isHeavyCleave);
                    }
                }
            }
        }

        public static void SpawnThunderboltStrike(Vector2 targetPos, float damage)
        {
            GameObject go = new GameObject("ThunderboltStrike");
            go.transform.position = new Vector3(targetPos.x, targetPos.y + 0.45f, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_lightning_bolt", 64f);
            sr.sortingOrder = 1800;

            SpriteAnimator anim = go.AddComponent<SpriteAnimator>();
            anim.Initialize("thunderbolt", 56f, 16f, false, true, "attack");

            CombatAoEEffect fx = go.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = 0.34f;
            fx._startColor = Color.white;
            fx._startScale = new Vector3(1.15f, 1.35f, 1f);
            fx._endScale = new Vector3(0.85f, 1.35f, 1f);
            go.transform.localScale = fx._startScale;

            var enemies = EnemyController.ActiveEnemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i >= enemies.Count) continue;
                EnemyController e = enemies[i];
                if (e == null || e.IsDead) continue;
                if (Vector2.Distance(targetPos, e.transform.position) <= 1.45f)
                {
                    e.TakeWeaponDamage(damage, true);
                }
            }
        }

        public static void SpawnBossShockwave(Vector2 center, float radius, float monsterStrength, bool appliesBurn = false, bool appliesBleed = false)
        {
            GameObject go = new GameObject("BossShockwave");
            go.transform.position = new Vector3(center.x, center.y, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_boss_aoe", 64f);
            sr.sortingOrder = -10;

            CombatAoEEffect fx = go.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = 0.45f;
            fx._startColor = Color.white;
            fx._startScale = Vector3.one * (radius * 0.4f);
            fx._endScale = Vector3.one * radius;
            go.transform.localScale = fx._startScale;

            PlayerController player = PlayerController.Instance;
            if (player != null && !player.IsDead)
            {
                if (Vector2.Distance(center, player.transform.position) <= radius + 0.2f)
                {
                    player.TakeDamageFromMonster(monsterStrength, "Chấn Động Boss");
                    if (appliesBurn || appliesBleed)
                    {
                        player.ApplyMonsterStatusEffect(appliesBurn, appliesBleed);
                    }
                }
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            float t = Mathf.Clamp01(_timer / _duration);
            transform.localScale = Vector3.Lerp(_startScale, _endScale, t);
            if (_sr != null)
            {
                _sr.color = new Color(_startColor.r, _startColor.g, _startColor.b, 1f - t);
            }
            if (_timer >= _duration)
            {
                Destroy(gameObject);
            }
        }
    }
}