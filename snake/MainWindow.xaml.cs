using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace snake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SnakeGame snakeGame;
        private DispatcherTimer gameTimer;
        private int squareSize = 20;
        private int score = 0;
        private TimeSpan gameTime; // Время игры

        public MainWindow()
        {
            InitializeComponent();
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(100); // Скорость игры
            gameTimer.Tick += GameTimer_Tick;

            gameTime = TimeSpan.Zero; // Начальное значение времени игры

            this.Loaded += MainWindow_Loaded; // Устанавливаем фокус при загрузке
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focus(); // Сразу устанавливаем фокус на окно
        }

        // Обработка нажатия на кнопку "Start Game"
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            bool autoPlay = AutoPlayCheckBox.IsChecked == true;
            snakeGame = new SnakeGame(GameCanvas, squareSize, autoPlay);
            score = 0;
            ScoreText.Text = "Score: 0";
            gameTime = TimeSpan.Zero; // Обнуляем время при старте игры
            TimerText.Text = "Time: 0"; // Обнуляем отображение времени

            gameTimer.Start(); // Запускаем таймер игры
            this.Focus(); // Устанавливаем фокус на окно после нажатия кнопки
        }

        // Обновление змейки по таймеру
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            gameTime += gameTimer.Interval; // Обновляем время игры
            TimerText.Text = $"Time: {gameTime.TotalSeconds:F1}"; // Отображаем время в интерфейсе

            snakeGame.Update();
            if (snakeGame.GameOver)
            {
                gameTimer.Stop();
                MessageBox.Show("Game Over! Final Score: " + score);
            }
            else
            {
                score = snakeGame.Score;
                ScoreText.Text = "Score: " + score;
            }
        }

        // Обработка нажатий на клавиши для управления змейкой
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!snakeGame.IsAutoPlay) // Если игра не автономная
            {
                if (e.Key == Key.Up) snakeGame.ChangeDirection(Direction.Up);
                if (e.Key == Key.Down) snakeGame.ChangeDirection(Direction.Down);
                if (e.Key == Key.Left) snakeGame.ChangeDirection(Direction.Left);
                if (e.Key == Key.Right) snakeGame.ChangeDirection(Direction.Right);
            }

            this.Focus(); // Снова устанавливаем фокус на окно после нажатия клавиши
        }
    }

    public class SnakeGame
    {
        private Canvas gameCanvas;
        private int squareSize;
        private List<Point> snake;
        private Point food;
        private Direction currentDirection;
        private Direction nextDirection;
        public bool IsAutoPlay { get; private set; }
        public bool GameOver { get; private set; }
        public int Score { get; private set; }

        private Random random;

        public SnakeGame(Canvas canvas, int size, bool autoPlay)
        {
            gameCanvas = canvas;
            squareSize = size;
            IsAutoPlay = autoPlay;

            snake = new List<Point> { new Point(100, 100), new Point(80, 100), new Point(60, 100) };
            currentDirection = Direction.Right;
            nextDirection = Direction.Right;

            random = new Random();
            PlaceFood();

            Draw(); // Рисуем начальное состояние змейки и еды
        }

        public void Update()
        {
            if (GameOver) return;

            if (IsAutoPlay)
            {
                AutoPlay(); // Логика автономного режима
            }

            MoveSnake(); // Перемещаем змейку
            CheckCollision(); // Проверяем столкновения
            Draw(); // Перерисовываем змейку и еду
        }

        public void ChangeDirection(Direction newDirection)
        {
            if ((newDirection == Direction.Up && currentDirection != Direction.Down) ||
                (newDirection == Direction.Down && currentDirection != Direction.Up) ||
                (newDirection == Direction.Left && currentDirection != Direction.Right) ||
                (newDirection == Direction.Right && currentDirection != Direction.Left))
            {
                nextDirection = newDirection;
            }
        }

        private void MoveSnake()
        {
            currentDirection = nextDirection;

            Point head = snake.First();
            Point newHead = head;

            switch (currentDirection)
            {
                case Direction.Up: newHead.Y -= squareSize; break;
                case Direction.Down: newHead.Y += squareSize; break;
                case Direction.Left: newHead.X -= squareSize; break;
                case Direction.Right: newHead.X += squareSize; break;
            }

            snake.Insert(0, newHead);
            if (newHead == food)
            {
                Score++;
                PlaceFood(); // Размещаем новую еду
            }
            else
            {
                snake.RemoveAt(snake.Count - 1); // Убираем хвост, если еду не съели
            }
        }

        private void CheckCollision()
        {
            Point head = snake.First();

            // Проверка столкновения с краями игрового поля
            if (head.X < 0 || head.X >= gameCanvas.ActualWidth || head.Y < 0 || head.Y >= gameCanvas.ActualHeight)
            {
                GameOver = true;
            }

            // Проверка столкновения змейки с самой собой
            if (snake.Skip(1).Any(part => part == head))
            {
                GameOver = true;
            }
        }

        private void PlaceFood()
        {
            int maxX = (int)(gameCanvas.ActualWidth / squareSize);
            int maxY = (int)(gameCanvas.ActualHeight / squareSize);

            food = new Point(random.Next(maxX) * squareSize, random.Next(maxY) * squareSize);
        }

        private void Draw()
        {
            gameCanvas.Children.Clear();

            foreach (Point part in snake)
            {
                DrawRectangle(part, Brushes.Green); // Рисуем змейку
            }

            DrawRectangle(food, Brushes.Red); // Рисуем еду
        }

        private void DrawRectangle(Point position, Brush color)
        {
            Rectangle rect = new Rectangle
            {
                Width = squareSize,
                Height = squareSize,
                Fill = color
            };

            Canvas.SetLeft(rect, position.X);
            Canvas.SetTop(rect, position.Y);
            gameCanvas.Children.Add(rect);
        }

        private void AutoPlay()
        {
            // Простейшая логика автономного режима для поиска еды
            Point head = snake.First();

            if (food.X > head.X) nextDirection = Direction.Right;
            if (food.X < head.X) nextDirection = Direction.Left;
            if (food.Y > head.Y) nextDirection = Direction.Down;
            if (food.Y < head.Y) nextDirection = Direction.Up;
        }
    }

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
}

