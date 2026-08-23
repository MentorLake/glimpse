# Glimpse

Glimpse is a Windows 11-style desktop panel for XFCE on Linux. It replaces the default XFCE panel with a modern taskbar experience, including a start menu, system tray, notifications, and more — all synchronized across multiple monitors.

## Features

- **Multi-monitor support** — Synchronized panels at the bottom of every connected monitor
- **System tray** — A system tray on every panel (replaces `ayatana-indicator-application`)
- **Notifications** — Desktop notifications with history (replaces `xfce4-notifyd`)
- **Start menu** — Windows 11-style start menu with app pinning and search
- **Customizable context menu** — Right-click context menu in the start menu
- **Side pane** — Slide-out panel with notification history and a calendar
- **Wallpaper** — Integrated Iris wallpaper manager

## Screenshots

_Coming soon._

## Requirements

- XFCE desktop environment
- `curl`, `jq`, and `git` (required for `glimpse update`)
- `gdbus` / `glib2` (required for install)
- `systemctl` with user session support

## Installation

Glimpse is installed per-user (no root required).

1. Download the latest release from the [Releases](https://github.com/mentorlake/glimpse/releases/latest) page.
2. Unzip the archive and run:

```bash
glimpse install
```

The installer will:
- Copy the `glimpse` binary to `~/.local/bin/`
- Replace `xfce4-panel` with Glimpse in the XFCE failsafe session
- Disable `xfce4-notifyd` and replace it with Glimpse notifications
- Disable `ayatana-indicator-application` and replace it with the Glimpse system tray
- Update the `Super_L` keyboard shortcut to open the Glimpse start menu instead of Whisker Menu
- Launch Glimpse immediately

> **Note:** If you use a saved X session (not the default failsafe session), you will need to save a new session after installation with Glimpse running.

## Uninstalling

```bash
glimpse uninstall
```

This will:
- Remove the `glimpse` binary from `~/.local/bin/`
- Re-enable `xfce4-panel` in the failsafe session
- Re-enable `xfce4-notifyd` and `ayatana-indicator-application`
- Reset the `Super_L` keyboard shortcut

> **Note:** If you have a saved X session, you will need to start `xfce4-panel` manually and save your session again.

## Updating

```bash
glimpse update
```

This checks the [GitHub Releases](https://github.com/mentorlake/glimpse/releases/latest) page for a newer version (format: `yyyy.mm.dd.buildnumber`). If a newer version is available, it downloads and installs it automatically.

## Usage

| Action | How |
|---|---|
| Open start menu | Press `Super` (Windows key) or `Ctrl+Esc` |
| Search for apps | Open the start menu and start typing |
| Pin/unpin apps | Right-click an app in the start menu |
| Open side pane | Click the calendar/notification button on the panel |
| View notification history | Open the side pane |

## License

See [LICENSE.md](LICENSE.md).
