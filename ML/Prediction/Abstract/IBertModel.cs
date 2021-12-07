using System.Collections.Generic;

namespace ML.Prediction.Abstract
{
    public interface IBertModel
    {
        (List<string> tokens, float probability) Predict(string context, string question);
    }
}
