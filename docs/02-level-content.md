# F02 — Dữ liệu và pipeline màn chơi

## Cấu trúc hiện tại

- `Assets/Resources/Levels/Level{id}.prefab`: level runtime được load theo ID.
- `LevelYarnInfoHolder`: tổng số len, layer count, màu, random color pool, threshold spawn box.
- `YarnPart`: màu ngoài, số inner layer, màu inner fixed/random, decoration.
- `GeneralConfig`: danh sách level definition và `levelOrder` (thứ tự chơi → real asset ID).
- `Assets/Resources/Configs/Image/Image{id}.asset`: dữ liệu hình màu/preview của level.
- `Assets/_Game/Mesh/Level{id}.fbx`: nguồn mesh/scene content; có `LevelsBackUp` để backup prefab.

Level chơi đầu tiên là order `1` nhưng map tới real level `3`; không được dùng order và asset ID như cùng một khóa.

## Thiết kế level mới

Một level cần khai báo:

- `levelId`: khóa asset bất biến.
- `orderIndex`: vị trí trong campaign.
- `mode`: Normal, Hard hoặc VeryHard.
- `seed`: seed random inner/box nếu cần replay.
- `pieces`: danh sách root piece; mỗi piece có màu và chuỗi inner layer.
- `boxCount`, `initialUnlockedBoxes`, `boxSpawnRule`.
- `waitingSlotCount` nếu level muốn override mặc định.
- `thumbnail` và dữ liệu preview.
- `tutorialFlags` nếu level có onboarding.

## Quy tắc content

- Root piece là layer 1; inner layer tăng dần.
- Inner `Fixed` phải khai báo màu; inner `Random` phải tiêu thụ đúng color pool.
- Tổng màu của level phải đủ để các box hoàn thành; validator phải phát hiện màu thiếu/thừa.
- Không để reference tới asset backup trong build.
- Mỗi order phải map tới đúng một level asset tồn tại.
- Mỗi level phải có trạng thái thắng có thể chứng minh; không tạo deadlock không có booster nếu đó không phải chủ ý.

## Redesign pipeline

1. Designer sửa data level bằng Editor tool hoặc file schema.
2. Validator kiểm tra ID, màu, layer, box, thumbnail và khả năng load.
3. Build tạo `LevelDefinition` bất biến.
4. Runtime load definition trước, sau đó spawn visual.
5. Test snapshot level bằng seed và sequence input.

## Quyết định tối giản

Giữ ScriptableObject/prefab ở giai đoạn đầu để tận dụng asset hiện tại. Chưa chuyển sang server hoặc Addressables chỉ vì có nhiều level; chỉ chuyển khi profiling chứng minh thời gian load hoặc memory là vấn đề.
