using Common;
using ML.DTO;
using ML.Prediction.Abstract;
using ML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ML.Prediction
{
    public class WordTokeniser : ITokeniser
    {
        private readonly List<string> vocabulary;

        public WordTokeniser(AppSettings appSettings,
            OnnxModelConfigurator<BertFeature> onnxModelConfigurator)
        {
            vocabulary = onnxModelConfigurator.GetVocabs();
        }

        public List<(string Token, int VocabularyIndex)> Tokenize(params string[] texts)
        {
            // [CLS] Words of sentence [SEP] Words of next sentence [SEP]
            IEnumerable<string> tokens = new string[] { LanguageProcessingConstants.DefaultTokens.Classification };

            foreach (var text in texts)
            {
                tokens = tokens.Concat(TokenizeSentence(text));
                tokens = tokens.Concat(new string[] { LanguageProcessingConstants.DefaultTokens.Separation });
            }

            return tokens
                .SelectMany(TokenizeSubwords)
                .ToList();
        }

        /**
         * Some words in the vocabulary are too big and will be broken up in to subwords
         * Example "Embeddings"
         * [‘em’, ‘##bed’, ‘##ding’, ‘##s’]
         * https://mccormickml.com/2019/05/14/BERT-word-embeddings-tutorial/
         * https://developpaper.com/bert-visual-learning-of-the-strongest-nlp-model/
         * https://medium.com/@_init_/why-bert-has-3-embedding-layers-and-their-implementation-details-9c261108e28a
         */

        private IEnumerable<(string Token, int VocabularyIndex)> TokenizeSubwords(string word)
        {
            if (vocabulary.Contains(word))
            {
                return new (string, int)[] { (word, vocabulary.IndexOf(word)) };
            }

            var tokens = new List<(string, int)>();
            var remaining = word;

            while (!string.IsNullOrEmpty(remaining) && remaining.Length > 2)
            {
                var prefix = vocabulary.Where(remaining.StartsWith)
                    .OrderByDescending(o => o.Count())
                    .FirstOrDefault();

                if (prefix == null)
                {
                    tokens.Add((LanguageProcessingConstants.DefaultTokens.Unknown, vocabulary.IndexOf(LanguageProcessingConstants.DefaultTokens.Unknown)));

                    return tokens;
                }

                remaining = remaining.Replace(prefix, "##");

                tokens.Add((prefix, vocabulary.IndexOf(prefix)));
            }

            if (!string.IsNullOrWhiteSpace(word) && !tokens.Any())
            {
                tokens.Add((LanguageProcessingConstants.DefaultTokens.Unknown, vocabulary.IndexOf(LanguageProcessingConstants.DefaultTokens.Unknown)));
            }

            return tokens;
        }

        private IEnumerable<string> TokenizeSentence(string text)
        {
            // remove spaces and split the , . : ; etc..
            return text.Split(new string[] { " ", "   ", "\r\n" }, StringSplitOptions.None)
                .SelectMany(o => o.SplitAndKeep(".,;:\\/?!#$%()=+-*\"'–_`<>&^@{}[]|~'".ToArray()))
                .Select(o => o.ToLower());
        }
    }
}