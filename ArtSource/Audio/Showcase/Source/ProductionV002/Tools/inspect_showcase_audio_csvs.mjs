import fs from "node:fs/promises";
import { Workbook } from "@oai/artifact-tool";

const matrixPath = process.argv[2];
const ledgerPath = process.argv[3];
if (!matrixPath) {
  throw new Error("usage: node inspect_showcase_audio_csvs.mjs <cue-matrix.csv> [source-ledger.csv]");
}

const csvText = await fs.readFile(matrixPath, "utf8");
const workbook = await Workbook.fromCSV(csvText, { sheetName: "Cue Matrix" });
const sheet = workbook.worksheets.getItem("Cue Matrix");
const values = sheet.getUsedRange().values;
const headers = values[0];
const rows = values.slice(1).filter((row) => row.some((cell) => String(cell ?? "").length > 0));
const cueIndex = headers.indexOf("Cue ID");
const usageIndex = headers.indexOf("Contest Usage");
const ids = rows.map((row) => String(row[cueIndex]));
const duplicateIds = ids.filter((id, index) => ids.indexOf(id) !== index);
const families = {};
const usage = {};

for (const row of rows) {
  const family = String(row[cueIndex]).split("-")[1];
  families[family] = (families[family] ?? 0) + 1;
  const usageKey = String(row[usageIndex]);
  usage[usageKey] = (usage[usageKey] ?? 0) + 1;
}

const inspect = await workbook.inspect({
  kind: "workbook,sheet,region",
  sheetId: "Cue Matrix",
  range: `A1:${String.fromCharCode(64 + Math.min(headers.length, 26))}${Math.min(rows.length + 1, 8)}`,
  maxChars: 3500,
  tableMaxRows: 8,
  tableMaxCols: Math.min(headers.length, 26),
  tableMaxCellChars: 80,
});

const output = {
  rows: rows.length,
  columns: headers.length,
  headers,
  duplicateIds,
  families,
  usage,
  inspect: inspect.ndjson,
};

if (ledgerPath) {
  const ledgerText = await fs.readFile(ledgerPath, "utf8");
  const ledgerWorkbook = await Workbook.fromCSV(ledgerText, { sheetName: "Source Ledger" });
  const ledgerSheet = ledgerWorkbook.worksheets.getItem("Source Ledger");
  const ledgerValues = ledgerSheet.getUsedRange().values;
  const ledgerHeaders = ledgerValues[0];
  const ledgerRows = ledgerValues.slice(1).filter((row) => row.some((cell) => String(cell ?? "").length > 0));
  const typeIndex = ledgerHeaders.indexOf("Record Type");
  const recordIdIndex = ledgerHeaders.indexOf("Record ID");
  const destinationIndex = ledgerHeaders.indexOf("Destination");
  const ledgerIds = ledgerRows.map((row) => String(row[recordIdIndex]));
  output.ledger = {
    rows: ledgerRows.length,
    columns: ledgerHeaders.length,
    headers: ledgerHeaders,
    recordTypes: ledgerRows.reduce((counts, row) => {
      const key = String(row[typeIndex]);
      counts[key] = (counts[key] ?? 0) + 1;
      return counts;
    }, {}),
    duplicateRecordIds: ledgerIds.filter((id, index) => ledgerIds.indexOf(id) !== index),
    blankDestinations: ledgerRows.filter((row) => !String(row[destinationIndex] ?? "")).length,
  };
}

console.log(JSON.stringify(output, null, 2));
