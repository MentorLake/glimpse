#!/bin/bash

scriptDirectory=$(dirname $0)
installationDirectory="$HOME/.local/bin/"

# Creating ${installationDirectory}"
if [ ! -d "$installationDirectory" ]; then
    mkdir -p "$installationDirectory"
fi

# Copy binary to ${installationDirectory}"
mv $scriptDirectory/glimpse $installationDirectory

# Updating SUPER_L shortcut"
xfconf-query \
	-c xfce4-keyboard-shortcuts \
	-p "/commands/custom/<Primary>Escape" \
	-n \
	-t string \
	-s "gdbus call --session --dest org.glimpse --object-path /org/glimpse --method org.gtk.Actions.Activate \"OpenStartMenu\" [] {}"

# Disabling XFCE panel and adding Glimpse to the failsafe session (the default X session)"
xfceStartupNumApps=$(xfconf-query -c xfce4-session -p /sessions/Failsafe/Count)

for ((i=0 ; i<$xfceStartupNumApps; i++)); do
	xfceApp=$(xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" | awk 'FNR == 3 {print}')
	if [[ $xfceApp == "xfce4-panel" ]]
	then
		xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" -s "${installationDirectory}glimpse" -a
	fi
done

# Stopping XFCE panel"
pkill -9 xfce4-panel

# Disable xfce4-notifyd.service"
systemctl --user mask xfce4-notifyd.service
pkill -9 xfce4-notifyd
tee ~/.config/autostart/xfce4-notifyd.desktop > /dev/null << EOF
[Desktop Entry]
Hidden=true
EOF

# Disable ayatana-indicator-application.service"
systemctl --user mask ayatana-indicator-application
pkill -9 ayatana-indicat
tee ~/.config/autostart/ayatana-indicator-application.desktop > /dev/null << EOF
[Desktop Entry]
Hidden=true
EOF

echo "Installation complete"
echo "*** If you use a saved X session then you will need to save a new one with Glimpse running"

pkill -9 $APP_NAME
sleep 1
setsid ${installationDirectory}/glimpse &
