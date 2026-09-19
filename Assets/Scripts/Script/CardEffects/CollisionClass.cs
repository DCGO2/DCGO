using System;

public class CollisionClass : ICardEffect, ICollisionEffect
{
    public void SetUpCollisionClass(Func<Permanent, bool> PermanentCondition)
    {
        this.PermanentCondition = PermanentCondition;
    }

    Func<Permanent, bool> PermanentCondition { get; set; }

    public bool HasCollision(Permanent permanent)
    {
        if (PermanentCondition != null)
        {
            if (permanent != null)
            {
                if (permanent.TopCard != null)
                {
                    if (PermanentCondition(permanent))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}