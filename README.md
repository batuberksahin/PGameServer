# PGameServer

Multiplayer matchmaking/game server with TCP/IP

## Installation

1. Clone the repository to your local machine:

   ```
   git clone https://https://github.com/batuberksahin/PGameServer.git
   ```

2. Build the project using the provided build scripts:

   For Windows you can run in your terminal:
   ```
   build.bat
   ```

   For Linux you can run in your terminal:
   ```
   ./build.sh
   ```

3. Port Forwarding:

   In order for clients to connect to the game server from other devices on the local network, you'll need to set up port forwarding on your router. Forward the port used by the game server (*13312* for MasterServer & GameServer port depends but default *13313*) to the internal IP address of the machine running the server.

    ![port](https://github.com/batuberksahin/PGameServer/assets/30266299/0480d372-7ec5-44ae-96a9-812417c8fb82)

4. Firewall Settings (Windows Only):

   If you're running the game server on a Windows machine, make sure to adjust the firewall settings to allow incoming connections on the port used by the game server (*13312* for MasterServer & GameServer port depends but default *13313*).

5. Connection:

   You can find out the IP address required to connect to the servers from the client by typing "ipconfig" in the terminal for Windows or "ifconfig" for Linux based operating systems and looking at the IPv4 Address of the Ethernet adapter in use.

## ISSUES TO BE CONSIDERED

1. You need to start MasterServer before GameServer.
2. Multiple GameServers is acceptable.

## Architecture

The project is designed using a microservices architecture, consisting of the following main components:

### Game Server

The Game Server is responsible for handling client connections and managing the game logic. It communicates with the players and synchronizes game state across all connected clients. The Game Server uses the Repository Library to interact with the database and store game-related data, such as player information and game scores.

### Master Server

The Master Server acts as a central hub for managing game server instances and player matchmaking. It keeps track of available game servers and their current player capacities. When a player wants to join a game, the Master Server assigns them to an appropriate game server based on factors like region and player count.

### Repository Library

The Repository Library provides a convenient interface for the Game Server and Master Server to interact with the database. It encapsulates database operations, such as saving and retrieving game-related data, behind a simple API. This helps keep the server code clean and maintainable.

## Troubleshooting

You can contact me via batuberkshn[at]gmail.com
