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
using TrainCrewTIDWindow.Manager;
using Newtonsoft.Json;
using TrainCrewTIDWindow.Models;
using Timer = System.Windows.Forms.Timer;

namespace TrainCrewTIDWindow.Communications
{
    public class ServerCommunication  {

        private readonly OpenIddictClientService _service;

        private readonly TIDWindow _window;

        private static HubConnection? connection;
        
        internal event Action<ConstantDataToServer>? DataUpdated;

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
                DialogResult result = MessageBox.Show($"サーバ認証中にタイムアウトしました。\n再認証しますか？", "サーバ認証失敗（タイムアウト） | TID - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes) {
                    await CheckUserAuthenticationAsync();
                }
            }

            catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error is OpenIddictConstants.Errors
                                                                               .AccessDenied) {

                _window.LabelStatusText = "Status：サーバ認証失敗（拒否）";
                TaskDialog.ShowDialog(new TaskDialogPage {
                    Caption = "サーバ認証失敗（拒否） | TID - ダイヤ運転会",
                    Heading = "サーバ認証失敗（拒否）",
                    Icon = TaskDialogIcon.Error,
                    Text = "サーバ認証は拒否されました。\n必要な権限を持っていない可能性があります。\n司令主任に連絡してください。\n再試行する場合はアプリケーションを再起動してください。"
                });
            }

            catch (Exception exception) {
                Debug.WriteLine(exception);
                _window.LabelStatusText = "Status：サーバ認証失敗";
                DialogResult result = MessageBox.Show($"サーバ認証に失敗しました。\n再認証しますか？\n\n{exception.Message}\n{exception.StackTrace})", "サーバ認証失敗 | TID - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes) {
                    await CheckUserAuthenticationAsync();
                }
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

                _window.LabelStatusText = "Status：データ正常受信";
            }
            catch (Exception exception) {
                Debug.WriteLine($"Server send failed: {exception.Message}");
                _window.LabelStatusText = "Status：データ受信失敗";
            }
        }
        
        private async Task OnTimedEvent() {
            if (connection == null) return;
            try {
                /*var trackCircuitList = await connection.InvokeAsync<List<TrackCircuitData>>("SendData_TID");
                var data = new ConstantDataToServer {
                    TrackCircuitDatas = trackCircuitList
                };*/
                var data = await connection.InvokeAsync<ConstantDataToServer>("SendData_TID");
                var trackCircuitList = data.TrackCircuitDatas;
                JsonDebugLogManager.AddJsonText(JsonConvert.SerializeObject(trackCircuitList));
                DataUpdated?.Invoke(data);
            }
            catch (Exception exception) {
                Debug.WriteLine($"Server send failed: {exception.Message}");
                Debug.WriteLine(exception.StackTrace);
            }
        }
    }
}
