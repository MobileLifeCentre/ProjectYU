package com.NewApp;

import android.app.Activity;

import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.Set;

import android.R.*;
import android.app.Activity;
import android.bluetooth.*;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.sax.TextElementListener;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.*;

import zephyr.android.BioHarnessBT.*;

public class MainActivity extends Activity {
    /** Called when the activity is first created. */
	BluetoothAdapter adapter = null;
	BTClient _bt;
	ZephyrProtocol _protocol;
	NewConnectedListener _NConnListener;
	private final int HEART_RATE = 0x100;
	private final int RESPIRATION_RATE = 0x101;
	private final int SKIN_TEMPERATURE = 0x102;
	private final int POSTURE = 0x103;
	private final int PEAK_ACCLERATION = 0x104;
	private final int BREATHING_RAW = 0x105;
	//  OSC data
	private final String IP = "IP";
	private final String PORT = "PORT";
	private Communicator _oscCommunicator;
	private Editor _preferencesEditor;
	private SharedPreferences _preferences;
	private EditText _ipEditText;
	private EditText _portEditText;
	
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        /*Sending a message to android that we are going to initiate a pairing request*/
        IntentFilter filter = new IntentFilter("android.bluetooth.device.action.PAIRING_REQUEST");
        /*Registering a new BTBroadcast receiver from the Main Activity context with pairing request event*/
       this.getApplicationContext().registerReceiver(new BTBroadcastReceiver(), filter);
        // Registering the BTBondReceiver in the application that the status of the receiver has changed to Paired
        IntentFilter filter2 = new IntentFilter("android.bluetooth.device.action.BOND_STATE_CHANGED");
       this.getApplicationContext().registerReceiver(new BTBondReceiver(), filter2);

       // OSC related data
       _ipEditText = (EditText) findViewById(R.id.IP);
       _portEditText = (EditText) findViewById(R.id.Port);
       
       _preferences = this.getPreferences(Context.MODE_PRIVATE);
       _preferencesEditor = _preferences.edit();
       
       _ipEditText.setText(_preferences.getString(IP, "192.168.43.213"));
       _portEditText.setText(_preferences.getString(PORT, "7780"));
       
       _oscCommunicator = ((Communicator)getApplicationContext());
    
       
       
      //Obtaining the handle to act on the CONNECT button
        TextView tv = (TextView) findViewById(R.id.labelStatusMsg);
		String ErrorText  = "Not Connected to BioHarness !";
		 tv.setText(ErrorText);

        Button btnConnect = (Button) findViewById(R.id.ButtonConnect);
        if (btnConnect != null)
        {
        	btnConnect.setOnClickListener(new OnClickListener() {
        		public void onClick(View v) {
        			// We set the IP of the OSC communicator
        			String ip = _ipEditText.getText().toString();
        			String port = _portEditText.getText().toString();
        			
        			_oscCommunicator.setHost(ip);
    		        _oscCommunicator.setPortout(port);
    		        _oscCommunicator.connect();
    		        
    		        _preferencesEditor.putString(IP, ip);
    		        _preferencesEditor.putString(PORT, port);
    		        
        			String BhMacID = "00:07:80:9D:8A:E8";
        			adapter = BluetoothAdapter.getDefaultAdapter();
        			
        			Set<BluetoothDevice> pairedDevices = adapter.getBondedDevices();
        			
        			if (pairedDevices.size() > 0) 
        			{
                        for (BluetoothDevice device : pairedDevices) 
                        {
                        	if (device.getName().startsWith("BH")) 
                        	{
                        		BluetoothDevice btDevice = device;
                        		BhMacID = btDevice.getAddress();
                                break;

                        	}
                        }
        			}
        			
        			BluetoothDevice Device = adapter.getRemoteDevice(BhMacID);
        			String DeviceName = Device.getName();
        			_bt = new BTClient(adapter, BhMacID);
        			_NConnListener = new NewConnectedListener(Newhandler,Newhandler);
        			_bt.addConnectedEventListener(_NConnListener);
        			
        			TextView tv1 = (EditText)findViewById(R.id.labelHeartRate);
        			tv1.setText("000");
        			
        			 tv1 = (EditText)findViewById(R.id.labelRespRate);
        			 tv1.setText("0.0");
        			 
        			 tv1 = 	(EditText)findViewById(R.id.labelSkinTemp);
        			 tv1.setText("0.0");
        			 
        			 tv1 = 	(EditText)findViewById(R.id.labelPosture);
        			 tv1.setText("000");
        			 
        			 tv1 = 	(EditText)findViewById(R.id.labelPeakAcc);
        			 tv1.setText("0.0");
        			if(_bt.IsConnected())
        			{
        				_bt.start();
        				TextView tv = (TextView) findViewById(R.id.labelStatusMsg);
        				String ErrorText  = "Connected to BioHarness "+DeviceName;
						 tv.setText(ErrorText);
						 //Reset all the values to 0s

        			}
        			else
        			{
        				TextView tv = (TextView) findViewById(R.id.labelStatusMsg);
        				String ErrorText  = "Unable to Connect !";
						 tv.setText(ErrorText);
        			}
        		}
        	});
        }
        /*Obtaining the handle to act on the DISCONNECT button*/
        Button btnDisconnect = (Button) findViewById(R.id.ButtonDisconnect);
        if (btnDisconnect != null)
        {
        	btnDisconnect.setOnClickListener(new OnClickListener() {
				
				/*Functionality to act if the button DISCONNECT is touched*/
				public void onClick(View v) {
					/*Reset the global variables*/
					TextView tv = (TextView) findViewById(R.id.labelStatusMsg);
    				String ErrorText  = "Disconnected from BioHarness!";
					 tv.setText(ErrorText);

					/*This disconnects listener from acting on received messages*/	
					_bt.removeConnectedEventListener(_NConnListener);
					/*Close the communication with the device & throw an exception if failure*/
					_bt.Close();
					
					// we disconnect OSC
					_oscCommunicator.close();
				}
        	});
        }
    }
    
    // create a Toast to display info/errors etc
 	protected void showToast(String anErrorMessage) {
 		Context context = getApplicationContext();
 		int duration = Toast.LENGTH_SHORT;
 		Toast.makeText(context, anErrorMessage, duration).show();
 	}
    
    
    private class BTBondReceiver extends BroadcastReceiver {
		@Override
		public void onReceive(Context context, Intent intent) {
			Bundle b = intent.getExtras();
			BluetoothDevice device = adapter.getRemoteDevice(b.get("android.bluetooth.device.extra.DEVICE").toString());
			Log.d("Bond state", "BOND_STATED = " + device.getBondState());
		}
    }
    private class BTBroadcastReceiver extends BroadcastReceiver {
		@Override
		public void onReceive(Context context, Intent intent) {
			Log.d("BTIntent", intent.getAction());
			Bundle b = intent.getExtras();
			Log.d("BTIntent", b.get("android.bluetooth.device.extra.DEVICE").toString());
			Log.d("BTIntent", b.get("android.bluetooth.device.extra.PAIRING_VARIANT").toString());
			try {
				BluetoothDevice device = adapter.getRemoteDevice(b.get("android.bluetooth.device.extra.DEVICE").toString());
				Method m = BluetoothDevice.class.getMethod("convertPinToBytes", new Class[] {String.class} );
				byte[] pin = (byte[])m.invoke(device, "1234");
				m = device.getClass().getMethod("setPin", new Class [] {pin.getClass()});
				Object result = m.invoke(device, pin);
				Log.d("BTTest", result.toString());
			} catch (SecurityException e1) {
				e1.printStackTrace();
			} catch (NoSuchMethodException e1) {
				e1.printStackTrace();
			} catch (IllegalArgumentException e) {
				e.printStackTrace();
			} catch (IllegalAccessException e) {
				e.printStackTrace();
			} catch (InvocationTargetException e) {
				e.printStackTrace();
			}
		}
    }
    

    final  Handler Newhandler = new Handler(){
    	public void handleMessage(Message msg)
    	{
    		TextView tv;
    		Log.w("MSG", msg.getData().toString());
    		switch (msg.what)
    		{
    		case HEART_RATE:
    			String HeartRatetext = msg.getData().getString("HeartRate");
    			tv = (EditText)findViewById(R.id.labelHeartRate);
    			System.out.println("Heart Rate Info is "+ HeartRatetext);
    			if (tv != null)tv.setText(HeartRatetext);
    			_oscCommunicator.sending("HEART_RATE", HeartRatetext);
    		break;
    		
    		case RESPIRATION_RATE:
    			String RespirationRatetext = msg.getData().getString("RespirationRate");
    			tv = (EditText)findViewById(R.id.labelRespRate);
    			if (tv != null)tv.setText(RespirationRatetext);
    			_oscCommunicator.sending("RESPIRATION_RATE", RespirationRatetext);
    		
    		break;
    		
    		case SKIN_TEMPERATURE:
    			String SkinTemperaturetext = msg.getData().getString("SkinTemperature");
    			tv = (EditText)findViewById(R.id.labelSkinTemp);
    			if (tv != null)tv.setText(SkinTemperaturetext);
    			_oscCommunicator.sending("SKIN_TEMPERATURE", SkinTemperaturetext);
    		break;
    		
    		case POSTURE:
    			String PostureText = msg.getData().getString("Posture");
    			tv = (EditText)findViewById(R.id.labelPosture);
    			if (tv != null)tv.setText(PostureText);
    			_oscCommunicator.sending("POSTURE", PostureText);
    		
    		break;
    		
    		case PEAK_ACCLERATION:
    			String PeakAccText = msg.getData().getString("PeakAcceleration");
    			tv = (EditText)findViewById(R.id.labelPeakAcc);
    			if (tv != null)tv.setText(PeakAccText);
    			_oscCommunicator.sending("PEAK_ACCLERATION", PeakAccText);
    		break;	
    		
    		case BREATHING_RAW:
    			short[] breathingRaw = msg.getData().getShortArray("BreathingRaw");
    			String result = "";
    			for (int i = 0; i < breathingRaw.length; ++i) {
    				result += breathingRaw[i] + " ";
    			}
    			_oscCommunicator.sending("BREATHING_RAW", result);
			break;
    		}
    	}

    };
    
}


