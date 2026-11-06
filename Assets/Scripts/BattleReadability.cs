using UnityEngine;
namespace GrindAndGlory
{
    // Lightweight visual cues reuse the existing sprite and require no new materials.
    public sealed class BattleReadability : MonoBehaviour
    {
        private SpriteRenderer source;
        private SpriteRenderer shadow;
        private float nextEcho;
        private void Start()
        {
            source = GetComponent<SpriteRenderer>();
            var obj = new GameObject("GroundShadow");
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = new Vector3(0.08f, -0.12f, 0f);
            obj.transform.localScale = new Vector3(1f, 0.35f, 1f);
            shadow = obj.AddComponent<SpriteRenderer>();
            shadow.color = new Color(0f, 0f, 0f, 0.3f);
        }
        private void LateUpdate()
        {
            if (source == null || shadow == null) return;
            shadow.sprite = source.sprite;
            shadow.flipX = source.flipX;
            shadow.sortingOrder = source.sortingOrder - 1;
            var player = GetComponent<PlayerController>();
            if (player == null || GrindAndGloryManager.Instance == null || !GrindAndGloryManager.Instance.IsGameplayActive) return;
            float elapsed = player.DashCooldownDuration - player.DashCooldownRemaining;
            if (player.DashCooldownRemaining <= 0f || elapsed > 0.22f || Time.time < nextEcho) return;
            nextEcho = Time.time + 0.045f;
            var echo = new GameObject("DashEcho");
            echo.transform.SetPositionAndRotation(transform.position, transform.rotation);
            echo.transform.localScale = transform.lossyScale;
            var sprite = echo.AddComponent<SpriteRenderer>();
            sprite.sprite = source.sprite;
            sprite.flipX = source.flipX;
            sprite.sortingOrder = source.sortingOrder - 1;
            echo.AddComponent<DashEchoFade>().Initialize(sprite);
        }
    }
    public sealed class DashEchoFade : MonoBehaviour
    {
        private SpriteRenderer sprite;
        private float age;
        public void Initialize(SpriteRenderer value) { sprite = value; }
        private void Update()
        {
            age += Time.deltaTime;
            if (sprite != null) sprite.color = new Color(0.3f, 0.9f, 1f, Mathf.Max(0f, 0.38f * (1f - age / 0.25f)));
            if (age >= 0.25f) Destroy(gameObject);
        }
    }
}
