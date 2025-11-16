using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }
    [SerializeField] private Transform playerTransform;
    [SerializeField] private bool dynamicDepth = true;

    [Header("Sun Light Settings for Dynamic Depth")]
    [SerializeField] private float maxSunLightIntensity = 2.0f;
    [SerializeField, Range(1f, 10f)] private float logScaling = 4.0f;
    private Light sunLight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // TODO: Add when player tag is set up
        // playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerTransform = Camera.main.transform;

        sunLight = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>();
        if (sunLight == null)
        {
            Debug.LogError("Sun Light not found in the scene. Please ensure there is a GameObject tagged 'Sun' with a Light component.");
        }
    }

    void Update()
    {
        if (dynamicDepth) updateDepth();
    }

    private void updateDepth()
    {
        if (sunLight == null || playerTransform == null)
        {
            return;
        }

        float playerHeight = playerTransform.position.y;

        float normalized = Mathf.Clamp01(playerHeight / 50f);
        float logValue = Mathf.Log10(normalized * logScaling + 1f); // balanced

        sunLight.intensity = logValue * maxSunLightIntensity;
        // Debug.Log($"Sun intensity updated to: {sunLight.intensity}");
    }
}
