namespace MiniHospital.Config
{
    public enum CharacterAction
    { 
        Standing, 
        Walking, 
        Sitting,
        Lying 
    }

    public enum Facing
    {
        Left,
        Right
    }

    public enum CharacterMood {
        Neutral, 
        Waiting, 
        Impatient,
        BeingTreated
    }

    public enum BubbleType {
        None, 
        Wait, 
        Angry, 
        Health, 
        Question, 
        Sleep, 
        Done 
    }
}
