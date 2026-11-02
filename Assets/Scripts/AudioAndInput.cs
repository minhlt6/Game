using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GrindAndGlory
{
    public static class InputHelper
    {
        public static Vector2 GetMoveVector()
        {
            float x = 0f;
            float y = 0f;

#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Mathf.Approximately(x, 0f) && Mathf.Approximately(y, 0f))
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            }
#endif
            Vector2 v = new Vector2(x, y);
            return v.sqrMagnitude > 1f ? v.normalized : v;
        }

        public static bool IsDashPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Space))
                return true;
#endif
            return false;
        }

        public static bool IsPausePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && (kb.rKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame))
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Escape))
                return true;
#endif
            return false;
        }

        public static bool IsQuickUnlockPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && kb.f1Key.wasPressedThisFrame)
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.F1))
                return true;
#endif
            return false;
        }

        public static bool IsAutoAimTogglePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null && kb.tKey.wasPressedThisFrame)
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.T))
                return true;
#endif
            return false;
        }

        public static int GetUpgradeCardKeyThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) return 0;
                if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) return 1;
                if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame) return 2;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 2;
#endif
            return -1;
        }
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _bgmSource;
        private AudioSource _sfxSource;
        private string _currentBgmPath = "";

        public float BgmVolume { get; private set; } = 0.55f;
        public float SfxVolume { get; private set; } = 0.75f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BgmVolume = PlayerPrefs.GetFloat("GG_BgmVol", 0.55f);
            SfxVolume = PlayerPrefs.GetFloat("GG_SfxVol", 0.75f);

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.volume = BgmVolume;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
            _sfxSource.volume = SfxVolume;
        }

        public void SetBgmVolume(float vol)
        {
            BgmVolume = Mathf.Clamp01(vol);
            if (_bgmSource != null) _bgmSource.volume = BgmVolume;
            PlayerPrefs.SetFloat("GG_BgmVol", BgmVolume);
        }

        public void SetSfxVolume(float vol)
        {
            SfxVolume = Mathf.Clamp01(vol);
            if (_sfxSource != null) _sfxSource.volume = SfxVolume;
            PlayerPrefs.SetFloat("GG_SfxVol", SfxVolume);
        }

        public void PlayBgm(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath)) return;
            if (_currentBgmPath == resourcePath && _bgmSource != null && _bgmSource.isPlaying) return;

            AudioClip clip = ResourceCache.GetAudio(resourcePath);
            if (clip != null && _bgmSource != null)
            {
                _currentBgmPath = resourcePath;
                _bgmSource.clip = clip;
                _bgmSource.volume = BgmVolume;
                _bgmSource.Play();
            }
        }

        public void PlaySfx(string sfxName, float volumeScale = 1.0f)
        {
            if (_sfxSource == null || SfxVolume <= 0.01f) return;
            string path = sfxName.StartsWith("Audio/") ? sfxName : $"Audio/{sfxName}";
            AudioClip clip = ResourceCache.GetAudio(path);
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, Mathf.Clamp01(SfxVolume * volumeScale));
            }
        }
    }
}