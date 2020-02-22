using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Абстрактный класс для пула
/// </summary>
public abstract class Pool : MonoBehaviour {

    //Пул активных элементов
    protected List<IPoolObject> _poolOfEnabled = new List<IPoolObject>();

    //Пул неактивных элементов
    protected List<IPoolObject> _poolOfDisabled = new List<IPoolObject>();

    //Префаб элементов пула
    [SerializeField] [Tooltip("Префаб элементов пула")] private GameObject _poolObjectPrefab;

    public GameObject PoolObjectPrefab { get { return _poolObjectPrefab; } set { _poolObjectPrefab = value; } }

    /// <summary>
    /// Функция, возвращающая элемент пула
    /// </summary>
    /// <param name="_searchParameters">Параметры поиска</param>
    /// <param name="_resultObject">Возращаемый элемент</param>
    /// <returns>Успешность поиска</returns>
    public virtual bool TryGetPoolObject(PoolObjectParameters _searchParameters, ref IPoolObject _resultObject)
    {
        //Перебираем пул активных элементов
        for(int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если мы нашли нужный элемент
            if (_poolOfEnabled[i].GetPoolObjectParameters() == _searchParameters)
            {
                //Заполнить возвращаемый элемент
                _resultObject = _poolOfEnabled[i];

                //Возвратить, что поиск окончился успешно
                return true;
            }
        }
        //Возвратить, что поиск окончился неуспешно
        return false;
    }

    /// <summary>
    /// Фунция проверяющая наличие элемента в пуле
    /// </summary>
    /// <param name="_searchParameters">Параметры поиска</param>
    /// <returns>Наличие или отсутствие в пуле</returns>
    public virtual bool IsPoolObjectExist(PoolObjectParameters _searchParameters)
    {
        //Перебираем пул активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            if (_poolOfEnabled[i].GetPoolObjectParameters() == _searchParameters)
            {
                //Возвратить, что поиск окончился успешно
                return true;
            }
        }
        //Возвратить, что поиск окончился неуспешно
        return false;
    }

    /// <summary>
    /// Функция, описывающая процесс инициализации элемента пула
    /// </summary>
    /// <param name="_createParameter">Параметры создания</param>
    public virtual void InstantiateObject(PoolObjectParameters _createParameter)
    {
        //Если пул выключенных объектов пуст
        if (_poolOfDisabled.Count == 0)
        {
            //Создаем новый элемент
            GameObject newItem = Instantiate(PoolObjectPrefab, transform);
            _poolOfEnabled.Add(newItem.GetComponent<IPoolObject>());
        }
        //Если в пуле выключенных объектов есть элементы
        else
        {
            //Переносим выключенный объект из пула неактивных элементов в пул активных
            _poolOfEnabled.Add(_poolOfDisabled[_poolOfDisabled.Count - 1]);
            _poolOfDisabled.RemoveAt(_poolOfDisabled.Count - 1);
        }
        //Включить элемент
        _poolOfEnabled[_poolOfEnabled.Count - 1].Enable(_createParameter);
    }

    /// <summary>
    /// Функция, описывающая процесс отключения объекта и перемещение его в резерв
    /// </summary>
    /// <param name="_objParameter">Параметры объекта, которого необходимо отключить</param>
    public virtual void DisableObj(PoolObjectParameters _objParameter)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i].GetPoolObjectParameters() == _objParameter)
            {
                //Отключить объект
                _poolOfEnabled[i].DisableObj();
                return;
            }
        }
    }

    /// <summary>
    /// Функция, описывающая процесс отключения объекта и перемещение его в резерв
    /// </summary>
    /// <param name="_object">Объект, который необходимо отключить</param>
    public virtual void DisableObj(IPoolObject _object)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i] == _object)
            {
                //Отключить объект
                _poolOfEnabled[i].DisableObj();
                return;
            }
        }
    }

    /// <summary>
    /// Функция, описывающая происходящее при переходе объекта в отключенный режим.
    /// Необходимо эту функцию подписать на событие отключения в объектах пула.
    /// </summary>
    /// <param name="_object">Объект, который был только что отключен</param>
    public virtual void OnDisabledObj(IPoolObject _object)
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = 0; i < _poolOfEnabled.Count; i++)
        {
            //Если нашли искомый объект
            if (_poolOfEnabled[i] == _object)
            {
                //Переносим объект из пула активных элементов в пул неактивных
                _poolOfDisabled.Add(_poolOfEnabled[i]);
                _poolOfEnabled.RemoveAt(i);
                return;
            }
        }
    }
    /// <summary>
    /// Функция, описывающая процесс отключения всех активных объектов и перемещение их в резерв
    /// </summary>
    public virtual void DisableAllObjects()
    {
        //Ищем указанный объект в пуле активных элементов
        for (int i = _poolOfEnabled.Count - 1; i >= 0; i--)
        {
            //Отключить объект
            _poolOfEnabled[i].DisableObj();
        }
    }
}
