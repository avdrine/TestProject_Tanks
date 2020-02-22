using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий контроллер HP бара
/// </summary>
public class HPBarController : MonoBehaviour 
{
    #region Vars

    #region Parameters

    /// <summary>
    /// Расстояние над объектом для фиксации позиции
    /// </summary>
    [Header("Parameters")]
    [SerializeField] [Tooltip("Расстояние над объектом для фиксации позиции")] private float _distanceToTarget;

    /// <summary>
    /// Максимальное количество жизней
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Максимальное количество жизней")] private float _maxHP;

    /// <summary>
    /// Текущее количество жизней
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Текущее количество жизней")] private float _currHP;

    /// <summary>
    /// Текущее количество жизней в процентах
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Текущее количество жизней в процентах")] private int _currHPInPercent;

    #endregion

    #region Colors

    /// <summary>
    /// Цвет большого количества жизней
    /// </summary>
    [Header("Colors")]
    [SerializeField] [Tooltip("Цвет большого количества жизней")] private Color32 _goodHPColor;

    /// <summary>
    /// Цвет среднего количества жизней
    /// </summary>
    [SerializeField] [Tooltip("Цвет среднего количества жизней")] private Color32 _normalHPColor;

    /// <summary>
    /// Цвет малого количества жизней
    /// </summary>
    [SerializeField] [Tooltip("Цвет малого количества жизней")] private Color32 _badHPColor;

    #endregion

    #region Components Links

    /// <summary>
    /// Ссылка на SpriteRenderer текущего состояния жизней
    /// </summary>
    [Header("Components Links")]
    [SerializeField] [Tooltip("Перенесите компонент SpriteRenderer текущего состояния жизней")] private SpriteRenderer _HPSprite;

    /// <summary>
    /// Объект для фиксации позиции
    /// </summary>
    [SerializeField] [Tooltip("Перенесите объект для фиксации позиции")] private Transform _target;

    #endregion

    #endregion

    #region Functions

    /// <summary>
    /// Инициализация компонента
    /// </summary>
    /// <param name="maxHP">Максимальное количество жизней</param>
    /// <param name="currHP">Текущее количество жизней</param>
    public void Initialize(float maxHP, float currHP)
    {
        if (maxHP < 1) maxHP = 1;
        if (currHP > maxHP) currHP = maxHP;
        else if (currHP < 0) currHP = 0;
        _maxHP = maxHP; _currHP = currHP;
        
        UpdateVisual();
    }

    /// <summary>
    /// Изменить количество текущих жизней
    /// </summary>
    /// <param name="newHP">Новое состояние жизней</param>
    public void ChangeHPCount(float newHP)
    {
        if (newHP > _maxHP) newHP = _maxHP;
        _currHP = newHP;
        UpdateVisual();
    }

    /// <summary>
    /// Обновить визуал HP бара
    /// </summary>
    private void UpdateVisual()
    {
        _currHPInPercent = (int)((_currHP / _maxHP) * 100);
        if (_currHPInPercent > 75) _HPSprite.color = _goodHPColor;
        else if(_currHPInPercent > 40) _HPSprite.color = _normalHPColor;
        else _HPSprite.color = _badHPColor;
        _HPSprite.size = new Vector2((float)_currHPInPercent / 100.0f, _HPSprite.size.y);
    }

    /// <summary>
    /// Буффер для Quaternion'а
    /// </summary>
    Quaternion _newRotation = new Quaternion();
    private void Update()
    {
        //Если есть объект для фиксации позиции, то преследовать его и проставить 
        //нужный угол поворота, чтобы HP бар отображался горизонтально
        if(_target != null)
        {
            _newRotation.eulerAngles = Vector3.zero;
            transform.position = new Vector3(_target.position.x, _target.position.y + _distanceToTarget, _target.position.z);
            transform.rotation = _newRotation;
        }
    }

    #endregion

}
