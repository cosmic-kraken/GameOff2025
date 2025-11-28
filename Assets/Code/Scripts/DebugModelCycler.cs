using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class DebugModelCycler : MonoBehaviour
{
    public float padding = 1.2f;
    public TMPro.TextMeshProUGUI modelNameText;

    private List<Transform> models = new List<Transform>();
    private int currentIndex = 0;
    private Camera targetCamera;

    void Start()
    {
        targetCamera = Camera.main;

        // Collect all direct children as models
        models.Clear();
        foreach (Transform child in transform)
        {
            models.Add(child);
        }

        if (models.Count > 0)
        {
            ShowModel(0);
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CycleNext();
        }
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CyclePrevious();
        }
    }

    private void CycleNext()
    {
        if (models.Count == 0) return;

        currentIndex = (currentIndex + 1) % models.Count;
        ShowModel(currentIndex);
    }
    private void CyclePrevious()
    {
        if (models.Count == 0) return;

        currentIndex = (currentIndex - 1 + models.Count) % models.Count;
        ShowModel(currentIndex);
    }

    private void ShowModel(int index)
    {
        for (int i = 0; i < models.Count; i++)
        {
            models[i].gameObject.SetActive(i == index);
        }
        if (modelNameText != null)
        {
            modelNameText.text = models[index].name;
        }

        FrameModel(models[index].gameObject);
    }

    private void FrameModel(GameObject model)
    {
        if (targetCamera == null) return;

        Bounds bounds = CalculateBounds(model);
        PositionCameraForBounds(targetCamera, bounds, padding);
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        Bounds combined = new Bounds(obj.transform.position, Vector3.zero);

        foreach (var rend in renderers)
            combined.Encapsulate(rend.bounds);

        return combined;
    }

    private void PositionCameraForBounds(Camera cam, Bounds bounds, float pad)
    {
        float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float distance = (maxSize * pad) / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        Vector3 direction = cam.transform.forward;
        cam.transform.position = bounds.center - direction * distance;

        cam.transform.LookAt(bounds.center);
    }
}
