using System;

[Serializable]
public class Seed : Object
{
    public int seed;
    public int oldSeed;

    public bool Changed ()
    {
        return seed != oldSeed;
    }

    public void Sync ()
    {
        oldSeed = seed;
    }
}
