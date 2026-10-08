# PHẦN CÒN THIẾU THEO TÀI LIỆU (gap analysis)

> Đối chiếu code thực tế với yêu cầu trong `docs/`, `ai-docs/`, `gamedesign/` và backlog `tasks/todo.md`.
> Quy ước: ❌ chưa có · ◐ có một phần/stub · ✅ đã có (xem file 02).

## 1. Điều hướng và màn hình (W3, GDD §6, `docs/05-ui-navigation.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| Scene Entry (loading + route level 1 → Gameplay, level sau → Home) | Chỉ có `Gameplay.unity`; `CoreSceneFlow.cs` là menu chữ placeholder | ❌ |
| Màn Home (carousel level, Play, shop, menu, daily popup) | Không có | ❌ |
| HUD đầy đủ (coin/heart, 3 booster, setting, remove-ads, slider) | Mới có tên level + slider scale | ◐ |
| Popup Win/Lose/Revive/Setting đúng spec | Mới có Win/Lose cơ bản (NEXT/RETRY), Setting in-game trống | ◐ |

## 2. Lưu game và progression (W3, GDD §4, `docs/03-progression-resources.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| `GameData`/save JSON + PlayerPrefs (level, coin 100, heart 5 hồi 1800s, booster, remove_ads, settings…) | Không có file save nào; tiến trình chỉ nằm RAM, tắt game mất | ❌ |
| Thắng +coin/+order, thua/restart −heart | Không có | ❌ |
| Daily reward 7 ngày (thường/x2 ad/bù ngày lỡ) | Không có | ❌ |
| Save versioned + migration, chống chỉnh giờ/chỉnh save | Không có | ❌ |
| Câu hỏi sản phẩm chưa chốt (trừ heart lúc vào hay lúc thua? replay màn cũ?…) | Vẫn mở (`docs/03-progression-resources.md`) | ❌ cần sếp chốt |

## 3. Core theo chuẩn redesign (W4–W5, `docs/10-redesign-architecture-and-roadmap.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| `PuzzleResolver` thuần C# tách khỏi Unity/animation/SDK | Luật nằm lẫn trong `GameManager/SlotManager/Box` + callback animation | ❌ (chạy đúng nhưng khó test tự động) |
| Deterministic replay (seed + command log, W5-04) | Pool random bốc `Random.Range` mỗi ván → cùng level ra layout khác | ❌ |
| Level order ≠ asset ID, validator typed-error, regression 3 level | Có validator Editor mạnh (thay thế một phần), nhưng order hiện = số file, chưa có config order riêng | ◐ |

## 4. Booster và revive (W6, GDD §5, `docs/04-boosters-and-revive.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| Add Slot (+1 waiting, cap 7) | Chưa có; `maxWaitingSlots = 7` mới là số cấu hình | ❌ |
| Clean (gom waiting vào temp box, không mất piece) | Chưa có | ❌ |
| Magic Box (hộp cầu vồng) | Chưa có (FX có trong anim, logic không) | ❌ |
| Revive khi thua (còn box → AddBasket, hết box → Clean) | Chưa có; thua chỉ có RETRY load lại | ❌ |
| Chống double-tap, ad fail không mutate state | Chưa áp dụng được (chưa có booster) | ❌ |

## 5. Tutorial (W6, GDD §7, `docs/06-tutorial.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| Dạy tap/hold/waiting/rotate/booster qua 3 level đầu, data-driven, có skip, không lặp | Mới có 1 marker data `TutorialTargetMarker.cs`, không controller | ❌ |
| Test tutorial bằng event giả | Chưa có | ❌ |

## 6. Kiếm tiền và platform (W7, GDD §8, `docs/07-monetization.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| Rewarded ads (revive, x2 reward/daily), interstitial có frequency cap | Không có SDK ads nào trong `Packages`, không code gọi ads | ❌ |
| IAP (No Ads, bundle, coin/booster pack, restore) + receipt validation | Không có | ❌ |
| Shop, analytics event (`level_started/completed/failed`…), remote-config, Firebase/AppsFlyer | Không có | ❌ |
| Core vẫn chơi được khi SDK lỗi/offline | Đúng một cách hiển nhiên (vì chưa có SDK) | ✅ theo nghĩa "không phụ thuộc" |

## 7. Ngôn ngữ/âm thanh/cài đặt (W6–W8, GDD §9, `docs/08-localization-audio-settings.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| Đa ngôn ngữ key-based (I2 + TMP) | Key localize bị comment; text đang cứng tiếng Anh | ❌ |
| Nhạc/nổ âm thanh theo sự kiện, toggle sound/music/vibration có hiệu lực | `KitSound` tồn tại nhưng 0 nơi gọi; 2 hàm SFX rỗng | ❌ |
| Safe-area, text không cắt màn nhỏ, tín hiệu không phụ thuộc màu duy nhất | Chưa kiểm tra | ❌ |

## 8. QA, thiết bị, build (W8–W9, `docs/11-qa-risk-register.md`)

| Yêu cầu tài liệu | Thực tế | Kết luận |
|---|---|---|
| Acceptance suite tự động (resolver, economy, booster, UI contract) | 1 PlayMode test; 16 EditMode tests chỉ nằm trong evidence W1 của prototype cũ | ◐ |
| Test máy Android thật, nhiều tỉ lệ màn dọc, profiling FPS/memory | Chưa có | ❌ |
| APK/AAB release + smoke test + build log | Chưa có | ❌ |
| Regression checklist (pause/kill/relaunch/mất mạng/đổi ngày) | Chưa chạy | ❌ |

## 9. Nợ kỹ thuật đã được tài liệu điểm tên và vẫn còn (`docs/00-current-state.md`)

* Luật + animation trộn nhau (`GameManager` vừa tính vừa gọi tween/coroutine) — còn nguyên.
* Singleton dày (`GameManager/BoxManager/SlotManager/WaitingArea/GameUI/RopeController…`) — còn nguyên.
* Chưa có `SaveStore` tập trung — chưa bắt đầu.
* Dữ liệu level nhiều nguồn (prefab + matrix + holder debug list) — đã đỡ nhờ validator + nút rebuild, nhưng chưa có `LevelDefinition` chuẩn duy nhất.
