import processing.serial.*;

import oscP5.*;
import netP5.*;

// Communicating with Arduino 
// Creating a txt file import processing.serial.*; 

import processing.serial.*;


Serial serialPort; 
int counter;
String[] data; 
int tLength = 60000;

//OSC related
OscP5 oscP5;
NetAddress remoteLocation;
String remoteIP = "127.0.0.1";
int remotePort = 7780;

void setup() 
{ 
//println(Serial.list()); 
// To see the ports 
  serialPort = new Serial(this, Serial.list()[0], 9600); 
  counter = 0; 
  data = new String[tLength];
  
  // OSC config set to listen port 12000
  oscP5 = new OscP5(this, 12000);
  remoteLocation = new NetAddress(remoteIP, remotePort);
} 

void sendOSCGSR(int GSR) 
{  
  OscMessage myMessage = new OscMessage("/GSR");
  myMessage.add(GSR);
  oscP5.send(myMessage, remoteLocation);  
}

void draw() 
{ 
  int temp = serialPort.read();
  // If there is some input print it 
  if (temp != -1) 
  { 
    println(counter + " " + temp); 
    //data[counter] = counter + "," + temp; counter++; 
    data[counter] = "" + temp;
    counter++;
    // OSC send value
    sendOSCGSR(temp);
  }
  
  // if we reached 20min save text and exit 
  //if (counter == 1300) { 
  if (counter == tLength) {
    saveStrings("thesis_data.txt", data);
    exit(); 
  } 
} 

