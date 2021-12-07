using System.Collections.Generic;

namespace Business.Services.Abstract
{
    public interface ILanguageProcessingService
    {
        (double, List<string>, string) Predict(string context, string question);
    }
}