namespace Common.Abstract
{
    public interface IOnnxObjectPrediction
    {
        float[] PredictedLabels { get; set; }
    }
}
