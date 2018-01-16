# Yuzu reference

  * [Item attributes](#item-attributes)
  * [Method attributes](#method-attributes)
  * [Class attributes](#class-attributes)
  * [Options](#options)

## Item attributes

Item attributes can be applied to either a public field or a public readable property.
The item is considered for serialization and deserialization if it is annotated with exacty one of (`[YuzuRequired]`, `[YuzuOptional]` and `[YuzuMember]`) attributes.

#### `[YuzuRequired]` or  `[YuzuRequired("alias")]`
Denotes required item. This item is always serialized. An exception is thrown if the item is absent during deserialization. If `alias` is provided, it is used instead of the item name both for serialization and deserialization.
Mutually exclusive with `[YuzuOptional]` and `[YuzuMember]`.

Can be substituted by changing `MetaOptions.RequiredAttribute`.

#### `[YuzuOptional]` or  `[YuzuOptional("alias")]`
Denotes optional item. This item may be omitted during serialization by using `YuzuSerializeIf` attribute. If the item is absent during deserialization, its value in the target object is left unchanged. If `alias` is provided, it is used instead of the item name both for serialization and deserialization.
Mutually exclusive with `[YuzuRequired]` and `[YuzuMember]`.

Can be substituted by changing `MetaOptions.OptionalAttribute`.

#### `[YuzuMember]` or  `[YuzuMember("alias")]`
Denotes optional item with the default falue. Immediately before serialization of the scalar item, item value is compared with the default value of the item's type. If the item is `ICollection`, it is checked for emptiness instead. If the item is absent during deserialization, its value in the target object is left unchanged.
If `alias` is provided, it is used instead of the item name both for serialization and deserialization.
Mutually exclusive with `[YuzuRequired]` and `[YuzuOptional]`.

Can be substituted by changing `MetaOptions.MemberAttribute`.

#### `[YuzuSerializeIf("conditionFunc")]`
#### `public bool conditionFunc() { ... }`
Denotes serialization condition. Can only be applied to `YuzuOptional` item. The argument must be a name of boolean function without arguments, member of the current class. Immediately before serialization of the item, this function is called. If the function returns `true`, the item is serialized, otherwise the item is omitted.

Can be substituted by changing `MetaOptions.SerializeIfAttribute`.

#### `[YuzuDefault(defValue)]`
Denotes default value for serialization. Can only be applied to `YuzuOptional` item. Immediately before serialization of the item, item value is compared with `defValue`. If they are equal, the item is omitted, otherwise the item is serialized.

#### `[YuzuMerge]`
Denotes that the deserialized value must be merged with the original item value instead of replacing it. Can only be applied to items of structured types: `class`, `struct`, `interface` or `object`.
When deserializing an item of structured type without merging, a new object is constructed, then sub-item values are deserialized into this new object, and finally new object if assigned to the item of the containing object.
When deserializing an item of structured type with merging, sub-item values are deserialized into the existing object. If some of the sub-items are omitted, previous values are retained.

Can be substituted by changing `MetaOptions.MergeAttribute`.

#### `[YuzuCompact]`
Selects more compact serialized representation at the expense of backward and forward compatibility.
This attrubute can be used for increasing serialization and deserialization speed as well as readablity of text-based formats. The downside is that changing `YuzuCompact` item will break compatibility with both old and new versions of the serialized data.

Can be substituted by changing `MetaOptions.CompactAttribute`.

#### `[YuzuExclude]`
Denotes that an item must not be serialized and deserialized despite the presence of `YuzuAll` attribute on the containing class.

Can be substituted by changing `MetaOptions.ExcludeAttribute`.

## Method attributes

#### `[YuzuBeforeSerialization]`
Denotes a `void` method without arguments, which will be called immediately before this object is serialized. If there are several `[YuzuBeforeSerialization]` methods, first methods of the current class are called in the order of source code definition, then methods from the parent class are called. This order is opposite of `[YuzuAfterDeserialization]`.

Can be substituted by changing `MetaOptions.BeforeSerializationAttribute`.

#### `[YuzuAfterDeserialization]`
Denotes a `void` method without arguments, which will be called immediately after this object is deserialized. If there are several `[YuzuAfterDeserialization]` methods, first methods from the parent class are called, then methods  of the current class are called in the order of source code definition. This order is opposite of `[YuzuBeforeSerialization]`.

Can be substituted by changing `MetaOptions.YuzuAfterDeserializationAttribute`.

## Class attributes

#### `[YuzuCompact]`
Selects more compact serialized representation at the expense of backward and forward compatibility.
This attrubute can be used for increasing serialization and deserialization speed as well as readablity of text-based formats. The downside is that changing `YuzuCompact` class will break compatibility with both old and new versions of the serialized data.

Can be substituted by changing `MetaOptions.CompactAttribute`.

#### `[YuzuMust]` or `[YuzuMust(itemKind)]`
Denotes that all items must be serialized. Exception is thrown if at least one public item lacks serialization attribute. If present, `itemKind` argument limits the requirement to either just fields (`YuzuItemKind.Field`) or just properties (`YuzuItemKind.Property`).

Can be substituted by changing `MetaOptions.MustAttribute`.

#### `[YuzuAll]` or `[YuzuAll(optionality, itemKind)]`
Denotes that all public items are serialized by default, even if not annotated by serialization attribute. Some items can be excluded by using `YuzuExclude` attrubute. If present, `optionality` argument indicates the level of optionality (`YuzuItemOptionality.Optional`, `YuzuItemOptionality.Required` or `YuzuItemOptionality.Member`) applied to the items by default. Annotating an item with `[YuzuRequired]`, `[YuzuOptional]` or `[YuzuMember]` attribute overrides default given by `YuzuAll`. If present, `itemKind` argument limits the default serialization to either fields (`YuzuItemKind.Field`) or properties (`YuzuItemKind.Property`). Using both `YuzuAll` and `YuzuMust` simultaneously prohibits `YuzuExclude`.

Can be substituted by changing `MetaOptions.AllAttribute`.

#### `[YuzuAllowReadingFromAncestor]`
Normally, an item of structured type can be deserialized if the serialized item is of the same class or descendant class. This attribute allows deserializing from ancestor class, as long as all fields not present in the ancestor are optional. It is NOT recommended to use this attribute.

Can be substituted by changing `MetaOptions.AllowReadingFromAncestorAttribute`.

#### `[YuzuAlias("alias")]` or `[YuzuAlias(read: readAliasList, write: writeAlias)]`
Denotes that during serializarion, `writeAlias` is used instead of class name, and during deserialization any of the given read aliases plus original class name is can be used for this class. All read aliases must be globally unique between all classes. Is the single-argument form is used, it defined both write alias and  a single read alias.

Can be substituted by changing `MetaOptions.AliasAttribute`.

## Options

Options common for all formats. Note that this is a struct, not a class, and has value semantics.

#### `Meta`

Contains attribute classes.
Change them to use different attributes, such as `Serializable` or `ProtoContract`.
Note that this field is a reference,
so it is usually better to create a new `MetaOptions` object instead of assigning directly to its fields.

#### `TagMode`

Experimental. Do not use.

#### `AllowUnknownFields`

Controls what happens when unknown field is encountered during deserialization.
If `true`, it is either ignored or stored in `YuzuUnknownStorage`.
Otherwise an exception is thrown. Default value is `false`.

#### `AllowEmptyTypes`

Controls what happens when a class without serializable fields is encountered during serialization or deserialization.
If `true`, it is ignored.
Otherwise an exception is thrown. Default value is `false`.

#### `ReportErrorPosition`

If `true`, a source stream position is included in error messages during deserialization.
Default value is `false`.

