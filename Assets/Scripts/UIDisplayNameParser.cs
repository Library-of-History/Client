using System.Collections.Generic;

public static class UIDisplayNameParser
{
    private static readonly Dictionary<UIControllerCollection, string> uCMap = new Dictionary<UIControllerCollection, string>()
    {
        { UIControllerCollection.LearningProgress, "Learning Progress" },
        { UIControllerCollection.LearningLog, "Learning Log" },
        { UIControllerCollection.ObjectModify, "Object Modify" },
    };
    private static readonly Dictionary<LearningProgressUICollection, string> lPMap = new Dictionary<LearningProgressUICollection, string>()
    {
        { LearningProgressUICollection.Subject, "Subject" },
        { LearningProgressUICollection.Age, "Age" },
    };
    private static readonly Dictionary<Subject, string> sMap = new Dictionary<Subject, string>()
    {
        { Subject.Lifestyle, "생활 양식" },
    };
    private static readonly Dictionary<Age, string> aMap = new Dictionary<Age, string>()
    {
        { Age.Prehistory, "선사 시대" },
    };

    public static string GetDisplayNameGeneric<T>(T collection)
    {
        switch (collection)
        {
            case Subject subject: return GetDisplayName(subject);
            case Age age: return GetDisplayName(age);
            case LearningProgressUICollection lP: return GetDisplayName(lP);
            case UIControllerCollection uC: return GetDisplayName(uC);
            default: return collection?.ToString() ?? string.Empty;
        }
    }

    private static string GetDisplayName(UIControllerCollection collection)
    {
        if (uCMap.TryGetValue(collection, out var name))
        {
            return name;
        }
        
        return collection.ToString();
    }
    
    private static string GetDisplayName(LearningProgressUICollection collection)
    {
        if (lPMap.TryGetValue(collection, out var name))
        {
            return name;
        }
        
        return collection.ToString();
    }
    
    private static string GetDisplayName(Subject collection)
    {
        if (sMap.TryGetValue(collection, out var name))
        {
            return name;
        }
        
        return collection.ToString();
    }
    
    private static string GetDisplayName(Age collection)
    {
        if (aMap.TryGetValue(collection, out var name))
        {
            return name;
        }
        
        return collection.ToString();
    }
}
