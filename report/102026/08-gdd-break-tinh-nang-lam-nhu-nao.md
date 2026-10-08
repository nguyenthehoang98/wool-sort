# GDD → BREAK TÍNH NĂNG + LÀM NHƯ NÀO

> Đối chiếu từ `05-tai-lieu-thiet-ke-game-tom-tat.md` (10 cụm flow).
> Mỗi tính năng ghi: **break ra việc con + làm như nào** (file/hàm/mẫu cụ thể của dự án này), kèm trạng thái ✅/◐/❌.
> Mức chi tiết: đủ để dev nhận việc làm ngay, không chia nhỏ tới từng dòng code.

---

## CỤM 1 — MỞ APP & VÀO GAME ❌

| # | Tính năng con | Làm như nào |
|---|---|---|
| 1.1 | Màn Loading + khởi tạo | Dựng scene `Entry`: thanh tiến trình; `Awake` load `GameConfig`, save (F1), init audio/settings; xong mới route. Không cho bấm gì khi đang load |
| 1.2 | Route theo tiến trình | Đọc `currentLevelOrder` từ save: =1 → `LoadScene(Gameplay)`; ≥2 → `LoadScene(Home)`. Viết trong 1 `NavigationService` duy nhất, UI chỉ gọi `NavigateTo()` |
| 1.3 | Chạy khi SDK lỗi/mất mạng | Mọi service mạng bọc interface có bản `NoOp`; core gameplay không `null-check` rải rác mà nhận service đã fallback từ composition root |

## CỤM 2 — MÀN HÌNH CHÍNH (Home) ❌

| # | Tính năng con | Làm như nào |
|---|---|---|
| 2.1 | Carousel level | Scene `Home`: list ~7 item quanh `currentLevelOrder`; item khóa/mở theo save; bấm Play → `NavigationService.PlayLevel(order)` (order ≠ asset ID, map qua config order) |
| 2.2 | Shop/menu/daily popup | Mỗi popup là 1 prefab + `ModalStack` (chỉ 1 modal nhận input; mở là chặn raycast xuống dưới). Nút mở popup gọi intent, không tự trừ tiền trong UI |
| 2.3 | Nút Play đúng màn | `PlayLevel(order)` load `Gameplay` rồi `GameManager.LoadLevel(order)` — thay `debugStartLevel`/`LoadNextLevel` wrap-1 hiện tại bằng order thật từ save |

## CỤM 3 — VÒNG LẶP CHƠI CHÍNH ✅ (còn 2 việc nhỏ)

| # | Tính năng con | Làm như nào |
|---|---|---|
| 3.1 | Tap → slot (giữ nguyên) | Đã chạy: `YarnPart` + `SlotManager.GetNextAvailableSlot`. Việc còn: tách luật chọn slot ra hàm thuần nhận state-trả slot để test không cần scene (W4-01) |
| 3.2 | Inner layer (giữ nguyên) | Đã chạy: `YarnPartLayerSpawner`. Việc còn: `Random.InitState(seed)` theo `(levelOrder, attemptCount)` trước khi bốc pool để QA lặp lại ván lỗi (W5-04) |
| 3.3 | Box 3 slot + sinh hộp (giữ nguyên) | Đã chạy: `BoxManager` + `ChooseNextBoxColor`. Không đụng thêm |
| 3.4 | Tô ảnh (giữ nguyên) | Đã chạy: `JamImage` + matrix 84x84. Việc còn: xóa nhánh đọc ảnh gốc khi chốt matrix-only (A3 trong file 07) |
| 3.5 | Thắng/thua/chơi lại (nối thêm economy) | Đã chạy: `GameManager` + popup. Việc thêm: sau Win gọi `Economy.Grant(coin)` + `Progression.UnlockNext()`; sau Lose gọi `Economy.Spend(heart)` — mỗi cái đúng 1 lần, có `transactionId` theo ván |

## CỤM 4 — CỨU VÁN: BOOSTER & HỒI SINH ❌ (nhiều việc nhất)

| # | Tính năng con | Làm như nào |
|---|---|---|
| 4.0 | Khung chung BoosterManager (làm trước) | Tạo `Gameplay/BoosterManager.cs` + state machine `Idle→Validating→Locked→Applying→Animating→Reconcile` (theo `ai-docs/04`). Mọi booster đi qua 1 cổng: check kho → khóa input → trừ 1 lần → apply state → phát animation từ event → mở khóa khi xong/timeout. Double-tap cùng lúc chỉ 1 lệnh chạy |
| 4.1 | Add Slot | `WaitingAreaManager.TryAddSlot()`: `if (slots >= maxWaitingSlots(7)) reject + tooltip`; ngược lại instantiate thêm slot, `SlotManager.RegisterSlot`, chạy `RefreshLayout`. UI trừ kho sau khi thành công |
| 4.2 | Clean | Gom `CurrentPart` của mọi waiting slot vào 1 temp-box (tái dùng prefab Box, không cho tương tác); waiting trống để chơi tiếp; temp box xử lý xong thì hủy. Viết test: animation ngắt giữa chừng vẫn đủ piece |
| 4.3 | Magic Box | Lấy box đầu hàng chờ (`BoxManager.GetFirstBox()` đã có) → `SetRainbowColor()` visual, giữ `boxColor` logic gốc để tô ảnh đúng ô; flag `magicActive`, dùng lặp khi đang active thì reject |
| 4.4 | Revive (gắn vào popup Lose) | Popup Lose thêm 2 nút theo tình huống: còn box chưa unlock → offer "Thêm hộp" (spawn 1 box đúng luật); hết box → offer "Dọn chờ" (= Clean). Trả coin/ad xong **giữ nguyên scene**, chỉ un-frozen state; nút còn lại là Restart (load lại như hiện tại) |

## CỤM 5 — TIẾN TRÌNH & TÀI NGUYÊN ❌

| # | Tính năng con | Làm như nào |
|---|---|---|
| 5.0 | Save game (làm trước) | Tạo `SaveStore`: 1 file JSON `PlayerProfile{schemaVersion, currentLevelOrder, coin, heart, heartTimestampUtc, boosters, removeAds, settings, tutorialFlags, daily}`; ghi nguyên tử (ghi file tạm → đổi tên); `migration` từ save cũ (thiếu field → default, backup trước khi migrate) |
| 5.1 | Economy qua 1 cổng | Tạo `EconomyService.Grant/Spend(resourceId, delta, reason, transactionId)`: `transactionId = $"{sessionId}:{purpose}"` lưu vào ledger; trùng ID → no-op; số dư kiểm tra trước khi trừ, thiếu → fail nguyên tử. UI gọi service, cấm tự `coin++` trong UI |
| 5.2 | Heart + hồi giờ | `heartTimestampUtc` lưu UTC; mở game tính `elapsed/1800s` cộng tim đến max 5. Chống chỉnh giờ: nếu giờ máy < timestamp → không cộng, báo sai giờ |
| 5.3 | Daily 7 ngày | `DailyService`: bảng thưởng 7 ngày + `lastClaimDateUtc`; claim thường; x2 và bù ngày lỡ qua rewarded ad (chờ nhóm H). Streak đứt nếu lỡ >1 ngày không bù |

## CỤM 6 — TUTORIAL ❌

| # | Tính năng con | Làm như nào |
|---|---|---|
| 6.1 | Data + controller | Tạo `TutorialData` (ScriptableObject): list step `{trigger: OnLevelStart/OnCustomEvent, target, text, skippable}`; `TutorialController` đọc session state (không hard-code singleton), chặn input sai step qua `InteractionGate`, cờ `tutorialVersion` trong save để không lặp |
| 6.2 | Nội dung 3 màn đầu | Màn 1: tap 1 sợi (target = part cụ thể, dùng sẵn `TutorialTargetMarker`); màn 2: đầy 1 hộp; màn 3: waiting + hold/rotate. Booster dạy sau, mỗi lần 1 cái, đúng lúc kẹt |
| 6.3 | An toàn | Step có timeout: target mất sau N giây → tự skip; không hiện trên popup/ad/intro; test bằng event giả không cần touch thật |

## CỤM 7 — SHOP & THƯỞNG NỘI TỆ (demo full-client: bỏ hết ads/IAP)

| # | Tính năng con | Làm như nào |
|---|---|---|
| 7.1 | Shop nội tệ | Shop bán booster/heart bằng **coin trong game** (giá đọc từ catalog local, không hard-code UI). Mua = `Economy.Spend` + cộng đồ, fail thì hoàn nguyên. Không tiền thật, không IAP |
| 7.2 | Daily reward (bản không ad) | Điểm danh 7 ngày nhận coin/giờ tim/booster; streak đứt nếu lỡ ngày (không bù ad, không x2 ad). Timer UTC như cũ |
| 7.3 | Thống kê local tối thiểu | Ghi event ra log/file local (`level_started/completed/failed`, `booster_used`, `resource_earned/spent`) làm bằng chứng QA/bảo vệ. Không SDK ngoài, không gửi mạng |

> Đã bỏ khỏi scope: rewarded ad, interstitial + frequency cap, IAP (No Ads/bundle/coin pack), restore purchase, receipt validation, remote-config offer/ad.

## CỤM 8 — CÀI ĐẶT, NGÔN NGỮ, ÂM THANH ❌

| # | Tính năng con | Làm như nào |
|---|---|---|
| 8.1 | Settings store | `SettingsState{language, sound, music, vibration}` lưu chung save (F1); các system subscribe event đổi settings, popup không chạm `AudioSource` trực tiếp |
| 8.2 | Localization | Mọi text UI thành key (dọn các `SetParamLocalize` đang comment); key thiếu chỉ log + hiện fallback, không crash; kiểm tra câu dài/font lớn |
| 8.3 | Audio + rung theo sự kiện | Nối cue vào event sẵn có: Tap/SlotFilled/BoxFull/Win/Lose; music loop tách công tắc với SFX; rung ngắn tắt được; refill nhiều len cùng lúc không chồng âm (1 cue/100ms như mẫu `lastTimeThreadSfx`) |
| 8.4 | Dễ tiếp cận | Nút ≥44pt, safe-area + màn dọc nhiều tỉ lệ, banner không che bàn, luật không truyền bằng mỗi màu sắc (kèm hình/ký hiệu) |

## CỤM 9 — YÊU CẦU NỀN ❌ (làm dần cùng các cụm trên)

| # | Tính năng con | Làm như nào |
|---|---|---|
| 9.1 | Validator trước build | Đã có validator content mạnh; thêm check: order↔asset tồn tại, key localization dùng mà thiếu, save schema version. Chạy 1 nút trước mọi build nội bộ |
| 9.2 | Deterministic replay | Seed (A2/3.2) + log command mỗi ván; chế độ replay đọc log phát lại event để QA đối chiếu |
| 9.3 | Remote-config (tối thiểu) | Bảng `offer/ad frequency/reward/mode` đọc lúc start session (snapshot, không đổi giữa ván); default local + fallback khi offline |

## CỤM 10 — NGHIỆM THU (việc test, không phải code)

| # | Việc | Làm như nào |
|---|---|---|
| 10.1 | Checklist 9 điều (file 05 cụm 10) | Mỗi điều thành 1 case test tay có bước + kết quả mong đợi; chạy hết trước mỗi build nội bộ, ghi pass/fail vào worklog |
| 10.2 | Regression cố định | Pause/kill/relaunch/mất mạng/đổi ngày/màn hình nhỏ: mỗi mục 1 kịch bản, chạy lại sau mọi sửa liên quan save/mạng/giờ |

---

## TỔNG HỢP KHỐI LƯỢNG (để xếp lịch)

| Cụm | Trạng thái | Việc mới phải code |
|---|---|---|
| 1 Entry | ❌ | 3 việc (scene + route + fallback) |
| 2 Home | ❌ | 3 việc (carousel + modal + Play đúng order) |
| 3 Core loop | ✅ + nối economy | 2 việc nhỏ (tách luật test được, seed replay, gắn thưởng/phạt) |
| 4 Booster/Revive | ❌ | 5 việc (khung + 3 booster + revive), cụm nặng nhất |
| 5 Progression | ❌ | 4 việc (save + economy + heart + daily) |
| 6 Tutorial | ❌ | 3 việc (data/controller + nội dung + an toàn) |
| 7 Shop/thưởng nội tệ | ❌ | 3 việc (shop coin + daily không ad + log local, đã bỏ ads/IAP) |
| 8 Settings/loc/audio | ❌ | 4 việc |
| 9 Nền | ◐ | 3 việc (validator thêm + replay + remote-config tối thiểu) |
| 10 Nghiệm thu | ❌ | 2 việc (checklist + regression) |

Thứ tự làm đề xuất: **5.0 save → 1+2 (vào/ra màn) → 3.5 (gắn thưởng) → 6 (tutorial) → 4 (booster/revive) → 7 rút gọn (shop coin + daily) + 8 → 9+10**. Lý do: save là móng mọi cụm khác đứng lên; booster cần trước revive; demo full-client nên bỏ ads/IAP.
