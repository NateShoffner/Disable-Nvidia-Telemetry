& git clone "https://github.com/ReVolly/NsisDotNetChecker" "$env:APPVEYOR_BUILD_FOLDER\Deploy\Installer\NsisDotNetChecker" 2>&1 | % { $_.ToString() }

& "C:\Program Files (x86)\NSIS\makensis.exe" /DAPPLICATION_VERSION="$env:APPVEYOR_BUILD_VERSION" /DSOLUTION_DIRECTORY="$env:APPVEYOR_BUILD_FOLDER" "$env:APPVEYOR_BUILD_FOLDER\Deploy\Installer\Installer.nsi"

$installer = "$env:APPVEYOR_BUILD_FOLDER\Deploy\Installer\DisableNvidiaTelemetry $env:APPVEYOR_BUILD_VERSION Setup.exe"

# move executable to project directory for clean AppVeyor artifact name
Move-Item "$installer" "$env:APPVEYOR_BUILD_FOLDER"