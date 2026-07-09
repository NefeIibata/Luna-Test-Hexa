using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Elements")]
    public GameObject HandObject; 
    public Transform StartTransform; 
    public Transform EndTransform; 

    [Header("Settings")]
    public float TimeToShowTutorial = 3f; 
    public float AnimationDuration = 1f; 
    public Vector3 HeightOffset = new Vector3(0, 2f, -0.5f); 

    private float _afkTimer = 0f;
    private bool _isTutorialActive = false;
    private bool _hasCompletedTutorial = false;
    private int _tweenId;

    private void Start()
    {
        _afkTimer = TimeToShowTutorial;
    }

    private void OnEnable()
    {
        StackController.OnStackPlaced += OnStackPlaced;
    }

    private void OnDisable()
    {
        StackController.OnStackPlaced -= OnStackPlaced;
    }

    private void Update()
    {
        if (_hasCompletedTutorial) return;

        if (Input.GetMouseButton(0))
        {
            _afkTimer = 0f;
            if (_isTutorialActive)
            {
                HideTutorial();
            }
        }
        else
        {
            _afkTimer += Time.deltaTime;
            
            if (_afkTimer >= TimeToShowTutorial && !_isTutorialActive)
            {
                ShowTutorial();
            }
        }
    }

    private void ShowTutorial()
    {
        if (HandObject == null || StartTransform == null || EndTransform == null) return;

        _isTutorialActive = true;
        HandObject.SetActive(true);

        HandObject.transform.position = StartTransform.position + HeightOffset;

        _tweenId = LeanTween.move(HandObject, EndTransform.position + HeightOffset, AnimationDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setLoopPingPong()
            .id;
    }

    private void HideTutorial()
    {
        _isTutorialActive = false;
        if (HandObject != null)
        {
            HandObject.SetActive(false);
            LeanTween.cancel(_tweenId);
        }
    }

    private void OnStackPlaced(GridCell cell)
    {
        StopTutorial();
    }

    public void StopTutorial()
    {
        _hasCompletedTutorial = true;
        HideTutorial();
    }
}
