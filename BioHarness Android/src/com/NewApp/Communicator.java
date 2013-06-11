package com.NewApp;

import java.net.InetAddress;
import com.illposed.osc.*;

import android.app.Application;
import android.util.Log;

public class Communicator extends Application {
	
	private String host;
	private String portin;
	private String portout;
	//Javaosc sender and receiver
	OSCPortOut sender;
	OSCPortIn receiver;
	//to know whenever the receiver or sender exist
	private boolean receiverIs = false;
	private boolean senderIs = false;
	
	@Override
    public void onCreate() {
		super.onCreate();
	}
	
	public void connect(){
		if(!senderIs){
			try {
				sender = new OSCPortOut(InetAddress.getByName(host), Integer.parseInt(portout));
				senderIs = true;
				Log.e("connecting to", InetAddress.getByName(host).toString());
				OSCMessage msg = new OSCMessage("/ready");
				msg.addArgument(host);
				msg.addArgument(portout);
				if (msg != null) {
					try {
						sender.send(msg);
					} catch (Exception e) {
						e.printStackTrace();
					}
				}
				
			} catch (Exception e) {
				Log.i("sender osc", e.toString());
			}
		}
		
		if(!receiverIs){
			try {
				receiver = new OSCPortIn(Integer.parseInt(portin));
				receiverIs = true;
			} catch (Exception e) {
				Log.i("receiver osc", e.toString());
			}
		}
	}
	
	public void close(){
		if (senderIs){
			sender.close();
		}
		if (receiverIs){
			receiver.close();
		}
	}
	
	public void sending(String name, int value){
		try {
			OSCMessage msg = new OSCMessage("/controller/"+name);
			msg.addArgument(value);
			if (msg != null) {
				try {
					sender.send(msg);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
			
		} catch (Exception e) {
			Log.i("sender osc", e.toString());
		}
	}
	
	public void sending(String name, String value){
		try {
			OSCMessage msg = new OSCMessage("/controller/"+name);
			msg.addArgument(value);
			if (msg != null) {
				try {
					sender.send(msg);
				} catch (Exception e) {
					e.printStackTrace();
				}
			}
			
		} catch (Exception e) {
			Log.i("sender osc", e.toString());
		}
	}
	
	
	public boolean receiverIs(){
		return receiverIs;
	}
	
	public boolean senderIs(){
		return senderIs;
	}
	
	public String getHost(){
	    return host;
	}
	
	public void setHost(String s){
	    host = s;
	    if (senderIs) sender.close();
	    senderIs = false;
	}
	
	public String getPortin(){
	    return portin;
	}
	
	public void setPortin(String s){
		portin = s;
		if(receiverIs) receiver.close();
		receiverIs = false;
	}
	
	public String getPortout(){
	    return portout;
	}
	
	public void setPortout(String s){
		portout = s;
		if (senderIs) sender.close();
	    senderIs = false;
	}
	
	
}
