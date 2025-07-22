using System;

public class LearningProgressUIModel
{
    public LearningProgressUICollection CurrentUI { get; private set; }
    public readonly LearningProgressUICollection[] UICollectionArray =
        Enum.GetValues(typeof(LearningProgressUICollection)) as LearningProgressUICollection[];
    
    public Subject CurrentSubject { get; private set; }
    public readonly Subject[] SubjectArray =
        Enum.GetValues(typeof(Subject)) as Subject[];
    
    public Age CurrentAge { get; private set; }
    public readonly Age[] AgeArray =
        Enum.GetValues(typeof(Age)) as Age[];

    public LearningProgressUIModel(LearningProgressUICollection collection)
    {
        CurrentUI = collection;
        CurrentSubject = Subject.None;
        CurrentAge = Age.None;
    }

    public void SwitchCurrentState(LearningProgressUICollection collection)
    {
        CurrentUI = collection;
    }

    public void SwitchCurrentState(Subject subject)
    {
        CurrentSubject = subject;
    }

    public void SwitchCurrentState(Age age)
    {
        CurrentAge = age;
    }
}
