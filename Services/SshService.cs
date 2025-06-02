using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using SirisDeviceManager.Model;
using System.Net.NetworkInformation;
using Renci.SshNet;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SirisDeviceManager.Services
{
    public class SshService
    {
        private static readonly Lazy<SshService> _instance = new Lazy<SshService>(() => new SshService());

        private SshService() { }

        public static SshService Instance => _instance.Value;

        public async Task<List<string>> ExecuteSSHCommand(List<Device> devices, string command)
        {
            MessageService.Instance.UpdateStatus($"[RUNNING COMMAND]: {command}]");

            var tasks = devices.Select(async dev =>
            {
                try
                {
                    MessageService.Instance.UpdateStatus($"[DEVICE]: {dev.SerialNumber}]");
                    await Task.Delay(200);

                    using (var client = new SshClient(dev.Ip, dev.User, dev.Password))
                    {
                        await Task.Run(() => client.Connect());
                        var result = await Task.Run(() => client.RunCommand(command));
                        MessageService.Instance.UpdateStatus($"Saída do comando: {result.Result}");

                        await Task.Delay(TimeSpan.FromSeconds(120));

                        client.Disconnect();
                        return result.Result;
                    }

                }catch(Exception ex)
                {
                    MessageService.Instance.UpdateStatus(ex.Message);
                    return ex.Message;
                }
            });

            MessageService.Instance.UpdateStatus($"[RUNNING COMMAND]: EXIT PROCESS]");
            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task UpdateAllDevices(List<Device> devices)
        {
            int maxConcurrency = 5;
            var options = new ParallelOptions {  MaxDegreeOfParallelism = maxConcurrency };
            var logLock = new Object();

            MessageService.Instance.UpdateStatus("[INFO] Starting parallel SSH execution...\n");

            await Task.Run(() =>
            {
                Parallel.ForEach(devices, options, dev =>
                {
                    try
                    {
                        Console.WriteLine(dev.SerialNumber);

                        lock(logLock)
                        {
                            MessageService.Instance.UpdateStatus($"[INFO] Connecting to {dev.SerialNumber}...");
                        }

                        using (var client = new SshClient(dev.Ip, dev.User, dev.Password))
                        {
                            client.Connect();
                            var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                            shell.WriteLine("cd /home/pi/_Configs && ./update.sh");

                            var buffer = new byte[4096];
                            TimeSpan inactivityTimeout = TimeSpan.FromSeconds(15);
                            DateTime lasDataReceived = DateTime.Now;

                            while(client.IsConnected)
                            {
                                bool dataRead = false;

                                if (shell.DataAvailable)
                                {
                                    int count = shell.Read(buffer, 0, buffer.Length);
                                    if (count > 0)
                                    {
                                        var output = Encoding.UTF8.GetString(buffer, 0, count);
                                        lasDataReceived = DateTime.Now;
                                        dataRead = true;       

                                        lock (logLock)
                                        {
                                            MessageService.Instance.UpdateStatus($"[{dev.SerialNumber}] {output}");
                                        }
                                    }
                                }
                                
                                if(!dataRead && DateTime.Now - lasDataReceived > inactivityTimeout)
                                {
                                    break;
                                }

                                Thread.Sleep(100);
                            }

                            client.Disconnect();
                       
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        lock(logLock)
                        {
                            MessageService.Instance.UpdateStatus($"[ERROR] {dev.SerialNumber}: {ex.Message}");
                        }
                    }
                });
            });
        }
    }
}
