using Common;
using Microsoft.ML;
using System.Collections.Generic;

namespace ML.Prediction
{
    public class OnnxModelConfigurator<TFeature> where TFeature : class
    {
        private readonly MLContext mlContext;
        private readonly ITransformer mlModel;
        private readonly AppSettings appSettings;
        private List<string> vocabulary;

        public string[] ModelInput => new[] { "unique_ids_raw_output___9:0", "segment_ids:0", "input_mask:0", "input_ids:0" };
        public string[] ModelOutput => new[] { "unstack:1", "unstack:0", "unique_ids:0" };

        public OnnxModelConfigurator(
            AppSettings appSettings,
            MLContext mlContext)
        {
            this.mlContext = mlContext;
            this.appSettings = appSettings;
            mlModel = SetupMlNetModel();
        }

        private ITransformer SetupMlNetModel()
        {
            bool hasGpu = false;

            var dataView = mlContext.Data
                .LoadFromEnumerable(new List<TFeature>());

            var pipeline = mlContext.Transforms
                            .ApplyOnnxModel(
                                modelFile: appSettings.MachineLearningSettings.LanguageProcessingApiOnnxModelUri,
                                outputColumnNames: ModelOutput,
                                inputColumnNames: ModelInput,
                                gpuDeviceId: hasGpu ? 0 : (int?)null);

            var mlNetModel = pipeline.Fit(dataView);

            return mlNetModel;
        }

        public List<string> GetVocabs()
        {
            if (!vocabulary.IsEmpty())
            {
                return vocabulary;
            }

            vocabulary = ML.Utils.FileReader.ReadVocabularyFile(appSettings.MachineLearningSettings.LanguageProcessingApiVocabUri);

            return vocabulary;
        }

        public PredictionEngine<TFeature, T> GetMlNetPredictionEngine<T>() where T : class, new()
        {
            return mlContext.Model.CreatePredictionEngine<TFeature, T>(mlModel);
        }

        public void SaveMLNetModel(string mlnetModelFilePath)
        {
            mlContext.Model.Save(mlModel, null, mlnetModelFilePath);
        }
    }
}