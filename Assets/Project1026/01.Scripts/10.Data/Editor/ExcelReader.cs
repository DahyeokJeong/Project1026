using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public static class ExcelReader
{
    private static readonly XNamespace MainNs =
        "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    private static readonly XNamespace RelNs =
        "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

    private static readonly XNamespace PackageRelNs =
        "http://schemas.openxmlformats.org/package/2006/relationships";

    public static List<string> GetSheetNames(string excelPath)
    {
        using (ZipArchive archive = ZipFile.OpenRead(excelPath))
        {
            XDocument workbook = LoadXml(archive, "xl/workbook.xml");

            return workbook.Descendants(MainNs + "sheet")
                .Select(sheet => (string)sheet.Attribute("name"))
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }
    }

    public static List<Dictionary<string, string>> Read(
        string excelPath,
        string sheetName)
    {
        List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();

        using (ZipArchive archive = ZipFile.OpenRead(excelPath))
        {
            XDocument workbook = LoadXml(archive, "xl/workbook.xml");
            XDocument relationships =
                LoadXml(archive, "xl/_rels/workbook.xml.rels");

            XElement sheet = workbook.Descendants(MainNs + "sheet")
                .FirstOrDefault(x =>
                    (string)x.Attribute("name") == sheetName);

            if (sheet == null)
                throw new Exception($"Excel 시트를 찾을 수 없습니다: {sheetName}");

            string relationshipId = (string)sheet.Attribute(RelNs + "id");

            XElement relationship =
                relationships.Descendants(PackageRelNs + "Relationship")
                    .FirstOrDefault(x =>
                        (string)x.Attribute("Id") == relationshipId);

            if (relationship == null)
                throw new Exception($"시트 관계 정보를 찾을 수 없습니다: {sheetName}");

            string target = (string)relationship.Attribute("Target");
            string sheetPath = ResolveSheetPath(target);

            XDocument sheetDocument = LoadXml(archive, sheetPath);
            List<string> sharedStrings = ReadSharedStrings(archive);

            List<XElement> sheetRows = sheetDocument
                .Descendants(MainNs + "sheetData")
                .Elements(MainNs + "row")
                .ToList();

            if (sheetRows.Count == 0)
                return rows;

            Dictionary<int, string> headers = ReadCells(sheetRows[0], sharedStrings);

            for (int i = 1; i < sheetRows.Count; i++)
            {
                Dictionary<int, string> cells = ReadCells(sheetRows[i], sharedStrings);

                Dictionary<string, string> row = new Dictionary<string, string>();

                foreach (KeyValuePair<int, string> header in headers)
                {
                    if (string.IsNullOrWhiteSpace(header.Value))
                        continue;

                    cells.TryGetValue(header.Key, out string value);
                    row[header.Value] = value ?? string.Empty;
                }

                if (row.Values.All(string.IsNullOrWhiteSpace))
                    continue;

                rows.Add(row);
            }
        }

        return rows;
    }

    private static XDocument LoadXml(
        ZipArchive archive,
        string path)
    {
        ZipArchiveEntry entry = archive.GetEntry(path);

        if (entry == null)
            throw new FileNotFoundException($"Excel 내부 파일을 찾을 수 없습니다: {path}");

        using (Stream stream = entry.Open())
        {
            return XDocument.Load(stream);
        }
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        List<string> strings = new List<string>();

        ZipArchiveEntry entry = archive.GetEntry("xl/sharedStrings.xml");

        if (entry == null)
            return strings;

        using (Stream stream = entry.Open())
        {
            XDocument document = XDocument.Load(stream);

            foreach (XElement item in document.Descendants(MainNs + "si"))
            {
                string value = string.Concat(item.Descendants(MainNs + "t").Select(x => x.Value));

                strings.Add(value);
            }
        }

        return strings;
    }

    private static Dictionary<int, string> ReadCells(
        XElement row,
        List<string> sharedStrings)
    {
        Dictionary<int, string> cells = new Dictionary<int, string>();

        foreach (XElement cell in row.Elements(MainNs + "c"))
        {
            string reference = (string)cell.Attribute("r");
            int columnIndex = GetColumnIndex(reference);

            string type = (string)cell.Attribute("t");
            string value = (string)cell.Element(MainNs + "v") ?? string.Empty;

            if (type == "s" &&
                int.TryParse(value, out int stringIndex) &&
                stringIndex >= 0 &&
                stringIndex < sharedStrings.Count)
            {
                value = sharedStrings[stringIndex];
            }
            else if (type == "inlineStr")
            {
                value = string.Concat(
                    cell.Descendants(MainNs + "t")
                        .Select(x => x.Value)
                );
            }

            cells[columnIndex] = value;
        }
        return cells;
    }

    private static int GetColumnIndex(string cellReference)
    {
        int index = 0;

        foreach (char character in cellReference)
        {
            if (!char.IsLetter(character))
                break;

            index = index * 26 +
                (char.ToUpperInvariant(character) - 'A' + 1);
        }

        return index - 1;
    }

    private static string ResolveSheetPath(string target)
    {
        string path = target.Replace('\\', '/');

        if (path.StartsWith("/"))
            return path.TrimStart('/');

        Uri baseUri = new Uri("http://excel.local/xl/workbook.xml");
        Uri resolvedUri = new Uri(baseUri, path);

        return resolvedUri.AbsolutePath.TrimStart('/');
    }
}
