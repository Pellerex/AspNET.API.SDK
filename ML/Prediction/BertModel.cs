using Common;
using Microsoft.ML;
using ML.DTO;
using ML.Prediction.Abstract;
using ML.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ML.Prediction
{
    public class BertModel : IBertModel
    {
        private readonly ITokeniser tokeniser;
        private readonly List<string> vocabulary;
        private readonly PredictionEngine<BertFeature, BertPredictionResult> predictionEngine;
        public int BestResultSize { get; set; } = 20;
        public int MaxSequenceLength { get; set; } = 256;
        public int MaxAnswerLength { get; set; } = 30;

        public BertModel(
            ITokeniser tokeniser,
            AppSettings appSettings,
            OnnxModelConfigurator<BertFeature> onnxModelConfigurator)
        {
            this.tokeniser = tokeniser;
            predictionEngine = onnxModelConfigurator.GetMlNetPredictionEngine<BertPredictionResult>();
            vocabulary = onnxModelConfigurator.GetVocabs();
        }

        public (List<string> tokens, float probability) Predict(string context, string question)
        {
            var tokens = tokeniser.Tokenize(question, context);

            var encodedFeature = Encode(tokens);

            var result = predictionEngine.Predict(encodedFeature);

            var contextStart = tokens.FindIndex(o => o.Token == LanguageProcessingConstants.DefaultTokens.Separation);

            var (startIndex, endIndex, probability) = GetBestPredictionFromResult(result, contextStart);

            var predictedTokens = encodedFeature.InputIds
                .Skip(startIndex)
                .Take(endIndex + 1 - startIndex)
                .Select(o => vocabulary[(int)o])
                .ToList();

            var stichedTokens = StitchSentenceBackTogether(predictedTokens);

            return (stichedTokens, probability);
        }

        private List<string> StitchSentenceBackTogether(List<string> tokens)
        {
            var currentToken = string.Empty;

            tokens.Reverse();

            var tokensStitched = new List<string>();

            foreach (var token in tokens)
            {
                if (!token.StartsWith("##"))
                {
                    currentToken = token + currentToken;
                    tokensStitched.Add(currentToken);
                    currentToken = string.Empty;
                }
                else
                {
                    currentToken = token.Replace("##", "") + currentToken;
                }
            }

            tokensStitched.Reverse();

            return tokensStitched;
        }

        private (int StartIndex, int EndIndex, float Probability) GetBestPredictionFromResult(BertPredictionResult result, int minIndex)
        {
            var bestN = BestResultSize;

            var bestStartLogits = result.StartLogits
                .Select((logit, index) => (Logit: logit, Index: index))
                .OrderByDescending(o => o.Logit)
                .Take(bestN);

            var bestEndLogits = result.EndLogits
                .Select((logit, index) => (Logit: logit, Index: index))
                .OrderByDescending(o => o.Logit)
                .Take(bestN);

            var bestResultsWithScore = bestStartLogits
                .SelectMany(startLogit =>
                    bestEndLogits
                    .Select(endLogit =>
                        (
                            StartLogit: startLogit.Index,
                            EndLogit: endLogit.Index,
                            Score: startLogit.Logit + endLogit.Logit
                        )
                     )
                )
                .Where(entry => !(entry.EndLogit < entry.StartLogit || entry.EndLogit - entry.StartLogit > MaxAnswerLength || entry.StartLogit == 0 && entry.EndLogit == 0 || entry.StartLogit < minIndex))
                .Take(bestN);

            var (item, probability) = bestResultsWithScore
                .Softmax(o => o.Score)
                .OrderByDescending(o => o.Probability)
                .FirstOrDefault();

            return (StartIndex: item.StartLogit, EndIndex: item.EndLogit, probability);
        }

        private BertFeature Encode(List<(string Token, int Index)> tokens)
        {
            var padding = Enumerable
                .Repeat(0L, MaxSequenceLength - tokens.Count)
                .ToList();

            var tokenIndexes = tokens
                .Select(token => (long)token.Index)
                .Concat(padding)
                .ToArray();

            var segmentIndexes = GetSegmentIndexes(tokens)
                .Concat(padding)
                .ToArray();

            var inputMask =
                tokens.Select(o => 1L)
                .Concat(padding)
                .ToArray();

            return new BertFeature()
            {
                InputIds = tokenIndexes,
                SegmentIds = segmentIndexes,
                InputMask = inputMask,
                UniqueIds = new long[] { 0 }
            };
        }

        private IEnumerable<long> GetSegmentIndexes(List<(string token, int index)> tokens)
        {
            var segmentIndex = 0;
            var segmentIndexes = new List<long>();

            foreach (var (token, index) in tokens)
            {
                segmentIndexes.Add(segmentIndex);

                if (token == LanguageProcessingConstants.DefaultTokens.Separation)
                {
                    segmentIndex++;
                }
            }

            return segmentIndexes;
        }
    }
}