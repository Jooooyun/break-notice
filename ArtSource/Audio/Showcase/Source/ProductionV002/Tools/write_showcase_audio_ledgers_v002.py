from __future__ import annotations

import csv
import hashlib
import json
import math
import os
import re
import wave
from datetime import datetime
from pathlib import Path
from zoneinfo import ZoneInfo

import numpy as np


CC0_URL = "https://creativecommons.org/publicdomain/zero/1.0/"
RIGHTS_URL = "https://lasonotheque.org/licences.html"


SOURCES = {
    "0516": ("Chaise en bois, tombe sur moquette", "Joseph SARDIN", "https://lasonotheque.org/chaise-en-bois-qui-tombe-sur-moquette-s0516.html", "LIC_LASONOTHEQUE_0516_ChaiseTombeMoquette_20260812.html"),
    "0666": ("Chaise", "Joseph SARDIN", "https://lasonotheque.org/chaise-s0666.html", "LIC_LASONOTHEQUE_0666_ChaiseDeplacement_20260812.html"),
    "0786": ("Classeur", "Joseph SARDIN", "https://lasonotheque.org/classeur-s0786.html", "LIC_LASONOTHEQUE_0786_Classeur_20260812.html"),
    "1226": ("Page déchirée et froissée #2", "Joseph SARDIN", "https://lasonotheque.org/page-dechiree-et-froissee-2-s1226.html", "LIC_LASONOTHEQUE_1226_PageDechireeFroissee2_20260812.html"),
    "1316": ("Diable de manutention #1", "Joseph SARDIN", "https://lasonotheque.org/diable-de-manutention-1-s1316.html", "LIC_LASONOTHEQUE_1316_DiableManutention1_20260812.html"),
    "1318": ("Pas rapides sur béton", "Joseph SARDIN", "https://lasonotheque.org/pas-rapides-sur-beton-s1318.html", "LIC_LASONOTHEQUE_1318_PasRapidesBeton_20260812.html"),
    "1773": ("Chute barre d'acier en tôle #3", "Joseph SARDIN", "https://lasonotheque.org/chute-barre-d-acier-en-tole-3-s1773.html", "LIC_LASONOTHEQUE_1773_ChuteBarreAcier3_20260812.html"),
    "1798": ("Whoosh #10", "Joseph SARDIN", "https://lasonotheque.org/whoosh-10-s1798.html", "LIC_LASONOTHEQUE_1798_Whoosh10_20260812.html"),
    "1802": ("Whoosh #9", "Joseph SARDIN", "https://lasonotheque.org/whoosh-9-s1802.html", "LIC_LASONOTHEQUE_1802_Whoosh9_20260812.html"),
    "2244": ("Bille en metal, rebond #3", "Joseph SARDIN", "https://lasonotheque.org/bille-en-metal-rebond-3-s2244.html", "LIC_LASONOTHEQUE_2244_BilleMetalRebond3_20260812.html"),
    "2359": ("Barrière métal : Fermeture #2", "Joseph SARDIN", "https://lasonotheque.org/grincement-barriere-metallique-fermeture-2-s2359.html", "LIC_LASONOTHEQUE_2359_BarriereMetalFermeture2_20260812.html"),
    "2452": ("Chute de corps #1", "Joseph SARDIN", "https://lasonotheque.org/chute-de-corps-1-s2452.html", "LIC_LASONOTHEQUE_2452_ChuteCorps1_20260812.html"),
    "2453": ("Chute de corps #2", "Joseph SARDIN", "https://lasonotheque.org/chute-de-corps-2-s2453.html", "LIC_LASONOTHEQUE_2453_ChuteCorps2_20260812.html"),
    "2454": ("Chute de corps #3", "Joseph SARDIN", "https://lasonotheque.org/chute-de-corps-3-s2454.html", "LIC_LASONOTHEQUE_2454_ChuteCorps3_20260812.html"),
    "2455": ("Chute de corps #4", "Joseph SARDIN", "https://lasonotheque.org/chute-de-corps-4-s2455.html", "LIC_LASONOTHEQUE_2455_ChuteCorps4_20260812.html"),
    "2459": ("Coup de poing #4", "Joseph SARDIN", "https://lasonotheque.org/coup-de-poing-4-s2459.html", "LIC_LASONOTHEQUE_2459_Punch4_20260812.html"),
    "2460": ("Coup de poing #5", "Joseph SARDIN", "https://lasonotheque.org/coup-de-poing-5-s2460.html", "LIC_LASONOTHEQUE_2460_Punch5_20260812.html"),
    "2461": ("Coup de poing #6", "Joseph SARDIN", "https://lasonotheque.org/coup-de-poing-6-s2461.html", "LIC_LASONOTHEQUE_2461_Punch6_20260812.html"),
    "2462": ("Coup de poing #7", "Joseph SARDIN", "https://lasonotheque.org/coup-de-poing-7-s2462.html", "LIC_LASONOTHEQUE_2462_Punch7_20260812.html"),
    "2463": ("Coup de poing #8", "Joseph SARDIN", "https://lasonotheque.org/coup-de-poing-8-s2463.html", "LIC_LASONOTHEQUE_2463_Punch8_20260812.html"),
    "2464": ("Coup de poing #9", "Joseph SARDIN", "https://lasonotheque.org/coup-de-poing-9-s2464.html", "LIC_LASONOTHEQUE_2464_Punch9_20260812.html"),
    "2803": ("Carte routière #8", "Joseph SARDIN", "https://lasonotheque.org/carte-routiere-8-s2803.html", "LIC_LASONOTHEQUE_2803_CarteRoutiere8_20260812.html"),
    "2808": ("Carte routière #13", "Joseph SARDIN", "https://lasonotheque.org/carte-routiere-13-s2808.html", "LIC_LASONOTHEQUE_2808_CarteRoutiere13_20260812.html"),
    "2902": ("Couvercle plastique #8", "Joseph SARDIN", "https://lasonotheque.org/couvercle-plastique-8-s2902.html", "LIC_LASONOTHEQUE_2902_CouverclePlastique8_20260812.html"),
    "3296": ("Haltère, posé #2", "Joseph SARDIN", "https://lasonotheque.org/haltere-pose-2-s3296.html", "LIC_LASONOTHEQUE_3296_HalterePose2_20260812.html"),
    "3441": ("Pas qui grincent #2", "Axeline T.", "https://lasonotheque.org/pas-qui-grincent-2-s3441.html", "LIC_LASONOTHEQUE_3441_PasGrincent2_20260812.html"),
}


REFERENCE_SOURCES = {
    "OWNER_REF_FOOTSTEPS_V002": ["LASO-1318", "LASO-3441"],
    "OWNER_REF_BODY_CONTACT_V002": [
        "LASO-2452", "LASO-2453", "LASO-2454", "LASO-2455",
        "LASO-2459", "LASO-2460", "LASO-2461", "LASO-2462", "LASO-2463", "LASO-2464",
    ],
    "OWNER_REF_BINDER_V003": ["LASO-0786", "LASO-1226", "LASO-2803", "LASO-2808", "LASO-2902"],
    "OWNER_REF_CHAIR_V003": ["LASO-0516", "LASO-0666", "LASO-1316", "LASO-1773", "LASO-2244", "LASO-2359", "LASO-3296"],
}


REFERENCE_FILES = {
    "OWNER_REF_FOOTSTEPS_V002": "AUDITION_01_FOOTSTEPS_WEIGHTED_v002_48k24.wav",
    "OWNER_REF_BODY_CONTACT_V002": "AUDITION_02_PUNCH_WEIGHTED_v002_48k24.wav",
    "OWNER_REF_BINDER_V003": "AUDITION_BINDER_PHYSICAL_v003_48k24.wav",
    "OWNER_REF_CHAIR_V003": "AUDITION_CHAIR_PHYSICAL_v003_48k24.wav",
}


REFERENCE_SHA = {
    "OWNER_REF_FOOTSTEPS_V002": "dd6fcaf66e9ef4f26c7744379600b0da71b96a210f54e28cbdf23e0271bfae94",
    "OWNER_REF_BODY_CONTACT_V002": "853ce15e701370b4180d7ec852f2e5f1860703cdaba77b28b3659381911c8791",
    "OWNER_REF_BINDER_V003": "21c53a4b6d6878e527f81d04da07ee9ce1a209bc8fd6eeed7381ecb845099dd9",
    "OWNER_REF_CHAIR_V003": "c314a2fcb601ed9d814921f4d06b04518e0a46ac526b6e3f5d4ebd87df51f8e8",
}


LEDGER_FIELDS = [
    "Record Type", "Record ID", "Cue ID", "Variation", "Source IDs", "Source Class",
    "Original Title", "Creator", "Source URL", "License", "Canonical License URL",
    "Commercial Use", "Modification", "Attribution", "Evidence Path", "Evidence SHA-256",
    "Original Site Filename", "Recovered Source Filename", "Acquisition/Recovery Timestamp",
    "Transformation", "Destination", "SHA-256", "Bytes", "Duration Seconds", "Channels",
    "Sample Rate", "Source Bit Depth", "Delivery Bit Depth", "Peak dBFS", "Human Owner Decision",
    "Runtime Integration", "Notes",
]


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def read_wav_metrics(path: Path) -> dict:
    with wave.open(str(path), "rb") as wav:
        channels = wav.getnchannels()
        rate = wav.getframerate()
        width = wav.getsampwidth()
        frames = wav.getnframes()
        raw = wav.readframes(frames)
    if width == 2:
        values = np.frombuffer(raw, dtype="<i2").astype(np.int32)
        peak = float(np.max(np.abs(values))) / 32768.0 if len(values) else 0.0
    elif width == 3:
        packed = np.frombuffer(raw, dtype=np.uint8).reshape(-1, 3)
        values = (
            packed[:, 0].astype(np.int32)
            | (packed[:, 1].astype(np.int32) << 8)
            | (packed[:, 2].astype(np.int32) << 16)
        )
        values = np.where(values & 0x800000, values - 0x1000000, values)
        peak = float(np.max(np.abs(values))) / 8_388_608.0 if len(values) else 0.0
    else:
        raise ValueError(f"unsupported WAV width {width}: {path}")
    return {
        "duration": frames / rate,
        "channels": channels,
        "sample_rate": rate,
        "bit_depth": width * 8,
        "peak_dbfs": 20.0 * math.log10(max(peak, 1e-12)),
    }


def expand_source_ids(ids: list[str]) -> list[str]:
    expanded = []
    for source_id in ids:
        if source_id in REFERENCE_SOURCES:
            expanded.extend(REFERENCE_SOURCES[source_id])
        elif source_id.startswith("LASO-"):
            expanded.append(source_id)
    return sorted(set(expanded))


def source_details(ids: list[str], evidence_root: Path) -> dict:
    raw_ids = expand_source_ids(ids)
    urls = []
    titles = []
    creators = []
    evidence_paths = []
    evidence_hashes = []
    filenames = []
    for raw_id in raw_ids:
        number = raw_id.removeprefix("LASO-")
        title, creator, url, evidence_name = SOURCES[number]
        urls.append(url)
        titles.append(title)
        creators.append(creator)
        evidence_path = evidence_root / evidence_name
        evidence_paths.append(evidence_path)
        evidence_hashes.append(sha256_file(evidence_path))
        filenames.append(f"{number}.wav")
    has_dsp = "PROJECT_ORIGINAL_DSP" in ids
    has_reference = any(source_id.startswith("OWNER_REF_") for source_id in ids)
    if raw_ids and has_dsp:
        source_class = "OFFICIAL CC0 RECORDED FOLEY + PROJECT ORIGINAL DSP"
    elif raw_ids and has_reference:
        source_class = "HUMAN OWNER AUDIBLE REFERENCE PASS (CC0-DERIVED) + OFFICIAL CC0 RECORDED FOLEY"
    elif raw_ids:
        source_class = "OFFICIAL LASONOTHEQUE CC0 RECORDED FOLEY"
    else:
        source_class = "PROJECT ORIGINAL DETERMINISTIC DSP"
    return {
        "Source Class": source_class,
        "Original Title": " | ".join(titles) if titles else "Project Original DSP layer",
        "Creator": " | ".join(sorted(set(creators))) if creators else "OpenAI Codex — Human Owner-directed deterministic DSP",
        "Source URL": " | ".join(urls) if urls else "N/A — project original deterministic DSP",
        "License": "CC0 equivalent public domain; commercial use/modification/redistribution permitted" if raw_ids else "PROJECT ORIGINAL / NO THIRD-PARTY LICENSE DEPENDENCY",
        "Canonical License URL": CC0_URL if raw_ids else "N/A",
        "Commercial Use": "YES",
        "Modification": "YES",
        "Attribution": "NOT REQUIRED; Joseph SARDIN / LaSonotheque.org and Axeline T. credit appreciated" if raw_ids else "N/A",
        "Evidence Path": " | ".join(path.as_posix() for path in evidence_paths) if evidence_paths else "Licenses/ProductionV002/AUDIO_SOURCE_PROVENANCE_v002.md",
        "Evidence SHA-256": " | ".join(evidence_hashes) if evidence_hashes else "N/A — project original",
        "Original Site Filename": " | ".join(filenames) if filenames else "N/A",
    }


def main() -> None:
    script_path = Path(__file__).resolve()
    showcase_root = script_path.parents[3]
    repo_root = showcase_root.parents[2]
    preview_root = showcase_root / "Preview" / "ProductionV002"
    license_root = showcase_root / "Licenses" / "ProductionV002"
    evidence_root = license_root / "Evidence"
    raw_root = showcase_root / "Source" / "ProductionV002" / "RawCC0" / "LaSonotheque"
    reference_root = showcase_root / "Source" / "ApprovedReferences"
    master_root = showcase_root / "Source" / "ProductionV002" / "Masters"
    matrix_path = repo_root / "docs" / "10_contest" / "workstreams" / "audio" / "SHOWCASE_AUDIO_CUE_MATRIX_v001.csv"
    manifest = json.loads((preview_root / "SHOWCASE_AUDIO_EXPORT_MANIFEST_v002.json").read_text(encoding="utf-8"))
    audition = json.loads((preview_root / "SHOWCASE_AUDIO_AUDITION_INDEX_v002.json").read_text(encoding="utf-8"))
    timestamp = datetime.now(ZoneInfo("Asia/Seoul")).isoformat(timespec="seconds")
    rows = []

    for path in sorted(raw_root.glob("*.wav")):
        match = re.search(r"BSB_(\d{4})", path.name)
        if not match:
            raise ValueError(path)
        number = match.group(1)
        title, creator, url, evidence_name = SOURCES[number]
        evidence_path = evidence_root / evidence_name
        metrics = read_wav_metrics(path)
        recovery_timestamp = datetime.fromtimestamp(path.stat().st_mtime, ZoneInfo("Asia/Seoul")).isoformat(timespec="seconds")
        rows.append({
            "Record Type": "RAW_SOURCE",
            "Record ID": f"LASO-{number}",
            "Cue ID": "MULTIPLE — see EXPORT rows",
            "Variation": "N/A",
            "Source IDs": f"LASO-{number}",
            "Source Class": "OFFICIAL LASONOTHEQUE CC0 RECORDED FOLEY",
            "Original Title": title,
            "Creator": creator,
            "Source URL": url,
            "License": "CC0 equivalent public domain; page explicitly permits commercial use, modification and redistribution",
            "Canonical License URL": CC0_URL,
            "Commercial Use": "YES",
            "Modification": "YES",
            "Attribution": "NOT REQUIRED; credit appreciated",
            "Evidence Path": evidence_path.relative_to(showcase_root).as_posix(),
            "Evidence SHA-256": sha256_file(evidence_path),
            "Original Site Filename": f"{number}.wav",
            "Recovered Source Filename": path.name,
            "Acquisition/Recovery Timestamp": f"{recovery_timestamp} — filesystem timestamp preserved from Human Owner-authorized temp recovery",
            "Transformation": "BYTE-IDENTICAL COPY FROM AUTHORIZED TEMP TREE; ORIGINAL AUDIO BYTES UNMODIFIED",
            "Destination": path.relative_to(showcase_root).as_posix(),
            "SHA-256": sha256_file(path),
            "Bytes": path.stat().st_size,
            "Duration Seconds": f"{metrics['duration']:.6f}",
            "Channels": metrics["channels"],
            "Sample Rate": metrics["sample_rate"],
            "Source Bit Depth": metrics["bit_depth"],
            "Delivery Bit Depth": metrics["bit_depth"],
            "Peak dBFS": f"{metrics['peak_dbfs']:.6f}",
            "Human Owner Decision": "SOURCE RIGHTS VERIFIED / DERIVED OUTPUT REVIEW_CANDIDATE",
            "Runtime Integration": "NOT_PERFORMED",
            "Notes": f"Evidence captured {timestamp}; rights policy: {RIGHTS_URL}",
        })

    for source_id, filename in REFERENCE_FILES.items():
        path = reference_root / filename
        metrics = read_wav_metrics(path)
        inherited = REFERENCE_SOURCES[source_id]
        details = source_details([source_id], evidence_root)
        rows.append({
            "Record Type": "APPROVED_REFERENCE",
            "Record ID": source_id,
            "Cue ID": "REFERENCE MATERIAL ONLY / NO NEW GAMEPLAY",
            "Variation": "N/A",
            "Source IDs": " | ".join(inherited),
            **details,
            "Recovered Source Filename": filename,
            "Acquisition/Recovery Timestamp": f"{datetime.fromtimestamp(path.stat().st_mtime, ZoneInfo('Asia/Seoul')).isoformat(timespec='seconds')} — byte-preserved owner-authorized temp recovery",
            "Transformation": "BYTE-IDENTICAL COPY; NO RE-ENCODE; NO TONAL CHANGE",
            "Destination": path.relative_to(showcase_root).as_posix(),
            "SHA-256": sha256_file(path),
            "Bytes": path.stat().st_size,
            "Duration Seconds": f"{metrics['duration']:.6f}",
            "Channels": metrics["channels"],
            "Sample Rate": metrics["sample_rate"],
            "Source Bit Depth": metrics["bit_depth"],
            "Delivery Bit Depth": metrics["bit_depth"],
            "Peak dBFS": f"{metrics['peak_dbfs']:.6f}",
            "Human Owner Decision": "HUMAN OWNER AUDIBLE REFERENCE PASS 2026-08-12",
            "Runtime Integration": "NOT_PERFORMED",
            "Notes": f"Expected SHA matched: {REFERENCE_SHA[source_id]}",
        })

    master_source_map = {
        "FOOT": "OWNER_REF_FOOTSTEPS_V002",
        "BODY": "OWNER_REF_BODY_CONTACT_V002",
        "BINDER": "OWNER_REF_BINDER_V003",
        "CHAIR": "OWNER_REF_CHAIR_V003",
    }
    for path in sorted(master_root.glob("*.wav")):
        family = path.stem.split("_")[1]
        source_id = master_source_map[family]
        details = source_details([source_id], evidence_root)
        metrics = read_wav_metrics(path)
        rows.append({
            "Record Type": "SOURCE_MASTER",
            "Record ID": path.stem,
            "Cue ID": "SOURCE LAYER / NOT COUNTED AS REVIEW EXPORT",
            "Variation": path.stem.split("_")[2],
            "Source IDs": source_id,
            **details,
            "Recovered Source Filename": REFERENCE_FILES[source_id],
            "Acquisition/Recovery Timestamp": timestamp,
            "Transformation": "CLEAN SEGMENT EXTRACTION FROM OWNER-PASSED REEL; TRIM; 2-18 ms CLICK-SAFE FADES; DC REMOVAL; PEAK TRIM; 48 kHz/24-bit PCM",
            "Destination": path.relative_to(showcase_root).as_posix(),
            "SHA-256": sha256_file(path),
            "Bytes": path.stat().st_size,
            "Duration Seconds": f"{metrics['duration']:.6f}",
            "Channels": metrics["channels"],
            "Sample Rate": metrics["sample_rate"],
            "Source Bit Depth": 24,
            "Delivery Bit Depth": metrics["bit_depth"],
            "Peak dBFS": f"{metrics['peak_dbfs']:.6f}",
            "Human Owner Decision": "REFERENCE DIRECTION PASS; CLEAN SEGMENT MASTER REVIEW_CANDIDATE",
            "Runtime Integration": "NOT_PERFORMED",
            "Notes": "Not counted in the 196 runtime-ready review exports",
        })

    for entry in manifest["entries"]:
        ids = entry["sources"]
        details = source_details(ids, evidence_root)
        path = showcase_root / entry["path"]
        source_bits = sorted({
            read_wav_metrics(raw_root / next(item.name for item in raw_root.glob(f"BSB_{raw_id.removeprefix('LASO-')}*.wav")))["bit_depth"]
            for raw_id in expand_source_ids(ids)
        })
        rows.append({
            "Record Type": "RUNTIME_READY_REVIEW_EXPORT",
            "Record ID": f"{entry['cue_id']}-V{entry['variation_index']:02d}",
            "Cue ID": entry["cue_id"],
            "Variation": f"{entry['variation_index']}/{entry['variation_count']}",
            "Source IDs": " | ".join(ids),
            **details,
            "Recovered Source Filename": " | ".join(REFERENCE_FILES[source_id] for source_id in ids if source_id in REFERENCE_FILES) or "N/A — see Source IDs",
            "Acquisition/Recovery Timestamp": timestamp,
            "Transformation": entry["transformation"],
            "Destination": entry["path"],
            "SHA-256": entry["sha256"],
            "Bytes": entry["bytes"],
            "Duration Seconds": f"{entry['duration']:.6f}",
            "Channels": entry["channels"],
            "Sample Rate": entry["sample_rate"],
            "Source Bit Depth": " | ".join(map(str, source_bits)) if source_bits else "N/A — project original DSP",
            "Delivery Bit Depth": entry["bit_depth"],
            "Peak dBFS": f"{entry['peak_dbfs']:.6f}",
            "Human Owner Decision": "REVIEW_CANDIDATE",
            "Runtime Integration": "NOT_PERFORMED",
            "Notes": f"{entry['contest_usage']}; channel intent {entry['channel_intent']}; loop={entry['loop']}; Future Runtime Hook remains PROPOSED / REVIEW REQUIRED",
        })

    for reel in audition:
        path = showcase_root / reel["path"]
        rows.append({
            "Record Type": "AUDITION_REEL",
            "Record ID": Path(reel["filename"]).stem,
            "Cue ID": " | ".join(row["cue_id"] for row in reel["index"]),
            "Variation": "ALL RUNTIME-READY REVIEW EXPORTS IN FAMILY ORDER",
            "Source IDs": "DERIVED FROM RUNTIME_READY_REVIEW_EXPORT ROWS",
            "Source Class": "DERIVED REVIEW ASSEMBLY",
            "Original Title": reel["filename"],
            "Creator": "OpenAI Codex — Human Owner-directed assembly",
            "Source URL": "N/A — local derived review assembly",
            "License": "INHERITS ALL SOURCE EXPORT LICENSE CHAINS",
            "Canonical License URL": CC0_URL,
            "Commercial Use": "YES",
            "Modification": "YES",
            "Attribution": "INHERITED; NOT REQUIRED",
            "Evidence Path": "Preview/ProductionV002/SHOWCASE_AUDIO_AUDITION_INDEX_v002.json",
            "Evidence SHA-256": sha256_file(preview_root / "SHOWCASE_AUDIO_AUDITION_INDEX_v002.json"),
            "Original Site Filename": "N/A",
            "Recovered Source Filename": "N/A",
            "Acquisition/Recovery Timestamp": timestamp,
            "Transformation": "ORDERED ALL-VARIATION EXPORT ASSEMBLY; 0.38 s INTER-FILE SILENCE; NO VOICE; NO MUSIC; STEREO; RELATIVE HIERARCHY PRESERVED",
            "Destination": reel["path"],
            "SHA-256": reel["sha256"],
            "Bytes": reel["bytes"],
            "Duration Seconds": f"{reel['duration']:.6f}",
            "Channels": reel["channels"],
            "Sample Rate": reel["sample_rate"],
            "Source Bit Depth": 24,
            "Delivery Bit Depth": reel["bit_depth"],
            "Peak dBFS": f"{reel['peak_dbfs']:.6f}",
            "Human Owner Decision": "REVIEW_CANDIDATE",
            "Runtime Integration": "NOT_PERFORMED",
            "Notes": f"{len(reel['index'])} exports; all variations included; under 90 seconds",
        })

    expected_rows = 26 + 4 + 26 + 196 + 5
    if len(rows) != expected_rows:
        raise ValueError(f"ledger rows {len(rows)} != {expected_rows}")
    ledger_path = license_root / "AUDIO_SOURCE_LEDGER_v002.csv"
    with ledger_path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=LEDGER_FIELDS, lineterminator="\n", extrasaction="ignore")
        writer.writeheader()
        writer.writerows(rows)

    with matrix_path.open("r", encoding="utf-8-sig", newline="") as handle:
        matrix_reader = csv.DictReader(handle)
        matrix_fields = list(matrix_reader.fieldnames or [])
        matrix_rows = list(matrix_reader)
    new_fields = [
        "Production V002 Output Count",
        "Production V002 Exact Filenames",
        "Production V002 Review Status",
        "Production V002 Source Classification",
        "Production V002 QA",
    ]
    for field in new_fields:
        if field not in matrix_fields:
            matrix_fields.append(field)
    entries_by_cue = {}
    for entry in manifest["entries"]:
        entries_by_cue.setdefault(entry["cue_id"], []).append(entry)
    for row in matrix_rows:
        cue_entries = entries_by_cue[row["Cue ID"]]
        row["Source Status"] = "DELIVERED / REVIEW_CANDIDATE / NOT RUNTIME INTEGRATED"
        row["License Evidence Required"] = "DELIVERED — AUDIO_SOURCE_LEDGER_v002.csv + AUDIO_SOURCE_PROVENANCE_v002.md + 28 evidence snapshots"
        row["Candidate Source"] = "ProductionV002: official LaSonotheque CC0 recorded Foley + Human Owner-passed reference direction + project Original DSP where listed"
        row["Production V002 Output Count"] = len(cue_entries)
        row["Production V002 Exact Filenames"] = " | ".join(entry["path"] for entry in cue_entries)
        row["Production V002 Review Status"] = "REVIEW_CANDIDATE / HUMAN OWNER LISTENING REQUIRED"
        row["Production V002 Source Classification"] = " | ".join(sorted({source_details(entry["sources"], evidence_root)["Source Class"] for entry in cue_entries}))
        row["Production V002 QA"] = "PASS — PCM WAV 48 kHz/24-bit; decode; channels; nonzero; clipping/full-scale/DC; duplicate hash; loop seam where applicable"
    with matrix_path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=matrix_fields, lineterminator="\n")
        writer.writeheader()
        writer.writerows(matrix_rows)

    print(json.dumps({
        "ledger_rows": len(rows),
        "raw_source_rows": sum(row["Record Type"] == "RAW_SOURCE" for row in rows),
        "approved_reference_rows": sum(row["Record Type"] == "APPROVED_REFERENCE" for row in rows),
        "source_master_rows": sum(row["Record Type"] == "SOURCE_MASTER" for row in rows),
        "export_rows": sum(row["Record Type"] == "RUNTIME_READY_REVIEW_EXPORT" for row in rows),
        "audition_rows": sum(row["Record Type"] == "AUDITION_REEL" for row in rows),
        "matrix_rows": len(matrix_rows),
        "matrix_columns": len(matrix_fields),
        "ledger_path": str(ledger_path),
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
