using dotnet_llm.Services;

var systemPrompt = "あなたは和歌の名手です。与えられた句を丁寧に解説してください。さらに、より良い句にするための添削を行い、添削のポイントを丁寧に解説してください。";
var userPrompt = "この世をば わが世とぞ思ふ 望月の 欠けたることも なしと思へば";

var prompt = $"<|system|>{systemPrompt}<|end|><|user|>{userPrompt}<|end|><|assistant|>";

// モデルパス
var modelDir = "/workspaces/dotnet-llm/models/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";

var service = new GenAIService(modelDir, systemPrompt);
service.Generate(userPrompt, Console.Write);
