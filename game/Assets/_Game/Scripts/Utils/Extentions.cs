using _Game.Scripts.Gameplay;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public static class Extentions
{
    public static void CheckKillTween(this Tween tween, bool OnComplate = false)
    {
        if (tween != null)
        {
            tween.Kill(OnComplate);
        }
    }

    public static bool CheckSetString(this TextMeshProUGUI text, string content)
    {
        if (text == null || string.IsNullOrEmpty(content))
        {
            return false;
        }
        text.text = content;
        return true;
    }

    public static string GetHoursBySeconds(this int secs)
    {
        int hours = secs / 3600;
        int mins = (secs % 3600) / 60;
        secs = secs % 60;
        return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, mins, secs);
    }


    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static string ExBoosterTypeToDataBuckets(this BoosterType type)
    {
        switch (type)
        {
            case BoosterType.AddSlot:
                return "add_hole";
            case BoosterType.Clean:
                return "clean";
            case BoosterType.MagicBox:
                return "magic_box";
            default:
                return type.ToString();
        }
    }

    public static string ExAdFormat2Databucket(this string adFormat)
    {
        if (adFormat.ToLower() == "rewarded")
        {
            return "video_reward";
        }
        else if (adFormat.ToLower() == "inter")
        {
            return "interstitial";
        }
        return adFormat;
    }
}
