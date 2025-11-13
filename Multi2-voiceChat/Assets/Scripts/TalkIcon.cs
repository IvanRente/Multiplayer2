using UnityEngine;
using UnityEngine.UI;

public class TalkIcon : MonoBehaviour
{
    public RawImage normalIcon;
    public RawImage radioIcon;
    public KeyCode pushToTalkKey = KeyCode.V;
    public bool showOnlyWhileTalking = true;
    private WalkieTalkieToggle radioToggle;

    void OnEnable() { ResolveLocalToggle(); }
    void Update()
    {
        if (radioToggle == null || !radioToggle.isActiveAndEnabled)
            ResolveLocalToggle();

        bool isTalking = !showOnlyWhileTalking || Input.GetKey(pushToTalkKey);
        bool isRadio = (radioToggle != null && radioToggle.UseRadio);

        SetExclusive(isTalking && !isRadio, isTalking && isRadio);
    }

    private void ResolveLocalToggle()
    {
        radioToggle = null;
        foreach (var vc in FindObjectsOfType<VoiceChat2>())
        {
            if (vc.IsOwner)
            {
                radioToggle =
                    vc.GetComponent<WalkieTalkieToggle>() ??
                    vc.GetComponentInChildren<WalkieTalkieToggle>(true) ??
                    vc.GetComponentInParent<WalkieTalkieToggle>(true);
                break;
            }
        }
        SetExclusive(false, false);
    }

    private void SetExclusive(bool showNormal, bool showRadio)
    {
        if (normalIcon) normalIcon.gameObject.SetActive(showNormal);
        if (radioIcon) radioIcon.gameObject.SetActive(showRadio);
    }
}
