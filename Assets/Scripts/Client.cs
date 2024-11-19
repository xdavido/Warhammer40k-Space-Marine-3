using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    Thread messageReciever;
    Thread waitForStart;

    string serverIP = "127.0.0.1";
    int serverPort;

    Socket socket;
    EndPoint remote;
    IPEndPoint ipep;

    bool startGame;
    bool startConnection;

    int playerID;

    private void Start()
    {
        startGame = false;
        startConnection = false;
        messageReciever = new Thread(ConnectToServer);
        waitForStart = new Thread(WaitForStart);

       // StartCoroutine(FillInputField());
    }

    private void Update()
    {
        if (startConnection)
        {
            FullyConnected();
            startConnection = false;
        }

        if (startGame)
        {
            ChangeScene();
            startGame = false;
        }
    }

    IEnumerator FillInputField()
    {
        //Get IP without last digits
        string myIp = GetMyIp();
        int i = myIp.LastIndexOf('.');
       // myIp = myIp.Substring(0, i + 1);

        //Fill input
        InputField ipInput = GameObject.Find("InputIP").GetComponent<InputField>();
        ipInput.text = myIp;
        ipInput.Select();

        InputField portInput = GameObject.Find("InputPort").GetComponent<InputField>();
        portInput.text = "9000";

        yield return new WaitForEndOfFrame();

        //Needs to wait to set cursor
        ipInput.caretPosition = ipInput.text.Length;
        ipInput.ForceLabelUpdate();
    }

    public void SelectPort()
    {
        StartCoroutine(SelectPortCo());
    }
    IEnumerator SelectPortCo()
    {
        InputField portInput = GameObject.Find("InputPort").GetComponent<InputField>();
        portInput.Select();

        yield return new WaitForEndOfFrame();

        portInput.caretPosition = portInput.text.Length;
        portInput.ForceLabelUpdate();
    }

    public void SetIP()
    {
        //InputField ipInput = GameObject.Find("InputIP").GetComponent<InputField>();
        //InputField portInput = GameObject.Find("InputPort").GetComponent<InputField>();
        serverIP = GetMyIp();
        serverPort = 9000;

        StartConnection();
    }

    void StartConnection()
    {
        ClientSetup();
        messageReciever.Start();
    }

    void ClientSetup()
    {
        ipep = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

        //Open Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        //Set port 0 to send messages
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        remote = (EndPoint)sender;
    }

    public void StopConnection()
    {
        socket.Close();
        Debug.Log("CLIENT DISCONNECTED");
    }

    void SendConfirmation()
    {
        byte[] data = Encoding.ASCII.GetBytes("ClientConnected");
        socket.SendTo(data, data.Length, SocketFlags.None, ipep);
    }

    //THREAD
    void ConnectToServer()
    {
        SendConfirmation();

        byte[] data = new byte[1024];
        int recv;

        try
        {
            recv = socket.ReceiveFrom(data, ref remote);
        }
        catch (System.Exception e)
        {
            Debug.Log("Client stopped listening! " + e.ToString());
            StopConnection();
            return;
        }

        string message = Encoding.ASCII.GetString(data, 0, recv);

        if (message == "ServerConnected")
        {
            startConnection = true;
        }
        else
        {
            Debug.Log("Incorrect confirmation message: " + message);
            StopConnection();
        }
    }

    void FullyConnected()
    {
        TransferInformation();
        waitForStart.Start();
    }

    void TransferInformation()
    {
        MessageManager.socket = socket;
        MessageManager.remote = remote;
    }

    void WaitForStart() 
    {
        Debug.Log("Waiting for the Server to Start...");

        byte[] data = new byte[1024];
        int recv;

        try
        {
            recv = socket.ReceiveFrom(data, ref remote);
        }
        catch
        {
            Debug.Log("Client did not want to wait for Start!");
            StopConnection();
            return;
        }

        string message = Encoding.ASCII.GetString(data, 0, recv);

        //ChangeScene
        if (message.Contains("StartGame"))
        {
            startGame = true;

            //Set player id (message = "0StartGame")
            playerID = (int)message[0] - 48;
        }
        else
        {
            Debug.Log("Message recieved to start game is INCORRECT: " + message);
            StopConnection();
        }
    }
    void ChangeScene()
    {
        //Change the scene to loading scene     The same as this
        SceneManager.LoadScene("MainScene");
        MessageManager.playerID = playerID;
        MessageManager.StartComunication();

        //ColorTank colorTank = FindAnyObjectByType<ColorTank>();
        //DataTank dataTank = FindAnyObjectByType<DataTank>();

        ////Send the info about the name and the color
        //MessageManager.SendMessage(new MessageTypes.Settings(colorTank.GetName(), colorTank.GetColor()));
        //dataTank.SaveDataTank(playerID, colorTank.GetName(), colorTank.GetColor());
    }

    string GetMyIp()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "";
    }
}
