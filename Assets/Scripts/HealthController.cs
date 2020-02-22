using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс, описывающий контроллер жизней
/// </summary>
public class HealthController : MonoBehaviour
{
    #region Vars

    #region Parameters

    /// <summary>
    /// Максимальное количество жизней
    /// </summary>
    [Header("Parameters")]
    [SerializeField] [Tooltip("Максимальное количество жизней")] private float _maxHP = 100;

    /// <summary>
    /// Значение жизней при старте
    /// </summary>
    [SerializeField] [Tooltip("Значение жизней при старте")] private float _startHP = 100;

    /// <summary>
    /// Броня
    /// </summary>
    [SerializeField] [Tooltip("Броня")] [Range(0,1)] private float _armor;

    /// <summary>
    /// Текущее количество жизней
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Текущее количество жизней")] private float _currHP;

    #endregion

    #region Components Links

    /// <summary>
    /// Ссылка на компонент HPBarController
    /// </summary>
    [Header("Components Links")]
    [SerializeField] [Tooltip("Перенесите компонент HPBarController из дочернего объекта")] private HPBarController _HPBar;

    #endregion

    #endregion

    #region Events

    /// <summary>
    /// Событие смерти
    /// </summary>
    [Space(20)]
    [SerializeField] [Tooltip("Событие смерти")] private UnityEvent _dieEvent = new UnityEvent();

    #endregion


    #region Properties

    /// <summary>
    /// Текущее количество жизней 
    /// </summary>
    public float CurrHP 
    { 
        get { return _currHP; } 
        set 
        {
            if (value <= 0 && _currHP > 0) _dieEvent.Invoke();
            if (value < MaxHP && value > 0) _currHP = value;
            else if (value <= 0) _currHP = 0;
            else _currHP = MaxHP;

            if (_HPBar != null) _HPBar.ChangeHPCount(CurrHP);
        } 
    }

    /// <summary>
    /// Максимальное количество жизней
    /// </summary>
    public float MaxHP { get { return _maxHP; } }

    #endregion

    #region Functions

    void Start () 
    {
        CurrHP = _startHP;
        if(_HPBar != null) _HPBar.Initialize(MaxHP, CurrHP);
    }

    /// <summary>
    /// Получить урон
    /// </summary>
    /// <param name="damage">Получаемый урон</param>
    public void GetDamage(float damage)
    {
        if (damage < 0) damage = 0;
        CurrHP -= damage * (1 - _armor);
    }

    /// <summary>
    /// Излечиться
    /// </summary>
    /// <param name="healPower">Сила излечения</param>
    public void Heal(float healPower)
    {
        if (healPower < 0) healPower = 0;
        CurrHP += healPower;
    }

    #endregion

    #region Debug buttons in inspector

#if UNITY_EDITOR
    [CustomEditor(typeof(HealthController))]
    public class DebugPanelEditor : Editor
    {
        HealthController targetObject;
        public override void OnInspectorGUI()
        {
            targetObject = target as HealthController;
            if (UnityEditor.EditorApplication.isPlaying)
            {
                GUILayout.Space(10);
                GUILayout.Label("Control buttons");
                if (GUILayout.Button("Full Heal"))
                {
                    targetObject.CurrHP = targetObject.MaxHP;
                }
                if (GUILayout.Button("Half Of max HP"))
                {
                    targetObject.CurrHP = targetObject.MaxHP/2;
                }
                if (GUILayout.Button("Kill"))
                {
                    targetObject.CurrHP = 0;
                }
                GUILayout.Space(20);
            }
            base.OnInspectorGUI();
        }
    }
#endif

    #endregion
}
