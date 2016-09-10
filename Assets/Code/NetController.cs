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
using Assets.Code.Interface.Signin;
using BinarySerializationExtensions;
using CommandInterface.Extensions;
using CommonStructures;
using VectorNet;

namespace Assets.Code
{
    public class NetController : MonoBehaviour
    {
        public class NetArgs
        {
            public Socket MainSocket { get; private set; }

            public NetArgs(Socket mainSocket)
            {
                MainSocket = mainSocket;
            }
        }



        public static NetController Instance { get; set; }

        public int ServerPort = 8005;
        public string ServerAddress = "192.168.0.100";

        public int ConnectTimeSeconds = 3;
        public int ConnectDelayMilliseconds = 100;

        public string Email;
        public string Password;

        public bool ConnectionActive;

        public Encoding Encoding = Encoding.GetEncoding(1251);

        public event Action OnConnectionFail;
        public event Action OnConnectionSuccess;



        protected Interface<NetArgs, bool> CommandInterface;

        protected Thread ConnectionThread;
        protected Socket MainSocket;

        private readonly object _socketLock = new object();



        protected virtual void Start()
        {
            Instance = this;

            #region commands

            CommandInterface = new Interface<NetArgs, bool>(
                new Command<NetArgs, bool>(
                    "login-result", new[] {"result"}, // TODO login-result to unactive
                    _loginResult),

                new Command<NetArgs, bool>(
                    "set-territory", new[] {"territory"},
                    _setTerritory),

                new Command<NetArgs, bool>(
                    "set-building-actions", new[] {"actions"},
                    _setBuildingActions),

                new Command<NetArgs, bool>(
                    "resources", new[] {"resources"},
                    _resources),

                new Command<NetArgs, bool>(
                    "upgrade-result", new[] {"building"},
                    _upgradeResult));

            #endregion

            UiController.BuildingChoosed += SendBuildingActionsRequest;
            UiController.ActionChoosed += SendUpgradeRequest;
        }

        protected virtual void OnDestroy()
        {
            if (ConnectionThread != null)
            {
                ConnectionThread.Abort();
            }
            End();
        }



        public void LoginConnect()
        {
            try
            {
                Connection();
                LoginConnection();
                ConnectionThread = new Thread(ConnectionLoopStart);
                ConnectionThread.Start();
            }
            catch (SocketException)
            {
            }
        }

        public void SignUpConnect()
        {
            Connection();
        }

        public void SendEmail(string email)
        {
            MainSocket.Send(Encoding.GetBytes("esn@" + email.Replace('@', '#')));
        }



        private void Connection()
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
            } while ((DateTime.Now - connectBeginTime).Seconds < ConnectTimeSeconds);

            if (!connected)
            {
                ActionsProcessor.AddActionToQueue(() =>
                {
                    if (OnConnectionFail != null)
                    {
                        OnConnectionFail();
                    }
                });

                throw new SocketException();
            }

            ActionsProcessor.AddActionToQueue(() =>
            {
                if (OnConnectionSuccess != null)
                {
                    OnConnectionSuccess();
                }
            });

            ActionsProcessor.AddActionToQueue(() =>
                    Ui.ResourcesLineConnectionStatus.Content = UiController.ConnectionStatusConnected);
        }

        private void LoginConnection() {
            MainSocket.Send(Encoding.GetBytes(
                StringExtensions.CreateCommand("login", SerializationHelper.Serialize(new CommonAccount(Email, Password), Encoding))));
        }

        private void ConnectionLoopStart()
        {
            try
            {
                ConnectionActive = true;
                while (ConnectionActive)
                {
                    var receivedString = Receive();
                    Debug.Log(receivedString);

                    ActionsProcessor.AddActionToQueue(() =>
                            CommandInterface.GetFunc(receivedString, new NetArgs(MainSocket))());
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

        private void SendBuildingActionsRequest(object sender, UiController.BuildingChoosedArgs args)
        {
            MainSocket.Send(Encoding.GetBytes(
                StringExtensions.CreateCommand(
                    "get-building-actions",
                    SerializationHelper.Serialize(
                        new CommonBuilding(
                            new IntVector(
                                (int) args.IsometricPosition.x,
                                (int) args.IsometricPosition.y)),
                        Encoding))));
        }

        private void SendUpgradeRequest(object sender, UiController.ActionChoosedArgs args)
        {
            MainSocket.Send(
                Encoding.GetBytes(
                    StringExtensions.CreateCommand(
                        "upgrade",
                        SerializationHelper.Serialize(
                            args.Action.Common,
                            Encoding))));
        }

        private void End()
        {
            if (MainSocket != null)
            {
                MainSocket.Close();
            }
        }

        private string Receive()
        {
            var currentStringBuilder = new StringBuilder();
            var receivedData = new byte[4096];

            lock (_socketLock)
            {
                do
                {
                    var bytes = MainSocket.Receive(receivedData);
                    currentStringBuilder.Append(Encoding.GetString(receivedData, 0, bytes));
                } while (MainSocket.Available > 0);
            }

            return currentStringBuilder.ToString();
        }



        // @result
        private bool _loginResult(IDictionary<string, string> args, NetArgs netArgs)
        {
            try
            {
                switch ((LoginResult)int.Parse(args["result"]))
                {
                    case LoginResult.Successful:
                        netArgs.MainSocket.Send(
                            Encoding.GetBytes("get-territory"));

                        Ui.LoginStatus.Content = LoginStatus.Succes;

                        LoginButtonController.AuthenticationFormActive = false;

                        Ui.ResourcesLineForm.GameObject.SetActive(true);
                        return true;

                    case LoginResult.Unsuccessful:
                        Ui.LoginStatus.Content =
                            LoginStatus.LoginFail;
                        return true;

                    case LoginResult.Banned:
                        Ui.LoginStatus.Content =
                            LoginStatus.Banned;
                        return true;

                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("[{0}; message = {1}]", ex.GetType(), ex.Message));
                return true;
            }
        }

        // @territory
		private bool _setTerritory(IDictionary<string, string> args, NetArgs netArgs)
		{
			Debug.Log("Buildings received");

		    var territory = SerializationHelper.Deserialize<CommonTerritory>(
                args["territory"], Encoding);

		    BuildingsContainer.Instance.BuildingsGrid
		        = new Building[
		            territory.PatternIDs.GetLength(0),
		            territory.PatternIDs.GetLength(1)];

            for (var y = 0; y < territory.PatternIDs.GetLength(1); y++)
            {
                for (var x = 0; x < territory.PatternIDs.GetLength(0); x++)
                {
                    BuildingsContainer.Instance.CreateBuilding(
                        territory.PatternIDs[x, y],
                        new Vector2(x, y));
                }
            }

            netArgs.MainSocket.Send(Encoding.GetBytes("get-resources"));

            return true;
        }

        // @actions
		private bool _setBuildingActions(Dictionary<string, string> args, NetArgs netArgs)
		{
		    var actions = SerializationHelper.Deserialize<List<CommonBuildingAction>>(args["actions"], Encoding);

            foreach (var action in actions)
            {
                Debug.Log(action.Active + ", " + action.Name);
            }

            UiController.BuildingActions = new ReadOnlyCollection<CommonBuildingAction>(actions);

            return true;
        }

        // @resources
		private bool _resources(Dictionary<string, string> args, NetArgs netArgs)
		{
            var resources = SerializationHelper.Deserialize<CommonStructures.Resources>(args["resources"], Encoding);

            for (var i = 0; i < resources.ResourcesArray.Length; i++)
            {
                Ui.Resource suitableResource;
                switch ((ResourceType)i)
                {
                    default:
                        Debug.LogError("Wrong resource name");
                        return false;

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

                    case ResourceType.People:
                        suitableResource = Ui.ResourcePeople;
                        break;
                }

                suitableResource.Content = resources.ResourcesArray[i];
            }

            return true;
        }

        // @building
        private bool _upgradeResult(Dictionary<string, string> args, NetArgs netArgs)
        {
            if (args["building"] == "-1")
            {
                return false;
            }

            var result = SerializationHelper.Deserialize<UpgradeResult>(
                args["building"], Encoding);
            var building = BuildingsContainer.Instance
                .BuildingsGrid[result.Position.X, result.Position.Y];

            // TODO Resources.Load -> Prefabs.*
            building.BuildingSpriteRenderer.sprite
                = Prefabs.Get(
                    BuildingsContainer.Instance.PrefabsNames[result.ID])
                    .GetComponent<SpriteRenderer>()
                    .sprite; // TODO Prefabs.GameObject -> Buildings.Building
            
            return true;
        }
    }
}
