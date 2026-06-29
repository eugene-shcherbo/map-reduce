#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
PAT_FILE="${SCRIPT_DIR}/nuget-pat.txt"

if [ ! -f "${PAT_FILE}" ]; then
  echo "ERROR: PAT file not found: ${PAT_FILE}" >&2
  echo "Create the file and put a GitHub Packages / NuGet PAT in it. This file is ignored by git." >&2
  exit 1
fi

PAT=$(cat "${PAT_FILE}")
FEED_URL="https://nuget.pkg.github.com/eugene-shcherbo/index.json"

echo "Using feed: ${FEED_URL}"

PROJECT_FILE="${SCRIPT_DIR}/MapReduceEngine.Abstractions.csproj"
if [ ! -f "${PROJECT_FILE}" ]; then
  echo "ERROR: Project file not found: ${PROJECT_FILE}" >&2
  exit 1
fi

echo "Building ${PROJECT_FILE} (Release)..."
dotnet build "${PROJECT_FILE}" -c Release || {
  echo "dotnet build failed." >&2
  exit 1
}

echo "Packing ${PROJECT_FILE} (Release)..."
VERSION=${1-}
if [ -n "${VERSION}" ]; then
  echo "Packing with explicit version ${VERSION}"
  dotnet pack "${PROJECT_FILE}" -c Release /p:PackageVersion="${VERSION}" || {
    echo "dotnet pack failed." >&2
    exit 1
  }
else
  dotnet pack "${PROJECT_FILE}" -c Release || {
    echo "dotnet pack failed." >&2
    exit 1
  }
fi

# Find the single package to push. If a version was provided, look for that exact nupkg.
if [ -n "${VERSION}" ]; then
  PACKAGE=$(find "${SCRIPT_DIR}" -type f -name "MapReduceEngine.Abstractions.${VERSION}.nupkg" -print -quit)
else
  PACKAGE=$(find "${SCRIPT_DIR}" -type f -name 'MapReduceEngine.Abstractions.*.nupkg' ! -name '*.symbols.nupkg' -print0 | xargs -0 ls -t 2>/dev/null | head -n1 || true)
fi

if [ -z "${PACKAGE}" ]; then
  echo "ERROR: No matching .nupkg for MapReduceEngine.Abstractions found." >&2
  echo "Looked in: ${SCRIPT_DIR}" >&2
  exit 1
fi

echo "Pushing ${PACKAGE} to ${FEED_URL}"
dotnet nuget push "${PACKAGE}" -s "${FEED_URL}" -k "${PAT}" --skip-duplicate || {
  echo "Failed to push ${PACKAGE}" >&2
  exit 1
}

echo "Done."
