#!/bin/bash

echo "Applying shared Git configurations..."

git config --local include.path ../.gitconfig.local

echo "Configuration applied!"
