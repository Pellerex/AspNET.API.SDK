using Business.Services.Abstract;
using ML.Prediction.Abstract;
using System;
using System.Collections.Generic;

namespace Business.Services
{
    public class LanguageProcessingService : ILanguageProcessingService
    {
        private readonly IBertModel bertModel;

        public LanguageProcessingService(
            IBertModel bertModel)
        {
            this.bertModel = bertModel;
        }

        public (double, List<string>, string) Predict(string context, string question)
        {
            var result = bertModel.Predict(context, question);
            return (Math.Round(result.probability * 100, 2), result.tokens, "");
        }
    }
}