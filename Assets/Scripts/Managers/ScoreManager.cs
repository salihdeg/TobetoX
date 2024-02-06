using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public event System.EventHandler OnEnemyCountChanged;

    private int _killedEnemyCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void AddEnemyCount()
    {
        _killedEnemyCount++;
        OnEnemyCountChanged?.Invoke(this, System.EventArgs.Empty);
    }

    public int GetKilledEnemyCount()
    {
        return _killedEnemyCount;
    }
}
