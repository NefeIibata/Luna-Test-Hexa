using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    [Header("UI References")]
    public Image ProgressBarFill;
    public Image ProgressBarBackground;
    public RectTransform StopwatchIcon;
    public RectTransform StopwatchHand;
    public Image RedQuarterFill;

    [Header("Flashing Elements")]
    public Image StopwatchBackground; 
    public Image StopwatchKnob;       

    [Header("Settings")]
    public float TotalTime = 60f;
    public Gradient ColorGradient; 
    
    [Header("State")]
    public float CurrentTime;
    private bool _isTimerRunning = false;
    private bool _isFlashing = false;
    private bool _isTimeUp = false;
    
    private int _pulseTweenId;
    private int _flashTweenId;
    
    private void Start()
    {
        CurrentTime = TotalTime;
        UpdateUI();
        
        if (RedQuarterFill != null)
            RedQuarterFill.fillAmount = 0;
    }

    private void OnEnable()
    {
        StackController.OnStackPlaced += StartTimer;
    }

    private void OnDisable()
    {
        StackController.OnStackPlaced -= StartTimer;
    }

    private void StartTimer(GridCell cell)
    {
        StackController.OnStackPlaced -= StartTimer;
        _isTimerRunning = true;
    }

    private void Update()
    {
        if (!_isTimerRunning || _isTimeUp) return;

        CurrentTime -= Time.deltaTime;
        
        if (CurrentTime <= 0)
        {
            CurrentTime = 0;
            TimeUp();
        }

        UpdateUI();
        CheckAlarms();
    }

    private void UpdateUI()
    {
        float ratio = CurrentTime / TotalTime;
        
        if (ProgressBarFill != null)
        {
            ProgressBarFill.fillAmount = ratio;
            ProgressBarFill.color = ColorGradient.Evaluate(ratio);
        }

        if (StopwatchHand != null)
        {
            float angle = (1f - ratio) * 360f;
            StopwatchHand.localEulerAngles = new Vector3(0, 0, angle);
        }

        if (RedQuarterFill != null)
        {
            if (ratio <= 0.25f)
            {
                float quarterRatio = ratio / 0.25f; 
                RedQuarterFill.fillAmount = 0.25f * (1f - quarterRatio);
            }
            else
            {
                RedQuarterFill.fillAmount = 0;
            }
        }
    }

    private void CheckAlarms()
    {
        float ratio = CurrentTime / TotalTime;

        if (ratio <= 0.25f && !_isFlashing && !_isTimeUp)
        {
            StartFlashingAndPulsing();
        }
    }

    private void StartFlashingAndPulsing()
    {
        _isFlashing = true;

        if (StopwatchIcon != null)
        {
            _pulseTweenId = LeanTween.scale(StopwatchIcon.gameObject, Vector3.one * 1.25f, 0.4f)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong()
                .id;

            StopwatchIcon.localEulerAngles = new Vector3(0, 0, -4f);
            LeanTween.rotateZ(StopwatchIcon.gameObject, 4f, 0.4f)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong();
        }

        _flashTweenId = LeanTween.value(gameObject, 0f, 1f, 0.4f)
            .setLoopPingPong()
            .setOnUpdate((float val) => 
            {
                bool isRedPhase = val > 0.5f;
                Color targetColor = isRedPhase ? Color.red : Color.white;

                if (ProgressBarBackground != null)
                    ProgressBarBackground.color = targetColor;

                if (StopwatchBackground != null)
                    StopwatchBackground.color = targetColor;

                if (StopwatchKnob != null)
                    StopwatchKnob.color = targetColor;
            }).id;
    }

    private void TimeUp()
    {
        _isTimeUp = true;
        _isTimerRunning = false;

        LeanTween.cancel(_pulseTweenId);
        LeanTween.cancel(_flashTweenId);
        if (StopwatchIcon != null) LeanTween.cancel(StopwatchIcon.gameObject);

        if (ProgressBarBackground != null) ProgressBarBackground.color = Color.red;
        if (StopwatchBackground != null) StopwatchBackground.color = Color.red;
        if (StopwatchKnob != null) StopwatchKnob.color = Color.red;

        if (StopwatchIcon != null)
        {
            StopwatchIcon.localScale = Vector3.one;
            StopwatchIcon.localEulerAngles = new Vector3(0, 0, -8f);
            LeanTween.rotateZ(StopwatchIcon.gameObject, 8f, 0.1f)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong();
        }

        LeanTween.delayedCall(1.5f, () => 
        {
            if (PackshotManager.Instance != null)
                PackshotManager.Instance.ShowPackshot();
        });
    }
}
