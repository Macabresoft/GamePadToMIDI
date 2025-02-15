# Gamepad to MIDI

Allows you to bind gamepad buttons to send specific midi events to a MIDI device.

## Virtual MIDI Device

You will need a virtual MIDI port for which to send events. A DAW (such as *Reaper* or *FL Studio*) can take input from this virtual MIDI port, while *Gamepad to MIDI* will send the events through this port.

In writing this application, I have used [loopMIDI](https://www.tobias-erichsen.de/software/loopmidi.html) to create virtual MIDI ports.