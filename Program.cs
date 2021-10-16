using LibreHardwareMonitor.Hardware;
using NLog;
using System;
using System.IO.Ports;
using System.Threading;

namespace TesteTemperaturaPCNetFull
{
    class Program
    {
        static Computer computer = new Computer()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true
        };

        static string _GPU, _CPU;
        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private static SerialPort _port = new SerialPort();
        private static void conect()
        {
            try
            {
                _log.Info("Conectando...");
                computer.Open();

                if (!_port.IsOpen)
                {
                    _port.PortName = "COM3";
                    _port.BaudRate = 9600;
                    _port.Open();
                    _log.Info("Conectado");
                }

            }
            catch (Exception ex)
            {
                _log.Error($"Erro ao conectar: {ex.Message}");
            }
        }
        static void Main(string[] args)
        {
            _log.Info("Iniciando");
            conect();

            _log.Info("Iniciando leitura das temperaturas");
            Timer t = new Timer(Status, null, 0, 1000);
            Console.Read();
        }
        private static void Status(object state)
        {
            foreach (var hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        _GPU = sensor.Value.GetValueOrDefault().ToString("00.00");
                    }
                }

                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core (Tctl/Tdie)"))
                        {
                            _CPU = sensor.Value.GetValueOrDefault().ToString("00.00");
                        }
                    }
                }
            }
            try
            {
                Console.WriteLine($"CPU={_CPU} GPU={_GPU}");
                _port.Write(_CPU + "*" + _GPU + "#");
            }
            catch (Exception ex)
            {
                _log.Error($"Erro ao lêr as temperaturas: {ex.Message}");
            }
        }
    }
}
