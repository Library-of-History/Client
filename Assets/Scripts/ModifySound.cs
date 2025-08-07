using UnityEngine;
using UnityEngine.UI;

public class ModifySound : MonoBehaviour
{
    public void OnBgmSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.GetBgmSource().volume = value;
    }
    
    public void OnSfxSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.GetSfxSource().volume = value;
    }
    
    public void OnDocentSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.GetDocentVoiceSource().volume = value;
    }
    
    public void OnCharaSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.GetCharaVoiceSource().volume = value;
    }
}
