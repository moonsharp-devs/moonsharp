#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
VERSION="${1:-2.0.0-local-dev}"
source "$REPO_ROOT/tools/upm/upm-common.sh"

DEV_ROOT="$REPO_ROOT/.upm-dev"

CORE_SRC_DIR="$REPO_ROOT/src/MoonSharp.Interpreter"
CORE_OUT_DIR="$DEV_ROOT/org.moonsharp.moonsharp"

DEBUG_SRC_DIR="$REPO_ROOT/src/MoonSharp.VsCodeDebugger"
DEBUG_OUT_DIR="$DEV_ROOT/org.moonsharp.debugger.vscode"

stage_core_package() {
  rm -rf "$CORE_OUT_DIR"
  mkdir -p "$CORE_OUT_DIR/Runtime"

  upm_link_top_level_entries "$CORE_SRC_DIR" "$CORE_OUT_DIR/Runtime"

  cat > "$CORE_OUT_DIR/package.json" <<JSON
{
  "name": "org.moonsharp.moonsharp",
  "version": "$VERSION",
  "displayName": "MoonSharp",
  "description": "A Lua interpreter for Unity and .NET.",
  "unity": "2020.3",
  "author": {
    "name": "MoonSharp Contributors"
  },
  "license": "MIT"
}
JSON

  cat > "$CORE_OUT_DIR/Runtime/MoonSharp.Interpreter.asmdef" <<'JSON'
{
  "name": "MoonSharp.Interpreter",
  "rootNamespace": "MoonSharp.Interpreter",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
JSON

  cp "$REPO_ROOT/LICENSE" "$CORE_OUT_DIR/LICENSE"
  cat > "$CORE_OUT_DIR/README.md" <<README
# MoonSharp Unity Package (Dev Symlink Layout)

Install in Unity \`manifest.json\`:
\`"org.moonsharp.moonsharp": "file:$CORE_OUT_DIR"\`
README

  generate_unity_metas_shallow "$CORE_OUT_DIR/Runtime" "org.moonsharp.moonsharp"
  generate_unity_metas_shallow "$CORE_OUT_DIR" "org.moonsharp.moonsharp"

  echo "Staged dev package: $CORE_OUT_DIR"
}

stage_vscode_debugger_package() {
  rm -rf "$DEBUG_OUT_DIR"
  mkdir -p "$DEBUG_OUT_DIR/Runtime"

  upm_link_top_level_entries "$DEBUG_SRC_DIR" "$DEBUG_OUT_DIR/Runtime"

  cat > "$DEBUG_OUT_DIR/package.json" <<JSON
{
  "name": "org.moonsharp.debugger.vscode",
  "version": "$VERSION",
  "displayName": "MoonSharp VSCode Debugger",
  "description": "Optional VSCode debug server for MoonSharp.",
  "unity": "2020.3",
  "author": {
    "name": "MoonSharp Contributors"
  },
  "license": "MIT",
  "dependencies": {
    "org.moonsharp.moonsharp": "$VERSION"
  }
}
JSON

  cat > "$DEBUG_OUT_DIR/Runtime/MoonSharp.VsCodeDebugger.asmdef" <<'JSON'
{
  "name": "MoonSharp.VsCodeDebugger",
  "rootNamespace": "MoonSharp.VsCodeDebugger",
  "references": [
    "MoonSharp.Interpreter"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
JSON

  cp "$REPO_ROOT/LICENSE" "$DEBUG_OUT_DIR/LICENSE"
  cat > "$DEBUG_OUT_DIR/README.md" <<README
# MoonSharp VSCode Debugger Unity Package (Dev Symlink Layout)

Install in Unity \`manifest.json\`:
\`"org.moonsharp.debugger.vscode": "file:$DEBUG_OUT_DIR"\`
README

  generate_unity_metas_shallow "$DEBUG_OUT_DIR/Runtime" "org.moonsharp.debugger.vscode"
  generate_unity_metas_shallow "$DEBUG_OUT_DIR" "org.moonsharp.debugger.vscode"

  echo "Staged dev package: $DEBUG_OUT_DIR"
}

stage_core_package
stage_vscode_debugger_package
