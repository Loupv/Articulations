# Articulations

Application made for the Articulation research project - a multiplayer experience where two and more dancers can immerse themselves
and see each other as a set of spheres or other minimalistic representation.
The software has to be launched multiple times : one as a server, that handles the registration of every player, can change the visual representation, and that loads and stores performance data ; and one or two as clients (with VR headsets or simple viewers).
The client can be either a player or a viewer, the player searches automatically for a Vive System to work with, the viewer can be
an invisible watcher that can control a camera around the scene to observe the performance.

We're planning to write a wiki soon. But the basics for now :

- Runs on Unity 2018.3.3f1
- Can run in unity editor and as executable (needs at least one executable to run two instances on the same computer)
- There's a "gameData.json" file in "Assets/StreamingAssets" folder  where you'll need to write the server IP address to help clients to log on the network
- First, launch the server, then the clients.
- If firewalls are shut and all computers are on the same network it should work fine
- It's also possible to run the server and one client on the same computer, just enter own IP address on client's gameData file and it should work.


For any question send me a email : loup.vuarnesson[at]ensad.fr
