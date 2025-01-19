using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Services;

namespace TrainCrewTIDWindow.Communications
{
    public class ServerCommunication {
        private readonly OpenIddictClientService _service;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="service"></param>
        public ServerCommunication(OpenIddictClientService service) {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// ユーザー認証
        /// </summary>
        /// <returns></returns>
        public async Task CheckUserAuthenticationAsync() {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));

            try {
                // 認証フローの開始
                var result = await _service.ChallengeInteractivelyAsync(new() {
                    CancellationToken = source.Token
                });

                // ユーザー認証の完了を待つ
                var resultAuth = await _service.AuthenticateInteractivelyAsync(new() {
                    CancellationToken = source.Token,
                    Nonce = result.Nonce
                });
                var token = resultAuth.BackchannelAccessToken;

                Debug.WriteLine($"Authentication successful. Token: {token}");
            }

            catch (OperationCanceledException) {
                Debug.WriteLine("Authentication timed out. The process was aborted.");
            }

            catch (OpenIddictExceptions.ProtocolException exception)
                when (exception.Error is OpenIddictConstants.Errors.AccessDenied) {
                Debug.WriteLine("The authorization was denied by the end user.");
            }

            catch (Exception exception) {
                Debug.WriteLine($"Authentication failed: {exception.Message}");
            }
        }

        /// <summary>
        /// サーバーへリクエスト送信
        /// </summary>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task SendRequestAsync(string token, CancellationToken cancellationToken) {
            try {
                // HubConnectionの作成
                await using var client = new HubConnectionBuilder()
                    .WithUrl($"{ServerAddress.SignalAddress}/hub/TID?access_token={token}")
                    .WithAutomaticReconnect() // 自動再接続
                    .Build();

                // 接続開始
                await client.StartAsync(cancellationToken);

                // サーバーメソッドの呼び出し
                var resource = await client.InvokeAsync<int>("Emit", cancellationToken);
            }
            catch (Exception exception) {
                Debug.WriteLine($"Server send failed: {exception.Message}");
            }
        }

        /// <summary>
        /// アプリケーション用のホスト構築
        /// </summary>
        /// <returns>IHost</returns>
        private static async Task BuildHost() {
            var host = new HostBuilder()
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
                        .AddCore(options => {
                            options.UseEntityFrameworkCore()
                                .UseDbContext<DbContext>();
                        })
                        .AddClient(options => {
                            options.AllowAuthorizationCodeFlow()
                                .AllowRefreshTokenFlow();

                            options.AddDevelopmentEncryptionCertificate()
                                .AddDevelopmentSigningCertificate();

                            options.AddRegistration(new OpenIddictClientRegistration {
                                Issuer = new Uri(ServerAddress.SignalAddress + "/hub/TID", UriKind.Absolute),
                                ClientId = "MultiATS_Client",
                                RedirectUri = new Uri("/", UriKind.Relative),
                            });
                        });

                    // Workerサービスを登録
                    services.AddHostedService<Worker>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
