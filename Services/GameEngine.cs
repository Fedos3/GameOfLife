using GameOfLife.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GameOfLife.Services
{
    /// <summary>
    /// Структура для хранения состояния игры
    /// </summary>
    internal struct GameStateData
    {
        public int Generation { get; }
        public bool[,] CellStates { get; }

        public GameStateData(int generation, bool[,] cellStates)
        {
            Generation = generation;
            CellStates = cellStates;
        }
    }
    
    /// <summary>
    /// Основной движок игры, обрабатывающий логику "Жизни"
    /// </summary>
    public class GameEngine : IGameEngine
    {
        // Основные компоненты
        private readonly Cell[,] _cells;
        private readonly Cell[,] _previousCells;
        private RuleParser _ruleParser;
        private INeighborhood _neighborhood;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly Random _random = new Random();

        // Состояние игры
        private bool _isRunning;
        private bool _isPaused;
        private bool _isReverseTime;
        private int _currentGeneration;
        private int _width;
        private int _height;
        
        // История состояний
        private readonly Stack<GameStateData> _history;
        private const int MAX_HISTORY_SIZE = 100;
        private bool _canGoBack;

        public int Width => _width;
        public int Height => _height;

        public event EventHandler StableStateReached;

        public GameEngine()
        {
            _ruleParser = new RuleParser("B3/S23");
            _neighborhood = new MooreNeighborhood();
            _history = new Stack<GameStateData>();
            
            _timer = new System.Windows.Forms.Timer
            {
                Interval = 100,
                Enabled = false
            };
            _timer.Tick += (s, e) => Update();
            
            _width = 50;
            _height = 50;
            _cells = new Cell[_width, _height];
            _previousCells = new Cell[_width, _height];
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _cells[x, y] = new Cell(false, x, y);
                    _previousCells[x, y] = new Cell(false, x, y);
                }
            }
        }

        #region Управление игрой

        /// <summary>
        /// Инициализирует новое игровое поле с указанными размерами
        /// </summary>
        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                    {
                        _cells[x, y] = new Cell(false, x, y);
                        _previousCells[x, y] = new Cell(false, x, y);
                    }
                }
            }
            
            _currentGeneration = 0;
            _isRunning = false;
            _isPaused = false;
            _isReverseTime = false;
            _history.Clear();
            _canGoBack = false;
        }

        /// <summary>
        /// Инициализирует игровое поле с указанным шаблоном
        /// </summary>
        public void Initialize(int width, int height, bool[,] pattern)
        {
            Initialize(width, height);
            PlacePattern(0, 0, pattern);
        }

        /// <summary>
        /// Помещает шаблон на игровое поле по указанным координатам
        /// </summary>
        public void PlacePattern(int startX, int startY, bool[,] pattern)
        {
            int patternWidth = pattern.GetLength(0);
            int patternHeight = pattern.GetLength(1);

            for (int y = 0; y < patternHeight; y++)
            {
                for (int x = 0; x < patternWidth; x++)
                {
                    int gridX = startX + x;
                    int gridY = startY + y;

                    if (IsInBounds(gridX, gridY))
                    {
                        _cells[gridX, gridY] = new Cell(pattern[x, y], gridX, gridY);
                    }
                }
            }
            
            SaveCurrentState();
        }

        /// <summary>
        /// Запускает симуляцию
        /// </summary>
        public void Start(bool reverse = false)
        {
            _isRunning = true;
            _isPaused = false;
            _isReverseTime = reverse;
            _timer.Start();
        }

        /// <summary>
        /// Приостанавливает симуляцию
        /// </summary>
        public void Pause()
        {
            if (_isRunning)
            {
                _isPaused = true;
                _timer.Stop();
            }
        }

        /// <summary>
        /// Возобновляет симуляцию
        /// </summary>
        public void Resume()
        {
            if (_isRunning && _isPaused)
            {
                _isPaused = false;
                _timer.Start();
            }
        }

        /// <summary>
        /// Останавливает симуляцию
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
            _timer.Stop();
        }

        /// <summary>
        /// Сбрасывает игровое поле
        /// </summary>
        public void Reset()
        {
            Stop();
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                    {
                        _cells[x, y] = new Cell(false, x, y);
                    }
                }
            }
            
            _currentGeneration = 0;
            _history.Clear();
            _canGoBack = false;
        }

        /// <summary>
        /// Заполняет поле случайным образом
        /// </summary>
        public void Randomize(double density = 0.3)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                    {
                        _cells[x, y] = new Cell(_random.NextDouble() < density, x, y);
                    }
                }
            }
            
            _currentGeneration = 0;
            _history.Clear();
            _canGoBack = false;
            SaveCurrentState();
        }

        /// <summary>
        /// Устанавливает состояние клетки по указанным координатам.
        /// </summary>
        public void SetCellState(int x, int y, bool isAlive)
        {
            if (IsInBounds(x, y))
            {
                _cells[x, y] = new Cell(isAlive, x, y);
                
                // TODO: Возможно, стоит добавить сохранение состояния в историю здесь?
            }
        }

        #endregion

        #region Обновление состояния

        /// <summary>
        /// Обновляет состояние игры на следующее поколение
        /// </summary>
        public void Update()
        {
            if (!_isRunning || _isPaused) return;

            if (_isReverseTime)
            {
                GoToPreviousGeneration();
            }
            else
            {
                GoToNextGeneration();
            }
        }

        /// <summary>
        /// Переходит к предыдущему поколению, используя историю
        /// </summary>
        public void GoToPreviousGeneration()
        {
            if (_history.Count > 0)
            {
                GameStateData previousState = _history.Pop();
                
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                        {
                            _cells[x, y] = new Cell(previousState.CellStates[x, y], x, y);
                        }
                    }
                }
                
                _currentGeneration = previousState.Generation;
                _canGoBack = _history.Count > 0;
            }
        }

        /// <summary>
        /// Переходит к следующему поколению, вычисляя новое состояние
        /// </summary>
        public void GoToNextGeneration()
        {
            SaveCurrentState();
            UpdateGrid();
        }

        /// <summary>
        /// Обновляет состояние игры на одно поколение вперед
        /// </summary>
        private void UpdateGrid()
        {
            bool isStable = true;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                    {
                        _previousCells[x, y] = new Cell(_cells[x, y].IsAlive, x, y);
                    }
                }
            }
            
            var temporaryGrid = new Grid(_width, _height);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _previousCells.GetLength(0) && y < _previousCells.GetLength(1))
                    {
                        temporaryGrid.SetCellState(x, y, _previousCells[x, y].IsAlive);
                    }
                }
            }
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                    {
                        var neighbors = _neighborhood.GetNeighbors(temporaryGrid, x, y);
                        
                        int aliveNeighbors = 0;
                        foreach (var cell in neighbors)
                        {
                            if (cell.IsAlive) aliveNeighbors++;
                        }
                        
                        bool isCurrentlyAlive = _previousCells[x, y].IsAlive;
                        bool willBeAlive;
                        
                        if (isCurrentlyAlive)
                        {
                            willBeAlive = _ruleParser.SurvivalRule(aliveNeighbors);
                        }
                        else
                        {
                            willBeAlive = _ruleParser.BirthRule(aliveNeighbors);
                        }
                        
                        _cells[x, y] = new Cell(willBeAlive, x, y);
                        
                        if (isCurrentlyAlive != willBeAlive)
                        {
                            isStable = false;
                        }
                    }
                }
            }
            
            if (!isStable)
            {
                _currentGeneration++;
            }
            else
            {
                Stop();
                OnStableStateReached();
            }
        }

        /// <summary>
        /// Обновляет состояние клетки в сетке
        /// </summary>
        private void UpdateCellInGrid(Grid targetGrid, int x, int y, bool isAlive)
        {
            targetGrid.SetCellState(x, y, isAlive);
        }

        #endregion

        #region Управление настройками

        /// <summary>
        /// Устанавливает правила игры
        /// </summary>
        public void SetRules(string rules)
        {
            _ruleParser = new RuleParser(rules);
        }

        /// <summary>
        /// Возвращает текущие правила игры
        /// </summary>
        public string GetCurrentRules()
        {
            return _ruleParser.ToString();
        }

        /// <summary>
        /// Устанавливает тип соседства
        /// </summary>
        public void SetNeighborhoodType(string type)
        {
            _neighborhood = type.ToLower() == "von neumann" || type.ToLower() == "фон нейман" 
                ? (INeighborhood)new VonNeumannNeighborhood() 
                : new MooreNeighborhood();
        }

        /// <summary>
        /// Возвращает текущий тип соседства
        /// </summary>
        public string GetCurrentNeighborhood()
        {
            return _neighborhood is MooreNeighborhood ? "Moore" : "Von Neumann";
        }

        /// <summary>
        /// Устанавливает интервал обновления
        /// </summary>
        public void SetInterval(int interval)
        {
            _timer.Interval = interval;
        }

        #endregion

        #region Вспомогательные методы и реализация интерфейса

        /// <summary>
        /// Проверяет, находятся ли координаты в пределах поля
        /// </summary>
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height && 
                   x < _cells.GetLength(0) && y < _cells.GetLength(1);
        }

        /// <summary>
        /// Возвращает сетку клеток
        /// </summary>
        public ICell[,] GetGrid()
        {
            ICell[,] result = new ICell[_width, _height];
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                    {
                        result[x, y] = _cells[x, y];
                    }
                    else
                    {
                        result[x, y] = new Cell(false, x, y);
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Возвращает текущее поколение
        /// </summary>
        public int GetCurrentGeneration()
        {
            return _currentGeneration;
        }
        
        /// <summary>
        /// Проверяет, запущена ли игра
        /// </summary>
        public bool IsRunning()
        {
            return _isRunning;
        }
        
        /// <summary>
        /// Проверяет, включен ли реверсивный режим
        /// </summary>
        public bool IsReverseTime()
        {
            return _isReverseTime;
        }

        /// <summary>
        /// Подсчитывает количество живых клеток
        /// </summary>
        public int CountAliveCells()
        {
            int count = 0;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1) && _cells[x, y].IsAlive)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Сохраняет текущее состояние в историю
        /// </summary>
        private void SaveCurrentState()
        {
            if (_history.Count >= MAX_HISTORY_SIZE)
            {
                Stack<GameStateData> tempStack = new Stack<GameStateData>();
                
                int toKeep = MAX_HISTORY_SIZE / 2;
                for (int i = 0; i < toKeep && _history.Count > 0; i++)
                {
                    tempStack.Push(_history.Pop());
                }
                
                _history.Clear();
                
                while (tempStack.Count > 0)
                {
                    _history.Push(tempStack.Pop());
                }
            }
            
            bool[,] cellStates = new bool[_width, _height];
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (x < _cells.GetLength(0) && y < _cells.GetLength(1))
                    {
                        cellStates[x, y] = _cells[x, y].IsAlive;
                    }
                }
            }
            
            _history.Push(new GameStateData(_currentGeneration, cellStates));
            _canGoBack = true;
        }

        /// <summary>
        /// Проверяет возможность вернуться назад
        /// </summary>
        public bool CanGoBack()
        {
            return _history.Count > 0 && _canGoBack;
        }

        /// <summary>
        /// Вызывает событие StableStateReached.
        /// </summary>
        protected virtual void OnStableStateReached()
        {
            StableStateReached?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
} 


