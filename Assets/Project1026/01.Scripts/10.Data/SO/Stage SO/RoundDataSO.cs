using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoundDataSO", menuName = "Data/Stage/Round")]
public class RoundDataSO : ScriptableObject
{
    [SerializeField] private List<WaveDataSO> waves = new();

    public IReadOnlyList<WaveDataSO> Waves => waves;
}
