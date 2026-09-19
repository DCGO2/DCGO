using System.Collections;

public class SkillInfo
{
    public SkillInfo(ICardEffect cardEffect, Hashtable hashtable, EffectTiming timing)
    {
        CardEffect = cardEffect;
        Hashtable = hashtable;
        Timing = timing;
    }

    public ICardEffect CardEffect { get; set; }
    public Hashtable Hashtable { get; set; }
    public EffectTiming Timing { get; set; }
}
