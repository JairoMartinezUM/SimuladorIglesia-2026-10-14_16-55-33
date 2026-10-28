using Unity.Collections;
using UnityEngine;

public enum FaultKind { CableDisconnected, CableDamaged, NodeFault }

[System.Serializable]
public class FaultSetup
{
    public FaultKind kind;
    public string targetId;
    public NodeFault nodeFault = NodeFault.Off;
}

[CreateAssetMenu(menuName = "Simulasor/Problema", fileName = "Problema_")]
public class ProblemData : ScriptableObject 
{
    public string title;
    [TextArea(2, 5)] public string briefing;
    [TextArea(2, 5)] public string report;
    [TextArea(2, 5)] public string[] hints;
    public float timeLimitSeconds = 120f;
    public string[] objectiveNodes;
    public FaultSetup[] faults;
}