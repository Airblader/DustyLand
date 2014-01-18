#!/usr/bin/env bash

echo "[I] Copying plugin data..."
cp -R "GameData" "$KSP_HOME"

echo "[I] Starting KSP..."
"$KSP_HOME/KSP.x86_64"

echo "[I] Done."
