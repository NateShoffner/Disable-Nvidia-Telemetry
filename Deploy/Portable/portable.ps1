$temp_directory = "$env:APPVEYOR_BUILD_FOLDER\Deploy\Portable\~TEMP"
New-Item "$temp_directory" -type directory

$zip_archive = "$env:APPVEYOR_BUILD_FOLDER\Deploy\Portable\DisableNvidiaTelemetry $env:APPVEYOR_BUILD_VERSION Portable.exe"

# build using portable configuration
& msbuild.exe "$env:APPVEYOR_BUILD_FOLDER\DisableNvidiaTelemetry.sln" /p:Configuration=Portable

$output_directory = "$env:APPVEYOR_BUILD_FOLDER\DisableNvidiaTelemetry\bin\Portable"

# copy files
Get-ChildItem -Path "$output_directory" | % {
    Copy-Item $_.fullname "$temp_directory" -Recurse -Force -Exclude @("*.xml", "*.pdb", "*.manifest", "*.application", "*.vshost.*") 
}

# zip contents in self-extracting archive
& 7z "a" "$zip_archive" "-mmt" "-mx5" "-sfx7z.sfx" "-r" "$temp_directory\*.*"

# move executable to project directory for clean AppVeyor artifact name
Move-Item "$zip_archive" "$env:APPVEYOR_BUILD_FOLDER"