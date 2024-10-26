using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_TetrisGame
{
    public class GameState
    {
        private Block currentBlock;
        public Block CurrentBlock
        {
            get => currentBlock;
            private set
            {
                currentBlock = value;
                currentBlock.Reset(); // Reset block về vị trí ban đầu khi được gán
            }
        }

        public GameGrid GameGrid { get; } // Lưới game chính
        public BlockQueue BlockQueue { get; } // Hàng đợi chứa các block tiếp theo
        public bool GameOver { get; private set; } // Trạng thái kết thúc game

        public GameState()
        {
            GameGrid = new GameGrid(22, 10); // Tạo lưới game 22 hàng x 10 cột
            BlockQueue = new BlockQueue(); // Khởi tạo hàng đợi block
            currentBlock = BlockQueue.GetAndUpdate(); // Lấy block đầu tiên từ hàng đợi
        }

        //Kiểm tra block có phù hợp vị trí
        private bool BlockFits()
        {
            foreach (Position p in CurrentBlock.TilePositions())
            {
                // Lặp qua từng ô của block hiện tại
                if (!GameGrid.IsEmpty(p.Row, p.Column))
                {
                    // Nếu có ô nào trùng với ô đã có trong lưới game
                    // thì block không vừa vị trí
                    return false;
                }
            }
            return true; // Tất cả ô đều phù hợp
        }

        // Xoay block
        public void RotateBlockCW()
        {
            CurrentBlock.RotateCW();  // Xoay block theo chiều kim đồng hồ
            if (!BlockFits())  // Kiểm tra sau khi xoay
            {
                CurrentBlock.RotateCCW();  // Nếu không vừa thì xoay ngược lại
            }
        }

        public void RotateBlockCCW()
        {
            CurrentBlock.RotateCCW();  // Xoay block ngược chiều kim đồng hồ
            if (!BlockFits())
            {
                CurrentBlock.RotateCW();  // Nếu không vừa thì xoay ngược lại
            }
        }

        //Di chuyển ngang
        public void MoveBlockLeft()
        {
            CurrentBlock.Move(0, -1);  // Di chuyển sang trái (cột giảm 1)
            if (!BlockFits())
            {
                CurrentBlock.Move(0, 1);  // Nếu không vừa thì di chuyển lại vị trí cũ
            }
        }

        public void MoveBlockRight()
        {
            CurrentBlock.Move(0, 1);   // Di chuyển sang phải (cột tăng 1)
            if (!BlockFits())
            {
                CurrentBlock.Move(0, -1);  // Nếu không vừa thì quay lại
            }
        }

        //Kiểm tra Game Over
        private bool IsGameOver()
        {
            // Game kết thúc khi 2 hàng đầu tiên (hàng 0 và 1) không còn trống
            // Vì block mới sẽ xuất hiện ở 2 hàng đầu
            return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));
        }

        //Đặt block vào lưới game
        private void PlaceBlock()
        {
            foreach (Position p in CurrentBlock.TilePositions())
            {
                // Đặt từng ô của block vào lưới game
                // Mỗi ô được đánh dấu bằng Id của block
                GameGrid[p.Row, p.Column] = CurrentBlock.Id;
            }

            GameGrid.ClearFullRows();  // Xóa các hàng đã đầy

            if (IsGameOver())
            {
                GameOver = true;  // Đánh dấu game kết thúc
            }
            else
            {
                // Lấy block mới từ hàng đợi nếu game chưa kết thúc
                CurrentBlock = BlockQueue.GetAndUpdate();
            }
        }

        // Di chuyển block xuống
        public void MoveBlockDown()
        {
            CurrentBlock.Move(1, 0);  // Di chuyển xuống (hàng tăng 1)
            if (!BlockFits())
            {
                CurrentBlock.Move(-1, 0);  // Nếu không vừa thì quay lại
                PlaceBlock();  // Đặt block vào lưới game vì đã chạm đáy
            }
        }

    }
}
