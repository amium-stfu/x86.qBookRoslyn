using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace qbook.Net
{
    public static class PipeCommands
    {

        public static async Task Rebuild()
        {
            try
            {
                string bookPath = Path.Combine(Core.ThisBook.Directory, Core.ThisBook.Filename);
                await Core.OpenQbookAsync(bookPath);
                BookRuntime.InitializeAll();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PipeCommands.Rebuild error: {ex.Message}");
            }
        }

        public static async Task Run()
        {
            await Task.Run(() =>
            {
                try
                {
                    BookRuntime.RunAll();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.Run error: {ex.Message}");
                }
            });
        }

        public static async Task Destroy()
        {
            await Task.Run(() =>
            {
                try
                {
                    BookRuntime.DestroyAll();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.Destroy error: {ex.Message}");
                }
            });
        }




        public static async Task PageText(PipeCommand command)
        {

            await Task.Run(() =>
            {
                try
                {
                    string pageName = command.Args[0];
                    string newText = command.Args[1];

                    foreach (oPage page in Core.ThisBook.Main.Objects)
                    {
                        if (page.Name == pageName)
                        {
                            page.Text = newText;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageText error: {ex.Message}");
                }
            });
        }

        public static async Task PageFormat(PipeCommand command)
        {

            await Task.Run(() =>
            {
                try
                {
                    string pageName = command.Args[0];
                    string newFormat = command.Args[1];

                    foreach (oPage page in Core.ThisBook.Main.Objects)
                    {
                        if (page.Name == pageName)
                        {
                            page.Format = newFormat;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageFormat error: {ex.Message}");
                }
            });
        }

        public static async Task HidePage(PipeCommand command)
        {

            await Task.Run(() =>
            {
                try
                {
                    string pageName = command.Args[0];
                    string set = command.Args[1];

                    foreach (oPage page in Core.ThisBook.Main.Objects)
                    {
                        if (page.Name == pageName)
                        {
                            page.Hidden = set.ToLower() == "true" ? true : false;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageHiden error: {ex.Message}");
                }
            });
        }

        public static async Task PageOrder(PipeCommand command)
        {

            await Task.Run(() =>
            {
                try
                {
                    Core.ThisBook.PageOrder = command.Args.ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"PipeCommands.PageOrder error: {ex.Message}");
                }
            });
        }




    }
}
