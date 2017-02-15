# Citrus
![](Orange/Logo.png)

## Cooking Rules

- Bool is `Yes` or `No`
- DDS Format is `DXTi` - DXTi, `ARGB8` `RGBA8` - Uncompressed
- PVR Format is `PVRTC4`, `PVRTC4_Forced`, `PVRTC2`, `RGBA4`, `RGB565`, `ARGB8`, `RGBA8`
- `AtlasOptimization` is `Memory`, `DrawCalls`
- `ModelCompressing` is `Deflate`, `LZMA`
- `TextureAtlas` : `None` (null), `${DirectoryName}`, `<custom_name>`
- `MipMaps` : `Yes` or `No`
- `HighQualityCompression` : `Yes`, `No`
- `PVRFormat` : ...
- `DDSFormat` : ...
- `Bundle` : `<default>`, `data` (CookingRules.MainBundleName), `anyotherbundlename`
- `Ignore` : `Yes`, `No`
- `ADPCMLimit` : int
- `TextureScaleFactor` : float
- `AtlasPacker` : `string` custom packer
- `ModelCompressing` : ...

### Format

Target is being parsed from cooking rules. Only cooking rules marked with same target that's being built now apply. Target is specified from `citproj` file.

If line is starting with `[` then it's target e.g.

[Target1]
....
[Tatget2]
....

Each line besides target MUST consist of two space separated strings.
word1 word2
if word1 is like word1(platform) then platform applies only to current platform



