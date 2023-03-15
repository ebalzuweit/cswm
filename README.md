![](./src/cswm/icon.ico)

# cswm

_/ swɪm /_

A simple window manager in C#.

## Installation <sub><sup>_(No installation required)_</sup></sub>

Grab the latest release from [here](https://github.com/ebalzuweit/cswm/releases/latest).

Unzip the folder and run `cswm.exe`.

### Uninstalling

Delete the folder containing `cswm.exe` and its contents.

## Usage

While running, cswm automatically manages active windows on the user's desktop.
It will arrange the active windows like a tiling window manager,
moving and resizing them to fill the screen without overlapping.

cswm is designed to be straightforward -
there are no keybindings and little user configuration.

### Configuration

Configuration is done through the `appsettings.json` file, found in the same folder as `cswm.exe`.

#### Margin

Border between active, non-fullscreen windows - in pixels.

#### Padding

Border on the outside of all active, non-fullscreen windows - in pixels.

## Acknowledgments

- [GlazeWM](https://github.com/lars-berger/GlazeWM)
- [PInvoke.net](https://www.pinvoke.net/index.aspx)
- [fts_winsnap](https://github.com/forrestthewoods/fts_winsnap)
