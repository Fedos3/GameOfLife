using GameOfLife.Models;
using GameOfLife.Services;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace GameOfLife.UI
{
    public partial class GameFieldForm : Form, IDisposable
    {
        private readonly GameEngine _gameEngine;
        private readonly GameStateManager _stateManager;
        private readonly AnimationManager _animationManager;
        private readonly System.Windows.Forms.Timer _gameTimer;
        private Panel _gamePanel;
        private Panel _minimapPanel;
        private Panel _toolPanel;
        private Panel _controlPanel;
        private Label _statsLabel;
        private Label _generationLabel;
        private Label _statusLabel;
        private Label _infoLabel;
        private NumericUpDown _speedUpDown;
        private NumericUpDown _widthInput;
        private NumericUpDown _heightInput;
        private ComboBox _neighborhoodComboBox;
        private ComboBox _patternComboBox;
        private TextBox _rulesTextBox;
        private Button _startButton;
        private Button _pauseButton;
        private Button _resetButton;
        private Button _reverseButton;
        private Button _saveButton;
        private Button _loadButton;
        private Button _randomizeButton;
        private Button _editRulesButton;
        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _clearMenuItem;
        private ToolStripMenuItem _randomizeMenuItem;
        private ToolStripMenuItem _saveMenuItem;
        private ToolStripMenuItem _loadMenuItem;
        private Button _brushButton;
        private Button _eraserButton;
        private Point _lastMousePosition;
        private bool _isPanning;
        private bool _isDrawing;
        private float _zoom = 1.0f;
        private PointF _panOffset = new PointF(0, 0);
        private DrawingTool _currentTool = DrawingTool.Brush;
        private const int DEFAULT_WIDTH = 50;
        private const int DEFAULT_HEIGHT = 50;
        private bool _disposed = false;

        private enum DrawingTool
        {
            Brush,
            Eraser
        }

        public GameFieldForm()
        {
            _gameEngine = new GameEngine();
            _gameEngine.Initialize(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            
            _gameEngine.StableStateReached += GameEngine_StableStateReached;
            
            _animationManager = new AnimationManager();
            _stateManager = new GameStateManager();
            
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 100;
            _gameTimer.Tick += GameTimer_Tick;
            _animationManager.AnimationUpdated += (s, e) => _gamePanel?.Invalidate();
            
            KeyPreview = true;
            KeyDown += GameFieldForm_KeyDown;
            
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.BackColor = ThemeManager.Colors.Background;
            this.ForeColor = ThemeManager.Colors.Text;
            this.ClientSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Игра 'Жизнь'";
            
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            
            InitializeUI();
            
            _gameEngine.Randomize(0.2);
            
            UpdateStats();
            UpdateControls();
            
            if (_gamePanel != null) _gamePanel.Invalidate();
            if (_minimapPanel != null) _minimapPanel.Invalidate();
        }

        /// <summary>
        /// Обработчик достижения стабильного состояния
        /// </summary>
        private void GameEngine_StableStateReached(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => GameEngine_StableStateReached(sender, e)));
                return;
            }

            _gameTimer.Stop();
            _statusLabel.Text = "Статус: Стабильно (остановлено)";
            _statusLabel.ForeColor = ThemeManager.Colors.Info;
            UpdateControls();
        }

        /// <summary>
        /// Инициализирует интерфейс
        /// </summary>
        private void InitializeUI()
        {
            InitializeGamePanel();
            InitializeToolPanel();
            InitializeControlPanel();
            InitializeMinimap();
            InitializeContextMenu();
            
            // Важно: перемещение панелей на передний план для правильного отображения
            if (_toolPanel != null) _toolPanel.BringToFront();
            if (_minimapPanel != null) _minimapPanel.BringToFront();
        }

        /// <summary>
        /// Инициализирует игровую панель
        /// </summary>
        private void InitializeGamePanel()
        {
            _gamePanel = new Panel
            {
                Location = new Point(0, 30),
                Size = new Size(this.ClientSize.Width - ThemeManager.Sizes.ControlPanelWidth, this.ClientSize.Height - 200),
                BackColor = ThemeManager.Colors.Background,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            
            typeof(Panel).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, _gamePanel, new object[] { true });
            
            _gamePanel.Paint += GamePanel_Paint;
            _gamePanel.MouseClick += GamePanel_MouseClick;
            _gamePanel.MouseWheel += GamePanel_MouseWheel;
            _gamePanel.MouseDown += GamePanel_MouseDown;
            _gamePanel.MouseMove += GamePanel_MouseMove;
            _gamePanel.MouseUp += GamePanel_MouseUp;
            
            this.Controls.Add(_gamePanel);
        }

        /// <summary>
        /// Инициализирует панель инструментов слева
        /// </summary>
        private void InitializeToolPanel()
        {
            _toolPanel = new Panel
            {
                Location = new Point(10, 40),
                Size = new Size(50, 85),
                BackColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;
            
            _brushButton = CreateToolButton("B", 10, DrawingTool.Brush);
            _brushButton.Click += (s, e) => { 
                _currentTool = DrawingTool.Brush; 
                UpdateToolButtonSelection(); 
            };
            toolTip.SetToolTip(_brushButton, "Кисть (B) - рисует живые клетки");
            
            _eraserButton = CreateToolButton("E", 45, DrawingTool.Eraser);
            _eraserButton.Click += (s, e) => { 
                _currentTool = DrawingTool.Eraser; 
                UpdateToolButtonSelection(); 
            };
            toolTip.SetToolTip(_eraserButton, "Ластик (E) - стирает живые клетки");
            
            _toolPanel.Controls.Add(_brushButton);
            _toolPanel.Controls.Add(_eraserButton);
            
            this.Controls.Add(_toolPanel);
            _toolPanel.BringToFront();
            
            UpdateToolButtonSelection();
        }

        /// <summary>
        /// Инициализирует миникарту
        /// </summary>
        private void InitializeMinimap()
        {
            // Создаем панель для мини-карты
            _minimapPanel = new Panel
            {
                Size = new Size(180, 180),
                BackColor = ThemeManager.Colors.Minimap,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            
            // Включаем двойную буферизацию для панели миникарты через рефлексию
            typeof(Panel).InvokeMember("DoubleBuffered", 
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, _minimapPanel, new object[] { true });
            
            // Обновляем положение миникарты
            UpdateMinimapPosition();
            
            // Добавляем обработчик отрисовки
            _minimapPanel.Paint += MinimapPanel_Paint;
            
            // Добавляем обработчик изменения размера формы
            this.Resize += (s, e) => UpdateMinimapPosition();
            
            // Добавляем миникарту на форму - поверх всех контролов
            this.Controls.Add(_minimapPanel);
            _minimapPanel.BringToFront();
        }

        /// <summary>
        /// Обновляет положение миникарты при изменении размеров формы
        /// </summary>
        private void UpdateMinimapPosition()
        {
            if (_minimapPanel != null)
            {
                _minimapPanel.Location = new Point(10, this.ClientSize.Height - _minimapPanel.Height - 10);
            }
        }

        /// <summary>
        /// Инициализирует контекстное меню
        /// </summary>
        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            _clearMenuItem = new ToolStripMenuItem("Очистить");
            _randomizeMenuItem = new ToolStripMenuItem("Случайно");
            _saveMenuItem = new ToolStripMenuItem("Сохранить");
            _loadMenuItem = new ToolStripMenuItem("Загрузить");

            _clearMenuItem.Click += (s, e) => _gameEngine.Reset();
            _randomizeMenuItem.Click += (s, e) => _gameEngine.Randomize();
            _saveMenuItem.Click += SaveButton_Click;
            _loadMenuItem.Click += LoadButton_Click;

            var items = new ToolStripItem[] {
                _clearMenuItem,
                _randomizeMenuItem,
                new ToolStripSeparator(),
                _saveMenuItem,
                _loadMenuItem
            };
            _contextMenu.Items.AddRange(items);
            
            // Привязываем к игровой панели, если она существует
            if (_gamePanel != null)
            {
                _gamePanel.ContextMenuStrip = _contextMenu;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _gameTimer.Stop();
                    _gameTimer.Tick -= GameTimer_Tick;
                    _gameTimer.Dispose();
                    if (_animationManager is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    if (_gameEngine != null)
                    {
                        _gameEngine.StableStateReached -= GameEngine_StableStateReached; // Отписываемся
                    }
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }

        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (_gameEngine == null) return;

            var g = e.Graphics;
            
            // Отключаем сглаживание для ускорения рендеринга
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            
            // Очищаем только видимую область
            g.Clear(ThemeManager.Colors.Background);

            // Используем размер ячейки из ThemeManager
            var cellSize = ThemeManager.Sizes.CellSize * _zoom;
            var grid = _gameEngine.GetGrid();
            
            // Вычисляем границы видимой области с оптимизацией
            int startX = Math.Max(0, (int)((-_panOffset.X) / cellSize) - 1);
            int startY = Math.Max(0, (int)((-_panOffset.Y) / cellSize) - 1);
            int endX = Math.Min(grid.GetLength(0), startX + (int)(_gamePanel.Width / cellSize) + 2);
            int endY = Math.Min(grid.GetLength(1), startY + (int)(_gamePanel.Height / cellSize) + 2);

            // Создаем повторно используемые объекты для рисования
            using (var aliveBrush = new SolidBrush(ThemeManager.Colors.Alive))
            using (var gridPen = new Pen(ThemeManager.Colors.Grid))
            {
                // Используем кеш преобразований
                for (int y = startY; y < endY; y++)
                {
                    float yPos = y * cellSize + _panOffset.Y;
                    
                    // Если строка не видна - пропускаем
                    if (yPos + cellSize < 0 || yPos > _gamePanel.Height)
                        continue;
                        
                    for (int x = startX; x < endX; x++)
                    {
                        float xPos = x * cellSize + _panOffset.X;
                        
                        // Если ячейка не видна - пропускаем
                        if (xPos + cellSize < 0 || xPos > _gamePanel.Width)
                            continue;
                        
                        var cell = grid[x, y];
                        var rect = new RectangleF(xPos, yPos, cellSize, cellSize);

                        // Рисуем только если клетка жива или если она в поле зрения
                        if (cell.IsAlive)
                        {
                            var point = new Point(x, y);
                            var alpha = _animationManager.GetAnimationAlpha(point);
                            
                            if (alpha < 1.0f)
                            {
                                using (var brush = new SolidBrush(Color.FromArgb(
                                    (int)(255 * alpha), 
                                    ThemeManager.Colors.Alive)))
                                {
                                    g.FillRectangle(brush, rect);
                                }
                            }
                            else
                            {
                                g.FillRectangle(aliveBrush, rect);
                            }
                        }
                        
                        // Рисуем сетку только если масштаб достаточно большой
                        if (_zoom >= 0.5f)
                        {
                            g.DrawRectangle(gridPen, rect.X, rect.Y, rect.Width, rect.Height);
                        }
                    }
                }
            }
        }

        private void GamePanel_MouseClick(object sender, MouseEventArgs e)
        {
            // Этот обработчик больше не нужен для левой кнопки мыши, 
            // так как рисование обрабатывается в MouseDown и MouseMove.
            // Оставляем только для потенциального использования других кнопок (например, контекстное меню)
            // if (e.Button == MouseButtons.Left)
            // {
            //      // ... логика инвертирования была здесь ...
            // }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // Запоминаем предыдущее состояние для визуализации изменений
            var oldGeneration = _gameEngine.GetCurrentGeneration();
            
            // Сохраняем состояние сетки для анимаций
            var grid = _gameEngine.GetGrid();
            var oldCellStates = new Dictionary<Point, bool>();
            
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    if (grid[x, y].IsAlive)
                    {
                        oldCellStates[new Point(x, y)] = true;
                    }
                }
            }
            
            // Обновляем состояние игры
            _gameEngine.Update();
            
            // Запускаем анимации для изменившихся клеток
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    var point = new Point(x, y);
                    bool wasAlive = oldCellStates.ContainsKey(point);
                    bool isAlive = grid[x, y].IsAlive;
                    
                    if (wasAlive != isAlive)
                    {
                        _animationManager.StartAnimation(point);
                    }
                }
            }
            
            // Обновляем только если что-то изменилось
            _gamePanel.Invalidate();
            _minimapPanel.Invalidate();
            
            // Обновляем статистику если изменилось поколение
            if (oldGeneration != _gameEngine.GetCurrentGeneration())
            {
                UpdateStats();
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            // Запускаем игру с нормальным (не обратным) направлением времени
            _gameEngine.Start(false);
            _gameTimer.Start();
            _statusLabel.Text = "Статус: Запущено";
            UpdateControls();
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            _gameEngine.Pause();
            _gameTimer.Stop();
            _statusLabel.Text = "Статус: Приостановлено";
            UpdateControls();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            _gameEngine.Reset();
            _gameTimer.Stop();
            _gamePanel.Invalidate();
            _minimapPanel.Invalidate();
            UpdateStats();
            UpdateControls();
        }

        private void ReverseButton_Click(object sender, EventArgs e)
        {
            _gameEngine.Start(true);
            _gameTimer.Start();
            _gamePanel.Invalidate();
            _minimapPanel.Invalidate();
            UpdateStats();
            UpdateControls();
        }

        private void NeighborhoodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_neighborhoodComboBox.SelectedIndex < 0) return;
            
            string neighborhoodType;
            
            // Преобразуем русские названия окрестностей в английские для GameEngine
            switch (_neighborhoodComboBox.SelectedIndex)
            {
                case 0:
                    neighborhoodType = "Moore";
                    break;
                case 1:
                    neighborhoodType = "VonNeumann";
                    break;
                default:
                    neighborhoodType = "Moore"; // По умолчанию
                    break;
            }
            
            try
            {
                _gameEngine.SetNeighborhoodType(neighborhoodType);
                _infoLabel.Text = $"Правила: {_gameEngine.GetCurrentRules()} | Соседство: {_gameEngine.GetCurrentNeighborhood()}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при установке типа окрестности: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PatternComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_patternComboBox.SelectedIndex == -1) return;

            bool[,] pattern = null;
            
            // Останавливаем игру
            _gameEngine.Stop();
            _gameTimer.Stop();
            
            // Убеждаемся в корректных правилах для всех паттернов
            _gameEngine.SetRules("B3/S23");
            _rulesTextBox.Text = "B3/S23";
            _gameEngine.SetNeighborhoodType("Moore");
            _neighborhoodComboBox.SelectedIndex = 0;
            
            // Получаем паттерн и устанавливаем размер поля
            int width = Math.Max(50, (int)_widthInput.Value);
            int height = Math.Max(50, (int)_heightInput.Value);
            
            switch (_patternComboBox.SelectedIndex)
            {
                case 0: // Блок
                    pattern = Patterns.StillLifes.Block;
                    break;
                case 1: // Улей
                    pattern = Patterns.StillLifes.Beehive;
                    break;
                case 2: // Буханка
                    pattern = Patterns.StillLifes.Loaf;
                    break;
                case 3: // Мигалка
                    pattern = Patterns.Oscillators.Blinker;
                    break;
                case 4: // Жаба
                    pattern = Patterns.Oscillators.Toad;
                    break;
                case 5: // Маяк
                    pattern = Patterns.Oscillators.Beacon;
                    break;
                case 6: // Планер
                    pattern = Patterns.Spaceships.Glider;
                    break;
                case 7: // Легкий корабль
                    pattern = Patterns.Spaceships.LightweightSpaceship;
                    break;
                case 8: // R-пентамино
                    pattern = Patterns.Methuselahs.RPentomino;
                    break;
                case 9: // Diehard
                    pattern = Patterns.Methuselahs.Diehard;
                    break;
            }
            
            if (pattern != null)
            {
                // Сбрасываем поле
                _gameEngine.Initialize(width, height);
                
                // Вычисляем центр поля с запасным пространством
                int centerX = width / 2 - pattern.GetLength(0) / 2;
                int centerY = height / 2 - pattern.GetLength(1) / 2;
                
                // Размещаем паттерн по центру
                _gameEngine.PlacePattern(centerX, centerY, pattern);
                
                // Установка начальных координат просмотра на центр паттерна
                _panOffset = new PointF(
                    _gamePanel.Width / 2 - (centerX + pattern.GetLength(0) / 2) * 20 * _zoom,
                    _gamePanel.Height / 2 - (centerY + pattern.GetLength(1) / 2) * 20 * _zoom
                );
            }

            // Обновляем статус и интерфейс
            _statusLabel.Text = "Статус: Остановлено";
            _gamePanel.Invalidate();
            _minimapPanel.Invalidate();
            UpdateControls();
            UpdateStats();
        }

        private void RulesTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _gameEngine.SetRules(_rulesTextBox.Text);
                _infoLabel.Text = $"Правила: {_gameEngine.GetCurrentRules()} | Соседство: {_gameEngine.GetCurrentNeighborhood()}";
            }
            catch
            {
                // При ошибке не выдаем сообщение, просто обновляем инфо-метку
                _infoLabel.Text = $"Некорректные правила! Используйте формат B3/S23";
            }
        }

        private void SizeInput_ValueChanged(object sender, EventArgs e)
        {
            if (_gameEngine.IsRunning()) return;
            _gameEngine.Initialize((int)_widthInput.Value, (int)_heightInput.Value);
            _gamePanel.Invalidate();
        }

        private void SpeedInput_ValueChanged(object sender, EventArgs e)
        {
            _gameTimer.Interval = (int)_speedUpDown.Value;
        }

        private void UpdateControls()
        {
            var isRunning = _gameEngine.IsRunning();
            _startButton.Enabled = !isRunning;
            _pauseButton.Enabled = isRunning;
            _resetButton.Enabled = true;
            _reverseButton.Enabled = _gameEngine.CanGoBack();
            _randomizeButton.Enabled = !isRunning;
            _editRulesButton.Enabled = !isRunning;
        }

        private void UpdateStats()
        {
            var grid = _gameEngine.GetGrid();
            int aliveCount = 0;
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    if (grid[x, y].IsAlive)
                    {
                        aliveCount++;
                    }
                }
            }

            double density = (double)aliveCount / (grid.GetLength(0) * grid.GetLength(1)) * 100;
            _statsLabel.Text = $"Статистика:\nЖивых клеток: {aliveCount}\nПлотность: {density:F1}%";
            _generationLabel.Text = $"Поколение: {_gameEngine.GetCurrentGeneration()}";
            _infoLabel.Text = $"Правила: {_gameEngine.GetCurrentRules()} | Соседство: {_gameEngine.GetCurrentNeighborhood()}";
        }

        private void MinimapPanel_Paint(object sender, PaintEventArgs e)
        {
            var grid = _gameEngine.GetGrid();
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Очищаем фон
            using (var brush = new SolidBrush(ThemeManager.Colors.Minimap))
            {
                g.FillRectangle(brush, 0, 0, _minimapPanel.Width, _minimapPanel.Height);
            }

            // Вычисляем масштаб для мини-карты
            float scaleX = (float)_minimapPanel.Width / (grid.GetLength(0) * ThemeManager.Sizes.CellSize);
            float scaleY = (float)_minimapPanel.Height / (grid.GetLength(1) * ThemeManager.Sizes.CellSize);
            float scale = Math.Min(scaleX, scaleY);

            // Отрисовка клеток на мини-карте
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y].IsAlive)
                    {
                        using (var brush = new SolidBrush(ThemeManager.Colors.Alive))
                        {
                            g.FillRectangle(brush,
                                x * ThemeManager.Sizes.CellSize * scale,
                                y * ThemeManager.Sizes.CellSize * scale,
                                ThemeManager.Sizes.CellSize * scale - 1,
                                ThemeManager.Sizes.CellSize * scale - 1);
                        }
                    }
                }
            }

            // Отрисовка области просмотра
            using (var pen = new Pen(ThemeManager.Colors.Text, 1))
            {
                float viewX = -_panOffset.X / _zoom;
                float viewY = -_panOffset.Y / _zoom;
                float viewWidth = _gamePanel.Width / _zoom;
                float viewHeight = _gamePanel.Height / _zoom;

                g.DrawRectangle(pen,
                    viewX * scale,
                    viewY * scale,
                    viewWidth * scale,
                    viewHeight * scale);
            }
        }

        private void GamePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // Используем положение курсора как центр масштабирования
            AdjustZoom(e.Delta > 0 ? 1.1f : 0.9f, e.X, e.Y);
        }

        private void GamePanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = true;
                _lastMousePosition = e.Location;
            }
            else if (e.Button == MouseButtons.Left)
            {
                _isDrawing = true;
                // Можно рисовать даже при запущенной симуляции
                HandleDrawing(e.Location);
            }
        }

        private void GamePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                var dx = e.X - _lastMousePosition.X;
                var dy = e.Y - _lastMousePosition.Y;
                _panOffset = new PointF(
                    _panOffset.X + dx,
                    _panOffset.Y + dy
                );
                _lastMousePosition = e.Location;
                _gamePanel.Invalidate();
            }
            else if (_isDrawing)
            {
                // Можно рисовать даже при запущенной симуляции
                HandleDrawing(e.Location);
            }
        }

        private void GamePanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _isPanning = false;
            }
            else if (e.Button == MouseButtons.Left)
            {
                _isDrawing = false;
            }
        }

        private void HandleDrawing(Point location)
        {
            var point = GetGridPoint(location);
            
            if (point.X >= 0 && point.X < _gameEngine.Width &&
                point.Y >= 0 && point.Y < _gameEngine.Height)
            {
                var grid = _gameEngine.GetGrid(); 
                if (point.X < grid.GetLength(0) && point.Y < grid.GetLength(1))
                {
                    var currentCell = grid[point.X, point.Y];
                    bool newState = currentCell.IsAlive;
                    bool changed = false;
                    
                    switch (_currentTool)
                    {
                        case DrawingTool.Brush:
                            if (!currentCell.IsAlive)
                            {
                                newState = true;
                                changed = true;
                            }
                            break;
                        case DrawingTool.Eraser:
                            if (currentCell.IsAlive)
                            {
                                newState = false;
                                changed = true;
                            }
                            break;
                    }
                    
                    if (changed)
                    {
                        _gameEngine.SetCellState(point.X, point.Y, newState);
                        _animationManager.StartAnimation(point);
                            
                        _gamePanel.Invalidate();
                        _gamePanel.Update();
                        
                        _minimapPanel.Invalidate();
                        _minimapPanel.Update();
                        
                        UpdateStats();
                    }
                }
            }
        }

        private void RandomizeButton_Click(object sender, EventArgs e)
        {
            _gameEngine.Stop();
            _gameTimer.Stop();
            _gameEngine.Initialize((int)_widthInput.Value, (int)_heightInput.Value);
            _gameEngine.Randomize(0.3); // 30% заполнение по умолчанию
            _statusLabel.Text = "Статус: Остановлено";
            _gamePanel.Invalidate();
            _minimapPanel.Invalidate();
            UpdateStats();
            UpdateControls();
        }

        private void SaveField()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON files (*.json)|*.json";
                dialog.Title = "Сохранить состояние";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _stateManager.SaveState(_gameEngine, dialog.FileName);
                }
            }
        }

        private void LoadField()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON files (*.json)|*.json";
                dialog.Title = "Загрузить состояние";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var state = _stateManager.LoadState(dialog.FileName);
                    _gameEngine.Initialize(state.Width, state.Height, state.Grid);
                    _gameEngine.SetRules(state.Rules);
                    _gameEngine.SetNeighborhoodType(state.NeighborhoodType);
                    UpdateStats();
                    UpdateControls();
                    Invalidate();
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveField();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            LoadField();
        }

        private void EditRulesButton_Click(object sender, EventArgs e)
        {
            using (var form = new RuleEditorForm(_gameEngine.GetCurrentRules()))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    _gameEngine.SetRules(form.GetRuleString());
                    _rulesTextBox.Text = form.GetRuleString();
                }
            }
        }

        private Point GetGridPoint(Point screenPoint)
        {
            var x = (int)((screenPoint.X - _panOffset.X) / (_zoom * ThemeManager.Sizes.CellSize));
            var y = (int)((screenPoint.Y - _panOffset.Y) / (_zoom * ThemeManager.Sizes.CellSize));
            return new Point(x, y);
        }

        private void GameFieldForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    // Пробел - запуск/пауза
                    if (_gameEngine.IsRunning())
                        PauseButton_Click(null, EventArgs.Empty);
                    else
                        StartButton_Click(null, EventArgs.Empty);
                    break;
                case Keys.R:
                    // R - сброс
                    ResetButton_Click(null, EventArgs.Empty);
                    break;
                case Keys.B:
                    _currentTool = DrawingTool.Brush;
                    UpdateToolButtonSelection();
                    break;
                case Keys.E:
                    _currentTool = DrawingTool.Eraser;
                    UpdateToolButtonSelection();
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    // Плюс - увеличение масштаба
                    AdjustZoom(1.1f, _gamePanel.Width / 2, _gamePanel.Height / 2);
                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    // Минус - уменьшение масштаба
                    AdjustZoom(0.9f, _gamePanel.Width / 2, _gamePanel.Height / 2);
                    break;
            }
        }

        private void AdjustZoom(float factor, int centerX, int centerY)
        {
            var oldZoom = _zoom;
            _zoom = Math.Max(0.1f, Math.Min(5.0f, _zoom * factor));
            
            // Корректируем смещение для сохранения позиции по центру
            _panOffset.X = centerX - (centerX - _panOffset.X) * (_zoom / oldZoom);
            _panOffset.Y = centerY - (centerY - _panOffset.Y) * (_zoom / oldZoom);
            
            _gamePanel.Invalidate();
        }

        /// <summary>
        /// Обновляет визуальное выделение кнопок инструментов
        /// </summary>
        private void UpdateToolButtonSelection()
        {
            Color unselectedColor = Color.FromArgb(80, 80, 80);
            _brushButton.BackColor = unselectedColor;
            _eraserButton.BackColor = unselectedColor;

            Color selectedColor = Color.FromArgb(60, 120, 60);
            switch (_currentTool)
            {
                case DrawingTool.Brush:
                    _brushButton.BackColor = selectedColor;
                    break;
                case DrawingTool.Eraser:
                    _eraserButton.BackColor = selectedColor;
                    break;
            }
        }

        /// <summary>
        /// Создает кнопку с заданными параметрами
        /// </summary>
        private Button CreateButton(string text, int x, int y, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = ThemeManager.Sizes.ButtonWidth,
                Height = ThemeManager.Sizes.ButtonHeight,
                FlatStyle = FlatStyle.Flat,
                BackColor = ThemeManager.Colors.Alive,
                ForeColor = ThemeManager.Colors.Text,
                Font = ThemeManager.Fonts.Normal,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(100, 200, 100);
            button.Click += clickHandler;
            return button;
        }

        /// <summary>
        /// Создает метку с заданными параметрами
        /// </summary>
        private Label CreateLabel(string text, int x, int y, Font font = null)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = font ?? ThemeManager.Fonts.Normal,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.Transparent
            };
        }

        /// <summary>
        /// Создает элемент выбора числа с заданными параметрами
        /// </summary>
        private NumericUpDown CreateNumericUpDown(int x, int y, int width, decimal min, decimal max, decimal value, decimal increment = 1)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Width = width,
                Minimum = min,
                Maximum = max,
                Value = value,
                Increment = increment,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = ThemeManager.Colors.Text
            };
        }

        /// <summary>
        /// Создает выпадающий список с заданными параметрами
        /// </summary>
        private ComboBox CreateComboBox(int x, int y, int width, string[] items)
        {
            var comboBox = new ComboBox
            {
                Location = new Point(x, y),
                Width = width,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = ThemeManager.Colors.Text
            };
            comboBox.Items.AddRange(items);
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        /// <summary>
        /// Создает текстовое поле с заданными параметрами
        /// </summary>
        private TextBox CreateTextBox(int x, int y, int width, string text)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = width,
                Text = text,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = ThemeManager.Colors.Text
            };
        }

        /// <summary>
        /// Создает кнопку инструмента с заданными параметрами
        /// </summary>
        private Button CreateToolButton(string text, int y, DrawingTool tool)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(30, 30);
            button.Location = new Point(10, y);
            button.BackColor = Color.FromArgb(80, 80, 80); // Стандартный фон
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(120, 120, 120);
            button.Font = new Font("Arial", 12, FontStyle.Bold);
            button.Tag = tool;
            
            // Создание уникальной иконки для каждого инструмента
            switch (tool)
            {
                case DrawingTool.Brush:
                    button.Image = CreateBrushIcon();
                    button.TextAlign = ContentAlignment.BottomCenter;
                    button.Text = "";
                    break;
                case DrawingTool.Eraser:
                    button.Image = CreateEraserIcon();
                    button.TextAlign = ContentAlignment.BottomCenter;
                    button.Text = "";
                    break;
            }
            
            return button;
        }

        /// <summary>
        /// Создает иконку для инструмента кисти
        /// </summary>
        private Image CreateBrushIcon()
        {
            Bitmap bmp = new Bitmap(20, 20);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Рисуем кисть
                using (Pen pen = new Pen(Color.White, 2))
                {
                    g.DrawLine(pen, 5, 15, 15, 5);  // Ручка кисти
                    g.FillEllipse(Brushes.White, 3, 12, 6, 5);  // Кончик кисти
                }
            }
            return bmp;
        }

        /// <summary>
        /// Создает иконку для инструмента ластика
        /// </summary>
        private Image CreateEraserIcon()
        {
            Bitmap bmp = new Bitmap(20, 20);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Рисуем ластик
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brush, 5, 5, 10, 10);
                    g.DrawRectangle(Pens.White, 5, 5, 10, 10);
                }
            }
            return bmp;
        }

        /// <summary>
        /// Инициализирует панель управления
        /// </summary>
        private void InitializeControlPanel()
        {
            var controlContainer = new Panel
            {
                Dock = DockStyle.Right,
                Width = ThemeManager.Sizes.ControlPanelWidth,
                AutoScroll = true,
                BackColor = ThemeManager.Colors.PanelBackground
            };
            
            _controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Width = ThemeManager.Sizes.ControlPanelWidth - 20,
                Height = 1050,
                AutoSize = true,
                BackColor = ThemeManager.Colors.PanelBackground
            };
            
            controlContainer.Controls.Add(_controlPanel);
            
            var titleLabel = new Label
            {
                Text = "Управление",
                Font = ThemeManager.Fonts.Title,
                ForeColor = ThemeManager.Colors.Text,
                Location = new Point(20, 20),
                AutoSize = true
            };

            var controlsGroup = new GroupBox
            {
                Text = "Управление симуляцией",
                Location = new Point(10, 60),
                Width = 210,
                Height = 200,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            _startButton = CreateButton("Старт", 20, 30, StartButton_Click);
            _pauseButton = CreateButton("Пауза", 20, 70, PauseButton_Click);
            _resetButton = CreateButton("Сброс", 20, 110, ResetButton_Click);
            _reverseButton = CreateButton("Реверс", 20, 150, ReverseButton_Click);
            
            _startButton.Width = 160;
            _pauseButton.Width = 160;
            _resetButton.Width = 160;
            _reverseButton.Width = 160;
            
            controlsGroup.Controls.AddRange(new Control[] {
                _startButton, _pauseButton, _resetButton, _reverseButton
            });
            
            var fileGroup = new GroupBox
            {
                Text = "Файл",
                Location = new Point(10, 270),
                Width = 210,
                Height = 120,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            _saveButton = CreateButton("Сохранить", 20, 30, SaveButton_Click);
            _loadButton = CreateButton("Загрузить", 20, 70, LoadButton_Click);
            
            _saveButton.Width = 160;
            _loadButton.Width = 160;
            
            fileGroup.Controls.AddRange(new Control[] {
                _saveButton, _loadButton
            });
            
            var toolsGroup = new GroupBox
            {
                Text = "Инструменты",
                Location = new Point(10, 400),
                Width = 210,
                Height = 120,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            _randomizeButton = CreateButton("Случайное поле", 20, 30, RandomizeButton_Click);
            _editRulesButton = CreateButton("Редактировать правила", 20, 70, EditRulesButton_Click);
            
            _randomizeButton.Width = 160;
            _editRulesButton.Width = 160;
            
            toolsGroup.Controls.AddRange(new Control[] {
                _randomizeButton, _editRulesButton
            });
            
            var statsGroup = new GroupBox
            {
                Text = "Информация",
                Location = new Point(10, 530),
                Width = 210,
                Height = 120,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            _generationLabel = CreateLabel("Поколение: 1", 20, 30, ThemeManager.Fonts.Subtitle);
            _statsLabel = CreateLabel("Статистика:\nЖивых клеток: 0\nПлотность: 0%", 20, 60);
            _generationLabel.AutoSize = true;
            _statsLabel.AutoSize = true;
            
            statsGroup.Controls.AddRange(new Control[] {
                _generationLabel, _statsLabel
            });
            
            var sizeGroup = new GroupBox
            {
                Text = "Размер поля",
                Location = new Point(10, 650),
                Width = 210,
                Height = 80,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            var widthLabel = CreateLabel("Ширина:", 20, 30);
            var heightLabel = CreateLabel("Высота:", 120, 30);
            _widthInput = CreateNumericUpDown(20, 50, 80, 10, 100, 30);
            _heightInput = CreateNumericUpDown(120, 50, 80, 10, 100, 20);
            
            sizeGroup.Controls.AddRange(new Control[] {
                widthLabel, heightLabel, _widthInput, _heightInput
            });
            
            var speedGroup = new GroupBox
            {
                Text = "Настройки",
                Location = new Point(10, 740),
                Width = 210,
                Height = 150,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            var speedLabel = CreateLabel("Скорость (мс):", 20, 30);
            _speedUpDown = CreateNumericUpDown(20, 50, 160, 50, 1000, 100, 50);
            
            var neighborhoodLabel = CreateLabel("Тип соседства:", 20, 80);
            _neighborhoodComboBox = CreateComboBox(20, 100, 160, new[] { "Мура", "Фон Нейман" });
            
            speedGroup.Controls.AddRange(new Control[] {
                speedLabel, _speedUpDown, neighborhoodLabel, _neighborhoodComboBox
            });
            
            var patternGroup = new GroupBox
            {
                Text = "Шаблоны",
                Location = new Point(10, 900),
                Width = 210,
                Height = 140,
                ForeColor = ThemeManager.Colors.Text,
                BackColor = Color.FromArgb(40, 40, 40)
            };
            
            var patternLabel = CreateLabel("Выберите шаблон:", 20, 30);
            _patternComboBox = CreateComboBox(20, 50, 160, new[] {
                "Блок", "Улей", "Буханка",
                "Мигалка", "Жаба", "Маяк",
                "Планер", "Легкий корабль",
                "R-пентамино", "Diehard"
            });
            
            var rulesLabel = CreateLabel("Правила:", 20, 80);
            _rulesTextBox = CreateTextBox(20, 100, 160, "B3/S23");
            
            patternGroup.Controls.AddRange(new Control[] {
                patternLabel, _patternComboBox, rulesLabel, _rulesTextBox
            });
            
            _statusLabel = CreateLabel("Статус: Остановлено", 20, 1050);
            _infoLabel = CreateLabel("Правила: B3/S23 | Соседство: Мура", 20, 1080);
            _statusLabel.AutoSize = true;
            _infoLabel.AutoSize = true;

            _neighborhoodComboBox.SelectedIndexChanged += NeighborhoodComboBox_SelectedIndexChanged;
            _patternComboBox.SelectedIndexChanged += PatternComboBox_SelectedIndexChanged;
            _rulesTextBox.TextChanged += RulesTextBox_TextChanged;
            _widthInput.ValueChanged += SizeInput_ValueChanged;
            _heightInput.ValueChanged += SizeInput_ValueChanged;
            _speedUpDown.ValueChanged += SpeedInput_ValueChanged;

            _controlPanel.Controls.AddRange(new Control[] {
                titleLabel, controlsGroup, fileGroup, toolsGroup, statsGroup, 
                sizeGroup, speedGroup, patternGroup, _statusLabel, _infoLabel
            });
            
            this.Controls.Add(controlContainer);
        }
    }
} 