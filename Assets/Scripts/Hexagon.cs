using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private new Renderer _renderer;
    [SerializeField] private new Collider _collider;

    public HexStack HexStack { get; private set; }

    public Color Color 
    {
        get => _renderer.material.color;
        set => _renderer.material.color = value;
    }

    public void Configure(HexStack hexStack)
    {
        HexStack = hexStack;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void DisableCollider() => _collider.enabled = false;

    public void Vanish(float delay)
    {
        LeanTween.cancel(gameObject);

        LeanTween.scale(gameObject, Vector3.zero, MergeManager.AnimationDuration)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(delay)
            .setOnComplete(() => Destroy(gameObject));
    }

    public void MoveToLocal(Vector3 targetLocalPos, float delay)
    {
        LeanTween.cancel(gameObject);

        Vector3 startLocalPos = transform.localPosition;
        
        LeanTween.value(gameObject, 0f, 1f, MergeManager.AnimationDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay)
            .setOnUpdate((float t) => 
            {
                if (this == null) return;
                Vector3 currentPos = Vector3.Lerp(startLocalPos, targetLocalPos, t);
                
                float arcHeight = Mathf.Sin(t * Mathf.PI) * 1.25f;
                currentPos.y += arcHeight;
                
                transform.localPosition = currentPos;
            });

        Vector3 direction = (targetLocalPos - startLocalPos);
        direction.y = 0;
        direction.Normalize();
        
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);
        if (rotationAxis.sqrMagnitude < 0.01f) rotationAxis = Vector3.right; 

        LeanTween.rotateAround(gameObject, rotationAxis, 180, MergeManager.AnimationDuration)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay);
    }
}
