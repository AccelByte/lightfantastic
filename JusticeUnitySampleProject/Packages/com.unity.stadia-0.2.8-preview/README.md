# Stadia Platform Support

* Provides Build & Run capabilities in the Unity Editor.
* Allows Stadia developers to call into public Stadia APIs that may not be exposed in Unity's public API surface.

## Status
[![](https://badge-proxy.cds.internal.unity3d.com/bea875dc-1d4e-4214-b107-0f44042f6144)](https://badges.cds.internal.unity3d.com/packages/com.unity.stadia/build-info?branch=master&testWorkflow=package-isolation)
[![](https://badge-proxy.cds.internal.unity3d.com/45a9b2da-e5ed-4d8c-a551-113db25af579)](https://badges.cds.internal.unity3d.com/packages/com.unity.stadia/warnings-info?branch=master)

## Special cases

### GgpGameEventAttributeValue union and GgpRecordGameEvent()

The GgpGameEventAttributeValue union causes marshaling problems since it has a scalar types and a string type. Since the string has to be marshaled and C# has to concept of only one of the fields in a union being used, it will try to marshal the string no matter how the union is actually used. To work around this, the string is now an IntPtr. To use the string field, the developer should pin the desired string in memory then assign the IntPtr to the string to the string_value field. Like so:

GgpStatus status;
var playerId = StadiaNativeApis.GgpGetPrimaryPlayerId();

GgpGameEventAttributeValue areaAttribute = new GgpGameEventAttributeValue();

// Pin the memory
areaAttribute.string_value = Marshal.StringToHGlobalAnsi("Denmark");

bool result = StadiaNativeApis.GgpRecordGameEvent(playerId, "area-entered", new GgpGameEventAttribute[]
{
    new GgpGameEventAttribute()
    {
        attribute_type_id = "area-id",
        value = areaAttribute,
        value_type = new GgpGameEventAttributeValueType() { Value = (int)GgpGameEventAttributeValueTypeValues.kGgpGameEventAttributeType_String }
    }
}, 1, out status);

// Unpin the memory
Marshal.FreeHGlobal(areaAttribute.string_value);

```

