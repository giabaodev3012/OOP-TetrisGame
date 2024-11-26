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
            // Trả về giá trị của block hiện tại
            get => currentBlock;

            // Thiết lập giá trị cho block hiện tại
            private set
            {
                currentBlock = value;
                currentBlock.Reset(); // Reset block về vị trí ban đầu khi được gán

                // Thực hiện điều chỉnh block trong lưới game
                for (int i = 0; i < 2; i++)
                {
                    // Di chuyển block xuống 1 hàng
                    currentBlock.Move(1, 0);

                    // Kiểm tra nếu block không khớp với lưới
                    if (!BlockFits())
                    {
                        // Nếu không khớp, di chuyển block lên lại 1 hàng để đảm bảo vị trí hợp lệ
                        currentBlock.Move(-1, 0);
                    }
                }
            }
        }

        public GameGrid GameGrid { get; } // Lưới game chính
        public BlockQueue BlockQueue { get; } // Hàng đợi chứa các block tiếp theo
        public bool GameOver { get; private set; } // Trạng thái kết thúc game
        public int Score { get; private set; } //Đại diện cho điểm số của người chơi, được cập nhật mỗi khi người chơi xóa được hàng trong lưới
        public Block HeldBlock { get; private set; } // Block mà người chơi đang giữ
        public bool CanHold { get; private set; } // Kiểm tra xem người chơi có thể giữ block hay không

        public int LinesCleared { get; set; } = 0;
        public int Level { get; private set; } = 1;

        public GameState()
        {
            GameGrid = new GameGrid(22, 10); // Tạo lưới game 22 hàng x 10 cột
            BlockQueue = new BlockQueue(); // Khởi tạo hàng đợi block
            currentBlock = BlockQueue.GetAndUpdate(); // Lấy block đầu tiên từ hàng đợi
            CanHold = true;
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

        // Giữ block hiện tại
        public void HoldBlock()
        {
            // Kiểm tra xem người chơi có thể giữ block hay không
            if (!CanHold)
            {
                return; // Nếu không thể giữ, kết thúc phương thức
            }

            // Nếu chưa có block nào được giữ
            if (HeldBlock == null)     
            {
                // Giữ block hiện tại
                HeldBlock = CurrentBlock;

                // Lấy block mới từ hàng đợi
                CurrentBlock = BlockQueue.GetAndUpdate();
            }
            else
            {
                // Nếu đã có block được giữ, hoán đổi block hiện tại với block đang giữ
                Block tmp = CurrentBlock; // Lưu trữ block hiện tại vào biến tạm
                CurrentBlock = HeldBlock; // Đặt block đang giữ làm block hiện tại
                HeldBlock = tmp; // Đặt block hiện tại vào block đang giữ
            }

            CanHold = false; // Đánh dấu là không thể giữ block cho đến khi block mới được đặt
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

        // Phương thức cập nhật level
        private void UpdateLevel()
        {
            // Tăng level dựa trên số dòng đã xóa
            Level = 1 + (LinesCleared / 10);
        }

        //Đặt block vào lưới game
        private void PlaceBlock()
        {
            foreach (Position p in CurrentBlock.TilePositions())
            {
                // Đặt từng ô của block vào lưới game
                GameGrid[p.Row, p.Column] = CurrentBlock.Id;
            }

            // Cập nhật số dòng đã xóa
            int clearedLines = GameGrid.ClearFullRows();
            LinesCleared += clearedLines;

            // Cập nhật level
            UpdateLevel();

            // Tính điểm kết hợp: block và dòng
            int blockPoints = CalculateBlockPoints(CurrentBlock);
            int linePoints = CalculateLinePoints(clearedLines);

            // Tổng điểm từ block và dòng
            Score += blockPoints + linePoints;

            if (IsGameOver())
            {
                GameOver = true;  // Đánh dấu game kết thúc
            }
            else
            {
                // Lấy block mới từ hàng đợi nếu game chưa kết thúc
                CurrentBlock = BlockQueue.GetAndUpdate();
                CanHold = true;
            }
        }

        public double GetDropInterval()
        {
            // Công thức tính tốc độ rơi, level càng cao thì thời gian delay càng ngắn
            return Math.Max(0.1, 1.0 - (Level - 1) * 0.1);
        }

        // Tính điểm theo loại block
        private int CalculateBlockPoints(Block block)
        {
            // Điểm theo từng loại block
            return block switch
            {
                IBlock => 10,     // I block (dài) được nhiều điểm nhất
                JBlock => 8,
                LBlock => 8,
                OBlock => 6,      // O block (vuông) ít điểm hơn
                SBlock => 7,
                TBlock => 9,
                ZBlock => 7,
                _ => 5            // Trường hợp không xác định
            };
        }

        // Tính điểm theo số dòng xóa
        private int CalculateLinePoints(int clearedLines)
        {
            return clearedLines switch
            {
                1 => 40,   // Xóa 1 dòng
                2 => 100,  // Xóa 2 dòng
                3 => 300,  // Xóa 3 dòng
                4 => 1200, // Xóa 4 dòng (Tetris)
                _ => 0     // Không xóa dòng
            };
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

        // Tính khoảng cách tối đa mà một ô (tile) của block có thể rơi xuống
        private int TileDropDistance(Position p)
        {
            int drop = 0;

            // Kiểm tra từng hàng phía dưới ô (tile) cho đến khi gặp ô không trống
            while (GameGrid.IsEmpty(p.Row + drop +1, p.Column))
            {
                drop ++;
            }

            return drop;
        }

        // Tính khoảng cách rơi tối đa mà toàn bộ block có thể rơi xuống
        public int BlockDropDistance()
        {
            // Khởi tạo khoảng cách rơi ban đầu bằng số hàng của lưới game
            int drop = GameGrid.Rows;

            foreach (Position p in CurrentBlock.TilePositions())
            {
                // Cập nhật khoảng cách rơi tối đa cho toàn bộ block
                drop = System.Math.Min(drop, TileDropDistance(p));
            }

            return drop;
        }

        // Thả block xuống khoảng cách rơi tối đa và đặt block vào lưới game
        public void DropBlock()
        {
            // Di chuyển block xuống theo khoảng cách rơi tối đa
            CurrentBlock.Move(BlockDropDistance(), 0);
            // Đặt block vào lưới game sau khi thả xuống
            PlaceBlock() ;
        }
    }
}
