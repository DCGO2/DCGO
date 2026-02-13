using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChangeEndTurnMinMemoryClass : ICardEffect, IChangeEndTurnMinMemoryEffect
{
    Func<int, int> _changetMinMemory = null;
    public void SetUpChangeEndTurnMinMemoryClass(Func<int, int> changetMinMemory)
    {
        _changetMinMemory = changetMinMemory;
    }

    public int GetMinMemory(int minMemory)
    {
        if (_changetMinMemory != null)
        {
            minMemory = _changetMinMemory(minMemory);
        }

        return minMemory;
    }
}