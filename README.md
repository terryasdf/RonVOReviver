# RON VO Reviver

This is a Ready or Not mod tool that greatly simplifies the process of creating packed-up VO mods for LSS Update. The following features are supported:

- Transparent reading and cloning process
- Auto audio format conversion to .ogg (quality=6)
- Auto naming format detection
- Transfer of multi-language subtitle files
- Missing VO type listing
- Auto paking (**unpaked version also available for creating post-LSS mods**)
- English and Simplifed Chinese UI
- ...

Existing mods can be revived and packed within a few clicks. For modders making new VO mods, **this tool also fills extra variants by copy existing ones from the mod files**. (e.g. there are 38 variants of yelling at civilian VO in vanilla game but only 10 from your mod, this tool copy & paste the 10 VOs to the count of 38)

## Installation

There's no need for configuring your own UnrealPak setup. Here is what you need to do for the VO Reviver to work:

- Download the VO Reviver
- ~~Prepare a set of vanilla VO files matching your mod characters, by downloading from my [Nexus Mods page](https://www.nexusmods.com/readyornot/mods/6324?tab=files), or by manually [extracting your local game files](https://unofficial-modding-guide.com/posts/thebasics/#extracting-game-files).~~

## How to Use

### 1. Select vanilla VO character

There's **no need** to prepare the full set of vanilla VO files. Simply choose the desired character from the drop-down menu and you will see the full list of original VO filenames.

### 2. Select the folder of your modded VO

E.g. if your modded character is `SWATJudge`, The path should be `<SomePath>\SWATJudge`. The list view below will show all valid VO files after selecting the folder.

Subtitle files (in format of `sub_*.csv`) in this folder will also be loaded. Note that subtitles are **not compulsory** and you may put in whatever languages supported by your mod.

### 3. Edit your .pak name
### 4. Check the Character Name
### 5. Choose the folder to save your generated files
### 6. Click the `Revive!` button

- Your packed mod, along with the unpacked version can be found under the same folder. In case that you need to adjust file names, subtitles, etc., you may also click the `Pak Only` button to pak the mod again after modifying in the unpacked folder.

# Conclusion

This is my first time coding in C#. Do create issues and bug reports if you have encountered any issues while using this tool. Thx XD
