using dotnet_llm.Services;
using Xunit;

namespace dotnet_llm.tests;

public class PromptTests
{
    [Fact]
    public void Constructor_ShouldSetSystemPromptCorrectly()
    {
        // Arrange
        var systemPrompt = "You are a helpful assistant.";

        // Act
        var prompt = new Prompt(systemPrompt);
        var result = prompt.AskAssistant();

        // Assert
        Assert.Equal($"<|system|>{systemPrompt}<|end|><|assistant|>", result);
    }

    [Fact]
    public void Append_UserRole_ShouldAppendUserPromptCorrectly()
    {
        // Arrange
        var systemPrompt = "System message";
        var userPrompt = "User message";
        var prompt = new Prompt(systemPrompt);

        // Act
        prompt.Append(userPrompt, Role.User);
        var result = prompt.AskAssistant();

        // Assert
        var expected = $"<|system|>{systemPrompt}<|end|><|user|>{userPrompt}<|end|><|assistant|>";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Append_AssistantRole_ShouldAppendAssistantPromptCorrectly()
    {
        // Arrange
        var systemPrompt = "System message";
        var assistantPrompt = "Assistant message";
        var prompt = new Prompt(systemPrompt);

        // Act
        prompt.Append(assistantPrompt, Role.Assistant);
        var result = prompt.AskAssistant();

        // Assert
        var expected = $"<|system|>{systemPrompt}<|end|><|assistant|>{assistantPrompt}<|end|><|assistant|>";
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Append_MultiplePrompts_ShouldBuildFullConversation()
    {
        // Arrange
        var systemPrompt = "You are a helpful assistant.";
        var userPrompt1 = "Hello!";
        var assistantResponse1 = "Hi there! How can I help?";
        var userPrompt2 = "What is C#?";
        var prompt = new Prompt(systemPrompt);

        // Act
        prompt.Append(userPrompt1, Role.User);
        prompt.Append(assistantResponse1, Role.Assistant);
        prompt.Append(userPrompt2, Role.User);
        var result = prompt.AskAssistant();

        // Assert
        var expected = $"<|system|>{systemPrompt}<|end|><|user|>{userPrompt1}<|end|><|assistant|>{assistantResponse1}<|end|><|user|>{userPrompt2}<|end|><|assistant|>";
        Assert.Equal(expected, result);
    }
}