!include "MUI2.nsh"
!include "NsisDotNetChecker\nsis\DotNetChecker.nsh"

!addplugindir "NsisDotNetChecker\bin"

;--------------------------------
;Constants

  !define PRIMARY_EXE_NAME "Disable Nvidia Telemetry"
  
  !define PRODUCT_NAME "Disable Nvidia Telemetry"
  !define PRODUCT_VERSION "${APPLICATION_VERSION}"
  !define PRODUCT_PUBLISHER "Nate Shoffner"
  !define PRODUCT_WEB_SITE "https://nateshoffner.com"
  !define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\${PRIMARY_EXE_NAME}.exe"
  !define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
  !define PRODUCT_UNINST_ROOT_KEY "HKLM"

;--------------------------------
;General

  ;Name and file
  Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
  OutFile "${PRODUCT_NAME} ${PRODUCT_VERSION} Setup.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\${PRODUCT_NAME}"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
  
  ShowInstDetails show
  ShowUnInstDetails show
  
  SetCompressor lzma

  ;Request application privileges for Windows Vista
  RequestExecutionLevel admin
  
  BrandingText "${PRODUCT_PUBLISHER}"

;--------------------------------
;Interface Settings
  
  !define MUI_ABORTWARNING
  !define MUI_ICON "Icon.ico"
  !define MUI_UNICON "Icon.ico"

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE  "License.txt"
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !define MUI_FINISHPAGE_RUN
  !define MUI_FINISHPAGE_RUN_TEXT "Run Program"
  !define MUI_FINISHPAGE_RUN_FUNCTION "LaunchProgram"
  !insertmacro MUI_PAGE_FINISH
	
Section

  SetOutPath "$INSTDIR"
  SetOverwrite ifnewer
  
  !insertmacro CheckNetFramework 35 ;
  
  File "License.txt"
  File "${SOLUTION_DIRECTORY}\DisableNvidiaTelemetry\bin\Release\Disable Nvidia Telemetry.exe"
  File "${SOLUTION_DIRECTORY}\DisableNvidiaTelemetry\bin\Release\log4net.dll"
  File "${SOLUTION_DIRECTORY}\DisableNvidiaTelemetry\bin\Release\Microsoft.Win32.TaskScheduler.dll"
  File "${SOLUTION_DIRECTORY}\DisableNvidiaTelemetry\bin\Release\Newtonsoft.Json.dll"  
  File "${SOLUTION_DIRECTORY}\DisableNvidiaTelemetry\bin\Release\ExtendedVersion.dll"  
  CreateDirectory "$SMPROGRAMS\${PRODUCT_NAME}"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRIMARY_EXE_NAME}.lnk" "$INSTDIR\${PRIMARY_EXE_NAME}.exe"

SectionEnd
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  Delete "$INSTDIR\LICENSE"
  Delete "$INSTDIR\Disable Nvidia Telemetry.exe"
  Delete "$INSTDIR\log4net.dll"
  Delete "$INSTDIR\Microsoft.Win32.TaskScheduler.dll"
  Delete "$INSTDIR\Newtonsoft.Json.dll"
  Delete "$INSTDIR\ExtendedVersion.dll"
  
  Delete "$INSTDIR\Uninstall.exe"
  Delete "$SMPROGRAMS\Disable Nvidia Telemetry\Disable Nvidia Telemetry.lnk"
  RMDir "$SMPROGRAMS\Disable Nvidia Telemetry"
  RMDir "$INSTDIR"
 
  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  
  SetAutoClose true

SectionEnd

Function .onInstSuccess
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\${PRIMARY_EXE_NAME}.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\${PRIMARY_EXE_NAME}.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
FunctionEnd

Function LaunchProgram
  ExecShell "" "$INSTDIR\Disable Nvidia Telemetry.exe"
FunctionEnd