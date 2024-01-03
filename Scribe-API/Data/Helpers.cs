using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSAS_Web.Data
{
    public class Helpers
    {
        /// <summary>
        /// Add id to filename. turns exampleFile.png into exampleFile_34.png
        /// </summary>
        public static string AddIdToFilename(string origFile, int id)
        {
            var fileArr = origFile.Split(new string[] {"."}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if(fileArr.Count > 1)
            {
                fileArr.Insert(fileArr.Count - 1, $"_{id}.");
            }
            else
            {
                fileArr.Add($"_{id}");
            }
            string newFileName = String.Join("", fileArr.ToArray());
            return newFileName;
        }
    }

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string TruncateFileName(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if(value.Length > maxLength)
            {
                var fileArr = value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if(fileArr.Count > 1)
                {
                    string newFileName = String.Join("", fileArr.Take(fileArr.Count - 1));
                    newFileName = newFileName.Truncate(maxLength - 5);
                    newFileName = $"{newFileName}.{fileArr[fileArr.Count - 1]}";
                    return newFileName;
                }
                else
                {
                    return value.Truncate(maxLength);
                }
            }
            else
            {
                return value;
            }
        }
    }
}
