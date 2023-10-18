![](./src/cswm/icon.ico)

# cswm

_/ swɪm /_

A simple window manager in C#.

## Installation <sub><sup>_(No installation required)_</sup></sub>

> **Note** Windows-only.

Grab the latest release from [here](https://github.com/ebalzuweit/cswm/releases/latest).

Unzip the folder and run `cswm.exe`.

### Uninstalling

Delete the folder containing `cswm.exe` and its contents.

## Usage

cswm is designed to be straightforward -
there are no keybindings and little user configuration.

While running, cswm automatically manages active windows on the user's desktop.
It will arrange the active windows like a tiling window manager,
moving and resizing them to fill the screen without overlapping.
You can rearrange windows by dragging them from one space to another.

cswm runs in the system tray, right-click on the icon to access the menu.

### Configuration

Configuration is done through the `appsettings.json` file, found in the same folder as `cswm.exe`.

| Setting         | Description                                      |
| --------------- | ------------------------------------------------ |
| Monitor Padding | Border on the inside of each monitor, in pixels. |
| Window Margin   | Space between adjacent windows, in pixels.       |

### Caveats

- Some windows (e.g. Task Manager) require cswm to be run as an administrator to be managed.

## Acknowledgments

- [GlazeWM](https://github.com/lars-berger/GlazeWM)
- [PInvoke.net](https://www.pinvoke.net/index.aspx)
- [fts_winsnap](https://github.com/forrestthewoods/fts_winsnap)
