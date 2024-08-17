using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

namespace DotTiled;

internal static class ExtensionsXmlReader
{
  internal static string GetRequiredAttribute(this XmlReader reader, string attribute)
  {
    return reader.GetAttribute(attribute) ?? throw new XmlException($"{attribute} attribute is required"); ;
  }

  internal static T GetRequiredAttributeParseable<T>(this XmlReader reader, string attribute) where T : IParsable<T>
  {
    var value = reader.GetAttribute(attribute) ?? throw new XmlException($"{attribute} attribute is required");
    return T.Parse(value, CultureInfo.InvariantCulture);
  }

  internal static T GetRequiredAttributeParseable<T>(this XmlReader reader, string attribute, Func<string, T> parser)
  {
    var value = reader.GetAttribute(attribute) ?? throw new XmlException($"{attribute} attribute is required");
    return parser(value);
  }

  internal static T GetRequiredAttributeEnum<T>(this XmlReader reader, string attribute, Func<string, T> enumParser) where T : Enum
  {
    var value = reader.GetAttribute(attribute) ?? throw new XmlException($"{attribute} attribute is required");
    return enumParser(value);
  }

  internal static string? GetOptionalAttribute(this XmlReader reader, string attribute, string? defaultValue = default)
  {
    return reader.GetAttribute(attribute) ?? defaultValue;
  }

  internal static T? GetOptionalAttributeParseable<T>(this XmlReader reader, string attribute) where T : struct, IParsable<T>
  {
    var value = reader.GetAttribute(attribute);
    if (value is null)
      return null;

    return T.Parse(value, CultureInfo.InvariantCulture);
  }

  internal static T? GetOptionalAttributeParseable<T>(this XmlReader reader, string attribute, Func<string, T> parser) where T : struct
  {
    var value = reader.GetAttribute(attribute);
    if (value is null)
      return null;

    return parser(value);
  }

  internal static T? GetOptionalAttributeClass<T>(this XmlReader reader, string attribute) where T : class, IParsable<T>
  {
    var value = reader.GetAttribute(attribute);
    if (value is null)
      return null;

    return T.Parse(value, CultureInfo.InvariantCulture);
  }

  internal static T? GetOptionalAttributeEnum<T>(this XmlReader reader, string attribute, Func<string, T> enumParser) where T : struct, Enum
  {
    var value = reader.GetAttribute(attribute);
    return value != null ? enumParser(value) : null;
  }

  internal static List<T> ReadList<T>(this XmlReader reader, string wrapper, string elementName, Func<XmlReader, T> readElement)
  {
    var list = new List<T>();

    if (reader.IsEmptyElement)
      return list;

    reader.ReadStartElement(wrapper);
    while (reader.IsStartElement(elementName))
    {
      list.Add(readElement(reader));

      if (reader.NodeType == XmlNodeType.EndElement)
        continue; // At end of list, no need to read again

      reader.Read();
    }
    reader.ReadEndElement();

    return list;
  }

  internal static void ProcessChildren(this XmlReader reader, string wrapper, Func<XmlReader, string, Action> getProcessAction)
  {
    if (reader.IsEmptyElement)
    {
      reader.ReadStartElement(wrapper);
      return;
    }

    reader.ReadStartElement(wrapper);
    while (reader.IsStartElement())
    {
      var elementName = reader.Name;
      var action = getProcessAction(reader, elementName);
      action();
    }
    reader.ReadEndElement();
  }

  [return: NotNull]
  internal static List<T> ProcessChildren<T>(this XmlReader reader, string wrapper, Func<XmlReader, string, T> getProcessAction)
  {
    var list = new List<T>();

    if (reader.IsEmptyElement)
    {
      reader.ReadStartElement(wrapper);
      return list;
    }

    reader.ReadStartElement(wrapper);
    while (reader.IsStartElement())
    {
      var elementName = reader.Name;
      var item = getProcessAction(reader, elementName);
      list.Add(item);
    }
    reader.ReadEndElement();

    return list;
  }

  internal static void SkipXmlDeclaration(this XmlReader reader)
  {
    if (reader.NodeType == XmlNodeType.XmlDeclaration)
      reader.Read();
  }
}