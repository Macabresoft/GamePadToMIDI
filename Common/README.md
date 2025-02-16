# Common

This solution contains important settings to be used by both Macabre2D.Framework and Macabre2D.Project.Gameplay. There are several files this project that can be edited, but should not be deleted.

## Configuration

### CustomSettings

This is the place to define any settings that the player is allowed to change. It will be saved during runtime in the same file as input bindings and display settings.

### PredefinedColors

A convenient place to store common colors used in a project. The public property `Colors` is used by the Macabre2D editor to create a selection palette for color editors and for choosing the background displayed when editing assets.

### ProjectConfiguration

Any additional information to be stored inside `GameProject`. These are saved when content is built and cannot be edited by a player.

## Enums

### InputAction

Input actions define the various actions a user can perform when using an input device. These will generically tie an action to an input, allowing gameplay code to be input agnostic.

Do not delete the InputAction enum.

### Layers

Layers are a core component of Macabre2D. Both the rendering and physics of Macabre2D use bitwise operations on the Layers enum. This enum is serialized as a numeric value, so feel free to rename the layers per project. 

Do not delete the Layers enum.

## Other

### SaveManager

A class to which `BaseGame` provides access. Not every game will have the same save systems, so there is no general save system provided by Macabre2D. Instead, this class should be constructed and used on a game by game basis.