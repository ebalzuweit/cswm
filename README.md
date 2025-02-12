![cswm icon](./docs/cswm_icon.png)

# cswm

_/ swɪm /_

A simple tiling window manager in C#

## Installation <sub><sup>_(No installation required)_</sup></sub>

> [!NOTE]
> cswm only supports Windows.

Download the latest release from [here](https://github.com/ebalzuweit/cswm/releases/latest).

Unzip the folder and double-click `cswm.exe`.

### Uninstalling

Delete the folder containing `cswm.exe` and its contents.

## Usage

cswm is designed to be straightforward -
there are no keybindings and little user configuration.

While running, cswm automatically manages active windows on the user's desktop.
It will arrange the active windows like a tiling window manager,
moving and resizing them to fill the screen without overlapping.
You can rearrange windows by dragging them from one space to another.
You can adjust the tiling by resizing the window border (or corner) adjacent to the partition.

cswm runs in the system tray, right-click on the icon to access the menu.

Via the menu, you can:

- Choose the arrangement strategy for a monitor

  - Silent - No arrangement
  - Split - [Binary space partitioning](https://en.wikipedia.org/wiki/Binary_space_partitioning), with a twist

- Flag windows - Flagging a window makes it float on top of other windows, and enables custom resizing.

### Configuration

Configuration is done through the `appsettings.json` file, found in the same directory as `cswm.exe`.

| Setting         | Description                                      |
| --------------- | ------------------------------------------------ |
| Monitor Padding | Border on the inside of each monitor, in pixels. |
| Window Margin   | Space between adjacent windows, in pixels.       |

### Caveats

- In order to manage windows running in Administrator mode (e.g. Task Manager), cswm must also be running in Administrator mode.
