using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OOP_TetrisGame
{
    public abstract class Block
    {
        protected abstract Position[][] Tiles { get; } //lưu trữ vị trí (tọa độ) của các ô trong mỗi trạng thái xoay của khối
        protected abstract Position StartOffset { get; } //StartOffsee: vị trí bắt đầu của khối trên lưới trò chơi
        public abstract int Id { get; } // định danh của mỗi loại khối

        private int rotationState; //rotationState: lưu trạng thái xoay hiện tại của khối
        private Position offset; //offset: lưu vị trí hiện tại của khối trên lưới trò chơi

        // Constructor khởi tạo vị trí bắt đầu của khối dựa trên StartOffset
        public Block()
        {
            offset = new Position(StartOffset.Row, StartOffset.Column);
        }

        // Trả về vị trí của các ô vuông thuộc khối dựa trên trạng thái xoay và vị trí offset
        public IEnumerable<Position> TilePositions()
        {
            // Với mỗi ô (p) trong trạng thái xoay hiện tại (Tiles[rotationState]), 
            // cộng tọa độ offset để lấy vị trí thực tế của ô trên lưới
            foreach (Position p in Tiles[rotationState])
            {
                yield return new Position(p.Row + offset.Row, p.Column + offset.Column);
            }
        }

        // Xoay khối theo chiều kim đồng hồ
        public void RotateCW()
        {
            rotationState = (rotationState + 1) % Tiles.Length;
        }

        // Xoay khối ngược chiều kim đồng hồ
        public void RotateCCW()
        {
            if(rotationState == 0)
            {
                rotationState = Tiles.Length - 1;
            }
            else
            {
                rotationState--;
            }
        }

        // Di chuyển khối theo hướng hàng(rows) và cột(columns)
        public void Move(int rows,  int columns)
        {
            offset.Row += rows;
            offset.Column += columns;
        }

        // Đặt lại khối về vị trí và trạng thái ban đầu
        public void Reset()
        {
            rotationState = 0;
            offset.Row = StartOffset.Row;
            offset.Column = StartOffset.Column;
        }
    }
}
