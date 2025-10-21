# Introduction

The quality of a game network is decided by a few factors.
Primaire there are three factors which the developer decides.
These factors are:

- Bandwidth consumption
- CPU
- Latency

These three resouces can be seen as a triangle with a constant area.
reducing your bandwidth consumption will probably increase your CPU cost and latency by using compression algorithms or heavier serialization strategies.
(Unity Technologies, 2023)

The developer needs to make choices while developing their multiplayer topology.
Other factors are not in direct control of the developer.
In this research we will look at:

- Latency (Ping)
- Jitter
- Out of order Packets
- Packet Loss

Though these factors lay outside the control of the developer, 
they can be accounted for.
(Simulating Bad Network Connections | Fish-Net: Networking Evolved, z.d.)

We will look at how a network can be measured,
how the network can be tested,
what a developer can do to increase the network capability
and lastly actions can be taken to enhance the developer and player experience.

Lastly keep in mind we use Fish-net to facilitate the networking.

# Findings

## Primary triangle

The developer should keep the wares a player uses in mind.
There is only a limited amount of bandwidth consumption and cpu power.
Trying to optimize these features gives more room for other features.
To measure the consumption of the network and cpu.
The developers can utilize their specified Profiler.

With this data the team or architect can discuss what they find acceptable.

Fish-Net has several features to enhance the bandwidth and cpu consumption.
The main feature to enhance performance is the Area of Interest System.
This system makes sure no information is send to those who need not hear it.
These AoI areas are scalable because it's not one area per scene.
The area can be any distance from any point.

Other factors include the topology of the system. 
How do the clients communicate with each other.

## Outside factors

As stated the developer does not have control over everything.
Latency is the delay between sending and receiving packets.
Jitter is the variations in latency over time.
Out-of-order Packets means packets may arrive out of the order from when they were sent.
Packet Loss which means some messages never arrive.

For Desktop broadband games developers should test with the following values:

- 100-150 ms latency
- 1-3% packet loss

These are regarded as average values. 
One could also stress test by going above 500ms latency.


To achieve this the developer uses the build in FishNet TransportManager.
In combination with external tools like [clumsy](https://jagt.github.io/clumsy/)
which allow you to further bend the network.

## Utilizing 

To monitor the network a developer can use a statisticsManager.
This component manages the statistics. Which can be displayerd via the BandwidthDisplay.
By adding the statisticsManager component to a GameObject.
It's values can be used like this:

```C#
StatisticsManager stats = this.GetComponent<StatisticsManager>();

var networkTraffic = stats.GetTrafficData();
var pingData = stats.GetPingData();
```

This information can be displayed via the GUI.
Add the BandwidthDisplay component to the UI.
Here you can also use this information to make averages or schemes.

# Sources
Unity Technologies. (2023). Profiler (Version 2.2).  
Simulating bad network connections | Fish-Net: Networking evolved. (z.d.). Fish-Net: Networking Evolved. https://fish-networking.gitbook.io/docs/tutorials/simple/simulating-bad-network-connections
