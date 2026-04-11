using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICounterable
{
    bool CanCounter { get; }

    void EnableCounter();
    void DisableCounter();

    void OnCounterSuccess(SDamageInfo info);

    IEnumerator Counter(float duration);
}
