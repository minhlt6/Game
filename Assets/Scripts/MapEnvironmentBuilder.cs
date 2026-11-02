using System.Collections.Generic;
using UnityEngine;

namespace GrindAndGlory
{
    public struct MapObstacle
    {
        public Vector2 Position;
        public float Radius;
        public GameObject VisualObject;
    }

    public class MapEnvironmentBuilder : MonoBehaviour
    {
        public static MapEnvironmentBuilder Instance { get; private set; }

        public Rect PlayableBounds = new Rect(-24f, -16f, 48f, 32f);
        public readonly List<MapObstacle> Obstacles = new List<MapObstacle>();

        private GameObject _mapRoot;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Tính sortingOrder chuẩn cho mọi thực thể (Nhân vật, Quái vật, Chướng ngại vật, Vật phẩm rơi)
        /// Đảm bảo luôn nằm trong dải [200..800], cao hơn hẳn mặt đất (-10000) và vòng Hào quang (-500)
        /// ở mọi tọa độ Y trên toàn bản đồ (tránh lỗi đi lên phía Bắc Y > 10 bị chìm dưới mặt đất).
        /// </summary>
        public static int GetEntitySortingOrder(float worldY, int offset = 0)
        {
            return 500 + Mathf.RoundToInt(-worldY * 12f) + offset;
        }

        public void BuildStageMap(int stageNumber)
        {
            if (_mapRoot != null)
            {
                Destroy(_mapRoot);
            }
            Obstacles.Clear();

            _mapRoot = new GameObject($"Stage_{stageNumber}_MapEnvironment");
            _mapRoot.transform.SetParent(transform, false);

            StageDefinition stage = GameDatabase.GetStage(stageNumber);
            Sprite mainGround = ResourceCache.GetSprite(stage.GroundSprite, 64f);
            Sprite pathGround = ResourceCache.GetSprite(stage.PathSprite, 64f);

            // 1. Tiled Ground Grid (-28..28, -20..20, step 2 units since 128px @ 64PPU = 2 units)
            GameObject groundContainer = new GameObject("GroundTiles");
            groundContainer.transform.SetParent(_mapRoot.transform, false);

            for (int gx = -15; gx <= 15; gx++)
            {
                for (int gy = -11; gy <= 11; gy++)
                {
                    Vector3 pos = new Vector3(gx * 2f, gy * 2f, 0f);
                    bool isPath = (Mathf.Abs(gx) <= 1 || Mathf.Abs(gy) <= 1 || (gx * gx + gy * gy <= 9));
                    GameObject tile = new GameObject($"Tile_{gx}_{gy}");
                    tile.transform.SetParent(groundContainer.transform, false);
                    tile.transform.position = pos;

                    SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite = isPath ? pathGround : mainGround;
                    sr.color = stage.AmbientTint;
                    sr.sortingOrder = isPath ? -9995 : -10000;
                }
            }

            // 2. Perimeter Boundary Obstacles (Tường đá, Rừng cây, Cột cổ giới hạn bản đồ)
            GameObject borderContainer = new GameObject("MapBoundaryWalls");
            borderContainer.transform.SetParent(_mapRoot.transform, false);

            string[] borderSprites = GetStagePropSprites(stageNumber, true);
            float minX = PlayableBounds.xMin - 1.2f;
            float maxX = PlayableBounds.xMax + 1.2f;
            float minY = PlayableBounds.yMin - 1.2f;
            float maxY = PlayableBounds.yMax + 1.2f;

            int idx = 0;
            for (float x = minX; x <= maxX; x += 2.0f)
            {
                CreateBoundaryProp(borderContainer.transform, new Vector2(x, maxY), borderSprites[idx++ % borderSprites.Length]);
                CreateBoundaryProp(borderContainer.transform, new Vector2(x, minY), borderSprites[idx++ % borderSprites.Length]);
            }
            for (float y = minY + 2.0f; y <= maxY - 2.0f; y += 2.0f)
            {
                CreateBoundaryProp(borderContainer.transform, new Vector2(minX, y), borderSprites[idx++ % borderSprites.Length]);
                CreateBoundaryProp(borderContainer.transform, new Vector2(maxX, y), borderSprites[idx++ % borderSprites.Length]);
            }

            // 3. Interior Natural Obstacles (Cây cối, Đá tảng, Cột cổ, Tường chắn - nhân vật không đi xuyên qua)
            GameObject obsContainer = new GameObject("NaturalObstacles");
            obsContainer.transform.SetParent(_mapRoot.transform, false);

            string[] propSprites = GetStagePropSprites(stageNumber, false);
            System.Random rng = new System.Random(stageNumber * 1337 + 42);

            // Deterministic symmetrical + organic obstacle positions leaving clean lanes and center spawn clear
            Vector2[] candidatePositions = new Vector2[]
            {
                new Vector2(-9f, 5.5f), new Vector2(9f, 5.5f),
                new Vector2(-9f, -5.5f), new Vector2(9f, -5.5f),
                new Vector2(-16f, 9f), new Vector2(16f, 9f),
                new Vector2(-16f, -9f), new Vector2(16f, -9f),
                new Vector2(0f, 9.5f), new Vector2(0f, -9.5f),
                new Vector2(-14f, 0f), new Vector2(14f, 0f),
                new Vector2(-5.5f, 11.5f), new Vector2(5.5f, 11.5f),
                new Vector2(-5.5f, -11.5f), new Vector2(5.5f, -11.5f),
                new Vector2(-19.5f, 4.5f), new Vector2(19.5f, 4.5f),
                new Vector2(-19.5f, -4.5f), new Vector2(19.5f, -4.5f)
            };

            for (int i = 0; i < candidatePositions.Length; i++)
            {
                Vector2 pos = candidatePositions[i];
                string spPath = propSprites[rng.Next(propSprites.Length)];
                CreateInteriorObstacle(obsContainer.transform, pos, spPath, i);
            }
        }

        private string[] GetStagePropSprites(int stageNumber, bool forBorder)
        {
            if (stageNumber == 1)
            {
                return forBorder
                    ? new[] { "Maps/stage1_prop_00", "Maps/stage1_prop_01", "Maps/stage1_prop_02", "Maps/stage1_prop_03", "Maps/stage1_prop_04" }
                    : new[] { "Maps/stage1_prop_01", "Maps/stage1_prop_03", "Maps/stage1_prop_04", "Maps/obstacle_03", "Maps/obstacle_09", "Maps/obstacle_10", "Maps/obstacle_14", "Maps/obstacle_24" };
            }
            if (stageNumber == 2)
            {
                return forBorder
                    ? new[] { "Maps/stage2_prop_00", "Maps/stage2_prop_01", "Maps/stage2_prop_02", "Maps/obstacle_07", "Maps/obstacle_11" }
                    : new[] { "Maps/stage2_prop_02", "Maps/stage2_prop_03", "Maps/obstacle_01", "Maps/obstacle_08", "Maps/obstacle_09", "Maps/obstacle_20", "Maps/obstacle_21", "Maps/obstacle_26" };
            }
            return forBorder
                ? new[] { "Maps/obstacle_00", "Maps/obstacle_22", "Maps/obstacle_23", "Maps/obstacle_28", "Maps/obstacle_29" }
                : new[] { "Maps/obstacle_00", "Maps/obstacle_01", "Maps/obstacle_05", "Maps/obstacle_23", "Maps/obstacle_24", "Maps/obstacle_28", "Maps/obstacle_35" };
        }

        private void CreateBoundaryProp(Transform parent, Vector2 pos, string spritePath)
        {
            GameObject go = new GameObject($"Border_{pos.x:0}_{pos.y:0}");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);

            Sprite sp = ResourceCache.GetSprite(spritePath, 64f, new Vector2(0.5f, 0.2f));
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = GetEntitySortingOrder(pos.y);
        }

        private void CreateInteriorObstacle(Transform parent, Vector2 pos, string spritePath, int index)
        {
            GameObject go = new GameObject($"Obstacle_{index:00}");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);

            Sprite sp = ResourceCache.GetSprite(spritePath, 64f, new Vector2(0.5f, 0.2f));
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = GetEntitySortingOrder(pos.y);

            // Normalize scale if very large prop
            if (sp != null && sp.rect.height > 180f)
            {
                go.transform.localScale = Vector3.one * 0.7f;
            }

            float radius = 0.72f;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = radius;

            Obstacles.Add(new MapObstacle
            {
                Position = pos,
                Radius = radius,
                VisualObject = go
            });
        }

        public Vector2 ClampAndResolveMovement(Vector2 desiredPos, float entityRadius = 0.42f)
        {
            // 1. Clamp to Map Bounds
            float x = Mathf.Clamp(desiredPos.x, PlayableBounds.xMin + entityRadius, PlayableBounds.xMax - entityRadius);
            float y = Mathf.Clamp(desiredPos.y, PlayableBounds.yMin + entityRadius, PlayableBounds.yMax - entityRadius);
            Vector2 resolved = new Vector2(x, y);

            // 2. Push out of any overlapping natural obstacles
            for (int i = 0; i < Obstacles.Count; i++)
            {
                MapObstacle obs = Obstacles[i];
                Vector2 diff = resolved - obs.Position;
                float minDist = obs.Radius + entityRadius;
                float distSq = diff.sqrMagnitude;
                if (distSq < minDist * minDist)
                {
                    float dist = Mathf.Sqrt(distSq);
                    Vector2 normal = dist > 0.0001f ? (diff / dist) : Vector2.up;
                    resolved = obs.Position + normal * minDist;
                }
            }

            // Final clamp
            resolved.x = Mathf.Clamp(resolved.x, PlayableBounds.xMin + entityRadius, PlayableBounds.xMax - entityRadius);
            resolved.y = Mathf.Clamp(resolved.y, PlayableBounds.yMin + entityRadius, PlayableBounds.yMax - entityRadius);
            return resolved;
        }
    }
}
