using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerPool : MonoBehaviour
{
    public static EnemySpawnerPool Instance { get; private set; }

    [SerializeField] private Transform[] _enemies;
    private int _currentEnemyIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach (var enemy in _enemies)
        {
            enemy.gameObject.SetActive(false);
        }

        NextEnemy();
    }

    public void NextEnemy()
    {
        int rndIndex = Random.Range(0, _enemies.Length);

        while (rndIndex == _currentEnemyIndex)
        {
            rndIndex = Random.Range(0, _enemies.Length);
        }

        _enemies[_currentEnemyIndex].gameObject.SetActive(false);
        _enemies[rndIndex].gameObject.SetActive(true);

        _currentEnemyIndex = rndIndex;
    }

    public void DanceAllEnemies()
    {
        foreach (Transform enemy in _enemies)
        {
            enemy.gameObject.SetActive(true);
            enemy.GetComponent<Enemy>().Dance();
        }
    }

}
