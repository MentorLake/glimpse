#!/bin/bash

installationDirectory="$HOME/.local/bin/"

echo
echo "### Remove binary from ${installationDirectory}"
rm ${installationDirectory}/glimpse

echo
echo "### Reset SUPER_L shortcut"
xfconf-query -c xfce4-keyboard-shortcuts -p "/commands/custom/<Primary>Escape" -r

echo
echo "### Enabling XFCE panel for failsafe session"

xfceStartupNumApps=$(xfconf-query -c xfce4-session -p /sessions/Failsafe/Count)

for ((i=0 ; i<$xfceStartupNumApps; i++)); do
	xfceApp=$(xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" | awk 'FNR == 3 {print}')
	if [[ $xfceApp =~ /glimpse$ ]]
	then
		xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" -r
	fi
done

echo "### Stopping Glimpse"
pkill -9 glimpse

echo
echo "### Enable xfce4-notifyd.service"
systemctl --user unmask xfce4-notifyd.service
systemctl --user start xfce4-notifyd.service
rm ~/.config/autostart/xfce4-notifyd.desktop

echo
echo "### Enable ayatana-indicator-application"
systemctl --user unmask ayatana-indicator-application
systemctl --user start ayatana-indicator-application
rm ~/.config/autostart/ayatana-indicator-application.desktop

echo
echo "Uninstall complete"
echo "*** If you have a saved X session then you will need to start xfce4-panel and save your session"
