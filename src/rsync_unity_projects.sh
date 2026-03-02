#!/usr/bin/env bash
set -euo pipefail

BASE_DIR="$(cd "$(dirname "$0")" && pwd)"

sync_cs_tree_preserve_meta() {
  local source_dir="$1"
  local dest_dir="$2"
  local label="$3"

  mkdir -p "${dest_dir}"

  # Remove only mirrored C# files; keep Unity .meta files/GUIDs stable.
  find "${dest_dir}" -type f -name '*.cs' -delete

  echo "... ${label}"
  rsync -a --prune-empty-dirs --exclude 'obj/' --exclude '*.csproj' --include '*/' --include '*.cs' --exclude '*' "${source_dir}/" "${dest_dir}/"
}

echo "Syncing Unity/MoonSharp source mirrors..."
sync_cs_tree_preserve_meta "${BASE_DIR}/MoonSharp.Interpreter" "${BASE_DIR}/Unity/MoonSharp/Assets/Plugins/MoonSharp/Interpreter" "Unity - interpreter"
sync_cs_tree_preserve_meta "${BASE_DIR}/MoonSharp.VsCodeDebugger" "${BASE_DIR}/Unity/MoonSharp/Assets/Plugins/MoonSharp/Debugger" "Unity - vscode debugger"
sync_cs_tree_preserve_meta "${BASE_DIR}/MoonSharp.Interpreter.Tests" "${BASE_DIR}/Unity/MoonSharp/Assets/Tests" "Unity - unit tests"
