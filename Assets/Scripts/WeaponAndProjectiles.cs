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
        public float HitRadius = 0.58f;
        public float Lifetime = 2.5f;
        public int PierceCount = 1;
        public float SlowPercent;
        public float SlowDuration;
        public bool AppliesBurnEffect;
        public bool AppliesBleedEffect;
        public bool IsTornadoVortex;
        public float VortexTickTimer;
        public Transform SecondarySwirlLayer;

        private float _age;
        private readonly HashSet<EnemyController> _hitEnemies = new HashSet<EnemyController>();

        public static void SpawnPlayerLinearProjectile(
            Vector2 startPos,
            Vector2 dir,
            float speed,
            float damage,
            string spritePath,
            int pierce = 2,
            float slowPct = 0f,
            float spinSpeed = 0f,
            float scale = 1.05f,
            string animFolder = "",
            bool appliesBurn = false)
        {
            GameObject go = new GameObject("PlayerProjectile");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * scale;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite(spritePath, 48f);
            sr.sortingOrder = 1520;

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = false;
            proj.Velocity = dir.normalized * speed;
            proj.Damage = damage;
            proj.PierceCount = pierce;
            proj.SlowPercent = slowPct;
            proj.SlowDuration = slowPct > 0f ? 2.6f : 0f;
            proj.AppliesBurnEffect = appliesBurn;
            proj.SpinSpeed = spinSpeed;
            proj.Lifetime = 2.1f;
        }

        /// <summary>
        /// Bắn loạt đạn năng lượng xuyên thấu từ Súng Tiểu Liên (weapon_02_1) thay vì ném khẩu súng đi.
        /// </summary>
        public static void SpawnSpinningAxe(Vector2 startPos, Vector2 generalDir, int index, int totalCount, float damage)
        {
            Vector2 aimDir = generalDir.sqrMagnitude > 0.001f ? generalDir.normalized : Vector2.right;
            float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            float spreadAngle = (index - (totalCount - 1) * 0.5f) * 11f;
            float rad = (baseAngle + spreadAngle) * Mathf.Deg2Rad;
            Vector2 bulletDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject go = new GameObject("SmgPiercingBullet");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * 1.15f;
            go.transform.rotation = Quaternion.Euler(0f, 0f, baseAngle + spreadAngle);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_bullet_smg", 48f);
            sr.sortingOrder = 1550;

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = false;
            proj.Velocity = bulletDir * 12.8f;
            proj.GravityY = 0f;
            proj.SpinSpeed = 0f;
            proj.Damage = damage;
            proj.HitRadius = 0.65f;
            proj.PierceCount = 5; // Đạn xuyên nhiều mục tiêu
            proj.Lifetime = 1.8f;
        }

        public static void SpawnHurricaneVortex(Vector2 startPos, Vector2 moveDir, float damage, float radiusScale)
        {
            GameObject go = new GameObject("HurricaneVortex");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            float finalScale = 1.25f * radiusScale;
            go.transform.localScale = Vector3.one * finalScale;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_vortex", 48f);
            sr.color = new Color(0.85f, 1f, 0.98f, 0.95f);
            sr.sortingOrder = 1480;

            // Inner counter-rotating core for a vivid 2-layer cyclone effect
            GameObject inner = new GameObject("VortexInnerCore");
            inner.transform.SetParent(go.transform, false);
            inner.transform.localScale = Vector3.one * 0.62f;
            SpriteRenderer isr = inner.AddComponent<SpriteRenderer>();
            isr.sprite = ResourceCache.GetSprite("Sprites/fx_vortex", 48f);
            isr.color = new Color(1f, 1f, 1f, 0.9f);
            isr.sortingOrder = 1481;

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = false;
            proj.Velocity = moveDir.normalized * 3.8f;
            proj.SpinSpeed = 320f;
            proj.SecondarySwirlLayer = inner.transform;
            proj.Damage = damage;
            proj.HitRadius = 1.55f * radiusScale;
            proj.IsTornadoVortex = true;
            proj.PierceCount = 999;
            proj.Lifetime = 2.7f;
        }

        public static void SpawnEnemyProjectile(Vector2 startPos, Vector2 dir, float monsterStrength, bool appliesBurn = false, bool appliesBleed = false)
        {
            GameObject go = new GameObject("EnemyProjectile");
            go.transform.position = new Vector3(startPos.x, startPos.y, 0f);
            go.transform.localScale = Vector3.one * 0.95f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_proj_enemy", 48f);
            sr.color = appliesBurn ? new Color(1f, 0.55f, 0.2f, 1f) : new Color(1f, 0.35f, 0.65f, 1f);
            sr.sortingOrder = 1520;

            CombatProjectile proj = go.AddComponent<CombatProjectile>();
            proj.IsEnemyProjectile = true;
            proj.Velocity = dir.normalized * 5.6f;
            proj.SpinSpeed = 240f;
            proj.Damage = monsterStrength;
            proj.AppliesBurnEffect = appliesBurn;
            proj.AppliesBleedEffect = appliesBleed;
            proj.HitRadius = 0.44f;
            proj.Lifetime = 2.8f;
        }

        public static bool SegmentHitsCircle(Vector2 start, Vector2 end, Vector2 center, float radius)
        {
            Vector2 segment = end - start;
            float t = segment.sqrMagnitude > 0.000001f
                ? Mathf.Clamp01(Vector2.Dot(center - start, segment) / segment.sqrMagnitude) : 0f;
            return (center - (start + segment * t)).sqrMagnitude <= radius * radius;
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

            Vector2 previousPosition = transform.position;
            transform.position += (Vector3)(Velocity * dt);

            if (Mathf.Abs(SpinSpeed) > 0.01f)
            {
                transform.Rotate(0f, 0f, SpinSpeed * dt);
            }
            if (SecondarySwirlLayer != null)
            {
                SecondarySwirlLayer.Rotate(0f, 0f, -SpinSpeed * 1.6f * dt);
            }

            Vector2 pos = transform.position;

            if (IsEnemyProjectile)
            {
                PlayerController player = PlayerController.Instance;
                if (player != null && !player.IsDead)
                {
                    if (SegmentHitsCircle(previousPosition, pos, (Vector2)player.transform.position + Vector2.up * 0.35f, HitRadius + 0.3f))
                    {
                        player.TakeDamageFromMonster(Damage, "Đạn Quái Vật", AppliesBurnEffect, AppliesBleedEffect);
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
                        Vector2 pull = (pos - ePos).normalized * 2.4f;
                        e.ApplyKnockback(pull * dt * 8.5f);
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
                if (dist <= HitRadius + 0.38f)
                {
                    _hitEnemies.Add(e);
                    if (SlowPercent > 0f)
                    {
                        e.ApplySlow(SlowPercent, SlowDuration);
                        CombatAoEEffect.SpawnImpactBurst(e.transform.position, "Sprites/fx_ice_burst", new Color(0.65f, 0.95f, 1f, 0.95f), 0.7f);
                    }
                    else if (AppliesBurnEffect)
                    {
                        e.ApplyBurn(Damage * 0.18f, 2.5f);
                        CombatAoEEffect.SpawnImpactBurst(e.transform.position, "Sprites/fx_muzzle_flash", new Color(1f, 0.65f, 0.2f, 0.95f), 0.8f);
                    }
                    else
                    {
                        CombatAoEEffect.SpawnImpactBurst(e.transform.position, "Sprites/fx_muzzle_flash", new Color(0.8f, 0.95f, 1f, 0.9f), 0.65f);
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

        public static void SpawnMuzzleFlash(Vector2 muzzlePos, Vector2 aimDir, Color tint, float scale = 0.85f)
        {
            GameObject go = new GameObject("MuzzleFlashVFX");
            go.transform.position = new Vector3(muzzlePos.x, muzzlePos.y, 0f);
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_muzzle_flash", 48f);
            sr.color = tint;
            sr.sortingOrder = 1750;

            CombatAoEEffect fx = go.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = 0.11f;
            fx._startColor = tint;
            fx._startScale = Vector3.one * scale;
            fx._endScale = Vector3.one * (scale * 1.35f);
            go.transform.localScale = fx._startScale;
        }

        public static void SpawnImpactBurst(Vector2 hitPos, string spritePath, Color tint, float scale = 0.75f)
        {
            GameObject go = new GameObject("HitImpactVFX");
            go.transform.position = new Vector3(hitPos.x, hitPos.y + 0.25f, 0f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite(spritePath, 48f);
            sr.color = tint;
            sr.sortingOrder = 1720;

            CombatAoEEffect fx = go.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = 0.18f;
            fx._startColor = tint;
            fx._startScale = Vector3.one * (scale * 0.65f);
            fx._endScale = Vector3.one * (scale * 1.25f);
            go.transform.localScale = fx._startScale;
        }

        public static void SpawnSlashArc(Vector2 origin, Vector2 facingDir, float range, float damage, bool isHeavyCleave, float knockbackStrength)
        {
            GameObject go = new GameObject(isHeavyCleave ? "HeavyCleaveSlash" : "LightSwordSlash");
            Vector2 dir = facingDir.sqrMagnitude > 0.001f ? facingDir.normalized : Vector2.right;
            go.transform.position = new Vector3(origin.x + dir.x * (range * 0.38f), origin.y + dir.y * (range * 0.38f) + 0.2f, 0f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite(isHeavyCleave ? "Sprites/fx_slash_heavy" : "Sprites/fx_slash_light", 48f);
            sr.sortingOrder = 1650;

            CombatAoEEffect fx = go.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = isHeavyCleave ? 0.34f : 0.24f;
            fx._startColor = Color.white;
            float baseScale = range * (isHeavyCleave ? 0.95f : 0.85f);
            fx._startScale = Vector3.one * (baseScale * 0.75f);
            fx._endScale = Vector3.one * (baseScale * 1.18f);
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
                if (dist <= range + 0.55f)
                {
                    float dot = dist > 0.01f ? Vector2.Dot(toEnemy / dist, dir) : 1f;
                    if (dot >= -0.25f || dist <= 1.15f)
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
            // 1. Tall Lightning Bolt from the Sky
            GameObject bolt = new GameObject("ThunderboltStrike");
            bolt.transform.position = new Vector3(targetPos.x, targetPos.y + 1.05f, 0f);

            SpriteRenderer sr = bolt.AddComponent<SpriteRenderer>();
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_lightning_bolt", 48f);
            sr.sortingOrder = 1820;

            CombatAoEEffect fx = bolt.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = 0.36f;
            fx._startColor = Color.white;
            fx._startScale = new Vector3(1.35f, 1.45f, 1f);
            fx._endScale = new Vector3(0.75f, 1.45f, 1f);
            bolt.transform.localScale = fx._startScale;

            // 2. Expanding Electric Shockwave Ring on the Ground
            GameObject ring = new GameObject("ThunderboltGroundRing");
            ring.transform.position = new Vector3(targetPos.x, targetPos.y + 0.1f, 0f);
            SpriteRenderer rsr = ring.AddComponent<SpriteRenderer>();
            rsr.sprite = ResourceCache.GetSprite("Sprites/fx_thunder_ring", 48f);
            rsr.sortingOrder = 1810;

            CombatAoEEffect rfx = ring.AddComponent<CombatAoEEffect>();
            rfx._sr = rsr;
            rfx._duration = 0.36f;
            rfx._startColor = new Color(0.8f, 0.95f, 1f, 1f);
            rfx._startScale = Vector3.one * 0.55f;
            rfx._endScale = Vector3.one * 1.45f;
            ring.transform.localScale = rfx._startScale;

            var enemies = EnemyController.ActiveEnemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (i >= enemies.Count) continue;
                EnemyController e = enemies[i];
                if (e == null || e.IsDead) continue;
                if (Vector2.Distance(targetPos, e.transform.position) <= 1.55f)
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
            sr.sprite = ResourceCache.GetSprite("Sprites/fx_boss_aoe", 48f);
            sr.sortingOrder = -400; // Above ground (-10000), below entities (300..700)

            CombatAoEEffect fx = go.AddComponent<CombatAoEEffect>();
            fx._sr = sr;
            fx._duration = 0.45f;
            fx._startColor = Color.white;
            fx._startScale = Vector3.one * (radius * 0.4f);
            fx._endScale = Vector3.one * radius;
            go.transform.localScale = fx._startScale;

            fx._bossRadius = radius;
            fx._bossDamage = monsterStrength;
            fx._bossBurn = appliesBurn;
            fx._bossBleed = appliesBleed;
        }

        private float _bossRadius;
        private float _bossDamage;
        private bool _bossBurn, _bossBleed, _bossHit;
        private void Update()
        {
            if (GrindAndGloryManager.Instance != null && !GrindAndGloryManager.Instance.IsGameplayActive) return;
            _timer += Time.deltaTime;
            if (_bossRadius > 0f && !_bossHit)
            {
                var player = PlayerController.Instance;
                float waveRadius = _bossRadius * Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(_timer / _duration));
                if (player != null && !player.IsDead && Vector2.Distance(transform.position, player.transform.position) <= waveRadius + 0.3f)
                {
                    _bossHit = true;
                    player.TakeDamageFromMonster(_bossDamage, "Chấn Động Boss", _bossBurn, _bossBleed);
                }
            }
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
