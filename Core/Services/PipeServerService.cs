using System.IO.Pipes;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.Core.Services
{
    public class PipeServerService
    {
        private readonly CommunicationService _commService;

        public PipeServerService(CommunicationService commService)
        {
            _commService = commService;
        }

        public async Task StartListenerAsync()
        {
            var server = new NamedPipeServerStream("PVLPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            await server.WaitForConnectionAsync();

            using var reader = new StreamReader(server);
            using var writer = new StreamWriter(server) { AutoFlush = true };

            while (true)
            {
                var jsonRequest = await reader.ReadLineAsync();
                if (jsonRequest == null) break;

                // Simple message type detection
                if (jsonRequest.Contains("\"Username\""))
                {
                    var loginReq = JsonSerializer.Deserialize<LoginRequest>(jsonRequest);
                    var loginResp = _commService.HandleLogin(loginReq);
                    var jsonResponse = JsonSerializer.Serialize(loginResp);
                    await writer.WriteLineAsync(jsonResponse);
                }
                else if (jsonRequest.Contains("\"FileBytes\""))
                {
                    var fileReq = JsonSerializer.Deserialize<FileTransferRequest>(jsonRequest);
                    var success = _commService.HandleFileTransfer(fileReq);
                    await writer.WriteLineAsync(success ? "OK" : "FAIL");
                }
                else
                {
                    await writer.WriteLineAsync("UNKNOWN_REQUEST");
                }
            }
        }
    }
}
