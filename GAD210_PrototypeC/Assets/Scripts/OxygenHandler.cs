using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class OxygenHandler : MonoBehaviour
{
    public delegate void OxygenDepletedDelegate();

    [SerializeField] [Range(0.001f, 1)] private float depletionRate;
    [SerializeField] [Range(0.001f, 1)] private float rechargeRate;
    [SerializeField] [Range(0.1f, 10)] private float targetFocalDistance = 0.1f;
    [SerializeField] [Range(0.001f, 1)] private float targetVignetteIntensity = 0.75f;
    [SerializeField] [Range(0.001f, 1)] private float targetVignetteSmoothness = 1.0f;
    [SerializeField] [Range(-100f, 100f)] private float targetGradingSaturation = -100.00f;
    [SerializeField] [Range(-100f, 100f)] private float targetGradingContrast = 100.0f;
    [SerializeField] private Color targetColourFilter = Color.black;
    [SerializeField] private AnimationCurve interpolationCurve;
    [SerializeField] private AnimationCurve colourInterpolationCurve;
    [SerializeField] private PostProcessVolume volume;
    [SerializeField] private float normalBreathingTime = 1.5f, deepBreathingTime = 1.0f, heavyBreathingTime = 0.8f, labouredBreathingTime = 0.5f;
    [SerializeField] private Vector2 normalBreathingBounds = new Vector2(0.7f, 1.0f), deepBreathingBounds = new Vector2(0.4f, 0.7f), heavyBreathingBounds = new Vector2(0.15f, 0.4f), labouredBreathingBounds = new Vector2(0f, 0.15f);
    [SerializeField] private AudioClip[] normalBreathingClips;
    [SerializeField] private AudioClip[] deepBreathingClips;
    [SerializeField] private AudioClip[] heavyBreathingClips;
    [SerializeField] private AudioClip[] labouredBreathingClips;
    [SerializeField] private AudioClip addOxygenClip;

    private float currentOxygen = 1.0f;
    private float defaultVignetteIntensity;
    private float defaultVignetteSmoothness;
    private float defaultFocalDistance;
    private float defaultSaturation = 0;
    private float defaultContrast = 0;
    private float breathTimer = -1f;
    private Color defaultColorFilter;
    private bool depleting = false;
    private Vignette pp_vignette;
    private DepthOfField pp_DOF;
    private ColorGrading pp_ColourGrading;
    private AudioSource aSrc;

    private event OxygenDepletedDelegate OxygenDepletedEvent;

    private void Awake()
    {
        if(volume.profile.TryGetSettings(out pp_vignette) == true)
        {
            defaultVignetteIntensity = pp_vignette.intensity.value;
            defaultVignetteSmoothness = pp_vignette.smoothness.value;
        }
        if(volume.profile.TryGetSettings(out pp_DOF) == true)
        {
            defaultFocalDistance = pp_DOF.focusDistance.value;
        }
        if(volume.profile.TryGetSettings(out pp_ColourGrading) == true)
        {
            defaultSaturation = pp_ColourGrading.saturation.value;
            defaultContrast = pp_ColourGrading.contrast.value;
            defaultColorFilter = pp_ColourGrading.colorFilter.value;
        }
        if (TryGetComponent(out aSrc) == true)
        {
        }
        breathTimer = normalBreathingTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (depleting == true)
        {
            if (currentOxygen > 0)
            {
                currentOxygen -= Time.deltaTime * depletionRate;
                if (currentOxygen <= 0f)
                {
                    currentOxygen = 0f;
                    OxygenDepletedEvent?.Invoke();
                }
                float curvedOxygen = interpolationCurve.Evaluate(1.0f - currentOxygen);
                float curvedColourFilter = colourInterpolationCurve.Evaluate(1.0f - currentOxygen);
                pp_DOF.focusDistance.value = defaultFocalDistance + (targetFocalDistance - defaultFocalDistance) * curvedOxygen;
                pp_vignette.intensity.value = defaultVignetteIntensity + (targetVignetteIntensity - defaultVignetteIntensity) * curvedOxygen;
                pp_vignette.smoothness.value = defaultVignetteSmoothness + (targetVignetteSmoothness - defaultVignetteSmoothness) * curvedOxygen;
                pp_ColourGrading.saturation.value = defaultSaturation + (targetGradingSaturation - defaultSaturation) * curvedOxygen;
                pp_ColourGrading.contrast.value = defaultContrast + (targetGradingContrast - defaultContrast) * curvedOxygen;
                pp_ColourGrading.colorFilter.value = defaultColorFilter + (targetColourFilter - defaultColorFilter) * curvedColourFilter;
            }
        }
        else if (currentOxygen < 1f)
        {
            currentOxygen += Time.deltaTime * rechargeRate;
            if (currentOxygen > 1f)
            {
                currentOxygen = 1f;
            }
            float curvedOxygen = interpolationCurve.Evaluate(1.0f - currentOxygen);
            float curvedColourFilter = colourInterpolationCurve.Evaluate(1.0f - currentOxygen);
            pp_DOF.focusDistance.value = defaultFocalDistance + (targetFocalDistance - defaultFocalDistance) * curvedOxygen;
            pp_vignette.intensity.value = defaultVignetteIntensity + (targetVignetteIntensity - defaultVignetteIntensity) * curvedOxygen;
            pp_vignette.smoothness.value = defaultVignetteSmoothness + (targetVignetteSmoothness - defaultVignetteSmoothness) * curvedOxygen;
            pp_ColourGrading.saturation.value = defaultSaturation + (targetGradingSaturation - defaultSaturation) * curvedOxygen;
            pp_ColourGrading.contrast.value = defaultContrast + (targetGradingContrast - defaultContrast) * curvedOxygen;
            pp_ColourGrading.colorFilter.value = defaultColorFilter + (targetColourFilter - defaultColorFilter) * curvedColourFilter;
        }
        if(breathTimer >= 0)
        {
            breathTimer += Time.deltaTime;
            if(currentOxygen > normalBreathingBounds.x && currentOxygen <= normalBreathingBounds.y && breathTimer >= normalBreathingTime)
            {
                aSrc.PlayOneShot(normalBreathingClips[Random.Range(0, normalBreathingClips.Length)]);
                breathTimer = 0f;
            }
            else if(currentOxygen > deepBreathingBounds.x && currentOxygen <= deepBreathingBounds.y && breathTimer >= deepBreathingTime)
            {
                aSrc.PlayOneShot(deepBreathingClips[Random.Range(0, deepBreathingClips.Length)]);
                breathTimer = 0f;
            }
            else if(currentOxygen > heavyBreathingBounds.x && currentOxygen <= heavyBreathingBounds.y && breathTimer >= heavyBreathingTime)
            {
                aSrc.PlayOneShot(heavyBreathingClips[Random.Range(0, heavyBreathingClips.Length)]);
                breathTimer = 0f;
            }
            else if(currentOxygen >= labouredBreathingBounds.x && currentOxygen <= labouredBreathingBounds.y && breathTimer >= labouredBreathingTime)
            {
                aSrc.PlayOneShot(labouredBreathingClips[Random.Range(0, labouredBreathingClips.Length)]);
                breathTimer = 0f;
            }
        }
    }

    public void AddOxygen(float amount)
    {
        if (amount > 0)
        {
            currentOxygen += amount;
            if (currentOxygen > 1)
            {
                currentOxygen = 1;
            }
            if(aSrc != null) { aSrc.PlayOneShot(addOxygenClip); }
        }
    }

    public void ToggleDepleting(bool toggle)
    {
        depleting = toggle;
    }

    public void SetSpawnTarget(OxygenDepletedDelegate eventTarget)
    {
        OxygenDepletedEvent = eventTarget;
    }
}
