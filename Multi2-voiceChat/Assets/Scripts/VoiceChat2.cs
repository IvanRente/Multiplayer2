using System.Collections;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;

public class VoiceChat2 : NetworkBehaviour
{
    public enum ChatType { Global, Proximity }
    public ChatType VoiceChatType = ChatType.Global;

    public enum DetectionType { PushToTalk, VoiceActivation }
    public DetectionType VoiceDetectionType = DetectionType.PushToTalk;

    public enum VoiceMode { Proximity = 0, Submarine = 1, Radio = 2 }

    [Header("General")]
    public bool Activated = true;
    public KeyCode PushToTalkKey = KeyCode.V;

    [Header("Audio")]
    public AudioSource source;
    public float proximityRange = 10f;
    public float voiceActivationThreshold = 0.002f;

    private bool canTalk = true;
    private bool previousCanTalk = false;

    private string deviceName;
    private const int sampleRate = 48000;
    private const int bufferSize = 16384;

    private float[] audioBuffer;
    private int position;
    private AudioClip microphoneClip;

    private float[] sampleData;
    private float[] micDataBuffer;

    private WalkieTalkieToggle _radio;
    private VoiceEnvironmentZone _env;

    private AudioLowPassFilter _lpf;
    private AudioHighPassFilter _hpf;
    private AudioEchoFilter _echo;
    private AudioReverbFilter _reverb;
    private AudioDistortionFilter _dist;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsOwner)
            return;

        if (source == null)
            Debug.LogError("[VOICE] AudioSource not assigned!");

        deviceName = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (string.IsNullOrEmpty(deviceName))
            Debug.LogError("[VOICE] No microphone device found!");

        audioBuffer = new float[bufferSize];
        sampleData = new float[bufferSize];
        micDataBuffer = new float[bufferSize];
        source.playOnAwake = false;

        _radio = GetComponent<WalkieTalkieToggle>();
        _env = GetComponent<VoiceEnvironmentZone>();
    }

    void Update()
    {
        if (!Activated || !IsOwner)
            return;

        string selectedDevice = MicrophoneManager.Instance.GetCurrentDeviceName();
        if (selectedDevice != deviceName)
            UpdateMicrophone(selectedDevice);

        switch (VoiceDetectionType)
        {
            case DetectionType.PushToTalk:
                canTalk = Input.GetKey(PushToTalkKey);
                if (canTalk && microphoneClip == null)
                {
                    StartMicrophone();
                    StartTalking();
                }
                else if (!canTalk && microphoneClip != null)
                {
                    StopTalking();
                    StopMicrophone();
                }
                break;

            case DetectionType.VoiceActivation:
                if (microphoneClip == null)
                    StartMicrophone();
                canTalk = IsVoiceActivated();
                break;
        }

        if (!previousCanTalk && canTalk)
            StartTalking();

        if (previousCanTalk && !canTalk)
            StopTalking();

        previousCanTalk = canTalk;
    }

    private void StartMicrophone()
    {
        if (string.IsNullOrEmpty(deviceName))
            return;

        position = 0;
        microphoneClip = Microphone.Start(deviceName, true, 10, sampleRate);
    }

    private void StopMicrophone()
    {
        if (string.IsNullOrEmpty(deviceName))
            return;

        Microphone.End(deviceName);
        microphoneClip = null;
    }

    private void UpdateMicrophone(string newDeviceName)
    {
        if (!string.IsNullOrEmpty(deviceName))
        {
            Debug.Log($"[VOICE] Switching microphone from '{deviceName}' to '{newDeviceName}'");

            StopTalking();
            StopMicrophone();
        }

        deviceName = newDeviceName;

        if (canTalk)
        {
            StartMicrophone();
            StartTalking();
        }
    }

    private void StartTalking()
    {
        if (string.IsNullOrEmpty(deviceName))
            return;

        StartCoroutine(TransmitVoice());
    }

    private void StopTalking()
    {
        if (string.IsNullOrEmpty(deviceName))
            return;

        StopCoroutine(TransmitVoice());
    }

    private IEnumerator TransmitVoice()
    {
        while (canTalk)
        {
            if (microphoneClip == null)
                yield break;

            int micPosition = Microphone.GetPosition(deviceName);

            if (micPosition < position)
                position = micPosition;

            if (position + bufferSize > micPosition)
            {
                yield return null;
                continue;
            }

            microphoneClip.GetData(audioBuffer, position);
            position = (position + bufferSize) % microphoneClip.samples;

            var mode = (int)GetCurrentVoiceMode();

            TransmitAudioServerRpc(audioBuffer, mode);

            yield return new WaitForSeconds(bufferSize / (float)sampleRate);
        }
    }

    private bool IsVoiceActivated()
    {
        if (microphoneClip == null)
            return false;

        int micPosition = Microphone.GetPosition(deviceName);

        int sampleStartPosition = micPosition - bufferSize;
        if (sampleStartPosition < 0)
            return false;

        microphoneClip.GetData(sampleData, sampleStartPosition);

        float sum = 0;
        for (int i = 0; i < sampleData.Length; i++)
            sum += Mathf.Abs(sampleData[i]);

        float average = sum / sampleData.Length;
        return average > voiceActivationThreshold;
    }


    private VoiceMode GetCurrentVoiceMode()
    {
        if (_radio != null && _radio.UseRadio) return VoiceMode.Radio;
        if (_env != null && _env.InSubmarine) return VoiceMode.Submarine;
        return VoiceMode.Proximity;
    }


    [ServerRpc(RequireOwnership = false)]
    private void TransmitAudioServerRpc(float[] audioData, int mode, NetworkConnection sender = null)
    {
        TransmitAudioObserversRpc(audioData, mode, sender.ClientId);
    }

    [ObserversRpc]
    private void TransmitAudioObserversRpc(float[] audioData, int mode, int senderClientId)
    {
        if (senderClientId == NetworkManager.ClientManager.Connection.ClientId)
            return;

        PlayReceivedAudio(audioData, senderClientId, (VoiceMode)mode);
    }

    private void PlayReceivedAudio(float[] audioData, int senderClientId, VoiceMode mode)
    {
        if (source == null)
        {
            Debug.LogError("[VOICE] AudioSource not assigned!");
            return;
        }

        if (mode != VoiceMode.Radio && VoiceChatType == ChatType.Proximity)
        {
            source.spatialBlend = 1.0f;
            source.maxDistance = proximityRange;

            Transform senderTransform = GetPlayerTransform(senderClientId);
            if (senderTransform != null)
            {
                float distance = Vector3.Distance(transform.position, senderTransform.position);
                if (distance > proximityRange)
                    return;
            }
        }
        else
        {
            source.spatialBlend = 0.0f;
        }

        ConfigurePlaybackChain(mode);

        AudioClip clip = AudioClip.Create("ReceivedVoice", audioData.Length, 1, sampleRate, false);
        clip.SetData(audioData, 0);
        source.clip = clip;
        source.Play();
    }

    private T EnsureFilter<T>() where T : Behaviour
    {
        var f = source.GetComponent<T>();
        if (f == null) f = source.gameObject.AddComponent<T>();
        return f;
    }

    private void DisableAllFilters()
    {
        if (_lpf) _lpf.enabled = false;
        if (_hpf) _hpf.enabled = false;
        if (_echo) _echo.enabled = false;
        if (_reverb) _reverb.enabled = false;
        if (_dist) _dist.enabled = false;
    }

    private void ConfigurePlaybackChain(VoiceMode mode)
    {
        _lpf = EnsureFilter<AudioLowPassFilter>();
        _hpf = EnsureFilter<AudioHighPassFilter>();
        _echo = EnsureFilter<AudioEchoFilter>();
        _reverb = EnsureFilter<AudioReverbFilter>();
        _dist = EnsureFilter<AudioDistortionFilter>();

        DisableAllFilters();

        switch (mode)
        {
            case VoiceMode.Proximity:
                _lpf.enabled = true; _lpf.cutoffFrequency = 8000f;
                _hpf.enabled = false;
                _echo.enabled = false;
                _reverb.enabled = false;
                _dist.enabled = false;
                break;

            case VoiceMode.Submarine:
                _hpf.enabled = true; _hpf.cutoffFrequency = 500f;
                _lpf.enabled = true; _lpf.cutoffFrequency = 4500f;

                _echo.enabled = true;
                _echo.delay = 120f; _echo.decayRatio = 0.3f; _echo.wetMix = 0.4f; _echo.dryMix = 0.8f;

                _reverb.enabled = true;
                _reverb.decayHFRatio = 0.6f;
                _reverb.decayTime = 1.6f;
                _reverb.reflectionsLevel = -2000f;
                _reverb.reverbLevel = -1000f;

                _dist.enabled = false;
                break;

            case VoiceMode.Radio:
                _hpf.enabled = true; _hpf.cutoffFrequency = 300f;
                _lpf.enabled = true; _lpf.cutoffFrequency = 3000f;

                _dist.enabled = true; _dist.distortionLevel = 0.1f;

                _echo.enabled = true;
                _echo.delay = 35f; _echo.decayRatio = 0.15f; _echo.wetMix = 0.25f; _echo.dryMix = 0.9f;

                _reverb.enabled = false;

                source.spatialBlend = 0f;
                break;
        }
    }


    private Transform GetPlayerTransform(int clientId)
    {
        foreach (var obj in FindObjectsOfType<NetworkObject>())
        {
            if (obj.Owner.ClientId == clientId)
                return obj.transform;
        }
        return null;
    }

    private float GetMicInputVolume()
    {
        if (microphoneClip == null || string.IsNullOrEmpty(deviceName))
            return 0f;

        int micPosition = Microphone.GetPosition(deviceName);

        int sampleStartPosition = micPosition - bufferSize;
        if (sampleStartPosition < 0)
            return 0f;

        microphoneClip.GetData(micDataBuffer, sampleStartPosition);

        float sum = 0;
        for (int i = 0; i < micDataBuffer.Length; i++)
            sum += micDataBuffer[i] * micDataBuffer[i]; // RMS
        float rmsValue = Mathf.Sqrt(sum / micDataBuffer.Length);

        float amplifiedVolume = Mathf.Clamp(rmsValue * 50f, 0f, 1f);
        return amplifiedVolume;
    }
}
