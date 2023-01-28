# Kafe's CVR CCK Mods

Welcome to my little collection of CCK mods for CVR, these will probably be CCK scripts used by my mods in game!

---

## In-Depth Mods info Links:

- [EyeMovementFix](EyeMovementFix) *in-depth url*

---

## Small Descriptions:  

### Eye Movement Fix

Setup the eye rotation limits on your avatar. Only people with
the [EyeMovementFix](https://github.com/kafeijao/Kafe_CVR_Mods/tree/master/EyeMovementFix) mod will see those limits
enforced!

Check [Eye Movement Fix In-Dept](EyeMovementFix) for more info.

---

## Building

In order to build this project follow the instructions (thanks [@Daky](https://github.com/dakyneko)):

- (1) Install NStrip.exe from https://github.com/BepInEx/NStrip into this directory (or into your PATH). This tools
  converts all assembly symbols to public ones. Make life easy!
- (2) Create a new Windows environment variable `CVRPATH` which should point to your game path (folder
  where `ChilloutVR.exe` resides). In Windows, look for Settings > Advanced system settings > Advanced > Environment
  Variables, add a new one there, it should point to something
  like `C:\Program Files (x86)\Steam\steamapps\common\ChilloutVR` or similar.
- (3) Run `copy_and_nstrip_dll.bat` (cmd prompt only) or `copy_and_nstrip_dll.ps1` (Powershell only) this will copy the
game + MelonLoader .dll into this project and run NStrip.exe to make them public (easier developers).
- (4) Copy `UnityEditor.dll` from your Unity Editor folder `...\2019.4.31f1\Editor\Data\Managed\UnityEditor.dll` into
  the `ManagedLibs` folder in this project's root.


---

# Disclosure  

> ---
> ⚠️ **Notice!**  
>
> This mod's developer(s) and the mod itself, along with the respective mod loaders, have no affiliation with ABI!
>
> ---
