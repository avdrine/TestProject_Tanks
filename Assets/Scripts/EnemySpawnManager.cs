using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Класс, описывающий контроллер спавна противников
/// </summary>
public class EnemySpawnManager : MonoBehaviour 
{
    #region Vars

    /// <summary>
    /// Точки спавна
    /// </summary>
    [SerializeField] [Tooltip("Точки спавна")] private Transform[] _spawnPoints;
    
    /// <summary>
    /// Количество спавнов с точек
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Количество спавнов с точек")] private List<int> _spawnCountOnPoints = new List<int>();

    /// <summary>
    /// Префабы противников
    /// </summary>
    [SerializeField] [Tooltip("Префабы противников")] private GameObject[] _enemiesPrefabs;

    /// <summary>
    /// Количество спавнов разных видов противников
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Количество спавнов разных видов противников")] private List<int> _spawnCountOfEnemies = new List<int>();

    /// <summary>
    /// Пулы для спавна противников
    /// </summary>
    private List<PoolOfEnemies> _pullsOfSpawnedEnemies = new List<PoolOfEnemies>();

    #region Parameters

    /// <summary>
    /// Максимальное количество заспавненных противников на сцене
    /// </summary>
    [Header("Parameters")]
    [SerializeField] [Range(0, 100)] [Tooltip("Максимальное количество заспавненных противников на сцене")] private int _maxCountsOfEnemies;

    /// <summary>
    /// Таймаут для спавна противников
    /// </summary>
    [SerializeField] [Range(0, 100)] [Tooltip("Таймаут для спавна противников")] private float _timeToPauseToSpawn;

    /// <summary>
    /// Время последнего спавна противника
    /// </summary>
    private float _timeOfLastSpawn;

    /// <summary>
    /// Пауза в спавне противников
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Пауза в спавне противников")] private bool _pauseToSpawn = false;

    /// <summary>
    /// Количество заспавненных противников в данный момент
    /// </summary>
    [ReadOnly] [SerializeField] [Tooltip("Количество заспавненных противников в данный момент")] private int _spawnedEnemies;

    /// <summary>
    /// Время активации паузы
    /// </summary>
    private float _timeOfPauseActivated;

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Пауза в спавне противников
    /// </summary>
    public bool PauseToSpawn 
    { 
        get { return _pauseToSpawn; }
        set
        {
            if (value)
            {
                _timeOfPauseActivated = Time.time;
            }
            else
            {
                _timeOfLastSpawn += Time.time - _timeOfPauseActivated;
            }
            _pauseToSpawn = value;
        } 
    }

    #endregion

    #region Functions

    /// <summary>
    /// Пересчитать количество заспавненных противников
    /// </summary>
    public void UpdateSpawnedEnemiesCount()
    {
        _spawnedEnemies = 0;

        //Посчитать количество заспавненных противников в каждом пуле
        for (int i = 0; i < _pullsOfSpawnedEnemies.Count; i++) _spawnedEnemies += _pullsOfSpawnedEnemies[i].GetActiveObjectsCount();
    }

    void Start () 
    {
        //Проинициализировать количество спавнов с точек
        for (int i = 0; i < _spawnPoints.Length; i++)
            _spawnCountOnPoints.Add(0);

        //Проинициализировать количество спавнов разных видов противников и создать пулы для них
        for (int i = 0; i < _enemiesPrefabs.Length; i++)
        {
            _spawnCountOfEnemies.Add(0);
            GameObject tmpGo = new GameObject();
            tmpGo.transform.SetParent(transform);
            tmpGo.name = "PullOf" + _enemiesPrefabs[i].name;
            tmpGo.AddComponent<PoolOfEnemies>();
            _pullsOfSpawnedEnemies.Add(tmpGo.GetComponent<PoolOfEnemies>());
            _pullsOfSpawnedEnemies[i].PoolObjectPrefab = _enemiesPrefabs[i];
            _pullsOfSpawnedEnemies[i].OnDisablePoolObject.AddListener(UpdateSpawnedEnemiesCount);
            _pullsOfSpawnedEnemies[i].OnActivatePoolObject.AddListener(UpdateSpawnedEnemiesCount);
        }

    }
	
	void Update () 
    {
        //Если не включена пауза в спавне и прошел таймаут - заспавнить противника
		if(!PauseToSpawn && Time.time - _timeOfLastSpawn > _timeToPauseToSpawn)
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// Спавн противника
    /// </summary>
    /// <param name="ignoreRequirements">Игнорировать условия спавна</param>
    public void SpawnEnemy(bool ignoreRequirements = false)
    {
        if ((!ignoreRequirements && _spawnedEnemies < _maxCountsOfEnemies) || ignoreRequirements)
        {
            _pullsOfSpawnedEnemies[GetIndexOfRareUsedObjectInMassive(_spawnCountOfEnemies)].InstantiateObject(_spawnPoints[GetIndexOfRareUsedObjectInMassive(_spawnCountOnPoints)].position);
            _timeOfLastSpawn = Time.time;
        }
    }

    /// <summary>
    /// Буффер для минимального значения
    /// </summary>
    private int _minCount;

    /// <summary>
    /// Буффер для индексов элементов листа с минимальным значением
    /// </summary>
    private List<int> _minIndexes = new List<int>(); 

    /// <summary>
    /// Получить случайный индекс с минимальным значением 
    /// </summary>
    /// <param name="massiveOfUsedCount">Лист количества использований</param>
    /// <returns>Случайный индекс с минимальным количеством использований</returns>
    private int GetIndexOfRareUsedObjectInMassive(List<int> massiveOfUsedCount)
    {
        if (massiveOfUsedCount.Count > 1)
        {
            _minCount = -1;
            _minIndexes.Clear();
            for (int i = 0; i < massiveOfUsedCount.Count; i++)
            {
                if (_minCount > massiveOfUsedCount[i] || _minCount == -1)
                {
                    _minCount = massiveOfUsedCount[i];
                    _minIndexes.Clear();
                    _minIndexes.Add(i);
                }
                else if (_minCount == massiveOfUsedCount[i])
                {
                    _minIndexes.Add(i);
                }
            }
            int result;
            if (_minIndexes.Count > 1)
                result = _minIndexes[Random.Range(0, _minIndexes.Count)];
            else
                result = _minIndexes[0];
            massiveOfUsedCount[result]++;
            return result;
        }
        else return 0;
    }

    /// <summary>
    /// Отключить всех противников
    /// </summary>
    public void KillAllEnemies()
    {
        for(int i = 0; i < _pullsOfSpawnedEnemies.Count; i++)
        {
            _pullsOfSpawnedEnemies[i].DisableAllObjects();
        }
    }

    #endregion

    #region Debug control buttons in Inspector

#if UNITY_EDITOR
    [CustomEditor(typeof(EnemySpawnManager))]
    public class EnemySpawnManagerEditor : Editor
    {
        EnemySpawnManager targetObject;
        public override void OnInspectorGUI()
        {
            targetObject = target as EnemySpawnManager;
            if (UnityEditor.EditorApplication.isPlaying)
            {
                GUILayout.Space(10);
                GUILayout.Label("Control buttons");
                if (GUILayout.Button("Try spawn enemy"))
                {
                    targetObject.SpawnEnemy();
                }
                if (GUILayout.Button("Spawn enemy (ignore requirements)"))
                {
                    targetObject.SpawnEnemy(true);
                }
                if (GUILayout.Button("Kill all enemies"))
                {
                    targetObject.KillAllEnemies();
                }
                GUILayout.Space(20);
            }
            base.OnInspectorGUI();
        }
    }
#endif

    #endregion
}
