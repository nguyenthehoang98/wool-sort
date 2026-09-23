using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEventBus;

namespace _Game.Scripts.Gameplay
{
    public class SlotManager : MonoBehaviour, IListener<YarnClickedMessage>
    {
        public static SlotManager Instance { get; private set; }

        private List<Slot> allSlots = new List<Slot>();

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Đăng ký các slot mới (cả Box và Waiting).
        /// </summary>
        public void RegisterSlots(IEnumerable<Slot> slots)
        {
            allSlots.AddRange(slots);
        }

        public void RegisterSlot(Slot slot)
        {
            allSlots.Add(slot);
        }

        /// <summary>
        /// Hủy đăng ký (khi Box biến mất).
        /// </summary>
        public void UnregisterSlots(IEnumerable<Slot> slots)
        {
            foreach (var slot in slots)
                allSlots.Remove(slot);
        }

        public void UnregisterSlot(Slot slot)
        {
            allSlots.Remove(slot);
        }

        public void UnregisterAllSlots()
        {
            allSlots.Clear();
        }

        /// <summary>
        /// Chỉ lấy slot trống của đúng preferredBox, không fallback.
        /// </summary>
        public Slot GetNextAvailableSlotInBox(YarnColor partColor, Box preferredBox)
        {
            return allSlots
                .FirstOrDefault(s => s.Area == Slot.AreaType.Box &&
                                     s.ParentBox == preferredBox &&
                                     !s.IsFilled &&
                                     s.CanAccept(partColor));
        }

        /// <summary>
        /// Lấy slot trống đầu tiên theo ưu tiên:
        /// 1. Slot của Box cùng màu, ưu tiên Box đã fill nhiều nhất.
        /// 2. Nếu không có, dùng slot của WaitingArea.
        /// </summary>
        public Slot GetNextAvailableSlot(YarnColor partColor, bool checkWaiting = true)
        {
            // // 0) Ưu tiên slot chưa fill của Rainbow-Box
            // var rainbow = allSlots
            //     .FirstOrDefault(s => s.Area==Slot.AreaType.Box
            //                          && s.ParentBox.BoxColor==YarnColor.Rainbow
            //                          && !s.IsFilled);
            // if (rainbow != null)
            // {
            //     Debug.Log("Chọn Rainbow-Slot");
            //     allSlots.Remove(rainbow);
            //     return rainbow;
            // }

            // --- 1) Tìm tất cả slot của các Box cùng màu và trống ---
            var boxSlots = new List<Slot>();
            foreach (var slot in allSlots)
            {
                if (slot.Area == Slot.AreaType.Box && slot.CanAccept(partColor))
                    boxSlots.Add(slot);
            }

            if (boxSlots.Count > 0)
            {
                // Nhóm theo Box, tính xem mỗi Box đã fill bao nhiêu slot
                var groups = boxSlots
                    .GroupBy(s => s.ParentBox)
                    .Select(g => new
                    {
                        Box = g.Key,
                        Slots = g.ToList(),
                        FilledCount = g.Key.Slots.Count(s => s.IsFilled)
                    });

                // Chọn nhóm Box có số slot đã fill lớn nhất
                var bestGroup = groups
                    .OrderByDescending(g => g.FilledCount)
                    .First();

                // Lấy slot đầu tiên trong nhóm đó  
                var chosenSlot = bestGroup.Slots[0];
                UnregisterSlot(chosenSlot); // Xoá khỏi danh sách quản lý
                Debug.Log($"Chọn Box-slot từ Box màu {bestGroup.Box.BoxColor}, đã fill {bestGroup.FilledCount} slot");

                return chosenSlot;
            }

            if (!checkWaiting) return null;

            // --- 2) Nếu không có slot Box, fallback sang WaitingArea ---
            // Lọc ra các slot Waiting có thể nhận partColor rồi sort theo vị trí X (từ trái qua) => luôn ưu tiên slot bên trái
            var waitingSlots = allSlots
                .Where(s => s.Area == Slot.AreaType.Waiting && s.CanAccept(partColor))
                .OrderBy(s => s.transform.position.x)    // hoặc .localPosition.x tuỳ hierarchy
                .ToList();

            if (waitingSlots.Count > 0)
            {
                var chosen = waitingSlots[0];
                UnregisterSlot(chosen); // Xoá khỏi danh sách quản lý
                Debug.Log($"Chọn Waiting-slot ở vị trí X={chosen.transform.position.x}");
                return chosen;
            }

            // --- 3) Không tìm được slot nào ---
            Debug.LogWarning($"Không tìm được slot phù hợp cho màu {partColor}");
            return null;
        }

        public List<Slot> GetAllSlots() => allSlots;

        private void OnEnable()
        {
            GlobalBus.Subscribe(this);
        }

        private void OnDisable()
        {
            GlobalBus.UnSubscribe(this);
        }

        public void React(in YarnClickedMessage e)
        {
            //var part = e.YarnPart;
            //var color = part.CurrentColor;
            //var slot = GetNextAvailableSlot(color);
            //if (slot != null)
            //{
            //    slot.Fill(part);
            //    YarnRopeController.Instance.SpawnRope(part, slot);
            //}
        }

        public Slot GetSlotToRope(YarnPart yarnPart)
        {
            var color = yarnPart.CurrentColor;
            var slot = GetNextAvailableSlot(color);

            return slot;
        }

        public bool SpawnRope(YarnPart yarnPart, Slot slot)
        {
            if (slot == null || !slot.Fill(yarnPart))
                return false;

            YarnRopeController.Instance.SpawnRope(yarnPart, slot);
            return true;
        }
    }
}
