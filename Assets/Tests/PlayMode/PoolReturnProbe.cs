using System;
using UnityEngine;

// Test-only lifecycle callback; no production API or assembly changes.
public class PoolReturnProbe : MonoBehaviour
{
    public Action Returned;
    private void OnDisable() => Returned?.Invoke();
}