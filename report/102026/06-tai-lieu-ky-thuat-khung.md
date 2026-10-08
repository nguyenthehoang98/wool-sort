# TÀI LIỆU KỸ THUẬT — WOOL SORT (soạn theo GDD)

> File technical triển khai lại dựa trên `05-tai-lieu-thiet-ke-game-tom-tat.md` (10 cụm flow).
> Mỗi cụm: **GDD yêu cầu gì → code hiện làm tới đâu → còn thiếu gì** (chi tiết cách làm xem `08-gdd-break-tinh-nang-lam-nhu-nao.md`).

## 0. Nền tảng kỹ thuật chung

* **Engine:** Unity 6000.3.21f1, Android target (`ProjectSettings/ProjectVersion.txt`).
* **Thư viện:** uGUI, TextMeshPro, DOTween (tween), LeanTouch (chạm), Dreamteck Splines (dây cong), EventBus nội bộ, Coffee UI.
* **Tool tự viết:** Level Studio 5 tab (`Assets/Editor/LevelStudio/`): tạo màn, độ khó & màu, validate + 4 nút tự sửa, prompt ảnh AI, matrix ảnh (extract/import JSON, ngưỡng gom màu).
* **AI trong quy trình:** OpenCode + Muse Spark — viết/sửa code, quét codebase bằng subagents, vẽ diagram (Archify, qua 4 cổng kiểm tra). Chi tiết: mục 2 file này bản trước + `06` cũ. Chưa dùng MCP server ngoài.
* **Tổ chức Unity:** 1 scene `Gameplay.unity`; prefab `Board/Box/Slot/Rope/Levels`; ScriptableObject `GameConfig`, `YarnColorDatabase` (21 màu), `LevelColorMatrix` (lưới 84x84); singleton runtime: GameManager, BoxManager, SlotManager, WaitingAreaManager, GameUIController, YarnRopeController, JamImage.

---

## PHỤ LỤC A — AI & CÔNG CỤ (chi tiết cách làm việc)

### A1. Vòng đời 1 request (6 bước)
1. **Nhận yêu cầu:** làm rõ phạm vi (`question`). 3 loại: **báo lỗi** (cần log/ảnh + bước tái hiện), **tính năng mới** (cần tiêu chí nghiệm thu), **tài liệu** (cần nguồn + nơi đặt file).
2. **Khảo sát bằng chứng:** `read`/`glob`/`grep`/`bash`; việc lớn tách 1–2 subagent (`task`) chạy song song rồi đối chiếu.
3. **Lập kế hoạch:** việc ≥3 bước ghi `todowrite`, xong đâu tick đó.
4. **Sửa:** ưu tiên `edit` (bắt buộc `read` trước), không tạo file mới trừ khi được yêu cầu.
5. **Tự kiểm tra:** cú pháp, chỗ gọi hàm đổi chữ ký, vùng sửa; không có Unity để Play test → ghi rõ "chưa kiểm chứng runtime".
6. **Bàn giao:** ngắn gọn + file:dòng + cách test; tài liệu vào `report/102026/`.

### A2. Tools AI đang có (11 tool, gọi trực tiếp — không qua MCP)
`read`/`write`/`edit` (file; `write` chỉ cho file mới được yêu cầu), `glob`/`grep` (tìm file/code toàn repo), `bash` PowerShell (git, `Select-String`, đo màu ảnh, copy file — không dùng để đọc/sửa file), `webfetch`/`websearch` (tài liệu ngoài), `task` (subagents song song), `todowrite`, `question`, `skill` (nạp khi đúng việc).

### A3. Skills & MCP
* **`customize-opencode`** (built-in): chỉ khi sửa config/agents/skills/plugins của OpenCode; đổi xong restart app.
* **`archify`** (`~/.agents/skills/archify`): vẽ diagram qua `finalize` + 4 cổng `validate→deliver→check→browser-check`.
* **MCP: chưa dùng server ngoài nào** (không có `opencode.json`/`mcp.json` trên máy). Đề xuất khi cần: MCP browser thật, MCP tra docs Unity đúng version.

### A4. Luật an toàn
Không commit/push khi chưa yêu cầu; không đoán URL/API (chưa chắc thì fetch hoặc ghi "chưa xác minh"); tôn trọng phạm vi user dặn; output ngắn gọn kèm file:dòng.

---

---

## CỤM 1 — MỞ APP & VÀO GAME ❌

* **GDD yêu cầu:** màn loading (tiến trình + load save/config) rồi rẽ nhánh: màn 1 vào thẳng Gameplay, màn sau vào Home; SDK lỗi/mất mạng vẫn chơi.
* **Hiện trạng:** chưa có scene Entry; `CoreSceneFlow.cs` chỉ là menu chữ placeholder.
* **Còn thiếu:** dựng scene `Entry` + `NavigationService` (1 nơi duy nhất đổi scene/modal) + service mạng bọc interface có bản `NoOp`.

## CỤM 2 — MÀN HÌNH CHÍNH (Home) ❌

* **GDD yêu cầu:** carousel ~7 màn quanh màn hiện tại, nút Play, shop, menu, popup daily/settings chặn input xuyên.
* **Hiện trạng:** chưa có scene Home.
* **Còn thiếu:** scene `Home` + `ModalStack` (1 modal nhận input) + `PlayLevel(order)` tách order chơi khỏi asset ID + HUD cơ bản (hiện mới có tên màn + slider).

## CỤM 3 — VÒNG LẶP CHƠI CHÍNH ✅ (còn việc nhỏ)

* **GDD yêu cầu:** tap lớp ngoài → slot (box cùng màu đầy nhất trước, hết thì waiting trái→phải) → hộp 3 sợi đóng nắp → tô ảnh → thắng khi hết sợi+hết box, thua khi chờ đầy; giữ 0.3s xem trước; tap vô lệ = no-op.
* **Hiện trạng:** chạy đủ — `YarnPart` (tap/hold), `SlotManager.cs:69-136` (chọn slot), `YarnPartLayerSpawner` (lớp trong, pool random), `BoxManager` + `ChooseNextBoxColor` (`GameManager.cs:722-879`), `JamImage` + matrix 84x84, popup Win/Lose + NEXT/RETRY (`GameUIController.cs:176-268`), 7 màn Level1–7.
* **Còn thiếu:** tách luật chọn slot ra hàm thuần để test (W4-01); seed pool random để replay (W5-04); gắn thưởng/phạt economy vào Win/Lose (chờ cụm 5); xóa nhánh đọc ảnh gốc khi chốt matrix-only.

## CỤM 4 — CỨU VÁN: BOOSTER & HỒI SINH ❌

* **GDD yêu cầu:** Add Slot (+1 chờ, cap 7), Clean (gom chờ vào hộp tạm, không mất sợi), Magic Box (hộp đầu thành cầu vồng), Revive giữ nguyên ván (còn hộp → thêm hộp, hết hộp → Clean); chống double-tap; ad/coin lỗi không mất state.
* **Hiện trạng:** mới ở mức comment + FX trong anim; `failOfferLevel = 10` không ai đọc.
* **Còn thiếu:** `BoosterManager` + state machine `Idle→Validating→Locked→Applying→Animating→Reconcile`; `WaitingAreaManager.TryAddSlot()` (check cap 7 + layout); temp-box cho Clean; `GetFirstBox()` đã có, chỉ cần gắn Magic Box + flag active; 2 nút revive trong popup Lose. Thứ tự: khung → AddSlot → Clean → Revive → Magic Box.

## CỤM 5 — TIẾN TRÌNH & TÀI NGUYÊN ❌

* **GDD yêu cầu:** thắng mở màn kế (Normal/Hard/VeryHard khó dần, mỗi màn có đường giải không booster); coin (khởi đầu 100), heart (max 5, hồi 1/1800s), Infinite Heart; daily 7 ngày (thường/x2 ad/bù lỡ, chống chỉnh giờ); số dư không âm, trừ thiếu hủy nguyên tử, 1 giao dịch không cộng 2 lần.
* **Hiện trạng:** tiến trình nằm RAM, tắt game mất; chưa có file save nào.
* **Còn thiếu:** `SaveStore` (1 JSON `PlayerProfile` + schema version + migration, ghi nguyên tử) — làm TRƯỚC vì là móng các cụm khác; `EconomyService.Grant/Spend` qua 1 cổng có `transactionId`; `DailyService` giờ UTC; chốt với sếp: trừ heart lúc Play hay lúc thua, hết heart thì sao, replay màn cũ được gì. (Demo full-client: bỏ x2/bù ad, xem cụm 7.)

## CỤM 6 — TUTORIAL ❌

* **GDD yêu cầu:** 3 màn đầu dạy tap → hộp/màu → chờ (+giữ/rotate sau, booster đúng lúc); bước phụ có Skip; xong không lặp; hỏng thì tự bỏ qua, không đè popup/ad.
* **Hiện trạng:** 1 marker data (`TutorialTargetMarker.cs`), chưa controller.
* **Còn thiếu:** `TutorialData` (ScriptableObject theo step `{trigger, target, text, skippable}`) + `TutorialController` đọc session state, chặn input sai step, cờ version trong save, timeout tự skip, test bằng event giả.

## CỤM 7 — SHOP & THƯỞNG NỘI TỆ (demo full-client, đã bỏ ads/IAP) ❌

* **GDD yêu cầu (bản rút gọn):** shop bán booster/heart bằng coin trong game; daily 7 ngày bản không ad; log event local làm bằng chứng QA.
* **Hiện trạng:** chưa có gì (đã bỏ scope: rewarded, interstitial, IAP, restore, receipt, remote-config offer/ad).
* **Còn thiếu:** catalog giá local + shop UI gọi `Economy` (fail hoàn nguyên); daily claim + streak; ghi event ra file local.

## CỤM 8 — CÀI ĐẶT, NGÔN NGỮ, ÂM THANH ❌

* **GDD yêu cầu:** toggle ngôn ngữ/nhạc/âm/rung độc lập, hiệu lực ngay, nhớ sau load; text key-based; cue riêng từng sự kiện, không chồng âm; nút ≥44pt, safe-area, không truyền luật bằng mỗi màu sắc.
* **Hiện trạng:** key localize bị comment; `KitSound` 0 nơi gọi; 2 hàm SFX rỗng; `PlayWarningLose()` rỗng.
* **Còn thiếu:** `SettingsState` trong save + event đổi settings; dọn key text; nối cue vào Tap/SlotFilled/BoxFull/Win/Lose (1 cue/100ms); màn Settings; kiểm tra màn nhỏ/font lớn.

## CỤM 9 — YÊU CẦU NỀN ◐

* **GDD yêu cầu:** 1 nguồn level chuẩn (thêm màn không sửa core, thiếu asset báo trước build); replay determinist (seed + log command); remote-config tối thiểu snapshot đầu ván.
* **Hiện trạng:** validator Editor mạnh (thay thế một phần LevelDefinition); chưa seed replay; chưa remote-config.
* **Còn thiếu:** `Random.InitState` theo `(levelOrder, attemptCount)`; chế độ replay đọc log; bảng config offer/reward đọc đầu session.

## CỤM 10 — NGHIỆM THU (việc test)

* **GDD yêu cầu:** 9 điều (qua màn 1 tự lực, không double/mất len, win/lose 1 lần, giữ tiến trình, booster rõ + an toàn, đa tỉ lệ, popup chặn xuyên, toggle hiệu lực, asset báo trước build; lỗi mạng/SDK không crash; offline chơi core).
* **Hiện trạng:** 1 PlayMode test popup; 16 EditMode tests của prototype cũ nằm ở evidence W1.
* **Còn thiếu:** checklist 9 điều thành case tay có bước + kết quả; regression cố định (pause/kill/relaunch/mất mạng/đổi ngày/màn nhỏ); chạy hết trước mỗi build nội bộ, ghi worklog.
