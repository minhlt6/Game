using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace GrindAndGlory
{
    public static class ResourceCache
    {
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, List<Sprite>> _animCache = new Dictionary<string, List<Sprite>>();
        private static readonly Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();

        public static Sprite GetSprite(string resourcePath, float pixelsPerUnit = 64f, Vector2? customPivot = null)
        {
            if (string.IsNullOrEmpty(resourcePath)) return GetWhiteSprite();

            string cacheKey = $"{resourcePath}_{pixelsPerUnit}_{(customPivot.HasValue ? customPivot.Value.ToString() : "center")}";
            if (_spriteCache.TryGetValue(cacheKey, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Texture2D tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null)
            {
                tex = TryLoadTextureFromDisk(resourcePath);
            }
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                Vector2 pivot = customPivot ?? new Vector2(0.5f, 0.5f);
                Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, pixelsPerUnit);
                sp.name = resourcePath;
                _spriteCache[cacheKey] = sp;
                return sp;
            }

            Sprite direct = Resources.Load<Sprite>(resourcePath);
            if (direct != null)
            {
                if (direct.texture != null) direct.texture.filterMode = FilterMode.Point;
                _spriteCache[cacheKey] = direct;
                return direct;
            }

            return GetWhiteSprite();
        }

        public static Sprite GetWhiteSprite()
        {
            const string key = "__white_pixel__";
            if (_spriteCache.TryGetValue(key, out Sprite s) && s != null) return s;

            Texture2D tex = Resources.Load<Texture2D>("Sprites/ui_white");
            if (tex == null)
            {
                tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                Color[] cols = new Color[16];
                for (int i = 0; i < 16; i++) cols[i] = Color.white;
                tex.SetPixels(cols);
                tex.Apply();
            }
            tex.filterMode = FilterMode.Point;
            Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 8f);
            _spriteCache[key] = sp;
            return sp;
        }

        public static List<Sprite> GetAnimationFrames(string entityFolder, string stateName, float pixelsPerUnit = 64f, bool centerPivot = false)
        {
            if (string.IsNullOrEmpty(entityFolder)) return new List<Sprite>();
            string key = $"{entityFolder}/{stateName}_{pixelsPerUnit}_{(centerPivot ? "c" : "b")}";
            if (_animCache.TryGetValue(key, out List<Sprite> list) && list != null && list.Count > 0)
            {
                return list;
            }

            Vector2 pivot = centerPivot ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0.18f);
            list = LoadStateFramesInternal(entityFolder, stateName, pixelsPerUnit, pivot);

            // Fallback chain: requested state -> idle -> attack -> run
            if (list.Count == 0 && stateName != "idle")
            {
                list = LoadStateFramesInternal(entityFolder, "idle", pixelsPerUnit, pivot);
            }
            if (list.Count == 0 && stateName != "attack")
            {
                list = LoadStateFramesInternal(entityFolder, "attack", pixelsPerUnit, pivot);
            }
            if (list.Count == 0 && stateName != "run")
            {
                list = LoadStateFramesInternal(entityFolder, "run", pixelsPerUnit, pivot);
            }

            _animCache[key] = list;
            return list;
        }

        private static List<Sprite> LoadStateFramesInternal(string entityFolder, string stateName, float pixelsPerUnit, Vector2 pivot)
        {
            List<Sprite> list = new List<Sprite>();
            for (int i = 0; i < 20; i++)
            {
                string path = $"Animations/{entityFolder}/{stateName}_{i:D2}";
                Texture2D tex = Resources.Load<Texture2D>(path);
                if (tex == null)
                {
                    tex = TryLoadTextureFromDisk(path);
                }
                if (tex != null)
                {
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivot, pixelsPerUnit);
                    sp.name = $"{entityFolder}_{stateName}_{i:D2}";
                    list.Add(sp);
                    continue;
                }

                Sprite loadedSprite = Resources.Load<Sprite>(path);
                if (loadedSprite != null)
                {
                    if (loadedSprite.texture != null) loadedSprite.texture.filterMode = FilterMode.Point;
                    list.Add(loadedSprite);
                    continue;
                }
                break;
            }
            return list;
        }

        public static AudioClip GetAudio(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return null;
            if (_audioCache.TryGetValue(resourcePath, out AudioClip clip) && clip != null) return clip;
            clip = Resources.Load<AudioClip>(resourcePath);
            if (clip != null) _audioCache[resourcePath] = clip;
            return clip;
        }

        private static Texture2D TryLoadTextureFromDisk(string resourcePath)
        {
            string baseRes = Path.Combine(Application.dataPath, "Resources");
            string pngPath = Path.Combine(baseRes, resourcePath + ".png");
            string filePath = File.Exists(pngPath) ? pngPath : Path.Combine(baseRes, resourcePath + ".jpg");
            if (!File.Exists(filePath)) return null;

            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (ImageConversion.LoadImage(tex, bytes))
            {
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                return tex;
            }
            return null;
        }
    }

    public class SpriteAnimator : MonoBehaviour
    {
        public string EntityFolder = "normal_nuri";
        public float PixelsPerUnit = 64f;
        public float FramesPerSecond = 10f;
        public bool UseUnscaledTime = false;
        public bool CenterPivot = false;
        public bool DestroyOnComplete = false;

        private SpriteRenderer _spriteRenderer;
        private Image _uiImage;
        private string _currentState = "idle";
        private bool _loop = true;
        private List<Sprite> _frames = new List<Sprite>();
        private int _frameIndex;
        private float _timer;
        private string _fallbackState = "idle";

        public void Initialize(string entityFolder, float ppu = 64f, float fps = 10f, bool unscaled = false, bool centerPivot = false, string initialState = "idle")
        {
            EntityFolder = entityFolder;
            PixelsPerUnit = ppu;
            FramesPerSecond = fps;
            UseUnscaledTime = unscaled;
            CenterPivot = centerPivot;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _uiImage = GetComponent<Image>();
            Play(initialState, true, true);
        }

        public void Play(string stateName, bool loop = true, bool forceRestart = false)
        {
            if (!forceRestart && _currentState == stateName && _frames.Count > 0) return;

            _currentState = stateName;
            _loop = loop;
            _frames = ResourceCache.GetAnimationFrames(EntityFolder, stateName, PixelsPerUnit, CenterPivot);
            _frameIndex = 0;
            _timer = 0f;
            ApplyCurrentFrame();
        }

        public void PlayOneShot(string stateName, string returnState = "idle")
        {
            _fallbackState = returnState;
            Play(stateName, false, true);
        }

        private void Update()
        {
            if (_frames == null || _frames.Count <= 1) return;

            float dt = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += dt * FramesPerSecond;
            if (_timer >= 1f)
            {
                int step = Mathf.FloorToInt(_timer);
                _timer -= step;
                _frameIndex += step;
                if (_frameIndex >= _frames.Count)
                {
                    if (_loop)
                    {
                        _frameIndex %= _frames.Count;
                    }
                    else
                    {
                        if (DestroyOnComplete)
                        {
                            Destroy(gameObject);
                            return;
                        }
                        _frameIndex = _frames.Count - 1;
                        if (!string.IsNullOrEmpty(_fallbackState) && _currentState != _fallbackState)
                        {
                            Play(_fallbackState, true, true);
                            return;
                        }
                    }
                }
                ApplyCurrentFrame();
            }
        }

        private void ApplyCurrentFrame()
        {
            if (_frames == null || _frames.Count == 0) return;
            Sprite sp = _frames[Mathf.Clamp(_frameIndex, 0, _frames.Count - 1)];
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_uiImage == null) _uiImage = GetComponent<Image>();

            if (_spriteRenderer != null) _spriteRenderer.sprite = sp;
            if (_uiImage != null) _uiImage.sprite = sp;
        }
    }
}