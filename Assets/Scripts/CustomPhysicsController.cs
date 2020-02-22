using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Перечисление типов контроллеров движения для кастомной физики
/// </summary>
public enum CustomPhysicsType { Basic, BasicMovingToTarget, Car }

/// <summary>
/// Класс описывающий контроллер кастомной физики
/// </summary>
public class CustomPhysicsController : MonoBehaviour
{
    #region Vars

    #region Components Links

    /// <summary>
    /// Ссылка на Rigidbody2D
    /// </summary>
    [Header("Components Links")]
    [SerializeField][Tooltip("Перенесите локальный компонент Rigidbody2D")] private Rigidbody2D _rb;

    #endregion

    #region Parameters

    /// <summary>
    /// Включить/выключить отскок при получении урона
    /// </summary>
    [Header("Parameters")]
    [SerializeField] [Tooltip("Отскок при получении урона")] private bool _pushThanTakeDamage;

    /// <summary>
    /// Сила отскока при получении урона
    /// </summary>
    [SerializeField] [Tooltip("Сила отскока при получении урона")] private float _pushPowerOnTakenDamage;

    /// <summary>
    /// Тип движения
    /// </summary>
    [SerializeField] [Tooltip("Тип движения")] private CustomPhysicsType _movingType;
    
    /// <summary>
    /// Цель для преследования
    /// </summary>
    [SerializeField] [Tooltip("Цель для преследования")] private Transform _target;
    
    /// <summary>
    /// Дистанция от цели, при которой останавливается движение
    /// </summary>
    [SerializeField] [Tooltip("Дистанция от цели, при которой останавливается движение")] private float _distanceToStop;
    
    /// <summary>
    /// Поворот объекта при смене направления движения
    /// </summary>
    [SerializeField] [Tooltip("Поворот объекта при смене направления движения")] private bool _needRotation;

    /// <summary>
    /// Максимальная скорость
    /// </summary>
    [SerializeField] [Tooltip("Максимальная скорость")] private float _maxSpeed;

    /// <summary>
    /// Максимальная скорость при движении назад
    /// </summary>
    [SerializeField] [Tooltip("Максимальная скорость при движении назад")] private float _maxBackSpeed;

    /// <summary>
    /// Время разгона до максимальной скорости
    /// </summary>
    [SerializeField] [Tooltip("Время разгона до максимальной скорости")] private float _timeToMaxSpeed;

    /// <summary>
    /// Время торможения
    /// </summary>
    [SerializeField] [Tooltip("Время торможения")] private float _timeToStopping;
    
    /// <summary>
    /// Скорость разворота
    /// </summary>
    [SerializeField] [Tooltip("Скорость разворота")] private float _rotateSpeed;

    /// <summary>
    /// Скорость подавления внешнего импульса
    /// </summary>
    [SerializeField] [Tooltip("Скорость подавления внешнего импульса")] private float _speedOfFadingPushPower;

    /// <summary>
    /// Флаг, влиющий на расчет максимальной скорости для вектора движения
    /// <para>Если true, то сумма осей не может превышать максимальной скорости</para>
    /// <para>Если false, то каждая из осей может равняться максимальной скорости</para>
    /// </summary>
    [SerializeField] [Tooltip("Флаг, влиющий на расчет максимальной скорости для вектора движения")] private bool _maxSpeedUsedForSumOFAxis;

    #endregion

    #region Moving flags

    /// <summary>
    /// Флаг запроса на движение вверх/вперед
    /// </summary>
    private bool _moveFront;

    /// <summary>
    /// Флаг запроса на движение вниз/назад
    /// </summary>
    private bool _moveBack;

    /// <summary>
    /// Флаг запроса на движение/разворота вправо
    /// </summary>
    private bool _moveRight;

    /// <summary>
    /// Флаг запроса на движение/разворота влево
    /// </summary>
    private bool _moveLeft;

    #endregion

    #region Object States

    /// <summary>
    /// Текущая скорость
    /// </summary>
    [Header("Object States")]
    [ReadOnly] [SerializeField] [Tooltip("Текущая скорость")] private float _currSpeed;

    /// <summary>
    /// Текущая скорость в формате вектора
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Текущая скорость в формате вектора")] private Vector2 _currSpeedVector;

    /// <summary>
    /// Направление движения вперед
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Направление движения вперед")] private Vector2 _forwardVector = new Vector2(0, 1);

    /// <summary>
    /// Внешний импульс
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Внешний импульс")] private Vector2 _pushVector = new Vector2(0, 0);

#endregion

    #region Buffers
    /// <summary>
    /// Предыдущее значение внешнего импульса
    /// </summary>
    private Vector2 _oldPushVector;

    /// <summary>
    /// Буффер для X
    /// </summary>
    private float _tmpX;

    /// <summary>
    /// Буффер для Y
    /// </summary>
    private float _tmpY;

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Текущая скорость
    /// </summary>
    public float CurrSpeed
    {
        get { return _currSpeed; }
        private set
        {
            if (value > _maxSpeed) _currSpeed = _maxSpeed;
            else if (value < -_maxBackSpeed) _currSpeed = -_maxBackSpeed;
            else _currSpeed = value;
        }
    }

    /// <summary>
    /// Текущая скорость в формате вектора
    /// </summary>
    public Vector2 CurrSpeedVector 
    { 
        get { return _currSpeedVector; } 
        private set 
        { 
            if(_maxSpeedUsedForSumOFAxis)
            {
                if (Mathf.Abs(value.x) + Mathf.Abs(value.y) < _maxSpeed) _currSpeedVector = value;
                else
                {
                    _currSpeedVector = new Vector2(value.x * (_maxSpeed / (Mathf.Abs(value.x) + Mathf.Abs(value.y))), value.y * (_maxSpeed / (Mathf.Abs(value.x) + Mathf.Abs(value.y))));
                }
            }
            else
            {
                if (Mathf.Abs(value.x) < _maxSpeed && Mathf.Abs(value.y) < _maxSpeed)
                {
                    _currSpeedVector = value;
                }
                else
                {
                    if (Mathf.Abs(value.x) > _maxSpeed && Mathf.Abs(value.y) > _maxSpeed) _currSpeedVector = new Vector2(_maxSpeed * (value.x > 0 ? 1 : -1), _maxSpeed * (value.y > 0 ? 1 : -1));
                    else if (Mathf.Abs(value.x) > _maxSpeed) _currSpeedVector = new Vector2(_maxSpeed * (value.x > 0 ? 1 : -1), value.y);
                    else if (Mathf.Abs(value.y) > _maxSpeed) _currSpeedVector = new Vector2(value.x, _maxSpeed * (value.y > 0 ? 1 : -1));
                }
            }
        } 
    }

    /// <summary>
    /// Флаг запроса на движение вверх/вперед
    /// </summary>
    public bool MoveFront { get { return _moveFront; } set { _moveFront = value; } }

    /// <summary>
    /// Флаг запроса на движение вниз/назад
    /// </summary>
    public bool MoveBack { get { return _moveBack; } set { _moveBack = value; } }

    /// <summary>
    /// Флаг запроса на движение/разворота вправо
    /// </summary>
    public bool MoveRight { get { return _moveRight; } set { _moveRight = value; } }

    /// <summary>
    /// Флаг запроса на движение/разворота влево
    /// </summary>
    public bool MoveLeft { get { return _moveLeft; } set { _moveLeft = value; } }

    /// <summary>
    /// Цель для преследования
    /// </summary>
    public Transform Target { get { return _target; } set { _target = value; } }

    /// <summary>
    /// Отскакивает ли объект при получении урона
    /// </summary>
    public bool PushThanTakeDamage { get { return _pushThanTakeDamage; } }

    #endregion

    #region Functions

    /// <summary>
    /// Сброс импульсов и ускорений, влияющих на Rigidbody
    /// </summary>
    public void RigidbodyReinitialize()
    {
        _rb.angularVelocity = 0;
        _rb.velocity = Vector2.zero;
        _pushVector = Vector2.zero;
        _currSpeed = 0;
        _currSpeedVector = Vector2.zero;
        _rb.rotation = 0;
        _forwardVector = new Vector2(0, 1);
    }
	
	void Update () 
    {
        UpdateMoving();
        FadingPushPower();
        if(_needRotation)
            UpdateRotation();
    }

    /// <summary>
    /// Применить импульс
    /// </summary>
    /// <param name="pushVector">Нормализованный вектор движения</param>
    /// <param name="mass">Масса объекта, давшего импульс</param>
    public void AddPush(Vector2 pushVector, float mass)
    {
        _pushVector += pushVector.normalized * Mathf.Abs(_rb.mass - mass);
    }

    /// <summary>
    /// Применить импульс от взрыва
    /// </summary>
    /// <param name="pushVector">Вектор импульса</param>
    public void AddExplosionPush(Vector2 pushVector)
    {
        CurrSpeed = 0;
        CurrSpeedVector = Vector2.zero;
        _pushVector += pushVector;
    }

    /// <summary>
    /// Применить импульс от урона
    /// </summary>
    /// <param name="pushVector">Вектор импульса для отскока</param>
    public void AddPushOnHit(Vector2 pushVector)
    {
        if(_pushThanTakeDamage)
            _pushVector += pushVector.normalized * _pushPowerOnTakenDamage;
    }

    /// <summary>
    /// Ослабление внешних импульсов
    /// </summary>
    private void FadingPushPower()
    {
        if(_pushVector != Vector2.zero)
        {
            if (Mathf.Abs(_pushVector.x) < _speedOfFadingPushPower && Mathf.Abs(_pushVector.y) < _speedOfFadingPushPower) _pushVector = Vector2.zero;
            else
            {
                _oldPushVector = _pushVector;
                _pushVector += ((-_pushVector.normalized) * _speedOfFadingPushPower) * Time.deltaTime; 
                if (Mathf.Abs(_oldPushVector.x) < _speedOfFadingPushPower) _pushVector = new Vector2(0, _pushVector.y);
                else if (Mathf.Abs(_oldPushVector.y) < _speedOfFadingPushPower) _pushVector = new Vector2(_pushVector.x, 0);
            }
        }
    }

    /// <summary>
    /// Просчитать движение
    /// </summary>
    private void UpdateMoving()
    {
        //Просчет движения для типа передвижения машины
        if(_movingType == CustomPhysicsType.Car)
        {
            //Увеличить скорость, если активен флаг движения вперед
            if (_moveFront) CurrSpeed += _maxSpeed * Time.deltaTime / _timeToMaxSpeed;

            //Сбросить скорость, если активен флаг движения назад
            if (_moveBack) CurrSpeed -= _maxBackSpeed * Time.deltaTime / _timeToMaxSpeed;

            //Увеличить или уменьшить скорость, в зависимости от ситуации, чтобы приблизиться к нулю,
            //если неактивны флаги движения вперед и назад
            if (!_moveFront && !_moveBack)
            {
                if (CurrSpeed > 0.1f) CurrSpeed -= _maxSpeed * Time.deltaTime / _timeToStopping;
                else if (CurrSpeed < -0.1f) CurrSpeed += _maxBackSpeed * Time.deltaTime / _timeToStopping;
                else CurrSpeed = 0;
            }

            //В зависимости от активности флагов поворотов - произвести или не произвести поворот
            if (_moveLeft && !_moveRight) _rb.angularVelocity = _rotateSpeed * (_currSpeed / _maxSpeed);
            else if (_moveRight) _rb.angularVelocity = -_rotateSpeed * (_currSpeed / _maxSpeed);
            else _rb.angularVelocity = 0;
            
            //Если поворачиваем, то перерасчитать вектор направления движения вперед
            if (_rb.angularVelocity != 0)
            {
                _forwardVector = Rotate(new Vector2(0, 1), transform.rotation.eulerAngles.z);
            }

            //Перерасчитать ускорение
            _rb.velocity = _forwardVector.normalized * CurrSpeed + _pushVector;
        }

        //Просчет движения для базового типа передвижения
        else if (_movingType == CustomPhysicsType.Basic)
        {
            //Сохранить в буфферах текущую скорость в векторе
            _tmpX = _currSpeedVector.x;
            _tmpY = _currSpeedVector.y;

            //Если не нужно двигаться вверх или вниз из-за состояний флагов движения, сбросить скорость движения по оси Y
            if ((_moveFront && _moveBack) || (!_moveFront && !_moveBack))
            {
                if (_tmpY != 0)
                {
                    if (Mathf.Abs(_tmpY) > 0.1f)
                        _tmpY = (Mathf.Abs(_tmpY) - _maxSpeed * Time.deltaTime / _timeToStopping) * ((_tmpY > 0) ? 1 : -1);
                    else
                        _tmpY = 0;
                }
            }

            //Если нужно двигаться по оси Y - увеличить скорость
            else
            {
                _tmpY += (_maxSpeed * Time.deltaTime / _timeToMaxSpeed) * ((_moveFront) ? 1 : -1);
            }

            //Если не надо двигаться по оси X, то сбросить скорость движения по X
            if ((_moveLeft && _moveRight) || (!_moveLeft && !_moveRight))
            {
                if (_tmpX != 0)
                {
                    if (Mathf.Abs(_tmpX) > 0.1f)
                        _tmpX = (Mathf.Abs(_tmpX) - _maxSpeed * Time.deltaTime / _timeToStopping) * ((_tmpX > 0) ? 1 : -1);
                    else
                        _tmpX = 0;
                }
            }

            //Если нужно двигаться по оси X - увеличить скорость
            else
            {
                _tmpX += (_maxSpeed * Time.deltaTime / _timeToMaxSpeed) * ((_moveRight) ? 1 : -1);
            }

            //Сохранить просчитанные скорости в векторе движения
            CurrSpeedVector = new Vector2(_tmpX, _tmpY);

            //Просчитать ускорение
            _rb.velocity = CurrSpeedVector + _pushVector;
        }

        //Просчет движения для базового типа передвижения к цели
        else if (_movingType == CustomPhysicsType.BasicMovingToTarget)
        {
            //Если дистанция больше, чем нужно для остановки, то увеличиваем скорость
            if (Vector2.Distance(Target.position, transform.position) > _distanceToStop)
            {
                CurrSpeed += _maxSpeed * Time.deltaTime / _timeToMaxSpeed;
                CurrSpeedVector = (Target.position - transform.position).normalized * CurrSpeed;
            }

            //Если мы достигли дистанции останова, то сбросить скорость
            else
            {
                CurrSpeedVector = Vector2.zero;
            }

            //Просчитать ускорение
            _rb.velocity = CurrSpeedVector + _pushVector;
        }
    }

    /// <summary>
    /// Буффер для кватерниона
    /// </summary>
    private Quaternion _tmpQuaternion = new Quaternion();

    /// <summary>
    /// Повернуть объект
    /// </summary>
    private void UpdateRotation()
    {
        if (_movingType == CustomPhysicsType.Basic || _movingType == CustomPhysicsType.BasicMovingToTarget)
        {
            if(CurrSpeedVector != Vector2.zero)
                transform.rotation = Quaternion.LookRotation(Vector3.forward, CurrSpeedVector);
        }
    }

        
    void OnCollisionEnter2D(Collision2D collision)
    {
        //Если тип движения машина и столкнулись с краем карты, то сделать отскок
        if(_movingType == CustomPhysicsType.Car && collision.gameObject.CompareTag("Finish"))
        {
            CurrSpeed = -CurrSpeed / 4;
        }

        //Если тип движения базовый, то сделать отскок
        else if(_movingType == CustomPhysicsType.Basic)
        {
            if((collision.transform.position.x > transform.position.x && CurrSpeedVector.x < 0) || 
               (collision.transform.position.x < transform.position.x && CurrSpeedVector.x > 0)) 
                _tmpX = -CurrSpeedVector.x / 4;
            if((collision.transform.position.y > transform.position.y && CurrSpeedVector.y < 0) || 
               (collision.transform.position.y < transform.position.y && CurrSpeedVector.y > 0)) 
                _tmpY = -CurrSpeedVector.y / 4;
            CurrSpeedVector = new Vector2(_tmpX, _tmpY);
        }
    }

    /// <summary>
    /// Развернуть вектор на определенный угол
    /// </summary>
    /// <param name="v">Изначальный вектор</param>
    /// <param name="degrees">Угол для поворота</param>
    /// <returns></returns>
    public static Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    /// <summary>
    /// Развернуть объект лицом к точке
    /// </summary>
    /// <param name="point">Точка слежения</param>
    private void LookAt(Vector2 point)
    {

        float angle = AngleBetweenPoints(transform.position, point);
        var targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);

    }

    /// <summary>
    /// Рассчитать угол между точками
    /// </summary>
    /// <param name="a">Точка 1</param>
    /// <param name="b">Точка 2</param>
    /// <returns>Угол между точками</returns>
    float AngleBetweenPoints(Vector2 a, Vector2 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    #endregion

    #region Custom Inspector

#if UNITY_EDITOR

    [CustomEditor(typeof(CustomPhysicsController))]
    public class CustomPhysicsControllerEditor : Editor
    {
        CustomPhysicsController targetObject;
        SerializedProperty _movingType;
        SerializedProperty _rb;
        SerializedProperty _pushThanTakeDamage;
        SerializedProperty _pushPowerOnTakenDamage;
        SerializedProperty _target;
        SerializedProperty _distanceToStop;
        SerializedProperty _needRotation;
        SerializedProperty _maxSpeed;
        SerializedProperty _maxBackSpeed;
        SerializedProperty _timeToMaxSpeed;
        SerializedProperty _timeToStopping;
        SerializedProperty _rotateSpeed;
        SerializedProperty _speedOfFadingPushPower;
        SerializedProperty _maxSpeedUsedForSumOFAxis;
        SerializedProperty _currSpeed;
        SerializedProperty _currSpeedVector;
        SerializedProperty _forwardVector;
        SerializedProperty _pushVector;

        void OnEnable()
        {
            _movingType = serializedObject.FindProperty("_movingType");
            _rb = serializedObject.FindProperty("_rb");
            _pushThanTakeDamage = serializedObject.FindProperty("_pushThanTakeDamage");
            _pushPowerOnTakenDamage = serializedObject.FindProperty("_pushPowerOnTakenDamage");
            _target = serializedObject.FindProperty("_target");
            _distanceToStop = serializedObject.FindProperty("_distanceToStop");
            _needRotation = serializedObject.FindProperty("_needRotation");
            _maxSpeed = serializedObject.FindProperty("_maxSpeed");
            _maxBackSpeed = serializedObject.FindProperty("_maxBackSpeed");
            _timeToMaxSpeed = serializedObject.FindProperty("_timeToMaxSpeed");
            _timeToStopping = serializedObject.FindProperty("_timeToStopping");
            _rotateSpeed = serializedObject.FindProperty("_rotateSpeed");
            _speedOfFadingPushPower = serializedObject.FindProperty("_speedOfFadingPushPower");
            _maxSpeedUsedForSumOFAxis = serializedObject.FindProperty("_maxSpeedUsedForSumOFAxis");
            _currSpeed = serializedObject.FindProperty("_currSpeed");
            _currSpeedVector = serializedObject.FindProperty("_currSpeedVector");
            _forwardVector = serializedObject.FindProperty("_forwardVector");
            _pushVector = serializedObject.FindProperty("_pushVector");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            targetObject = target as CustomPhysicsController;
            //base.OnInspectorGUI();


            EditorGUILayout.PropertyField(_rb);
            EditorGUILayout.PropertyField(_pushThanTakeDamage);
            if (targetObject._pushThanTakeDamage)
                EditorGUILayout.PropertyField(_pushPowerOnTakenDamage);

            EditorGUILayout.PropertyField(_movingType);
            switch ((CustomPhysicsType)_movingType.enumValueIndex)
            {
                case CustomPhysicsType.Basic:
                    targetObject._movingType = CustomPhysicsType.Basic;
                    EditorGUILayout.PropertyField(_needRotation);
                    EditorGUILayout.PropertyField(_maxSpeedUsedForSumOFAxis);
                    break;
                case CustomPhysicsType.BasicMovingToTarget:
                    targetObject._movingType = CustomPhysicsType.BasicMovingToTarget;
                    EditorGUILayout.PropertyField(_target);
                    EditorGUILayout.PropertyField(_distanceToStop);
                    EditorGUILayout.PropertyField(_needRotation);
                    break;
                case CustomPhysicsType.Car:
                    targetObject._movingType = CustomPhysicsType.Car;
                    EditorGUILayout.PropertyField(_maxBackSpeed);
                    EditorGUILayout.PropertyField(_rotateSpeed);
                    break;
            }


            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_maxSpeed);
            EditorGUILayout.PropertyField(_timeToMaxSpeed);
            EditorGUILayout.PropertyField(_timeToStopping);
            EditorGUILayout.PropertyField(_speedOfFadingPushPower);

            EditorGUILayout.PropertyField(_currSpeed);
            EditorGUILayout.PropertyField(_currSpeedVector);
            EditorGUILayout.PropertyField(_forwardVector);
            EditorGUILayout.PropertyField(_pushVector);
            serializedObject.ApplyModifiedProperties();



            if (UnityEditor.EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Stop"))
                {
                    targetObject.RigidbodyReinitialize();
                }
            }
        }
    }
#endif
    #endregion
}