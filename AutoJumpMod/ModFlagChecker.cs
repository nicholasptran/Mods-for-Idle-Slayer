using System.Reflection;

namespace AutoJumpMod;

internal class ModFlagChecker(string qualifiedTypeName, string privateFieldName = "")
{
    private Type _type;
    private FieldInfo _field;
    private bool _initialized;

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        _type = Type.GetType(qualifiedTypeName);
        if (_type == null)
        {
            Plugin.DLog($"Type not found: {qualifiedTypeName}");
            return;
        }

        if (string.IsNullOrEmpty(privateFieldName)) return;

        _field = _type.GetField(
            privateFieldName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        if (_field == null)
            Plugin.DLog($"Field '{privateFieldName}' not found on {_type.FullName}");
    }

    private UnityEngine.Object FindInstance()
    {
        Initialize();
        if (_type == null) return null;

        var findMethod = typeof(UnityEngine.Object)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "FindObjectOfType"
                && m.IsGenericMethodDefinition
                && m.GetParameters().Length == 0
            );
        if (findMethod == null)
        {
            Plugin.DLog("Couldn't find generic FindObjectOfType<>()");
            return null;
        }

        var generic = findMethod.MakeGenericMethod(_type);
        return generic.Invoke(null, []) as UnityEngine.Object;
    }

    public bool IsLoaded() => FindInstance() != null;

    public bool GetBoolFlag()
    {
        var inst = FindInstance();
        if (inst == null || _field == null) return false;
        return (bool)_field.GetValue(inst)!;
    }
}