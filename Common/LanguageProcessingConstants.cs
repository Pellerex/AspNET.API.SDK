using Common.Attributes;

namespace Common
{
    /// <summary>
    /// Constants used in LanguageProcessing API
    /// </summary>
    public static class LanguageProcessingConstants
    {
        public static readonly string EnvironmentVariable = "ASPNETCORE_ENVIRONMENT";

        public class DefaultTokens
        {
            public const string Padding = "";
            public const string Unknown = "[UNK]";
            public const string Classification = "[CLS]";
            public const string Separation = "[SEP]";
            public const string Mask = "[MASK]";
        }

        public class ErrorCodes
        {
            public const string InternalServerError = "";
        }
    }
}