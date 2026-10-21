# -*- coding: utf-8 -*-
"""
reskin.py — ghi de hang loat anh trong Unity project (khong dung Unity).

Y tuong: thay the PNG bang cach COPY DE file (ghi de noi dung), KHONG xoa + tao
lai. Nho vay `.meta` giu nguyen -> GUID, spriteBorder (9-slice), PPU, texture
settings khong doi -> moi prefab/scene/material tro toi anh van dung.

Vi du:
    python tools/reskin.py --src D:/new_skin                # xem ke hoach (dry-run)
    python tools/reskin.py --src D:/new_skin --apply        # thuc thi
    python tools/reskin.py --src D:/new_skin --apply --mirror-booster
    python tools/reskin.py --src D:/new_skin --map map.csv --apply

map.csv (tuy chon) — moi dong: "duong_dan_anh_cu,file_anh_moi" de doi khi
ten file khac nhau. Duong dan anh cu la duong dan tuong doi trong `game/Assets`.
"""
import os
import re
import sys
import csv
import shutil
import struct
import argparse
import datetime

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(HERE)

IMG_EXT = (".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tif", ".tiff", ".webp", ".psd", ".exr", ".hdr")

# Thu muc trong Assets -> ban mirror trong Resources (can copy de khi reskin)
MIRRORS = [
    ("_Game/BoosterUI_Picked/", "_Game/Resources/BoosterUI/"),
]

ACTION_REPLACE = "REPLACE"
ACTION_NEW = "NEW"
ACTION_AMBIGUOUS = "AMBIGUOUS"
ACTION_NOT_FOUND = "NOT_FOUND"
ACTION_SAME = "SAME"


# ------------------------------------------------------------------ helpers
def is_image(path):
    return os.path.splitext(path)[1].lower() in IMG_EXT


def read_size(path):
    """Tra (w, h) cho PNG/JPEG, None neu khong doc duoc."""
    ext = os.path.splitext(path)[1].lower()
    try:
        if ext == ".png":
            d = open(path, "rb").read(26)
            if d[:8] != b"\x89PNG\r\n\x1a\n":
                return None
            w, h = struct.unpack(">II", d[16:24])
            return (w, h)
        if ext in (".jpg", ".jpeg"):
            f = open(path, "rb")
            if f.read(2) != b"\xff\xd8":
                f.close()
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
                    break
                m = marker[0]
                if m in (0xC0, 0xC1, 0xC2, 0xC3, 0xC5, 0xC6, 0xC7,
                         0xC9, 0xCA, 0xCB, 0xCD, 0xCE, 0xCF):
                    f.read(3)
                    h, w = struct.unpack(">HH", f.read(4))
                    f.close()
                    return (w, h)
                seg = struct.unpack(">H", f.read(2))[0]
                f.read(seg - 2)
            f.close()
    except Exception:
        return None
    return None


def read_sprite_border(meta_path):
    """Tra spriteBorder (x, y, z, w) tu .meta; None neu khong phai 9-slice."""
    if not os.path.exists(meta_path):
        return None
    try:
        t = open(meta_path, encoding="utf-8", errors="ignore").read()
    except Exception:
        return None
    m = re.search(r"^\s* spriteBorder:\s*\{([^}]*)\}", t, re.M)
    if not m:
        return None
    nums = re.findall(r"-?\d+", m.group(1))
    if len(nums) != 4:
        return None
    vals = tuple(int(n) for n in nums)
    return vals if any(vals) else None


def files_identical(a, b):
    try:
        if os.path.getsize(a) != os.path.getsize(b):
            return False
        with open(a, "rb") as fa, open(b, "rb") as fb:
            while True:
                ca, cb = fa.read(65536), fb.read(65536)
                if ca != cb:
                    return False
                if not ca:
                    return True
    except Exception:
        return False


# ------------------------------------------------------------------ index
def build_index(assets_root):
    """rel_path (duong dan tuong doi trong Assets) -> thong tin asset."""
    index = {}
    for dp, dn, fns in os.walk(assets_root):
        for fn in fns:
            if fn.endswith(".meta"):
                continue
            full = os.path.join(dp, fn)
            rel = os.path.relpath(full, assets_root).replace("\\", "/")
            index[rel] = {
                "full": full,
                "meta": full + ".meta",
                "size": read_size(full),
                "border": read_sprite_border(full + ".meta"),
            }
    return index


def build_name_map(index):
    """ten file (lower) -> danh sach rel_path."""
    m = {}
    for rel in index:
        m.setdefault(os.path.basename(rel).lower(), []).append(rel)
    return m


def collect_sources(src):
    """Danh sach file anh trong folder nguon -> rel_path trong folder nguon."""
    out = []
    if os.path.isfile(src):
        out.append((os.path.basename(src), src))
        return out
    for dp, dn, fns in os.walk(src):
        for fn in sorted(fns):
            if fn.endswith(".meta") or not is_image(fn):
                continue
            full = os.path.join(dp, fn)
            rel = os.path.relpath(full, src).replace("\\", "/")
            out.append((rel, full))
    return out


def load_map(path):
    """CSV: duong_dan_anh_cu,file_anh_moi -> dict."""
    m = {}
    if not path:
        return m
    with open(path, newline="", encoding="utf-8-sig") as f:
        for row in csv.reader(f):
            if not row or len(row) < 2:
                continue
            old = row[0].strip().replace("\\", "/")
            new = row[1].strip()
            if not old or old.startswith("#"):
                continue
            m[old] = new
    return m


# ------------------------------------------------------------------ plan
def build_plan(src, assets_root, name_map, csv_map, all_matches=False):
    index = build_index(assets_root)
    index = {k: v for k, v in index.items() if is_image(k)}
    by_name = build_name_map(index)
    plan = []

    def is_resources(rel):
        return "/Resources/" in "/" + rel

    for rel_src, full_src in collect_sources(src):
        targets = []          # danh sach rel_path se bi ghi de
        note = ""
        alt = []              # cac ban trung ten khac (de thong bao)

        # 1) khop chinh xac duong dan tuong doi trong Assets
        if rel_src in index:
            targets = [rel_src]
        # 2) khop qua CSV
        elif rel_src in csv_map:
            cand = csv_map[rel_src].replace("\\", "/")
            if cand in index:
                targets = [cand]
            else:
                cands = by_name.get(os.path.basename(cand).lower(), [])
                if len(cands) == 1:
                    targets = cands
                    note = "CSV → %s" % targets[0]
                else:
                    note = "CSV khong tim thay '%s'%s" % (
                        cand, " (gia nhieu noi)" if len(cands) > 1 else "")
        # 3) khop theo ten file
        else:
            cands = sorted(by_name.get(os.path.basename(rel_src).lower(), []))
            if len(cands) == 1:
                targets = cands
                note = "khop theo ten file"
            elif len(cands) > 1:
                # uu tien ban editable (khong nam trong Resources/) — ban do moi dung
                editable = [c for c in cands if not is_resources(c)]
                if all_matches:
                    targets = cands
                    note = "thay ca %d ban trung ten" % len(cands)
                elif len(editable) == 1:
                    targets = editable
                    alt = [c for c in cands if c != editable[0]]
                    note = "khop theo ten file (chon ban editable)"
                else:
                    plan.append({
                        "action": ACTION_AMBIGUOUS, "src": rel_src, "src_full": full_src,
                        "targets": [], "note": "trung ten %d noi: %s" % (len(cands), ", ".join(cands[:4])),
                    })
                    continue
            else:
                plan.append({
                    "action": ACTION_NOT_FOUND, "src": rel_src, "src_full": full_src,
                    "targets": [], "note": "khong co anh nao trong Assets trung ten",
                })
                continue

        # tach thanh nhieu muc de gan ke hoach
        for i, target_rel in enumerate(targets):
            info = index[target_rel]
            new_size = read_size(full_src)
            old_size = info["size"]

            if files_identical(full_src, info["full"]):
                plan.append({
                    "action": ACTION_SAME, "src": rel_src, "src_full": full_src,
                    "targets": [target_rel], "note": "noi dung giong het",
                })
                continue

            warn = []
            if info["border"] and old_size and new_size and old_size != new_size:
                warn.append("9-slice %s nhung kich thuoc %dx%d → %dx%d" % (
                    ",".join(str(x) for x in info["border"]), old_size[0], old_size[1],
                    new_size[0], new_size[1]))
            elif old_size and new_size and old_size != new_size:
                warn.append("kich thuoc %dx%d → %dx%d" % (old_size[0], old_size[1], new_size[0], new_size[1]))

            extra = ""
            if i == 0 and alt:
                extra = " | ban trung ten khac: %s" % ", ".join(alt)

            plan.append({
                "action": ACTION_REPLACE, "src": rel_src, "src_full": full_src,
                "targets": [target_rel],
                "note": "; ".join(filter(None, [note] + warn + [extra])),
                "old_size": old_size, "new_size": new_size,
                "border": info["border"],
            })

    return plan


# ------------------------------------------------------------------ apply
def apply_plan(plan, assets_root, backup_dir, mirror):
    replaced, skipped, failed = [], [], []

    for item in plan:
        if item["action"] != ACTION_REPLACE:
            skipped.append(item)
            continue

        src_full = item["src_full"]
        for target_rel in item["targets"]:
            target_full = os.path.join(assets_root, target_rel.replace("/", os.sep))

            # backup ban goc
            if backup_dir:
                bdir = os.path.join(backup_dir, os.path.dirname(target_rel))
                os.makedirs(bdir, exist_ok=True)
                shutil.copy2(target_full, os.path.join(bdir, os.path.basename(target_rel)))

            try:
                # ghi de noi dung, GIU NGUYEN .meta
                with open(src_full, "rb") as fs:
                    data = fs.read()
                with open(target_full, "wb") as ft:
                    ft.write(data)
                replaced.append({"src": item["src"], "target": target_rel, "note": item["note"]})
            except Exception as e:
                failed.append({"src": item["src"], "target": target_rel,
                               "note": "LOI: %s" % e})

            # mirror sang Resources
            if mirror:
                for src_prefix, dst_prefix in MIRRORS:
                    if target_rel.startswith(src_prefix):
                        name = os.path.basename(target_rel)
                        dst = os.path.join(assets_root, dst_prefix.replace("/", os.sep), name)
                        if os.path.exists(dst):
                            try:
                                shutil.copy2(src_full, dst)
                            except Exception as e:
                                failed.append({"src": item["src"], "target": dst_prefix + name,
                                               "note": "LOI mirror: %s" % e})
                        break

    return replaced, skipped, failed


# ------------------------------------------------------------------ main
def main():
    ap = argparse.ArgumentParser(
        description="Ghi de hang loat anh trong Unity project, giu nguyen .meta (GUID/9-slice/PPU).")
    ap.add_argument("--src", required=True, help="folder (hoac 1 file) chua anh moi")
    ap.add_argument("--assets", default=os.path.join(REPO, "game", "Assets"),
                    help="thu muc Assets (mac dinh: game/Assets)")
    ap.add_argument("--map", dest="csv_map", help="CSV: duong_dan_anh_cu,file_anh_moi")
    ap.add_argument("--apply", action="store_true", help="thuc thi (mac dinh chi xem ke hoach)")
    ap.add_argument("--mirror-booster", action="store_true",
                    help="copy them sang _Game/Resources/BoosterUI/ (ban runtime fallback)")
    ap.add_argument("--all-matches", action="store_true",
                    help="khi trung ten o nhieu noi thi thay ca tat ca (mac dinh: uu tien ban editable)")
    ap.add_argument("--no-backup", action="store_true", help="khong luu ban goc vao .reskin_backup/")
    args = ap.parse_args()

    if not os.path.exists(args.src):
        print("Khong tim thay: %s" % args.src)
        return 1
    if not os.path.isdir(args.assets):
        print("Khong tim thay thu muc Assets: %s" % args.assets)
        return 1

    name_map = None
    csv_map = load_map(args.csv_map)
    plan = build_plan(args.src, args.assets, name_map, csv_map, args.all_matches)

    replaces = [p for p in plan if p["action"] == ACTION_REPLACE]
    same = [p for p in plan if p["action"] == ACTION_SAME]
    ambiguous = [p for p in plan if p["action"] == ACTION_AMBIGUOUS]
    notfound = [p for p in plan if p["action"] == ACTION_NOT_FOUND]

    print("")
    print("=" * 78)
    print("KE HOACH RESKIN   nguon: %s" % args.src)
    print("=" * 78)
    print("  se thay the      : %d" % len(replaces))
    print("  giong het       : %d" % len(same))
    print("  trung ten       : %d" % len(ambiguous))
    print("  khong tim thay  : %d" % len(notfound))
    print("")

    if replaces:
        print("--- SE THAY THE ---")
        for p in replaces:
            old = p["old_size"]
            new = p["new_size"]
            dim = ""
            if old and new:
                dim = "  %dx%d -> %dx%d" % (old[0], old[1], new[0], new[1])
            print("  %s" % p["targets"][0])
            print("      tu: %s%s" % (p["src"], dim))
            if p["note"]:
                print("      %s" % p["note"])
        print("")

    if ambiguous:
        print("--- TRUNG TEN (can chon tay) ---")
        for p in ambiguous:
            print("  %s" % p["src"])
            print("      %s" % p["note"])
        print("")

    if notfound:
        print("--- KHONG TIM THAY ANH CU ---")
        for p in notfound:
            print("  %s" % p["src"])
            print("      %s" % p["note"])
        print("")

    if not args.apply:
        print(">>> DRY-RUN: chua thay gi ca. them --apply de thuc thi.")
        return 0

    backup_dir = None
    if not args.no_backup:
        stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
        # cung thu muc voi EditorWindow (BulkReskinWindow) de undo dung cho
        backup_dir = os.path.join(REPO, "game", ".reskin_backup", stamp)

    replaced, skipped, failed = apply_plan(plan, args.assets, backup_dir, args.mirror_booster)

    print("=" * 78)
    print("KET QUA")
    print("=" * 78)
    print("  da thay the : %d" % len(replaced))
    print("  bo qua     : %d" % len(skipped))
    print("  loi        : %d" % len(failed))
    if backup_dir:
        print("  ban goc    : %s" % os.path.relpath(backup_dir, REPO).replace("\\", "/"))
    print("")
    for p in replaced:
        print("  [OK] %s" % p["target"])
    for p in failed:
        print("  [LOI] %s — %s" % (p["src"], p["note"]))
    print("")
    print("Unity se tu reimport cac file da doi. Neu Unity dang mo, chay lai de thay het.")
    print("Neu anh 9-slice doi kich thuoc: kiem tra lai `spriteBorder` trong Inspector.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
