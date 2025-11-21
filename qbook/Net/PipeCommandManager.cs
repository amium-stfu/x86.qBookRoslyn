using qbook.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace qbook.Studio
{
    internal static class PipeCommandManager
    {
        static readonly ConcurrentQueue<PipeCommand> commandQueue = new();
        static readonly Dictionary<string, Func<PipeCommand, Task>> commandHandlers = new();
        static readonly CancellationTokenSource cts = new();
        static Task idle;

        public static void EnqueueCommand(PipeCommand command)
        {
            if (command == null) return;
            commandQueue.Enqueue(command);
        }

        public static void RegisterCommandHandler(string commandName, Func<PipeCommand, Task> handler)
        {
            if (string.IsNullOrWhiteSpace(commandName) || handler == null) return;
            commandHandlers[commandName] = handler;
        }


        private static Task StartIdleProcessing(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (commandQueue.TryDequeue(out PipeCommand command))
                    {
                        try
                        {
                            if (commandHandlers.TryGetValue(command.Command, out var handler))
                            {
                                await handler(command);
                                QB.Logger.Debug($"Executing PipeCommand '{command.Command}'");
                            }
                            else
                            {
                                QB.Logger.Warn($"Unknown PipeCommand '{command.Command}'");
                            }
                        }
                        catch (Exception ex)
                        {
                            QB.Logger.Error($"Failed to process PipeCommand '{command.Command}': {ex.Message}");
                        }
                    }

                    else
                    {
                        await Task.Delay(100, token); // abbrechbar
                    }
                }
            }, token);
        }

        public static void Start()
        {
            idle = StartIdleProcessing(cts.Token);
        }

        public static void Stop()
        {
            cts.Cancel();
            try { idle.Wait(500); } catch { /* ignore */ }
            cts.Dispose();
        }
    }

}
