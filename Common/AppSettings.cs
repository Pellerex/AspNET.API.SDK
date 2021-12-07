namespace Common
{
    public class AppSettings
    {
        public bool ContainerMode { get; set; }
        public string AllowedOrigins { get; set; }

        public MachineLearning MachineLearningSettings { get; set; }

        public class MachineLearning
        {
            public string LanguageProcessingApiOnnxModelUri { get; set; }
            public string LanguageProcessingApiVocabUri { get; set; }
        }
    }
}