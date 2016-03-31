var BUTTON_PRESS = 0;

$(document).ready(function() { 
	console.log('bloop');
	setInterval(mainLoop, 15);
});


//TODO
function setupMessages() {
    // Incoming MSG_LOGIN
    var m1 = createMsgStruct(BUTTON_PRESS, false);
    // This packet will be carrying two chars
    m1.addChars(2);

    // Outgoing MSG_LOGIN
    var i1 = createMsgStruct(BUTTON_PRESS, true);
    // This packet sends a string (our name) to the server
    i1.addString();
}

function init() {
	// This will be called when the connection is successful
    var onopen = function() {
        // We ask for a new packet for type MSG_LOGIN
        var packet = newPacket(BUTTON_PRESS);
        // Writing our name. 'Write' is currently expecting a String,
        // as that is what we defined earlier.
        packet.write(("This is a test"));
        // and then we send the packet!
        packet.send();
        $("#notify").text("Connected!");
    }

    // This will be called when the connection is closed
    var onclose = function() {
        window.location.href = '/';
    }

    // Start the connection!
    wsconnect("ws://localhost:8885", onopen, onclose);
};

// This function handles incoming packets
function handleNetwork() {
    // First we check if we have enough data
    // to handle a full packet
    if (!canHandleMsg()) {
        return;
    }

    // Read the packet in
    var packet = readPacket();

    // Find out what type of packet it is
    msgID = packet.msgID;

    // And handle it!
    if (msgID === MSG_LOGIN) {
        var pid = packet.read();
        alert("You are client number " + pid);
    }
}

function mainLoop() {
    handleNetwork();
}