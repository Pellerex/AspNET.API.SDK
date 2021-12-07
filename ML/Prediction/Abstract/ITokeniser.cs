using System.Collections.Generic;

namespace ML.Prediction.Abstract
{
    public interface ITokeniser
    {
        List<(string Token, int VocabularyIndex)> Tokenize(params string[] texts);
    }
}
