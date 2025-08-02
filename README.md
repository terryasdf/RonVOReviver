# RON VO Reviver

This is a Ready or Not mod tool that greatly simplifies the process of creating packed-up VO mods for LSS Update. The following features are supported:

- Transparent reading and cloning process
- Auto format detection
- Transfer of multi-language subtitle files
- Missing VO type listing
- Auto paking
- English and Simplifed Chinese UI
- ...

Existing mods can be revived and packed within a few clicks. For modders making new VO mods, there is **no need to replace all the VO files** (e.g. 38 variants of yelling at civilian), reducing the whole workload of making a LSS-compatible VO mod.

## Installation

There's no need for configuring your own UnrealPak setup. Here is what you need to do for the VO Reviver to work:

- Download the VO Reviver
- Prepare a set of vanilla VO files matching your mod characters, by downloading from my [Nexus Mods page](https://www.nexusmods.com/readyornot/mods/6324?tab=files), or by manually [extracting your local game files](https://unofficial-modding-guide.com/posts/thebasics/#extracting-game-files).

## How to Use

### 1. Select the folder of the vanilla VO

E.g. if your modded character is `SWATJudge`, the path should be `<SomePath>\SWATJudge`. Note that:

- Character name will be auto-detected (also editable)
- [Zero padding format](#4-check-and-edit-the-character-name-and-other-settings) will be auto-detected (you can view the padding results at the preview section on the middle right)

### 2. Select the folder of your modded VO

E.g. if your modded character is `SWATJudge`, The path should be `<SomePath>\SWATJudge`.

Subtitle files (in format of `sub_*.csv`) in this folder will be loaded. Note that subtitles are **not compulsory** and you may put in whatever languages supported by your mod.

### 3. Edit your .pak name
### 4. Check the Character Name and Zero Padding Setting

Zero padding length determines how many zeros should be placed before the index number.

- \[CALL\]ArrestFemale_**1**.ogg has no zero padding
- HelicopterApproachingLevel_**01**.ogg has zero padding of length 2, since one `'0'` character is added before `'1'`.

### 5. Choose the folder to save your generated files
### 6. Click the `Rivive!` button

- Your packed mod, along with the unpacked version can be found under the same folder. In case that you need to adjust file names, subtitles, etc., you may also click the `Pak Only` button to pak the mod again after modifying in the unpacked folder.

# Conclusion

This is my first time coding in C#. Do create issues and bug reports if you have encountered any issues while using this tool. Thx XD

>p.s. I have no idea why VOID developers have chosen to pak VO files in the LSS update. It helps nothing other than adding up difficulty of creating new VO mods, as well as ruining all of the existing ones. This is a BAD decision if you ask me.👎
