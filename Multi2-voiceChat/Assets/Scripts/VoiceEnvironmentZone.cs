using UnityEngine;

public class VoiceEnvironmentZone : MonoBehaviour
{
    public bool InSubmarine { get; private set; }
    void OnTriggerEnter(Collider other) { if (other.CompareTag("SubmarineZone")) InSubmarine = true; }
    void OnTriggerExit(Collider other) { if (other.CompareTag("SubmarineZone")) InSubmarine = false; }
}