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
using Renci.SshNet.Common;

namespace SirisDeviceManager.Services
{
    public class SshService
    {
        private static readonly Lazy<SshService> _instance = new Lazy<SshService>(() => new SshService());

        private SshService() { }

        public static SshService Instance => _instance.Value;
        private readonly DeviceService _deviceService = new();

        public async Task<List<string>> ExecuteSSHCommand(List<Device> devices, string command)
        {
            MessageService.Instance.UpdateStatus($"[RUNNING COMMAND]: {command}]");

            List<Device> activeDevices = await _deviceService.ScanActiveDevices(devices, AppSessionStorage.Instance.Network);
            var tasks = activeDevices.Select(async dev =>
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

                        await Task.Delay(TimeSpan.FromSeconds(5));

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

        public async Task<bool> TryConnectWithTimeoutAsync(SshClient client, int timeoutMilliseconds)
        {
            try
            {
                var connectTask = Task.Run(() =>
                {
                    try
                    {
                        client.Connect();
                    }
                    catch (Exception)
                    {
                        // Suprime a exceção dentro da task
                        throw new InvalidOperationException("SSH connection failed");
                    }
                });

                if (await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds)) == connectTask)
                {
                    await connectTask; // Propaga exceção se aconteceu dentro da task
                    return true; // Conectado com sucesso
                }
                else
                {
                    return false; // Timeout
                }
            }
            catch (Exception)
            {
                // Aqui captura qualquer erro (host não conhecido, rede, etc.)
                return false;
            }
        }

        public async Task UpdateAllDevices(List<Device> devices)
        {
            var logLock = new Object();

            MessageService.Instance.UpdateStatus("[INFO] Starting parallel SSH execution...\n");

            List<Device> activeDevices = await _deviceService.ScanActiveDevices(devices, AppSessionStorage.Instance.Network);

            var tasks = activeDevices.Select(async dev =>
            {
                try
                {
                    lock (logLock)
                    {
                        MessageService.Instance.UpdateStatus($"[INFO] Connecting to {dev.SerialNumber}...");
                    }
                    
                    await Task.Delay(200);

                    using (var client = new SshClient(dev.Ip, dev.User, dev.Password))
                    {
                        client.Connect();

                        var shell = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                        shell.WriteLine("cd /home/pi/_Configs && ./update.sh");

                        var buffer = new byte[4096];
                        TimeSpan inactivityTimeout = TimeSpan.FromSeconds(1000);
                        DateTime lasDataReceived = DateTime.Now;

                        while (client.IsConnected)
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

                            if (!dataRead && DateTime.Now - lasDataReceived > inactivityTimeout)
                            {
                                break;
                            }

                            await Task.Delay(100);
                        }

                        client.Disconnect();
                    }

                }
                catch (Exception ex)
                {
                    lock (logLock)
                    {
                        MessageService.Instance.UpdateStatus($"[ERROR] {dev.SerialNumber}: {ex.Message}");
                    }
                }
            });

            await Task.WhenAll(tasks);
            MessageService.Instance.UpdateStatus($"[RUNNING COMMAND]: EXIT PROCESS]");
        }
    }
}
