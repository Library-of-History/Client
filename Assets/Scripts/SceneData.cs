using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Scriptable Objects/SceneData")]
public class SceneData : ScriptableObject
{
    [Serializable]
    private class SceneTuple
    {
        public string sceneName;
        public int choiceCount;
        public string summary;
    }
    
    [SerializeField] private SceneTuple[] scenes;

    public Dictionary<string, List<string>> BookMap { get; private set; }
    public Dictionary<string, int> CountMap { get; private set; }
    public Dictionary<string, string> SummaryMap { get; private set; }
    
    private StringBuilder sb;

    public void Init()
    {
        BookMap = new Dictionary<string, List<string>>();
        CountMap = new Dictionary<string, int>();
        SummaryMap = new FlexibleDictionary<string, string>();
        sb = new StringBuilder();
        
        foreach (var tuple in scenes)
        {
            CountMap[tuple.sceneName] = tuple.choiceCount;
            SummaryMap[tuple.sceneName] = tuple.summary;
            
            string[] names = tuple.sceneName.Split('_');

            sb.Clear();
            sb.Append(names[0]);
            sb.Append("_");
            sb.Append(names[1]);

            if (!BookMap.TryGetValue(sb.ToString(), out var nameList))
            {
                nameList = new List<string>();
                BookMap[sb.ToString()] = nameList;
            }
            
            nameList.Add(names[2]);
        }
    }
}
