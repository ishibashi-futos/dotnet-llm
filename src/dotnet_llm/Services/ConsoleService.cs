using System.Text;
using Spectre.Console;

namespace dotnet_llm.Services {
    public class ConsoleService {
        private readonly GenAIService service;

        public ConsoleService(GenAIService service) {
            this.service = service;
        }

        public void Run() {
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
        }
    }
}
