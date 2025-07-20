using System.Text;
using dotnet_llm.Services;
using Spectre.Console;

// reference: https://github.com/LouisShark/chatgpt_system_prompt/blob/main/prompts/official-product/google/gemini-pro-20240603.md
var systemPrompt = @"
You are Phi-3, an advanced AI model.

You prioritize the accuracy of your response.

You are a helpful and harmless AI assistant and will always adhere to the safety guidelines. You are not capable of generating harmful or unsafe content.

You are not able to perform any actions in the physical world, such as setting timers or alarms, controlling lights, making phone calls, sending text messages, creating reminders, taking notes, adding items to lists, creating calendar events, scheduling meetings, or taking screenshots.

You do not have personal opinions, but you can generate human-like text in response to a wide range of prompts and questions, e.g., to write creative stories or poems, or to summarize factual topics or create reports.

For contentious topics without broad consensus, you provide a neutral response summarizing the relevant points of view without taking a side. If asked to represent a specific side of a contentious issue, you follow the user's instructions while maintaining a neutral, distanced tone.";

// モデルパス
var modelDir = "/workspaces/dotnet-llm/models/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";

var service = new GenAIService(modelDir, systemPrompt);

// --- アプリケーションの状態を管理する変数 ---
var inputBuilder = new StringBuilder();
var outputHistory = new List<string> { "ここにAIの応答が表示されます。" };
int historyIndex = 0;
bool isAcceptingInput = true; // true: 入力モード, false: 履歴閲覧モード
var temporaryOutputBuilder = new StringBuilder();
var isGenerating = false;

AnsiConsole.Live(
    new Grid()
).Start(ctx => {
    void RenderLayout() {
        string inputContent = isAcceptingInput
            ? inputBuilder.ToString() + "█"
            : "[grey](何かキーを押して入力を開始)[/]";
        var inputPanel = new Panel(inputContent)
            .Header("入力欄")
            .Border(BoxBorder.Rounded)
            .Expand();

        string outputContent = outputHistory.Any() ? outputHistory[historyIndex] : "";
        var outputPanel = new Panel(outputContent)
            .Header("出力欄")
            .Border(BoxBorder.Rounded)
            .Expand();

        var leftGrid = new Grid()
            .AddColumn()
            .AddRow(inputPanel)
            .AddRow(outputPanel);

        var rightPanel = new Panel(temporaryOutputBuilder.ToString())
            .Header("ライブ出力")
            .Border(BoxBorder.Rounded)
            .Expand();

        Grid grid;

        if (isGenerating) {
            grid = new Grid()
                .AddColumn()
                .AddColumn()
                .AddRow(leftGrid, rightPanel)
                .Expand();
        }
        else {
            grid = new Grid()
                .AddColumn()
                .AddRow(leftGrid)
                .Expand();
        }

        ctx.UpdateTarget(grid);
    }

    // --- メインの入力ループ ---
    while (true) {
        RenderLayout(); // 常に最新の状態で画面を描画

        var keyInfo = Console.ReadKey(true);

        // Escキーでアプリケーションを終了
        if (keyInfo.Key == ConsoleKey.Escape) break;

        if (isAcceptingInput) {
            // --- 入力モードの処理 ---
            switch (keyInfo.Key) {
                case ConsoleKey.Enter:
                    if (inputBuilder.Length > 0) {
                        isGenerating = true;
                        var submittedText = inputBuilder.ToString();
                        service.Generate(submittedText, (message) => {
                            temporaryOutputBuilder.Append(message);
                            RenderLayout(); // トークンごとに画面を更新する
                        });
                        outputHistory.Add(temporaryOutputBuilder.ToString());
                        historyIndex = outputHistory.Count - 1;
                        temporaryOutputBuilder.Clear();
                        inputBuilder.Clear();
                        isGenerating = false;
                        isAcceptingInput = false; // 履歴閲覧モードに移行
                    }
                    break;
                case ConsoleKey.Backspace:
                    if (inputBuilder.Length > 0) inputBuilder.Length--;
                    break;
                default:
                    if (!char.IsControl(keyInfo.KeyChar)) inputBuilder.Append(keyInfo.KeyChar);
                    break;
            }
        }
        else {
            // --- 履歴閲覧モードの処理 ---
            switch (keyInfo.Key) {
                case ConsoleKey.UpArrow:
                    if (historyIndex > 0) historyIndex--;
                    break;
                case ConsoleKey.DownArrow:
                    if (historyIndex < outputHistory.Count - 1) historyIndex++;
                    break;
                default:
                    // 何か文字が入力されたら、入力モードに戻る
                    if (!char.IsControl(keyInfo.KeyChar)) {
                        isAcceptingInput = true;
                        inputBuilder.Clear();
                        inputBuilder.Append(keyInfo.KeyChar);
                    }
                    break;
            }
        }
    }
});
