# Cooking Rules

Asset cooking options are set and propagated using cooking rules files. Cooking rules files could be named in two ways:
1. `#CookingRules.txt`
2. `<asset_filename_with_extension>.txt` e.g. `illuminator.png.txt`

In first case cooking rules are applied to all files and directories in current directory recursively until overridden with another cooking rules.

In second case cooking rules are only applied to specified asset file.
Cooking rules only override concrete rule lines specified.

## Format

grammar:

```
rule_list: one of
    rule '\n' rule_list
    rule

rule: rule_name ['(' target_name ')'] ' ' rule_value '\n'

target_name: one of
    'Win'
    'Mac'
    'iOS'
    'Android'
    <user_defined_in_citproj_file>

rule_name: one of
    'TextureAtlas'
    'MipMaps'
    'HighQualityCompression'
    'PVRFormat'
    'DDSFormat'
    'Bundles'
    'Ignore'
    'ADPCMLimit'
    'TextureScaleFactor'
    'AtlasOptimization'
    'AtlasPacker'
    'ModelCompression'
    'CustomRule'
    'WrapMode'
    'MinFilter'
    'MagFilter'
```

e.g.:
```
Rule1(Target1) Value
Rule2(Target1) Value
Rule2(Target1) Value
Rule3 Value
...
Rule1(Target2) Value
Rule10 Value
...
```

There's a list of default targets, which are Win, Mac, iOS and Android. Other targets are listed in `.citproj` project file.

## Rules

'TextureAtlas'
'MipMaps'
'HighQualityCompression'
'PVRFormat'
'DDSFormat'
'Bundles'
'Ignore'
'ADPCMLimit'
'TextureScaleFactor'
'AtlasOptimization'
'AtlasPacker'
'ModelCompression'
'CustomRule'
'WrapMode'
'MinFilter'
'MagFilter'

| rule                     | values              | description  |
| ------------------------ | ------------------- | ------------ |
| `DDSFormat`              | `DXTi`              | DXTi         |
|                          | `ARGB8`, `RGBA8`    | Uncompressed |
| `PVRFormat`              | `PVRTC4`            | Falls back to PVRTC2 if image has no alpha |
|                          | `PVRTC4_Forced`     | |
|                          | `PVRTC2`            | |
|                          | `RGBA4`             | |
|                          | `RGB565`            | |
|                          | `ARGB8`             | |
|                          | `RGBA8`             | |
| `AtlasOptimization`      | `Memory`            | Default; best pack rate heuristics |
|                          | `DrawCalls`         | try to fit as many items to atlas as possible |
| `ModelCompression`       | `Deflate`           | |
|                          | `LZMA`              | |
| `TextureAtlas`           | `None`              | |
|                          | `${DirectoryName}`  | atlas name will be the same as directory name |
|                          | `<atlas_name>`      | user defined atlas |
| `MipMaps`                | `Yes` or `No`       | doesn't work |
| `HighQualityCompression` | `Yes` or `No`       | |
| `Bundles`                | `<default>`, `data` | main bundle |
|                          | `<bundle_name>`     | user defined bundle name; it's possible to specify multiple bundles; it's possible to include directory name e.g. `Bundles/Restaurant` |
| `Ignore`                 | `Yes`, `No`         | if set to `Yes` applicable assets won't make it to bundle |
| `ADPCMLimit`             | int                 | |
| `TextureScaleFactor`     | float               | designed to be texture size multiplier. however if it's not 1.0f texture size multiplied by 0.75 with a mix of some logic. see code for detail. |
| `AtlasPacker`            | string              | custom packer defined via plugin |
| `CustomRule`             | string              | any string
| `WrapMode`               | `Clamp`             | texture wrap mode, default is `Clamp`
|                          | `Repeat`            |
|                          | `MirroredRepeat`    |
| `MinFilter`              | `Linear`            | texture min filter, default is `Linear`
|                          | `Nearest`           |
| `MagFilter`              | `Linear`            | texture mag filter, default is `Linear`
|                          | `Nearest`           |

