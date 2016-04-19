var BUTTON_PRESS = 0;

$(document).ready(function() { 
	setInterval(mainLoop, 15);
    init();
});


//TODO
function setupMessages() {
    //Incoming
    var testIn = createMsgStruct(BUTTON_PRESS, false);
    testIn.addString();

    // Outgoing 
    var testOut = createMsgStruct(BUTTON_PRESS, true);
    testOut.addString();
}

function sendMsg() {
    var packet = newPacket(BUTTON_PRESS);
    packet.write($("#msg").val());
    packet.send();
    $("#notify").text("Sent msg");
}

function init() {
        setupMessages();
	// This will be called when the connection is successful
    	var onopen = function() {
        console.log("bloop");
        var packet = newPacket(BUTTON_PRESS);
        packet.write(("This is a test"));
        // and then we send the packet!
        console.log("This is a test");
        packet.send();
        $("#notify").text("Connected!");
    }

    // This will be called when the connection is closed
    var onclose = function() {
        window.location.href = '/';
    }

    console.log("connection started")
    // Start the connection!
    wsconnect("ws://128.61.105.215:8886", onopen, onclose);
    console.log("why");
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
    //if (msgID === MSG_LOGIN) {
    //    var pid = packet.read();
    //    alert("You are client number " + pid);
    //}
}

function mainLoop() {
    handleNetwork();
}
