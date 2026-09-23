# Feature summary template

Mỗi feature đã implement phải có một file `docs/features/Fxx-<feature-name>.md` theo mẫu này.

## 1. Mục tiêu
- Người chơi làm được gì?
- Phạm vi và điều kiện không làm.

## 2. Luồng xử lý
```text
Input -> Command -> Domain/Application -> View/Animation -> Outcome
```

Ghi rõ state trước/sau và các nhánh lỗi.

## 3. Thiết kế OOP
| Thành phần | Trách nhiệm | Không được biết về |
|---|---|---|
| Domain/Core | Luật thuần, state, invariant | UnityEngine, UI, SDK, PlayerPrefs |
| Application | Điều phối use case và port | Chi tiết prefab/SDK |
| Unity/Presentation | Hiển thị, input, animation | Tự quyết định luật gameplay |
| Platform | Adapter cho save, ads, IAP, audio... | Luật puzzle |

## 4. File và điểm vào
- Entry point/caller:
- Domain classes:
- Application service:
- Unity components/prefabs/scenes:
- Data/config:
- Tests:

## 5. Placeholder và reskin
- Placeholder assets đang dùng:
- Asset/config nào thay để reskin:
- Màu, font, sprite, animation và âm thanh có nằm ngoài luật gameplay không?
- Không hard-code art trong resolver/core.

## 6. Verification
- Unit/contract test:
- Unity scene smoke test:
- Build/log:
- Known limitations:

## 7. Thay đổi so với reference
Reference chỉ dùng để kiểm tra hành vi. Ghi rõ phần re-implement khác reference; không copy code.
