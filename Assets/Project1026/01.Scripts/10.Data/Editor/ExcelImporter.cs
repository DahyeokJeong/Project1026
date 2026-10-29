using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public static class ExcelImporter
{
    private const string ExcelFolderPath = "Project1026/06.Datas/Excel";

    private const string JsonFolderPath = "Project1026/06.Datas/JSON";

    [MenuItem("Tools/Data/Read Excel")]
    public static void ReadExcel()
    {
        string excelDirectory = Path.Combine(Application.dataPath, ExcelFolderPath);

        if (!Directory.Exists(excelDirectory))
        {
            Debug.LogError($"Excel 폴더를 찾을 수 없습니다: {excelDirectory}");
            return;
        }

        string[] excelFiles = Directory.GetFiles(
            excelDirectory,
            "*.xlsx",
            SearchOption.AllDirectories
        );

        Dictionary<string, Type> dataTypes = FindExcelDataTypes();
        HashSet<string> importedSheets = new HashSet<string>();

        foreach (string excelPath in excelFiles)
        {
            if (Path.GetFileName(excelPath).StartsWith("~$"))
                continue;

            try
            {
                List<string> sheetNames = ExcelReader.GetSheetNames(excelPath);

                foreach (string sheetName in sheetNames)
                {
                    if (!dataTypes.TryGetValue(sheetName, out Type dataType))
                        continue;

                    if (importedSheets.Contains(sheetName))
                    {
                        Debug.LogError($"중복된 Excel 시트입니다: {sheetName} / 파일: {excelPath}");
                        continue;
                    }

                    Type loaderType = FindLoaderType(dataType);

                    if (loaderType == null)
                    {
                        Debug.LogError($"Loader 클래스를 찾을 수 없습니다: {dataType.Name}");
                        continue;
                    }

                    if (ImportSheet(excelPath, sheetName, dataType, loaderType))
                        importedSheets.Add(sheetName);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Excel 읽기 실패 / 파일: {excelPath} / 오류: {e.Message}");
            }
        }

        AssetDatabase.Refresh();

        Debug.Log($"Excel Import 완료 / 시트: {importedSheets.Count}개");
    }

    private static Dictionary<string, Type> FindExcelDataTypes()
    {
        Dictionary<string, Type> dataTypes = new();

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in GetAssemblyTypes(assembly))
            {
                if (type == null || !type.IsClass || type.IsAbstract)
                    continue;

                ExcelSheetAttribute attribute =
                    type.GetCustomAttribute<ExcelSheetAttribute>();

                if (attribute == null)
                    continue;

                if (dataTypes.ContainsKey(attribute.SheetName))
                {
                    Debug.LogError($"중복된 Excel 시트 등록: {attribute.SheetName}");
                    continue;
                }

                dataTypes.Add(attribute.SheetName, type);
            }
        }
        return dataTypes;
    }

    private static Type FindLoaderType(Type dataType)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in GetAssemblyTypes(assembly))
            {
                if (type == null || !type.IsClass || type.IsAbstract)
                    continue;

                if (!typeof(IDataLoader).IsAssignableFrom(type))
                    continue;

                foreach (FieldInfo field in type.GetFields())
                {
                    if (!field.FieldType.IsGenericType)
                        continue;

                    if (field.FieldType.GetGenericTypeDefinition() != typeof(List<>))
                        continue;

                    Type elementType = field.FieldType.GetGenericArguments()[0];

                    if (elementType == dataType)
                        return type;
                }
            }
        }
        return null;
    }

    private static IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return Array.FindAll(e.Types, type => type != null);
        }
    }

    private static bool ImportSheet(
        string excelPath,
        string sheetName,
        Type dataType,
        Type loaderType)
    {
        List<Dictionary<string, string>> rows = ExcelReader.Read(excelPath, sheetName);

        IDataLoader loader = (IDataLoader)Activator.CreateInstance(loaderType);

        IList dataList = loader.GetDataList();

        foreach (Dictionary<string, string> rowData in rows)
        {
            object dataObject = Activator.CreateInstance(dataType);

            foreach (KeyValuePair<string, string> data in rowData)
            {
                FieldInfo field = dataType.GetField(data.Key);

                if (field == null)
                {
                    Debug.LogWarning($"필드를 찾을 수 없습니다 / 시트: {sheetName} / 필드: {data.Key}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(data.Value))
                    continue;

                try
                {
                    object convertedValue = Convert.ChangeType(
                        data.Value,
                        field.FieldType,
                        CultureInfo.InvariantCulture
                    );

                    field.SetValue(dataObject, convertedValue);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"데이터 변환 실패 / 시트: {sheetName} / " +
                        $"필드: {data.Key} / 값: {data.Value} / 오류: {e.Message}"
                    );

                    return false;
                }
            }

            dataList.Add(dataObject);
        }

        string json = JsonUtility.ToJson(loader, true);

        string outputDirectory = Path.Combine(
            Application.dataPath, JsonFolderPath
        );

        Directory.CreateDirectory(outputDirectory);

        string outputPath = Path.Combine(outputDirectory, $"{sheetName}.json");


        File.WriteAllText(outputPath, json);

        AssetDatabase.Refresh();

        string assetPath = $"Assets/{JsonFolderPath}/{sheetName}.json";

        bool registered = AddressableRegistrar.Register(assetPath, sheetName);

        if (!registered)
        {
            Debug.LogError($"Addressables 등록 실패 / 시트: {sheetName}");

            return false;
        }

        Debug.Log(
            $"Excel 변환 완료 / 시트: {sheetName} / " +
            $"Loader: {loaderType.Name} / 데이터: {dataList.Count}개"
        );

        return true;
    }
}
