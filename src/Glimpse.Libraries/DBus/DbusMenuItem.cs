using Glimpse.Libraries.DBus.Core;

namespace Glimpse.Libraries.DBus;

public class DbusMenuItem
{
	public int Id { get; set; }
	public bool? Enabled { get; set; }
	public string Label { get; set; }
	public bool? Visible { get; set; }
	public string IconName { get; set; }
	public int? ToggleState { get; set; }
	public string ToggleType { get; set; }
	public string Type { get; set; }
	public byte[] IconData { get; set; }

	public DbusMenuItem[] Children { get; set; }

	public static DbusMenuItem From((int, Dictionary<string, VariantValue>, VariantValue[]) root)
	{
		root.Item2.TryGetValue("enabled", out var enabled);
		root.Item2.TryGetValue("label", out var label);
		root.Item2.TryGetValue("visible", out var visible);
		root.Item2.TryGetValue("icon-name", out var iconName);
		root.Item2.TryGetValue("toggle-state", out var toggleState);
		root.Item2.TryGetValue("toggle-type", out var toggleType);
		root.Item2.TryGetValue("icon-data", out var iconData);
		root.Item2.TryGetValue("type", out var type);

		var item = new DbusMenuItem()
		{
			Id = root.Item1,
			Enabled = ReadBool(enabled),
			Label = RemoveMnemonics(ReadString(label)),
			Visible = ReadBool(visible),
			IconName = ReadString(iconName),
			ToggleState = toggleState.Type == VariantValueType.Int32 ? toggleState.GetInt32()
				: toggleState.Type == VariantValueType.UInt32 ? (int)toggleState.GetUInt32()
				: 0,
			ToggleType = ReadString(toggleType),
			Type = ReadString(type),
			Children = ProcessChildren(root.Item3)
		};

		if (iconData.Type == VariantValueType.Array)
		{
			item.IconData = iconData.GetArray<byte>();
		}

		return item;
	}

	private static string ReadString(VariantValue value)
		=> value.Type == VariantValueType.String ? value.GetString() : null;

	private static bool? ReadBool(VariantValue value)
		=> value.Type == VariantValueType.Bool ? value.GetBool() : null;

	private static string RemoveMnemonics(string label)
	{
		if (string.IsNullOrEmpty(label)) return label;
		return string.Join('_', label.Split("__").Select(s => s.Replace("_", "")));
	}

	private static DbusMenuItem[] ProcessChildren(VariantValue[] children)
	{
		if (!children.Any()) return Array.Empty<DbusMenuItem>();

		var processedChildren = new LinkedList<DbusMenuItem>();

		foreach (var child in children)
		{
			var id = child.GetItem(0).Type == VariantValueType.Int32 ? child.GetItem(0).GetInt32()
				: child.GetItem(0).Type == VariantValueType.UInt32 ? (int)child.GetItem(0).GetUInt32()
				: throw new Exception("Unabled to parse menu item of type " + child.GetItem(0).Type);

			var properties = ReadProperties(child.GetItem(1));
			var childrenOfChild = ReadChildVariants(child.GetItem(2));
			var tuple = (id, properties, subChildren: childrenOfChild);

			processedChildren.AddLast(From(tuple));
		}

		return processedChildren.ToArray();
	}

	private static Dictionary<string, VariantValue> ReadProperties(VariantValue properties)
	{
		if (properties.Type == VariantValueType.Dictionary)
		{
			var dictionary = new Dictionary<string, VariantValue>(properties.Count);
			for (var i = 0; i < properties.Count; i++)
			{
				var entry = properties.GetDictionaryEntry(i);
				dictionary[entry.Key.GetString()] = entry.Value;
			}

			return dictionary;
		}

		var asArray = new Dictionary<string, VariantValue>(properties.Count);
		for (var i = 0; i < properties.Count; i++)
		{
			var entry = properties.GetItem(i);
			asArray[entry.GetItem(0).GetString()] = entry.GetItem(1);
		}

		return asArray;
	}

	private static VariantValue[] ReadChildVariants(VariantValue childrenArray)
	{
		var result = new VariantValue[childrenArray.Count];
		for (var i = 0; i < childrenArray.Count; i++)
		{
			result[i] = childrenArray.GetItem(i);
		}

		return result;
	}
}
