using System;
using System.Collections.Generic;
using UnityEngine;

public enum SignalType { Video, Audio }
public enum NodeFault { None, Off, Muted, WrongInput, Broken }

public class SignalNode : MonoBehaviour
{
    public string displayName;
    public SignalType signalType;
    public bool isSource;                 // ej. laptop, micrófono
    public NodeFault fault = NodeFault.None;

    public readonly List<Cable> incoming = new();
    public event Action Changed;

    public bool HasSignal() => Evaluate(new HashSet<SignalNode>());

    bool Evaluate(HashSet<SignalNode> visited)
    {
        if (fault != NodeFault.None) return false;
        if (!visited.Add(this)) return false;   // evita ciclos
        if (isSource) return true;

        foreach (var cable in incoming)
            if (cable.IsPassing && cable.from.Evaluate(visited))
                return true;
        return false;
    }

    public void SetFault(NodeFault newFault)
    {
        fault = newFault;
        NotifyChanged();
    }

    public void NotifyChanged() => Changed?.Invoke();
}