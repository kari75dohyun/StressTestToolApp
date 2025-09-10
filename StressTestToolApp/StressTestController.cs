using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StressTestToolApp // ★Form1.cs와 같은 네임스페이스로 맞추세요!
{
    public class StressTestController
    {
        public int ClientCount { get; set; } = 700;
        public string ServerIp { get; set; } = "127.0.0.1";
        public int ServerPort { get; set; } = 12345;
        public int UdpPort { get; set; } = 54321;
        public List<TestClient> Clients = new List<TestClient>();
        public Action<string>? LogAction = null;

        public async Task StartAsync(CancellationToken token)
        {
            try
            {
                LogAction?.Invoke($"[StressTest] {ClientCount} clients connecting...");

                for (int i = 0; i < ClientCount; ++i)
                {
                    if (token.IsCancellationRequested) break;
                    var client = new TestClient(ServerIp, ServerPort, UdpPort, i, LogAction ?? (_ => { }));
                    Clients.Add(client);
                    _ = client.StartAsync(token);
                    await Task.Delay(100, token); // 생성 딜레이
                }

                //LogAction?.Invoke("[StressTest] All clients started.");

                while (!token.IsCancellationRequested)
                {
                    int active = 0;
                    foreach (var c in Clients)
                        if (c.Connected) active++;
                    //LogAction?.Invoke($"[Stats] Active: {active}/{ClientCount}");
                    CleanupClients();
                    Clients.RemoveAll(c => !c.Connected);
                    await Task.Delay(5000, token);
                }
            }
            catch (OperationCanceledException)
            {
                LogAction?.Invoke("[StressTest] 취소됨 (중지 버튼 클릭됨)");
            }
            catch (Exception ex)
            {
                LogAction?.Invoke($"[StressTest] 예외 발생: {ex.Message}");
            }
        }

        public void CleanupClients()
        {
            Clients.RemoveAll(c => !c.Connected);
        }

    }
}
