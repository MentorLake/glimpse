#!/bin/bash

function disableXfcePanel() {

	# Disabling XFCE panel and adding Glimpse to the failsafe session (the default X session)"
	xfceStartupNumApps=$(xfconf-query -c xfce4-session -p /sessions/Failsafe/Count)

	for ((i=0 ; i<$xfceStartupNumApps; i++)); do
		xfceApp=$(xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" | awk 'FNR == 3 {print}')
		if [[ $xfceApp == "xfce4-panel" ]]
		then
			xfconf-query -c xfce4-session -p "/sessions/Failsafe/Client${i}_Command" -s "${installationDirectory}glimpse" -a
		fi
	done

	# Check if gdbus is installed
	if ! command -v gdbus &> /dev/null; then
		echo "Error: gdbus is not installed. Please install the glib2 package."
		return
	fi

	# Check if XFCE4 session manager is running
	if ! pgrep -x "xfce4-session" > /dev/null; then
		echo "Error: XFCE4 session manager is not running."
		return
	fi

	# Get list of clients
	clients_output=$(gdbus call --session \
		--dest org.xfce.SessionManager \
		--object-path /org/xfce/SessionManager \
		--method org.xfce.Session.Manager.ListClients 2>/dev/null)

	if [ $? -ne 0 ] || [ "$clients_output" = "((),)" ]; then
		echo "Error: Failed to retrieve list of clients or no clients available."
		return
	fi

	# Extract client object paths
	client_paths=$(echo "$clients_output" | sed -n 's/^ *( *\[\(.*\)\], *)$/\1/p' | sed 's/,/ /g' | sed "s/'//g")

	if [ -z "$client_paths" ]; then
		echo "Error: No client paths found."
		return
	fi

	# Find the xfce4-panel client
	panel_client=""
	for path in $client_paths; do
		props_output=$(gdbus call --session \
			--dest org.xfce.SessionManager \
			--object-path "$path" \
			--method org.xfce.Session.Client.GetAllSmProperties 2>/dev/null)

		if [ $? -eq 0 ] && echo "$props_output" | grep -q "'Program': <'xfce4-panel'>"; then
			panel_client="$path"
			break
		fi
	done

	if [ -z "$panel_client" ]; then
		echo "Error: XFCE4 panel client not found."
		return
	fi

	# Set the RestartStyle property to RestartIfRunning
	set_output=$(gdbus call --session \
		--dest org.xfce.SessionManager \
		--object-path "$panel_client" \
		--method org.xfce.Session.Client.SetSmProperties \
		"{ 'RestartStyleHint': <byte 0x00> }" 2>/dev/null)

	if [ $? -eq 0 ] && echo "$set_output" | grep -q "^(true,)$"; then
		echo "Successfully set XFCE4 panel restart style to IfRunning."
	else
		echo "Error: Failed to set XFCE4 panel restart style."
	fi

	pkill -9 xfce4-panel
}

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

xfconf-query \
	-c xfce4-keyboard-shortcuts \
	-p "/commands/custom/Super_L" \
	-n \
	-t string \
	-s "gdbus call --session --dest org.glimpse --object-path /org/glimpse --method org.gtk.Actions.Activate \"OpenStartMenu\" [] {}"

disableXfcePanel

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

pkill -9 glimpse
sleep 1
setsid ${installationDirectory}/glimpse &
