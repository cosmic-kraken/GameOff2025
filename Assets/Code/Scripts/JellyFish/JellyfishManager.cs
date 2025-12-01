using UnityEngine;
using System.Collections;

public class JellyfishManager : MonoBehaviour
{
    public void ScheduleRespawn(GameObject obj, float delay)
    {
        StartCoroutine(RespawnRoutine(obj, delay));
    }
    private IEnumerator RespawnRoutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Re-enable object
        obj.SetActive(true);
    }
}
