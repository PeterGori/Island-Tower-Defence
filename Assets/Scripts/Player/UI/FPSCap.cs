using UnityEngine;

public class FPSCap : MonoBehaviour
{
    public int targetFrameRate;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }
}
