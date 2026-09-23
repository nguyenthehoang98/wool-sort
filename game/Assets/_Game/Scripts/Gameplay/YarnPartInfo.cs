// YarnPartInfo.cs (chứa thông tin của part, dùng cho thuật toán tính toán sinh box, vì đang sinh các layer bên trong theo demand thay vì sinh 1 lần tất cả từ đầu)

using System.Collections.Generic;
using _Game.Scripts;

namespace _Game.Scripts.Gameplay
{
public enum PartState
    {
        OnBoard,   // Chưa pick, đang nằm chồng
        InQueue,   // Đã pick lên waiting slot
        InBox      // Đã fill vào box
    }

    public class YarnPartInfo
    {
        public YarnColor Color { get; private set; }
        public int LayerIndex { get; private set; }  // vẫn giữ để build ban đầu, nhưng không dùng trực tiếp

        public PartState State { get; private set; }

        public YarnPartInfo Parent { get; set; }
        public List<YarnPartInfo> Children { get; set; } = new List<YarnPartInfo>(); // sử dụng list cho nhu cầu mở rộng về sau nếu có nhiều part con, hiện chỉ có 1 node con

        // Priority bây giờ tính động:
        public int Priority
        {
            get
            {
                switch (State)
                {
                    case PartState.InBox:
                        return -1;
                    case PartState.InQueue:
                        return 0;
                    case PartState.OnBoard:
                        // Tính số cha hiện vẫn OnBoard
                        int p = 1;
                        var cur = this.Parent;
                        while (cur != null && cur.State == PartState.OnBoard)
                        {
                            p++;
                            cur = cur.Parent;
                        }
                        return p;
                    default:
                        return LayerIndex;
                }
            }
        }

        public YarnPartInfo(YarnColor color, int layer, YarnPartInfo parent = null)
        {
            Color = color;
            LayerIndex = layer;
            State = PartState.OnBoard;
            Parent = parent;
            if (Parent != null)
                Parent.Children.Add(this);
        }

        public void MarkPickedToQueue()
        {
            if (State == PartState.OnBoard)
                State = PartState.InQueue;
        }

        public void MarkFilledIntoBox()
        {
            if (State != PartState.InBox)
                State = PartState.InBox;
        }
    }
}