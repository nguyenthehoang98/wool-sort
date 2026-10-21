# Inventory ảnh — target project (`game/Assets`)

Sinh tự động để **reskin**. Mỗi ảnh có: kích thước, PPU, kiểu texture, và **dùng ở đâu**.

Quy tắc phân loại:

- **Đang dùng** = reachable từ scene / prefab gốc / material / code (đi theo đồ thị reference).
- Ảnh trong thư mục `Resources/` được load runtime theo path → coi là đang dùng dù không có reference trực tiếp.
- **Không dùng** = không reachable từ bất kỳ gốc nào (kể cả asset chỉ trỏ tới nó cũng không dùng).

| Nhóm | Tổng ảnh game | Đang dùng | Không dùng |
|---|---|---|---|
| Ảnh game (`_Game/`, TMP) | 94 | 87 | 7 |
| Ảnh plugin/editor (không reskin) | 191 | 4 | 187 |

## 1. Ảnh game ĐANG DÙNG (87)

Cột **9-slice** = `spriteBorder` (x,y,z,w) trong `.meta`. Không 0 → ảnh bị cắt viền, đổi PNG phải giữ nguyên.

| # | Path | Nhóm | Kích thước | PPU | Kiểu | 9-slice | Mipmap | Dùng ở |
|---|---|---|---|---|---|---|---|---|
| 1 | `TextMesh Pro/Sprites/EmojiOne.png` | TMP | 512x512 | 100 | Default | - | có | `TextMesh Pro/Resources/Sprite Assets/EmojiOne.asset` |
| 2 | `_Game/BoosterUI_Picked/Button_Blue.png` | Booster | 356x168 | 100 | Sprite | 50,50,50,50 | không | `Scenes/Gameplay.unity` |
| 3 | `_Game/BoosterUI_Picked/Button_Boosters.png` | Booster | 194x100 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 4 | `_Game/BoosterUI_Picked/Button_Boosters_Gray.png` | Booster | 194x100 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 5 | `_Game/BoosterUI_Picked/Button_Close.png` | Booster | 54x53 | 100 | Sprite | - | không | `Scenes/Gameplay.unity`<br>`_Game/Prefab/UI/PurchaseSuccessPopup.prefab` |
| 6 | `_Game/BoosterUI_Picked/Button_Green.png` | Booster | 356x168 | 100 | Sprite | 50,50,50,50 | không | `Scenes/Gameplay.unity`<br>`_Game/Prefab/UI/PurchaseSuccessPopup.prefab` |
| 7 | `_Game/BoosterUI_Picked/Frame_Popup_1.png` | Booster | 552x552 | 100 | Sprite | 150,150,150,150 | không | `_Game/Prefab/UI/PurchaseSuccessPopup.prefab` |
| 8 | `_Game/BoosterUI_Picked/Frame_Popup_2.png` | Booster | 264x264 | 100 | Sprite | 150,150,150,150 | không | `Scenes/Gameplay.unity`<br>`_Game/Prefab/CoinHUD.prefab`<br>`_Game/Prefab/UI/PurchaseSuccessPopup.prefab` |
| 9 | `_Game/BoosterUI_Picked/Icon_ADS.png` | Booster | 80x66 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 10 | `_Game/BoosterUI_Picked/Icon_Broom_2.png` | Booster | 306x350 | 100 | Sprite | - | không | `Scenes/Gameplay.unity`<br>`Scenes/Home.unity` |
| 11 | `_Game/BoosterUI_Picked/Icon_Broom_3.png` | Booster | 306x350 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 12 | `_Game/BoosterUI_Picked/Icon_Coin_S.png` | Booster | 98x98 | 100 | Sprite | - | không | `Scenes/Gameplay.unity`<br>`_Game/Prefab/CoinHUD.prefab` |
| 13 | `_Game/BoosterUI_Picked/Icon_Lock_2.png` | Booster | 73x90 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 14 | `_Game/BoosterUI_Picked/Icon_Noti.png` | Booster | 56x56 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 15 | `_Game/BoosterUI_Picked/Icon_Plus.png` | Booster | 55x56 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 16 | `_Game/BoosterUI_Picked/Icon_Slot_3.png` | Booster | 302x306 | 100 | Sprite | - | không | `Scenes/Gameplay.unity`<br>`Scenes/Home.unity` |
| 17 | `_Game/BoosterUI_Picked/Icon_Slot_4.png` | Booster | 302x306 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 18 | `_Game/BoosterUI_Picked/Icon_Spool_2.png` | Booster | 382x234 | 100 | Sprite | - | không | `Scenes/Gameplay.unity`<br>`Scenes/Home.unity` |
| 19 | `_Game/BoosterUI_Picked/Icon_Spool_3.png` | Booster | 382x234 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 20 | `_Game/Resources/BoosterUI/Button_Blue.png` | Booster | 356x168 | 100 | Default | - | có | *(Resources — load theo path)* |
| 21 | `_Game/Resources/BoosterUI/Button_Boosters.png` | Booster | 194x100 | 100 | Default | - | có | *(Resources — load theo path)* |
| 22 | `_Game/Resources/BoosterUI/Button_Boosters_Gray.png` | Booster | 194x100 | 100 | Default | - | có | *(Resources — load theo path)* |
| 23 | `_Game/Resources/BoosterUI/Button_Close.png` | Booster | 54x53 | 100 | Default | - | có | *(Resources — load theo path)* |
| 24 | `_Game/Resources/BoosterUI/Button_Gray.png` | Booster | 356x168 | 100 | Default | - | có | *(Resources — load theo path)* |
| 25 | `_Game/Resources/BoosterUI/Button_Green.png` | Booster | 356x168 | 100 | Default | - | có | *(Resources — load theo path)* |
| 26 | `_Game/Resources/BoosterUI/Frame_Popup_1.png` | Booster | 552x552 | 100 | Default | - | có | *(Resources — load theo path)* |
| 27 | `_Game/Resources/BoosterUI/Frame_Popup_2.png` | Booster | 264x264 | 100 | Default | - | có | *(Resources — load theo path)* |
| 28 | `_Game/Resources/BoosterUI/Icon_ADS.png` | Booster | 80x66 | 100 | Default | - | có | *(Resources — load theo path)* |
| 29 | `_Game/Resources/BoosterUI/Icon_Broom_2.png` | Booster | 306x350 | 100 | Default | - | có | *(Resources — load theo path)* |
| 30 | `_Game/Resources/BoosterUI/Icon_Broom_3.png` | Booster | 306x350 | 100 | Default | - | có | *(Resources — load theo path)* |
| 31 | `_Game/Resources/BoosterUI/Icon_Coin_S.png` | Booster | 98x98 | 100 | Default | - | có | *(Resources — load theo path)* |
| 32 | `_Game/Resources/BoosterUI/Icon_Lock_2.png` | Booster | 73x90 | 100 | Default | - | có | *(Resources — load theo path)* |
| 33 | `_Game/Resources/BoosterUI/Icon_Noti.png` | Booster | 56x56 | 100 | Default | - | có | *(Resources — load theo path)* |
| 34 | `_Game/Resources/BoosterUI/Icon_Plus.png` | Booster | 55x56 | 100 | Default | - | có | *(Resources — load theo path)* |
| 35 | `_Game/Resources/BoosterUI/Icon_Slot_3.png` | Booster | 302x306 | 100 | Default | - | có | *(Resources — load theo path)* |
| 36 | `_Game/Resources/BoosterUI/Icon_Slot_4.png` | Booster | 302x306 | 100 | Default | - | có | *(Resources — load theo path)* |
| 37 | `_Game/Resources/BoosterUI/Icon_Spool_2.png` | Booster | 382x234 | 100 | Default | - | có | *(Resources — load theo path)* |
| 38 | `_Game/Resources/BoosterUI/Icon_Spool_3.png` | Booster | 382x234 | 100 | Default | - | có | *(Resources — load theo path)* |
| 39 | `_Game/Resources/Shop/btn2Green.png` | Shop | 176x192 | 100 | Sprite | 60,80,60,60 | không | `_Game/Prefab/UI/ShopItem.prefab` |
| 40 | `_Game/Resources/Shop/coinShopPack1.png` | Shop | 200x200 | 100 | Sprite | - | không | `Scenes/Home.unity` |
| 41 | `_Game/Resources/Shop/coinShopPack2.png` | Shop | 200x200 | 100 | Sprite | - | không | `Scenes/Home.unity` |
| 42 | `_Game/Resources/Shop/shopTopBar2.png` | Shop | 256x192 | 100 | Sprite | - | không | `Scenes/Home.unity` |
| 43 | `_Game/Resources/Shop/slot2.png` | Shop | 204x196 | 100 | Sprite | 40,40,40,40 | không | `_Game/Prefab/UI/ShopItem.prefab` |
| 44 | `_Game/Resources/WinLoseUI/Button_Green.png` | WinLose | 356x168 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 45 | `_Game/Resources/WinLoseUI/Button_Yellow.png` | WinLose | 172x168 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 46 | `_Game/Resources/WinLoseUI/Frame_Picture_GP_3.png` | WinLose | 572x574 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 47 | `_Game/Resources/WinLoseUI/Frame_Progress_1.png` | WinLose | 736x62 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 48 | `_Game/Resources/WinLoseUI/Frame_Progress_2.png` | WinLose | 718x46 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 49 | `_Game/Resources/WinLoseUI/Icon_ADS_1.png` | WinLose | 80x66 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 50 | `_Game/Resources/WinLoseUI/Icon_Home_3.png` | WinLose | 82x84 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 51 | `_Game/Resources/WinLoseUI/Pattern_01.png` | WinLose | 784x784 | 100 | Sprite | - | không | `Scenes/Gameplay.unity`<br>`_Game/Prefab/Board.prefab` |
| 52 | `_Game/Resources/WinLoseUI/Pattern_02.png` | WinLose | 784x784 | 100 | Sprite | - | không | `_Game/Prefab/Board.prefab` |
| 53 | `_Game/Resources/WinLoseUI/Text_Congratulations.png` | WinLose | 822x208 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 54 | `_Game/Resources/WinLoseUI/Text_OhNooo.png` | WinLose | 880x200 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 55 | `_Game/Textures/Fabric_Pattern/Fabric_Pattern_ambientocclusion.png` | Gameplay/Material | 627x627 | 100 | Default | - | có | `_Game/Materials/StringMat_Blue.mat`<br>`_Game/Materials/StringMat_Brown.mat`<br>`_Game/Materials/StringMat_Charcoal.mat`<br>`_Game/Materials/StringMat_Coral.mat`<br>`_Game/Materials/StringMat_Cream.mat`<br>`_Game/Materials/StringMat_Crimson.mat`<br>`_Game/Materials/StringMat_Deep.mat`<br>`_Game/Materials/StringMat_Forest.mat`<br>`_Game/Materials/StringMat_Gray.mat`<br>`_Game/Materials/StringMat_Green.mat`<br>`_Game/Materials/StringMat_Lime.mat`<br>`_Game/Materials/StringMat_Magenta.mat`<br>`_Game/Materials/StringMat_Orange.mat`<br>`_Game/Materials/StringMat_Pink.mat`<br>`_Game/Materials/StringMat_Plum.mat`<br>`_Game/Materials/StringMat_Purple.mat`<br>`_Game/Materials/StringMat_Sky.mat`<br>`_Game/Materials/StringMat_Snow.mat`<br>`_Game/Materials/StringMat_Steel.mat`<br>`_Game/Materials/StringMat_Teal.mat`<br>`_Game/Materials/StringMat_Yellow.mat` |
| 56 | `_Game/Textures/Fabric_Pattern/Fabric_Pattern_basecolor.png` | Gameplay/Material | 627x627 | 100 | Default | - | có | `_Game/Materials/StringMat_Blue.mat`<br>`_Game/Materials/StringMat_Brown.mat`<br>`_Game/Materials/StringMat_Charcoal.mat`<br>`_Game/Materials/StringMat_Coral.mat`<br>`_Game/Materials/StringMat_Cream.mat`<br>`_Game/Materials/StringMat_Crimson.mat`<br>`_Game/Materials/StringMat_Deep.mat`<br>`_Game/Materials/StringMat_Forest.mat`<br>`_Game/Materials/StringMat_Gray.mat`<br>`_Game/Materials/StringMat_Green.mat`<br>`_Game/Materials/StringMat_Lime.mat`<br>`_Game/Materials/StringMat_Magenta.mat`<br>`_Game/Materials/StringMat_Orange.mat`<br>`_Game/Materials/StringMat_Pink.mat`<br>`_Game/Materials/StringMat_Plum.mat`<br>`_Game/Materials/StringMat_Purple.mat`<br>`_Game/Materials/StringMat_Sky.mat`<br>`_Game/Materials/StringMat_Snow.mat`<br>`_Game/Materials/StringMat_Steel.mat`<br>`_Game/Materials/StringMat_Teal.mat`<br>`_Game/Materials/StringMat_Yellow.mat` |
| 57 | `_Game/Textures/Fabric_Pattern/Fabric_Pattern_normal.png` | Gameplay/Material | 627x627 | 100 | NormalMap | - | có | `_Game/Materials/StringMat_Blue.mat`<br>`_Game/Materials/StringMat_Brown.mat`<br>`_Game/Materials/StringMat_Charcoal.mat`<br>`_Game/Materials/StringMat_Coral.mat`<br>`_Game/Materials/StringMat_Cream.mat`<br>`_Game/Materials/StringMat_Crimson.mat`<br>`_Game/Materials/StringMat_Deep.mat`<br>`_Game/Materials/StringMat_Forest.mat`<br>`_Game/Materials/StringMat_Gray.mat`<br>`_Game/Materials/StringMat_Green.mat`<br>`_Game/Materials/StringMat_Lime.mat`<br>`_Game/Materials/StringMat_Magenta.mat`<br>`_Game/Materials/StringMat_Orange.mat`<br>`_Game/Materials/StringMat_Pink.mat`<br>`_Game/Materials/StringMat_Plum.mat`<br>`_Game/Materials/StringMat_Purple.mat`<br>`_Game/Materials/StringMat_Sky.mat`<br>`_Game/Materials/StringMat_Snow.mat`<br>`_Game/Materials/StringMat_Steel.mat`<br>`_Game/Materials/StringMat_Teal.mat`<br>`_Game/Materials/StringMat_Yellow.mat` |
| 58 | `_Game/Textures/Fabric_Pattern/Fabric_Pattern_roughness.png` | Gameplay/Material | 256x256 | 100 | Default | - | có | `_Game/Materials/StringMat_Blue.mat`<br>`_Game/Materials/StringMat_Brown.mat`<br>`_Game/Materials/StringMat_Charcoal.mat`<br>`_Game/Materials/StringMat_Coral.mat`<br>`_Game/Materials/StringMat_Cream.mat`<br>`_Game/Materials/StringMat_Crimson.mat`<br>`_Game/Materials/StringMat_Deep.mat`<br>`_Game/Materials/StringMat_Forest.mat`<br>`_Game/Materials/StringMat_Gray.mat`<br>`_Game/Materials/StringMat_Green.mat`<br>`_Game/Materials/StringMat_Lime.mat`<br>`_Game/Materials/StringMat_Magenta.mat`<br>`_Game/Materials/StringMat_Orange.mat`<br>`_Game/Materials/StringMat_Pink.mat`<br>`_Game/Materials/StringMat_Plum.mat`<br>`_Game/Materials/StringMat_Purple.mat`<br>`_Game/Materials/StringMat_Sky.mat`<br>`_Game/Materials/StringMat_Snow.mat`<br>`_Game/Materials/StringMat_Steel.mat`<br>`_Game/Materials/StringMat_Teal.mat`<br>`_Game/Materials/StringMat_Yellow.mat` |
| 59 | `_Game/Textures/Gameplay/Pattern.png` | Gameplay/Material | 480x480 | 100 | Sprite | - | không | `Scenes/Gameplay.unity`<br>`_Game/Materials/MaskPattern.mat` |
| 60 | `_Game/Textures/Home/Home_icon_big.png` | Gameplay/Material | 251x247 | 100 | Sprite | - | không | `_Game/_HomeFrame/Resources/MenuBar.prefab` |
| 61 | `_Game/Textures/MasterColor.png` | Gameplay/Material | 1024x1024 | 100 | Default | - | có | `_Game/Materials/SpoolBase.mat`<br>`_Game/Materials/Spool_Lime.mat`<br>`_Game/Materials/baseMat.mat` |
| 62 | `_Game/Textures/MasterColorMask.png` | Gameplay/Material | 1024x1024 | 100 | Default | - | có | `_Game/Materials/baseMat.mat` |
| 63 | `_Game/Textures/Pattern/Pattern_ambientocclusion.png` | Gameplay/Material | 627x627 | 100 | Default | - | có | `_Game/Materials/ThreadMat_Blue.mat`<br>`_Game/Materials/ThreadMat_Brown.mat`<br>`_Game/Materials/ThreadMat_Charcoal.mat`<br>`_Game/Materials/ThreadMat_Coral.mat`<br>`_Game/Materials/ThreadMat_Cream.mat`<br>`_Game/Materials/ThreadMat_Crimson.mat`<br>`_Game/Materials/ThreadMat_Deep.mat`<br>`_Game/Materials/ThreadMat_Forest.mat`<br>`_Game/Materials/ThreadMat_Gray.mat`<br>`_Game/Materials/ThreadMat_Green.mat`<br>`_Game/Materials/ThreadMat_Lime.mat`<br>`_Game/Materials/ThreadMat_Magenta.mat`<br>`_Game/Materials/ThreadMat_Orange.mat`<br>`_Game/Materials/ThreadMat_Pink.mat`<br>`_Game/Materials/ThreadMat_Plum.mat`<br>`_Game/Materials/ThreadMat_Purple.mat`<br>`_Game/Materials/ThreadMat_Sky.mat`<br>`_Game/Materials/ThreadMat_Snow.mat`<br>`_Game/Materials/ThreadMat_Steel.mat`<br>`_Game/Materials/ThreadMat_Teal.mat`<br>`_Game/Materials/ThreadMat_Yellow.mat` |
| 64 | `_Game/Textures/Pattern/Pattern_basecolor.png` | Gameplay/Material | 627x627 | 100 | Default | - | có | `_Game/Materials/ThreadMat_Blue.mat`<br>`_Game/Materials/ThreadMat_Brown.mat`<br>`_Game/Materials/ThreadMat_Charcoal.mat`<br>`_Game/Materials/ThreadMat_Coral.mat`<br>`_Game/Materials/ThreadMat_Cream.mat`<br>`_Game/Materials/ThreadMat_Crimson.mat`<br>`_Game/Materials/ThreadMat_Deep.mat`<br>`_Game/Materials/ThreadMat_Forest.mat`<br>`_Game/Materials/ThreadMat_Gray.mat`<br>`_Game/Materials/ThreadMat_Green.mat`<br>`_Game/Materials/ThreadMat_Lime.mat`<br>`_Game/Materials/ThreadMat_Magenta.mat`<br>`_Game/Materials/ThreadMat_Orange.mat`<br>`_Game/Materials/ThreadMat_Pink.mat`<br>`_Game/Materials/ThreadMat_Plum.mat`<br>`_Game/Materials/ThreadMat_Purple.mat`<br>`_Game/Materials/ThreadMat_Sky.mat`<br>`_Game/Materials/ThreadMat_Snow.mat`<br>`_Game/Materials/ThreadMat_Steel.mat`<br>`_Game/Materials/ThreadMat_Teal.mat`<br>`_Game/Materials/ThreadMat_Yellow.mat` |
| 65 | `_Game/Textures/Pattern/Pattern_normal.png` | Gameplay/Material | 627x627 | 100 | NormalMap | - | có | `_Game/Materials/ThreadMat_Blue.mat`<br>`_Game/Materials/ThreadMat_Brown.mat`<br>`_Game/Materials/ThreadMat_Charcoal.mat`<br>`_Game/Materials/ThreadMat_Coral.mat`<br>`_Game/Materials/ThreadMat_Cream.mat`<br>`_Game/Materials/ThreadMat_Crimson.mat`<br>`_Game/Materials/ThreadMat_Deep.mat`<br>`_Game/Materials/ThreadMat_Forest.mat`<br>`_Game/Materials/ThreadMat_Gray.mat`<br>`_Game/Materials/ThreadMat_Green.mat`<br>`_Game/Materials/ThreadMat_Lime.mat`<br>`_Game/Materials/ThreadMat_Magenta.mat`<br>`_Game/Materials/ThreadMat_Orange.mat`<br>`_Game/Materials/ThreadMat_Pink.mat`<br>`_Game/Materials/ThreadMat_Plum.mat`<br>`_Game/Materials/ThreadMat_Purple.mat`<br>`_Game/Materials/ThreadMat_Sky.mat`<br>`_Game/Materials/ThreadMat_Snow.mat`<br>`_Game/Materials/ThreadMat_Steel.mat`<br>`_Game/Materials/ThreadMat_Teal.mat`<br>`_Game/Materials/ThreadMat_Yellow.mat` |
| 66 | `_Game/Textures/Pattern/Pattern_roughness.png` | Gameplay/Material | 627x627 | 100 | Default | - | có | `_Game/Materials/ThreadMat_Blue.mat`<br>`_Game/Materials/ThreadMat_Brown.mat`<br>`_Game/Materials/ThreadMat_Charcoal.mat`<br>`_Game/Materials/ThreadMat_Coral.mat`<br>`_Game/Materials/ThreadMat_Cream.mat`<br>`_Game/Materials/ThreadMat_Crimson.mat`<br>`_Game/Materials/ThreadMat_Deep.mat`<br>`_Game/Materials/ThreadMat_Forest.mat`<br>`_Game/Materials/ThreadMat_Gray.mat`<br>`_Game/Materials/ThreadMat_Green.mat`<br>`_Game/Materials/ThreadMat_Lime.mat`<br>`_Game/Materials/ThreadMat_Magenta.mat`<br>`_Game/Materials/ThreadMat_Orange.mat`<br>`_Game/Materials/ThreadMat_Pink.mat`<br>`_Game/Materials/ThreadMat_Plum.mat`<br>`_Game/Materials/ThreadMat_Purple.mat`<br>`_Game/Materials/ThreadMat_Sky.mat`<br>`_Game/Materials/ThreadMat_Snow.mat`<br>`_Game/Materials/ThreadMat_Steel.mat`<br>`_Game/Materials/ThreadMat_Teal.mat`<br>`_Game/Materials/ThreadMat_Yellow.mat` |
| 67 | `_Game/Textures/RainbowTexture.png` | Gameplay/Material | 14x885 | 100 | Default | - | có | `_Game/Materials/SpoolBase.mat`<br>`_Game/Materials/Spool_Rainbow.mat` |
| 68 | `_Game/Textures/UI/BGR_Gameplay.png` | UI | 1620x2640 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 69 | `_Game/Textures/UI/Frame_Picture_GP.png` | UI | 572x624 | 100 | Sprite | - | không | `_Game/Prefab/Board.prefab` |
| 70 | `_Game/Textures/UI/Icon_WoolBasket.png` | UI | 140x132 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 71 | `_Game/Textures/UI/Slider/Frame_GamePlay_Minus.png` | UI | 28x6 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 72 | `_Game/Textures/UI/Slider/Frame_GamePlay_Plus.png` | UI | 28x28 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 73 | `_Game/Textures/UI/Slider/Frame_Slider_1.png` | UI | 50x250 | 100 | Sprite | 0,50,0,50 | không | `Scenes/Gameplay.unity` |
| 74 | `_Game/Textures/UI/Slider/Frame_Slider_2.png` | UI | 58x258 | 100 | Sprite | 0,50,0,50 | không | `Scenes/Gameplay.unity` |
| 75 | `_Game/Textures/UI/Slider/Frame_Slider_3.png` | UI | 64x62 | 100 | Sprite | - | không | `Scenes/Gameplay.unity` |
| 76 | `_Game/Textures/UI/Warning_Stroke.png` | UI | 512x512 | 100 | Sprite | 137,136,134,140 | không | `_Game/Prefab/VFX/Warning_Lose.prefab` |
| 77 | `_Game/Textures/UIV2/Frame_Bar_1.png` | UI | 359x330 | 100 | Sprite | 150,150,150,150 | không | `Scenes/EntryScene.unity`<br>`Scenes/Home.unity`<br>`_Game/_HomeFrame/Resources/MenuBar.prefab` |
| 78 | `_Game/Textures/UIV2/Frame_Bar_2.png` | UI | 364x378 | 100 | Sprite | 150,150,150,150 | không | `_Game/_HomeFrame/Resources/MenuBar.prefab` |
| 79 | `_Game/Textures/UIV2/Frame_Bar_3.png` | UI | 358x334 | 100 | Sprite | 150,150,150,150 | không | `_Game/_HomeFrame/Resources/MenuBar.prefab` |
| 80 | `_Game/Textures/UIV2/Frame_Popup_4.png` | UI | 820x142 | 100 | Sprite | - | không | `_Game/Prefab/UI/PurchaseSuccessPopup.prefab` |
| 81 | `_Game/Textures/UIV2/Frame_Tooltips.png` | UI | 860x246 | 100 | Sprite | 300,0,300,0 | không | `_KIT/GUI/Prefab/TooltipManager.prefab` |
| 82 | `_Game/Textures/UIV2/Icon_Coin_1.png` | UI | 100x100 | 100 | Sprite | - | không | `_Game/Prefab/UI/ImgCoin.prefab` |
| 83 | `_Game/Textures/UIV2/Icon_Lock.png` | UI | 190x234 | 100 | Sprite | - | không | `_Game/_HomeFrame/Resources/MenuBar.prefab` |
| 84 | `_Game/Textures/UIV2/Icon_Shop.png` | UI | 234x226 | 100 | Sprite | - | không | `_Game/_HomeFrame/Resources/MenuBar.prefab` |
| 85 | `_Game/Textures/box28x28.png` | Gameplay/Material | 28x28 | 100 | Sprite | - | không | `_Game/Prefab/Board.prefab` |
| 86 | `_Game/Textures/box28x28_white.png` | Gameplay/Material | 28x28 | 100 | Sprite | - | không | `_Game/Prefab/Board.prefab` |
| 87 | `_Game/Textures/shadow.png` | Gameplay/Material | 512x512 | 100 | Sprite | - | không | `_Game/Materials/shadow.mat` |

## 2. Ảnh game KHÔNG DÙNG (7)

> Xoá được (nếu không tính tới việc dùng lại sau này). Kiểm tra kỹ trước khi xoá — xem cột *Lý do*.

| # | Path | Nhóm | Kích thước | PPU | Kiểu | 9-slice | Lý do |
|---|---|---|---|---|---|---|---|
| 1 | `_Game/BoosterUI_Picked/Button_Gray.png` | Booster | 356x168 | 100 | Sprite | - | Không asset nào trỏ tới |
| 2 | `_Game/Textures/Home/background.png` | Gameplay/Material | 1082x2160 | 100 | Sprite | - | Không asset nào trỏ tới |
| 3 | `_Game/Textures/UIV2/Frame_Popup_3.png` | UI | 278x68 | 100 | Sprite | 50,34,50,34 | Không asset nào trỏ tới |
| 4 | `_Game/Textures/text_diffuse.jpg` | Gameplay/Material | 512x512 | 100 | Default | - | Chỉ được trỏ bởi asset không dùng: _Game/Materials/WoodMat_Gray.mat, _Game/Materials/WoodMat_White.mat, _Game/Materials/WoodMat_Yellow.mat |
| 5 | `_Game/Textures/text_normal.png` | Gameplay/Material | 256x256 | 100 | NormalMap | - | Chỉ được trỏ bởi asset không dùng: _Game/Materials/WoodMat_Gray.mat, _Game/Materials/WoodMat_White.mat, _Game/Materials/WoodMat_Yellow.mat |
| 6 | `_Game/Textures/text_roughness_1.jpg` | Gameplay/Material | 512x512 | 100 | Default | - | Chỉ được trỏ bởi asset không dùng: _Game/Materials/WoodMat_Gray.mat, _Game/Materials/WoodMat_Yellow.mat |
| 7 | `_Game/Textures/text_roughness_2.jpg` | Gameplay/Material | 512x512 | 100 | Default | - | Chỉ được trỏ bởi asset không dùng: _Game/Materials/WoodMat_White.mat |

## 3. Ảnh plugin/editor — không reskin (187 không dùng)

> Icon của DOTween / DemiLib / Dreamteck Splines. Không liên quan tới game, đừng đụng vào.

| # | Path | Kích thước |
|---|---|---|
| 1 | `Demigiant/DOTween/Editor/Imgs/DOTweenIcon.png` | 93x18 |
| 2 | `Demigiant/DOTween/Editor/Imgs/Footer.png` | 194x24 |
| 3 | `Demigiant/DOTween/Editor/Imgs/Footer_dark.png` | 194x24 |
| 4 | `Demigiant/DOTween/Editor/Imgs/Header.jpg` | 512x164 |
| 5 | `Demigiant/DemiLib/Core/Editor/Imgs/blackSquare.png` | 16x16 |
| 6 | `Demigiant/DemiLib/Core/Editor/Imgs/blackSquareAlpha10.png` | 16x16 |
| 7 | `Demigiant/DemiLib/Core/Editor/Imgs/blackSquareAlpha15.png` | 16x16 |
| 8 | `Demigiant/DemiLib/Core/Editor/Imgs/blackSquareAlpha25.png` | 16x16 |
| 9 | `Demigiant/DemiLib/Core/Editor/Imgs/blackSquareAlpha50.png` | 16x16 |
| 10 | `Demigiant/DemiLib/Core/Editor/Imgs/blackSquareAlpha80.png` | 16x16 |
| 11 | `Demigiant/DemiLib/Core/Editor/Imgs/blueSquare.png` | 16x16 |
| 12 | `Demigiant/DemiLib/Core/Editor/Imgs/circle.png` | 32x32 |
| 13 | `Demigiant/DemiLib/Core/Editor/Imgs/greenSquare.png` | 16x16 |
| 14 | `Demigiant/DemiLib/Core/Editor/Imgs/grid_bright.png` | 64x64 |
| 15 | `Demigiant/DemiLib/Core/Editor/Imgs/grid_dark.png` | 64x64 |
| 16 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alert.png` | 23x20 |
| 17 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignB.png` | 16x16 |
| 18 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignBC.png` | 20x20 |
| 19 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignBL.png` | 20x20 |
| 20 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignBR.png` | 20x20 |
| 21 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignCC.png` | 20x20 |
| 22 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignCL.png` | 20x20 |
| 23 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignCR.png` | 20x20 |
| 24 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignHC.png` | 16x16 |
| 25 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignL.png` | 16x16 |
| 26 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignR.png` | 16x16 |
| 27 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignT.png` | 16x16 |
| 28 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignTC.png` | 20x20 |
| 29 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignTL.png` | 20x20 |
| 30 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignTR.png` | 20x20 |
| 31 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_alignVC.png` | 16x16 |
| 32 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_camera.png` | 11x10 |
| 33 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_camera_border.png` | 11x11 |
| 34 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_cog.png` | 10x10 |
| 35 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_cog_border.png` | 10x10 |
| 36 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_comment.png` | 8x8 |
| 37 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_comment_border.png` | 10x10 |
| 38 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_delete.png` | 8x8 |
| 39 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_demigiant.png` | 16x16 |
| 40 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_distributeHAlignT.png` | 16x16 |
| 41 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_distributeVAlignL.png` | 16x16 |
| 42 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_end.png` | 20x20 |
| 43 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_flipV.png` | 10x12 |
| 44 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_foldout_closed.png` | 14x14 |
| 45 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_foldout_open.png` | 14x14 |
| 46 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_heart.png` | 10x10 |
| 47 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_heart_border.png` | 10x10 |
| 48 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_light.png` | 7x10 |
| 49 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_light_border.png` | 7x10 |
| 50 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_lock.png` | 13x14 |
| 51 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_lock_open.png` | 13x14 |
| 52 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_nodeArrow.png` | 10x16 |
| 53 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_ok.png` | 20x20 |
| 54 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_optionsDropdown.png` | 16x11 |
| 55 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_play.png` | 12x12 |
| 56 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_play_border.png` | 12x12 |
| 57 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_skull.png` | 10x10 |
| 58 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_skull_border.png` | 10x10 |
| 59 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_star.png` | 10x10 |
| 60 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_star_border.png` | 10x10 |
| 61 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_ui.png` | 10x10 |
| 62 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_ui_border.png` | 12x12 |
| 63 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_visibility.png` | 16x12 |
| 64 | `Demigiant/DemiLib/Core/Editor/Imgs/ico_visibility_off.png` | 16x12 |
| 65 | `Demigiant/DemiLib/Core/Editor/Imgs/orangeSquare.png` | 16x16 |
| 66 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_atlas.png` | 14x14 |
| 67 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_audio.png` | 14x15 |
| 68 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_bundle.png` | 16x16 |
| 69 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_cog.png` | 15x15 |
| 70 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_cross.png` | 15x15 |
| 71 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_demigiant.png` | 10x10 |
| 72 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_folder.png` | 15x13 |
| 73 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_fonts.png` | 17x13 |
| 74 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_heart.png` | 15x15 |
| 75 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_materials.png` | 13x13 |
| 76 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_models.png` | 13x13 |
| 77 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_particles.png` | 15x16 |
| 78 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_play.png` | 12x13 |
| 79 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_prefab.png` | 13x13 |
| 80 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_scripts.png` | 17x13 |
| 81 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_shaders.png` | 11x13 |
| 82 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_skull.png` | 15x15 |
| 83 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_star.png` | 15x15 |
| 84 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_terrains.png` | 16x13 |
| 85 | `Demigiant/DemiLib/Core/Editor/Imgs/project/ico_textures.png` | 13x13 |
| 86 | `Demigiant/DemiLib/Core/Editor/Imgs/purpleSquare.png` | 16x16 |
| 87 | `Demigiant/DemiLib/Core/Editor/Imgs/redSquare.png` | 16x16 |
| 88 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorder.png` | 16x16 |
| 89 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderAlpha15.png` | 16x16 |
| 90 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurved.png` | 16x16 |
| 91 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurved02.png` | 16x16 |
| 92 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurved02_darkBorders.png` | 16x16 |
| 93 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurvedAlpha.png` | 16x16 |
| 94 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurvedEmpty.png` | 16x16 |
| 95 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurvedEmpty02.png` | 16x16 |
| 96 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurvedEmptyThick.png` | 16x16 |
| 97 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurved_darkBorders.png` | 16x16 |
| 98 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderCurved_darkBordersAlpha.png` | 16x16 |
| 99 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderEmpty.png` | 16x16 |
| 100 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderEmpty01.png` | 16x16 |
| 101 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderEmpty02.png` | 16x16 |
| 102 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderEmpty03.png` | 16x16 |
| 103 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderThickEmpty.png` | 16x16 |
| 104 | `Demigiant/DemiLib/Core/Editor/Imgs/squareBorderThickerEmpty.png` | 16x16 |
| 105 | `Demigiant/DemiLib/Core/Editor/Imgs/squareCorners03.png` | 16x16 |
| 106 | `Demigiant/DemiLib/Core/Editor/Imgs/squareCornersEmpty02.png` | 16x16 |
| 107 | `Demigiant/DemiLib/Core/Editor/Imgs/tileBars_empty.png` | 32x32 |
| 108 | `Demigiant/DemiLib/Core/Editor/Imgs/tileBars_slanted.png` | 32x32 |
| 109 | `Demigiant/DemiLib/Core/Editor/Imgs/tileBars_slanted_alpha.png` | 32x32 |
| 110 | `Demigiant/DemiLib/Core/Editor/Imgs/transparentSquare.png` | 16x16 |
| 111 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteDot.png` | 8x8 |
| 112 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteDot_darkBorder.png` | 8x8 |
| 113 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteDot_whiteBorderAlpha.png` | 8x8 |
| 114 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteSquare.png` | 16x16 |
| 115 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteSquareAlpha10.png` | 16x16 |
| 116 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteSquareAlpha15.png` | 16x16 |
| 117 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteSquareAlpha25.png` | 16x16 |
| 118 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteSquareAlpha50.png` | 16x16 |
| 119 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteSquareAlpha80.png` | 16x16 |
| 120 | `Demigiant/DemiLib/Core/Editor/Imgs/whiteSquare_fadeOut_bt.png` | 16x16 |
| 121 | `Demigiant/DemiLib/Core/Editor/Imgs/yellowSquare.png` | 16x16 |
| 122 | `Plugins/Dreamteck/Splines/Components/Icons/BoxColliderGenerator.png` | 128x128 |
| 123 | `Plugins/Dreamteck/Splines/Components/Icons/CapsuleColliderGenerator.png` | 128x128 |
| 124 | `Plugins/Dreamteck/Splines/Components/Icons/EdgeColliderGenerator.png` | 128x128 |
| 125 | `Plugins/Dreamteck/Splines/Components/Icons/LengthCalculator.png` | 128x128 |
| 126 | `Plugins/Dreamteck/Splines/Components/Icons/MultiSplineSurfaceGenerator.png` | 128x128 |
| 127 | `Plugins/Dreamteck/Splines/Components/Icons/NodeIcon.png` | 128x128 |
| 128 | `Plugins/Dreamteck/Splines/Components/Icons/ObjectBender.png` | 128x128 |
| 129 | `Plugins/Dreamteck/Splines/Components/Icons/ObjectController.png` | 128x128 |
| 130 | `Plugins/Dreamteck/Splines/Components/Icons/ObjectControllerCustomRule.png` | 128x128 |
| 131 | `Plugins/Dreamteck/Splines/Components/Icons/ParticleController.png` | 128x128 |
| 132 | `Plugins/Dreamteck/Splines/Components/Icons/PathGenerator.png` | 128x128 |
| 133 | `Plugins/Dreamteck/Splines/Components/Icons/PolygonColliderGenerator.png` | 128x128 |
| 134 | `Plugins/Dreamteck/Splines/Components/Icons/SplineComputer.png` | 128x128 |
| 135 | `Plugins/Dreamteck/Splines/Components/Icons/SplineFollower.png` | 128x128 |
| 136 | `Plugins/Dreamteck/Splines/Components/Icons/SplineMesh.png` | 128x128 |
| 137 | `Plugins/Dreamteck/Splines/Components/Icons/SplineMorph.png` | 128x128 |
| 138 | `Plugins/Dreamteck/Splines/Components/Icons/SplinePositioner.png` | 128x128 |
| 139 | `Plugins/Dreamteck/Splines/Components/Icons/SplineProjector.png` | 128x128 |
| 140 | `Plugins/Dreamteck/Splines/Components/Icons/SplineRenderer.png` | 128x128 |
| 141 | `Plugins/Dreamteck/Splines/Components/Icons/SplineUser.png` | 128x128 |
| 142 | `Plugins/Dreamteck/Splines/Components/Icons/SurfaceGenerator.png` | 128x128 |
| 143 | `Plugins/Dreamteck/Splines/Components/Icons/TubeGenerator.png` | 128x128 |
| 144 | `Plugins/Dreamteck/Splines/Components/Icons/WaveformGenerator.png` | 128x128 |
| 145 | `Plugins/Dreamteck/Splines/Editor/Icons/add.png` | 20x20 |
| 146 | `Plugins/Dreamteck/Splines/Editor/Icons/add_dark.png` | 20x20 |
| 147 | `Plugins/Dreamteck/Splines/Editor/Icons/add_on.png` | 20x20 |
| 148 | `Plugins/Dreamteck/Splines/Editor/Icons/add_on_dark.png` | 20x20 |
| 149 | `Plugins/Dreamteck/Splines/Editor/Icons/edit_cursor.png` | 16x16 |
| 150 | `Plugins/Dreamteck/Splines/Editor/Icons/forever.png` | 64x64 |
| 151 | `Plugins/Dreamteck/Splines/Editor/Icons/merge.png` | 20x20 |
| 152 | `Plugins/Dreamteck/Splines/Editor/Icons/merge_dark.png` | 20x20 |
| 153 | `Plugins/Dreamteck/Splines/Editor/Icons/merge_on.png` | 20x20 |
| 154 | `Plugins/Dreamteck/Splines/Editor/Icons/merge_on_dark.png` | 20x20 |
| 155 | `Plugins/Dreamteck/Splines/Editor/Icons/mirror.png` | 20x20 |
| 156 | `Plugins/Dreamteck/Splines/Editor/Icons/mirror_dark.png` | 20x20 |
| 157 | `Plugins/Dreamteck/Splines/Editor/Icons/mirror_on.png` | 20x20 |
| 158 | `Plugins/Dreamteck/Splines/Editor/Icons/mirror_on_dark.png` | 20x20 |
| 159 | `Plugins/Dreamteck/Splines/Editor/Icons/normal.png` | 20x20 |
| 160 | `Plugins/Dreamteck/Splines/Editor/Icons/normal_dark.png` | 20x20 |
| 161 | `Plugins/Dreamteck/Splines/Editor/Icons/normal_on.png` | 20x20 |
| 162 | `Plugins/Dreamteck/Splines/Editor/Icons/normal_on_dark.png` | 20x20 |
| 163 | `Plugins/Dreamteck/Splines/Editor/Icons/plugin_header.png` | 450x82 |
| 164 | `Plugins/Dreamteck/Splines/Editor/Icons/primitives.png` | 20x20 |
| 165 | `Plugins/Dreamteck/Splines/Editor/Icons/primitives_dark.png` | 20x20 |
| 166 | `Plugins/Dreamteck/Splines/Editor/Icons/primitives_on.png` | 20x20 |
| 167 | `Plugins/Dreamteck/Splines/Editor/Icons/primitives_on_dark.png` | 20x20 |
| 168 | `Plugins/Dreamteck/Splines/Editor/Icons/remove.png` | 20x20 |
| 169 | `Plugins/Dreamteck/Splines/Editor/Icons/remove_dark.png` | 20x20 |
| 170 | `Plugins/Dreamteck/Splines/Editor/Icons/remove_on.png` | 20x20 |
| 171 | `Plugins/Dreamteck/Splines/Editor/Icons/remove_on_dark.png` | 20x20 |
| 172 | `Plugins/Dreamteck/Splines/Editor/Icons/split.png` | 20x20 |
| 173 | `Plugins/Dreamteck/Splines/Editor/Icons/split_dark.png` | 20x20 |
| 174 | `Plugins/Dreamteck/Splines/Editor/Icons/split_on.png` | 20x20 |
| 175 | `Plugins/Dreamteck/Splines/Editor/Icons/split_on_dark.png` | 20x20 |
| 176 | `Plugins/Dreamteck/Utilities/Editor/Images/changelog.png` | 64x64 |
| 177 | `Plugins/Dreamteck/Utilities/Editor/Images/discord.png` | 64x64 |
| 178 | `Plugins/Dreamteck/Utilities/Editor/Images/examples.png` | 64x64 |
| 179 | `Plugins/Dreamteck/Utilities/Editor/Images/get_started.png` | 64x64 |
| 180 | `Plugins/Dreamteck/Utilities/Editor/Images/manual.png` | 64x64 |
| 181 | `Plugins/Dreamteck/Utilities/Editor/Images/pdf.png` | 64x64 |
| 182 | `Plugins/Dreamteck/Utilities/Editor/Images/playmaker.png` | 64x64 |
| 183 | `Plugins/Dreamteck/Utilities/Editor/Images/rate.png` | 64x64 |
| 184 | `Plugins/Dreamteck/Utilities/Editor/Images/support.png` | 64x64 |
| 185 | `Plugins/Dreamteck/Utilities/Editor/Images/tmpro.png` | 128x128 |
| 186 | `Plugins/Dreamteck/Utilities/Editor/Images/tutorials.png` | 64x64 |
| 187 | `Plugins/Dreamteck/Utilities/Editor/Images/youtube.png` | 64x64 |

## Ghi chú khi reskin

1. **`_Game/BoosterUI_Picked/`** là bản để sửa. Sau khi đổi PNG ở đây phải **copy đè** sang
   `_Game/Resources/BoosterUI/` (giữ nguyên tên file) vì đó là bản runtime fallback
   (`BoosterBarUI.ResolveSprites()` → `Resources.Load("BoosterUI/…")`).
2. **PPU = 100** cho toàn bộ ảnh UI → 1 pixel = 0.01 unit. Đổi kích thước PNG mà giữ PPU
   sẽ làm UI phình to/nhỏ lại; nếu muốn giữ nguyên kích thước trên màn hình thì giữ nguyên pixel.
3. Ảnh 9-slice (`Frame_Bar_1/2/3`, `Frame_Popup_1/2`, `Button_Green`, `Button_Close`, `slot2`, `btn2Green`)
   có `spriteBorder` trong `.meta` — đổi PNG phải giữ nguyên `spriteBorder`, nếu không sẽ méo.
4. `TextMesh Pro/Sprites/EmojiOne.png` là sprite sheet của TMP — không reskin tự do.
5. Bộ texture gỗ (`text_diffuse.jpg`, `text_normal.png`, `text_roughness_1/2.jpg`) chỉ được 3
   material `WoodMat_Gray/White/Yellow.mat` trỏ tới, mà 3 material đó **không asset nào dùng** →
   cả bộ đều có thể xoá cùng nhau.
