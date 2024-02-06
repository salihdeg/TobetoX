using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int _maxHealth;
    private Animator _animator;
    private int _health;
    private Vector3 _rotation;

    private const string HURT = "Hurt";
    private const string DIE = "Die";
    private const string DIE_INDEX = "DieIndex";
    private const string DANCE = "Dance";

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rotation = transform.eulerAngles;
    }

    private void Start()
    {
        _health = _maxHealth;
    }

    public void Damage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            //OlmeAnim
            ScoreManager.Instance.AddEnemyCount();
            _animator.SetInteger(DIE_INDEX, Random.Range(0, 2));
            _animator.SetTrigger(DIE);
            //It will destory on anim state end!
        }
        else
        {
            _animator.SetTrigger(HURT);
        }
    }
    
    public void Dance()
    {
        _animator.SetTrigger(DANCE);
    }

    private void OnEnable()
    {
        _health = _maxHealth;
        transform.eulerAngles = _rotation;
    }
}
