// ================================================
// MaskSystem Visual - 战斗HUD
// ================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 战斗HUD - 显示战斗信息
    /// </summary>
    public class BattleHUD : MonoBehaviour
    {
        #region 玩家血量

        [Header("玩家血量")]
        [SerializeField] private Image playerHealthFill;
        [SerializeField] private Text playerHealthText;
        [SerializeField] private RectTransform playerHealthBar;

        #endregion

        #region 敌人血量

        [Header("敌人血量")]
        [SerializeField] private Image enemyHealthFill;
        [SerializeField] private Text enemyHealthText;
        [SerializeField] private Text enemyNameText;

        #endregion

        #region 预警

        [Header("预警指示")]
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private Image warningFill;
        [SerializeField] private Text warningText;
        [SerializeField] private CanvasGroup warningCanvasGroup;

        #endregion

        #region 关卡信息

        [Header("关卡信息")]
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text waveInfoText;
        [SerializeField] private GameObject levelTitlePanel;
        [SerializeField] private Text levelTitleText;
        [SerializeField] private Text levelDescText;

        #endregion

        #region 结算画面

        [Header("结算画面")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text resultText;

        #endregion

        #region 反击成功提示

        [Header("反击提示")]
        [SerializeField] private GameObject counterSuccessPanel;
        [SerializeField] private Text counterSuccessText;

        #endregion

        #region 私有字段

        private GameAssetsConfig _assets;
        private Coroutine _warningCoroutine;

        #endregion

        #region 初始化

        public void Initialize(GameAssetsConfig assets)
        {
            _assets = assets;

            // 隐藏所有弹出面板
            HideAllPanels();
        }

        private void HideAllPanels()
        {
            if (warningPanel != null) warningPanel.SetActive(false);
            if (levelTitlePanel != null) levelTitlePanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (counterSuccessPanel != null) counterSuccessPanel.SetActive(false);
        }

        #endregion

        #region 玩家血量

        public void UpdatePlayerHealth(int current, int max)
        {
            if (playerHealthFill != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                playerHealthFill.fillAmount = ratio;

                // 颜色渐变
                playerHealthFill.color = GetHealthColor(ratio);
            }

            if (playerHealthText != null)
            {
                playerHealthText.text = $"{current}/{max}";
            }

            // 低血量时血条闪烁
            if (current <= 1 && current > 0)
            {
                StartCoroutine(LowHealthPulse(playerHealthFill));
            }
        }

        #endregion

        #region 敌人血量

        public void UpdateEnemyHealth(int current, int max)
        {
            if (enemyHealthFill != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                enemyHealthFill.fillAmount = ratio;
                enemyHealthFill.color = GetHealthColor(ratio);
            }

            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{current}/{max}";
            }
        }

        public void SetEnemyName(string name)
        {
            if (enemyNameText != null)
            {
                enemyNameText.text = name;
            }
        }

        #endregion

        #region 预警

        public void ShowWarning()
        {
            if (warningPanel != null)
            {
                warningPanel.SetActive(true);

                if (_warningCoroutine != null)
                {
                    StopCoroutine(_warningCoroutine);
                }
                _warningCoroutine = StartCoroutine(WarningAnimation());
            }

            if (warningText != null)
            {
                warningText.text = "按 SPACE 反击!";
            }
        }

        public void UpdateWarningProgress(float progress)
        {
            if (warningFill != null)
            {
                warningFill.fillAmount = progress;
                warningFill.color = Color.Lerp(Color.yellow, Color.red, progress);
            }
        }

        public void HideWarning()
        {
            if (_warningCoroutine != null)
            {
                StopCoroutine(_warningCoroutine);
                _warningCoroutine = null;
            }

            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }
        }

        private IEnumerator WarningAnimation()
        {
            float time = 0f;
            float pulseSpeed = 8f;

            while (true)
            {
                time += Time.deltaTime;

                if (warningCanvasGroup != null)
                {
                    float alpha = (Mathf.Sin(time * pulseSpeed) + 1f) * 0.25f + 0.5f;
                    warningCanvasGroup.alpha = alpha;
                }

                if (warningText != null)
                {
                    float scale = 1f + Mathf.Sin(time * pulseSpeed) * 0.1f;
                    warningText.transform.localScale = Vector3.one * scale;
                }

                yield return null;
            }
        }

        #endregion

        #region 关卡信息

        public void SetLevelInfo(string levelName, int waveIndex, int totalWaves)
        {
            if (levelNameText != null)
            {
                levelNameText.text = levelName;
            }

            if (waveInfoText != null)
            {
                waveInfoText.text = $"波次 {waveIndex + 1}/{totalWaves}";
            }
        }

        public void ShowLevelTitle(string title, string description)
        {
            if (levelTitlePanel != null)
            {
                levelTitlePanel.SetActive(true);

                if (levelTitleText != null)
                {
                    levelTitleText.text = title;
                }

                if (levelDescText != null)
                {
                    levelDescText.text = description;
                }

                StartCoroutine(HideLevelTitleAfterDelay(3f));
            }
        }

        private IEnumerator HideLevelTitleAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (levelTitlePanel != null)
            {
                // 淡出
                var canvasGroup = levelTitlePanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    float duration = 0.5f;
                    float elapsed = 0f;

                    while (elapsed < duration)
                    {
                        elapsed += Time.deltaTime;
                        canvasGroup.alpha = 1 - (elapsed / duration);
                        yield return null;
                    }

                    canvasGroup.alpha = 1f;
                }

                levelTitlePanel.SetActive(false);
            }
        }

        #endregion

        #region 结算

        public void ShowVictory()
        {
            HideAllPanels();

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                StartCoroutine(VictoryAnimation());
            }

            if (resultText != null)
            {
                resultText.text = "恭喜通关!\n按 R 重新挑战";
            }
        }

        public void ShowGameOver()
        {
            HideAllPanels();

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (resultText != null)
            {
                resultText.text = "游戏结束\n按 R 重新开始";
            }
        }

        private IEnumerator VictoryAnimation()
        {
            if (victoryPanel == null) yield break;

            var canvasGroup = victoryPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = victoryPanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = elapsed / duration;
                victoryPanel.transform.localScale = Vector3.one * (0.8f + 0.2f * (elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = 1f;
            victoryPanel.transform.localScale = Vector3.one;
        }

        #endregion

        #region 反击成功

        public void ShowCounterSuccess()
        {
            if (counterSuccessPanel != null)
            {
                counterSuccessPanel.SetActive(true);
                StartCoroutine(CounterSuccessAnimation());
            }
        }

        private IEnumerator CounterSuccessAnimation()
        {
            if (counterSuccessPanel == null) yield break;

            if (counterSuccessText != null)
            {
                counterSuccessText.text = "反击成功!";
            }

            // 放大然后缩小淡出
            float duration = 0.8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.3f;
                counterSuccessPanel.transform.localScale = Vector3.one * scale;

                var canvasGroup = counterSuccessPanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t < 0.7f ? 1f : 1f - ((t - 0.7f) / 0.3f);
                }

                yield return null;
            }

            counterSuccessPanel.SetActive(false);
            counterSuccessPanel.transform.localScale = Vector3.one;
        }

        #endregion

        #region 辅助方法

        private Color GetHealthColor(float ratio)
        {
            if (ratio > 0.5f)
                return Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2);
            else
                return Color.Lerp(Color.red, Color.yellow, ratio * 2);
        }

        private IEnumerator LowHealthPulse(Image healthBar)
        {
            if (healthBar == null) yield break;

            float duration = 0.5f;
            float elapsed = 0f;
            Color originalColor = healthBar.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin(elapsed * 20f);
                healthBar.color = Color.Lerp(originalColor, Color.white, t * 0.5f);
                yield return null;
            }

            healthBar.color = originalColor;
        }

        #endregion
    }
}

