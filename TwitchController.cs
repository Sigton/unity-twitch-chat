using UnityEngine;

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;

public class TwitchController : MonoBehaviour {

    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    public string username, password, channelName; // password has to be OAuth token, which you can get from here: https://twitchapps.com/tmi/
                                                   // username is the account name, and channelName is the chat you want to join.
    void Start()
    {
        // Connects to IRC server immediately
        Connect();
    }

    void Update()
    {
        // Program will automatically reconnect if connection is lost
        if (!twitchClient.Connected)
        {
            Connect();
        }
    }

    // Will connect to the channel given using the username and password
    public void Connect()
    {
        // Connect to port 6667 (unless using SSL, then connect to 443)
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);

        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        // Format needs to remain as written here.
        writer.WriteLine("PASS " + password);
        writer.WriteLine("NICK " + username);
        writer.WriteLine("USER " + username + " 8 * :" + username);
        writer.WriteLine("JOIN #" + channelName);
        writer.Flush();

    }

    public KeyValuePair<string, string> ReadChat()
    {

        // Returns a key-value pair of the user who sent the message and the content of the message
        // ReadChat().Key -> username
        // ReadChat().Value -> message content
        //
        // If no messages have been sent then a KeyValue pair of empty strings are returned.

        if (twitchClient.Available > 0)
        {
            var message = reader.ReadLine();

            if (message.Contains("PRIVMSG"))
            {
                var splitPoint = message.IndexOf("!", 1);
                var chatName = message.Substring(0, splitPoint);
                chatName = chatName.Substring(1);

                splitPoint = message.IndexOf(":", 1);
                message = message.Substring(splitPoint + 1);

                return new KeyValuePair<string, string>(chatName, message);

            }

        }
        return new KeyValuePair<string, string>(String.Empty, String.Empty);
        
    }

    // Sends the message given to the chat that the bot is currently connected to
    public void sendMessage(string message)
    {
        // Format to send messages in: "PRIVMSG #<channel name> :<message content>"
        string message_to_send = "PRIVMSG #" + channelName + " :" + message;
        writer.WriteLine(message_to_send);
        writer.Flush();
    }

}