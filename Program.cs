using k8s;
using System;
using System.Runtime.Loader;
using System.Threading;

namespace AksAuth
{
    class Program
    {
        static void Main(string[] args)
        {
            var ended = new ManualResetEventSlim();
            var starting = new ManualResetEventSlim();

            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                System.Console.WriteLine("Unloding fired");
                starting.Set();
                System.Console.WriteLine("Waiting for completion");
                ended.Wait();
            };

            // LOGIC

            AKSAdministration();

            System.Console.WriteLine("Waiting for signals");
            starting.Wait();

            System.Console.WriteLine("Received signal gracefully shutting down");
            Thread.Sleep(5000);
            ended.Set();

        }

        private static void AKSAdministration()
        {
            var configuration = new k8s.KubernetesClientConfiguration
            {
                Host = "https://ari-aks-22-demo-f681ed-b4579bd4.hcp.southeastasia.azmk8s.io:443",
                AccessToken = ReadAccessToken()
            };
            configuration.SkipTlsVerify = true;
            var client = new k8s.Kubernetes(configuration);

            var namespaces = client.ListNamespace();
            foreach (var ns in namespaces.Items)
            {
                Console.WriteLine(ns.Metadata.Name);
            }
            // ## Uncomment below lines to test creation of namespace
            //client.CreateNamespace(new k8s.Models.V1Namespace { 
            // Metadata= new k8s.Models.V1ObjectMeta
            // {
            //     Name= "automation"
            // }
            //});
        }

        static string ReadAccessToken()
        {
            var tokenLocation = "/var/run/secrets/kubernetes.io/serviceaccount/token";
            using (var sr = new System.IO.StreamReader(tokenLocation))
            {
                var token = sr.ReadToEnd();
                return token;
            }
        }
    }
}
