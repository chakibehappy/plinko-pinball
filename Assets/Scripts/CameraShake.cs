using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.2f; // Duration of the shake
    public float shakeStrength = 0.5f; // Strength of the shake
    public int vibrato = 2; // Number of shakes
    public float randomness = 90f; // Randomness of shake direction

    private Camera mainCamera;
    private Vector3 initialPosition;

    void Start()
    {
        mainCamera = Camera.main;
        initialPosition = mainCamera.transform.position;
    }

    public void Shake()
    {
        mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato, randomness)
            .OnKill(() => mainCamera.transform.position = initialPosition); // Reset camera position after shake
    }
}
