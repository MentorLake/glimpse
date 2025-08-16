#!/bin/bash

# Configuration variables
REPO_OWNER="mentorlake"  # Replace with your GitHub username or organization
REPO_NAME="glimpse"       # Replace with your repository name
APP_NAME="glimpse"         # Replace with your application name
INSTALL_DIR="/tmp" # Installation directory (modify as needed)

# Function to check if command exists
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

# Get current version (assuming the app outputs its version with --version)
if command_exists "$APP_NAME"; then
    CURRENT_VERSION=$($APP_NAME --version 2>/dev/null | grep -oE '[0-9]{4}\.[0-9]{2}\.[0-9]{2}\.[0-9]+' || echo "0.0.0.0")
else
    CURRENT_VERSION="0.0.0.0"
fi

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

# Download the asset
TEMP_FILE=$(mktemp)
curl -L -o "$TEMP_FILE" "$ASSET_URL"

# Install the binary (modify as needed based on your asset type)
if [[ "$ASSET_URL" == *.tar.gz ]]; then
    tar -xzf "$TEMP_FILE" -C /tmp
    sudo mv "/tmp/$APP_NAME" "$INSTALL_DIR/$APP_NAME"
elif [[ "$ASSET_URL" == *.zip ]]; then
    unzip -o "$TEMP_FILE" -d /tmp
    sudo mv "/tmp/$APP_NAME" "$INSTALL_DIR/$APP_NAME"
else
    sudo mv "$TEMP_FILE" "$INSTALL_DIR/$APP_NAME"
fi

# Set permissions
sudo chmod +x "$INSTALL_DIR/$APP_NAME"

# Clean up
rm -f "$TEMP_FILE"

# Verify installation
if command_exists "$APP_NAME"; then
    NEW_VERSION=$($APP_NAME --version 2>/dev/null | grep -oE '[0-9]{4}\.[0-9]{2}\.[0-9]{2}\.[0-9]+' || echo "unknown")
    if [ "$NEW_VERSION" = "$LATEST_RELEASE" ]; then
        echo "Successfully updated to $APP_NAME version $NEW_VERSION"
    else
        echo "Warning: Update completed but version check failed (got $NEW_VERSION, expected $LATEST_RELEASE)"
        exit 1
    fi
else
    echo "Error: Update failed, $APP_NAME not found after installation"
    exit 1
fi
