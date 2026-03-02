#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
VERSION="${1:-2.0.0-local}"

CORE_SRC_DIR="$REPO_ROOT/src/MoonSharp.Interpreter"
CORE_OUT_DIR="$REPO_ROOT/.upm-staging/org.moonsharp.moonsharp"

DEBUG_SRC_DIR="$REPO_ROOT/src/MoonSharp.VsCodeDebugger"
DEBUG_OUT_DIR="$REPO_ROOT/.upm-staging/org.moonsharp.debugger.vscode"

make_guid() {
  local rel_path="$1"
  if command -v shasum >/dev/null 2>&1; then
    printf '%s' "$rel_path" | shasum -a 1 | awk '{print substr($1,1,32)}'
  else
    printf '%s' "$rel_path" | md5 | awk '{print $NF}'
  fi
}

write_meta_for_dir() {
  local abs_path="$1"
  local rel_path="$2"
  local scope="$3"
  local guid
  guid="$(make_guid "dir:${scope}:${rel_path}")"
  cat > "${abs_path}.meta" <<META
fileFormatVersion: 2
guid: ${guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
META
}

write_meta_for_file() {
  local abs_path="$1"
  local rel_path="$2"
  local scope="$3"
  local ext="${abs_path##*.}"
  local guid
  guid="$(make_guid "file:${scope}:${rel_path}")"

  case "$ext" in
    cs)
      cat > "${abs_path}.meta" <<META
fileFormatVersion: 2
guid: ${guid}
MonoImporter:
  externalObjects: {}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {instanceID: 0}
  userData:
  assetBundleName:
  assetBundleVariant:
META
      ;;
    asmdef)
      cat > "${abs_path}.meta" <<META
fileFormatVersion: 2
guid: ${guid}
AssemblyDefinitionImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
META
      ;;
    *)
      cat > "${abs_path}.meta" <<META
fileFormatVersion: 2
guid: ${guid}
DefaultImporter:
  externalObjects: {}
  userData:
  assetBundleName:
  assetBundleVariant:
META
      ;;
  esac
}

generate_unity_metas() {
  local root="$1"
  local scope="$2"
  find "$root" -name '*.meta' -delete

  while IFS= read -r dir; do
    [ "$dir" = "$root" ] && continue
    local rel="${dir#"$root"/}"
    write_meta_for_dir "$dir" "$rel" "$scope"
  done < <(find "$root" -type d | LC_ALL=C sort)

  while IFS= read -r file; do
    local rel="${file#"$root"/}"
    write_meta_for_file "$file" "$rel" "$scope"
  done < <(find "$root" -type f ! -name '*.meta' | LC_ALL=C sort)
}

stage_core_package() {
  rm -rf "$CORE_OUT_DIR"
  mkdir -p "$CORE_OUT_DIR/Runtime"

  rsync -a \
    --exclude 'bin/' \
    --exclude 'obj/' \
    --exclude '_Projects/' \
    --exclude '*.csproj' \
    --exclude '*.sln' \
    --exclude '*.snk' \
    "$CORE_SRC_DIR/" \
    "$CORE_OUT_DIR/Runtime/"

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
# MoonSharp Unity Package

Install options:

1. Local path in Unity \`manifest.json\`:
   \`"org.moonsharp.moonsharp": "file:$CORE_OUT_DIR"\`
2. Tarball via Unity Package Manager:
   Use "Add package from tarball..." and select a release \`.tgz\` asset.
README

  generate_unity_metas "$CORE_OUT_DIR" "org.moonsharp.moonsharp"

  echo "Staged: $CORE_OUT_DIR"
}

stage_vscode_debugger_package() {
  rm -rf "$DEBUG_OUT_DIR"
  mkdir -p "$DEBUG_OUT_DIR/Runtime"

  rsync -a \
    --exclude 'bin/' \
    --exclude 'obj/' \
    --exclude '_Projects/' \
    --exclude '*.csproj' \
    --exclude '*.sln' \
    --exclude '*.snk' \
    "$DEBUG_SRC_DIR/" \
    "$DEBUG_OUT_DIR/Runtime/"

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
# MoonSharp VSCode Debugger Unity Package

Install options:

1. Local path in Unity \`manifest.json\`:
   \`"org.moonsharp.debugger.vscode": "file:$DEBUG_OUT_DIR"\`
2. Tarball via Unity Package Manager:
   Use "Add package from tarball..." and select a release \`.tgz\` asset.
README

  generate_unity_metas "$DEBUG_OUT_DIR" "org.moonsharp.debugger.vscode"

  echo "Staged: $DEBUG_OUT_DIR"
}

stage_core_package
stage_vscode_debugger_package
