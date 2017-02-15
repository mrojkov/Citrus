# Citrus
![](Orange/Logo.png)

## Cooking Rules

Asset cooking options are set and propagated using cooking rules files. Cooking rules files could be named in two ways:
1. `#CookingRules.txt`
2. `<asset_filename_with_extension>.txt` e.g. `illuminator.png.txt`

In first case cooking rules are applied to all files and directories in current directory recursively until overridden with another cooking rules.

In second case cooking rules are only applied to specified asset file.
Cooking rules only override concrete rule lines specified.

Second way doesn't work for `.txt` files. Gummy Drop has workaround for this, ask Buzer for details.

### Format

grammar:

```
cooking_rules_file: one of
    target_line
    rule_line

target_line: '[' <target_name> ']' '\n'

rule_line: one of
    rule_id ' ' <rule_value>
    rule_id '(' platform_name ')' ' ' <rule_value> '\n'

platform_name: one of
    'Win'
    'Mac'
    'iOS'
    'Android'
    'Unity'

rule_id: one of
    'TextureAtlas'
    'MipMaps'
    'HighQualityCompression'
    'PVRFormat'
    'DDSFormat'
    'Bundle'
    'Ignore'
    'ADPCMLimit'
    'TextureScaleFactor'
    'AtlasOptimization'
    'AtlasPacker'
    'ModelCompressing'
```

e.g.:
```
[Target1]
Rule1(Platform1) Value
Rule2(Platform2) Value
Rule2(Platform1) Value
Rule3 Value
...
[Target2]
Rule1(Platform1) Value
Rule10 Value
...
```

Targets are listed in `.citproj` project file. Only current target's rules are being used. Targets act as sections in cooking rules file.

Rules can be optionally marked with platform identifier to specify to which platform the rule applies to.

### Rules

'TextureAtlas'
'MipMaps'
'HighQualityCompression'
'PVRFormat'
''
'Bundle'
'Ignore'
'ADPCMLimit'
'TextureScaleFactor'
'AtlasOptimization'
'AtlasPacker'
'ModelCompressing'

| rule                | values           | description  |
| -----------         | ---------------- | ------------ |
| `DDSFormat`         | `DXTi`           | DXTi         |
|                     | `ARGB8`, `RGBA8` | Uncompressed |
| `PVRFormat`         | `PVRTC4`         | Fallbacks to PVRTC2 if image has no alpha |
|                     | `PVRTC4_Forced`  | |
|                     | `PVRTC2`         | |
|                     | `RGBA4`          | |
|                     | `RGB565`         | |
|                     | `ARGB8`          | |
|                     | `RGBA8`          | |
| `AtlasOptimization` | `Memory` | Default |
|                     | `DrawCalls` | |
| `ModelCompressing`  | `Deflate` | |
|                     | `LZMA` | |
| `TextureAtlas`      | `None` | |
|                     | `${DirectoryName}` | |
|                     | `<custom_name>` | |

`MipMaps` : `Yes` or `No`
`HighQualityCompression` : `Yes`, `No`
`PVRFormat` : ...
`DDSFormat` : ...
`Bundle` : `<default>`, `data` (CookingRules.MainBundleName), `anyotherbundlename`
`Ignore` : `Yes`, `No`
`ADPCMLimit` : int
`TextureScaleFactor` : float
`AtlasPacker` : `string` custom packer
`ModelCompressing` : ...


