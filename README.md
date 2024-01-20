## Overview

Glimpse is a modern looking and familiar Windows 11-like panel for XFCE.  It was written to provide a high quality alternative to existing panels and to increase developer accessibility by leveraging modern tools and programming paradigms.  Its goal is to be an all-in-one package that minimizes dependencies on other libraries and components.

## Features

1. Synchronized panels at the bottom of every monitor
2. A system tray for every panel
3. Notifications
4. Windows 11-like start menu with pinning and searching
5. Customizable start menu context menu

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
