using System.Collections.Generic;

public static class UIDisplayNameParser
{
    private static readonly Dictionary<UICollection, string> Map = new Dictionary<UICollection, string>()
    {
        { UICollection.LearningProgress, "Learning Progress" },
        { UICollection.LearningLog, "Learning Log" },
    };

    public static string GetDisplayName(UICollection collection)
    {
        if (Map.TryGetValue(collection, out var name))
        {
            return name;
        }
        
        return collection.ToString();
    }
}
