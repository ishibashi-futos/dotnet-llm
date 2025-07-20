using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.ML.OnnxRuntimeGenAI;
using dotnet_llm.Services;

[assembly: InternalsVisibleTo("dotnet-llm.Tests")]

namespace dotnet_llm.Services {
    public class GenAIService {

        private readonly TokenizerService tokenizerService;
        private Generator generator;
        private readonly PromptManager promptManager;


        public GenAIService(string modelPath, string systemPrompt) {
            Model model = new(modelPath);
            tokenizerService = new TokenizerService(model);
            promptManager = new PromptManager(systemPrompt);
            GeneratorParams generatorParams = new(model);
            generatorParams.SetSearchOption("max_length", 2048);

            generator = new Generator(model, generatorParams);
        }

        public void Generate(string userPrompt, Action<string> handleMessage) {
            promptManager.AppendUserPrompt(userPrompt);
            var sequences = tokenizerService.GetTokenizer().Encode(promptManager.GetPromptForAssistant());
            generator.AppendTokenSequences(sequences);

            StringBuilder sb = new();

            while (!generator.IsDone()) {
                generator.GenerateNextToken();
                var chunk = tokenizerService.GetTokenizerStream().Decode(generator.GetSequence(0)[^1]);
                sb.Append(chunk);
                handleMessage(chunk);
            }
            // アシスタントとしての結果をプロンプトに追記
            promptManager.AppendAssistantResponse(sb.ToString());
        }
    }
}
