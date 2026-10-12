using System.Collections.Generic;
using UnityEngine;

public static class Configs
{
    //Heart System
    public const int MAX_HEART = 5;
    public const int TIME_HEART_COUNTDOWN = 1800;

    //Revive Popup
    public const int REVIVE_COST = 190;
    public const float TIME_RESET_CLICKED = 1f;

    //More Heart Popup
    public const int REFILL_HEART_COST = 190;

    //More Booster Popup
    public static List<int> LIST_BOOSTER_COST = new List<int>() { 80, 120, 60 };
    public static List<int> LIST_BOOSTER_UNLOCK = new List<int>() { 2, 8, 4 };

    //Win Popup
    public const int WIN_REWARD = 10;
    public static List<int> LIST_WIN_REWARD_MULTI = new List<int>() { 1, 2, 2 };

    //More Basket Popup
    public const int MORE_BASKET_COS = 190;

    //Shop
    public const int FREE_COIN_ADS = 50;
    public const int BUNDLE_PACK_ENABLE_AT_LEVEL = 10;

    //Daily Reward
    public const int LEVEL_START_SHOW_DAILY = 5;
    public const int LEVEL_START_UNLOCK_DAILY = 10;

    //Image Config
    public const int IMAGE_SIZE = 28;
    public const int MAXIMUM_COLOR = 21;

    //Setting Home Popup
    public const int PANEL_HOME_HEIGHT_NO_RESTORE = 860;
    public const int PANEL_HOME_HEIGHT_RESTORE = 1008;

    //Setting InGame Popup
    public const int PANEL_INGAME_HEIGHT_NO_RESTORE = 1018;
    public const int PANEL_INGAME_HEIGHT_RESTORE = 1186;

    public static Color GetUnitColor(int index)
    {
        switch (index)
        {
            case 0: return new Color(55 / 255f, 133 / 255f, 234 / 255f);
            case 1: return new Color(195 / 255f, 105 / 255f, 81 / 255f);
            case 2: return new Color(62 / 255f, 66 / 255f, 72 / 255f);
            case 3: return new Color(248 / 255f, 70 / 255f, 73 / 255f);
            case 4: return new Color(255 / 255f, 234 / 255f, 189 / 255f);
            case 5: return new Color(178 / 255f, 37 / 255f, 40 / 255f);
            case 6: return new Color(64 / 255f, 69 / 255f, 177 / 255f);
            case 7: return new Color(51 / 255f, 151 / 255f, 104 / 255f);
            case 8: return new Color(174 / 255f, 174 / 255f, 191 / 255f);
            case 9: return new Color(108 / 255f, 200 / 255f, 56 / 255f);
            case 10: return new Color(158 / 255f, 202 / 255f, 39 / 255f);
            case 11: return new Color(235 / 255f, 92 / 255f, 243 / 255f);
            case 12: return new Color(243 / 255f, 138 / 255f, 35 / 255f);
            case 13: return new Color(247 / 255f, 84 / 255f, 164 / 255f);
            case 14: return new Color(205 / 255f, 27 / 255f, 146 / 255f);
            case 15: return new Color(168 / 255f, 75 / 255f, 215 / 255f);
            case 16: return new Color(65 / 255f, 211 / 255f, 240 / 255f);
            case 17: return new Color(242 / 255f, 243 / 255f, 251 / 255f);
            case 18: return new Color(113 / 255f, 120 / 255f, 132 / 255f);
            case 19: return new Color(61 / 255f, 164 / 255f, 155 / 255f);
            case 20: return new Color(245 / 255f, 208 / 255f, 37 / 255f);
        }
        return Color.black;
    }

    public static string GetUnitColorName(Color color)
    {
        int index = GetUnitColorId(color);
        if (index != -1) return GetUnitColorName(index);
        return string.Empty;
    }

    public static int GetUnitColorId(Color color)
    {
        int index = -1;
        for (int i = 0; i < MAXIMUM_COLOR; i++)
        {
            if (color == GetUnitColor(i))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public static string GetUnitColorName(int index)
    {
        switch (index)
        {
            case 0: return "Blue";
            case 1: return "Brown";
            case 2: return "Charcoal";
            case 3: return "Coral";
            case 4: return "Cream";
            case 5: return "Crimson";
            case 6: return "Deep";
            case 7: return "Forest";
            case 8: return "Gray";
            case 9: return "Green";
            case 10: return "Lime";
            case 11: return "Magenta";
            case 12: return "Orange";
            case 13: return "Pink";
            case 14: return "Plum";
            case 15: return "Purple";
            case 16: return "Sky";
            case 17: return "Snow";
            case 18: return "Steel";
            case 19: return "Teal";
            case 20: return "Yellow";
        }
        return "Black N/A";
    }
}

public enum LevelModeType
{
    Normal,
    Hard,
    VeryHard,
}

public enum MenuBarOption
{
    Shop,
    Home,
    Ranking,
}

public enum ReviveType
{
    AddBasket,
    Clean,
}

public enum UnlockBoxType
{
    Normal,
    Revive,
}

public enum ButtonBoosterState
{
    Lock,
    Noti,
    Plus,
}
