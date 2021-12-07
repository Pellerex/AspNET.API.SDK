using Microsoft.ML;
using ML.DTO;
using ML.Prediction.Abstract;
using System.Collections.Generic;

namespace ML.Prediction
{
    public class ModelTrainer : IModelTrainer
    {
        private readonly MLContext mlContext;

        public ModelTrainer(MLContext mLContext)
        {
            this.mlContext = mLContext;
        }

        public ITransformer BuildAndTrain(string bertModelPath, bool useGpu)
        {
            var pipeline = mlContext.Transforms
                            .ApplyOnnxModel(modelFile: bertModelPath,
                                            outputColumnNames: new[] { "unstack:1",
                                                                       "unstack:0",
                                                                       "unique_ids:0" },
                                            inputColumnNames: new[] {"unique_ids_raw_output___9:0",
                                                                      "segment_ids:0",
                                                                      "input_mask:0",
                                                                      "input_ids:0" },
                                            gpuDeviceId: useGpu ? 0 : (int?)null);

            return pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<BertInput>()));
        }

    }
}
