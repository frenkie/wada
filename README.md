# We Are Dead Animals (WADA)

## Installation

We Are Dead Animals (WADA) is a VR project build with Unity and aiming
to be experienced on the Oculus Quest 2 while using your hands as controllers.

### Git
WADA uses the 'main' branch for developing and feature branches for testing
and developing big changes. WADA uses a Semver release branching strategy of
which you can find the details [further down this document](#release-strategy).

### Unity
WADA is currently build in Unity 2022.1.23f1 and uses the Universal Render
Pipeline.
The build target is Android, so make sure that is set in Unity. In the Build
Settings for the Android target Texture Compression is set to 'ASTC'.
Some notable Player settings under 'Other Settings':
- Color Space: Linear
- Vulcan Graphics API
- Minimum API Level: Android 6.0
- Target API Level: Automatic
- Scripting Backend: IL2CPP
- Api Compatibility Level: .NET
- Target Architecture: check off ARM64
- Active Input Handling: Input System Package (New)

### Running on the Oculus Quest 2

#### Unity
To preview the work in Unity on an Oculus Quest the XR Plug-in Management has
the Oculus plugin checked for both the Android as well as PC settings tab.
Oculus plugin settings for the PC tab are Single Pass Instanced and all other
boxes checked.
Oculus plugin settings for the Android tab should at least be Multi Pass for
Stereo Rendering Mode.

Connect the Oculus Quest with the link cable to your PC and start Oculus Link
in the headset. On your PC the Oculus app should be started too.

#### Build
Make sure that in the Build Settings the correct scenes are checked.

##### Quest install
We build an .APK file and the result can be uploaded to your Oculus Quest with 
the SideQuest app and a Quest in developer mode.


## Scene hierarchy and naming conventions
Scenes are in the `/Scenes` folder and should adhere to the following
object hierarchy.


...


### Naming conventions
Every object should preferably be in English, using
Capitals for each word and spaces between words.

### Timelines
The timelines are the main drivers for our scenes, including audio timing,
so it's a place to begin with if something needs to happen in the scene.
We are also using the Signals feature of Unity to drive specific time based
script triggers.

## Folder and file setup

Preferably scenes and their settings (e.g. lighting, volumes) can be found
under `/Scenes`.
Scripts, models, materials and textures belonging to the same component
should preferably have their own component folder with subfolders for
the separate entities.

Example Setup has been added to `/WADA Components`:

![image of the Folder hierarchy setup](./ReadmeAssets/Wada-Components.jpg "Folder Hierarchy")

Where a `Dev` folder contains the asset setup for the rough development of a component.
When code is good enough, move it to the root folders.
When refactoring code make sure to move it to this folder setup. When in doubt
about the right kind of component structure discuss it with other team members.

### Naming conventions
Folder names and objects should preferably be in English. Folder names
that we create ourselves use Capitals for each word and spaces between words.

### Script Style Guide
We do not yet have an automated style guide checker configured so we have mixed
styles.
The convention is:
- Public variables use PascalCase
- Private variables use camelCase and do not need to be declared with the 'private'
  keyword
- Methods always use PascalCase, whether public or private

Preferably, code in the `WADA Component` structure is namespaced with `Wada`
so we have less possibility of clashing Class names with 3rd party code.
It also makes our code more easily exportable and reusable.

<a id="release-strategy"></a>
## Release strategy
We use Release branches with [Semver](https://semver.org/) notation for the project.
Every Release branch has the Major.Minor naming convention, from which
Major.Minor.Patch versions are build and Git tagged. The meaning of Major, Minor
and Patch is explained in the above linked page.

### Versioning
The semver version is maintained in the Unity project 'Player' settings, so be sure
to adjust the settings when needed.

![Image of the title Versioning](./ReadmeAssets/ReleaseTitleVersioning.png "Release title versioning")

Under other settings:

![Image of the Versioning](./ReadmeAssets/ReleaseVersioning.png "Release versioning")

The bundle version is the Major.Minor.Patch version where Minor and Patch are represented
with 2 digits if Major is >= 1. In this case 01 and 00, so we have room for 99 Minors and Patches,
which should be enough.
The reason is that Bundle Version Codes should always be higher in a newer version,
so a raise in Patch above 9 wil keep the same amount of numbers in this code when
Minor is still lower than 10. A version of 1.6.0 will then be 10600 which is higher
than 1.5.12 which is 10512, whereas without those 2 digits 160 would be lower than
1512 and lead to a deployment error of 1.6.0 on the Quest.

### Release Flow
Starting from the main branch. The version in the main branch should always be an
x.y.0 version, at least one x or y higher than the last release.

#### Releasing - release branch
When we're ready for a release we create a release branch from the main, called
'release-{Major}.{Minor}'. We create an APK build and test it in its entirety
on the Quest, to make sure there are no bugs left. If so, fix them in this
new branch. When everything is set, commit and push this branch and distribute
the APK + volcaps to the right share location.
If you made any changes to this branch after the initial creation, be sure to
port those back to the main branch.
From now on, this release branch will be the branch we use when we want to fix
a bug in this version.
After releasing a patch version, update this release branch's project settings
to version `{major}.{minor}.{patch+1}-dev` so we instantly know which patch will be
the next one.

#### Releasing - git tag
We can create a git tag so we always have the code for a specific
{Major}.{Minor}.{Patch} version if ever we need to reproduce a build.
You can do this by running `git tag v{Major}.{Minor}.{Patch} -m "Version {Major}.{Minor}.{Patch}"`
in the release branch or do it in your Git software.
For version 1.5.2 this would become `git tag v1.5.2 -m "Version 1.5.2"`

#### Releasing - main branch update
Port any changes you might have done in the release branch after the initial
creation back into the main branch. Go into Unity and change the version by
upping either the Major or the Minor version. In most cases just the Minor,
unless you know you're going to work on a backwards incompatible change.
When creating a new release branch later from this updated main you can
always adjust either the Major or Minor number according to what's needed.

### Bugfix Flow
When a bug has been found in one of the releases and we want to create an updated
version of the build to deploy, we now have a clean release branch to fix
the bug in. Of course we also have to check the main branch whether the bug
is still there.

#### Is the bugfix needed asap in an existing release?
Usually the flow is then as follows:
- Check out the release branch
- Reproduce the bug in the release branch
- Fix the bug
- Create a new APK build
- Test it and make sure everything works
- Remove `-dev` as an appendix
- Commit and push this update in the release branch
- Create a git tag for this new patch version commit
- Up the patch version one higher and add `-dev` as appendix, plus commit this to the remote
- Upload the patch APK version to the release branch share locations (Google Drive?)
- Port the fixes back to the main branch if needed (probably manually to be wary of
  overwriting new developments)

#### Can we fix the bug in an upcoming release?
The flow can be:
- Reproduce the bug in the main branch
- If existing, then fix it
- Leave the versioning as is

### Test builds
It's always okay to create a build from the main branch and deploy it, but the
difference with release branches should be that release branches only contain
working scenes and approved features.

### Release deployment on Google Drive
Release builds are posted in the Google Drive including a readme, Windows batch file
and screenshots on how to automatically or manually install the release. We don't
keep every release build in the Google Drive, just the last -3 release versions, because
older ones can potentially be rebuilt from their release branch/tag.

It's a good idea to include the version number in the APK, but make sure the Update
script and/or README files mirror this version as well


## Versions
If we want to get real serious, we could keep a log here of the versions that
have been released and what has been fixed/created in them. Even more beautiful
if we can link it to issues on a todo list.


## 0.3
Contains the swim hands where fingers point the way with forward/initial thrust or > speed limit
makes the move. Also has the upper river flow path turned off, so it's free roaming.