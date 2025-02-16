﻿// Decompiled with JetBrains decompiler
// Type: UnityEditor.iOS.Xcode.PlistDocument
// Assembly: UnityEditor.iOS.Extensions.Xcode, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 36E9AB58-F6A7-4B4D-BAB5-BE8059694DCD
// Assembly location: D:\workspace\pk\trunk\usdk\publish\ios\tools\UnityEditor.iOS.Extensions.Xcode.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace UnityEditor.iOS.Xcode
{
  public class PlistDocument
  {
    public PlistElementDict root;
    public string version;
    private XDocumentType documentType;

    public PlistDocument()
    {
      this.root = new PlistElementDict();
      this.version = "1.0";
    }

    internal static XDocument ParseXmlNoDtd(string text)
    {
      return XDocument.Load(XmlReader.Create((TextReader) new StringReader(text), new XmlReaderSettings()
      {
        ProhibitDtd = false,
        XmlResolver = (XmlResolver) null
      }));
    }

    internal static string CleanDtdToString(XDocument doc, XDocumentType documentType)
    {
      if (documentType != null)
      {
        XDeclaration declaration1 = new XDeclaration("1.0", "utf-8", (string) null);
        object[] objArray1 = new object[2];
        int index1 = 0;
        XDocumentType xdocumentType = new XDocumentType(documentType.Name, documentType.PublicId, documentType.SystemId, (string) null);
        objArray1[index1] = (object) xdocumentType;
        int index2 = 1;
        XElement xelement = new XElement(doc.Root.Name);
        objArray1[index2] = (object) xelement;
        XDocument xdocument = new XDocument(declaration1, objArray1);
        object[] objArray2 = new object[7];
        int index3 = 0;
        string str1 = "";
        objArray2[index3] = (object) str1;
        int index4 = 1;
        XDeclaration declaration2 = xdocument.Declaration;
        objArray2[index4] = (object) declaration2;
        int index5 = 2;
        string str2 = "\n";
        objArray2[index5] = (object) str2;
        int index6 = 3;
        XDocumentType documentType1 = xdocument.DocumentType;
        objArray2[index6] = (object) documentType1;
        int index7 = 4;
        string str3 = "\n";
        objArray2[index7] = (object) str3;
        int index8 = 5;
        XElement root = doc.Root;
        objArray2[index8] = (object) root;
        int index9 = 6;
        string str4 = "\n";
        objArray2[index9] = (object) str4;
        return string.Concat(objArray2);
      }
      XDeclaration declaration3 = new XDeclaration("1.0", "utf-8", (string) null);
      object[] objArray3 = new object[1];
      int index10 = 0;
      XElement xelement1 = new XElement(doc.Root.Name);
      objArray3[index10] = (object) xelement1;
      XDocument xdocument1 = new XDocument(declaration3, objArray3);
      object[] objArray4 = new object[5];
      int index11 = 0;
      string str5 = "";
      objArray4[index11] = (object) str5;
      int index12 = 1;
      XDeclaration declaration4 = xdocument1.Declaration;
      objArray4[index12] = (object) declaration4;
      int index13 = 2;
      string newLine = Environment.NewLine;
      objArray4[index13] = (object) newLine;
      int index14 = 3;
      XElement root1 = doc.Root;
      objArray4[index14] = (object) root1;
      int index15 = 4;
      string str6 = "\n";
      objArray4[index15] = (object) str6;
      return string.Concat(objArray4);
    }

    internal static string CleanDtdToString(XDocument doc)
    {
      return PlistDocument.CleanDtdToString(doc, doc.DocumentType);
    }

    private static string GetText(XElement xml)
    {
      return string.Join("", Enumerable.ToArray<string>(Enumerable.Select<XText, string>(Enumerable.OfType<XText>((IEnumerable) xml.Nodes()), (Func<XText, string>) (x => x.Value))));
    }

    private static PlistElement ReadElement(XElement xml)
    {
      string localName = xml.Name.LocalName;
      if (localName != null)
      {
          Dictionary<string,int> types = new Dictionary<string, int>(8)
          {
            {
              "dict",
              0
            },
            {
              "array",
              1
            },
            {
              "string",
              2
            },
            {
              "integer",
              3
            },
            {
              "real",
              4
            },
            {
              "date",
              5
            },
            {
              "true",
              6
            },
            {
              "false",
              7
            }
          };

        int num = 0;
        if (types.TryGetValue(localName, out num))
        {
          switch (num)
          {
            case 0:
              List<XElement> list1 = Enumerable.ToList<XElement>(xml.Elements());
              PlistElementDict plistElementDict = new PlistElementDict();
              if (list1.Count % 2 == 1)
                throw new Exception("Malformed plist file");
              for (int index1 = 0; index1 < list1.Count - 1; ++index1)
              {
                if (list1[index1].Name != (XName) "key")
                  throw new Exception("Malformed plist file. Found '" + (object) list1[index1].Name + "' where 'key' was expected.");
                string index2 = PlistDocument.GetText(list1[index1]).Trim();
                PlistElement plistElement = PlistDocument.ReadElement(list1[index1 + 1]);
                if (plistElement != null)
                {
                  ++index1;
                  plistElementDict[index2] = plistElement;
                }
              }
              return (PlistElement) plistElementDict;
            case 1:
              List<XElement> list2 = Enumerable.ToList<XElement>(xml.Elements());
              PlistElementArray plistElementArray = new PlistElementArray();
              foreach (XElement xml1 in list2)
              {
                PlistElement plistElement = PlistDocument.ReadElement(xml1);
                if (plistElement != null)
                  plistElementArray.values.Add(plistElement);
              }
              return (PlistElement) plistElementArray;
            case 2:
              return (PlistElement) new PlistElementString(PlistDocument.GetText(xml));
            case 3:
              int result1;
              if (int.TryParse(PlistDocument.GetText(xml), out result1))
                return (PlistElement) new PlistElementInteger(result1);
              return (PlistElement) null;
            case 4:
              float result2;
              if (float.TryParse(PlistDocument.GetText(xml), out result2))
                return (PlistElement) new PlistElementReal(result2);
              return (PlistElement) null;
            case 5:
              DateTime result3;
              if (DateTime.TryParse(PlistDocument.GetText(xml), out result3))
                return (PlistElement) new PlistElementDate(result3.ToUniversalTime());
              return (PlistElement) null;
            case 6:
              return (PlistElement) new PlistElementBoolean(true);
            case 7:
              return (PlistElement) new PlistElementBoolean(false);
          }
        }
      }
      return null;
    }

    public void Create()
    {
      this.ReadFromString("<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\"><plist version=\"1.0\"><dict></dict></plist>");
    }

    public void ReadFromFile(string path)
    {
      this.ReadFromString(File.ReadAllText(path));
    }

    public void ReadFromStream(TextReader tr)
    {
      this.ReadFromString(tr.ReadToEnd());
    }

    public void ReadFromString(string text)
    {
      XDocument xdocument = PlistDocument.ParseXmlNoDtd(text);
      this.version = (string) xdocument.Root.Attribute((XName) "version");
      PlistElement plistElement = PlistDocument.ReadElement(System.Xml.XPath.Extensions.XPathSelectElement((XNode) xdocument, "plist/dict"));
      if (plistElement == null)
        throw new Exception("Error parsing plist file");
      this.root = plistElement as PlistElementDict;
      if (this.root == null)
        throw new Exception("Malformed plist file");
      this.documentType = xdocument.DocumentType;
    }

    private static XElement WriteElement(PlistElement el)
    {
      if (el is PlistElementBoolean)
        return new XElement((XName) (!(el as PlistElementBoolean).value ? "false" : "true"));
      if (el is PlistElementInteger)
        return new XElement((XName) "integer", (object) (el as PlistElementInteger).value.ToString());
      if (el is PlistElementString)
        return new XElement((XName) "string", (object) (el as PlistElementString).value);
      if (el is PlistElementReal)
        return new XElement((XName) "real", (object) (el as PlistElementReal).value.ToString());
      if (el is PlistElementDate)
        return new XElement((XName) "date", (object) (el as PlistElementDate).value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
      if (el is PlistElementDict)
      {
        PlistElementDict plistElementDict = el as PlistElementDict;
        XElement xelement1 = new XElement((XName) "dict");
        foreach (KeyValuePair<string, PlistElement> keyValuePair in (IEnumerable<KeyValuePair<string, PlistElement>>) plistElementDict.values)
        {
          XElement xelement2 = new XElement((XName) "key", (object) keyValuePair.Key);
          XElement xelement3 = PlistDocument.WriteElement(keyValuePair.Value);
          if (xelement3 != null)
          {
            xelement1.Add((object) xelement2);
            xelement1.Add((object) xelement3);
          }
        }
        return xelement1;
      }
      if (!(el is PlistElementArray))
        return (XElement) null;
      PlistElementArray plistElementArray = el as PlistElementArray;
      XElement xelement4 = new XElement((XName) "array");
      foreach (PlistElement el1 in plistElementArray.values)
      {
        XElement xelement1 = PlistDocument.WriteElement(el1);
        if (xelement1 != null)
          xelement4.Add((object) xelement1);
      }
      return xelement4;
    }

    public void WriteToFile(string path)
    {
      Encoding encoding = (Encoding) new UTF8Encoding(false);
      File.WriteAllText(path, this.WriteToString(), encoding);
    }

    public void WriteToStream(TextWriter tw)
    {
      tw.Write(this.WriteToString());
    }

    public string WriteToString()
    {
      XElement xelement1 = PlistDocument.WriteElement((PlistElement) this.root);
      XElement xelement2 = new XElement((XName) "plist");
      xelement2.Add((object) new XAttribute((XName) "version", (object) this.version));
      xelement2.Add((object) xelement1);
      XDocument doc = new XDocument();
      doc.Add((object) xelement2);
      return PlistDocument.CleanDtdToString(doc, this.documentType).Replace("\r\n", "\n");
    }
  }
}
