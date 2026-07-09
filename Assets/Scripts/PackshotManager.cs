using UnityEngine;

public class PackshotManager : MonoBehaviour
{
    public static PackshotManager Instance;

    [Header("UI Elements")]
    public CanvasGroup PackshotPanel;
    public RectTransform Logo;
    public RectTransform PlayNowButton;

    private bool _isGameEnded = false;
    
    private Vector3 _initialLogoScale = Vector3.one;
    private Vector3 _initialButtonScale = Vector3.one;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (Logo != null) _initialLogoScale = Logo.localScale;
        if (PlayNowButton != null) _initialButtonScale = PlayNowButton.localScale;

        if (PackshotPanel != null)
        {
            PackshotPanel.alpha = 0f;
            PackshotPanel.gameObject.SetActive(false);
            PackshotPanel.interactable = false;
            PackshotPanel.blocksRaycasts = false;
        }

        if (Logo != null) Logo.localScale = Vector3.zero;
        if (PlayNowButton != null) PlayNowButton.localScale = Vector3.zero;
    }

    public void ShowPackshot()
    {
        if (_isGameEnded) return;
        _isGameEnded = true;

        Debug.Log("Game Over! Showing Packshot. Luna.Unity.LifeCycle.GameEnded();");

        Luna.Unity.LifeCycle.GameEnded();

        if (PackshotPanel != null)
        {
            PackshotPanel.gameObject.SetActive(true);
            PackshotPanel.interactable = true;
            PackshotPanel.blocksRaycasts = true;

            LeanTween.alphaCanvas(PackshotPanel, 1f, 0.5f);

            if (Logo != null)
            {
                LeanTween.scale(Logo.gameObject, _initialLogoScale, 0.6f).setEase(LeanTweenType.easeOutBack).setDelay(0.2f);
            }

            if (PlayNowButton != null)
            {
                LeanTween.scale(PlayNowButton.gameObject, _initialButtonScale, 0.6f).setEase(LeanTweenType.easeOutBack).setDelay(0.4f).setOnComplete(() => 
                {
                    LeanTween.scale(PlayNowButton.gameObject, _initialButtonScale * 1.05f, 0.5f).setEase(LeanTweenType.easeInOutSine).setLoopPingPong();
                });
            }
        }
    }

    public void OnPackshotClicked()
    {
        Debug.Log("Redirecting to App Store! Luna.Unity.Playable.InstallFullGame()");

        Luna.Unity.Playable.InstallFullGame();
    }
}
