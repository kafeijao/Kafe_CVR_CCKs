# NavMeshFollower

Rushed instructions on how to create your own follower!

You can Download the latest NavMeshFollower unity package on
the [latest releases](https://github.com/kafeijao/Kafe_CVR_CCKs/releases/latest)

I suggest starting on an empty project in case something goes wrong. Keep in mind this is still in early development!
Feel free to contribute to these instructions, it would help me a lot :3

There are 3 types, each one increasing in complexity:

- Simple follower that just moves a game object around [section](#Simple-follower)
- Follower with head IK, so it can look at the targets using IK [section](#Follower-with-LookAt-IK)
- Follower with head IK and Arms IK, so it can look and lift the arms to interact with the
  targets. [section](#Follower-with-LookAt-IK-and-VRIK-for-Arm-movement)

## Simple follower

This is the most basic setup, and it only requires a `NavMeshAgent` for the setup.

### Requirements

- [CVR CCK](https://developers.abinteractive.net/cck/setup/)

### Instructions

1. Drag the example CVRChan prefab from `Assets/kafeijao/Followers/CVRChan/CVRChan Example.Prefab` to the root of the
   scene
2. Place your model inside of `CVRChan Example` next to `CVRChan` game object, I'll call it `Custom Model`
3. Make sure the avatar is at position `(0, 0, 0)` inside `Taipan Example`
4. [Optional] Rename `CVRChan Example` to anything you want
5. Copy the `Parent Constraint` constraint setup from `CVRChan` to `Custom Model`, update the value so it
   references itself instead of `CVRChan` on the source with weight `1` and press `Activate`
6. Slot your animator on your model, you can take a look at how `CVRChan_Example_Animator` is made (it's a very simple
   blend tree that uses different animations for idle/walk/run). And in the `CVRSpawnable` update the references on the
   SyncValues to point at your character animator, and pick the correct parameter name to associate.
7. Set the `Culling Mode` on the animator to `Always Animate`, otherwise some stuff won't happen when you're not
   looking at the follower.
8. And to finish adjust the `NavMeshAgent` in `NavMeshAgent` to match your character size, and set the speed,
   stopping distance, etc for the agent. **Note: The `NavMeshAgent` component should be `disabled`! Don't change the
   `Agent Type` (Humanoid), `Base Offset` (0), `Auto Traverse Off Mesh Link` (On), `Auto Repath` (On),
   and `Mask Area` (Everything).


## Follower with LookAt IK

A bit more advanced setup that includes a LookAt IK script from the final IK asset in order for the follower to look at
the targets.

### Requirements

- Head Bone
- [CVR CCK](https://developers.abinteractive.net/cck/setup/)
- FinalIK asset (or stub) *[More Info](#FinalIK)*

### Instructions

1. Drag the example Taipan prefab from `Assets/kafeijao/Followers/Taipan/Taipan Example.Prefab` to the root of the scene
2. Place your model inside of `Taipan Example` next to `Taipan Model`, I'll call it `Custom Model`
3. Make sure the avatar is at position `(0, 0, 0)` inside `Taipan Example`
4. [Optional] Rename `Taipan Example` to anything you want
5. Copy the `Parent Constraint` constraint setup from `Taipan Model` to `Custom Model`, update the value so it
   references itself instead of `Taipan Model` on the source with weight `1` and press `Activate`
6. Copy the `Look At IK` like it's setup on `Taipan Model`, and update the `Head` to your character's Head transform, on
   the `Spine` section you should input the bones you want to move while looking at stuff, do not include the head! On a
   regular humanoid rig would be: `Spine`, `Chest`, `Upper Chest`, `Neck` in that order. If any is missing remove the
   slot (in our example we have `neck.01` and `neck.02`). Also Update the `Eyes` if your character has bones for them (
   this might not work depending on the bone rolls, so you might have to remove them)
7. Move `[NavMeshFollower]/LookAtTarget [Raw]` to the height of your head bone (re-parenting and zeroing can help)
8. Move `[NavMeshFollower]/LookAtTarget [Raw] -> Offset` to the height of the eyes and then invert the y value (this is
   to create the offset for the look at, otherwise the character will be looking too high/low). The y value of the inner
   will almost be a negative value, after aligning with the eye level, just make the y value negative.
9. Slot your animator on your model, you can take a look at how `Taipan_Example_Animator` is made (it's a very simple
   blend tree that uses different animations for idle/walk/run). And in the `CVRSpawnable` update the references on the 
   SyncValues to point at your character animator, and pick the correct parameter name to associate.
10. Set the `Culling Mode` on the animator to `Always Animate`, otherwise some stuff won't happen when you're not
    looking at the follower.
11. Finally go to the root of the prop where the `CVRSpawnable` is, and scroll down to the component `Follower Info`,
    some stuff should be already filled, but you need to update the `Head Transform` to your character's head transform.
    The rest should be fine.
12. And to finish adjust the `NavMeshAgent` in `NavMeshAgent [Raw]` to match your character size, and set the speed,
    stopping distance, etc for the agent. **Note: The `NavMeshAgent` component should be `disabled`! Don't change the
    `Agent Type` (Humanoid), `Base Offset` (0), `Auto Traverse Off Mesh Link` (On), `Auto Repath` (On),
    and `Mask Area` (Everything).


## Follower with LookAt IK and VRIK for Arm movement

This example is a bit complex, it has a lot of hacks like the dampen constraint to keep movement smooth for remote
users. This is because Sub-Sync transforms don't have interpolation, so we need to make our own.

Also since remote users don't have the mod there are some particularities of the setup to keep things working, for
example the need of the nav mesh agent to be disabled by default (so the mod later enables for the person controlling)
and the remote users don't. When objects are suffixed as `[Raw]` it means they are synced over the network, so they need
to be smoothed so other players don't see it jittery (solved by dampening constraints mostly named`[Smooth]`)

Feel free to experiment with it, if you do find easier ways to setup or improve do tell! c: Either way here's the steps
to "quickly" get your custom follower:

### Requirements

- Humanoid Animator Rig
- [CVR CCK](https://developers.abinteractive.net/cck/setup/)
- FinalIK asset (or stub) *[More Info](#FinalIK)*

### Instructions

1. Drag the example kyle prefab from `Assets/kafeijao/Followers/Kyle/Kyle Example.Prefab` to the root of the scene.
2. Place your model inside of `Kyle Example` next to `Robot Kyle`, I'll call it `Custom Model`
3. Make sure the avatar is at position `(0, 0, 0)` inside `Kyle Example`
4. [Optional] Rename `Kyle Example` to anything you want
5. Copy the `Parent Constraint` constraint setup from `Robot Kyle` to `Custom Model` update the value so it
   references itself instead of `Robot Kyle` on the source with weight `1` and press `Activate`
6. Copy the `Look At IK` like it's setup on `Robot Kyle`, and update the `Head` to your character's Head transform, on
   the `Spine` section you should input the bones `Spine`, `Chest`, `Upper Chest`, `Neck` in that order. If any is
   missing remove the slot (kyle doesn't have `Spine` nor `Upper Chest` for example). Also Update the `Eyes` if your
   character has bones for them (this might not work depending on the bone rolls, so you might have to remove them)
7. Move `[NavMeshFollower]/LookAtTarget [Raw]` to the height of your head bone (re-parenting and zeroing can help)
8. Move `[NavMeshFollower]/LookAtTarget [Raw] -> Offset` to the height of the eyes and then invert the y value (this is
   to create the offset for the look at, otherwise the character will be looking too high/low). The y value of the inner
   will almost be a negative value, after aligning with the eye level, just make the y value negative.
9. Inside of `[NavMeshFollower]/VRIK/Scripts` you have 3 game object with VRIK scripts on them. You need to update their
   bone references. The best way to do this is grabbing the VRIK component and drag on the root of your character where
   your animator is, and then if you right click on top of `References` and click on the
   option `Auto-detect References`. This should've updated the references to your avatar bones, now drag it back to the
   object where it was originally. Do this for all 3 GameObjects (this option will only be available if you have FinalIK
   installed in your project (not the stub), otherwise you need to do it manually). **Make sure you keep the VRIK
   objects disabled**!
10. Inside of `[NavMeshFollower]/VRIK/Targets`, go to the `LeftArm [Raw]` and `RightArm [Raw]`, set your character's
    hand transforms as source of the `Parent Constraint`, right click on top of the `Parent Constraint` and click the
    option `Reset`, and now click on the button `Zero`.
11. Slot the animator `Kyle_Example_Animator` in your character Animator Slot (you can expand this animator when you
    know what you're doing). And in the `CVRSpawnable` update the references on the SyncValues to point at your
    character animator, and pick the correct parameter name to associate. **Do NOT put your character animator
    on `VRIK/LeftArm/Weight` and `VRIK/RightArm/Weight`, that one should point to the animator that's
    on `VRIK` GameObject!**
12. Set the `Culling Mode` on the animator to `Always Animate`, otherwise some stuff won't happen when you're not
    looking at the follower.
13. Finally go to the root of the prop where the `CVRSpawnable` is, and scroll down to the component `Follower Info`,
    some stuff should be already filled, but you need to update the `Head Transform` to your character's head transform,
    and the `HumanoidAnimator` to your character's animator. The rest should be fine.
14. Now you need to adjust the `LeftHandAttachmentPoint` and `RightHandAttachmentPoint`, reset the constraints so you
    can move it, and drag where you would like the grabbing point to be for each hand, after that you need to align it
    properly. If you set the tool handle rotation to `Local` the orientation for the GameObject should be blue arrow
    facing forward along your index finger, and the green arrow pointing towards the thumb direction.
15. And to finish adjust the `NavMeshAgent` in `NavMeshAgent [Raw]` to match your character size, and set the speed, 
    stopping distance, etc for the agent. **Note: The `NavMeshAgent` component should be `disabled`! Don't change the
    `Agent Type` (Humanoid), `Base Offset` (0), `Auto Traverse Off Mesh Link` (On), `Auto Repath` (On),
    and `Mask Area` (Everything)


## FinalIK

If you want to take advantage of the look at, or the arm ik you will need FinalIK. If you don't have 
[FinalIK Unity Asset](https://assetstore.unity.com/packages/tools/animation/final-ik-14290) you can
use [FinalIK 1.9 Stub](https://github.com/VRLabs/Final-IK-Stub). But FinalIK is recommended because some steps become
easier and you can even test it in the editor.

---

## Credits

I based my example animator on @NotAKidOnSteam [CCK.BaseAnimatorPatch](https://github.com/NotAKidOnSteam/CCK.BaseAnimatorPatch)

---

## Disclosure

> ---
> ⚠️ **Notice!**  
>
> This mod's developer(s) and the mod itself, along with the respective mod loaders, have no affiliation with ABI!
>
> ---
