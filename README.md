# 📂 Backy --- Game Save Backup Manager

Backy is a simple **WPF desktop app** built with **.NET 8** that helps
you manage and back up your game save files.\
It's designed for gamers who want to **safeguard progress** by quickly
copying saves into a chosen backup location.

------------------------------------------------------------------------

## ✨ Features

-   🎮 **Manage your games**\
    Add, list, and remove games with their save locations.

-   💾 **Backup saves with one click**\
    Automatically copy your game save folders into a secure backup
    directory.

-   🔒 **Handles restricted folders**\
    Prompts for admin rights when required.

-   🗑 **Safe deletion**\
    Optionally delete a game and its backup from the store.

-   📌 **Backup indicator**\
    Shows whether a game has been backed up.

-   ⚡ **Persistent settings**\
    Store location and game list are saved across restarts.

------------------------------------------------------------------------

## 🚀 Getting Started

### Prerequisites

-   Windows 10/11\
-   [.NET 8
    Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
    installed

### Installation

1.  Download the latest release from [Releases](../../releases).\
2.  Run the **Backy Setup.exe** installer.\
3.  Launch **Backy** from Start Menu or Desktop shortcut.

------------------------------------------------------------------------

## 🖥 Usage

1.  Click **Store** to choose your global backup folder (e.g.,
    `D:\Backups`).\
2.  Add a game:
    -   Name: `Skyrim`
    -   Save Location: `C:\Users\You\Documents\My Games\Skyrim\Saves`\
3.  Press **Backup** → your saves are copied into the store.\
    Example: `D:\Backups\Skyrim\`\
4.  Delete games if you don't need them anymore (optionally delete the
    stored backup too).

------------------------------------------------------------------------

## 📸 Screenshots

*(Add your screenshots here --- UI, backup confirmation, etc.)*

------------------------------------------------------------------------

## ⚙️ Build from Source

1.  Clone the repo:

    ``` bash
    git clone https://github.com/yourusername/Backy.git
    cd Backy
    ```

2.  Open in Visual Studio 2022+\

3.  Set configuration to **Release**\

4.  Publish with:

    ``` powershell
    dotnet publish -c Release -r win-x64 --self-contained true
    ```

5.  Package into installer using **Inno Setup**.

------------------------------------------------------------------------

## 📦 Packaging (Inno Setup Example)

``` ini
[Setup]
AppName=Backy
AppVersion=1.0
DefaultDirName={pf}\Backy

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\Backy"; Filename: "{app}\Backy.exe"
Name: "{commondesktop}\Backy"; Filename: "{app}\Backy.exe"
```

------------------------------------------------------------------------

## 🤝 Contributing

Contributions are welcome!\
Feel free to fork, open issues, and submit pull requests.

------------------------------------------------------------------------

## 📜 License

This project is licensed under the **MIT License**.\
See [LICENSE](LICENSE) for details.
