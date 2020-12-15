using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;

namespace UnityEditor.GGP
{
    public class GGPProcess : IDisposable
    {
        private readonly Process process;
        private readonly string friendlyCommandString;
        private string stdoutBuffer;
        private string stderrBuffer;

        public GGPProcess(string ggpExe, string command, string args, bool structured)
        {
            process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    Arguments = command + (structured ? " -s " : " ") + args,
                    FileName = ggpExe,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            friendlyCommandString = $"ggp {command} {args}";

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(stdoutBuffer))
                {
                    stdoutBuffer += "\n";
                }

                stdoutBuffer += e.Data;
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(stderrBuffer))
                {
                    stderrBuffer += "\n";
                }

                stderrBuffer += e.Data + "\n";
            };
        }

        public object RunProcessAsyncWithProgressBarAndGetOutputOrThrow(string title, string info, float progress, bool structured = false)
        {
            if (!process.Start())
            {
                throw new Build.BuildFailedException($"Unable to start process '{friendlyCommandString}'.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (true)
            {
                if (process.HasExited)
                    return GetProcessOutputOrThrow(structured);

                if (EditorUtility.DisplayCancelableProgressBar(title, info, progress))
                {
                    try
                    {
                        var processStartInfo = new ProcessStartInfo($"taskkill", $"/F /T /PID {process.Id}")
                        {
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                        };

                        Process.Start(processStartInfo);
                    }
                    catch
                    {
                        // ignored
                    }
                    throw new OperationCanceledException();
                }

                // Don't burn the cpu
                Thread.Sleep(50);
            }
        }

        public object RunProcessAndGetOutputOrThrow(bool structured, int timeoutInMs)
        {
            if (!process.Start())
            {
                throw new Build.BuildFailedException($"Unable to start process '{friendlyCommandString}'.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(timeoutInMs))
            {
                process.Close();
                throw new Build.BuildFailedException($"Timeout waiting for process '{friendlyCommandString}'.");
            }

            return GetProcessOutputOrThrow(structured);
        }

        private object GetProcessOutputOrThrow(bool structured)
        {
            if (process.ExitCode != 0)
            {
                string errorMessage = $"Error running command '{friendlyCommandString}'.\n" +
                                      $"Returned exit code {process.ExitCode}.\n" +
                                      "Output: " + ((string.IsNullOrEmpty(stderrBuffer)) ? "Not available" : $"{stderrBuffer}");

                throw new Build.BuildFailedException(errorMessage);
            }

            if (!structured)
                return stdoutBuffer;

            try
            {
                return JsonConvert.DeserializeObject<List<object>>(stdoutBuffer);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            return string.Empty;
        }

        public void Dispose()
        {
            process.Close();
            process.Dispose();
        }
    }
}
