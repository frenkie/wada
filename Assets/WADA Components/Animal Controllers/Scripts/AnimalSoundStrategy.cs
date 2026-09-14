namespace Wada
{
    public enum AnimalSoundStrategy
    {
        FireOnce,
        FireOnceAndLoop, // TODO loop count after which it will be retriggered

        FireEveryTime // is fire by animation event
        //TODO: FireOnStateEnter
    }
}