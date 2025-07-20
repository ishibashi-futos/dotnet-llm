using Microsoft.ML.OnnxRuntimeGenAI;

namespace dotnet_llm.Services {
    public class TokenizerService {
        private readonly Tokenizer tokenizer;
        private readonly TokenizerStream tokenizerStream;

        public TokenizerService(Model model) {
            tokenizer = new Tokenizer(model);
            tokenizerStream = tokenizer.CreateStream();
        }

        public TokenizerStream GetTokenizerStream() {
            return tokenizerStream;
        }

        public Tokenizer GetTokenizer() {
            return tokenizer;
        }
    }
}
