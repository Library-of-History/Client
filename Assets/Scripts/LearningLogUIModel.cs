using System.Collections.Generic;
using UnityEngine;

public class LearningLogUIModel : LearningProgressUIModel
{
    public float CurrentEntirePercent { get; private set; }
    public Dictionary<string, float> CurrentPartialPercent { get; private set; }
        
    public LearningLogUIModel(LearningProgressUICollection collection) : base(collection)
    {
        CurrentPartialPercent = new Dictionary<string, float>();
        CalcPercent();
    }

    public void CalcPercent()
    {
        int sum = 0;
        int selectedCount = 0;
        
        foreach (var count in SystemManager.Inst.SceneDataInst.CountMap)
        {
            sum += count.Value;
            selectedCount += PlayerPrefs.GetInt(count.Key, 0);
            
            CurrentPartialPercent[count.Key] = PlayerPrefs.GetInt(count.Key, 0) / (float)count.Value * 100f;
        }
        
        CurrentEntirePercent = (float)selectedCount / sum * 100f;
    }
}
