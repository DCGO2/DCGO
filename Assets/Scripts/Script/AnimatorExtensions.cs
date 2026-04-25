using UnityEngine;

public static class AnimatorExtensions
{
    // Toggle this if you want strict original behavior vs safe behavior
    public static bool EnableSafetyChecks = true;

    public static void SafeSetInt(this Animator anim, int hash, int value)
    {
        if (EnableSafetyChecks)
        {
            if (anim == null) return;
            if (!anim.isActiveAndEnabled) return;
            if (anim.runtimeAnimatorController == null) return;
        }

        anim.SetInteger(hash, value);
    }

    public static void SafeSetTrigger(this Animator anim, int hash)
    {
        if (EnableSafetyChecks)
        {
            if (anim == null) return;
            if (!anim.isActiveAndEnabled) return;
            if (anim.runtimeAnimatorController == null) return;
        }

        anim.SetTrigger(hash);
    }

    public static void SafeSetBool(this Animator anim, int hash, bool value)
    {
        if (EnableSafetyChecks)
        {
            if (anim == null) return;
            if (!anim.isActiveAndEnabled) return;
            if (anim.runtimeAnimatorController == null) return;
        }

        anim.SetBool(hash, value);
    }
}