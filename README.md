signalRGameHubvs2013
====================
This is a signalR client and server application.

The Signal R Server is hosted in a MVC 5 web site and the client is an XNA client.

The sequence of events is as follows.

An XNA client request to join the Server. On completion of this request a player object created on the server is returned to the client. A graphical game player object with a Player embedded is created on the client. The client then requests other players from the Server to be represented in the game. These are returned by the server and added to the collection of other players which is maintained by the Game Player. A collection of collectables is produced on the server when the first Player joins the server and delivered to each client when they join. 

Once more than two clients have joined a game is in progress and a countdown occurs on each client. Then the game begins. and all the collectables that have been delivered to the client games are made visible. The players in the clients then all try to collect as many collectables as they can. Collectables have varying values and are color coded accordingly. As each collectable is collected the client reports back to the server and the other clients are infomed that the collectable is no longer available to them. When all the collectables are collected the server informs all the Game clients that the game is over by sending down a final scoreboard to the clients. There is tehn a count down to display the Final Scores and then the Clients are forced to quit and the server is set up to accept players for the next game.

