using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
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
        public MainWindow()
        {
            InitializeComponent(); // Khởi tạo các thành phần giao diện
            imageControls = SetupGameCanvas(gameState.GameGrid); // Thiết lập canvas game
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
            for (int r = 0;r < grid.Rows;r++)
            {
                for(int c=0;c < grid.Columns; c++)
                {
                    // Lấy id của ô tại vị trí [r,c]
                    int id = grid[r, c];
                    // Đặt độ trong suốt của ô về mức 1 (không trong suốt)
                    imageControls[r, c].Opacity = 1;
                    // Gán hình ảnh tương ứng với id cho Image control
                    // Sử dụng mảng tileImages để tìm hình ảnh theo ID của ô
                    imageControls[r,c].Source = tileImages[id];
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
                // Nếu chưa có block được giữ, hiển thị hình ảnh trống
                HoldImage.Source = blockImages[0];
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
            DrawGhostBlock(gameState.CurrentBlock); //// Vẽ "ghost block" để gợi ý vị trí của block khi rơi xuống hoàn toàn
            DrawBlock(gameState.CurrentBlock); // Vẽ block đang rơi
            DrawNextBlock(gameState.BlockQueue); //// Vẽ block kế tiếp từ hàng đợi block trong trạng thái game
            DrawHeldBlock(gameState.HeldBlock); // Vẽ block đang được giữ (nếu có)
            ScoreText.Text = $"Score: {gameState.Score}"; // Cập nhật hiển thị điểm số của người chơi trên giao diện
        }

        // Chứa vòng lặp chính của game
        private async Task GameLoop()
        {
            // Vẽ trạng thái ban đầu của game
            Draw(gameState);

            // Vòng lặp chính - chạy liên tục cho đến khi game kết thúc
            while (!gameState.GameOver)
            {
                int delay = Math.Max(minDelay, maxDelay - (gameState.Score * delayDecrease));
                // Tạm dừng trước mỗi lần di chuyển block xuống
                await Task.Delay(delay);

                // Tự động di chuyển block xuống một đơn vị
                gameState.MoveBlockDown();

                // Vẽ lại trạng thái game sau khi block di chuyển
                Draw(gameState);
            }

            // Hiển thị menu kết thúc game
            GameOverMenu.Visibility = Visibility.Visible;
            // Hiển thị điểm số cuối cùng của người chơi trên menu kết thúc
            FinalScoreText.Text = $"Score: {gameState.Score}";
        }

        // Xử lý sự kiện khi người chơi nhấn phím
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Kiểm tra nếu game đã kết thúc thì không xử lý phím nữa
            if (gameState.GameOver)
            {
                return;
            }

            // Sử dụng switch để xử lý các phím khác nhau
            switch (e.Key)
            {
                case Key.Left:
                    gameState.MoveBlockLeft();
                    break;
                case Key.Right:
                    gameState.MoveBlockRight();
                    break;
                case Key.Down:
                    gameState.MoveBlockDown();
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
                    break;
                default:
                    return;
            }

            Draw(gameState);
        }

        // Xử lý sự kiện khi GameCanvas được load
        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            // Bắt đầu vòng lặp game khi giao diện được load
            await GameLoop();
        }

        // Xử lý sự kiện khi người chơi nhấn nút chơi lại
        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            // Khởi tạo lại trạng thái của trò chơi khi bắt đầu ván mới
            gameState = new GameState();
            // Ẩn menu Game Over khỏi giao diện để chuẩn bị cho trò chơi mới
            GameOverMenu.Visibility = Visibility.Hidden;
            // Bắt đầu vòng lặp trò chơi mới
            await GameLoop();
        }
    }
}