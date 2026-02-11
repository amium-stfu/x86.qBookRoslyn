using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace qbookCode.Net
{
    public static class PipeNames
    {
        private const string server = @"amium.pipe.server";   // Editor -> Runtime
        private const string client = @"amium.pipe.client";   // Runtime -> Editor

        static long Id = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static string Server = $"{server}-{Id}";
        public static string Client = $"{client}-{Id}";

        public static void ResetPipes()
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Server = $"{server}-{Id}";
            Client = $"{client}-{Id}";
        }
    }

    public class PipeCommand
    {
        public string? Command { get; set; }
        public string[]? Args { get; set; }
    }

    /// <summary>
    /// Serverseite (Runtime):
    ///  - hört auf ToServer (Editor -> Runtime) und feuert OnReceived
    ///  - sendet auf ToClient (Runtime -> Editor) via Send/SendAsync
    ///  - Send ist intern entkoppelt über eine Queue + Hintergrund-Task
    /// </summary>
    public sealed class ServerSide : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _receiveTask;

        // Queue + Sender-Task für nicht-blockierendes Senden
        private readonly BlockingCollection<PipeCommand> _sendQueue = new();
        private readonly Task _sendTask;

        private NamedPipeServerStream? _eventServer;
        private StreamWriter? _eventWriter;
        private readonly SemaphoreSlim _sendLock = new(1, 1);

        /// <summary>
        /// Wird ausgelöst, wenn ein PipeCommand vom Editor ankommt.
        /// </summary>
        public event Action<PipeCommand>? OnReceived;

        public ServerSide()
        {
            // Listener für Commands (Editor -> Runtime)
            _receiveTask = Task.Run(() => CommandLoopAsync(_cts.Token));

            // Sender-Loop, der die Queue seriell abarbeitet
            _sendTask = Task.Run(() => SendLoopAsync(_cts.Token));
        }

        /// <summary>
        /// Fügt ein Kommando in die Send-Queue ein (Runtime -> Editor).
        /// Blockiert praktisch nie im UI/Sequencer-Thread.
        /// </summary>
        public void Send(PipeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            // wenn _cts bereits gecancelt ist, wirft das eine OperationCanceledException
            _sendQueue.Add(command, _cts.Token);
        }

        /// <summary>
        /// Asynchrone Variante – das eigentliche Senden passiert im Hintergrund-Task.
        /// </summary>
        public Task SendAsync(PipeCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            _sendQueue.Add(command, linkedCts.Token);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Arbeitet die Send-Queue im Hintergrund ab.
        /// </summary>
        private async Task SendLoopAsync(CancellationToken token)
        {
            try
            {
                foreach (var command in _sendQueue.GetConsumingEnumerable(token))
                {
                    try
                    {
                        await SendInternalAsync(command, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        // Verbindung abgerissen -> beim nächsten Command wird neu verbunden
                        CleanupEventConnection();
                    }
                    catch
                    {
                        // andere Fehler beim Senden (kannst du bei Bedarf loggen)
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normaler Abbruch
            }
        }

        /// <summary>
        /// Tatsächlicher Send-Vorgang (eine Message). Wird nur aus SendLoopAsync aufgerufen.
        /// </summary>
        private async Task SendInternalAsync(PipeCommand command, CancellationToken token)
        {
            await _sendLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                await EnsureEventConnectionAsync(token).ConfigureAwait(false);
                if (_eventWriter == null)
                    return;

                string line = JsonSerializer.Serialize(command);
                await _eventWriter.WriteLineAsync(line).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        /// <summary>
        /// Sorgt dafür, dass eine verbundene Pipe zu ToClient existiert.
        /// </summary>
        private async Task EnsureEventConnectionAsync(CancellationToken token)
        {
            if (_eventServer != null && _eventWriter != null && _eventServer.IsConnected)
                return;

            CleanupEventConnection();

            _eventServer = new NamedPipeServerStream(PipeNames.Client, PipeDirection.Out);
            await _eventServer.WaitForConnectionAsync(token).ConfigureAwait(false);

            _eventWriter = new StreamWriter(_eventServer, Encoding.UTF8)
            {
                AutoFlush = true
            };
        }

        private void CleanupEventConnection()
        {
            try { _eventWriter?.Dispose(); } catch { /* ignore */ }
            try { _eventServer?.Dispose(); } catch { /* ignore */ }
            _eventWriter = null;
            _eventServer = null;
        }

        /// <summary>
        /// Hört dauerhaft auf ToServer (Editor -> Runtime) und feuert OnReceived pro Message.
        /// JSON pro Zeile: PipeCommand.
        /// </summary>
        private async Task CommandLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(PipeNames.Server, PipeDirection.In);
                    await server.WaitForConnectionAsync(token).ConfigureAwait(false);

                    using var reader = new StreamReader(server, Encoding.UTF8);
                    string? line;

                    while (!token.IsCancellationRequested &&
                           (line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        try
                        {
                            var cmd = JsonSerializer.Deserialize<PipeCommand>(line);
                            if (cmd != null)
                                OnReceived?.Invoke(cmd);
                        }
                        catch (JsonException)
                        {
                            // Ungültige Message -> ignorieren oder loggen
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // normaler Abbruch
                    break;
                }
                catch
                {
                    await Task.Delay(200, token).ConfigureAwait(false);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _sendQueue.CompleteAdding();

            try { await _receiveTask.ConfigureAwait(false); } catch { }
            try { await _sendTask.ConfigureAwait(false); } catch { }

            _cts.Dispose();
            _sendLock.Dispose();
            _sendQueue.Dispose();
            CleanupEventConnection();
        }
    }

    /// <summary>
    /// Clientseite (Editor):
    ///  - sendet auf ToServer (Editor -> Runtime) via Send/SendAsync
    ///  - hört auf ToClient (Runtime -> Editor) und feuert OnReceived
    ///  - Send ist intern entkoppelt über eine Queue + Hintergrund-Task
    /// </summary>
    public sealed class ClientSide : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _receiveTask;

        private readonly BlockingCollection<PipeCommand> _sendQueue = new();
        private readonly Task _sendTask;

        /// <summary>
        /// Wird ausgelöst, wenn ein PipeCommand von der Runtime ankommt.
        /// </summary>
        public event Action<PipeCommand>? OnReceived;

        public ClientSide()
        {
            // Listener für Events (Runtime -> Editor)
            _receiveTask = Task.Run(() => EventLoopAsync(_cts.Token));

            // Sender-Loop für Kommandos (Editor -> Runtime)
            _sendTask = Task.Run(() => SendLoopAsync(_cts.Token));
        }

        /// <summary>
        /// Sende ein Kommando an die Runtime (Editor -> Runtime) über die interne Queue.
        /// </summary>
        public void Send(PipeCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _sendQueue.Add(command, _cts.Token);
        }

        /// <summary>
        /// Asynchrone Variante – das eigentliche Senden passiert im Hintergrund-Task.
        /// </summary>
        public Task SendAsync(PipeCommand command, CancellationToken cancellationToken = default)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);
            _sendQueue.Add(command, linkedCts.Token);
            return Task.CompletedTask;
        }

        private async Task SendLoopAsync(CancellationToken token)
        {
            try
            {
                foreach (var command in _sendQueue.GetConsumingEnumerable(token))
                {
                    try
                    {
                        await SendInternalAsync(command, token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        break;
                    }
                    catch
                    {
                        // Fehler beim Senden ignorieren / loggen
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normaler Abbruch
            }
        }

        /// <summary>
        /// Tatsächlicher Send-Vorgang (eine Message). Wird nur aus SendLoopAsync aufgerufen.
        /// Erstellt für jede Nachricht eine eigene Client-Verbindung.
        /// </summary>
        private static async Task SendInternalAsync(PipeCommand command, CancellationToken token)
        {
            using var client = new NamedPipeClientStream(".", PipeNames.Server, PipeDirection.Out);
            await client.ConnectAsync(token).ConfigureAwait(false);

            using var writer = new StreamWriter(client, Encoding.UTF8)
            {
                AutoFlush = true
            };

            string line = JsonSerializer.Serialize(command);
            await writer.WriteLineAsync(line).ConfigureAwait(false);
        }

        /// <summary>
        /// Hört dauerhaft auf ToClient (Runtime -> Editor) und feuert OnReceived pro Message.
        /// JSON pro Zeile: PipeCommand.
        /// </summary>
        private async Task EventLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var client = new NamedPipeClientStream(".", PipeNames.Client, PipeDirection.In);
                    await client.ConnectAsync(token).ConfigureAwait(false);

                    using var reader = new StreamReader(client, Encoding.UTF8);
                    string? line;

                    while (!token.IsCancellationRequested &&
                           (line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        try
                        {
                            var cmd = JsonSerializer.Deserialize<PipeCommand>(line);
                            if (cmd != null)
                                OnReceived?.Invoke(cmd);
                        }
                        catch (JsonException)
                        {
                            // Ungültige Message -> ignorieren oder loggen
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    await Task.Delay(200, token).ConfigureAwait(false);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _sendQueue.CompleteAdding();

            try { await _receiveTask.ConfigureAwait(false); } catch { }
            try { await _sendTask.ConfigureAwait(false); } catch { }

            _cts.Dispose();
            _sendQueue.Dispose();
        }
    }
}