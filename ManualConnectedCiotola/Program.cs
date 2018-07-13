using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TwinExtension;

namespace ManualConnectedCiotola
{
    static class Program
    {
        private static int ciotolaState;
        private static string ciotolaName = "";

        private static bool statusTelecamera = false;
        private static bool releDosatoreState = false;

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var deviceId = configuration["deviceId"];
            var authenticationMethod =
                new DeviceAuthenticationWithRegistrySymmetricKey(
                    deviceId,
                    configuration["deviceKey"]
                )
            ;
            ciotolaName = deviceId;


            var transportType = TransportType.Mqtt;
            if (!string.IsNullOrWhiteSpace(configuration["transportType"]))
            {
                transportType = (TransportType)
                    Enum.Parse(typeof(TransportType),
                    configuration["transportType"], true);
            }

            var client = DeviceClient.Create(
                configuration["hostName"],
                authenticationMethod,
                transportType
            );

           
            Console.WriteLine($"stai controllando la ciotola: {deviceId}");
            while (true)
            {
                //faccio l'enconding di un messaggio da bytes a string
                var message = await client.ReceiveAsync();
                //hardbeat per evitare il timeout
                if (message == null) { continue; }
                var bytes = message.GetBytes();
                //hardbeat per evitare il timeout
                if (bytes == null) { continue; }

                var text = Encoding.UTF8.GetString(bytes);

                Console.WriteLine($"Messaggio ricevuto: {text}");
                var textParts = text.ToLower().Split();
                switch (textParts[0])
                {
                    case "takephoto":
                        await takePhoto(client);
                        break;
                    default:
                        Console.WriteLine("Syntax error");
                        break;
                }
                await client.CompleteAsync(message);
            }
        }

        private static async void SendToBlobAsync()
        {
            string fileName = photonumber +".jpg";
            Console.WriteLine("foto scattata: {0}", fileName);
            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var sourceData = new FileStream(@".jpg", FileMode.Open))
            {
                await deviceClient.UploadToBlobAsync(fileName, sourceData);
            }

            watch.Stop();
            Console.WriteLine("upload completato");
        }

        private static async Task takePhoto(DeviceClient client)
        {
            statusTelecamera = true;
            Random rnd = new Random();
            int photoNumber = rnd.Next(1, 3);


            Console.WriteLine("foto della ciotola acquisita");
            var coll = new TwinCollection();
            coll["photo"] = photonumber;
            await client.UpdateReportedPropertiesAsync(coll);
        }

    }
}
