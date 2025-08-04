using System.Collections.Generic;

public static class UIDisplayNameParser
{
    private static readonly Dictionary<Subject, string> subjectMap = new Dictionary<Subject, string>()
    {
        { Subject.Lifestyle, "생활 양식" },
    };
    private static readonly Dictionary<Age, string> ageMap = new Dictionary<Age, string>()
    {
        { Age.Prehistory, "선사 시대" },
    };

    public static string GetDisplayNameGeneric<T>(T collection)
    {
        switch (collection)
        {
            case Subject subject: return GetDisplayName(subject);
            case Age age: return GetDisplayName(age);
            default: return collection?.ToString() ?? string.Empty;
        }
    }
    
    private static string GetDisplayName(Subject collection)
    {
        if (subjectMap.TryGetValue(collection, out var name))
        {
            return name;
        }
        
        return collection.ToString();
    }
    
    private static string GetDisplayName(Age collection)
    {
        if (ageMap.TryGetValue(collection, out var name))
        {
            return name;
        }
        
        return collection.ToString();
    }
}
