using Common.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{
    public static class General
    {
        public static bool IsEmpty<T>(this IEnumerable<T> target)
        {
            return (target is null) || !target.Any();
        }

        public static bool IsEmpty<T>(this IList<T> target)
        {
            return (target is null) || !target.Any();
        }

        public static bool IsEmpty<T>(this ICollection<T> target)
        {
            return (target is null) || !target.Any();
        }

        public static bool IsEmpty(this object target)
        {
            return target == null;
        }

        public static bool IsEmpty(this string target)
        {
            return string.IsNullOrWhiteSpace(target);
        }

        public static bool IsEmpty(this Guid? target)
        {
            return target == null ||
                !target.HasValue ||
                target == Guid.Empty;
        }

        public static bool IsEmpty(this Guid target)
        {
            return target == Guid.Empty;
        }

        public static string ByteArrayToString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }
            return sOutput.ToString();
        }

        public static string RequestEnvironment
        {
            get
            {
                return Environment.GetEnvironmentVariable(LanguageProcessingConstants.EnvironmentVariable);
            }
        }

        public static string GetAbsolutePath<Type>(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Type).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }

        public static T GetEnumValueFromStringValue<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(StringValueAttribute)) is StringValueAttribute attribute)
                {
                    if (attribute.StringValue == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }
    }
}