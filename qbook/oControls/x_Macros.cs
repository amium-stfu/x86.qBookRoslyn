using CefSharp.DevTools.DOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main
{
    public class x_Macros
    {

        //static string Table ="";
        static void SetValue(string name, int row, int column, string text)
        {
            string table = Main.Qb.GetS(null, name);
            if (table == null)
                table = "";
            if (row < 1)
                return;
            if (column < 1)
                return;
            string[] rows = table.Replace("\r\n", "\n").Split('\n');
            while (rows.Length < row)
            {
                table += "\r\n";
                rows = table.Replace("\r\n", "\n").Split('\n');
            }
            string[] columns = rows[row - 1].Split(';');
            while (columns.Length < column)
            {
                rows[row - 1] += ";";
                columns = rows[row - 1].Split(';');
            }
            columns[column - 1] = text;

            rows[row - 1] = string.Join(";", columns);
            table = string.Join("\r\n", rows);

            Main.Qb.Set(null, name, string.Join("\r\n", rows));
        }



        static bool lincheckRunning = false;
        static public void Lincheck(string args)//string dut, string gd)
        {
            if (lincheckRunning)
                return;
            lincheckRunning = true;
            string src = "lincheck";
            Main.Qb.Set(null, "status", "lincheck");
            string lcSteps = "ZG:60s, SG:60s";//, ZG:120s, 0%, 10%, 20%, 30%, 40%, 50%, 60%, 70%, 80%, 90%, 100%, ZG:30s";
            var argList = args.Replace(" ", "").Replace("lincheck(", "").Replace(")", "").SplitOutsideQuotesAndParenthesis();

            string gd = "gd0";
            string dut = "";
            string mut = "";

            foreach (var arg in argList)
            {
                var splits = arg.Split(new char[] { '=' }, 2);
                if (splits.Count() == 2)
                {
                    if (splits[0].Trim() == "steps")
                        lcSteps = splits[1].Trim().Trim('\"');
                    if (splits[0].Trim() == "gd")
                        gd = splits[1].Trim().Trim('\"');
                    if (splits[0].Trim() == "dut")
                        dut = splits[1].Trim().Trim('\"');
                    if (splits[0].Trim() == "mut")
                        mut = splits[1].Trim().Trim('\"');
                }
            }

            string gdSteps = "";
            string a = Main.Qb.GetS(null, gd + ".steps");
            if (a != null)
                gdSteps = a.Trim();

            string[] als = gdSteps.Split(new char[] { ' ' });
            Main.Qb.Set(null, src, "");

            int row = 1;
            SetValue(src, row, 1, "Date");
            SetValue(src, row++, 2, DateTime.Now.ToString("yyyy-MM-dd"));
            SetValue(src, row, 1, "Time");
            SetValue(src, row++, 2, DateTime.Now.ToString("HH:mm:ss"));
            SetValue(src, row++, 1, "User");
            SetValue(src, row++, 1, "Cal-Id");
            SetValue(src, row, 1, "GD-Id");
            SetValue(src, row++, 2, Main.Qb.GetS(null, gd + ".id"));
            SetValue(src, row, 1, "DUT-Id");
            SetValue(src, row++, 2, Main.Qb.GetS(null, dut + ".id"));

            row = 10;
            SetValue(src, row, 2, "  step");
            SetValue(src, row, 3, "  time");
            SetValue(src, row, 4, "  set");
            SetValue(src, row, 5, "  read");
            SetValue(src, row, 6, "  dev/abs");
            SetValue(src, row, 7, "  dev/rel");

            row = 11;
            SetValue(src, row, 2, "");
            SetValue(src, row, 3, "  [s]");
            SetValue(src, row, 4, "");
            SetValue(src, row, 5, "");
            SetValue(src, row, 6, "");
            SetValue(src, row, 7, "  [%]");

            double span;
            span = Main.Qb.GetD(null, dut + ".spangas");
            row++;

            Main.Qb.ScriptingEngine.RunScript(dut + ".remote()");
            Main.Qb.ScriptingEngine.RunScript(gd + ".remote()");
            Task.Delay(500).Wait();
            Main.Qb.ScriptingEngine.RunScript(gd + ".spangasSet(" + span + ")");
            Task.Delay(500).Wait();
            Main.Qb.ScriptingEngine.RunScript(gd + ".ready()");
            Task.Delay(500).Wait();
            Main.Qb.ScriptingEngine.RunScript(dut + ".sample()");
            Main.Qb.ScriptingEngine.RunScript(gd + ".lincheck()");

            foreach (string step in lcSteps.Split(','))
            {
                string[] st = step.Trim().Split(':');
                int duration = 60000;
                string cmd = st[0].Trim();

                if (st.Length > 1)
                {
                    if (st[1].Trim().EndsWith("ms"))
                        duration = int.Parse(st[1].Trim().Replace("ms", ""));
                    else if (st[1].Trim().EndsWith("s"))
                        duration = int.Parse(st[1].Trim().Replace("s", "")) * 1000;
                    else if (st[1].Trim().EndsWith("m"))
                        duration = int.Parse(st[1].Trim().Replace("m", "")) * 60000;
                    else
                        duration = int.Parse(st[1].Trim());
                }

                SetValue(src, row, 2, cmd);
                SetValue(src, row, 3, "" + (duration / 1000));

                if (cmd == "ZG")
                {
                    Task.Delay(500).Wait();
                    Main.Qb.ScriptingEngine.RunScript(dut + ".sampleZero()");
                    Main.Qb.Set(null, mut + ".set", 0);
                    int delay = duration / 1000;
                    for (int i = 0; i < delay; i++)
                    {
                        Task.Delay(1000).Wait();
                        Main.Qb.Set(null, gd + ".status", "0");
                        SetValue(src, row, 3, ((delay - i - 1)).ToString("0"));
                    }
                    Main.Qb.Set(null, mut + ".set", 0);
                    Main.Qb.ScriptingEngine.RunScript(dut + ".sample()");
                }
                else if (cmd == "SG")
                {
                    Task.Delay(500).Wait();
                    Main.Qb.ScriptingEngine.RunScript(dut + ".sampleSpan()");
                    Main.Qb.Set(null, mut + ".set", span);
                    int delay = duration / 1000;
                    for (int i = 0; i < delay; i++)
                    {
                        Task.Delay(1000).Wait();
                        Main.Qb.Set(null, gd + ".status", "100");
                        SetValue(src, row, 3, ((delay - i - 1)).ToString("0"));
                    }
                    Main.Qb.Set(null, mut + ".set", span);
                    Main.Qb.ScriptingEngine.RunScript(dut + ".sample()");
                }
                else
                {
                    double division = 0;
                    if (cmd.EndsWith("%"))
                    {
                        division = double.Parse(cmd.Substring(0, cmd.Length - 1));
                    }
                    else
                    {
                        int i = 0;
                        division = 0;
                        while (cmd.Trim() != als[i * 2].Trim())
                        {
                            i++;
                            if ((i * 2) > als.Length)
                            {
                                division = 0;
                                i = 0;
                                break;
                            }
                        }
                        division = double.Parse(als[i * 2 + 1]);
                        Main.Qb.ScriptingEngine.RunScript(gd + ".step(" + int.Parse(cmd) + ")");
                    }
                    SetValue(src, row, 2, cmd + "/" + division + "%");
                    double set = span * division / 100.0f;
                    SetValue(src, row, 4, set.ToString("0.0"));
                    Main.Qb.Set(null, gd + ".status", "" + division);
                    Main.Qb.Set(null, mut + ".set", set);

                    int delay = duration / 1000;
                    for (int i = 0; i < delay; i++)
                    {
                        Task.Delay(1000).Wait();
                        double read = Main.Qb.GetD(null, mut + ".read");
                        double diff = read - set;
                        Main.Qb.Set(null, mut + ".set", set);

                        SetValue(src, row, 3, ((delay - i - 1)).ToString("0"));
                        SetValue(src, row, 5, read.ToString("0.0"));
                        SetValue(src, row, 6, diff.ToString("0.0"));
                        if (set == 0)
                            SetValue(src, row, 7, "-");
                        else
                            SetValue(src, row, 7, (diff * 100.0 / set).ToString("0.00"));
                    }
                }
                Main.Qb.Book.Modified = true;
                row++;
            }
            Main.Qb.ScriptingEngine.RunScript(gd + ".step(" + 0 + ")");
            Main.Qb.Set(null, mut + ".set", 0);
            Main.Qb.ScriptingEngine.RunScript(gd + ".ready()");
            Main.Qb.Set(null, "status", "");
            lincheckRunning = false;
        }

        static bool egcRun = false;
        static public void Egc(string args)
        {
            args = args.Replace(" ", "").Replace("egc(", "").Replace(")", "");
            double set = 0;// args.GetItemDoubleValue("set", 0);
            var argList = args.Replace(" ", "").Replace("lincheck(", "").Replace(")", "").SplitOutsideQuotesAndParenthesis();
            string mut = "egc0.efm.read";
            // mut = "egc0.m290.read";

            foreach (var arg in argList)
            {
                var splits = arg.Split(new char[] { '=' }, 2);
                if (splits.Count() == 2)
                {
                    if (splits[0].Trim() == "set")
                        set = splits[1].Trim().Trim('\"').ToDouble();
                    if (splits[0].Trim() == "mut")
                        mut = splits[1].Trim().Trim('\"');
                }
            }

            if (set == 0)
            {
                Main.Qb.Set(null, "egc0.m310.set", 0);
                Main.Qb.SetInternal(null, "egc0.efm.set", 0);
                egcRun = false;
                return;
            }
            if (egcRun)
            {
                egcRun = false;
                Task.Delay(500).Wait();
            }
            egcRun = true;
            qbAutomation.Pid pid = new qbAutomation.Pid();
             pid.init(0.1, 5, 3, 2);
            /*
            pid.t = 0.1;
            pid.ks = 5;
            pid.tu = 3;
            pid.tg = 2;
            */
            pid.set.value = set;
            Main.Qb.SetInternal(null, "egc0.efm.set", set);
            double o = 0;
            //   pid.Set = 630;

            Main.Qb.Set(null, "pid.set", 600);
            Main.Qb.Set(null, "pid.readFilter", 200);
            Main.Qb.Set(null, "pid.outFilter", 50);
            while (egcRun)
            {
                Main.Qb.Set(null, "egc0.m310.set", 1);
                Task.Delay(100).Wait();

                pid.set.value = Main.Qb.GetD(null, "pid.set");
                int readFilter = (int)Main.Qb.GetD(null, "pid.readFilter");
                //  pid.Read = (pid.Read * 5 + Main.Qb.GetD(mut))/6;
                //  float lam = 500 - (Main.Qb.GetD("egc0.m290.read") - 500);

                pid.read.value = (pid.read.value * readFilter + Main.Qb.GetD(null, "egc0.m290.read")) / (readFilter + 1);


               // Main.Qb.Set(null, "pid.read", pid.Read);
                //    Main.Qb.Set("pid.set", pid.Set);

                int outFilter = (int)Main.Qb.GetD(null, "pid.outFilter");
                o = (o * outFilter + (100 - pid.@out.value)) / (outFilter + 1);

                Main.Qb.Set(null, "pid.out", o);

             //   pid.Process();
                Main.Qb.Set(null, "egc0.m3C0.set", o);
            }
        }
    }

    
}
