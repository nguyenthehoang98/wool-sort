using System;

namespace _Game.Scripts.Utils
{
    /// <summary>Format số tiền hiển thị cho người chơi: >=1000000 -> "1.2M", >=1000 -> "1.2K", ngược lại nguyên.</summary>
    public static class NumberFormatter
    {
        public static string FormatCoin(int amount)
        {
            if (amount >= 1_000_000)
                return $"{(amount / 1_000_000f):F1}M";
            if (amount >= 1_000)
                return $"{(amount / 1_000f):F1}K";
            return amount.ToString();
        }
    }
}
