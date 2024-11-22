using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OOP_TetrisGame
{
    public class GameGrid
    {
        private readonly int[,] grid;
        private MediaPlayer clearLineSound;

        //Properties: chỉ ra số hàng số cột của lưới
        public int Rows { get; } //truy xuất giá trị, chỉ đọc
        public int Columns { get; }

        //indexer
        public int this[int r, int c]
        {
            get => grid[r, c]; //Trả về giá trị tại hàng r, cột c 
            set => grid[r, c] = value; //Gán giá trị value
        }

        //Constructor
        public GameGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            grid = new int[rows, columns];

            // Tải âm thanh khi xóa hàng
            clearLineSound = new MediaPlayer();
            clearLineSound.Open(new Uri("Assets/Sounds/ClearLine.mp3", UriKind.Relative));
            clearLineSound.Volume = 1;
        }

        public void PlayClearLineSound()
        {
            clearLineSound.Stop();  // Dừng âm thanh nếu đang phát
            clearLineSound.Play();  // Phát lại âm thanh
        }

        //Kiểm tra tọa độ có nằm trong lưới không
        public bool IsInside(int r, int c)
        {
            return (r>=0 && r < Rows) && (c>=0 && c<Columns);
        }

        //Kiểm tra ô có trống không
        public bool IsEmpty(int r, int c)
        {
            return IsInside(r, c) && grid[r, c] == 0;
        }

        //Kiểm tra hàng có đầy đủ khối không
        public bool IsRowFull(int r)
        {
            for (int c=0; c < Columns; c++)
            {
                if (grid[r, c] == 0)
                {
                    return false;
                }
            }

            return true; 
        }

        //Kiểm tra hàng có trống hoàn toàn không
        public bool IsRowEmpty(int r)
        {
            for (int c=0;c < Columns; c++)
            {
                if (grid[r, c] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        //Xóa một hàng trong lưới
        private void ClearRow(int r)
        {
            for (int c=0; c < Columns; c++)
            {
                grid[r, c] = 0; // Đặt tất cả các ô trong hàng r về giá trị 0
            }
        }

        //Di chuyển một hàng xuống phía dưới trong lưới
        private void MoveRowDown(int r, int numRows)
        {
            for(int c=0; c < Columns;c++)
            {
                grid[r + numRows, c] = grid[r, c]; // Di chuyển ô từ hàng r xuống hàng (r + numRows)
                grid[r, c] = 0; // Đặt ô gốc về 0
            }
        }

        public int ClearFullRows()
        {
            int cleared = 0;

            for(int r = Rows -1; r>=0; r--)
            {
                if (IsRowFull(r))
                {
                    ClearRow(r);
                    cleared++;

                    // Phát âm thanh khi xóa một dòng
                    PlayClearLineSound();
                }
                else if (cleared > 0)
                {
                    MoveRowDown(r, cleared);
                }
            }

            return cleared;
        }
    }
}
