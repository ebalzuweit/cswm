# Architecture

This document provides a high-level overview of the architecture of cswm.

## Services

cswm is composed of multiple services, that communicate via an internal `MessageBus`.

Important services include:
- `WindowEventRelayService`
- `WindowTrackingService`
- `WindowArrangementService`
- `SystemTrayService`

## Events

cswm uses [event hooks](https://learn.microsoft.com/en-us/windows/win32/winmsg/about-hooks) to track windows being created / destroyed / moved / etc. on the user's desktop.
These events are transformed via `WindowEventRelayService` into cswm `Event` objects, then relayed through the `MessageBus`.

All inter-service communication is handled through subscribing to, and sending events.

## Tracking

In order to manage windows, cswm must determine _which_ windows should be managed - this is internally referred to as **tracking**.

Windows are tracked by the `WindowTrackingService`, which uses a strategy to determine which windows should be tracked.

The only window tracking strategy at this time is the `DefaultWindowTrackingStrategy`.

## Arrangement

Window arrangement is handled by the `WindowArrangementService`, which uses an `IArrangementStrategy` to determine how the windows are positioned in a space.

There are two arrangement strategies currently:
- `SilentArrangementStrategy` - no arrangement
- `SplitArrangementStrategy` - [binary space partitioning](https://en.wikipedia.org/wiki/Binary_space_partitioning), with a twist

## User Interaction

Users interact with cswm via the notification tray.
`SystemTrayService` and `SystemTrayMenu` are responsible for the notification icon and any user interactions.