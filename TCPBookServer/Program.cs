using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BookLibraryAssignemnt;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TCPBookServer
{

    class Program
    {
        private static List<Book> Library = new List<Book>
        {
        new Book("John Cena's autobiography","John Cena",200,"1234567890123"),
        new Book("Kpop","Jim",200,"1234567890124"),
        new Book("Bebop","Kevin",200,"1234567890125"),
        new Book("How to train your dragon","Ben",200,"1234567890126"),
        new Book("Spiderman","sdlgyfahgf",200,"1234567890127")
        };
        private static TcpListener tcpListener; //welcoming socket
        private static TcpClient serverSocket;
        private static string prevrequest = "";
        static void Main(string[] args)
        {
            var ip = IPAddress.Parse("127.0.0.1");
            serverSocket = new TcpClient();
            tcpListener = new TcpListener(ip, 4646);
            Console.WriteLine("Server is ready to listen for requests");
            //this is the handshake
            tcpListener.Start();
            using (serverSocket = tcpListener.AcceptTcpClient())
            {
                Console.WriteLine("Client Ip" + (IPEndPoint)(serverSocket.Client.RemoteEndPoint));
                while (serverSocket.Connected)
                {
                    //method is now multi threaded
                    Task.Run(() => Work(serverSocket));
                }

            }
            tcpListener.Stop();
        }

        public static void Work(TcpClient server)
        {
            if (server.Connected)
                using (Stream ns = server.GetStream())
                {
                    StreamWriter streamWriter = new StreamWriter(ns) { AutoFlush = true };
                    StreamReader streamReader = new StreamReader(ns);
                    var request = streamReader.ReadLine();
                    while (request != null)
                    {
                        string message="";
                        Console.WriteLine("Client Message: " + request);
                        if (prevrequest != "")
                        {
                            if (prevrequest == "Get")
                            {
                                
                                if (request.Length == 13)
                                {
                                    message = Get(request);
                                    prevrequest = "";
                                }
                                else
                                {
                                    if (request == "Cancel")
                                    {
                                        message = "Cancelled";
                                        prevrequest = "";
                                    }
                                    else
                                    message = "yOU DIDN'T TYPE IT RIGHT";
                                }
                                
                            }
                            if(prevrequest == "Save")
                            {
                                if (request == "Cancel")
                                {
                                    message = "Cancelled";
                                    prevrequest = "";
                                }
                                else
                                {
                                    try
                                    {
                                        Book newBook = JsonConvert.DeserializeObject<Book>(request);
                                        Library.Add(newBook);
                                        message = "Saved";
                                        prevrequest = "";
                                    }
                                    catch (Exception e)
                                    {
                                        message = "Failed to save new object. Try again or Cancel";
                                        Console.WriteLine(e.Message);
                                    }
                                }
                                
                            }
                        }
                        else
                        {
                            if (request == "GetAll")
                            {
                                message = GetAll();
                            }
                            if (request == "Get")
                            {
                                message = "Enter isbn or Cancel";
                                prevrequest = "Get";
                            }
                            if (request == "Save")
                            {
                                message = "Enter book as json object";
                                prevrequest = "Save";
                            }
                            
                        }
                        streamWriter.WriteLine(message);//response



                        request = streamReader.ReadLine();
                    }
                    Console.WriteLine("Stopped");
                    server.Close();
                }
            
        }
        public static string GetAll()
        {
            string xy="";
            foreach(Book x in Library)
            {
                xy += JsonConvert.SerializeObject(x) + " ; ";
            }
            return xy;
        }

        public static string Get(string isbn)
        {
            string xy = "";
            foreach (Book x in Library)
            {
                if (x.Isbn == isbn)
                {
                    xy = JsonConvert.SerializeObject(x);
                    break;
                }
            }
            return xy;
        }
    }
}
