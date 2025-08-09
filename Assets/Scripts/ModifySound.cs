using UnityEngine;
using UnityEngine.UI;

public class ModifySound : MonoBehaviour
{
    public void OnBgmSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.MasterBgmVolume = value;
    }
    
    public void OnSfxSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.MasterSfxVolume = value;
    }
    
    public void OnDocentSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.MasterDocentVolume = value;
    }
    
    public void OnCharaSliderValueChanged(float value)
    {
        SystemManager.Inst.AudioManagerInst.MasterCharaVolume = value;
    }
}
