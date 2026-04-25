using UnityEngine;

public static class AnimatorExtensions
{
    public static void SafeSetInt(this Animator anim, int hash, int value)
    {
        if (anim == null) return;
        if (!anim.isActiveAndEnabled) return;
        if (anim.runtimeAnimatorController == null) return;

        anim.SetInteger(hash, value);
    }
}