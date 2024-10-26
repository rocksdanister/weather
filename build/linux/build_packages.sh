#!/bin/bash

# Clean previous package directory
rm -rf package/
mkdir -p package/usr/bin
mkdir -p package/usr/lib/livelyweather
mkdir -p package/usr/share/applications
mkdir -p package/usr/share/icons/hicolor/256x256/apps

# Copy application files
cp -r build/* package/usr/lib/livelyweather/

# Create symlink
ln -s ../lib/livelyweather/Drizzle.UI.Avalonia.Desktop package/usr/bin/livelyweather

# Copy desktop file and icon
cp resources/com.rocksdanister.LivelyWeather.desktop package/usr/share/applications/
cp resources/com.rocksdanister.LivelyWeather.png package/usr/share/icons/hicolor/256x256/apps/

# Ensure executables have correct permissions
chmod +x package/usr/lib/livelyweather/Drizzle.UI.Avalonia.Desktop

# Build package
fpm -t deb 
fpm -t rpm 
# tar -> tar.gz
fpm -t tar
gzip livelyweather.tar
