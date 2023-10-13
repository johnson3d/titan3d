using Jither.OpenEXR.Attributes;

namespace Jither.OpenEXR;

internal class EXRHeader
{
    public static readonly Chromaticities DefaultChromaticities = new(
        0.6400f, 0.3300f,
        0.3000f, 0.6000f,
        0.1500f, 0.0600f,
        0.3127f, 0.3290f
    );

    private readonly List<EXRAttribute> attributes = new();
    private readonly Dictionary<string, EXRAttribute> attributesByName = new();

    public IReadOnlyDictionary<string, EXRAttribute> AttributesByName => attributesByName;
    public IReadOnlyList<EXRAttribute> Attributes => attributes;

    public bool IsEmpty => attributes.Count == 0;

    public EXRHeader()
    {
    }

    public static EXRHeader ReadFrom(EXRReader reader, int maxNameLength)
    {
        var result = new EXRHeader();
        while (true)
        {
            var attribute = EXRAttribute.ReadFrom(reader, maxNameLength);
            if (attribute == null)
            {
                break;
            }
            result.SetAttribute(attribute);
        }
        return result;
    }

    public void WriteTo(EXRWriter writer)
    {
        foreach (var attribute in attributes)
        {
            attribute.WriteTo(writer);
        }
        writer.WriteByte(0);
    }

    /// <summary>
    /// Adds or replaces attribute. Only a single attribute with a given name may exist in the header.
    /// </summary>
    public void SetAttribute(EXRAttribute attribute)
    {
        if (attributesByName.TryGetValue(attribute.Name, out var existingAttribute))
        {
            int index = attributes.IndexOf(existingAttribute);
            attributes.Remove(existingAttribute);
            attributes.Insert(index, attribute);
        }
        else
        {
            attributes.Add(attribute);
        }
        attributesByName[attribute.Name] = attribute;
    }

    public void RemoveAttribute(string name)
    {
        attributes.RemoveAll(a => a.Name == name);
        attributesByName.Remove(name);
    }

    public T GetAttributeOrThrow<T>(string name)
    {
        if (!TryGetAttribute<T>(name, out var attribute) || attribute == null)
        {
            throw new EXRFormatException($"Missing {name} attribute.");
        }
        return attribute;
    }

    public T? GetAttributeOrDefault<T>(string name)
    {
        if (!TryGetAttribute<T>(name, out var attribute) || attribute == null)
        {
            return default;
        }
        return attribute;
    }

    public bool HasAttribute(string name)
    {
        return attributesByName.ContainsKey(name);
    }

    public bool TryGetAttribute<T>(string name, out T? result)
    {
        if (!attributesByName.TryGetValue(name, out var attr))
        {
            result = default;
            return false;
        }

        if (attr.UntypedValue == null)
        {
            result = default;
            return !typeof(T).IsClass && !typeof(T).IsInterface && !typeof(T).IsArray;
        }

        if (typeof(T).IsEnum)
        {
            if (Enum.TryParse(typeof(T), attr.UntypedValue.ToString(), true, out var enumResult))
            {
                result = (T)enumResult;
                return true;
            }
        }

        if (typeof(T).IsAssignableFrom(attr.UntypedValue.GetType()))
        {
            result = (T)attr.UntypedValue;
            return true;
        }
        result = default;
        return false;
    }
}
