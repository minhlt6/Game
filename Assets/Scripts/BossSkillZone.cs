using UnityEngine;
namespace GrindAndGlory
{
    // Locked ground target: the hero can leave during the warning, then punish recovery.
    public sealed class BossSkillZone : MonoBehaviour
    {
        private EnemyController owner;
        private float radius, warning, age, nextTick;
        private bool fired;
        private LineRenderer ring;
        private Material material;
        private Color tint;
        private string skill;
        public static void Spawn(EnemyController boss, Vector2 target, float radius, float warning)
        {
            var obj = new GameObject("BossSkillWarning");
            obj.transform.position = target;
            var zone = obj.AddComponent<BossSkillZone>();
            zone.owner = boss;
            zone.radius = radius;
            zone.warning = warning;
            string id = boss.Definition.Id;
            zone.skill = id == "boss_goblin" ? "BẪY TRÓI CHÂN" : id == "boss_golem" ? "ĐỊA CHẤN" : id == "boss_bone_gargoyle" ? "MƯA XƯƠNG" : id == "boss_grim_reaper" ? "LƯỠI HÁI TỬ THẦN" : id == "boss_guardian_fire" ? "BIỂN LỬA" : "ẤN TỬ VONG";
            zone.tint = boss.Definition.AppliesBurn ? new Color(1f, 0.35f, 0.1f) : boss.Definition.AppliesBleed ? new Color(0.9f, 0.25f, 0.65f) : new Color(1f, 0.75f, 0.2f);
            zone.ring = obj.AddComponent<LineRenderer>();
            zone.material = new Material(Shader.Find("Sprites/Default"));
            zone.ring.sharedMaterial = zone.material;
            zone.ring.useWorldSpace = false;
            zone.ring.loop = true;
            zone.ring.widthMultiplier = 0.09f;
            zone.ring.positionCount = 64;
            zone.ring.sortingOrder = 1500;
            for (int i = 0; i < 64; i++)
            {
                float angle = i * Mathf.PI * 2f / 64;
                zone.ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius);
            }
            FloatingTextPopup.Spawn(target + Vector2.up, zone.skill + "!", zone.tint, 0.14f);
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
        private void Update()
        {
            var manager = GrindAndGloryManager.Instance;
            if (owner == null || owner.IsDead || manager == null || (manager.State != GameFlowState.Playing && manager.State != GameFlowState.Paused && manager.State != GameFlowState.LevelUpSelection)) { Destroy(gameObject); return; }
            if (!manager.IsGameplayActive) return;
            age += Time.deltaTime;
            ring.startColor = ring.endColor = new Color(tint.r, tint.g, tint.b, age < warning ? 0.4f + 0.5f * Mathf.PingPong(age * 4f, 1f) : 1f);
            if (age < warning) return;
            bool fire = owner.Definition.AppliesBurn;
            bool bleed = owner.Definition.AppliesBleed;
            if (!fired || (fire && age >= nextTick))
            {
                fired = true;
                nextTick = age + 0.85f;
                var player = PlayerController.Instance;
                if (player != null && !player.IsDead && Vector2.Distance(transform.position, player.transform.position) <= radius + 0.3f)
                {
                    // Apply slow before damage starts the hit grace period; dodge immunity still applies.
                    if (!fire && !bleed) player.ApplyBossSlow(1.5f);
                    player.TakeDamageFromMonster(owner.Definition.Strength * (fire ? 0.8f : 1.2f), skill, fire, bleed);
                }
                CombatAoEEffect.SpawnImpactBurst(transform.position, "Sprites/fx_boss_aoe", tint, radius);
            }
            if (age >= warning + (fire ? 2.6f : 0.3f)) Destroy(gameObject);
        }
    }
}
