Using [fpm](https://github.com/jordansissel/fpm) to create installer packages (deb, rpm.)

Make the build script executable `chmod +x build_packages.sh` and run `./build_packages.sh`

The script will create a `package` directory using the application files directory `build` and `resources` as follows:
```
package/
├── usr/
│   ├── bin/
│   │   └── livelyweather (symlink to ../lib/livelyweather/Drizzle.UI.Avalonia.Desktop)
│   └── lib/
│       └── livelyweather/
│           ├── Drizzle.UI.Avalonia.Desktop
│           ├── (other application files)
├── usr/share/
│   ├── applications/
│   │   └── com.rocksdanister.LivelyWeather.desktop
│   └── icons/hicolor/256x256/apps/
│       └── com.rocksdanister.LivelyWeather.png
```
Then create packages using the .fpm configuration file.