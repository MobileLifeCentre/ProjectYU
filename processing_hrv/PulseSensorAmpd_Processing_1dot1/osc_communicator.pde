import oscP5.*;
import netP5.*;
import processing.serial.*;

class OscCommunicator {
  OscP5 oscP5;
  NetAddress remoteLocation;
  String remoteIP = "127.0.0.1";
  int remotePort = 7780;
  OscCommunicator () {
    setup();
  }  
  
  void setup() 
  { 
    // OSC config set to listen port 12000
    oscP5 = new OscP5(this, 12000);
    remoteLocation = new NetAddress(remoteIP, remotePort);
  } 
  
  public void Send(String id, String content) {
    OscMessage myMessage = new OscMessage("/"+id);
    myMessage.add(content);
    oscP5.send(myMessage, remoteLocation);  
  }
  
  public void Send(String id, int content) {
    Send(id, content+"");
  }
}

