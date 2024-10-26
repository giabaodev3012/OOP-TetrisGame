using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_TetrisGame
{
    public class BlockQueue
    {
        // Mảng các khối có thể xuất hiện, bao gồm các loại khối
        private readonly Block[] blocks = new Block[]
        {
            new IBlock(),
            new JBlock(),
            new LBlock(),
            new OBlock(),
            new SBlock(),
            new TBlock(),
            new ZBlock(),
        };

        // Khởi tạo đối tượng Random để lấy ngẫu nhiên khối từ mảng blocks
        private readonly Random random = new Random();

        // Thuộc tính NextBlock dùng để lưu khối Tetris tiếp theo
        public Block NextBlock { get; private set; }

        // Constructor: khởi tạo NextBlock với một khối ngẫu nhiên
        public BlockQueue()
        {
            NextBlock = RandomBlock();
        }

        // Trả về một khối ngẫu nhiên từ mảng blocks
        private Block RandomBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }

        // Trả về khối hiện tại và cập nhật NextBlock với khối mới không trùng với khối vừa trả về
        public Block GetAndUpdate()
        {
            Block block = NextBlock;

            do
            {
                NextBlock = RandomBlock();
            }
            while (block.Id == NextBlock.Id);

            return block;
        }
    }
}
