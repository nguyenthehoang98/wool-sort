# -*- coding: utf-8 -*-
"""Inventory image assets trong target project: dang dung / khong dung (de reskin)."""
import os, re, struct, json, collections

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(HERE)
ROOT = os.path.join(REPO, "game", "Assets")
OUT_MD = os.path.join(REPO, "docs", "asset-inventory-images.md")

IMG_EXT = (".png", ".jpg", ".jpeg", ".tga", ".psd", ".bmp", ".gif", ".exr", ".hdr", ".tif", ".tiff", ".webp", ".psb")
# file duoc Unity parse lam YAML/text
TEXT_EXT = (".unity", ".prefab", ".asset", ".mat", ".controller", ".anim", ".overridecontroller",
            ".playable", ".mask", ".spriteatlas", ".spriteatlasv2", ".shadervariants", ".inputactions",
            ".preset", ".physicmaterial", ".terrainlayer", ".guiskin", ".fontsettings", ".signal")
CODE_EXT = (".cs", ".shader", ".hlsl", ".cginc", ".compute")

# ---------------------------------------------------------------- build index
guid_of = {}          # guid -> rel path (asset, khong phai .meta)
path_of_guid = {}
meta_of_guid = {}
for dp, dn, fns in os.walk(ROOT):
    for fn in fns:
        if not fn.endswith(".meta"):
            continue
        mp = os.path.join(dp, fn)
        try:
            head = open(mp, "r", encoding="utf-8", errors="ignore").read(600)
        except Exception:
            continue
        g = re.search(r"^guid:\s*([0-9a-f]{32})", head, re.M)
        if not g:
            continue
        asset = os.path.join(dp, fn[:-5])
        rel = os.path.relpath(asset, ROOT).replace("\\", "/")
        guid = g.group(1)
        guid_of[guid] = rel
        path_of_guid[rel] = guid
        meta_of_guid[rel] = mp

def is_image(rel):
    return rel.lower().endswith(IMG_EXT)

# ---------------------------------------------------------------- refs
GUID_RE = re.compile(r"guid:\s*([0-9a-f]{32})")
refs_from = collections.defaultdict(set)   # rel -> set(guid referenced)
for rel, g in path_of_guid.items():
    ext = os.path.splitext(rel)[1].lower()
    if ext not in TEXT_EXT and ext not in CODE_EXT:
        continue
    p = os.path.join(ROOT, rel.replace("/", os.sep))
    try:
        t = open(p, "r", encoding="utf-8", errors="ignore").read()
    except Exception:
        continue
    for m in GUID_RE.findall(t):
        refs_from[rel].add(m)

# roots: scene + Resources/ + code
roots = set()
for rel in path_of_guid:
    ext = os.path.splitext(rel)[1].lower()
    if ext == ".unity" or "/Resources/" in "/" + rel or rel.startswith("Resources/"):
        roots.add(rel)
    elif ext in CODE_EXT:
        roots.add(rel)

# reachability
adj = {rel: set() for rel in path_of_guid}
for rel, gs in refs_from.items():
    for g in gs:
        tgt = guid_of.get(g)
        if tgt:
            adj[rel].add(tgt)

reach = set()
stack = [r for r in roots if r in adj]
while stack:
    cur = stack.pop()
    if cur in reach:
        continue
    reach.add(cur)
    for nxt in adj.get(cur, ()):
        if nxt not in reach:
            stack.append(nxt)

# ---------------------------------------------------------------- image info
def png_size(p):
    try:
        d = open(p, "rb").read(26)
        if d[:8] != b"\x89PNG\r\n\x1a\n":
            return None
        w, h = struct.unpack(">II", d[16:24])
        return (w, h)
    except Exception:
        return None

def jpg_size(p):
    try:
        f = open(p, "rb")
        if f.read(2) != b"\xff\xd8":
            return None
        f.read(2)
        while True:
            b = f.read(1)
            while b and b != b"\xff":
                b = f.read(1)
            marker = f.read(1)
            while marker == b"\xff":
                marker = f.read(1)
            if not marker:
                return None
            m = marker[0]
            if m in (0xC0, 0xC1, 0xC2, 0xC3, 0xC5, 0xC6, 0xC7, 0xC9, 0xCA, 0xCB, 0xCD, 0xCE, 0xCF):
                f.read(3)
                h, w = struct.unpack(">HH", f.read(4))
                return (w, h)
            seg = struct.unpack(">H", f.read(2))[0]
            f.read(seg - 2)
    except Exception:
        return None

def img_size(p):
    low = p.lower()
    if low.endswith(".jpg") or low.endswith(".jpeg"):
        return jpg_size(p)
    return png_size(p)

def meta_info(rel):
    mp = meta_of_guid.get(rel)
    if not mp:
        return {}
    try:
        t = open(mp, "r", encoding="utf-8", errors="ignore").read()
    except Exception:
        return {}
    out = {}
    m = re.search(r"^  spritePixelsToUnits:\s*([\d.]+)", t, re.M)
    if m:
        out["ppu"] = m.group(1)
    m = re.search(r"^  textureType:\s*(\d+)", t, re.M)
    if m:
        out["texType"] = m.group(1)
    m = re.search(r"^  spriteBorder:\s*\{([^}]*)\}", t, re.M)
    if m:
        nums = re.findall(r"[-]?\d+", m.group(1))
        if len(nums) == 4 and any(int(n) != 0 for n in nums):
            out["border"] = ",".join(nums)
    m = re.search(r"^\s*enableMipMap:\s*(\d+)", t, re.M)
    if m:
        out["mipmap"] = "có" if m.group(1) == "1" else "không"
    m = re.search(r"^  alphaIsTransparency:\s*(\d+)", t, re.M)
    if m:
        out["alpha"] = m.group(1)
    m = re.search(r"^  textureFormat:\s*(-?\d+)", t, re.M)
    if m:
        out["fmt"] = m.group(1)
    m = re.search(r"^  maxTextureSize:\s*(\d+)", t, re.M)
    if m:
        out["maxSize"] = m.group(1)
    return out

# Unity TextureImporterType: 0 Default, 1 NormalMap, 2 GUI, 3 Sprite, 4 Cursor,
# 5 Cookie, 6 Lightmap, 7 SingleChannel, 8 Sprite (Unity 2020+)
TEX_TYPE = {"0": "Default", "1": "NormalMap", "2": "GUI", "3": "Sprite", "4": "Cursor",
            "5": "Cookie", "6": "Lightmap", "7": "SingleChannel", "8": "Sprite"}

# who references an asset (direct)
used_by = collections.defaultdict(set)
for rel, gs in refs_from.items():
    for g in gs:
        tgt = guid_of.get(g)
        if tgt and tgt != rel:
            used_by[tgt].add(rel)

images = [r for r in path_of_guid if is_image(r)]
used, unused = [], []
for rel in sorted(images):
    full = os.path.join(ROOT, rel.replace("/", os.sep))
    sz = img_size(full)
    info = meta_info(rel)
    row = {
        "path": rel,
        "size": ("%dx%d" % sz) if sz else "-",
        "ppu": info.get("ppu", "-"),
        "type": TEX_TYPE.get(info.get("texType", ""), info.get("texType", "-")),
        "mipmap": info.get("mipmap", "-"),
        "border": info.get("border", "-"),
        "usedby": sorted(used_by.get(rel, [])),
        "inResources": "/Resources/" in "/" + rel,
    }
    (used if rel in reach else unused).append(row)

PLUGIN_PREFIXES = ("Demigiant/", "Plugins/", "Lana Studio/", "TextMesh Pro/Editor")
GAME_PREFIXES = ("_Game/", "TextMesh Pro/Sprites/")

def group_of(rel):
    if rel.startswith("_Game/Textures/UIV2/") or rel.startswith("_Game/Textures/UI/"):
        return "UI"
    if rel.startswith("_Game/BoosterUI_Picked/") or "/Resources/BoosterUI/" in rel:
        return "Booster"
    if rel.startswith("_Game/Resources/Shop/"):
        return "Shop"
    if rel.startswith("_Game/Resources/WinLoseUI/"):
        return "WinLose"
    if rel.startswith("_Game/Textures/"):
        return "Gameplay/Material"
    if rel.startswith("_Game/_HomeFrame/"):
        return "Home"
    if rel.startswith("TextMesh Pro/"):
        return "TMP"
    if rel.startswith("Lana Studio/"):
        return "VFX"
    return "Khác"

def is_game(rel):
    return rel.startswith(GAME_PREFIXES)

def reason_of(r):
    if r["inResources"]:
        if "/Resources/BoosterUI/" in r["path"]:
            return "Fallback runtime: `Resources.Load(\"BoosterUI/…\")` khi field Inspector trống"
        return "Trong `Resources/` → load runtime theo path"
    if r["usedby"]:
        return "Chỉ được trỏ bởi asset không dùng: " + ", ".join(r["usedby"][:3])
    return "Không asset nào trỏ tới"

# ---------------------------------------------------------------- markdown
def esc(s):
    return s.replace("|", "\\|")

g_used = [r for r in used if is_game(r["path"])]
g_unused = [r for r in unused if is_game(r["path"])]
p_unused = [r for r in unused if not is_game(r["path"])]

lines = []
lines.append("# Inventory ảnh — target project (`game/Assets`)")
lines.append("")
lines.append("Sinh tự động để **reskin**. Mỗi ảnh có: kích thước, PPU, kiểu texture, và **dùng ở đâu**.")
lines.append("")
lines.append("Quy tắc phân loại:")
lines.append("")
lines.append("- **Đang dùng** = reachable từ scene / prefab gốc / material / code (đi theo đồ thị reference).")
lines.append("- Ảnh trong thư mục `Resources/` được load runtime theo path → coi là đang dùng dù không có reference trực tiếp.")
lines.append("- **Không dùng** = không reachable từ bất kỳ gốc nào (kể cả asset chỉ trỏ tới nó cũng không dùng).")
lines.append("")
lines.append("| Nhóm | Tổng ảnh game | Đang dùng | Không dùng |")
lines.append("|---|---|---|---|")
lines.append("| Ảnh game (`_Game/`, TMP) | %d | %d | %d |" % (len(g_used) + len(g_unused), len(g_used), len(g_unused)))
lines.append("| Ảnh plugin/editor (không reskin) | %d | %d | %d |" % (
    len([r for r in used if not is_game(r["path"])]) + len(p_unused),
    len([r for r in used if not is_game(r["path"])]), len(p_unused)))
lines.append("")

lines.append("## 1. Ảnh game ĐANG DÙNG (%d)" % len(g_used))
lines.append("")
lines.append("Cột **9-slice** = `spriteBorder` (x,y,z,w) trong `.meta`. Không 0 → ảnh bị cắt viền, đổi PNG phải giữ nguyên.")
lines.append("")
lines.append("| # | Path | Nhóm | Kích thước | PPU | Kiểu | 9-slice | Mipmap | Dùng ở |")
lines.append("|---|---|---|---|---|---|---|---|---|")
for i, r in enumerate(g_used, 1):
    ub = "<br>".join("`%s`" % esc(x) for x in r["usedby"]) or ("*(Resources — load theo path)*" if r["inResources"] else "*(root)*")
    lines.append("| %d | `%s` | %s | %s | %s | %s | %s | %s | %s |" % (
        i, esc(r["path"]), group_of(r["path"]), r["size"], r["ppu"], r["type"],
        r["border"], r["mipmap"], ub))
lines.append("")

lines.append("## 2. Ảnh game KHÔNG DÙNG (%d)" % len(g_unused))
lines.append("")
lines.append("> Xoá được (nếu không tính tới việc dùng lại sau này). Kiểm tra kỹ trước khi xoá — xem cột *Lý do*.")
lines.append("")
lines.append("| # | Path | Nhóm | Kích thước | PPU | Kiểu | 9-slice | Lý do |")
lines.append("|---|---|---|---|---|---|---|---|")
for i, r in enumerate(g_unused, 1):
    lines.append("| %d | `%s` | %s | %s | %s | %s | %s | %s |" % (
        i, esc(r["path"]), group_of(r["path"]), r["size"], r["ppu"], r["type"],
        r["border"], esc(reason_of(r))))
lines.append("")

lines.append("## 3. Ảnh plugin/editor — không reskin (%d không dùng)" % len(p_unused))
lines.append("")
lines.append("> Icon của DOTween / DemiLib / Dreamteck Splines. Không liên quan tới game, đừng đụng vào.")
lines.append("")
lines.append("| # | Path | Kích thước |")
lines.append("|---|---|---|")
for i, r in enumerate(p_unused, 1):
    lines.append("| %d | `%s` | %s |" % (i, esc(r["path"]), r["size"]))
lines.append("")

lines.append("## Ghi chú khi reskin")
lines.append("")
lines.append("1. **`_Game/BoosterUI_Picked/`** là bản để sửa. Sau khi đổi PNG ở đây phải **copy đè** sang")
lines.append("   `_Game/Resources/BoosterUI/` (giữ nguyên tên file) vì đó là bản runtime fallback")
lines.append("   (`BoosterBarUI.ResolveSprites()` → `Resources.Load(\"BoosterUI/…\")`).")
lines.append("2. **PPU = 100** cho toàn bộ ảnh UI → 1 pixel = 0.01 unit. Đổi kích thước PNG mà giữ PPU")
lines.append("   sẽ làm UI phình to/nhỏ lại; nếu muốn giữ nguyên kích thước trên màn hình thì giữ nguyên pixel.")
lines.append("3. Ảnh 9-slice (`Frame_Bar_1/2/3`, `Frame_Popup_1/2`, `Button_Green`, `Button_Close`, `slot2`, `btn2Green`)")
lines.append("   có `spriteBorder` trong `.meta` — đổi PNG phải giữ nguyên `spriteBorder`, nếu không sẽ méo.")
lines.append("4. `TextMesh Pro/Sprites/EmojiOne.png` là sprite sheet của TMP — không reskin tự do.")
lines.append("5. Bộ texture gỗ (`text_diffuse.jpg`, `text_normal.png`, `text_roughness_1/2.jpg`) chỉ được 3")
lines.append("   material `WoodMat_Gray/White/Yellow.mat` trỏ tới, mà 3 material đó **không asset nào dùng** →")
lines.append("   cả bộ đều có thể xoá cùng nhau.")
lines.append("")

os.makedirs(os.path.dirname(OUT_MD), exist_ok=True)
open(OUT_MD, "w", encoding="utf-8", newline="\n").write("\n".join(lines))
print("wrote", OUT_MD)
print("images=%d | game used=%d game unused=%d | plugin unused=%d" % (
    len(images), len(g_used), len(g_unused), len(p_unused)))
print()
print("--- ANH GAME KHONG DUNG ---")
for r in g_unused:
    print("  %-58s %-10s %s" % (r["path"], r["size"], reason_of(r)[:70]))

