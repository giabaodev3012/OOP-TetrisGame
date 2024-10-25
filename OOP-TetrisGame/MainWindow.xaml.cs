﻿using System.Text;
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
                    Canvas.SetTop(imageControl, (r - 2) * cellSize);  // Vị trí top (trừ 2 vì 2 hàng đầu ẩn)
                    Canvas.SetLeft(imageControl, c * cellSize);        // Vị trí left

                    GameCanvas.Children.Add(imageControl);            // Thêm Image vào Canvas
                    imageControls[r, c] = imageControl;                // Lưu Image vào mảng
                }
            }
            return imageControls;
        }
    }
}