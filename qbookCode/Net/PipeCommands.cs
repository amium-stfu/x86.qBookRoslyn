using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace qbookCode.Net
{
    public static class PipeCommands
    {

        public static async Task CloseEditor()
        {
            try
            {
                Application.Exit();
            }
            catch (Exception ex)
            {
                Program.LogError($"PipeCommands.CloseEditor error",ex);
            }
        }

        public static Task RuntimeErrors(PipeCommand cmd)
        {
            try
            {
                string[] errorData = cmd.Args ?? Array.Empty<string>();
                RuntimeManager.EnqueueCommand(errorData); // nur in Queue legen
            }
            catch (Exception ex)
            {
                Program.LogError($"PipeCommands.RuntimeErrors: {ex.Message}", ex);
            }

            return Task.CompletedTask;
        }

    }
}
