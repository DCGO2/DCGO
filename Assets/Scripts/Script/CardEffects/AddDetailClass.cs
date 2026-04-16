using System.Collections;
using System.Collections.Generic;
using System;

public class AddDetailClass : ICardEffect, IAddDetailEffect
{
    Func<Permanent, bool> _permanentCondition = null;
    string _detail = null;
    bool _triggerEffect = false;

    public void SetUpAddDetailClass(Func<Permanent, bool> permanentCondition, string detail, bool triggerEffect)
    {
        _permanentCondition = permanentCondition;
        _detail = detail;
        _triggerEffect = triggerEffect;
    }

    public bool PermanentCondition(Permanent permanent)
    {
        return _permanentCondition != null
            && permanent != null
            && permanent.TopCard != null
            && _permanentCondition(permanent);
    }
    public string GetDetail()
    {
        return _detail;
    }

    public bool TriggerEffect()
    {
        return _triggerEffect;
    }
}