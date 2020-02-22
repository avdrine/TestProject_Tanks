using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Класс, описывающий событие, которое передает контроллер ракеты
/// </summary>
public class RocketEvent : UnityEvent<RocketController> { };

/// <summary>
/// Структура, описывающая требования к параметрам, используемых для инициализации элементов пула 
/// </summary>
public struct RocketPoolObjectParameters : PoolObjectParameters 
{ 
    /// <summary>
    /// Урон
    /// </summary>
    public float damage; 

    /// <summary>
    /// Точка спавна
    /// </summary>
    public Transform spawnTransform; 

    /// <summary>
    /// Пул для создания взрыва
    /// </summary>
    public PoolOfExplosions poolOfExplosions;
    public RocketPoolObjectParameters(float newDamage, Transform newSpawnTransform, PoolOfExplosions newPoolOfExplosions) { damage = newDamage; spawnTransform = newSpawnTransform; poolOfExplosions = newPoolOfExplosions; } 
}

/// <summary>
/// Класс, описывающий контроллер ракеты
/// </summary>
public class RocketController : MonoBehaviour, IPoolObject
{
    #region Vars

    /// <summary>
    /// Скорость полета ракеты
    /// </summary>
    [SerializeField] [Tooltip("Скорость полета ракеты")] private float _speed;

    /// <summary>
    /// Спрайт огня из хвоста ракеты при полете
    /// </summary>
    [SerializeField] [Tooltip("Спрайт огня из хвоста ракеты при полете")] private GameObject _fireSprite;

    /// <summary>
    /// Теги для просчета столкновений
    /// </summary>
    [SerializeField] [Tooltip("Теги для просчета столкновений")] private string[] _tagsToCollide;

    /// <summary>
    /// Ссылка на rigidbody
    /// </summary>
    [SerializeField] [Tooltip("Перенесите локальный компонент Rigidbody2D")] private Rigidbody2D _rb;

    /// <summary>
    /// Урон
    /// </summary>
    [SerializeField] [Tooltip("Урон")] private float _damage;

    /// <summary>
    /// Статус активации
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Статус активации")] private bool _activated;

    /// <summary>
    /// Точка спавна
    /// </summary>
    [SerializeField] [Tooltip("Точка спавна")] private Transform _spawnPoint;

    /// <summary>
    /// Пул для взрывов
    /// </summary>
    [SerializeField] [Tooltip("Пул для взрывов")] private PoolOfExplosions _poolOfExplosions;

    #endregion

    #region Events

    /// <summary>
    /// Событие отключения объекта
    /// </summary>
    [HideInInspector] public RocketEvent ObjectDisabledEvent = new RocketEvent();

    #endregion


    #region Implementation of IPoolObject
    /// <summary>
    /// Функция для включения и инициализации объекта
    /// </summary>
    /// <param name="_parameters">Параметры, необходимые для инициализации (необязательно)</param>
    /// <returns>Успешность инициализации</returns>
    public bool Enable(PoolObjectParameters _parameters = null)
    {
        RocketPoolObjectParameters initializeParameters = (RocketPoolObjectParameters)_parameters;

        //Устанавливаем параметры инициализации
        _damage = initializeParameters.damage;
        _spawnPoint = initializeParameters.spawnTransform;
        _poolOfExplosions = initializeParameters.poolOfExplosions;
        transform.position = _spawnPoint.position;
        transform.rotation = _spawnPoint.rotation;

        //Сбросить ускорения rigidbody
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0;
        
        //Выключаем хвост огня
        _fireSprite.SetActive(false);

        //Переключаем режим RigidBody, чтобы не просчитывались импульсы столкновений
        _rb.bodyType = RigidbodyType2D.Kinematic;

        //Устанавливаем состояние активации
        _activated = false;

        //Включаем объект
        gameObject.SetActive(true);

        //Возвращаем успешный результат
        return true;
    }

    /// <summary>
    /// Функция для получения параметров объекта, которые используются для индексации элементов пула
    /// </summary>
    /// <returns>Параметры объекта, используемые для индексации</returns>
    public PoolObjectParameters GetPoolObjectParameters()
    {
        return null;
    }

    /// <summary>
    /// Получить текущее состояние активности (включен / выключен)
    /// </summary>
    /// <returns>Текущее состояние активности</returns>
    public bool GetState()
    {
        return gameObject.activeSelf;
    }

    /// <summary>
    /// Функция, выключающая объект
    /// </summary>
    public void DisableObj()
    {
        gameObject.SetActive(false);
        ObjectDisabledEvent.Invoke(this);
    }
    #endregion

    #region Functions

    /// <summary>
    /// Активация ракеты
    /// </summary>
    /// <param name="_newDamage">Урон ракеты</param>
    public void Activate(float _newDamage)
    {
        //Включаем огненный хвост ракеты
        _fireSprite.SetActive(true);

        //Переключаем режим Rigidbody для просчет физики столкновений
        _rb.bodyType = RigidbodyType2D.Dynamic;

        //Задаем стартовый импульс
        _rb.AddRelativeForce(Vector2.up * _speed * _rb.mass);

        //Устанавливаем новый урон
        _damage = _newDamage;

        //Меняем стату активации
        _activated = true;
    }

    void Update()
    {
        //Если ракета не активирована, то следим за позицией точки спавна
        if(!_activated)
        {
            transform.position = _spawnPoint.position;
            transform.rotation = _spawnPoint.rotation;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Если сталкиваемся с нужным тегом, то организуем взрыв
        for(int i = 0; i < _tagsToCollide.Length; i++)
        {
            if(collision.gameObject.CompareTag(_tagsToCollide[i]))
            {
                _poolOfExplosions.InstantiateObject( new ExplosionPoolObjectParameters(_damage, 0.3f, transform));
                DisableObj();
                break;
            }
        }
        
    }

    #endregion

}
