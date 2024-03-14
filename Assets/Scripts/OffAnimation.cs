using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffAnimation : MonoBehaviour
{
    public virtual void Off()
    {
        this.gameObject.SetActive(false);
    }

    public virtual void OffAnimEnable()
    {

    }
}
