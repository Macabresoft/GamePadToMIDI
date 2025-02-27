# Game Pad to MIDI

Allows you to bind game pad buttons to send specific MIDI events to a MIDI device.

## Virtual MIDI Device

You will need a virtual MIDI port for which to send events. A DAW (such as *Reaper* or *FL Studio*) can take input from this virtual MIDI port, while *Gamepad to MIDI* will send the events through this port.

In writing this application, I have used [loopMIDI](https://www.tobias-erichsen.de/software/loopmidi.html) to create virtual MIDI ports.

## Macabre2D

This is a Macabre2D project and therefore requires the Macabre2D game engine code to run. Follow instructions for setting up and building a Macabre2D project [here](https://github.com/Macabresoft/Macabre2D).

## Fonts and Game Pad Icons

Fonts and game pad icons were created by somepx, support their work:

* [itch.io](https://somepx.itch.io/)
* [patreon](https://www.patreon.com/c/somepx/posts)
* [mastodon](https://mastodon.gamedev.place/@somepx)
* [bluesky](https://bsky.app/profile/somepx.com)

To use these fonts and game pad icons in another project, you will need to purchase them from somepx. They are not included in this repository in their original form.