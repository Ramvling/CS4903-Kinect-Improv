

$(document).ready(function() { 
	console.log('bloop');
});

mainLoop = function() {

};

sendMsg = function() {
	console.log('brbrbr');
}

var client = new SockJS('http://127.0.0.1:8000/');

 client.onopen = function() {
     console.log('open');
 };
 client.onmessage = function(e) {
     console.log('message', e.data);
 };
 client.onclose = function() {
     console.log('close');
 };

 client.send("test2");
 client.close();