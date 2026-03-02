#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "MoonSharp: syncing source mirrors..."
"${SRC_ROOT}/rsync_unity_projects.sh"

TESTBED_SOURCE_DIR="${SRC_ROOT}/MoonSharp.Interpreter.Tests/bin/Release/net45"

TESTBED_DEST_DIR="${SCRIPT_DIR}/UnityTestBed/Assets/Plugins"

copy_filtered() {
  local source_dir="$1"
  local dest_dir="$2"
  local label="$3"

  if [[ ! -d "${source_dir}" ]]; then
    echo "${label}: source directory not found: ${source_dir}"
    exit 1
  fi

  mkdir -p "${dest_dir}"

  shopt -s nullglob
  local files=("${source_dir}"/*.dll "${source_dir}"/*.pdb "${source_dir}"/*.xml)
  shopt -u nullglob

  local filtered=()
  local file
  for file in "${files[@]}"; do
    local name
    name="$(basename "${file}")"
    if [[ "${name}" == "nunit.framework.dll" ]]; then
      continue
    fi
    filtered+=("${file}")
  done

  if [[ ${#filtered[@]} -eq 0 ]]; then
    echo "${label}: no matching files found in ${source_dir}"
    exit 1
  fi

  cp -f "${filtered[@]}" "${dest_dir}/"
  printf '%s: copied %d file(s).\n' "${label}" "${#filtered[@]}"
}

copy_filtered "${TESTBED_SOURCE_DIR}" "${TESTBED_DEST_DIR}" "UnityTestBed"
