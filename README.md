## Overview

Glimpse is a Windows 11-like panel for XFCE.

## Features

1. Synchronized panels at the bottom of every monitor
2. A system tray for every panel (replaces ayatana-indicator-application)
3. Notifications (replaces xfce4-notifyd)
4. Windows 11-like start menu with pinning and searching
5. Customizable start menu context menu
6. Slide-out side pane with notification history and a calendar

## Installation - Per User

1. Run ```glimpse install```.
2. Installation will do the following (see install.sh):
   * Copies the Glimpse binary into the ~/.local/bin/ directory.
   * Replaces the xfce-panel with glimpse.
   * Replaces ayantana-indicator-application for the system tray.
   * Replaces xfce-notifyd for notifications.
   * Updates the SUPER_L shortcut to open the Glimpse start menu instead of the whisker menu.
   * NOTE: You need to save a new X session if you're not using the default one.

## Uninstalling - Per User

1. Run ```glimpse uninstall```.
2. The uninstall script will reactivate xfce-panel, ayatana indicators, xfce-notifyd, and the SUPER_L shortcut.


## Updating to latest version

1. Run ```glimpse update```
