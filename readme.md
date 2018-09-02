# Network physics test bed

This project simulates the interaction between a client and server on a single Unity instance.

User can control a paddle using WASD, and try to prevent multiple bouncing cubes getting past. The server detemines if a cube has gone past the paddle, and increments the score.

## Client-side prediction

Server is authoritative of all rigidbodies. Client side prediction to smooth gameplay experience for client.

Client sends input to the server, and both the server and client simulate physics. When the client receives a snapshot update from the server, it determines if the client state at the server snapshot's timestamp is sufficiently different to the server snapshot state. If so, it resets the client rigidbodies to the server snapshot state, and re-simulates the physics from the server snapshot's timestamp.

Without correction, the client cubes (grey) diverge from the server cubes (red).

![uncorrected](uncorrected.gif)

With correction, the grey client cubes stay in sync.

![corrected](corrected.gif)

# Future work
- Simulate latency for client messages
- Variable snapshot rate for server messages
- Simulate packet loss
	- Accomodate for this by re-sending redundant inputs to server
- Simulate out of order packets
- Compress packet size
- Correction smoothing
- Send packets across network rather than simulation