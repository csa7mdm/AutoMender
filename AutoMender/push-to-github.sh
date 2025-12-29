#!/bin/bash

echo "🐙 AutoMender GitHub Deployer"
echo "=============================="

# 1. Ensure Git is ready
if [ ! -d ".git" ]; then
    echo "❌ Error: Not a git repository. Something went wrong with initialization."
    exit 1
fi

# 2. Get GitHub Details
echo "Please enter your GitHub Username:"
read USERNAME

if [ -z "$USERNAME" ]; then
    echo "❌ Username cannot be empty."
    exit 1
fi

REPO_NAME="AutoMender"
REMOTE_URL="https://github.com/$USERNAME/$REPO_NAME.git"

# 3. Configure Remote
echo ""
echo "🔗 Configuring remote 'origin' to: $REMOTE_URL"

# Remove old origin if it exists to prevent errors
git remote remove origin 2>/dev/null
git remote add origin $REMOTE_URL

# 4. Push
echo ""
echo "📤 Attempting to push to GitHub..."
git branch -M main

if git push -u origin main; then
    echo ""
    echo "✅ SUCCESS! Your project is live at:"
    echo "   $REMOTE_URL"
else
    echo ""
    echo "❌ Push failed."
    echo "Reason: The repository '$REPO_NAME' probably doesn't exist on GitHub yet."
    echo ""
    echo "👉 ACTION REQUIRED:"
    echo "   1. Open this URL: https://github.com/new"
    echo "   2. Name the repository: $REPO_NAME"
    echo "   3. Click 'Create repository'"
    echo "   4. Run this script again!"
fi
