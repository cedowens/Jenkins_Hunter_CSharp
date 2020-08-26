using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Net;
using System.IO;
using System.Text;

namespace JenkinsHunter
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine("--->Jenkins Hunter<---");
            Console.WriteLine("=======================================================");
            Console.WriteLine("Dumping a list of computers from AD...");
            List<string> CompNames = new List<string>();
            string mydom = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            DirectoryEntry ent = new DirectoryEntry("LDAP://" + mydom);
            DirectorySearcher searcher = new DirectorySearcher(ent);
            searcher.Filter = ("(objectClass=computer)");
            searcher.SizeLimit = int.MaxValue;
            searcher.PageSize = int.MaxValue;

            foreach (SearchResult result in searcher.FindAll())
            {
                string ComputerName = result.GetDirectoryEntry().Name;
                if (ComputerName.StartsWith("CN="))
                {
                    ComputerName = ComputerName.Remove(0, "CN=".Length);
                }
                CompNames.Add(ComputerName);
            }
            searcher.Dispose();
            ent.Dispose();

            Console.WriteLine("=======================================================");
            Console.WriteLine("Performing sweep for unauth Jenkins hosts...");
            Console.WriteLine("=======================================================");

            Queue myqueue = new Queue(CompNames);
            List<Thread> threads = new List<Thread>();

            for (int i = 1; i <= myqueue.Count; i++)
            {
                Thread newThread = new Thread(() => Threader(myqueue));
                newThread.IsBackground = true;
                newThread.Start();
                threads.Add(newThread);
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }


        }


        static void Threader(Queue myqueue)
        {
            while (myqueue.Count != 0)
            {
                var worker = myqueue.Dequeue();
                string worker2 = Convert.ToString(worker);
                Connector(worker2);
            }
        }

        static void Connector(String hostname)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {

                try
                {
                    socket.Connect(hostname, 8080);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[+] Port 8080" + " open on " + hostname);
                    Console.ForegroundColor = ConsoleColor.White;
                    socket.Close();

                    HttpWebRequest jenkReq = (HttpWebRequest)WebRequest.Create("http://" + hostname + ":8080/script");
                    HttpWebResponse jenkResponse = (HttpWebResponse)jenkReq.GetResponse();
                    using (Stream stream = jenkResponse.GetResponseStream())
                    {
                        StreamReader rdr = new StreamReader(stream, Encoding.UTF8);
                        String responsetext = rdr.ReadToEnd();

                        if (jenkResponse.StatusCode == HttpStatusCode.OK && responsetext.Contains("Jenkins") && responsetext.Contains("Console"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("===> Unauthenticated Jenkins likely at http://" + hostname + ":8080/script");
                            Console.ForegroundColor = ConsoleColor.White;

                        }
                    }
                }
                catch
                {

                }

                try
                {
                    socket.Connect(hostname, 443);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[+] Port 443" + " open on " + hostname);
                    Console.ForegroundColor = ConsoleColor.White;
                    socket.Close();

                    HttpWebRequest jenkReq2 = (HttpWebRequest)WebRequest.Create("https://" + hostname + "/script");
                    HttpWebResponse jenkResponse2 = (HttpWebResponse)jenkReq2.GetResponse();
                    using (Stream stream2 = jenkResponse2.GetResponseStream())
                    {
                        StreamReader rdr2 = new StreamReader(stream2, Encoding.UTF8);
                        String responsetext2 = rdr2.ReadToEnd();

                        if (jenkResponse2.StatusCode == HttpStatusCode.OK && responsetext2.Contains("Jenkins") && responsetext2.Contains("Console"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("===> Unauthenticated Jenkins likely at https://" + hostname + "/script");
                            Console.ForegroundColor = ConsoleColor.White;

                        }
                    }
                }
                catch
                {

                }

                
                try
                {
                    socket.Connect(hostname, 80);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[+] Port 80" + " open on " + hostname);
                    Console.ForegroundColor = ConsoleColor.White;
                    socket.Close();
                    HttpWebRequest jenkReq3 = (HttpWebRequest)WebRequest.Create("http://" + hostname + "/script");
                    HttpWebResponse jenkResponse3 = (HttpWebResponse)jenkReq3.GetResponse();
                    using (Stream stream3 = jenkResponse3.GetResponseStream())
                    {
                        StreamReader rdr3 = new StreamReader(stream3, Encoding.UTF8);
                        String responsetext3 = rdr3.ReadToEnd();

                        if (jenkResponse3.StatusCode == HttpStatusCode.OK && responsetext3.Contains("Jenkins") && responsetext3.Contains("Console"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("===> Unauthenticated Jenkins likely at http://" + hostname + "/script");
                            Console.ForegroundColor = ConsoleColor.White;

                        }
                    }

                }
                catch
                {

                }

               


                


            }
        }


    }



}

