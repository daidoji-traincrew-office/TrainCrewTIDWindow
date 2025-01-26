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
        public ServerCommunication(TIDWindow window, string address) {
            _window = window;

            // IHostの初期化
            _host = new HostBuilder()
                .ConfigureLogging(options => options.AddDebug())
                .ConfigureServices(services => {
                    // DbContextの設定
                    services.AddDbContext<DbContext>(options => {
                        options.UseSqlite(
                            $"Filename={Path.Combine(Path.GetTempPath(), "trancrew-multiats-client.sqlite3")}");
                        options.UseOpenIddict();
                    });

                    // OpenIddictの設定
                    services.AddOpenIddict()

                        // Register the OpenIddict core components.
                        .AddCore(options => {
                            // Configure OpenIddict to use the Entity Framework Core stores and models.
                            // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
                            options.UseEntityFrameworkCore()
                                .UseDbContext<DbContext>();
                        })

                        // Register the OpenIddict client components.
                        .AddClient(options => {
                            // Note: this sample uses the authorization code flow,
                            // but you can enable the other flows if necessary.
                            options.AllowAuthorizationCodeFlow()
                                .AllowRefreshTokenFlow();

                            // Register the signing and encryption credentials used to protect
                            // sensitive data like the state tokens produced by OpenIddict.
                            options.AddDevelopmentEncryptionCertificate()
                                .AddDevelopmentSigningCertificate();

                            // Add the operating system integration.
                            options.UseSystemIntegration();

                            // Register the System.Net.Http integration and use the identity of the current
                            // assembly as a more specific user agent, which can be useful when dealing with
                            // providers that use the user agent as a way to throttle requests (e.g Reddit).
                            options.UseSystemNetHttp()
                                .SetProductInformation(typeof(Program).Assembly);

                            // Add a client registration matching the client application definition in the server project.
                            options.AddRegistration(new OpenIddictClientRegistration {
                                Issuer = new Uri(address, UriKind.Absolute),

                                ClientId = "MultiATS_Client",
                                RedirectUri = new Uri("/", UriKind.Relative),

                            });
                        });

                    services.AddSingleton(window);

                    // Register the worker responsible for creating the database used to store tokens
                    // and adding the registry entries required to register the custom URI scheme.
                    //
                    // Note: in a real world application, this step should be part of a setup script.
                    services.AddHostedService<Worker>();

                })
                .Build();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            _ = _host.RunAsync();
            _service = _host.Services.GetRequiredService<OpenIddictClientService>();
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
