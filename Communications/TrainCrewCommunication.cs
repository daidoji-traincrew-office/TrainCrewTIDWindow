using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace TrainCrewTIDWindow.Communications
{

    /// <summary>
    /// TRAIN CREW本体用WebSocketクライアント通信クラス（すいねさん作TrainCrewAPIv3のCommunicationを流用）
    /// </summary>
    public class TrainCrewCommunication
    {
        private ClientWebSocket ws;
        internal TrainCrewStateData TcData { get; private set; } = new TrainCrewStateData();
        internal CommandToTrainCrew Cmd { get; private set; } = new CommandToTrainCrew();
        internal event Action<string>? ConnectionStatusChanged;
        internal event Action<TrainCrewStateData>? TCDataUpdated;
        private string[] request = new[] { "all" };
        // JSONシリアライザ設定
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        /// <summary>
        /// TrainCrew側データ要求引数
        /// (all, trackcircuit, signal, train)
        /// </summary>
        internal string[] Request
        {
            get => request;
            set
            {
                if (value != null)
                {
                    if (value.Length == 1 && value[0] == "all" ||
                        value.All(str => str == "trackcircuit" || str == "signal" || str == "train"))
                    {
                        request = value;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid string");
                    }
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TrainCrewCommunication()
        {
            ws = new ClientWebSocket();
        }

        /// <summary>
        /// 受信データ処理メソッド
        /// </summary>
        private void ProcessingReceiveData()
        {
            // その他処理など…
        }

        /// <summary>
        /// WebSocket接続試行
        /// </summary>
        /// <returns></returns>
        internal async Task TryConnectWebSocket()
        {
            while (true)
            {
                ws = new ClientWebSocket();

                try
                {
                    // 接続処理
                    await ConnectWebSocket();
                    break;
                }
                catch (Exception)
                {
                    ConnectionStatusChanged?.Invoke("Status：接続待機中...");
                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// WebSocket接続処理
        /// </summary>
        /// <returns></returns>
        private async Task ConnectWebSocket()
        {
            //TRAIN CREWのポート番号は50300
            await ws.ConnectAsync(new Uri("ws://localhost:50300/"), CancellationToken.None);

            Cmd.command = "data req";
            Cmd.args = request;

            string json = JsonConvert.SerializeObject(Cmd);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            // 受信処理
            await ReceiveMessages();
        }

        /// <summary>
        /// WebSocket受信処理
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveMessages()
        {
            var buffer = new byte[2048];
            var messageBuilder = new StringBuilder();

            while (ws.State == WebSocketState.Open)
            {
                ConnectionStatusChanged?.Invoke("Status：接続完了");

                WebSocketReceiveResult result;
                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // サーバーからの切断要求を受けた場合
                        await CloseAsync();
                        ConnectionStatusChanged?.Invoke("Status：接続待機中...");
                        await TryConnectWebSocket();
                        return;
                    }
                    else
                    {
                        string partMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(partMessage);
                    }

                } while (!result.EndOfMessage);

                string jsonResponse = messageBuilder.ToString();
                messageBuilder.Clear();


                // 一旦Data_Base型でデシリアライズ
                var baseData = JsonConvert.DeserializeObject<Data_Base>(jsonResponse, JsonSerializerSettings);


                if (baseData != null)
                {
                }
                // JSON受信データ処理
                lock (TcData)
                {
                    // Typeプロパティに応じて処理
                    if (baseData.Type == "TrainCrewStateData")
                    {
                        var newData = JsonConvert.DeserializeObject<TrainCrewStateData>(baseData.Data.ToString());
                        if (newData != null)
                        {
                            UpdateFieldsAndProperties(TcData, newData);
                        }

                        // MainFormへTCData受け渡し
                        TCDataUpdated?.Invoke(TcData);

                        // その他処理
                        ProcessingReceiveData();
                    }
                }
            }
        }

        /// <summary>
        /// WebSocket終了処理
        /// </summary>
        /// <returns></returns>
        private async Task CloseAsync()
        {
            if (ws != null)
            {
                if (ws.State == WebSocketState.Open)
                {
                    // 正常に接続を閉じる
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
                }
                ws.Dispose();
            }
        }

        /// <summary>
        /// フィールド・プロパティ置換メソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private void UpdateFieldsAndProperties<T>(T target, T source) where T : class
        {
            if (target == null || source == null)
            {
                throw new ArgumentNullException("target or source cannot be null");
            }

            foreach (var property in target.GetType().GetProperties())
            {
                if (property.CanWrite)
                {
                    var newValue = property.GetValue(source);
                    property.SetValue(target, newValue);
                }
            }

            foreach (var field in target.GetType().GetFields())
            {
                var newValue = field.GetValue(source);
                field.SetValue(target, newValue);
            }
        }
    }
}
