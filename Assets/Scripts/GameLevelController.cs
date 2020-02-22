using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Перечисление, описывающее состояния игрового процесса
/// </summary>
public enum GameState { GameStarted, GameOver }

/// <summary>
/// Класс, описывающий контроллер игрового процесса
/// </summary>
public class GameLevelController : MonoBehaviour 
{
    /// <summary>
    /// Singleton
    /// </summary>
    static public GameLevelController Instance;

    #region Vars

    /// <summary>
    /// Ссылка на GUIController
    /// </summary>
    [SerializeField] [Tooltip("Перенесите локальный компонент GUIController")] private GUIController _GUIController;

    /// <summary>
    /// Ссылка на EnemySpawnManager
    /// </summary>
    [SerializeField] [Tooltip("Перенесите компонент EnemySpawnManager со сцены")] private EnemySpawnManager _enemySpawnManager;

    /// <summary>
    /// Ссылка на PlayerController
    /// </summary>
    [SerializeField] [Tooltip("Перенесите компонент игрока со сцены")] private PlayerController _playerController;

    /// <summary>
    /// Текущее состояние игрового процесса
    /// </summary>
    [SerializeField] [Tooltip("Текущее состояние игрового процесса")] private GameState _gameState;

    /// <summary>
    /// Ссылки на пулы
    /// </summary>
    [SerializeField] [Tooltip("Пулы игровых объектов, которые будут отключаться при GameOver")] private GameObject[] pools;

    #endregion

    #region Properties

    /// <summary>
    /// компонент GUIController
    /// </summary>
    public GUIController GUIController { get { return _GUIController; } }

    /// <summary>
    /// Текущее состояние игрового процесса
    /// </summary>
    public GameState GameState { get { return _gameState; } }

    #endregion

    #region Functions

    void Awake()
    {
        //Заполнение Singleton
        if(Instance == null) Instance = this;
    }

    /// <summary>
    /// Отключение всех объектов в пулах
    /// </summary>
    private void DisableAllObjectsInPools()
    {
        for(int i = 0; i < pools.Length; i++)
        {
            pools[i].GetComponent<Pool>().DisableAllObjects();
        }
    }
    
    /// <summary>
    /// Переключение игрового состояние в GameOver
    /// </summary>
    public void GameOver()
    {
        //Убить всех противников
        _enemySpawnManager.KillAllEnemies();

        //Установить паузу на спавн противников
        _enemySpawnManager.PauseToSpawn = true;

        //Отключить игрока
        _playerController.gameObject.SetActive(false);

        //Изменить текущее состояние игрового процесса
        _gameState = GameState.GameOver;

        //Отключить все объекты в пулах
        DisableAllObjectsInPools();

        //Передать статус GameOver в GUI
        GUIController.ChangeGameOverState(true);
    }

    /// <summary>
    /// Переключение игрового состояние в GameStarted
    /// </summary>
    public void StartGame()
    {
        //Снять паузу на спавн противников
        _enemySpawnManager.PauseToSpawn = false;

        //Заспавнить игрока
        _playerController.Spawn();

        //Изменить текущее состояние игрового процесса
        _gameState = GameState.GameStarted;

        //Передать статус GameOver в GUI
        GUIController.ChangeGameOverState(false);
    }

   void Update()
    {
        // Если состояние игрового процесса - GameOver и была нажата клавиша F - перезапустить игру
        if(_gameState == GameState.GameOver && Input.GetKeyDown(KeyCode.F))
        {
            StartGame();
        }
    }

    #endregion
}
