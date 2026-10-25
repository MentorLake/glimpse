#!/bin/bash
# Runs Glimpse inside an isolated Xephyr session:
#   Xephyr -> dbus session bus -> window manager -> wallpaper (xfdesktop) -> Glimpse
#
# Usage:
#   ./scripts/test-xephyr.sh [--size 1600x900] [--config Debug] [--no-build] [-- <extra glimpse args>]
#
# Requirements: Xephyr, a window manager (xfwm4, or picom/openbox/cage as fallbacks),
# dbus (dbus-launch), xfdesktop (wallpaper).

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SRC_DIR="$REPO_ROOT/src"

DISPLAY_GEOMETRY="1800x900"
CONFIGURATION="Debug"
DO_BUILD=true
GLIMPSE_ARGS=()

while [[ $# -gt 0 ]]; do
	case "$1" in
		--size)        DISPLAY_GEOMETRY="$2"; shift 2 ;;
		--config)      CONFIGURATION="$2"; shift 2 ;;
		--no-build)    DO_BUILD=false; shift ;;
		--)            shift; GLIMPSE_ARGS=("$@"); break ;;
		*)             echo "Unknown option: $1" >&2; exit 1 ;;
	esac
done

for tool in Xephyr dbus-launch; do
	command -v "$tool" >/dev/null || { echo "Error: $tool is required but not installed." >&2; exit 1; }
done

# --- pick a window manager / compositor -------------------------------------
pick_wm() {
	if command -v xfwm4 >/dev/null;   then echo "xfwm4";   return; fi
	if command -v openbox >/dev/null; then echo "openbox"; return; fi
	if command -v cage >/dev/null;    then echo "cage";    return; fi
	echo ""
}
WM="$(pick_wm)"
if [[ -z "$WM" ]]; then
	echo "Error: no window manager found (install xfwm4, openbox, or cage)." >&2
	exit 1
fi

# --- build -------------------------------------------------------------------
if $DO_BUILD; then
	echo "Building Glimpse ($CONFIGURATION)..."
	dotnet build "$SRC_DIR/Glimpse/Glimpse.csproj" -c "$CONFIGURATION" --nologo -v quiet
fi
GLIMPSE_ASSEMBLY="glimpse"
[[ "$CONFIGURATION" == "Debug" ]] && GLIMPSE_ASSEMBLY="glimpse_dev"
GLIMPSE_BIN="$SRC_DIR/Glimpse/bin/$CONFIGURATION/net10.0/$GLIMPSE_ASSEMBLY"
[[ -x "$GLIMPSE_BIN" ]] || { echo "Error: built binary not found at $GLIMPSE_BIN" >&2; exit 1; }

# --- isolated X display --------------------------------------------------------
XEPHYR_DISPLAY_NUM=$(( 20 + RANDOM % 200 ))
XEPHYR_DISPLAY=":$XEPHYR_DISPLAY_NUM"
XEPHYR_LOG="$(mktemp /tmp/glimpse-xephyr.XXXXXX.log)"

echo "Starting Xephyr on $XEPHYR_DISPLAY ($DISPLAY_GEOMETRY)..."
Xephyr "$XEPHYR_DISPLAY" -screen "${DISPLAY_GEOMETRY}x24" -ac -br -noreset -name "Glimpse Test" >"$XEPHYR_LOG" 2>&1 &
XEPHYR_PID=$!

cleanup() {
	trap - EXIT
	echo "Shutting down..."
	kill "${GLIMPSE_PID:-}" 2>/dev/null || true
	kill "${WALLPAPER_PID:-}" 2>/dev/null || true
	kill "${WM_PID:-}" 2>/dev/null || true
	kill "${DBUS_PID:-}" 2>/dev/null || true
	kill "$XEPHYR_PID" 2>/dev/null || true
	wait 2>/dev/null || true
	rm -f "$XEPHYR_LOG"
}
trap cleanup EXIT

for _ in $(seq 1 50); do
	[[ -e "/tmp/.X11-unix/X${XEPHYR_DISPLAY_NUM}" ]] && break
	sleep 0.1
done
kill -0 "$XEPHYR_PID" 2>/dev/null || { echo "Error: Xephyr failed to start:"; cat "$XEPHYR_LOG"; exit 1; }

# --- private dbus session bus --------------------------------------------------
# Glimpse needs a session bus (StatusNotifier, Accounts, XFCE session mgmt).
# A fresh bus per run keeps the test isolated from the developer's real session.
eval "$(dbus-launch --sh-syntax)"
export DBUS_SESSION_BUS_ADDRESS DBUS_SESSION_BUS_PID
DBUS_PID="$DBUS_SESSION_BUS_PID"
echo "Private dbus session bus: $DBUS_SESSION_BUS_ADDRESS"

export DISPLAY="$XEPHYR_DISPLAY"
unset GDK_BACKEND WAYLAND_DISPLAY 2>/dev/null || true

# --- window manager / compositor ------------------------------------------------
echo "Starting $WM..."
"$WM" >/dev/null 2>&1 &
WM_PID=$!
sleep 1

# --- wallpaper ------------------------------------------------------------------
# Glimpse sets the wallpaper via xfconf (xfce4-desktop backdrop properties),
# so xfdesktop is the process that actually paints it.
WALLPAPER_PID=""
if command -v xfdesktop >/dev/null; then
	echo "Starting xfdesktop (wallpaper)..."
	xfdesktop >/dev/null 2>&1 &
	WALLPAPER_PID=$!
else
	echo "Warning: xfdesktop not found - wallpaper will not be rendered." >&2
fi

# --- Glimpse ---------------------------------------------------------------------
echo "Starting Glimpse..."
"$GLIMPSE_BIN" "${GLIMPSE_ARGS[@]}" &
GLIMPSE_PID=$!

echo ""
echo "Glimpse test environment ready:"
echo "  display:    $XEPHYR_DISPLAY (pid $XEPHYR_PID)"
echo "  dbus bus:   $DBUS_SESSION_BUS_ADDRESS"
echo "  wm:         $WM (pid $WM_PID)"
echo "  glimpse:    pid $GLIMPSE_PID"
echo "Press Ctrl+C to tear everything down."
echo ""

wait "$GLIMPSE_PID"
