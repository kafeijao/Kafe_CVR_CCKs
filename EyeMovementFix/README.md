# EyeMovementFix

This CCK Mod component allows you to define eye rotation limits on your avatars.

## Installation & Usage

1. Download and import `EyeMovementFixCCKMod.unitypackage` 
   from the [latest releases](https://github.com/kafeijao/Kafe_CVR_CCKs/releases/latest) into your unity project.
2. Go to the root of your avatar (where the `CVR Avatar` script is located) and add the component `Eye Rotation Limits`.
3. You will get presented with something like the following image:
   ![](EyeRotationLimitsScript.png)
4. You can configure the limits for the eye angles, and click the `Preview Eye Rotations` and play with the sliders to
   see what it will look on your avatar.
5. After you got values that you're satisfied with, you can upload the avatar (don't remove the script from the avatar).

The limits should be:
- Box on the `top` to the max angle looking up
- Box on the `left` is the max angle looking horizontally inwards (direction of the nose)
- Box on the `right` is the max angle looking horizontally outwards (in direction opposite of the nose)
- Box on the `bottom` is the max angle looking down

## Notes

1. The preview will rotate your eye bones, and it should restore the rotations when you stop previewing. But if you want
   to be entirely safe, create a duplicate of your avatar to do the
   previewing.
2. The limits will only be enforced if the person seeing your avatar has
   the [EyeMovementFix](https://github.com/kafeijao/Kafe_CVR_Mods/tree/master/EyeMovementFix) mod installed.

---

# Disclosure

> ---
> ⚠️ **Notice!**
>
> This project is an independent creation and is not affiliated with, supported by, or approved by Alpha Blend
> Interactive
>
> ---
