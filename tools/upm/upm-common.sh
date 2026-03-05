#!/usr/bin/env bash

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

generate_unity_metas_recursive() {
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

generate_unity_metas_shallow() {
  local root="$1"
  local scope="$2"

  find "$root" -mindepth 1 -maxdepth 1 -name '*.meta' -delete

  shopt -s dotglob nullglob
  for entry in "$root"/*; do
    local name rel
    name="$(basename "$entry")"
    if [[ "$name" == *.meta ]]; then
      continue
    fi

    rel="${entry#"$root"/}"
    if [ -d "$entry" ]; then
      write_meta_for_dir "$entry" "$rel" "$scope"
    else
      write_meta_for_file "$entry" "$rel" "$scope"
    fi
  done
  shopt -u dotglob nullglob
}

upm_should_exclude_name() {
  local name="$1"
  local base_no_meta="$name"

  if [[ "$name" == *.meta ]]; then
    base_no_meta="${name%.meta}"
  fi

  case "$name" in
    *.meta|bin|obj|_Projects|*.csproj|*.sln|*.snk)
      return 0
      ;;
  esac

  case "$base_no_meta" in
    bin|obj|_Projects|*.csproj|*.sln|*.snk)
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

upm_rsync_filtered() {
  local src_dir="$1"
  local dst_dir="$2"

  rsync -a \
    --exclude 'bin/' \
    --exclude 'obj/' \
    --exclude '_Projects/' \
    --exclude '*.csproj' \
    --exclude '*.sln' \
    --exclude '*.snk' \
    "$src_dir/" \
    "$dst_dir/"
}

upm_link_top_level_entries() {
  local src_dir="$1"
  local runtime_dir="$2"
  local linked_count=0

  mkdir -p "$runtime_dir"
  shopt -s dotglob nullglob
  for entry in "$src_dir"/*; do
    local name
    name="$(basename "$entry")"

    if upm_should_exclude_name "$name"; then
      continue
    fi

    ln -s "$entry" "$runtime_dir/$name"
    linked_count=$((linked_count + 1))
  done
  shopt -u dotglob nullglob

  echo "Linked $linked_count entries from $src_dir -> $runtime_dir"
}
