using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using CommandInterface;
using UnityEngine;
using Assets.Code.Tools;
using Assets.Code.Interface;
using Assets.Code.Game;
using Assets.Code.Interface.Authentication;
using CompressedStructures;
using VectorNet;

namespace Assets.Code
{
    public class NetController : MonoBehaviour
    {
        public class BuildingAction
        {
            public string Name;
            public bool Possible;

            public BuildingAction(string name, bool possible)
            {
                Name = name;
                Possible = possible;
            }
        }



        public class NetArgs
        {
            public Socket MainSocket { get; private set; }

            public NetArgs(Socket mainSocket)
            {
                MainSocket = mainSocket;
            }
        }



        public static int ServerPort = 8005;
        public static string ServerAddress = "192.168.0.100";

        public static int ConnectTimeSeconds = 3;
        public static int ConnectDelayMilliseconds = 100;

        public static string Email;
        public static string Password;

        public static bool ConnectionActive;

        public static event EventHandler OnConnectionFail;
        public static event EventHandler OnConnectionSuccess;



        private static readonly Dictionary<string, string> BuildingsDictionary
            = new Dictionary<string, string> {
                { "f", "Forest" },
                { ".", "Plain" },
                { "r", "Rock" },
                { "~", "Water" },
                { "H", "House - wood, 1" }
            };

        protected static Interface<NetArgs> NetInterface;

        protected static Thread ConnectionThread;
        protected static Socket MainSocket;

        private static readonly object SocketLock = new object();



        protected virtual void Start()
        {
            #region commands
            NetInterface = new Interface<NetArgs>(
                new List<Command<NetArgs>> {
                    new Command<NetArgs>(
                        "ln-r",
                        "(LogiN Result) ",
                        "result",
                        (args, netArgs) => { // TODO to methods
                            if (args[0] == "0") {
                                Debug.Log("Connection error");
                                
                                Ui.LoginStatus.Content = 
                                    LoginButtonController.LoginStatusLoginFail;

                                Thread.CurrentThread.Abort();
                            }

                            Debug.Log("Connection success!");
                            netArgs.MainSocket.Send(
                                Encoding.ASCII.GetBytes("gt"));

                            Ui.LoginStatus.Content =
                                LoginButtonController.LoginStatusSucces;

                            LoginButtonController.AuthenticationFormActive = false;

                            Ui.ResourcesLineForm.GameObject.SetActive(true);
                        }),

                    new Command<NetArgs>(
                        "st",
                        "Sets the Territory buildings",
                        "@size,buildings",
                        (args, netArgs) => {
                            Debug.Log("Buildings received");

                            var l = args[0].Substring(1, args[0].Length - 2).Split(';');
                            var width = Convert.ToInt32(l[0]);
                            var length = Convert.ToInt32(l[1]);
                            
                            for (var y = 0; y < length; y++) {
                                for (var x = 0; x < width; x++) {
                                    var instance = (GameObject) Instantiate(
                                        Resources.Load(BuildingsDictionary[args[1][y * width + x].ToString()]));
                                    var holder = (GameObject) Instantiate(Resources.Load("Holder"));

                                    instance.transform.SetParent(Ui.GameBuildingsContainer.Transform, true);
                                    holder.transform.SetParent(Ui.GameBuildingsContainer.Transform, true);

                                    instance.GetComponent<IsometricController>().IsometricPosition =
                                        new Vector2(x, y);
                                    holder.GetComponent<IsometricController>().IsometricPosition = new Vector2(
                                        x, y);

                                    instance.GetComponent<BuildingController>().Holder = holder;
                                    holder.GetComponent<HolderController>().Building = instance;
                                }
                            }
                            Debug.Log("Territory buildings generation end");

                            netArgs.MainSocket.Send(Encoding.ASCII.GetBytes("gr"));
                        }),
                    
                    new Command<NetArgs>(
                        "sba",
                        "(sba -> Set Building context Actions) sets building context actions",
                        "@list(action)",
                        (args, netArgs) => {
                            var actions = args[0].ParseList<CommonBuildingAction>(CommonBuildingAction.GetFromString);

                            foreach (var action in actions)
                            {
                                Debug.Log(action.Active + ", " + action.Name);
                            }

                            UiController.BuildingActions = new ReadOnlyCollection<BuildingAction>(
                                new List<BuildingAction>(actions.Select(
                                    action => new BuildingAction(action.Name, action.Active))));
                        }),

                    new Command<NetArgs>(
                        "r",
                        "(r -> Refresh) refreshes current resources",
                        "@resources]",
                        (args, netArgs) => {
                            var resources = CommonResources.GetFromString(args[0]);
                            
                            foreach (var pair in resources.Resource)
                            {
                                Ui.Resource suitableResource = null;
                                switch (pair.Key)
                                {
                                    default:
                                        Debug.LogError("Wrong resource name");
                                        break;

                                    case ResourceType.Wood:
                                        suitableResource = Ui.ResourceWood;
                                        break;

                                    case ResourceType.Meat:
                                        suitableResource = Ui.ResourceMeat;
                                        break;

                                    case ResourceType.Corn:
                                        suitableResource = Ui.ResourceCorn;
                                        break;

                                    case ResourceType.Stone:
                                        suitableResource = Ui.ResourceStone;
                                        break;

                                    case ResourceType.Gold:
                                        suitableResource = Ui.ResourceGold;
                                        break;

                                    case ResourceType.Progress:
                                        suitableResource = Ui.ResourceProgress;
                                        break;

                                    case ResourceType.People:
                                        suitableResource = Ui.ResourcePeople;
                                        break;
                                }

                                suitableResource.Content = pair.Value;
                            }
                        }),
                });
            #endregion

            UiController.BuildingChoosed += SendBuildingActionsRequest;
        }

        protected virtual void OnDestroy()
        {
            if (ConnectionThread != null)
            {
                ConnectionThread.Abort();
            }
            End();
        }



        public static void LoginConnect()
        {
            try
            {
                Connection();
                LoginConnection();
                ConnectionThread = new Thread(ConnectionLoopStart);
                ConnectionThread.Start();
            }
            catch (SocketException) {}
        }

        public static void SignUpConnect()
        {
            Connection();
        }

        public static void SendEmail(string email)
        {
            MainSocket.Send(Encoding.ASCII.GetBytes("esn@" + email.Replace('@', '#')));
        }



        private static void Connection()
        {
            ActionsProcessor.AddActionToQueue(() =>
                Ui.ResourcesLineConnectionStatus.Content = UiController.ConnectionStatusConnection);

            var ipPoint = new IPEndPoint(IPAddress.Parse(ServerAddress), ServerPort);
            MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var connected = false;
            var connectBeginTime = DateTime.Now;

            do
            {
                try
                {
                    MainSocket.Connect(ipPoint);
                    connected = true;
                    break;
                }
                catch (SocketException)
                {
                    Thread.Sleep(ConnectDelayMilliseconds);
                }
            }
            while (( DateTime.Now - connectBeginTime ).Seconds < ConnectTimeSeconds);

            if (!connected)
            {
                ActionsProcessor.AddActionToQueue(() => {
                    if (OnConnectionFail != null)
                    {
                        OnConnectionFail(null, EventArgs.Empty);
                    }
                });

                throw new SocketException();
            }

            ActionsProcessor.AddActionToQueue(() => {
                if (OnConnectionSuccess != null)
                {
                    OnConnectionSuccess(null, EventArgs.Empty);
                }
            });

            ActionsProcessor.AddActionToQueue(() =>
                Ui.ResourcesLineConnectionStatus.Content = UiController.ConnectionStatusConnected);
        }

        private static void LoginConnection()
        {
            MainSocket.Send(Encoding.ASCII.GetBytes(
                "ln@" + new CommonAccount(Email, Password).GetString.ToArgument()));
            Debug.Log("ln@" + new CommonAccount(Email, Password).GetString.ToArgument());
        }

        private static void ConnectionLoopStart()
        {
            try
            {
                ConnectionActive = true;
                while (ConnectionActive)
                {
                    var receivedString = Receive();
                    Debug.Log(receivedString);

                    ActionsProcessor.AddActionToQueue(() =>
                        NetInterface.UseCommand(receivedString, new NetArgs(MainSocket)));
                }

                MainSocket.Shutdown(SocketShutdown.Both);
                MainSocket.Close();
                ConnectionActive = false;
            }
            catch (SocketException)
            {
                Debug.LogError("Disconnect!");
                LoginConnect();
            }
            catch (ThreadAbortException)
            {
                End();
            }
        }

        private static void SendBuildingActionsRequest(object sender, UiController.BuildingChoosedArgs args)
        {
            MainSocket.Send(Encoding.ASCII.GetBytes("gba@" + new CommonBuilding(args.Name, new IntVector(
                (int) args.IsometricPosition.x,
                (int) args.IsometricPosition.y)).GetString));
        }

        private static void End()
        {
            if (MainSocket != null)
            {
                MainSocket.Close();
            }
        }

        private static string Receive()
        {
            var currentStringBuilder = new StringBuilder();
            var receivedData = new byte[4096];

            lock (SocketLock)
            {
                do
                {
                    var bytes = MainSocket.Receive(receivedData);
                    currentStringBuilder.Append(Encoding.ASCII.GetString(receivedData, 0, bytes));
                } while (MainSocket.Available > 0);
            }

            return currentStringBuilder.ToString();
        }
    }
}
