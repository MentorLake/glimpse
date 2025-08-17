#!/bin/bash

REPO_OWNER="mentorlake"
REPO_NAME="glimpse"
APP_NAME="glimpse"
CURRENT_VERSION=$1

command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check for required tools
for cmd in curl jq git; do
    if ! command_exists "$cmd"; then
        echo "Error: $cmd is required but not installed."
        exit 1
    fi
done

# Function to compare version numbers (yyyy.mm.dd.buildnumber)
compare_versions() {
    local v1=$1
    local v2=$2

    # Split versions into components
    IFS='.' read -ra v1_parts <<< "$v1"
    IFS='.' read -ra v2_parts <<< "$v2"

    # Compare each component
    for i in {0..3}; do
        if [ "${v1_parts[i]}" -lt "${v2_parts[i]}" ]; then
            return 1
        elif [ "${v1_parts[i]}" -gt "${v2_parts[i]}" ]; then
            return 2
        fi
    done
    return 0
}

# Get latest release from GitHub
LATEST_RELEASE=$(curl -s "https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/releases/latest" | jq -r '.tag_name')

# Validate release version format
if [[ ! $LATEST_RELEASE =~ ^[0-9]{4}\.[0-9]{2}\.[0-9]{2}\.[0-9]+$ ]]; then
    echo "Error: Latest release tag ($LATEST_RELEASE) does not match expected format (yyyy.mm.dd.buildnumber)"
    exit 1
fi

# Compare versions
compare_versions "$CURRENT_VERSION" "$LATEST_RELEASE"
VERSION_COMPARE=$?

if [ $VERSION_COMPARE -eq 0 ]; then
    echo "You are already running the latest version ($CURRENT_VERSION)"
    exit 0
elif [ $VERSION_COMPARE -eq 2 ]; then
    echo "Your version ($CURRENT_VERSION) is newer than the latest release ($LATEST_RELEASE)"
    exit 0
fi

echo "New version available: $LATEST_RELEASE (current: $CURRENT_VERSION)"

# Download and install the latest release
echo "Downloading $APP_NAME version $LATEST_RELEASE..."

# Construct download URL (modify asset name as per your release assets)
ASSET_URL=$(curl -s "https://api.github.com/repos/$REPO_OWNER/$REPO_NAME/releases/latest" | jq -r ".assets[] | select(.name | contains(\"$APP_NAME\")) | .browser_download_url")

if [ -z "$ASSET_URL" ]; then
    echo "Error: Could not find download URL for $APP_NAME in latest release"
    exit 1
fi

TEMP_FILE=$(mktemp)
curl -L -o "$TEMP_FILE" "$ASSET_URL"
unzip -o "$TEMP_FILE" -d /tmp
rm -f "$TEMP_FILE"
chmod +x "/tmp/$APP_NAME"
bash -c "/tmp/$APP_NAME install"
pkill -9 $APP_NAME
