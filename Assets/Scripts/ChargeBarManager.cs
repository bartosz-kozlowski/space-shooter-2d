using UnityEngine;

public class ChargeBarManager : MonoBehaviour
{
    [SerializeField] Sprite[][] chargeBarFrames;
    [SerializeField] Sprite[] blueFrames;
    [SerializeField] Sprite[] orangeFrames;
    [SerializeField] float maxChargeTime = 1.5f;

    SpriteRenderer spriteRenderer;
    CharSelectManager charSelectManager;
    Sprite[] currentFrames;
    bool isCharging;
    float chargeTimer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        charSelectManager = FindFirstObjectByType<CharSelectManager>();
    }

    void Start()
    {
        Sprite[][] allFrames = { blueFrames, orangeFrames };
        int index = charSelectManager.GetCurrentShipIndex();
        currentFrames = allFrames[index];
        spriteRenderer.enabled = false;
    }

    void Update()
    {
        if (!isCharging) return;

        chargeTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(chargeTimer / maxChargeTime);
        int frameIndex = Mathf.Min(Mathf.FloorToInt(progress * currentFrames.Length), currentFrames.Length - 1);
        spriteRenderer.sprite = currentFrames[frameIndex];
    }

    public void PlayChargeBarAnimation()
    {
        isCharging = true;
        chargeTimer = 0f;
        spriteRenderer.enabled = true;
    }

    public void StopChargeBarAnimation()
    {
        isCharging = false;
        spriteRenderer.enabled = false;
    }
}
