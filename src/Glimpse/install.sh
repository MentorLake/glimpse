#!/bin/bash

installationDirectory="$HOME/.local/bin/"

echo
echo "### Creating ${installationDirectory}"
mkdir $installationDirectory

echo
echo "### Copy binary to ${installationDirectory}"
cp ./glimpse $installationDirectory

echo
echo "### Updating SUPER_L shortcut"
xfconf-query \
	-c xfce4-keyboard-shortcuts \
	-p "/commands/custom/<Primary>Escape" \
	-s "gdbus call --session --dest org.glimpse --object-path /org/glimpse --method org.gtk.Actions.Activate \"OpenStartMenu\" [] {}"

echo
echo "### Disabling XFCE panel and adding Glimpse to the failsafe session (the default X session)"

xfceStartupNumApps=$(xfconf-query -c xfce4-session -p /sessions/Failsafe/Count)

for ((i=0 ; i<$xfceStartupNumApps; i++)); do
	xfceApp=$(xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" | awk 'FNR == 3 {print}')
	if [[ $xfceApp == "xfce4-panel" ]]
	then
		xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" -s "${installationDirectory}glimpse" -a
	fi
done

echo
echo "### Stopping XFCE panel"
pkill -9 xfce4-panel

echo
echo "### Disable xfce4-notifyd.service"
systemctl --user mask xfce4-notifyd.service
pkill -9 xfce4-notifyd
sudo tee ~/.config/autostart/xfce4-notifyd.desktop << EOF
[Desktop Entry]
Hidden=true
EOF

echo
echo "### Disable ayatana-indicator-application.service"
systemctl --user mask ayatana-indicator-application
pkill -9 ayatana-indicat
sudo tee ~/.config/autostart/ayatana-indicator-application.desktop << EOF
[Desktop Entry]
Hidden=true
EOF

echo
echo "Installation complete"
echo "*** If you use a saved X session then you will need to save a new one with Glimpse running"
