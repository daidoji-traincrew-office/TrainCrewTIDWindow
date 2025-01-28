using System;
using System.Diagnostics;
using System.Timers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using TrainCrewTIDWindow.Services;
using Timer = System.Windows.Forms.Timer;

namespace TrainCrewTIDWindow.Communications
{
    public class ServerCommunication  {
        private readonly IHost _host;

        private readonly OpenIddictClientService _service;

        private readonly TIDWindow _window;

        private static HubConnection? connection;
        
        internal event Action<TrainCrewStateData>? TCDataUpdated;

        /// <summary>
        /// アプリケーション用のホスト構築
        /// </summary>
        /// <param name="window">TIDWindowのオブジェクト</param>
        /// <param name="address">接続先のアドレス</param>
        public ServerCommunication(TIDWindow window, string address, OpenIddictClientService service) {
            _window = window;
            _service = service;
             // 1/3秒ごとにデータを送信 
            var timer = new System.Timers.Timer(333);
            // Hook up the Elapsed event for the timer. 
            timer.Elapsed += (_, _) => OnTimedEvent();
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        /// <summary>
        /// ユーザー認証
        /// </summary>
        /// <returns></returns>
        public async Task CheckUserAuthenticationAsync() {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));

            try {
                _window.LabelStatusText = "Status：サーバ認証待機中";

                // 認証フローの開始
                var result = await _service.ChallengeInteractivelyAsync(new() {
                    CancellationToken = source.Token
                });

                // ユーザー認証の完了を待つ
                var resultAuth = await _service.AuthenticateInteractivelyAsync(new() {
                    CancellationToken = source.Token,
                    Nonce = result.Nonce
                });
                var token = resultAuth.BackchannelAccessToken!;

                _window.LabelStatusText = "Status：サーバ認証成功";
                await ConnectAsync(token);
            }

            catch (OperationCanceledException) {

                _window.LabelStatusText = "Status：サーバ認証失敗（タイムアウト）";
                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "サーバ認証失敗（タイムアウト）",
                    Heading = "サーバ認証失敗（タイムアウト）",
                    Icon = TaskDialogIcon.Error,
                    Text = "サーバ認証中にタイムアウトしました。\n再試行するにはアプリケーションを再起動してください。"
                });
            }

            catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error is OpenIddictConstants.Errors
                                                                               .AccessDenied) {

                _window.LabelStatusText = "Status：サーバ認証失敗（拒否）";
                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "サーバ認証失敗（拒否）",
                    Heading = "サーバ認証失敗（拒否）",
                    Icon = TaskDialogIcon.Error,
                    Text = "サーバ認証は拒否されました。\n再試行するにはアプリケーションを再起動してください。"
                });
            }

            catch (Exception exception) {
                Debug.WriteLine(exception);
                _window.LabelStatusText = "Status：サーバ認証失敗";
                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "サーバ認証失敗",
                    Heading = "サーバ認証失敗",
                    Icon = TaskDialogIcon.Error,
                    Text = "サーバ認証に失敗しました。\n再試行するにはアプリケーションを再起動してください。"
                });
            }
        }

        /// <summary>
        /// 接続開始 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ConnectAsync(string token) {
            try {
                // HubConnectionの作成
                connection = new HubConnectionBuilder()
                    .WithUrl($"{ServerAddress.SignalAddress}/hub/TID?access_token={token}")
                    .WithAutomaticReconnect() // 自動再接続
                    .Build();

                // 接続開始
                await connection.StartAsync(); 
            }
            catch (Exception exception) {
                Debug.WriteLine($"Server send failed: {exception.Message}");
            }
        }
        
        private async Task OnTimedEvent() {
            if (connection == null) return;
            try {
                var trackCircuitList = await connection.InvokeAsync<List<TrackCircuitData>>("SendData_TID");
                var data = new TrainCrewStateData
                {
                    trackCircuitList = trackCircuitList
                };
                TCDataUpdated?.Invoke(data);
            }
            catch (Exception exception) {
                Debug.WriteLine($"Server send failed: {exception.Message}");
            }
        }
    }
}
