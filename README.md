# Online_Videogame
A Online Videogame example for "Xarxes i Videojocs Online" subject.

In this small project, we have the option to start a waiting room as a Host or as a Client. 

When running the game, a screen will appear where we will have to provide our user name, and then enter the room as a Host (with the option ‘Host Game’) or as a Client (‘Join Game’).

After this, a scene opens in which we can easily manage a character to approach the different consoles.

In the case of the Host: we can approach a console that will help us to launch the TCP and UDP server. Both consoles will give us the confirmation of the server launch (as long as it is successful) and will also give us the IP to which the client has to connect.
In both cases, we open a text chat in which the server can also participate.

In the case of the Client: we can approach two separate consoles, one for TCP and one for UDP.
In both UDP and TCP clients we will connect to their respective servers, by default it is configured for the IP 127.0.0.1, which would allow us to connect locally, but this value can be changed to configure any IP. After making the connection, we can chat with the server.
