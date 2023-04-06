using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public static class CsvLoader
{
    public static List<T> Load<T>(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return new List<T>(csv.GetRecords<T>());
    }
}