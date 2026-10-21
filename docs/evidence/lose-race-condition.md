# Lỗi logic: báo thua trước khi sợi len kịp vào hộp (2026-10-21)

Báo cáo: *"khi trên thanh ngang có màu, tôi pick đúng sợi len đó nhưng tốc độ nhanh
nó chưa kịp load xong đã bắn thông báo thua rồi"*.

## Nguyên nhân gốc

`Slot.Fill()` đánh dấu slot **đã đầy** ngay lập tức, nhưng chuyển sợi len từ ô chờ
sang hộp lại chạy **sau khi animation ring-appear kết thúc**:

```
Slot.Fill(ô chờ)
  ├─ IsFilled = true            ← ô chờ bị coi là đầy NGAY
  ├─ StartCoroutine(WaitAndTriggerBoxBounce)   ← animation chạy song song
  └─ GlobalBus.Send<SlotFilledEvent>(this)      ← event bắn NGAY
        └─ GameManager.React(SlotFilledEvent)
             slot.Area == Waiting && IsAllWaitingSlotsFilled()  → TRUE
             !BoxManager.IsProcessingBox                       → TRUE (hộp chưa bắt đầu xử lý)
             → LoseLevel()  ❌ BÁO THUA SỚM

  ... sau đó mới xảy ra:
  animation xong → OnAnimationFilled → WaitingAreaManager.TryRefillSlot()
                 → sợi đúng màu mới được đẩy vào hộp (lúc này đã thua rồi)
```

Nói cách khác: **thua được quyết định trước khi sợi len kịp vào hộp**. Tốc độ chơi
càng nhanh (double speed) càng dễ trúng.

> Lưu ý: logic này **có sẵn ở dự án gốc** (`D:\BB_Project\wool-sort`), không phải do
> port gây ra — đã diff `GameManager.React(SlotFilledEvent)` và
> `WaitingAreaManager.IsAllWaitingSlotsFilled()` giữa 2 bên: **giống hệt**.

## Đã sửa

| File | Sửa |
|---|---|
| `GameManager.React(in SlotFilledEvent)` | Nếu ô chờ vừa nhận sợi **còn đang animation** (`!slot.IsAnimationFilled`) → **không kết luận thua**, đặt `isCheckingLose = true` và trì hoãn. |
| `GameManager.RecheckLoseAfterWaitingAnimation(Slot)` | Hàm mới: check lại thua **sau khi** animation xong và đã thử đẩy sợi vào hộp. Vùng chờ còn trống → không thua; đang có hộp xử lý → đợi; vẫn đầy và không hộp nào nhận được → thua. |
| `WaitingAreaManager.TryRefillSlot(Slot)` | Nhánh **không có hộp nào nhận được sợi** → gọi `RecheckLoseAfterWaitingAnimation` (trước đây nhánh này im lặng → thua bị bỏ sót). |
| `WaitingAreaManager.AutoRefillCoroutine` | Nhánh refill **thất bại** → cũng gọi `RecheckLoseAfterWaitingAnimation`. |

### Vì sao check lại đặt ở `WaitingAreaManager` mà không đặt ở `OnAnimationFilled`?

Vì thứ tự gọi trong `Slot.WaitAndTriggerBoxBounce()` là:

```
IsAnimationFilled = true;
OnAnimationFilled?.Invoke();                    ← callback ở đây chạy TRƯỚC
if (Area == Waiting) TryRefillSlot(this);      ← refill chạy SAU
```

Nếu check lại trong `OnAnimationFilled` thì vẫn check **trước** khi refill → lại thua
sớm. Chỉ `TryRefillSlot` (sau khi đã thử refill) mới là lúc biết chắc sợi đó có vào
được hộp hay không.

## Luồng sau khi sửa

| Tình huống | Kết quả |
|---|---|
| Vùng chờ 4/5, bấm sợi **đúng màu** hộp | ô chờ đầy → **trì hoãn** → animation xong → refill vào hộp → vùng chờ 4/5 → **không thua** ✓ |
| Vùng chờ 4/5, bấm sợi **sai màu** | ô chờ đầy → trì hoãn → animation xong → không hộp nào nhận → vùng chờ vẫn 5/5 → **thua** ✓ |
| Vùng chờ 4/5, bấm sợi đúng màu nhưng hộp đó đã đầy | không refill được → vùng chờ vẫn 5/5 → **thua** ✓ |
| Đang có hộp xử lý khi vùng chờ đầy | `isCheckingLose = true` → đợi `HandleAllBoxProcessed` check lại (giữ nguyên hành vi cũ) ✓ |

## Checklist test tay (cần chạy trong Editor)

1. Mở scene `Gameplay` → Play.
2. Bấm các sợi **sai màu** cho tới khi vùng chờ đầy (5/5 ô chờ có sợi).
   → Màn thua **phải hiện** (đúng luật).
3. `Restart` / chơi lại. Để vùng chờ còn **1 ô trống**, rồi bấm sợi **đúng màu**
   của hộp đang mở trên thanh hộp.
   → Sợi phải **bay vào hộp**, vùng chờ giảm 1 ô, **KHÔNG** hiện màn thua.
4. Lặp lại bước 3 với **tốc độ x2** (nếu level có nút double speed) → vẫn không thua.
5. Xem Console: không còn log
   `[GameManager] Vùng chờ đầy nhưng slot vừa nhận sợi còn đang animation → trì hoãn check thua.`
   kèm màn thua.

## Verify

| Kiểm tra | Kết quả |
|---|---|
| Diff `GameManager.React(SlotFilledEvent)` / `IsAllWaitingSlotsFilled` với bản gốc | giống hệt → xác nhận lỗi có sẵn ở gốc |
| Compile 3 assembly | PASS — 0 error (warning đều có sẵn từ file cũ) |
| Audit asset toàn repo | 0 thiếu `.meta`, 0 `.meta` mồ côi, 0 GUID trùng |

**Còn**: chưa có test PlayMode tự động cho race này (cần mô phỏng bấm sợi + đợi animation
trong scene Gameplay) → **cần người dùng chạy checklist ở trên**.
