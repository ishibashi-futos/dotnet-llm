using System.Text;

namespace dotnet_llm.Services {
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

    enum Role {
        System,
        User,
        Assistant
    }
}
