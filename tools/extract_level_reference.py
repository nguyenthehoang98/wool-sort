#!/usr/bin/env python3
"""Trích dữ liệu level (thứ tự chơi + độ khó) từ GeneralConfig của dự án gốc.

Đầu vào : GeneralConfig.asset của dự án gốc (Unity YAML)
Đầu ra : tools/level_reference.json

File JSON này là INPUT cho tool Unity `Tools/Level/Build Level Catalog`, nên
project Unity không phụ thuộc đường dẫn máy cụ thể — chạy tool này 1 lần khi
muốn cập nhật dữ liệu gốc, commit JSON lại.

Cấu trúc GeneralConfig.asset (reference):
    levels:                    # 300 entry  { id, levelModeType }
    - id: 1
      levelModeType: 0
    levelOrder:                # 88 entry   { index, levelId }
    - index: 1
      levelId: 3
"""
import argparse
import json
import os
import re
import sys

DEFAULT_REF = os.path.join(
    os.path.dirname(os.path.abspath(__file__)), "..", "..", "wool-sort (2)",
    "Assets", "Resources", "Configs", "GeneralConfig.asset")
DEFAULT_OUT = os.path.join(
    os.path.dirname(os.path.abspath(__file__)), "level_reference.json")

RE_LEVEL = re.compile(r"- id: (\d+)\s*\n\s*levelModeType: (\d+)")
RE_ORDER = re.compile(r"- index: (\d+)\s*\n\s*levelId: (\d+)")


def main():
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument("--src", default=DEFAULT_REF,
                    help="duong dan GeneralConfig.asset cua du an goc")
    ap.add_argument("--out", default=DEFAULT_OUT,
                    help="duong dan JSON output")
    args = ap.parse_args()

    src = os.path.abspath(args.src)
    if not os.path.exists(src):
        sys.exit(f"Khong tim thay GeneralConfig: {src}")

    with open(src, encoding="utf-8", errors="ignore") as fh:
        text = fh.read()

    levels = [{"id": int(i), "modeType": int(m)}
              for i, m in RE_LEVEL.findall(text)]
    order = [{"index": int(i), "levelId": int(l)}
             for i, l in RE_ORDER.findall(text)]

    if not levels:
        sys.exit("Khong doc duoc danh sach 'levels' — sai dinh dang file?")
    if not order:
        sys.exit("Khong doc duoc danh sach 'levelOrder' — sai dinh dang file?")

    mode_names = {0: "Normal", 1: "Hard", 2: "VeryHard"}
    data = {
        "source": "reference Assets/Resources/Configs/GeneralConfig.asset",
        "modeTypeNames": mode_names,
        "levels": levels,
        "order": order,
    }

    out = os.path.abspath(args.out)
    os.makedirs(os.path.dirname(out), exist_ok=True)
    with open(out, "w", encoding="utf-8") as fh:
        json.dump(data, fh, ensure_ascii=False, indent=1)
        fh.write("\n")

    ids = [l["levelId"] for l in order]
    print(f"da ghi {out}")
    print(f"  levels   : {len(levels)} (id {min(l['id'] for l in levels)}..{max(l['id'] for l in levels)})")
    print(f"  order    : {len(order)} (levelId {min(ids)}..{max(ids)}, trung: {len(ids) - len(set(ids))})")
    hard = sum(1 for l in levels if l["modeType"] == 1)
    vhard = sum(1 for l in levels if l["modeType"] == 2)
    print(f"  do kho   : Normal {len(levels) - hard - vhard}, Hard {hard}, VeryHard {vhard}")


if __name__ == "__main__":
    main()
