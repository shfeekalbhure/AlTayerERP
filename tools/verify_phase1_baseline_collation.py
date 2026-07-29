from __future__ import annotations

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
SQL_PATH = ROOT / "Database/Baseline/Baseline_Phase1.sql"

if not SQL_PATH.exists():
    raise SystemExit("لم يتم العثور على سكربت Baseline المولد لفحص Collation.")

sql = SQL_PATH.read_text(encoding="utf-8-sig")

# COLLATE صالح فقط للأنواع النصية. نطابق الأنواع غير النصية كاملةً حتى لا
# نخلط بين int و bigint أو بين binary و varbinary.
non_text_type = r"(?:"
non_text_type += r"(?:tiny|small|medium|big)?int(?:\(\d+\))?|"
non_text_type += r"(?:decimal|numeric)\(\d+\s*,\s*\d+\)|"
non_text_type += r"(?:double|float)(?:\(\d+(?:\s*,\s*\d+)?\))?|"
non_text_type += r"(?:date|datetime|timestamp|time)(?:\(\d+\))?|"
non_text_type += r"(?:tiny|medium|long)?blob|blob|"
non_text_type += r"(?:var)?binary(?:\(\d+\))?|bit(?:\(\d+\))?"
non_text_type += r")"

pattern = re.compile(rf"\b{non_text_type}\b\s+COLLATE\b", re.IGNORECASE)
violations = []
for line_number, line in enumerate(sql.splitlines(), start=1):
    if pattern.search(line):
        violations.append(f"{line_number}: {line.strip()}")

if violations:
    print("فشل فحص Collation: عُثر على COLLATE بعد أنواع غير نصية:", file=sys.stderr)
    print("\n".join(violations), file=sys.stderr)
    raise SystemExit(1)

print("نجح فحص Collation: لا يوجد COLLATE بعد int/bigint/tinyint/datetime/decimal/blob/binary أو أي نوع غير نصي.")
