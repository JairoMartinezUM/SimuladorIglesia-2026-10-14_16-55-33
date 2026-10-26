using UnityEngine;

public class Cable : MonoBehaviour
{
    public SignalNode from;
    public SignalNode to;
    public SignalType signalType;
    public bool isConnected = true;
    public bool isDamaged;

    public bool IsPassing => isConnected && !isDamaged && from != null && to != null;

    void OnEnable() { if (to != null) to.incoming.Add(this); }
    void OnDisable() { if (to != null) to.incoming.Remove(this); }

    public void SetConnected(bool value)
    {
        isConnected = value;
        if (to != null) to.NotifyChanged();
    }
}