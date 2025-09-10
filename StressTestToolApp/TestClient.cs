using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StressTestToolApp
{
    public class TestClient
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly int _udpPort;
        private readonly int _id;
        private readonly Action<string> _log;
        private TcpClient? _tcp;
        private SslStream? _ssl;
        private UdpClient? _udpClient;
        private IPEndPoint? _udpRemoteEp;

        private string? _udpToken = null;

        public bool Connected { get; private set; } = false;

        public TestClient(string ip, int port, int udpPort, int id, Action<string> log)
        {
            _ip = ip;
            _port = port;
            _udpPort = udpPort;
            _id = id;
            _log = log;
        }

        public async Task StartAsync(CancellationToken token)
        {
            try
            {
                _tcp = new TcpClient();
                await _tcp.ConnectAsync(_ip, _port);
                _ssl = new SslStream(_tcp.GetStream(), false, (sender, cert, chain, sslPolicyErrors) => true);
                await _ssl.AuthenticateAsClientAsync(_ip);
                Connected = true;
                _log?.Invoke($"[Client {_id}] Connected!");

                // (1) 서버로부터 닉네임 입력 프롬프트 수신 (길이 프리픽스)
                string? noticeMsg = await ReadOneJsonMessageAsync(token);

                if (noticeMsg != null)
                {
                    try
                    {
                        var doc = JsonDocument.Parse(noticeMsg);
                        string type = doc.RootElement.GetProperty("type").GetString() ?? "";
                        string msg = doc.RootElement.GetProperty("msg").GetString() ?? "";
                        _log?.Invoke($"[Client {_id}] Server says: type={type}, msg={msg}");
                    }
                    catch (Exception ex)
                    {
                        _log?.Invoke($"[Client {_id}] JSON Parse Error: {ex.Message}");
                    }
                }

                _log?.Invoke($"[Client {_id}] Server notice: {noticeMsg}");



                // (2) 닉네임 로그인 (길이 프리픽스 붙여서 전송)
                string nickname = $"stress_{_id}";
                string loginJson = $"{{\"type\":\"login\",\"nickname\":\"{nickname}\"}}";
                await SendLengthPrefixedAsync(loginJson);

                // (3) 로그인 성공 응답 수신
                string? loginResp = await ReadOneJsonMessageAsync(token);

                if (loginResp != null)
                {
                    try
                    {
                        var doc = JsonDocument.Parse(loginResp);
                        string type = doc.RootElement.GetProperty("type").GetString() ?? "";
                        string nick_name = doc.RootElement.GetProperty("nickname").GetString() ?? "";
                        _log?.Invoke($"[Client {_id}] Server says: type={type}, nickname={nick_name}");
                    }
                    catch (Exception ex)
                    {
                        _log?.Invoke($"[Client {_id}] JSON Parse Error: {ex.Message}");
                    }
                }

                _log?.Invoke($"[Client {_id}] Login response: {loginResp}");

                // (4) UDP 준비 및 등록 메시지 전송
                _udpClient = new UdpClient();
                _udpRemoteEp = new IPEndPoint(IPAddress.Parse(_ip), _udpPort);

                // UDP 수신 루프 시작!
                //_ = UdpReceiveLoopAsync(token);

                // 서버가 세션 준비할 시간 약간 대기
                await Task.Delay(1000, token);

                string regMsg = $"{{\"type\":\"udp_register\",\"nickname\":\"{nickname}\"}}";
                await SendUdpAsync(regMsg);

/*                if (_udpToken != null)
                {
                    string udpMsg = $"{{\"type\":\"udp_chat\",\"token\":\"{_udpToken}\",\"msg\":\"Hello from stress_{_id}\"}}";
                    await SendUdpAsync(udpMsg);
                }*/

                string? ack = await ReceiveOneUdpMessageAsync(token);
                // ack가 "udp_register_ack" 타입인지 확인

                if (ack != null)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(ack);
                        string type = doc.RootElement.GetProperty("type").GetString() ?? "";
                        string msg = doc.RootElement.GetProperty("msg").GetString() ?? "";
                        string? token_ = doc.RootElement.GetProperty("token").GetString();
                        string nick_name = doc.RootElement.GetProperty("nickname").GetString() ?? "";

                        if (token_ != null)
                        {
                            _udpToken = token_;
                            _log?.Invoke($"[Client {_id}] UDP ack received: type={type}, msg={msg}, token={_udpToken}, nickname={nick_name}");
                        }
                        else
                        {
                            _log?.Invoke($"[Client {_id}] UDP ack received but no token: type={type}, msg={msg}, nickname={nick_name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _log?.Invoke($"UDP ack 파싱 실패: {ex.Message}");
                    }
                }
                else
                {
                    _log?.Invoke("UDP ack 메시지 수신 실패 (null)");
                }

                // (5) 무한 대기 (테스트용)
                await Task.Delay(-1, token);
            }
            catch (Exception ex)
            {
                Connected = false;
                _log?.Invoke($"[Client {_id}] ERROR: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }

        // 길이 프리픽스 붙여서 TCP로 보내기
        private async Task SendLengthPrefixedAsync(string json)
        {
            if (_ssl == null) return;
            byte[] body = Encoding.UTF8.GetBytes(json);
            int len = body.Length;
            byte[] prefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(len)); // 빅엔디안
            await _ssl.WriteAsync(prefix, 0, 4);
            await _ssl.WriteAsync(body, 0, len);
            await _ssl.FlushAsync();
        }

        // 길이 프리픽스 메시지 한 번 읽기
        private async Task<string?> ReadOneJsonMessageAsync(CancellationToken token)
        {
            if (_ssl == null) return null;
            byte[] lenBuf = new byte[4];
            int read = 0;
            while (read < 4)
            {
                int r = await _ssl.ReadAsync(lenBuf, read, 4 - read, token);
                if (r == 0) return null;
                read += r;
            }
            int bodyLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBuf, 0));
            if (bodyLen <= 0 || bodyLen > 100_000) return null; // sanity check
            byte[] bodyBuf = new byte[bodyLen];
            read = 0;
            while (read < bodyLen)
            {
                int r = await _ssl.ReadAsync(bodyBuf, read, bodyLen - read, token);
                if (r == 0) return null;
                read += r;
            }
            return Encoding.UTF8.GetString(bodyBuf, 0, bodyLen);
        }

        private async Task SendUdpAsync(string msg)
        {
            if (_udpClient == null || _udpRemoteEp == null) return;
            byte[] bytes = Encoding.UTF8.GetBytes(msg);
            await _udpClient.SendAsync(bytes, bytes.Length, _udpRemoteEp);
        }

        private async Task UdpReceiveLoopAsync(CancellationToken token)
        {
            if (_udpClient == null) return;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();
                    string msg = Encoding.UTF8.GetString(result.Buffer);

                    // 로그 찍기 (필요시)
                    //_log?.Invoke($"[Client {_id}] UDP 수신: {msg}");

                    // 파싱 시도
                    using var doc = JsonDocument.Parse(msg);
                    var root = doc.RootElement;
                    var type = root.GetProperty("type").GetString();

                    if (type == "udp_register_ack")
                    {
                        // 토큰 저장!
                        if (root.TryGetProperty("token", out var tokenElem))
                        {
                            _udpToken = tokenElem.GetString();
                            _log?.Invoke($"[Client {_id}] [UDP] Token 저장됨: {_udpToken}");
                        }
                    }
                    else if (type == "udp_reply")
                    {
                        _log?.Invoke($"[Client {_id}] [UDP] 에코/응답: {root.GetProperty("msg").GetString()}");
                    }
                    // 필요에 따라 다른 type도 처리
                }
                catch (Exception ex)
                {
                    _log?.Invoke($"[Client {_id}] [UDP수신에러]: {ex.Message}");
                    await Task.Delay(100, token);
                }
            }
        }
        private async Task<string?> ReceiveOneUdpMessageAsync(CancellationToken token)
        {
            if (_udpClient == null) return null;
            try
            {
                var result = await _udpClient.ReceiveAsync(token);
                return Encoding.UTF8.GetString(result.Buffer);
            }
            catch
            {
                return null;
            }
        }

        private void Close()
        {
            try { _ssl?.Close(); } catch { }
            try { _tcp?.Close(); } catch { }
            _log?.Invoke($"[Client {_id}] Disconnected (finalize).");
        }
    }
}
