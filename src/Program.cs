using Spectre.Console;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Diagnostics;

var systemPrompt = "あなたは和歌の名手です。与えられた句を丁寧に解説してください。さらに、より良い句にするための添削を行い、添削のポイントを丁寧に解説してください。";
var userPrompt = "この世をば わが世とぞ思ふ 望月の 欠けたることも なしと思へば";

var prompt = $"<|system|>{systemPrompt}<|end|><|user|>{userPrompt}<|end|><|assistant|>";

// モデルパス
var modelDir = "/workspaces/dotnet-llm/models/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";

Console.WriteLine("start: Loading model");

var sw = Stopwatch.StartNew();
Model model = new(modelDir);
Tokenizer tokenizer = new(model);
sw.Stop();

AnsiConsole.MarkupLine(
    $"End: Loading model. Model loading took [bold blue]{sw.ElapsedMilliseconds}[/] ms"
);

using var tokenizerStream = tokenizer.CreateStream();

var sequences = tokenizer.Encode(prompt);

// 各パラメータを設定
GeneratorParams generatorParams = new(model);
generatorParams.SetSearchOption("max_length", 2048);

using var generator = new Generator(model, generatorParams);
generator.AppendTokenSequences(sequences);

AnsiConsole.MarkupLine(
    "[bold blue]======== gen start ========[/]"
);
while (!generator.IsDone()) {
    generator.GenerateNextToken();
    Console.Write(
        tokenizerStream.Decode(generator.GetSequence(0)[^1])
    );
}

Console.WriteLine();
AnsiConsole.MarkupLine(
    "[bold blue]======== gen end   ========[/]"
);
