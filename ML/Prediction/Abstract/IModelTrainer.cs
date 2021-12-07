using Microsoft.ML;

namespace ML.Prediction.Abstract
{
    public interface IModelTrainer
    {
        ITransformer BuildAndTrain(string bertModelPath, bool useGpu);
    }
}
