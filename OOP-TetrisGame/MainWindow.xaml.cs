using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace OOP_TetrisGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Mảng lưu trữ hình ảnh cho các ô trong lưới game
        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative)),
        };

        // Mảng lưu trữ hình ảnh cho các block
        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            null,
            new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative)),
        };

        // Mảng 2 chiều lưu trữ các control Image để hiển thị trên giao diện
        private readonly Image[,] imageControls;
        // Độ trễ tối đa giữa các lần di chuyển của block (tính bằng milliseconds)
        private readonly int maxDelay = 1000;
        // Độ trễ tối thiểu giữa các lần di chuyển của block
        private readonly int minDelay = 75;
        // Mức giảm của độ trễ khi người chơi đạt đến cấp độ mới
        private readonly int delayDecrease = 25;

        // Đối tượng quản lý trạng thái game
        private GameState gameState = new GameState();
        private bool gameRunning = false;


        // Add a MediaPlayer for background music
        private MediaPlayer backgroundMusic;
        private MediaPlayer gameStartMusic;
        private MediaPlayer gameOverMusic;
        private MediaPlayer moveSound;
        private MediaPlayer dropSound;
        private MediaPlayer countdownSound;
        private const int COUNTDOWN_INTERVAL = 800; // 0.8 giây cho mỗi số
        private const int START_DISPLAY_TIME = 1600; // 1.6 giây cho chữ "START!"


        public MainWindow()
        {
            InitializeComponent(); // Khởi tạo các thành phần giao diện
            imageControls = SetupGameCanvas(gameState.GameGrid); // Thiết lập canvas game

            // Setup background music
            backgroundMusic = new MediaPlayer();
            backgroundMusic.Open(new Uri("Assets/Sounds/Mainsound.mp3", UriKind.Relative));


            backgroundMusic.Volume = 0.3; // Set volume between 0 and 1
            backgroundMusic.MediaEnded += BackgroundMusic_MediaEnded; // Loop the music
            backgroundMusic.Play(); // Start playing immediately

            // Setup GameStart music
            gameStartMusic = new MediaPlayer();
            gameStartMusic.Open(new Uri("Assets/Sounds/GameStart.mp3", UriKind.Relative));
            gameStartMusic.Volume = 0.6; // Set volume between 0 and 1

            gameOverMusic = new MediaPlayer();
            gameOverMusic.Open(new Uri("Assets/Sounds/GameOver.mp3", UriKind.Relative));
            gameOverMusic.Volume = 0.7;

            moveSound = new MediaPlayer();
            moveSound.Open(new Uri("Assets/Sounds/Move.mp3", UriKind.Relative));
            moveSound.Volume = 1;

            dropSound = new MediaPlayer();
            dropSound.Open(new Uri("Assets/Sounds/Drop.mp3", UriKind.Relative));
            dropSound.Volume = 1;

            countdownSound = new MediaPlayer();
            countdownSound.Open(new Uri("Assets/Sounds/CountDown.mp3", UriKind.Relative));
            countdownSound.Volume = 0.7;



            // Đảm bảo menu hiển thị và game interface ẩn khi khởi động
            MainMenu.Visibility = Visibility.Visible;
            GameInterface.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;
        }

        // Event handler to loop the music
        private void BackgroundMusic_MediaEnded(object sender, EventArgs e)
        {
            backgroundMusic.Position = TimeSpan.Zero;
            backgroundMusic.Play();
        }

        // Phương thức thiết lập giao diện game
        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            // Mảng 2 chiều chứa các control Image theo kích thước lưới game
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];

            int cellSize = 25; // Kích thước mỗi ô là 25x25 pixels

            // Duyệt qua từng ô trong lưới game
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    // Tạo mới một control Image cho mỗi ô
                    Image imageControl = new Image
                    {
                        Width = cellSize,   // Đặt chiều rộng
                        Height = cellSize   // Đặt chiều cao
                    };

                    // Đặt vị trí cho Image trên Canvas
                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);  // Vị trí top (trừ 2 vì 2 hàng đầu ẩn)
                    Canvas.SetLeft(imageControl, c * cellSize);        // Vị trí left

                    GameCanvas.Children.Add(imageControl);            // Thêm Image vào Canvas
                    imageControls[r, c] = imageControl;                // Lưu Image vào mảng
                }
            }
            return imageControls;
        }

        // Vẽ lưới game
        private void DrawGrid(GameGrid grid)
        {
            // Duyệt qua từng ô trong lưới game
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    // Lấy id của ô tại vị trí [r,c]
                    int id = grid[r, c];

                    // Các ô trống làm rất mờ, các ô có block giữ nguyên độ hiển thị
                    imageControls[r, c].Opacity = (id == 0) ? 0.05 : 1;

                    // Gán hình ảnh tương ứng với id cho Image control
                    imageControls[r, c].Source = tileImages[id];
                }
            }
        }

        // Vẽ block đang rơi
        private void DrawBlock(Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1;
                // Gán hình ảnh tương ứng với id của block cho các ô của block
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        // Vẽ block kế tiếp
        private void DrawNextBlock(BlockQueue blockQueue)
        {
            // Lấy block kế tiếp từ hàng đợi block
            Block next = blockQueue.NextBlock;
            // Đặt hình ảnh của block kế tiếp vào NextImage để hiển thị
            NextImage.Source = blockImages[next.Id];
        }

        // Vẽ block đang được giữ
        private void DrawHeldBlock(Block heldBlock)
        {
            if (heldBlock == null)
            {
                // Nếu chưa có block được giữ, đặt nguồn hình ảnh thành null
                HoldImage.Source = null;
            }
            else
            {
                // Nếu có block được giữ, hiển thị hình ảnh của block đó
                HoldImage.Source = blockImages[heldBlock.Id];
            }
        }

        // Vẽ "ghost block" (block ảo) tại vị trí mà block hiện tại sẽ chạm đáy khi thả xuống
        private void DrawGhostBlock(Block block)
        {
            // Tính khoảng cách rơi tối đa cho block hiện tại (khoảng cách "ghost")
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                // Cài đặt độ trong suốt cho ô ghost block để tạo hiệu ứng mờ
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                // Đặt hình ảnh của ô ghost block dựa vào Id của block
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        // Vẽ toàn bộ trạng thái game
        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid); // Vẽ lưới game
            DrawGhostBlock(gameState.CurrentBlock); // Vẽ "ghost block" để gợi ý vị trí của block khi rơi xuống hoàn toàn
            DrawBlock(gameState.CurrentBlock); // Vẽ block đang rơi
            DrawNextBlock(gameState.BlockQueue); // Vẽ block kế tiếp từ hàng đợi block trong trạng thái game
            DrawHeldBlock(gameState.HeldBlock); // Vẽ block đang được giữ (nếu có)
            ScoreText.Text = $"{gameState.Score}"; // Cập nhật hiển thị điểm số của người chơi trên giao diện
            LinesText.Text = $"{gameState.LinesCleared}";
        }

        // Chứa vòng lặp chính của game
        private async Task GameLoop()
        {
            Draw(gameState);
            while (!gameState.GameOver && gameRunning)
            {
                // Sử dụng phương thức GetDropInterval() từ GameState
                int delay = (int)(gameState.GetDropInterval() * 1000);
                await Task.Delay(delay);

                gameState.MoveBlockDown();
                Draw(gameState);

                // Cập nhật hiển thị level
                LevelText.Text = $"{gameState.Level}";
            }



            if (gameState.GameOver)
            {
                // Kiểm tra trạng thái âm thanh khi game over
                if (isMusicEnabled)
                {
                    backgroundMusic.Stop();
                    gameOverMusic.Position = TimeSpan.Zero;
                    gameOverMusic.Play();
                }

                GameOverMenu.Visibility = Visibility.Visible;
                FinalScoreText.Text = $"Score: {gameState.Score}";
                gameRunning = false;
            }
        }

        // Thêm biến để theo dõi trạng thái âm thanh
        private bool isMusicEnabled = true;

        // Phương thức xử lý sự kiện khi nhấn nút toggle âm thanh
        private void MusicToggleButton_Click(object sender, RoutedEventArgs e)
        {
            // Đảo ngược trạng thái âm thanh
            isMusicEnabled = !isMusicEnabled;

            if (isMusicEnabled)
            {
                // Bật lại tất cả các âm thanh
                backgroundMusic.Play();
                MusicToggleImage.Source = new BitmapImage(new Uri("Assets/MusicOn.png", UriKind.Relative));
            }
            else
            {
                // Tắt tất cả các âm thanh
                backgroundMusic.Pause();
                gameStartMusic.Pause();
                gameOverMusic.Pause();
                moveSound.Pause();
                dropSound.Pause();
                countdownSound.Pause();

                MusicToggleImage.Source = new BitmapImage(new Uri("Assets/MusicOff.png", UriKind.Relative));
            }
        }

        // Phương thức phát âm thanh có kiểm tra trạng thái
        private void PlaySound(MediaPlayer player)
        {
            if (isMusicEnabled)
            {
                player.Position = TimeSpan.Zero;
                player.Play();
            }
        }



        // Xử lý sự kiện khi người chơi nhấn phím
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameRunning || gameState.GameOver)
                return;

            switch (e.Key)
            {
                case Key.Left:
                    gameState.MoveBlockLeft();
                    PlaySound(moveSound);
                    break;
                case Key.Right:
                    gameState.MoveBlockRight();
                    PlaySound(moveSound);
                    break;
                case Key.Down:
                    gameState.MoveBlockDown();
                    PlaySound(moveSound);
                    break;
                case Key.Up:
                    gameState.RotateBlockCW();
                    break;
                case Key.Z:
                    gameState.RotateBlockCCW();
                    break;
                case Key.C:
                    gameState.HoldBlock();
                    break;
                case Key.Space:
                    gameState.DropBlock();
                    PlaySound(dropSound);
                    break;
                default:
                    return;
            }

            Draw(gameState);
        }

        private async void StartGame_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.Visibility = Visibility.Hidden;
            GameInterface.Visibility = Visibility.Visible;
            CountdownOverlay.Visibility = Visibility.Visible;

            // Bắt đầu phát nhạc countdown (sử dụng PlaySound)
            PlaySound(countdownSound);

            // Countdown sequence
            for (int i = 3; i >= 1; i--)
            {
                CountdownText.Text = i.ToString();
                await Task.Delay(COUNTDOWN_INTERVAL);
            }

            // Show "START!"
            CountdownText.Text = "START!";
            await Task.Delay(START_DISPLAY_TIME);

            // Hide countdown and start game
            CountdownOverlay.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;

            gameState = new GameState();
            gameState.LinesCleared = 0; // Reset số dòng khi bắt đầu game mới
            gameRunning = true;

            // Kiểm tra trạng thái âm thanh khi bắt đầu game
            if (isMusicEnabled)
            {
                // Chỉ phát lại nếu nhạc đã dừng hoàn toàn
                if (backgroundMusic.Position == backgroundMusic.NaturalDuration)
                {
                    backgroundMusic.Position = TimeSpan.Zero;
                }
                // Không cần stop, chỉ đảm bảo nhạc đang chạy
                backgroundMusic.Play();
            }

            GameLoop();
        }

        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            CountdownOverlay.Visibility = Visibility.Visible;
            GameOverMenu.Visibility = Visibility.Hidden;

            // Bắt đầu phát nhạc countdown (sử dụng PlaySound)
            PlaySound(countdownSound);

            // Countdown sequence
            for (int i = 3; i >= 1; i--)
            {
                CountdownText.Text = i.ToString();
                await Task.Delay(COUNTDOWN_INTERVAL);
            }

            // Show "START!"
            CountdownText.Text = "START!";
            await Task.Delay(START_DISPLAY_TIME);

            // Hide countdown and start game
            CountdownOverlay.Visibility = Visibility.Hidden;
            GameOverMenu.Visibility = Visibility.Hidden;

            gameState = new GameState();
            gameState.LinesCleared = 0;
            gameRunning = true;

            // Kiểm tra trạng thái âm thanh khi chơi lại
            if (isMusicEnabled)
            {
                backgroundMusic.Stop();
                backgroundMusic.Play();
            }

            GameLoop();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            backgroundMusic.Stop();
            Application.Current.Shutdown();
        }

        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            // No need to start game loop here anymore since it starts from StartGame_Click
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // Shuts down the application
        }

        private void GameStartButton_MouseEnter(object sender, MouseEventArgs e)
        {
            // Kiểm tra trạng thái âm thanh
            PlaySound(gameStartMusic);
        }

    }
}