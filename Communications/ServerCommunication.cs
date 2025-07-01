using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using TrainCrewTIDWindow.Manager;
using TrainCrewTIDWindow.Models;
using System.Net;
using System.Net.WebSockets;

namespace TrainCrewTIDWindow.Communications
{
    public class ServerCommunication : IAsyncDisposable
    {
        private readonly TimeSpan _renewMargin = TimeSpan.FromMinutes(1);
        private readonly OpenIddictClientService _service;
        private readonly TIDWindow _window;

        private HubConnection? _connection;
        private bool _eventHandlersSet = false;

        private string _token = "";
        private string _refreshToken = "";
        private DateTimeOffset _tokenExpiration = DateTimeOffset.MinValue;

        internal event Action<ConstantDataToServer>? DataUpdated;
        internal event Action<bool>? ConnectionStatusChanged;

        private static bool error = false;
        public static bool connected { get; set; } = false;

        // 再接続間隔（ミリ秒）
        private const int ReconnectIntervalMs = 500; // 0.5秒

        public bool Error
        {
            get { return error; }
            set { error = value; }
        }

        public DateTime? UpdatedTime { get; private set; } = null;

        /// <summary>
        /// アプリケーション用のホスト構築
        /// </summary>
        /// <param name="window">TIDWindowのオブジェクト</param>
        /// <param name="address">接続先のアドレス</param>
        public ServerCommunication(TIDWindow window, string address, OpenIddictClientService service)
        {
            _window = window;
            _service = service;
        }

        /// <summary>
        /// インタラクティブ認証を行い、SignalR接続を試みる
        /// </summary>
        /// <returns>ユーザーのアクションが必要かどうか</returns>
        public async Task<bool> Authorize()
        {
            if (!ServerAddress.IsDebug)
            {
                // 認証を行う
                var isAuthenticated = await CheckUserAuthenticationAsync();
                if (!isAuthenticated)
                {
                    return false;
                }
            }

            await DisposeAndStopConnectionAsync(CancellationToken.None); // 古いクライアントを破棄
            InitializeConnection(); // 新しいクライアントを初期化

            // 接続を試みる
            var isActionNeeded = await ConnectAsync();
            if (isActionNeeded)
            {
                return true;
            }

            SetEventHandlers(); // イベントハンドラを設定
            return false;
        }

        /// <summary>
        /// ユーザー認証
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckUserAuthenticationAsync()
        {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));
            return await CheckUserAuthenticationAsync(source.Token);
        }

        /// <summary>
        /// ユーザー認証
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckUserAuthenticationAsync(CancellationToken cancellationToken)
        {
            try
            {
                _window.LabelStatusText = "Status：サーバ認証待機中";
                error = false;

                // 認証フローの開始
                var result = await _service.ChallengeInteractivelyAsync(new()
                {
                    CancellationToken = cancellationToken,
                    Scopes = [ OpenIddictConstants.Scopes.OfflineAccess ]
                });

                // ユーザー認証の完了を待つ
                var resultAuth = await _service.AuthenticateInteractivelyAsync(new()
                {
                    CancellationToken = cancellationToken,
                    Nonce = result.Nonce
                });

                _token = resultAuth.BackchannelAccessToken ?? "";
                _tokenExpiration = resultAuth.BackchannelAccessTokenExpirationDate ?? DateTimeOffset.MinValue;
                _refreshToken = resultAuth.RefreshToken ?? "";

                return true;
            }

            catch (OperationCanceledException)
            {
                error = true;

                _window.LabelStatusText = "Status：サーバ認証失敗（タイムアウト）";
                DialogResult result = MessageBox.Show($"サーバ認証中にタイムアウトしました。\n再認証しますか？", "サーバ認証失敗（タイムアウト） | TID - ダイヤ運転会",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await CheckUserAuthenticationAsync();
                    return r;
                }

                return false;
            }

            catch (OpenIddictExceptions.ProtocolException exception) when (exception.Error is OpenIddictConstants.Errors
                                                                               .AccessDenied)
            {
                error = true;


                 _window.LabelStatusText = "Status：サーバ認証失敗（拒否）";

                TaskDialog.ShowDialog(new TaskDialogPage
                {
                    Caption = "サーバ認証失敗（拒否） | TID - ダイヤ運転会",
                    Heading = "サーバ認証失敗（拒否）",
                    Icon = TaskDialogIcon.Error,
                    Text = "サーバ認証は拒否されました。\n入鋏されていない可能性があります。\n入鋏を受け、必要な権限を取得してください。\n再試行する場合はアプリケーションを再起動してください。"
                });
                return false;
            }

            catch (Exception exception)
            {
                error = true;

                Debug.WriteLine(exception);
                _window.LabelStatusText = "Status：サーバ認証失敗";
                DialogResult result =
                    MessageBox.Show($"サーバ認証に失敗しました。\n再認証しますか？\n\n{exception.Message}\n{exception.StackTrace})",
                        "サーバ認証失敗 | TID - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result == DialogResult.Yes)
                {
                    var r = await Authorize();
                    return r;
                }

                return false;
            }
        }

        /// <summary>
        /// 破棄処理
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAndStopConnectionAsync(CancellationToken.None);
        }

        /// <summary>
        /// コネクションの破棄
        /// </summary>
        private async Task DisposeAndStopConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection == null)
            {
                return;
            }

            try
            {
                await _connection.StopAsync(cancellationToken);
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dispose error: {ex.Message}");
            }

            _connection = null;
            _eventHandlersSet = false;
        }

        /// <summary>
        /// コネクション初期化
        /// </summary>
        private void InitializeConnection()
        {
            if (_connection != null)
            {
                throw new InvalidOperationException("_connection is already initialized.");
            }

            _connection = new HubConnectionBuilder()
                .WithUrl($"{ServerAddress.SignalAddress}/hub/TID?access_token={_token}")
                .Build();
            _eventHandlersSet = false;
        }

        /// <summary>
        /// イベントハンドラ設定
        /// </summary>
        private void SetEventHandlers()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("_connection is not initialized.");
            }

            if (_eventHandlersSet)
            {
                return; // イベントハンドラは一度だけ設定する
            }

            _connection.On<ConstantDataToServer>("ReceiveData", OnReceiveDataFromServer);

            _connection.Closed += async error =>
            {
                Debug.WriteLine(error == null
                    ? "Connection closed normally."
                    : $"Connection closed with error: {error.Message}");

                connected = false;
                ConnectionStatusChanged?.Invoke(connected);
                await TryReconnectAsync();
            };

            _eventHandlersSet = true;
        }

        /// <summary>
        /// 再接続とリフレッシュトークンフロー
        /// </summary>
        /// <returns>ユーザーアクションが必要かどうか</returns>
        private async Task TryReconnectAsync()
        {
            while (true)
            {
                try
                {
                    var isActionNeeded = await TryReconnectOnceAsync();
                    if (isActionNeeded)
                    {
                        return;
                    }

                    if (_connection != null && _connection.State == HubConnectionState.Connected)
                    {
                        Debug.WriteLine("Reconnected successfully.");
                        connected = true;
                        ConnectionStatusChanged?.Invoke(connected);
                        _window.LabelStatusText = "Status：サーバ接続成功";
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Reconnect attempt failed: {ex.Message}");
                    _window.LabelStatusText = "Status：サーバ再接続失敗。再試行中...";
                }

                await Task.Delay(ReconnectIntervalMs);
            }
        }

        /// <summary>
        /// 再接続を一度試みます。
        /// </summary>
        /// <returns>ユーザーによるアクションが必要かどうか</returns>
        private async Task<bool> TryReconnectOnceAsync()
        {
            // トークンが切れていない場合 かつ 切れるまで余裕がある場合はそのまま再接続
            if (_tokenExpiration > DateTimeOffset.UtcNow + _renewMargin)
            {
                Debug.WriteLine("Try reconnect with current token...");
                var isActionNeeded = await ConnectAsync();
                if (isActionNeeded)
                {
                    return true; // アクションが必要な場合はtrueを返す
                }

                SetEventHandlers(); // イベントハンドラを設定
                return false;
            }

            // トークンが切れていてリフレッシュトークンが有効な場合はリフレッシュ
            try
            {
                Debug.WriteLine("Refreshing token...");
                await RefreshTokenWithHandlingAsync(CancellationToken.None);

                await DisposeAndStopConnectionAsync(CancellationToken.None); // 古いクライアントを破棄
                InitializeConnection(); // 新しいクライアントを初期化

                var isActionNeeded = await ConnectAsync();
                if (isActionNeeded)
                {
                    return true;
                }

                SetEventHandlers(); // イベントハンドラを設定
                return false;
            }
            catch (OpenIddictExceptions.ProtocolException ex)
                when (ex.Error is
                          OpenIddictConstants.Errors.InvalidToken
                          or OpenIddictConstants.Errors.InvalidGrant
                          or OpenIddictConstants.Errors.ExpiredToken)
            {
                Debug.WriteLine($"Refresh token error: {ex.Error}");
                // リフレッシュトークンが無効な場合、再認証が必要
                return await HandleTokenRefreshFailure();
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Refresh token is not set.");
                return await HandleTokenRefreshFailure();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unexpected error during token refresh: {ex.Message}");
                return await HandleTokenRefreshFailure();
            }
        }

        /// <summary>
        /// トークンリフレッシュ失敗時の処理
        /// </summary>
        private async Task<bool> HandleTokenRefreshFailure()
        {
            Debug.WriteLine("Refresh token is invalid or expired.");

            DialogResult dialogResult = MessageBox.Show(
                "トークンが切れました。\n再認証してください。\n※いいえを選択した場合、再認証にはアプリケーション再起動が必要です。",
                "認証失敗 | TID - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (dialogResult == DialogResult.Yes)
            {
                var r = await Authorize();
                return r;
            }

            return true;
        }

        /// <summary>
        /// リフレッシュトークンを使用してトークンを更新します。
        /// </summary>
        private async Task RefreshTokenWithHandlingAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                throw new InvalidOperationException("Refresh token is not set.");
            }

            var result = await _service.AuthenticateWithRefreshTokenAsync(new()
            {
                CancellationToken = cancellationToken,
                RefreshToken = _refreshToken
            });

            _token = result.AccessToken;
            _tokenExpiration = result.AccessTokenExpirationDate ?? DateTimeOffset.MinValue;
            _refreshToken = result.RefreshToken ?? "";
            Debug.WriteLine($"Token refreshed successfully");
        }

        /// <summary>
        /// 接続処理
        /// </summary>
        /// <returns>ユーザーのアクションが必要かどうか</returns>
        private async Task<bool> ConnectAsync()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Connection is not initialized.");
            }

            try
            {
                await _connection.StartAsync();
                connected = true;
                ConnectionStatusChanged?.Invoke(connected);
                return false;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                DialogResult dialogResult = MessageBox.Show(
                    "認証が拒否されました。\n再認証してください。",
                    "認証拒否 | TID - ダイヤ運転会", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (dialogResult == DialogResult.Yes)
                {
                    var r = await Authorize();
                    return r;
                }

                return true;
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("Maybe using disposed connection");
                // 一旦接続を破棄して再初期化
                await DisposeAndStopConnectionAsync(CancellationToken.None);
                InitializeConnection();
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Connection error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// サーバーからデータが来たときの処理
        /// </summary>
        /// <param name="data">サーバーから受信されたデータ</param>
        private void OnReceiveDataFromServer(ConstantDataToServer data)
        {
            if (data == null)
            {
                Debug.WriteLine("Failed to receive Data.");
                return;
            }

            try
            {
                var trackCircuitList = data.TrackCircuitDatas;
                DataUpdated?.Invoke(data);
                error = false;
                _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：データ正常受信"; }));
                UpdatedTime = DateTime.Now;
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
            }
            catch (WebSocketException e)
            {
                Debug.WriteLine($"Server send failed: {e.Message}\nerrorCode: {e.WebSocketErrorCode}\n{e.StackTrace}");
                if (!error)
                {
                    error = true;
                    _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：データ受信失敗"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage
                    {
                        Caption = "データ受信失敗 | TID - ダイヤ運転会",
                        Heading = "データ受信失敗",
                        Icon = TaskDialogIcon.Error,
                        Text =
                            $"データの受信に失敗しました。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をお願いします。\nerrorcode:{e.WebSocketErrorCode}"
                    });
                }
            }
            catch (TimeoutException e)
            {
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
                if (!error)
                {
                    error = true;
                    _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：タイムアウト"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage
                    {
                        Caption = "タイムアウト | TID - ダイヤ運転会",
                        Heading = "タイムアウト",
                        Icon = TaskDialogIcon.Error,
                        Text = "サーバとの通信がタイムアウトしました。\n大変恐れ入りますが、アプリケーションの再起動をお願いします。"
                    });
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
                if (!error)
                {
                    error = true;
                    _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：データ受信失敗"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage
                    {
                        Caption = "データ受信失敗 | TID - ダイヤ運転会",
                        Heading = "データ受信失敗",
                        Icon = TaskDialogIcon.Error,
                        Text = "データの受信に失敗しました。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をお願いします。"
                    });
                }
            }
        }

        /*private async Task OnTimedEvent()
        {
            if (connection == null)
            {
                Debug.WriteLine("Connection is not active. Skipping data send.");
                return;
            }
            try
            {
                var data = await connection.InvokeAsync<ConstantDataToServer>("SendData_TID");
                var trackCircuitList = data.TrackCircuitDatas;
                DataUpdated?.Invoke(data);
                error = false;
                _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：データ正常受信"; }));
            }
            catch (WebSocketException e) when (e.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");

            }
            catch (WebSocketException e) {
                Debug.WriteLine($"Server send failed: {e.Message}\nerrorCode: {e.WebSocketErrorCode}\n{e.StackTrace}");
                if (!error) {
                    error = true;
                    _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：データ受信失敗"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage {
                        Caption = "データ受信失敗 | TID - ダイヤ運転会",
                        Heading = "データ受信失敗",
                        Icon = TaskDialogIcon.Error,
                        Text = $"データの受信に失敗しました。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をお願いします。\nerrorcode:{e.WebSocketErrorCode}"
                    });
                }
            }
            catch (TimeoutException e) {
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
                if (!error)
                {
                    error = true;
                    _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：タイムアウト"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage
                    {
                        Caption = "タイムアウト | TID - ダイヤ運転会",
                        Heading = "タイムアウト",
                        Icon = TaskDialogIcon.Error,
                        Text = "サーバとの通信がタイムアウトしました。\n大変恐れ入りますが、アプリケーションの再起動をお願いします。"
                    });
                }
            }
            catch (Exception e) {
                Debug.WriteLine($"Server send failed: {e.Message}\n{e.StackTrace}");
                if (!error)
                {
                    error = true;
                    _window.Invoke(new Action(() => { _window.LabelStatusText = "Status：データ受信失敗"; }));
                    TaskDialog.ShowDialog(new TaskDialogPage
                    {
                        Caption = "データ受信失敗 | TID - ダイヤ運転会",
                        Heading = "データ受信失敗",
                        Icon = TaskDialogIcon.Error,
                        Text = "データの受信に失敗しました。\n復旧を試みますが、しばらく経っても復旧しない場合はアプリケーションの再起動をお願いします。"
                    });
                }
            }
        }*/
    }
}