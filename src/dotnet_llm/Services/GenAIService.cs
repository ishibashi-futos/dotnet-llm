using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.ML.OnnxRuntimeGenAI;

[assembly: InternalsVisibleTo("dotnet-llm.Tests")]

namespace dotnet_llm.Services {
    public class GenAIService {

        private Tokenizer tokenizer;
        private TokenizerStream tokenizerStream;
        private Generator generator;
        private readonly Prompt prompt;


        public GenAIService(string modelPath, string systemPrompt) {
            Model model = new(modelPath);
            tokenizer = new(model);
            tokenizerStream = tokenizer.CreateStream();
            GeneratorParams generatorParams = new(model);
            generatorParams.SetSearchOption("max_length", 2048);

            generator = new Generator(model, generatorParams);

            this.prompt = new Prompt(systemPrompt);
        }

        public void Generate(string userPrompt, Action<string> handleMessage) {
            prompt.Append(userPrompt, Role.User);
            var sequences = tokenizer.Encode(prompt.AskAssistant());
            generator.AppendTokenSequences(sequences);

            StringBuilder sb = new();

            while (!generator.IsDone()) {
                generator.GenerateNextToken();
                var chunk = tokenizerStream.Decode(generator.GetSequence(0)[^1]);
                sb.Append(chunk);
                handleMessage(chunk);
            }
            // アシスタントとしての結果をプロンプトに追記
            prompt.Append(sb.ToString(), Role.Assistant);
        }
    }

    enum Role {
        System,
        User,
        Assistant
    }

    internal class Prompt {
        private readonly StringBuilder prompt = new();

        public Prompt(string systemPrompt) {
            Append(systemPrompt, Role.System);
        }

        public void Append(string prompt, Role role) {
            this.prompt.Append(
                $"<|{role.ToString().ToLower()}|>{prompt}<|end|>"
            );
        }

        public string AskAssistant() {
            return $"{prompt}<|assistant|>";
        }

    }

}
