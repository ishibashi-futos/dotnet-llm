using System.Text;

namespace dotnet_llm.Services {
    public class PromptManager {
        private readonly Prompt prompt;

        public PromptManager(string systemPrompt) {
            prompt = new Prompt(systemPrompt);
        }

        public void AppendUserPrompt(string userPrompt) {
            prompt.Append(userPrompt, Role.User);
        }

        public void AppendAssistantResponse(string response) {
            prompt.Append(response, Role.Assistant);
        }

        public string GetPromptForAssistant() {
            return prompt.AskAssistant();
        }
    }
}
